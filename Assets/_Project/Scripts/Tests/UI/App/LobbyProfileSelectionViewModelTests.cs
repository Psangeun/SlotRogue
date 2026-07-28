using NUnit.Framework;
using R3;
using SlotRogue.UI.App;
using SlotRogue.UI.Leaderboard;
using UnityEngine;

namespace SlotRogue.UI.Tests.App
{
    public sealed class LobbyProfileSelectionViewModelTests
    {
        [SetUp]
        public void SetUp()
        {
            ClearStores();
        }

        [TearDown]
        public void TearDown()
        {
            ClearStores();
        }

        [Test]
        public void Open_PublishesVisibleStateWithUnlockedOptions()
        {
            MonsterProfileCollectionStore.RecordDefeated("Slime");
            using var viewModel = new LobbyProfileSelectionViewModel();
            viewModel.SetOptions(new[]
            {
                new LobbyProfileOptionDefinition(new[] { "Slime" }, null),
                new LobbyProfileOptionDefinition(new[] { "Bat" }, null),
            });
            LobbyProfileSelectionViewState latest = null;
            viewModel.State.Subscribe(state => latest = state);

            viewModel.Open();

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest.IsVisible, Is.True);
            Assert.That(latest.HasUnlockedOptions, Is.True);
            Assert.That(latest.Options[0].IsUnlocked, Is.True);
            Assert.That(latest.Options[1].IsUnlocked, Is.False);
        }

        [Test]
        public void SelectOption_RejectsLockedOption()
        {
            using var viewModel = new LobbyProfileSelectionViewModel();
            viewModel.SetOptions(new[]
            {
                new LobbyProfileOptionDefinition(new[] { "Slime" }, null),
            });
            viewModel.Open();

            viewModel.SelectOption(0);

            Assert.That(LeaderboardPlayerCosmeticStore.ProfileIconId, Is.Empty);
            Assert.That(viewModel.State.CurrentValue.IsVisible, Is.True);
        }

        [Test]
        public void SelectOption_SavesUnlockedOptionAndClosesPanel()
        {
            MonsterProfileCollectionStore.RecordDefeated("Slime");
            using var viewModel = new LobbyProfileSelectionViewModel();
            viewModel.SetOptions(new[]
            {
                new LobbyProfileOptionDefinition(new[] { "Slime" }, null),
            });
            viewModel.Open();

            viewModel.SelectOption(0);

            Assert.That(
                LeaderboardPlayerCosmeticStore.ProfileIconId,
                Is.EqualTo("Slime"));
            Assert.That(viewModel.State.CurrentValue.IsVisible, Is.False);
            Assert.That(viewModel.State.CurrentValue.SelectedOptionIndex, Is.Zero);
            Assert.That(viewModel.State.CurrentValue.Options[0].IsSelected, Is.True);
        }

        private static void ClearStores()
        {
            MonsterProfileCollectionStore.ResetForDebug();
            LeaderboardPlayerCosmeticStore.SaveProfileIcon(string.Empty);
            PlayerPrefs.Save();
        }
    }
}
