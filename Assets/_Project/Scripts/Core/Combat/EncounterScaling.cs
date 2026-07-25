using System;

namespace SlotRogue.Core.Combat
{
    public sealed class EncounterScaling
    {
        private readonly EncounterBalanceConfig _config;

        public EncounterScaling(EncounterBalanceConfig config)
        {
            _config = config;
        }

        public EncounterScaleResult Scale(EncounterScaleRequest request)
        {
            float multiplier = ResolveMultiplier(request);
            int maxHp = Math.Max(
                1,
                (int)Math.Round(request.BaseMaxHp * multiplier, MidpointRounding.AwayFromZero));
            return new EncounterScaleResult(maxHp, multiplier);
        }

        private float ResolveMultiplier(EncounterScaleRequest request)
        {
            float battleGrowth = (request.BattleNumber - 1) * _config.IncreasePerBattle;
            float themeSectionGrowth = request.ThemeSectionIndex * _config.IncreasePerThemeSection;
            return (1f + battleGrowth + themeSectionGrowth) * request.TierMultiplier;
        }
    }
}
