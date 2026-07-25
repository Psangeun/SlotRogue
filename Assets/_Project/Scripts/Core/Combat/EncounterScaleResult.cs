using System;

namespace SlotRogue.Core.Combat
{
    public readonly struct EncounterScaleResult
    {
        public EncounterScaleResult(int maxHp, float multiplier)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            }

            if (multiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier));
            }

            MaxHp = maxHp;
            Multiplier = multiplier;
        }

        public int MaxHp { get; }

        public float Multiplier { get; }

        public int ScaleAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount == 0)
            {
                return 0;
            }

            return Math.Max(
                1,
                (int)Math.Round(amount * Multiplier, MidpointRounding.AwayFromZero));
        }
    }
}
