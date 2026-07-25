using System;
using NUnit.Framework;
using SlotRogue.Core.Combat;

namespace SlotRogue.Core.Tests.Combat
{
    public sealed class EncounterScalingTests
    {
        [Test]
        public void Scale_NormalTier_UsesBaseHp()
        {
            var scaling = new EncounterScaling(Config());

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 1,
                themeSectionIndex: 0,
                tierMultiplier: 1f));

            Assert.That(result.MaxHp, Is.EqualTo(20));
        }

        [Test]
        public void Scale_EliteTier_UsesTierMultiplier()
        {
            var scaling = new EncounterScaling(Config(eliteTierMultiplier: 1.5f));

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 1,
                themeSectionIndex: 0,
                tierMultiplier: 1.5f));

            Assert.That(result.MaxHp, Is.EqualTo(30));
        }

        [Test]
        public void Scale_BossTier_UsesTierMultiplier()
        {
            var scaling = new EncounterScaling(Config(bossTierMultiplier: 2f));

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 1,
                themeSectionIndex: 0,
                tierMultiplier: 2f));

            Assert.That(result.MaxHp, Is.EqualTo(40));
        }

        [Test]
        public void Scale_BattleNumberIncrease_AddsBattleGrowth()
        {
            var scaling = new EncounterScaling(Config(increasePerBattle: 0.1f));

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 3,
                themeSectionIndex: 0,
                tierMultiplier: 1f));

            Assert.That(result.MaxHp, Is.EqualTo(24));
        }

        [Test]
        public void Scale_ThemeSectionIncrease_AddsThemeSectionGrowth()
        {
            var scaling = new EncounterScaling(Config(increasePerThemeSection: 0.25f));

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 1,
                themeSectionIndex: 2,
                tierMultiplier: 1f));

            Assert.That(result.MaxHp, Is.EqualTo(30));
        }

        [Test]
        public void Scale_EffectAmounts_UseSharedMultiplierAndPreserveZero()
        {
            var scaling = new EncounterScaling(Config(
                increasePerBattle: 0.1f,
                increasePerThemeSection: 0.25f,
                eliteTierMultiplier: 1.5f));

            EncounterScaleResult result = scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 20,
                battleNumber: 3,
                themeSectionIndex: 1,
                tierMultiplier: 1.5f));

            Assert.That(result.MaxHp, Is.EqualTo(44));
            Assert.That(result.ScaleAmount(4), Is.EqualTo(9));
            Assert.That(result.ScaleAmount(1), Is.EqualTo(2));
            Assert.That(result.ScaleAmount(0), Is.EqualTo(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => result.ScaleAmount(-1));
        }

        [Test]
        public void Scale_InvalidInput_Fails()
        {
            var scaling = new EncounterScaling(Config());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EncounterScaleRequest(baseMaxHp: 0, battleNumber: 1, themeSectionIndex: 0, tierMultiplier: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EncounterScaleRequest(baseMaxHp: 10, battleNumber: 0, themeSectionIndex: 0, tierMultiplier: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EncounterScaleRequest(baseMaxHp: 10, battleNumber: 1, themeSectionIndex: -1, tierMultiplier: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EncounterScaleRequest(baseMaxHp: 10, battleNumber: 1, themeSectionIndex: 0, tierMultiplier: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new EncounterBalanceConfig(
                    increasePerBattle: -0.1f,
                    increasePerThemeSection: 0f,
                    normalTierMultiplier: 1f,
                    eliteTierMultiplier: 1f,
                    bossTierMultiplier: 1f));

            Assert.DoesNotThrow(() => scaling.Scale(new EncounterScaleRequest(
                baseMaxHp: 10,
                battleNumber: 1,
                themeSectionIndex: 0,
                tierMultiplier: 1f)));
        }

        private static EncounterBalanceConfig Config(
            float increasePerBattle = 0f,
            float increasePerThemeSection = 0f,
            float normalTierMultiplier = 1f,
            float eliteTierMultiplier = 1f,
            float bossTierMultiplier = 1f)
        {
            return new EncounterBalanceConfig(
                increasePerBattle,
                increasePerThemeSection,
                normalTierMultiplier,
                eliteTierMultiplier,
                bossTierMultiplier);
        }
    }
}
