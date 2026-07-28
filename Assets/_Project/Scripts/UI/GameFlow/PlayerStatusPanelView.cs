using System.Collections.Generic;
using DG.Tweening;
using SlotRogue.Core.Combat;
using SlotRogue.UI.Combat.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.GameFlow
{
    public sealed class PlayerStatusPanelView : MonoBehaviour
    {
        private readonly Dictionary<StatusEffectKind, StatusSlot> _slotsByKind = new();
        private readonly List<StatusSlot> _slots = new();
        private readonly List<StatusEffectKind> _buffOrder = new();
        private readonly List<StatusEffectKind> _debuffOrder = new();

        [SerializeField] private Vector2 _minimumSlotSize = new(26f, 16f);

        private StatusSlot _slotTemplate;
        private StatusSlot _buffSlotTemplate;
        private StatusSlot _debuffSlotTemplate;
        private Sprite _buffBackgroundSprite;
        private Sprite _debuffBackgroundSprite;
        private RectTransform _root;
        private bool _initialized;
        private bool _reportedMissingReferences;

        private RectTransform Root =>
            _root != null ? _root : _root = transform as RectTransform;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDisable()
        {
            KillSlotTweens(complete: true);
        }

        private void OnDestroy()
        {
            KillSlotTweens(complete: true);
        }

        public bool EnsureReferences()
        {
            return EnsureInitialized();
        }

        public void Render(IReadOnlyList<StatusEffectViewData> statuses)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            var visibleKinds = new HashSet<StatusEffectKind>();
            if (statuses != null)
            {
                for (int index = 0; index < statuses.Count; index++)
                {
                    StatusEffectViewData status = statuses[index];
                    if (status.Kind == StatusEffectKind.None)
                    {
                        continue;
                    }

                    visibleKinds.Add(status.Kind);
                    AddOrUpdateStatus(status);
                }
            }

            RemoveMissingStatuses(visibleKinds);
            ApplyLayout();
        }

        private bool EnsureInitialized()
        {
            if (_initialized)
            {
                return _slotTemplate != null;
            }

            _initialized = true;
            RectTransform root = Root;
            VerticalLayoutGroup legacyLayoutGroup = root != null
                ? root.GetComponent<VerticalLayoutGroup>()
                : null;
            if (legacyLayoutGroup != null)
            {
                legacyLayoutGroup.enabled = false;
            }

            int childCount = root != null ? root.childCount : 0;
            for (int index = 0; index < childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (TryCreateSlot(child.gameObject, index, out StatusSlot slot))
                {
                    slot.Root.gameObject.SetActive(false);
                    _slots.Add(slot);
                    _slotTemplate ??= slot;
                    if (slot.DisplayGroup == StatusEffectDisplayGroup.Buff)
                    {
                        _buffSlotTemplate ??= slot;
                        _buffBackgroundSprite ??= slot.Background.sprite;
                    }
                    else
                    {
                        _debuffSlotTemplate ??= slot;
                        _debuffBackgroundSprite ??= slot.Background.sprite;
                    }
                }
            }

            if (_slotTemplate == null)
            {
                ReportMissingReferences();
                return false;
            }

            return true;
        }

        private void AddOrUpdateStatus(StatusEffectViewData status)
        {
            StatusEffectDisplayGroup displayGroup =
                StatusEffectPresentationMapper.GetDisplayGroup(status.Kind);
            if (!_slotsByKind.TryGetValue(status.Kind, out StatusSlot slot))
            {
                slot = AcquireSlot(displayGroup);
                if (slot == null)
                {
                    return;
                }

                _slotsByKind.Add(status.Kind, slot);
                GetOrder(displayGroup).Add(status.Kind);
                slot.Root.gameObject.SetActive(true);
            }

            ApplySlotSize(slot.Root);
            slot.Background.sprite = GetBackgroundSprite(displayGroup, slot.Background.sprite);
            slot.Background.color = Color.white;
            slot.LabelText.text = FormatLabel(status);
        }

        private StatusSlot AcquireSlot(StatusEffectDisplayGroup displayGroup)
        {
            for (int index = 0; index < _slots.Count; index++)
            {
                StatusSlot slot = _slots[index];
                if (slot.DisplayGroup == displayGroup &&
                    !_slotsByKind.ContainsValue(slot))
                {
                    return slot;
                }
            }

            StatusSlot template = displayGroup == StatusEffectDisplayGroup.Buff
                ? _buffSlotTemplate
                : _debuffSlotTemplate;
            template ??= _slotTemplate;

            GameObject clone = UnityEngine.Object.Instantiate(
                template.Root.gameObject,
                Root);
            clone.name = "Player Status";
            if (!TryCreateSlot(clone, Root != null ? Root.childCount - 1 : 0, out StatusSlot created))
            {
                UnityEngine.Object.Destroy(clone);
                return null;
            }

            created.Root.gameObject.SetActive(false);
            _slots.Add(created);
            return created;
        }

        private void RemoveMissingStatuses(HashSet<StatusEffectKind> visibleKinds)
        {
            var removedKinds = new List<StatusEffectKind>();
            foreach (KeyValuePair<StatusEffectKind, StatusSlot> pair in _slotsByKind)
            {
                if (!visibleKinds.Contains(pair.Key))
                {
                    removedKinds.Add(pair.Key);
                }
            }

            for (int index = 0; index < removedKinds.Count; index++)
            {
                StatusEffectKind kind = removedKinds[index];
                StatusSlot slot = _slotsByKind[kind];
                slot.Root.DOKill(complete: true);
                slot.Root.gameObject.SetActive(false);
                _slotsByKind.Remove(kind);
                _buffOrder.Remove(kind);
                _debuffOrder.Remove(kind);
            }
        }

        private void ApplyLayout()
        {
            for (int index = 0; index < _buffOrder.Count; index++)
            {
                StatusSlot slot = _slotsByKind[_buffOrder[index]];
                PositionSlot(slot, anchorY: 1f, pivotY: 1f, anchoredY: -GetSlotHeight(slot) * index);
            }

            for (int index = 0; index < _debuffOrder.Count; index++)
            {
                StatusSlot slot = _slotsByKind[_debuffOrder[index]];
                PositionSlot(slot, anchorY: 0f, pivotY: 0f, anchoredY: GetSlotHeight(slot) * index);
            }
        }

        private static void PositionSlot(
            StatusSlot slot,
            float anchorY,
            float pivotY,
            float anchoredY)
        {
            RectTransform root = slot.Root;
            root.anchorMin = new Vector2(0.5f, anchorY);
            root.anchorMax = new Vector2(0.5f, anchorY);
            root.pivot = new Vector2(0.5f, pivotY);
            root.anchoredPosition = new Vector2(0f, anchoredY);
        }

        private static float GetSlotHeight(StatusSlot slot)
        {
            float height = slot.Root.sizeDelta.y;
            return height > 0f ? height : 16f;
        }

        private List<StatusEffectKind> GetOrder(StatusEffectDisplayGroup displayGroup)
        {
            return displayGroup == StatusEffectDisplayGroup.Buff
                ? _buffOrder
                : _debuffOrder;
        }

        private Sprite GetBackgroundSprite(
            StatusEffectDisplayGroup displayGroup,
            Sprite fallback)
        {
            if (displayGroup == StatusEffectDisplayGroup.Buff)
            {
                return _buffBackgroundSprite != null ? _buffBackgroundSprite : fallback;
            }

            return _debuffBackgroundSprite != null ? _debuffBackgroundSprite : fallback;
        }

        private void ApplySlotSize(RectTransform root)
        {
            root.sizeDelta = new Vector2(
                Mathf.Max(root.sizeDelta.x, _minimumSlotSize.x),
                Mathf.Max(root.sizeDelta.y, _minimumSlotSize.y));
        }

        private static string FormatLabel(StatusEffectViewData status)
        {
            string label = GetDisplayName(status.Kind);
            return status.ShowValue
                ? $"{label} {status.DisplayValue}"
                : label;
        }

        private static string GetDisplayName(StatusEffectKind kind)
        {
            switch (kind)
            {
                case StatusEffectKind.Burn:
                    return "화상";
                case StatusEffectKind.Freeze:
                    return "빙결";
                case StatusEffectKind.Infection:
                    return "감염";
                case StatusEffectKind.Vulnerable:
                    return "취약";
                case StatusEffectKind.Weaken:
                    return "약화";
                case StatusEffectKind.Lifesteal:
                    return "흡혈";
                case StatusEffectKind.Thorns:
                    return "가시";
                case StatusEffectKind.None:
                default:
                    return kind.ToString();
            }
        }

        private bool TryCreateSlot(GameObject root, int siblingIndex, out StatusSlot slot)
        {
            slot = null;
            if (root == null || root.transform is not RectTransform rectTransform)
            {
                return false;
            }

            Image background = root.GetComponent<Image>();
            TMP_Text labelText = root.GetComponentInChildren<TMP_Text>(includeInactive: true);
            if (background == null || labelText == null)
            {
                return false;
            }

            Image[] images = root.GetComponentsInChildren<Image>(includeInactive: true);
            for (int index = 0; index < images.Length; index++)
            {
                Image image = images[index];
                if (image != null && image != background)
                {
                    image.gameObject.SetActive(false);
                }
            }

            background.raycastTarget = false;
            labelText.raycastTarget = false;
            ApplySlotSize(rectTransform);

            slot = new StatusSlot(
                rectTransform,
                background,
                labelText,
                InferDisplayGroup(background.sprite, siblingIndex));
            return true;
        }

        private static StatusEffectDisplayGroup InferDisplayGroup(Sprite sprite, int siblingIndex)
        {
            if (sprite != null)
            {
                if (sprite.name.EndsWith("_1"))
                {
                    return StatusEffectDisplayGroup.Debuff;
                }

                if (sprite.name.EndsWith("_0"))
                {
                    return StatusEffectDisplayGroup.Buff;
                }
            }

            return siblingIndex >= 3
                ? StatusEffectDisplayGroup.Debuff
                : StatusEffectDisplayGroup.Buff;
        }

        private void ReportMissingReferences()
        {
            if (_reportedMissingReferences)
            {
                return;
            }

            _reportedMissingReferences = true;
            Debug.LogError(
                "[PlayerStatusPanelView] At least one status slot with a background Image and TMP text is required.",
                this);
        }

        private void KillSlotTweens(bool complete)
        {
            for (int index = 0; index < _slots.Count; index++)
            {
                StatusSlot slot = _slots[index];
                if (slot?.Root != null)
                {
                    slot.Root.DOKill(complete);
                }
            }
        }

        private sealed class StatusSlot
        {
            public StatusSlot(
                RectTransform root,
                Image background,
                TMP_Text labelText,
                StatusEffectDisplayGroup displayGroup)
            {
                Root = root;
                Background = background;
                LabelText = labelText;
                DisplayGroup = displayGroup;
            }

            public RectTransform Root { get; }

            public Image Background { get; }

            public TMP_Text LabelText { get; }

            public StatusEffectDisplayGroup DisplayGroup { get; }
        }
    }
}
