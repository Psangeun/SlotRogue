using System;
using NUnit.Framework;
using SlotRogue.UI.App;
using UnityEngine;

namespace SlotRogue.UI.Tests.App
{
    public sealed class LobbyPlayerResourceStoreTests
    {
        private static readonly DateTime BaseUtc =
            new(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public void SetUp()
        {
            ClearResources();
        }

        [TearDown]
        public void TearDown()
        {
            ClearResources();
        }

        [Test]
        public void Load_FirstTime_StartsWithFullEnergyAndZeroCurrency()
        {
            LobbyPlayerResourceSnapshot snapshot =
                LobbyPlayerResourceStore.Load(BaseUtc);

            Assert.That(snapshot.Energy, Is.EqualTo(LobbyPlayerResourceStore.MaxEnergy));
            Assert.That(snapshot.Currency, Is.Zero);
            Assert.That(snapshot.HasStartEnergy, Is.True);
            Assert.That(PlayerPrefs.HasKey(LobbyPlayerResourceStore.EnergyKey), Is.True);
        }

        [Test]
        public void Load_RecoversOneEnergyEveryTenMinutesAndCapsAtMax()
        {
            LobbyPlayerResourceStore.SaveForDebug(28, 7, BaseUtc);

            LobbyPlayerResourceSnapshot snapshot =
                LobbyPlayerResourceStore.Load(BaseUtc.AddMinutes(25));

            Assert.That(snapshot.Energy, Is.EqualTo(LobbyPlayerResourceStore.MaxEnergy));
            Assert.That(snapshot.Currency, Is.EqualTo(7));
            Assert.That(snapshot.TimeUntilNextEnergy, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void TrySpendEnergy_FailsWhenCurrentEnergyIsBelowCost()
        {
            LobbyPlayerResourceStore.SaveForDebug(4, 0, BaseUtc);

            bool spent = LobbyPlayerResourceStore.TrySpendEnergy(
                LobbyPlayerResourceStore.StartEnergyCost,
                BaseUtc,
                out LobbyPlayerResourceSnapshot snapshot);

            Assert.That(spent, Is.False);
            Assert.That(snapshot.Energy, Is.EqualTo(4));
            Assert.That(snapshot.HasStartEnergy, Is.False);
        }

        [Test]
        public void TrySpendEnergy_SpendsCostAndStartsRecoveryWhenFull()
        {
            DateTime spendTime = BaseUtc.AddMinutes(3);
            LobbyPlayerResourceStore.SaveForDebug(
                LobbyPlayerResourceStore.MaxEnergy,
                2,
                BaseUtc);

            bool spent = LobbyPlayerResourceStore.TrySpendEnergy(
                LobbyPlayerResourceStore.StartEnergyCost,
                spendTime,
                out LobbyPlayerResourceSnapshot snapshot);

            Assert.That(spent, Is.True);
            Assert.That(
                snapshot.Energy,
                Is.EqualTo(
                    LobbyPlayerResourceStore.MaxEnergy -
                    LobbyPlayerResourceStore.StartEnergyCost));

            LobbyPlayerResourceSnapshot beforeTick =
                LobbyPlayerResourceStore.Load(spendTime.AddMinutes(9));
            LobbyPlayerResourceSnapshot afterTick =
                LobbyPlayerResourceStore.Load(spendTime.AddMinutes(10));

            Assert.That(beforeTick.Energy, Is.EqualTo(snapshot.Energy));
            Assert.That(afterTick.Energy, Is.EqualTo(snapshot.Energy + 1));
            Assert.That(afterTick.Currency, Is.EqualTo(2));
        }

        [Test]
        public void FormatEnergyTimerLabel_ShowsRemainingRecoveryTime()
        {
            LobbyPlayerResourceStore.SaveForDebug(25, 0, BaseUtc);

            LobbyPlayerResourceSnapshot initial =
                LobbyPlayerResourceStore.Load(BaseUtc);
            LobbyPlayerResourceSnapshot afterOneMinuteMinusFraction =
                LobbyPlayerResourceStore.Load(BaseUtc.AddSeconds(59.2));

            Assert.That(
                GameStartSceneRoot.FormatEnergyTimerLabel(initial),
                Is.EqualTo("10:00"));
            Assert.That(
                GameStartSceneRoot.FormatEnergyTimerLabel(
                    afterOneMinuteMinusFraction),
                Is.EqualTo("09:01"));
        }

        [Test]
        public void FormatEnergyTimerLabel_HidesWhenEnergyIsFull()
        {
            LobbyPlayerResourceStore.SaveForDebug(
                LobbyPlayerResourceStore.MaxEnergy,
                0,
                BaseUtc);

            LobbyPlayerResourceSnapshot snapshot =
                LobbyPlayerResourceStore.Load(BaseUtc);

            Assert.That(
                GameStartSceneRoot.FormatEnergyTimerLabel(snapshot),
                Is.Empty);
        }

        [Test]
        public void AddCurrency_PersistsCurrencyWithoutGrantingEnergy()
        {
            LobbyPlayerResourceStore.SaveForDebug(12, 1, BaseUtc);

            LobbyPlayerResourceSnapshot snapshot =
                LobbyPlayerResourceStore.AddCurrency(9, BaseUtc.AddMinutes(5));

            Assert.That(snapshot.Currency, Is.EqualTo(10));
            Assert.That(snapshot.Energy, Is.EqualTo(12));
        }

        private static void ClearResources()
        {
            PlayerPrefs.DeleteKey(LobbyPlayerResourceStore.EnergyKey);
            PlayerPrefs.DeleteKey(LobbyPlayerResourceStore.EnergyLastUtcTicksKey);
            PlayerPrefs.DeleteKey(LobbyPlayerResourceStore.CurrencyKey);
            PlayerPrefs.Save();
        }
    }
}
