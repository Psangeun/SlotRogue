using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotRogue.UI.App
{
    public sealed class LobbyProfileSelectionView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _currentProfileImage;
        [SerializeField] private Color _emptyProfileColor = Color.black;
        [SerializeField] private TMP_Text _emptyText;
        [SerializeField] private LobbyProfileOptionView[] _optionViews;

        private bool _buttonsSubscribed;
        private bool _optionsSubscribed;
        private bool _bound;
        private bool _hasCurrentProfileImageDefaultColor;
        private Color _currentProfileImageDefaultColor = Color.white;

        internal event Action OpenRequested;

        internal event Action CloseRequested;

        internal event Action<int> OptionSelected;

        internal void Bind(LobbyProfileSelectionViewModel viewModel)
        {
            if (viewModel == null || _bound)
            {
                return;
            }

            if (!EnsureRuntimeLayout())
            {
                return;
            }

            _bound = true;
            OpenRequested += viewModel.Open;
            CloseRequested += viewModel.Close;
            OptionSelected += viewModel.SelectOption;

            viewModel.SetOptions(CreateOptionDefinitions());
            viewModel.State.Subscribe(Render).AddTo(this);
        }

        internal bool EnsureRuntimeLayout()
        {
            if (!ValidateRequiredReferences())
            {
                Debug.LogError(
                    "[LobbyProfileSelectionView] UI references must be wired in the inspector. " +
                    $"Missing: {BuildMissingReferenceSummary()}",
                    this);
                return false;
            }

            SubscribeButtons();
            SubscribeOptions();
            CaptureCurrentProfileImageDefaultColor();

            for (int index = 0; index < _optionViews.Length; index++)
            {
                _optionViews[index].EnsureRuntimeLayout();
            }

            _panel.SetActive(false);
            return true;
        }

        private void Render(LobbyProfileSelectionViewState state)
        {
            if (!ValidateRequiredReferences())
            {
                return;
            }

            LobbyProfileSelectionViewState safe =
                state ?? LobbyProfileSelectionViewState.Hidden;

            RenderOptions(safe.Options);
            RenderCurrentProfile(safe.SelectedOptionIndex);

            if (_emptyText != null)
            {
                _emptyText.gameObject.SetActive(!safe.HasUnlockedOptions);
            }

            _panel.SetActive(safe.IsVisible);
            if (safe.IsVisible)
            {
                _panel.transform.SetAsLastSibling();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
            UnsubscribeOptions();
        }

        private IReadOnlyList<LobbyProfileOptionDefinition> CreateOptionDefinitions()
        {
            var definitions = new LobbyProfileOptionDefinition[_optionViews.Length];
            for (int index = 0; index < _optionViews.Length; index++)
            {
                definitions[index] = _optionViews[index].CreateDefinition();
            }

            return definitions;
        }

        private void RenderOptions(
            IReadOnlyList<LobbyProfileOptionViewState> optionStates)
        {
            IReadOnlyList<LobbyProfileOptionViewState> safe =
                optionStates ?? Array.Empty<LobbyProfileOptionViewState>();
            for (int index = 0; index < _optionViews.Length; index++)
            {
                LobbyProfileOptionViewState state = index < safe.Count
                    ? safe[index]
                    : default;
                _optionViews[index].Render(state);
            }
        }

        private void RenderCurrentProfile(int selectedOptionIndex)
        {
            if (_currentProfileImage == null)
            {
                return;
            }

            if (selectedOptionIndex < 0 ||
                selectedOptionIndex >= _optionViews.Length)
            {
                RenderEmptyCurrentProfile();
                return;
            }

            Sprite icon = _optionViews[selectedOptionIndex].Icon;
            if (icon == null)
            {
                RenderEmptyCurrentProfile();
                return;
            }

            _currentProfileImage.sprite = icon;
            _currentProfileImage.preserveAspect = true;
            _currentProfileImage.color = _currentProfileImageDefaultColor;
        }

        private void CaptureCurrentProfileImageDefaultColor()
        {
            if (_hasCurrentProfileImageDefaultColor ||
                _currentProfileImage == null)
            {
                return;
            }

            _currentProfileImageDefaultColor = _currentProfileImage.color;
            _hasCurrentProfileImageDefaultColor = true;
        }

        private void RenderEmptyCurrentProfile()
        {
            _currentProfileImage.color = _emptyProfileColor;
        }

        private bool ValidateRequiredReferences()
        {
            if (_panel == null ||
                _openButton == null ||
                _closeButton == null ||
                _optionViews == null ||
                _optionViews.Length == 0)
            {
                return false;
            }

            for (int index = 0; index < _optionViews.Length; index++)
            {
                if (_optionViews[index] == null || !_optionViews[index].IsValid)
                {
                    return false;
                }
            }

            return true;
        }

        private string BuildMissingReferenceSummary()
        {
            var builder = new System.Text.StringBuilder();
            AppendMissing(builder, _panel != null, "Panel");
            AppendMissing(builder, _openButton != null, "Open Button");
            AppendMissing(builder, _closeButton != null, "Close Button");
            AppendMissing(
                builder,
                _optionViews != null && _optionViews.Length > 0,
                "Profile Options");

            if (_optionViews != null)
            {
                for (int index = 0; index < _optionViews.Length; index++)
                {
                    bool valid = _optionViews[index] != null &&
                        _optionViews[index].IsValid;
                    string missing = _optionViews[index] != null
                        ? _optionViews[index].BuildMissingReferenceSummary()
                        : "View";
                    AppendMissing(
                        builder,
                        valid,
                        $"Profile Option {index}: {missing}");
                }
            }

            return builder.Length > 0 ? builder.ToString() : "none";
        }

        private void SubscribeButtons()
        {
            if (_buttonsSubscribed)
            {
                return;
            }

            _openButton.onClick.AddListener(HandleOpenClicked);
            _closeButton.onClick.AddListener(HandleCloseClicked);
            _buttonsSubscribed = true;
        }

        private void UnsubscribeButtons()
        {
            if (!_buttonsSubscribed)
            {
                return;
            }

            if (_openButton != null)
            {
                _openButton.onClick.RemoveListener(HandleOpenClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(HandleCloseClicked);
            }

            _buttonsSubscribed = false;
        }

        private void SubscribeOptions()
        {
            if (_optionsSubscribed)
            {
                return;
            }

            for (int index = 0; index < _optionViews.Length; index++)
            {
                _optionViews[index].Clicked += HandleOptionClicked;
            }

            _optionsSubscribed = true;
        }

        private void UnsubscribeOptions()
        {
            if (!_optionsSubscribed || _optionViews == null)
            {
                return;
            }

            for (int index = 0; index < _optionViews.Length; index++)
            {
                if (_optionViews[index] != null)
                {
                    _optionViews[index].Clicked -= HandleOptionClicked;
                }
            }

            _optionsSubscribed = false;
        }

        private void HandleOpenClicked()
        {
            OpenRequested?.Invoke();
        }

        private void HandleCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        private void HandleOptionClicked(LobbyProfileOptionView option)
        {
            if (_optionViews == null)
            {
                return;
            }

            for (int index = 0; index < _optionViews.Length; index++)
            {
                if (_optionViews[index] == option)
                {
                    OptionSelected?.Invoke(index);
                    return;
                }
            }
        }

        private static void AppendMissing(
            System.Text.StringBuilder builder,
            bool hasReference,
            string label)
        {
            if (hasReference)
            {
                return;
            }

            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(label);
        }
    }
}
