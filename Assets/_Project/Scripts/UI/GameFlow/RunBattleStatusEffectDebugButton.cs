using SlotRogue.Core.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.GameFlow
{
    public sealed class RunBattleStatusEffectDebugButton : MonoBehaviour
    {
        private static readonly StatusEffectKind[] DefaultStatusButtons =
        {
            StatusEffectKind.Burn,
            StatusEffectKind.Infection,
            StatusEffectKind.Vulnerable,
            StatusEffectKind.Weaken,
            StatusEffectKind.Thorns,
            StatusEffectKind.Lifesteal,
        };

        private static bool s_spawningClone;

        [SerializeField] private BattleSceneHost _compositionRoot;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private bool _createEnemyStatusButtons = true;
        [SerializeField] private StatusEffectKind _statusEffectKind = StatusEffectKind.Burn;
        [SerializeField, Min(1)] private int _amount = 3;

        private bool _spawnedStatusButtons;

        private void Awake()
        {
            ResolveReferences();
            if (!s_spawningClone)
            {
                SpawnDefaultStatusButtonsIfNeeded();
            }

            RefreshLabel();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClicked);
            }
        }

        private void HandleClicked()
        {
            ResolveReferences();
            if (_compositionRoot == null || _statusEffectKind == StatusEffectKind.None)
            {
                return;
            }

            _compositionRoot.DevApplyRelicStatusTurn(
                _statusEffectKind,
                _amount,
                TargetModeFor(_statusEffectKind));
        }

        private void SpawnDefaultStatusButtonsIfNeeded()
        {
            if (!_createEnemyStatusButtons || _spawnedStatusButtons || transform.parent == null)
            {
                return;
            }

            _spawnedStatusButtons = true;
            for (int index = 0; index < DefaultStatusButtons.Length; index++)
            {
                StatusEffectKind kind = DefaultStatusButtons[index];
                if (kind == _statusEffectKind)
                {
                    continue;
                }

                GameObject clone;
                try
                {
                    s_spawningClone = true;
                    clone = Instantiate(gameObject, transform.parent);
                }
                finally
                {
                    s_spawningClone = false;
                }

                clone.name = $"{name}_{kind}";
                if (clone.TryGetComponent(out RunBattleStatusEffectDebugButton debugButton))
                {
                    debugButton._compositionRoot = _compositionRoot;
                    debugButton._createEnemyStatusButtons = false;
                    debugButton._statusEffectKind = kind;
                    debugButton._amount = _amount;
                    debugButton.RefreshLabel();
                }
            }
        }

        private void ResolveReferences()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            if (_label == null)
            {
                _label = GetComponentInChildren<TMP_Text>(includeInactive: true);
            }

            if (_compositionRoot == null)
            {
                _compositionRoot = GetComponentInParent<BattleSceneHost>(includeInactive: true);
            }
        }

        private void RefreshLabel()
        {
            if (_label == null)
            {
                return;
            }

            _label.text = $"{_statusEffectKind} {_amount}";
        }

        private static CombatTargetMode TargetModeFor(StatusEffectKind kind)
        {
            switch (kind)
            {
                case StatusEffectKind.Thorns:
                case StatusEffectKind.Lifesteal:
                    return CombatTargetMode.Self;
                default:
                    return CombatTargetMode.SelectedEnemy;
            }
        }
    }
}
