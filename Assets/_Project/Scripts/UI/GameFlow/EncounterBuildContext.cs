using System;
using SlotRogue.Core.Combat;
using SlotRogue.Data.GameFlow;

namespace SlotRogue.UI.GameFlow
{
    public readonly struct EncounterBuildContext
    {
        public EncounterBuildContext(
            EncounterTier tier,
            int battleNumber,
            int themeSectionIndex)
        {
            if (battleNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(battleNumber));
            }

            if (themeSectionIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(themeSectionIndex));
            }

            Tier = tier;
            BattleNumber = battleNumber;
            ThemeSectionIndex = themeSectionIndex;
        }

        public EncounterTier Tier { get; }

        public int BattleNumber { get; }

        public int ThemeSectionIndex { get; }

        public float ResolveTierMultiplier(EncounterBalanceConfig config)
        {
            return Tier switch
            {
                EncounterTier.Elite => config.EliteTierMultiplier,
                EncounterTier.Boss => config.BossTierMultiplier,
                _ => config.NormalTierMultiplier,
            };
        }
    }
}
