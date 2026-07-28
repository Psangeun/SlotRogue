using TMPro;
using UnityEngine;

namespace SlotRogue.UI.GameFlow
{
    /// <summary>
    /// 현재 전투의 스왑 잔여 횟수를 현재/최대 형식으로 표시한다.
    /// </summary>
    public sealed class RunBattleSwapHudView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _text;

        public void Render(RunBattleSwapState state, bool hidden)
        {
            bool visible = !hidden;
            if (_panel != null)
            {
                _panel.SetActive(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }

            if (!visible || _text == null)
            {
                return;
            }

            _text.text = $"{state.SwapsRemaining} / {state.MaxSwaps}";
        }
    }
}
