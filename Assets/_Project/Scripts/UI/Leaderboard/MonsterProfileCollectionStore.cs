using System;
using System.Collections.Generic;
using SlotRogue.Data.Combat;
using UnityEngine;

namespace SlotRogue.UI.Leaderboard
{
    internal static class MonsterProfileCollectionStore
    {
        internal const string UnlockedMonsterIdsKey =
            "SlotRogue.Leaderboard.Profile.UnlockedMonsterIds";

        private const char EntrySeparator = '\n';

        internal static event Action Changed;

        internal static IReadOnlyCollection<string> LoadUnlockedIds()
        {
            return LoadUnlockedIdSet();
        }

        internal static bool IsUnlocked(string profileIconId)
        {
            string normalized = NormalizeId(profileIconId);
            return !string.IsNullOrEmpty(normalized) &&
                LoadUnlockedIdSet().Contains(normalized);
        }

        internal static bool TryGetFirstUnlocked(
            IEnumerable<string> profileIconIds,
            out string unlockedProfileIconId)
        {
            unlockedProfileIconId = string.Empty;
            if (profileIconIds == null)
            {
                return false;
            }

            HashSet<string> unlockedIds = LoadUnlockedIdSet();
            foreach (string profileIconId in profileIconIds)
            {
                string normalized = NormalizeId(profileIconId);
                if (string.IsNullOrEmpty(normalized) ||
                    !unlockedIds.Contains(normalized))
                {
                    continue;
                }

                unlockedProfileIconId = normalized;
                return true;
            }

            return false;
        }

        internal static bool TrySelectProfileIcon(string profileIconId)
        {
            string normalized = NormalizeId(profileIconId);
            if (string.IsNullOrEmpty(normalized) || !IsUnlocked(normalized))
            {
                return false;
            }

            LeaderboardPlayerCosmeticStore.SaveProfileIcon(normalized);
            return true;
        }

        internal static bool TrySelectFirstUnlocked(
            IEnumerable<string> profileIconIds,
            out string selectedProfileIconId)
        {
            if (!TryGetFirstUnlocked(profileIconIds, out selectedProfileIconId))
            {
                return false;
            }

            return TrySelectProfileIcon(selectedProfileIconId);
        }

        internal static bool RecordDefeated(MonsterDefinition monsterDefinition)
        {
            if (monsterDefinition == null)
            {
                return false;
            }

            var ids = new List<string>();
            AddUniqueId(ids, monsterDefinition.name);

            MonsterVisualDefinition visual = monsterDefinition.Visual;
            if (visual != null)
            {
                AddUniqueId(ids, visual.name);
                if (visual.Portrait != null)
                {
                    AddUniqueId(ids, visual.Portrait.name);
                }
            }

            return RecordDefeated(ids);
        }

        internal static bool RecordDefeated(string profileIconId)
        {
            string normalized = NormalizeId(profileIconId);
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            HashSet<string> unlockedIds = LoadUnlockedIdSet();
            if (!unlockedIds.Add(normalized))
            {
                return false;
            }

            SaveUnlockedIdSet(unlockedIds);
            return true;
        }

        internal static bool RecordDefeated(IEnumerable<string> profileIconIds)
        {
            if (profileIconIds == null)
            {
                return false;
            }

            bool changed = false;
            HashSet<string> unlockedIds = LoadUnlockedIdSet();
            foreach (string profileIconId in profileIconIds)
            {
                string normalized = NormalizeId(profileIconId);
                if (!string.IsNullOrEmpty(normalized) &&
                    unlockedIds.Add(normalized))
                {
                    changed = true;
                }
            }

            if (!changed)
            {
                return false;
            }

            SaveUnlockedIdSet(unlockedIds);
            return true;
        }

        internal static void ResetForDebug()
        {
            PlayerPrefs.DeleteKey(UnlockedMonsterIdsKey);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        private static HashSet<string> LoadUnlockedIdSet()
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            string raw = PlayerPrefs.GetString(UnlockedMonsterIdsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return result;
            }

            string[] entries = raw.Split(EntrySeparator);
            for (int index = 0; index < entries.Length; index++)
            {
                string normalized = NormalizeId(entries[index]);
                if (!string.IsNullOrEmpty(normalized))
                {
                    result.Add(normalized);
                }
            }

            return result;
        }

        private static void SaveUnlockedIdSet(HashSet<string> unlockedIds)
        {
            var orderedIds = new List<string>(unlockedIds);
            orderedIds.Sort(StringComparer.Ordinal);
            PlayerPrefs.SetString(
                UnlockedMonsterIdsKey,
                string.Join(EntrySeparator.ToString(), orderedIds));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        private static void AddUniqueId(List<string> ids, string profileIconId)
        {
            string normalized = NormalizeId(profileIconId);
            if (string.IsNullOrEmpty(normalized) || ids.Contains(normalized))
            {
                return;
            }

            ids.Add(normalized);
        }

        private static string NormalizeId(string profileIconId)
        {
            return profileIconId?.Trim() ?? string.Empty;
        }
    }
}
