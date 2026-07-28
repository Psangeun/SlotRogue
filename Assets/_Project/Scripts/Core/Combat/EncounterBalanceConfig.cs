using System;

namespace SlotRogue.Core.Combat
{
    public readonly struct EncounterBalanceConfig
    {
        public float IncreasePerBattle { get; }
        public float IncreasePerThemeSection { get; }
        public float NormalTierMultiplier { get; }
        public float EliteTierMultiplier { get; }
        public float BossTierMultiplier { get; }

        public EncounterBalanceConfig(
            float increasePerBattle,
            float increasePerThemeSection,
            float normalTierMultiplier,
            float eliteTierMultiplier,
            float bossTierMultiplier)
        {
            if (increasePerBattle < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(increasePerBattle));
            }

            if (increasePerThemeSection < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(increasePerThemeSection));
            }

            if (normalTierMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(normalTierMultiplier));
            }

            if (eliteTierMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(eliteTierMultiplier));
            }

            if (bossTierMultiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(bossTierMultiplier));
            }

            IncreasePerBattle = increasePerBattle;
            IncreasePerThemeSection = increasePerThemeSection;
            NormalTierMultiplier = normalTierMultiplier;
            EliteTierMultiplier = eliteTierMultiplier;
            BossTierMultiplier = bossTierMultiplier;
        }
    }
}
