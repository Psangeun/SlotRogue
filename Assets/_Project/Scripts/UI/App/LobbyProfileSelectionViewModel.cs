using System;
using System.Collections.Generic;
using R3;
using SlotRogue.UI.Leaderboard;
using UnityEngine;

namespace SlotRogue.UI.App
{
    internal sealed class LobbyProfileSelectionViewModel : IDisposable
    {
        private readonly ReactiveProperty<LobbyProfileSelectionViewState> _state =
            new(LobbyProfileSelectionViewState.Hidden);
        private IReadOnlyList<LobbyProfileOptionDefinition> _options =
            Array.Empty<LobbyProfileOptionDefinition>();
        private bool _isVisible;
        private bool _disposed;

        internal LobbyProfileSelectionViewModel()
        {
            MonsterProfileCollectionStore.Changed += Publish;
            LeaderboardPlayerCosmeticStore.ProfileIconChanged +=
                HandleProfileIconChanged;
        }

        internal ReadOnlyReactiveProperty<LobbyProfileSelectionViewState> State =>
            _state;

        internal void SetOptions(IReadOnlyList<LobbyProfileOptionDefinition> options)
        {
            _options = options ?? Array.Empty<LobbyProfileOptionDefinition>();
            Publish();
        }

        internal void Open()
        {
            _isVisible = true;
            Publish();
        }

        internal void Close()
        {
            _isVisible = false;
            Publish();
        }

        internal void SelectOption(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= _options.Count)
            {
                return;
            }

            if (!MonsterProfileCollectionStore.TrySelectFirstUnlocked(
                    _options[optionIndex].ProfileIconIds,
                    out _))
            {
                return;
            }

            _isVisible = false;
            Publish();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            MonsterProfileCollectionStore.Changed -= Publish;
            LeaderboardPlayerCosmeticStore.ProfileIconChanged -=
                HandleProfileIconChanged;
            _state.Dispose();
            _disposed = true;
        }

        private void HandleProfileIconChanged(string profileIconId)
        {
            Publish();
        }

        private void Publish()
        {
            string selectedProfileIconId =
                LeaderboardPlayerCosmeticStore.ProfileIconId;
            var optionStates =
                new LobbyProfileOptionViewState[_options.Count];
            int selectedOptionIndex = -1;
            int unlockedCount = 0;

            for (int index = 0; index < _options.Count; index++)
            {
                LobbyProfileOptionDefinition option = _options[index];
                bool unlocked = MonsterProfileCollectionStore.TryGetFirstUnlocked(
                    option.ProfileIconIds,
                    out _);
                bool selected = ContainsId(
                    option.ProfileIconIds,
                    selectedProfileIconId);

                if (unlocked)
                {
                    unlockedCount++;
                }

                if (selected)
                {
                    selectedOptionIndex = index;
                }

                optionStates[index] =
                    new LobbyProfileOptionViewState(unlocked, selected);
            }

            _state.Value = new LobbyProfileSelectionViewState(
                _isVisible,
                unlockedCount > 0,
                selectedOptionIndex,
                optionStates);
        }

        private static bool ContainsId(
            IReadOnlyList<string> ids,
            string profileIconId)
        {
            string normalized = profileIconId?.Trim() ?? string.Empty;
            if (ids == null || string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            for (int index = 0; index < ids.Count; index++)
            {
                if (string.Equals(ids[index], normalized, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal readonly struct LobbyProfileOptionDefinition
    {
        internal LobbyProfileOptionDefinition(
            IReadOnlyList<string> profileIconIds,
            Sprite icon)
        {
            ProfileIconIds = profileIconIds ?? Array.Empty<string>();
            Icon = icon;
        }

        internal IReadOnlyList<string> ProfileIconIds { get; }

        internal Sprite Icon { get; }
    }

    internal sealed class LobbyProfileSelectionViewState
    {
        internal static readonly LobbyProfileSelectionViewState Hidden = new(
            false,
            false,
            -1,
            Array.Empty<LobbyProfileOptionViewState>());

        internal LobbyProfileSelectionViewState(
            bool isVisible,
            bool hasUnlockedOptions,
            int selectedOptionIndex,
            IReadOnlyList<LobbyProfileOptionViewState> options)
        {
            IsVisible = isVisible;
            HasUnlockedOptions = hasUnlockedOptions;
            SelectedOptionIndex = selectedOptionIndex;
            Options = options ?? Array.Empty<LobbyProfileOptionViewState>();
        }

        internal bool IsVisible { get; }

        internal bool HasUnlockedOptions { get; }

        internal int SelectedOptionIndex { get; }

        internal IReadOnlyList<LobbyProfileOptionViewState> Options { get; }
    }

    internal readonly struct LobbyProfileOptionViewState
    {
        internal LobbyProfileOptionViewState(bool isUnlocked, bool isSelected)
        {
            IsUnlocked = isUnlocked;
            IsSelected = isSelected;
        }

        internal bool IsUnlocked { get; }

        internal bool IsSelected { get; }
    }
}
