using System;
using SlotRogue.Core.Combat;
using SlotRogue.Data.Combat;

namespace SlotRogue.UI.GameFlow
{
    public sealed class EnemyCombatantFactory
    {
        private readonly EnemyActionPlannerFactory _plannerFactory;

        public EnemyCombatantFactory()
            : this(new EnemyActionPlannerFactory())
        {
        }

        public EnemyCombatantFactory(EnemyActionPlannerFactory plannerFactory)
        {
            _plannerFactory = plannerFactory ?? throw new ArgumentNullException(nameof(plannerFactory));
        }

        public EnemyCombatant Create(MonsterDefinition definition, int rosterIndex)
        {
            return CreateWithPresentation(definition, rosterIndex).Combatant;
        }

        public EnemyCombatantBuildResult CreateWithPresentation(MonsterDefinition definition, int rosterIndex)
        {
            return CreateWithPresentation(definition, rosterIndex, maxHpOverride: null);
        }

        public EnemyCombatantBuildResult CreateWithPresentation(
            MonsterDefinition definition,
            int rosterIndex,
            int? maxHpOverride)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return CreateWithPresentation(definition, rosterIndex, definition.maxHp);
        }

        public EnemyCombatantBuildResult CreateWithPresentation(
            MonsterDefinition definition,
            int rosterIndex,
            int maxHp)
        {
            return CreateWithPresentation(definition, rosterIndex, maxHp, scaleResult: null);
        }

        public EnemyCombatantBuildResult CreateWithPresentation(
            MonsterDefinition definition,
            int rosterIndex,
            EncounterScaleResult scaleResult)
        {
            return CreateWithPresentation(
                definition,
                rosterIndex,
                scaleResult.MaxHp,
                scaleResult);
        }

        private EnemyCombatantBuildResult CreateWithPresentation(
            MonsterDefinition definition,
            int rosterIndex,
            int maxHp,
            EncounterScaleResult? scaleResult)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            }

            EnemyActionPlannerBuildResult plannerResult = scaleResult.HasValue
                ? _plannerFactory.Build(definition.turnPattern, scaleResult.Value)
                : _plannerFactory.Build(definition.turnPattern);
            EnemyCombatant combatant = Create(rosterIndex, maxHp, plannerResult.Planner);
            return new EnemyCombatantBuildResult(combatant, plannerResult.PresentationMap);
        }

        public EnemyCombatant Create(
            int rosterIndex,
            int maxHp,
            IEnemyActionPlanner planner)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            }

            CombatParticipant participant = RunCombatParticipantFactory.CreateEnemy(rosterIndex, maxHp);
            return new EnemyCombatant(
                participant,
                planner ?? throw new ArgumentNullException(nameof(planner)));
        }
    }
}
