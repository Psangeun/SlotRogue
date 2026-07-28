using System;
using System.Collections.Generic;
using SlotRogue.Data.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.App
{
    public sealed class LobbyProfileOptionView : MonoBehaviour
    {
        [SerializeField] private MonsterDefinition _monsterDefinition;
        [SerializeField] private string _profileIconId;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedIndicator;
        [SerializeField] private GameObject _lockedOverlay;

        private readonly List<string> _candidateIds = new();
        private bool _buttonSubscribed;

        internal event Action<LobbyProfileOptionView> Clicked;

        internal bool IsValid => _button != null;

        internal Sprite Icon => _iconImage != null
            ? _iconImage.sprite
            : ResolveDefinitionPortrait();

        internal void EnsureRuntimeLayout()
        {
            if (_monsterDefinition != null &&
                _iconImage != null &&
                _iconImage.sprite == null)
            {
                Sprite portrait = ResolveDefinitionPortrait();
                if (portrait != null)
                {
                    _iconImage.sprite = portrait;
                    _iconImage.preserveAspect = true;
                }
            }

            if (_monsterDefinition != null &&
                _nameText != null &&
                string.IsNullOrWhiteSpace(_nameText.text))
            {
                _nameText.text = _monsterDefinition.name;
            }

            SubscribeButton();
        }

        internal LobbyProfileOptionDefinition CreateDefinition()
        {
            CollectCandidateIds(_candidateIds);
            return new LobbyProfileOptionDefinition(
                _candidateIds.ToArray(),
                Icon);
        }

        internal void Render(LobbyProfileOptionViewState state)
        {
            if (_button != null)
            {
                _button.interactable = state.IsUnlocked;
            }

            if (_selectedIndicator != null)
            {
                _selectedIndicator.SetActive(state.IsSelected);
            }

            if (_lockedOverlay != null)
            {
                _lockedOverlay.SetActive(!state.IsUnlocked);
            }

            if (_iconImage != null)
            {
                Color color = _iconImage.color;
                color.a = state.IsUnlocked ? 1f : 0.45f;
                _iconImage.color = color;
            }
        }

        internal string BuildMissingReferenceSummary()
        {
            return _button != null ? string.Empty : "Button";
        }

        private void OnDestroy()
        {
            UnsubscribeButton();
        }

        private void SubscribeButton()
        {
            if (_buttonSubscribed || _button == null)
            {
                return;
            }

            _button.onClick.AddListener(HandleClicked);
            _buttonSubscribed = true;
        }

        private void UnsubscribeButton()
        {
            if (!_buttonSubscribed || _button == null)
            {
                return;
            }

            _button.onClick.RemoveListener(HandleClicked);
            _buttonSubscribed = false;
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(this);
        }

        private void CollectCandidateIds(List<string> destination)
        {
            destination.Clear();
            AddCandidateId(destination, _profileIconId);

            if (_monsterDefinition != null)
            {
                AddCandidateId(destination, _monsterDefinition.name);

                MonsterVisualDefinition visual = _monsterDefinition.Visual;
                if (visual != null)
                {
                    AddCandidateId(destination, visual.name);
                    if (visual.Portrait != null)
                    {
                        AddCandidateId(destination, visual.Portrait.name);
                    }
                }
            }

            if (_iconImage != null && _iconImage.sprite != null)
            {
                AddCandidateId(destination, _iconImage.sprite.name);
            }
        }

        private Sprite ResolveDefinitionPortrait()
        {
            if (_monsterDefinition == null ||
                _monsterDefinition.Visual == null ||
                _monsterDefinition.Visual.Portrait == null)
            {
                return null;
            }

            return _monsterDefinition.Visual.Portrait;
        }

        private static void AddCandidateId(
            List<string> destination,
            string profileIconId)
        {
            string normalized = profileIconId?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(normalized) ||
                destination.Contains(normalized))
            {
                return;
            }

            destination.Add(normalized);
        }
    }
}
