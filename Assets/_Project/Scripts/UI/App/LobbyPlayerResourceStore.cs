using System;
using System.Globalization;
using UnityEngine;

namespace SlotRogue.UI.App
{
    public readonly struct LobbyPlayerResourceSnapshot
    {
        public LobbyPlayerResourceSnapshot(
            int energy,
            int maxEnergy,
            int startEnergyCost,
            int currency,
            DateTime lastEnergyUtc,
            DateTime nowUtc,
            TimeSpan energyRecoveryInterval)
        {
            Energy = Math.Max(0, energy);
            MaxEnergy = Math.Max(1, maxEnergy);
            StartEnergyCost = Math.Max(0, startEnergyCost);
            Currency = Math.Max(0, currency);
            LastEnergyUtc = lastEnergyUtc;
            NowUtc = nowUtc;
            EnergyRecoveryInterval = energyRecoveryInterval;
        }

        public int Energy { get; }

        public int MaxEnergy { get; }

        public int StartEnergyCost { get; }

        public int Currency { get; }

        public DateTime LastEnergyUtc { get; }

        public DateTime NowUtc { get; }

        public TimeSpan EnergyRecoveryInterval { get; }

        public bool HasStartEnergy => Energy >= StartEnergyCost;

        public bool IsEnergyFull => Energy >= MaxEnergy;

        public TimeSpan TimeUntilNextEnergy
        {
            get
            {
                if (IsEnergyFull)
                {
                    return TimeSpan.Zero;
                }

                TimeSpan elapsed = NowUtc - LastEnergyUtc;
                if (elapsed < TimeSpan.Zero)
                {
                    return EnergyRecoveryInterval;
                }

                TimeSpan remaining = EnergyRecoveryInterval - elapsed;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }
    }

    public static class LobbyPlayerResourceStore
    {
        public const int MaxEnergy = 30;
        public const int StartEnergyCost = 5;
        public const int EnergyRecoveryMinutes = 10;

        public const string EnergyKey = "SlotRogue.Lobby.Resources.Energy";
        public const string EnergyLastUtcTicksKey =
            "SlotRogue.Lobby.Resources.EnergyLastUtcTicks";
        public const string CurrencyKey = "SlotRogue.Lobby.Resources.Currency";

        public static readonly TimeSpan EnergyRecoveryInterval =
            TimeSpan.FromMinutes(EnergyRecoveryMinutes);

        public static event Action<LobbyPlayerResourceSnapshot> Changed;

        public static LobbyPlayerResourceSnapshot Load()
        {
            return Load(DateTime.UtcNow);
        }

        public static LobbyPlayerResourceSnapshot Load(DateTime utcNow)
        {
            DateTime now = NormalizeUtc(utcNow);
            bool changed = false;
            bool hadEnergy = PlayerPrefs.HasKey(EnergyKey);
            bool hadLastEnergy = PlayerPrefs.HasKey(EnergyLastUtcTicksKey);
            bool hadCurrency = PlayerPrefs.HasKey(CurrencyKey);

            int energy = hadEnergy
                ? Clamp(PlayerPrefs.GetInt(EnergyKey, MaxEnergy), 0, MaxEnergy)
                : MaxEnergy;
            int currency = hadCurrency
                ? Math.Max(0, PlayerPrefs.GetInt(CurrencyKey, 0))
                : 0;
            DateTime lastEnergyUtc = hadLastEnergy
                ? ReadLastEnergyUtc(now)
                : now;

            changed |= !hadEnergy || !hadLastEnergy || !hadCurrency;
            changed |= RefillEnergy(ref energy, ref lastEnergyUtc, now);

            var snapshot = CreateSnapshot(energy, currency, lastEnergyUtc, now);
            if (changed)
            {
                SaveSnapshot(snapshot);
            }

            return snapshot;
        }

        public static bool TrySpendStartEnergy(
            out LobbyPlayerResourceSnapshot snapshot)
        {
            return TrySpendEnergy(StartEnergyCost, DateTime.UtcNow, out snapshot);
        }

        public static bool TrySpendEnergy(
            int amount,
            DateTime utcNow,
            out LobbyPlayerResourceSnapshot snapshot)
        {
            DateTime now = NormalizeUtc(utcNow);
            snapshot = Load(now);

            int cost = Math.Max(0, amount);
            if (snapshot.Energy < cost)
            {
                return false;
            }

            int energyBeforeSpend = snapshot.Energy;
            int energyAfterSpend = energyBeforeSpend - cost;
            DateTime lastEnergyUtc = snapshot.LastEnergyUtc;
            if (cost > 0 &&
                energyBeforeSpend >= MaxEnergy &&
                energyAfterSpend < MaxEnergy)
            {
                lastEnergyUtc = now;
            }

            snapshot = CreateSnapshot(
                energyAfterSpend,
                snapshot.Currency,
                lastEnergyUtc,
                now);
            SaveSnapshot(snapshot);
            Changed?.Invoke(snapshot);
            return true;
        }

        public static LobbyPlayerResourceSnapshot AddCurrency(int amount)
        {
            return AddCurrency(amount, DateTime.UtcNow);
        }

        public static LobbyPlayerResourceSnapshot AddCurrency(
            int amount,
            DateTime utcNow)
        {
            LobbyPlayerResourceSnapshot current = Load(utcNow);
            int currency = Math.Max(0, current.Currency + Math.Max(0, amount));
            var snapshot = CreateSnapshot(
                current.Energy,
                currency,
                current.LastEnergyUtc,
                NormalizeUtc(utcNow));
            SaveSnapshot(snapshot);
            Changed?.Invoke(snapshot);
            return snapshot;
        }

        public static void ResetForDebug()
        {
            ResetForDebug(DateTime.UtcNow);
        }

        public static void ResetForDebug(DateTime utcNow)
        {
            PlayerPrefs.DeleteKey(EnergyKey);
            PlayerPrefs.DeleteKey(EnergyLastUtcTicksKey);
            PlayerPrefs.DeleteKey(CurrencyKey);
            PlayerPrefs.Save();
            Changed?.Invoke(Load(utcNow));
        }

        public static void SaveForDebug(
            int energy,
            int currency,
            DateTime lastEnergyUtc)
        {
            var snapshot = CreateSnapshot(
                Clamp(energy, 0, MaxEnergy),
                Math.Max(0, currency),
                NormalizeUtc(lastEnergyUtc),
                NormalizeUtc(lastEnergyUtc));
            SaveSnapshot(snapshot);
            Changed?.Invoke(snapshot);
        }

        private static LobbyPlayerResourceSnapshot CreateSnapshot(
            int energy,
            int currency,
            DateTime lastEnergyUtc,
            DateTime nowUtc)
        {
            return new LobbyPlayerResourceSnapshot(
                energy,
                MaxEnergy,
                StartEnergyCost,
                currency,
                NormalizeUtc(lastEnergyUtc),
                NormalizeUtc(nowUtc),
                EnergyRecoveryInterval);
        }

        private static bool RefillEnergy(
            ref int energy,
            ref DateTime lastEnergyUtc,
            DateTime now)
        {
            if (energy >= MaxEnergy)
            {
                return false;
            }

            if (now < lastEnergyUtc)
            {
                lastEnergyUtc = now;
                return true;
            }

            TimeSpan elapsed = now - lastEnergyUtc;
            int recovered = (int)(elapsed.Ticks / EnergyRecoveryInterval.Ticks);
            if (recovered <= 0)
            {
                return false;
            }

            int previousEnergy = energy;
            energy = Clamp(energy + recovered, 0, MaxEnergy);
            lastEnergyUtc = energy >= MaxEnergy
                ? now
                : lastEnergyUtc.AddTicks(
                    EnergyRecoveryInterval.Ticks * (energy - previousEnergy));

            return energy != previousEnergy;
        }

        private static void SaveSnapshot(LobbyPlayerResourceSnapshot snapshot)
        {
            PlayerPrefs.SetInt(EnergyKey, Clamp(snapshot.Energy, 0, MaxEnergy));
            PlayerPrefs.SetString(
                EnergyLastUtcTicksKey,
                NormalizeUtc(snapshot.LastEnergyUtc)
                    .Ticks
                    .ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetInt(CurrencyKey, Math.Max(0, snapshot.Currency));
            PlayerPrefs.Save();
        }

        private static DateTime ReadLastEnergyUtc(DateTime fallback)
        {
            string raw = PlayerPrefs.GetString(EnergyLastUtcTicksKey, string.Empty);
            if (!long.TryParse(
                    raw,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks) ||
                ticks <= 0)
            {
                return fallback;
            }

            try
            {
                return new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                return fallback;
            }
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            };
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}
