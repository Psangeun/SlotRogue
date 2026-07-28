using System;
using System.Collections.Generic;
using SlotRogue.Core.Combat;
using SlotRogue.Data.Combat;

namespace SlotRogue.UI.GameFlow
{
    public sealed class EnemyActionPlannerFactory
    {
        public IEnemyActionPlanner Create(MonsterTurnPatternDefinition pattern)
        {
            return Build(pattern).Planner;
        }

        public EnemyActionPlannerBuildResult Build(MonsterTurnPatternDefinition pattern)
        {
            return Build(pattern, scaleResult: null);
        }

        public EnemyActionPlannerBuildResult Build(
            MonsterTurnPatternDefinition pattern,
            EncounterScaleResult scaleResult)
        {
            return Build(pattern, (EncounterScaleResult?)scaleResult);
        }

        private EnemyActionPlannerBuildResult Build(
            MonsterTurnPatternDefinition pattern,
            EncounterScaleResult? scaleResult)
        {
            if (pattern == null)
            {
                return BuildDefaultFallback(scaleResult);
            }

            if (pattern.turns == null || pattern.turns.Length == 0)
            {
                return BuildEmptyPlanner();
            }

            var plans = new EnemyActionPlan[pattern.turns.Length];
            var presentations = new List<EnemyActionPresentation>();
            int nextActionKey = 1;
            for (int turnIndex = 0; turnIndex < pattern.turns.Length; turnIndex++)
            {
                plans[turnIndex] = ToActionPlan(
                    pattern.turns[turnIndex].actions,
                    presentations,
                    ref nextActionKey,
                    scaleResult);
            }

            return new EnemyActionPlannerBuildResult(
                new FixedSequenceEnemyActionPlanner(plans),
                new EnemyActionPresentationMap(presentations));
        }

        public IEnemyActionPlanner Create(IReadOnlyList<IReadOnlyList<CombatEffect>> turnEffects)
        {
            if (turnEffects == null || turnEffects.Count == 0)
            {
                return CreateDefaultFallback();
            }

            var plans = new EnemyActionPlan[turnEffects.Count];
            for (int turnIndex = 0; turnIndex < turnEffects.Count; turnIndex++)
            {
                plans[turnIndex] = new EnemyActionPlan(turnEffects[turnIndex]);
            }

            return new FixedSequenceEnemyActionPlanner(plans);
        }

        private static EnemyActionPlannerBuildResult BuildDefaultFallback(EncounterScaleResult? scaleResult)
        {
            return new EnemyActionPlannerBuildResult(
                CreateDefaultFallback(scaleResult),
                EnemyActionPresentationMap.Empty);
        }

        private static EnemyActionPlannerBuildResult BuildEmptyPlanner()
        {
            return new EnemyActionPlannerBuildResult(
                CreateEmptyPlanner(),
                EnemyActionPresentationMap.Empty);
        }

        private static IEnemyActionPlanner CreateDefaultFallback()
        {
            return CreateDefaultFallback(scaleResult: null);
        }

        private static IEnemyActionPlanner CreateDefaultFallback(EncounterScaleResult? scaleResult)
        {
            return new FixedSequenceEnemyActionPlanner(new[]
            {
                new EnemyActionPlan(new[]
                {
                    new CombatEffect(
                        CombatEffectKind.Damage,
                        ScaleAmount(2, scaleResult),
                        CombatEffectTarget.Enemy),
                }),
            });
        }

        private static IEnemyActionPlanner CreateEmptyPlanner()
        {
            return new FixedSequenceEnemyActionPlanner(new[]
            {
                new EnemyActionPlan(null),
            });
        }

        private static EnemyActionPlan ToActionPlan(
            IReadOnlyList<EnemyActionDefinition> actions,
            List<EnemyActionPresentation> presentations,
            ref int nextActionKey,
            EncounterScaleResult? scaleResult)
        {
            if (actions == null || actions.Count == 0)
            {
                return EnemyActionPlan.FromActions(null);
            }

            var plannedActions = new EnemyPlannedAction[actions.Count];
            for (int index = 0; index < actions.Count; index++)
            {
                EnemyActionDefinition action = actions[index];
                EnemyActionKey actionKey = new(nextActionKey++);
                if (action == null)
                {
                    plannedActions[index] = new EnemyPlannedAction(actionKey, string.Empty, null);
                    continue;
                }

                presentations.Add(new EnemyActionPresentation(
                    actionKey,
                    action.ActionName,
                    action.IntentIcon));
                plannedActions[index] = new EnemyPlannedAction(
                    actionKey,
                    action.ActionName,
                    ToActionEffect(action.Effect, scaleResult));
            }

            return EnemyActionPlan.FromActions(plannedActions);
        }

        private static EnemyActionEffect ToActionEffect(
            EnemyEffectDefinition definition,
            EncounterScaleResult? scaleResult)
        {
            if (definition == null)
            {
                throw new InvalidOperationException("Enemy action effect is not configured.");
            }

            switch (definition)
            {
                case DamageEffectDefinition damage:
                    return EnemyActionEffect.FromCombatEffect(new CombatEffect(
                        CombatEffectKind.Damage,
                        ScaleAmount(damage.Amount, scaleResult),
                        ToCombatEffectTarget(damage.Target)));
                case ShieldEffectDefinition shield:
                    return EnemyActionEffect.FromCombatEffect(new CombatEffect(
                        CombatEffectKind.Shield,
                        ScaleAmount(shield.Amount, scaleResult),
                        ToCombatEffectTarget(shield.Target)));
                case HealEffectDefinition heal:
                    return EnemyActionEffect.FromCombatEffect(new CombatEffect(
                        CombatEffectKind.Heal,
                        ScaleAmount(heal.Amount, scaleResult),
                        ToCombatEffectTarget(heal.Target)));
                case StatusEffectDefinition status:
                    return EnemyActionEffect.FromCombatEffect(CombatEffect.ApplyStatus(
                        StatusEffectSpec.FromAmount(
                            status.StatusKind,
                            ScaleStatusAmount(status, scaleResult)),
                        ToCombatEffectTarget(status.Target)));
                case LockSlotEffectDefinition lockSlot:
                    return EnemyActionEffect.LockSlot(lockSlot.LockCount, lockSlot.DurationTurns);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(definition),
                        definition.GetType(),
                        "Unsupported enemy effect definition.");
            }
        }

        private static int ScaleAmount(int amount, EncounterScaleResult? scaleResult)
        {
            return scaleResult.HasValue
                ? scaleResult.Value.ScaleAmount(amount)
                : amount;
        }

        private static int ScaleStatusAmount(
            StatusEffectDefinition status,
            EncounterScaleResult? scaleResult)
        {
            return status.StatusKind switch
            {
                StatusEffectKind.Vulnerable or
                StatusEffectKind.Weaken or
                StatusEffectKind.Lifesteal => status.Amount,
                _ => ScaleAmount(status.Amount, scaleResult),
            };
        }

        private static CombatEffectTarget ToCombatEffectTarget(CombatEffectTargetDefinition target)
        {
            switch (target.TargetMode)
            {
                case CombatTargetMode.Self:
                    return CombatEffectTarget.Self;
                case CombatTargetMode.SelectedEnemy:
                    return target.TargetParticipantId > 0
                        ? CombatEffectTarget.SelectedEnemy(
                            new CombatParticipantId(target.TargetParticipantId))
                        : CombatEffectTarget.Enemy;
                case CombatTargetMode.AllEnemies:
                    return new CombatEffectTarget(CombatTargetMode.AllEnemies);
                case CombatTargetMode.RandomEnemy:
                    return new CombatEffectTarget(CombatTargetMode.RandomEnemy);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(target),
                        target.TargetMode,
                        "Unsupported combat target mode.");
            }
        }

    }
}
