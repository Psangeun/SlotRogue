using SlotRogue.Core.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace SlotRogue.Data.GameFlow
{
    [CreateAssetMenu(
        fileName = "EncounterBalanceSettings",
        menuName = "SlotRogue/GameFlow/Encounter Balance Settings")]
    public sealed class EncounterBalanceSettings : ScriptableObject
    {
        [SerializeField, FormerlySerializedAs("_hpIncreasePerBattle")]
        private float _increasePerBattle = 0.05f;
        [SerializeField, FormerlySerializedAs("_hpIncreasePerThemeSection"), FormerlySerializedAs("_hpIncreasePerCycle")]
        private float _increasePerThemeSection = 0.25f;
        [SerializeField, FormerlySerializedAs("_normalTierHpMultiplier")]
        private float _normalTierMultiplier = 1f;
        [SerializeField, FormerlySerializedAs("_eliteTierHpMultiplier")]
        private float _eliteTierMultiplier = 1.35f;
        [SerializeField, FormerlySerializedAs("_bossTierHpMultiplier")]
        private float _bossTierMultiplier = 1.8f;

        public EncounterBalanceConfig CreateConfig()
        {
            return new EncounterBalanceConfig(
                _increasePerBattle,
                _increasePerThemeSection,
                _normalTierMultiplier,
                _eliteTierMultiplier,
                _bossTierMultiplier);
        }
    }
}
