using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.GameFlow
{
    /// <summary>
    /// 상점 패널 뷰. 연결된 오퍼 셀 + 리롤 + 광고 보상 표시를 관리한다.
    /// open/close는 외부 상점 토글 버튼이, 유물 설명은 이 프리팹 밖의 별개 패널이 담당하므로
    /// 여기서는 그 어느 것도 참조하지 않는다.
    /// </summary>
    public sealed class RunBattleShopView : MonoBehaviour
    {
        private const string AlertTextObjectName = "Alert Text";
        private const float FallbackAlertFontSize = 18f;

        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private ShopArtifactOptionView[] _offerViews;
        [SerializeField] private Button _rerollButton;
        [SerializeField] private TMP_Text _rerollButtonTmpText;
        [SerializeField] private Button _adButton;
        [SerializeField] private TMP_Text _adButtonTmpText;
        [SerializeField] private TMP_Text _alertText;
        [SerializeField] private float _alertHoldDuration = 0.45f;
        [SerializeField] private float _alertFadeDuration = 0.75f;
        [SerializeField] private TMP_SpriteAsset _currencySpriteAsset;
        [SerializeField] private Texture2D _rarityFrameSheet;

        private ShopArtifactOptionView[] _subscribedCells;
        private Action[] _cellPurchaseHandlers;
        private Button _subscribedRerollButton;
        private Button _subscribedAdButton;
        private Sequence _alertSequence;
        private bool _referencesResolved;

        public event Action<int> PurchaseRequested;

        public event Action RerollRequested;

        public event Action AdRewardRequested;

        public event Action<RunBattleRelicShopOfferState> OfferSelected;

        public RectTransform PanelTransform
        {
            get
            {
                EnsureReferences();
                return _shopPanel != null ? _shopPanel.transform as RectTransform : null;
            }
        }

        private void Awake()
        {
            EnsureReferences();
            HideAlertImmediate();
            SubscribeButtons();
        }

        private void OnDisable()
        {
            HideAlertImmediate();
        }

        private void OnDestroy()
        {
            KillAlertTween(false);
            UnsubscribeButtons();
        }

        public bool EnsureReferences()
        {
            bool complete = HasRequiredReferences();
            if (!complete)
            {
                Debug.LogError(
                    "[RunBattleShopView] Shop UI references must be wired in the inspector. " +
                    $"Missing: {BuildMissingReferenceSummary()}");
                return false;
            }

            if (!_referencesResolved)
            {
                _referencesResolved = true;
                SubscribeButtons();
            }

            return true;
        }

        private bool HasRequiredReferences()
        {
            return _shopPanel != null &&
                HasEntries(_offerViews, 1) &&
                _rerollButton != null &&
                _rerollButtonTmpText != null &&
                _adButton != null &&
                _adButtonTmpText != null &&
                _rarityFrameSheet != null;
        }

        private string BuildMissingReferenceSummary()
        {
            var missing = new List<string>();
            if (_shopPanel == null) missing.Add("Shop Panel");
            if (!HasEntries(_offerViews, 1)) missing.Add("Offer Views");
            if (_rerollButton == null) missing.Add("Reroll Button");
            if (_rerollButtonTmpText == null) missing.Add("Reroll Button Text");
            if (_adButton == null) missing.Add("Ad Button");
            if (_adButtonTmpText == null) missing.Add("Ad Button Text");
            if (_rarityFrameSheet == null) missing.Add("Rarity Frame Sheet");
            return missing.Count > 0 ? string.Join(", ", missing) : "None";
        }

        private static bool HasEntries<T>(T[] entries, int requiredCount)
            where T : class
        {
            if (entries == null || entries.Length < requiredCount)
            {
                return false;
            }

            for (int index = 0; index < requiredCount; index++)
            {
                if (entries[index] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static T GetAt<T>(T[] entries, int index)
            where T : class
        {
            return entries != null && index >= 0 && index < entries.Length
                ? entries[index]
                : null;
        }

        public void Render(RunBattleScreenState state)
        {
            if (!EnsureReferences())
            {
                return;
            }

            RunBattleRelicShopState shop = state?.RelicShop ?? RunBattleRelicShopState.Empty;
            _shopPanel.SetActive(shop.Visible);
            ApplyCurrencySpriteAsset();
            if (!shop.Visible)
            {
                HideAlertImmediate();
                return;
            }

            for (int index = 0; index < _offerViews.Length; index++)
            {
                RunBattleRelicShopOfferState? offer = index < shop.Offers.Count
                    ? shop.Offers[index]
                    : null;
                RenderOffer(index, offer, shop);
            }

            if (_rerollButton != null)
            {
                _rerollButton.interactable = shop.CanReroll;
                SetButtonLabel(_rerollButtonTmpText, BuildCurrencyLabel(shop.RerollCost));
            }

            if (_adButton != null)
            {
                _adButton.interactable = shop.CanClaimAdReward;
                SetText(
                    _adButtonTmpText,
                    RunCurrencyText.FormatBonusAmount(WaveAdRewardModel.RewardPerClaim));
            }
        }

        private void RenderOffer(
            int index,
            RunBattleRelicShopOfferState? nullableOffer,
            RunBattleRelicShopState shop)
        {
            ShopArtifactOptionView cell = GetAt(_offerViews, index);
            if (cell == null)
            {
                return;
            }

            bool hasOffer = nullableOffer.HasValue &&
                !string.IsNullOrEmpty(nullableOffer.Value.RelicId);
            cell.gameObject.SetActive(hasOffer);
            if (!hasOffer)
            {
                return;
            }

            cell.SetRarityFrameSheet(_rarityFrameSheet);
            cell.Render(nullableOffer.Value, shop.CanUseShop, _currencySpriteAsset);
        }

        public void ShowAlert(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                HideAlertImmediate();
                return;
            }

            TMP_Text alertText = ResolveAlertText(createIfMissing: true);
            if (alertText == null)
            {
                return;
            }

            KillAlertTween(false);
            alertText.gameObject.SetActive(true);
            alertText.text = message;
            SetAlertAlpha(alertText, 1f);

            _alertSequence = DOTween.Sequence()
                .SetTarget(alertText)
                .SetUpdate(true)
                .AppendInterval(Mathf.Max(0f, _alertHoldDuration))
                .Append(DOTween.To(
                    () => alertText != null ? alertText.color.a : 0f,
                    alpha => SetAlertAlpha(alertText, alpha),
                    0f,
                    Mathf.Max(0f, _alertFadeDuration)))
                .OnComplete(() =>
                {
                    if (alertText != null)
                    {
                        alertText.gameObject.SetActive(false);
                    }

                    _alertSequence = null;
                });
        }

        private TMP_Text ResolveAlertText(bool createIfMissing)
        {
            if (_alertText != null)
            {
                return _alertText;
            }

            if (_shopPanel == null)
            {
                return null;
            }

            TMP_Text[] texts = _shopPanel.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                TMP_Text text = texts[index];
                if (text != null && text.gameObject.name == AlertTextObjectName)
                {
                    _alertText = text;
                    return _alertText;
                }
            }

            return createIfMissing ? CreateFallbackAlertText() : null;
        }

        private TMP_Text CreateFallbackAlertText()
        {
            if (_shopPanel == null)
            {
                return null;
            }

            var alertObject = new GameObject(
                AlertTextObjectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            alertObject.transform.SetParent(_shopPanel.transform, false);
            alertObject.transform.SetAsLastSibling();

            if (alertObject.transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(1f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = new Vector2(0f, 50f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            TextMeshProUGUI text = alertObject.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = FallbackAlertFontSize;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.text = string.Empty;
            text.color = new Color(1f, 0.1f, 0.1f, 0f);

            alertObject.SetActive(false);
            _alertText = text;
            return _alertText;
        }

        private void HideAlertImmediate()
        {
            KillAlertTween(false);

            TMP_Text alertText = ResolveAlertText(createIfMissing: false);
            if (alertText == null)
            {
                return;
            }

            SetAlertAlpha(alertText, 0f);
            alertText.gameObject.SetActive(false);
        }

        private void KillAlertTween(bool complete)
        {
            if (_alertSequence == null)
            {
                return;
            }

            _alertSequence.Kill(complete);
            _alertSequence = null;
        }

        private static void SetAlertAlpha(TMP_Text alertText, float alpha)
        {
            if (alertText == null)
            {
                return;
            }

            Color color = alertText.color;
            color.a = Mathf.Clamp01(alpha);
            alertText.color = color;
        }

        private static void SetText(TMP_Text tmpText, string value)
        {
            if (tmpText != null)
            {
                tmpText.text = value ?? string.Empty;
            }
        }

        private void SubscribeButtons()
        {
            SubscribeCells();
            SubscribeRerollButton();
            SubscribeAdButton();
        }

        private void SubscribeCells()
        {
            if (_offerViews == null || _subscribedCells == _offerViews)
            {
                return;
            }

            UnsubscribeCells();
            _subscribedCells = _offerViews;
            _cellPurchaseHandlers = new Action[_subscribedCells.Length];

            for (int index = 0; index < _subscribedCells.Length; index++)
            {
                ShopArtifactOptionView cell = _subscribedCells[index];
                if (cell == null)
                {
                    continue;
                }

                int capturedIndex = index;
                Action purchaseHandler = () => PurchaseRequested?.Invoke(capturedIndex);
                _cellPurchaseHandlers[index] = purchaseHandler;
                cell.PurchaseRequested += purchaseHandler;
                cell.Selected += HandleCellSelected;
            }
        }

        private void HandleCellSelected(RunBattleRelicShopOfferState offer)
        {
            OfferSelected?.Invoke(offer);
        }

        private void SubscribeRerollButton()
        {
            if (_rerollButton == null || _subscribedRerollButton == _rerollButton)
            {
                return;
            }

            if (_subscribedRerollButton != null)
            {
                _subscribedRerollButton.onClick.RemoveListener(HandleRerollClicked);
            }

            _rerollButton.onClick.AddListener(HandleRerollClicked);
            _subscribedRerollButton = _rerollButton;
        }

        private void SubscribeAdButton()
        {
            if (_adButton == null || _subscribedAdButton == _adButton)
            {
                return;
            }

            if (_subscribedAdButton != null)
            {
                _subscribedAdButton.onClick.RemoveListener(HandleAdRewardClicked);
            }

            _adButton.onClick.AddListener(HandleAdRewardClicked);
            _subscribedAdButton = _adButton;
        }

        private void UnsubscribeButtons()
        {
            UnsubscribeCells();
            if (_subscribedRerollButton != null)
            {
                _subscribedRerollButton.onClick.RemoveListener(HandleRerollClicked);
                _subscribedRerollButton = null;
            }

            if (_subscribedAdButton != null)
            {
                _subscribedAdButton.onClick.RemoveListener(HandleAdRewardClicked);
                _subscribedAdButton = null;
            }
        }

        private void UnsubscribeCells()
        {
            if (_subscribedCells == null)
            {
                _cellPurchaseHandlers = null;
                return;
            }

            for (int index = 0; index < _subscribedCells.Length; index++)
            {
                ShopArtifactOptionView cell = _subscribedCells[index];
                if (cell == null)
                {
                    continue;
                }

                if (_cellPurchaseHandlers != null &&
                    index < _cellPurchaseHandlers.Length &&
                    _cellPurchaseHandlers[index] != null)
                {
                    cell.PurchaseRequested -= _cellPurchaseHandlers[index];
                }

                cell.Selected -= HandleCellSelected;
            }

            _subscribedCells = null;
            _cellPurchaseHandlers = null;
        }

        private void HandleRerollClicked()
        {
            RerollRequested?.Invoke();
        }

        private void HandleAdRewardClicked()
        {
            AdRewardRequested?.Invoke();
        }

        private string BuildCurrencyLabel(int amount)
        {
            return RunCurrencyText.FormatPlainAmount(amount);
        }

        private void SetButtonLabel(TMP_Text tmpText, string value)
        {
            SetText(tmpText, value);
        }

        private void ApplyCurrencySpriteAsset()
        {
            if (_currencySpriteAsset == null)
            {
                return;
            }

            RunCurrencyText.ApplySpriteAsset(_rerollButtonTmpText, _currencySpriteAsset);
        }
    }

    internal static class RunCurrencyText
    {
        private const string SpriteTag = "<sprite index=0>";

        public static string FormatAmount(int amount, TMP_SpriteAsset spriteAsset)
        {
            int safeAmount = Mathf.Max(0, amount);
            return spriteAsset != null
                ? $"{SpriteTag} {safeAmount}"
                : safeAmount.ToString();
        }

        public static string FormatPlainAmount(int amount)
        {
            return Mathf.Max(0, amount).ToString();
        }

        public static string FormatBonusAmount(int amount)
        {
            return $"+{Mathf.Max(0, amount)}";
        }

        public static void ApplySpriteAsset(TMP_Text text, TMP_SpriteAsset spriteAsset)
        {
            if (text == null || spriteAsset == null || text.spriteAsset == spriteAsset)
            {
                return;
            }

            text.spriteAsset = spriteAsset;
        }
    }
}
