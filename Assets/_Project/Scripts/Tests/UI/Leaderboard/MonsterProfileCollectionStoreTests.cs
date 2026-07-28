using NUnit.Framework;
using SlotRogue.UI.Leaderboard;
using UnityEngine;

namespace SlotRogue.UI.Tests.Leaderboard
{
    public sealed class MonsterProfileCollectionStoreTests
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
        public void RecordDefeated_SavesUniqueMonsterProfileIds()
        {
            bool firstChanged = MonsterProfileCollectionStore.RecordDefeated("Slime");
            bool secondChanged = MonsterProfileCollectionStore.RecordDefeated("Slime");
            bool thirdChanged = MonsterProfileCollectionStore.RecordDefeated("Bat");

            Assert.That(firstChanged, Is.True);
            Assert.That(secondChanged, Is.False);
            Assert.That(thirdChanged, Is.True);
            Assert.That(
                MonsterProfileCollectionStore.LoadUnlockedIds(),
                Is.EquivalentTo(new[] { "Slime", "Bat" }));
        }

        [Test]
        public void TrySelectProfileIcon_RejectsLockedProfile()
        {
            bool selected =
                MonsterProfileCollectionStore.TrySelectProfileIcon("Slime");

            Assert.That(selected, Is.False);
            Assert.That(LeaderboardPlayerCosmeticStore.ProfileIconId, Is.Empty);
        }

        [Test]
        public void TrySelectProfileIcon_SavesUnlockedProfile()
        {
            MonsterProfileCollectionStore.RecordDefeated("Slime");

            bool selected =
                MonsterProfileCollectionStore.TrySelectProfileIcon("Slime");

            Assert.That(selected, Is.True);
            Assert.That(
                LeaderboardPlayerCosmeticStore.ProfileIconId,
                Is.EqualTo("Slime"));
        }

        [Test]
        public void TrySelectFirstUnlocked_UsesFirstUnlockedCandidate()
        {
            MonsterProfileCollectionStore.RecordDefeated("Portrait_Slime");

            bool selected = MonsterProfileCollectionStore.TrySelectFirstUnlocked(
                new[] { "SlimeButton", "Portrait_Slime" },
                out string selectedProfileIconId);

            Assert.That(selected, Is.True);
            Assert.That(selectedProfileIconId, Is.EqualTo("Portrait_Slime"));
            Assert.That(
                LeaderboardPlayerCosmeticStore.ProfileIconId,
                Is.EqualTo("Portrait_Slime"));
        }

        private static void ClearStores()
        {
            MonsterProfileCollectionStore.ResetForDebug();
            LeaderboardPlayerCosmeticStore.SaveProfileIcon(string.Empty);
            PlayerPrefs.Save();
        }
    }
}
