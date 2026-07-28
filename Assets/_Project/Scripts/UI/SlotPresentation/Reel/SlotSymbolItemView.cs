using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.SlotPresentation.Reel
{
    /// <summary>
    /// A single reusable symbol cell living inside a <see cref="SlotReelView"/> content strip.
    /// Wraps one <see cref="Image"/>; reels recycle these items instead of instantiating per spin.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SlotSymbolItemView : MonoBehaviour
    {
        private const float NativeSizeMultiplier = 1.25f;

        [SerializeField] private Image _icon;

        [Tooltip("'다시' 표식 배지. ReIcon 오브젝트를 Inspector에서 연결한다.")]
        [SerializeField] private GameObject _againMark;

        public RectTransform RectTransform => _rectTransform != null ? _rectTransform : _rectTransform = (RectTransform)transform;

        public Image Icon => _icon;

        public float LocalY => RectTransform.anchoredPosition.y;

        /// <summary>
        /// Builds an item anchored to the top-center of <paramref name="parent"/> (the reel content),
        /// sized to one cell. Items are positioned manually by the owning reel.
        /// </summary>
        public static SlotSymbolItemView Create(RectTransform parent, string itemName, Vector2 cellSize)
        {
            var go = new GameObject(itemName, typeof(RectTransform), typeof(Image));
            var item = go.AddComponent<SlotSymbolItemView>();
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = cellSize;
            rect.localScale = Vector3.one;

            var icon = go.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            item._icon = icon;
            item._rectTransform = rect;
            return item;
        }

        public void SetSprite(Sprite sprite)
        {
            if (_icon == null)
            {
                return;
            }

            _icon.sprite = sprite;
            _icon.enabled = sprite != null;
            ApplyNativeSize();
        }

        /// <summary>"다시" 표식 배지를 켜거나 끈다. 배지가 연결되어 있지 않으면 무시한다.</summary>
        public void SetAgainMark(bool on)
        {
            if (_againMark != null && _againMark.activeSelf != on)
            {
                _againMark.SetActive(on);
            }
        }

        public void SetLocalY(float y)
        {
            Vector2 position = RectTransform.anchoredPosition;
            position.y = y;
            RectTransform.anchoredPosition = position;
        }

        public void OffsetLocalY(float delta)
        {
            SetLocalY(LocalY + delta);
        }

        public void SetScale(float scale)
        {
            RectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        public void ApplyNativeSize()
        {
            if (_icon == null || _icon.sprite == null)
            {
                return;
            }

            _icon.SetNativeSize();
            RectTransform.sizeDelta *= NativeSizeMultiplier;
        }

        private RectTransform _rectTransform;
    }
}
