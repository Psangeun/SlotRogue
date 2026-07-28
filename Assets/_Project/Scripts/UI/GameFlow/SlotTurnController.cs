using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SlotRogue.Core.Combat;
using SlotRogue.Relics.Pool;
using SlotRogue.Slot.Data;
using SlotRogue.Slot.ViewModels;
using SlotRogue.UI.SlotPresentation;
using UnityEngine;

namespace SlotRogue.UI.GameFlow
{
    internal sealed class SlotTurnController
    {
        private readonly SlotMachineModel _slotViewModel;
        private readonly RunBattleSpinSequence _spinSequence;
        private readonly SlotPresentationManager _presentationManager;
        private readonly Func<string, Sprite> _relicIconResolver;

        internal event Action<SlotPatternPresentationResult> PatternPresentationStarted;

        internal event Action<SlotPatternPresentationResult> PatternPresentationCompleted;

        internal event Action LeverRaiseStarted;

        internal SlotTurnController(
            SlotMachineModel slotViewModel,
            RunBattleSpinSequence spinSequence,
            SlotPresentationManager presentationManager,
            Func<string, Sprite> relicIconResolver = null)
        {
            _slotViewModel = slotViewModel;
            _spinSequence = spinSequence;
            _presentationManager = presentationManager;
            _relicIconResolver = relicIconResolver;
        }

        internal void SetupImmediate()
        {
            _spinSequence.SetupImmediate();
            _presentationManager?.ShowImmediate(CreateInitialSlotDisplayResult());
        }

        internal async UniTask<SlotTurnResult> SpinAsync(SlotTurnRequest request)
        {
            _spinSequence.Reset();
            await _spinSequence.PlayDownAsync(request.CancellationToken);
            _spinSequence.StartSpin();
            _slotViewModel.Spin();
            await PlaySpinPresentationAsync(
                _slotViewModel.CurrentSpinResult,
                request.CancellationToken);
            return BuildCurrentPreviewTurnResult(
                spinCoinReward: 0,
                runCoinsAfterReward: GameFlowSession.RunCoins);
        }

        // Plays the animated reel spin and settles the lever/frame, leaving the spun result on
        // screen so the player can swap symbols before the pattern resolution runs.
        private async UniTask PlaySpinPresentationAsync(
            SlotSpinResult spinResult,
            CancellationToken cancellationToken)
        {
            if (_presentationManager == null)
            {
                await _spinSequence.SettleIfNeededAsync(cancellationToken);
                return;
            }

            void HandleSlotReelStopped(int reelIndex)
            {
                _spinSequence.SetReelIdle(reelIndex);
            }

            _presentationManager.SlotReelStopped += HandleSlotReelStopped;
            try
            {
                bool spinDone = false;
                _presentationManager.PlaySpinOnly(spinResult, () => spinDone = true);
                await UniTask.WaitUntil(
                    () => spinDone,
                    cancellationToken: cancellationToken);
                await _spinSequence.SettleIfNeededAsync(cancellationToken);
            }
            finally
            {
                _presentationManager.SlotReelStopped -= HandleSlotReelStopped;
            }
        }

        // 스핀 생성 직후 상위 레이어가 계산한 "다시" 표식을 모델에 주입하고 릴에 아이콘을 표시한다.
        internal void ApplyAgainMarks(IReadOnlyList<bool> marks)
        {
            _slotViewModel.SetAgainMarks(marks);
            _presentationManager?.ShowAgainMarks(_slotViewModel.CurrentAgainMarks);
        }

        // 스왑으로 표식이 심볼을 따라 이동한 뒤, 현재 모델 표식을 릴 아이콘에 다시 반영한다.
        internal void RefreshAgainMarks()
        {
            _presentationManager?.ShowAgainMarks(_slotViewModel.CurrentAgainMarks);
        }

        internal bool TrySwapCurrentSpinResult(
            int firstIndex,
            int secondIndex,
            out SlotTurnResult slotTurnResult)
        {
            bool swapped = _slotViewModel.TrySwapAdjacentSymbols(firstIndex, secondIndex);
            slotTurnResult = BuildCurrentPreviewTurnResult(
                spinCoinReward: 0,
                runCoinsAfterReward: GameFlowSession.RunCoins);
            return swapped;
        }

        // 두 셀이 자리를 바꾸는 연출을 재생하고, 연출이 끝나면(최종 결과로 정착) 완료된다.
        // 모델 스왑(TrySwapCurrentSpinResult) 이후에 호출해야 하며, 정착 결과는 현재 스핀 결과다.
        internal async UniTask PlaySwapPresentationAsync(int firstIndex, int secondIndex)
        {
            if (_presentationManager == null)
            {
                return;
            }

            var completion = new UniTaskCompletionSource();
            _presentationManager.PlaySwap(
                firstIndex,
                secondIndex,
                _slotViewModel.CurrentSpinResult,
                () => completion.TrySetResult());
            await completion.Task;
        }

        internal SlotTurnResult ResolveCurrentSpinResult(
            int spinCoinReward,
            int runCoinsAfterReward)
        {
            _slotViewModel.ResolveCurrentSpinResult();
            return BuildCurrentTurnResult(spinCoinReward, runCoinsAfterReward);
        }

        internal async UniTask PlayPresentationAsync(
            SlotTurnResult slotTurnResult,
            RelicResolveResult relicResult,
            RunCombatRequestResult combatRequestResult,
            RelicSpecResolveResult specResult,
            CancellationToken cancellationToken)
        {
            if (_presentationManager == null)
            {
                await _spinSequence.SettleIfNeededAsync(cancellationToken);
                return;
            }

            SlotPresentationResult presentationResult =
                BuildPresentationResult(slotTurnResult, relicResult, combatRequestResult, specResult);
            bool presentationDone = false;

            void HandlePatternStepStarted(SlotPatternPresentationResult pattern)
            {
                PatternPresentationStarted?.Invoke(pattern);
            }

            void HandlePatternStepCompleted(SlotPatternPresentationResult pattern)
            {
                PatternPresentationCompleted?.Invoke(pattern);
            }

            _presentationManager.PatternStepStarted += HandlePatternStepStarted;
            _presentationManager.PatternStepCompleted += HandlePatternStepCompleted;
            try
            {
                _presentationManager.PlayResolved(presentationResult, _ => presentationDone = true);

                await UniTask.WaitUntil(
                    () => presentationDone,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _presentationManager.PatternStepStarted -= HandlePatternStepStarted;
                _presentationManager.PatternStepCompleted -= HandlePatternStepCompleted;
                PatternPresentationCompleted?.Invoke(null);
            }
        }

        internal async UniTask BeforeBattleEventPresentedAsync(
            CombatEvent combatEvent,
            int eventIndex,
            IReadOnlyList<CombatEvent> events)
        {
            if (ShouldRaiseLeverBeforeEvent(combatEvent))
            {
                await RaiseLeverForTurnAsync();
            }
        }

        internal async UniTask AfterBattleEventPresentedAsync(
            CombatEvent combatEvent,
            int eventIndex,
            IReadOnlyList<CombatEvent> events)
        {
            if (IsLastPlayerAttackPresentation(combatEvent, eventIndex, events))
            {
                await RaiseLeverForTurnAsync();
            }
        }

        internal void ResetImmediate()
        {
            _spinSequence.ResetImmediate();
        }

        private async UniTask RaiseLeverForTurnAsync()
        {
            if (_spinSequence.LeverRaised)
            {
                return;
            }

            LeverRaiseStarted?.Invoke();
            await _spinSequence.RaiseLeverIfNeededAsync();
        }

        private SlotPresentationResult BuildPresentationResult(
            SlotTurnResult slotTurnResult,
            RelicResolveResult relicResult,
            RunCombatRequestResult combatRequestResult,
            RelicSpecResolveResult specResult)
        {
            IReadOnlyList<SlotPatternMatch> matches = slotTurnResult.PatternMatches;
            IReadOnlyList<bool> againMarks = slotTurnResult.AgainMarks;
            int[] retriggerRepeatCounts = BuildRetriggerRepeatCounts(specResult, matches.Count);
            int retriggerRepeatTotal = Sum(retriggerRepeatCounts);
            var patternPresentations =
                new List<SlotPatternPresentationResult>(matches.Count + retriggerRepeatTotal);

            // "다시" 반복 연출의 sfxLevel은 유물 매칭(TriggerPatternIndex 0..matches.Count-1, -1)과
            // 겹치지 않도록 matches.Count 이상에서 부여한다(반복엔 유물이 붙지 않음).
            int repeatSfxLevel = matches.Count;

            for (int index = 0; index < matches.Count; index++)
            {
                SlotPatternMatch match = matches[index];
                var cellIndices = new int[match.MatchedCells.Count];
                int againInPattern = 0;

                for (int cellIndex = 0; cellIndex < match.MatchedCells.Count; cellIndex++)
                {
                    SlotCell cell = match.MatchedCells[cellIndex];
                    int flatIndex = SlotSpinResult.ToIndex(cell.Col, cell.Row);
                    cellIndices[cellIndex] = flatIndex;
                    if (againMarks != null && flatIndex < againMarks.Count && againMarks[flatIndex])
                    {
                        againInPattern++;
                    }
                }

                int row = match.MatchedCells.Count > 0 ? match.MatchedCells[0].Row : -1;
                int col = match.MatchedCells.Count > 0 ? match.MatchedCells[0].Col : -1;
                bool isFinale = match.Definition.IsJackpot;
                string countText = $"{match.Symbol} x{match.MatchedCells.Count} / x{match.Multiplier:0.0}";
                string bonusText = $"+{match.CalculatedValue} DMG";

                patternPresentations.Add(new SlotPatternPresentationResult(
                    match.PresentationTitle,
                    match.Symbol,
                    row,
                    col,
                    match.MatchedCells.Count,
                    cellIndices,
                    countText,
                    bonusText,
                    isFinale,
                    index,
                    match.CalculatedValue));

                // 이 족보에 든 "다시" 표식 수만큼 같은 족보 연출을 이어서 한 번 더 재생한다.
                for (int repeat = 0; repeat < againInPattern; repeat++)
                {
                    patternPresentations.Add(new SlotPatternPresentationResult(
                        $"{match.PresentationTitle} 다시!",
                        match.Symbol,
                        row,
                        col,
                        match.MatchedCells.Count,
                        cellIndices,
                        countText,
                        bonusText,
                        isFinale,
                        repeatSfxLevel++,
                        match.CalculatedValue));
                }

                int retriggerInPattern = retriggerRepeatCounts[index];
                for (int repeat = 0; repeat < retriggerInPattern; repeat++)
                {
                    patternPresentations.Add(new SlotPatternPresentationResult(
                        $"{match.PresentationTitle} 한번 더!",
                        match.Symbol,
                        row,
                        col,
                        match.MatchedCells.Count,
                        cellIndices,
                        countText,
                        bonusText,
                        isFinale,
                        repeatSfxLevel++,
                        match.CalculatedValue));
                }
            }

            SlotCombatRequest request =
                combatRequestResult?.FinalRequest ?? SlotCombatRequest.Empty;
            SlotRelicTriggerPresentationResult[] relicPresentations =
                BuildRelicPresentations(
                    relicResult,
                    combatRequestResult,
                    combatRequestResult?.BaseRequest);
            var finalResult = new SlotFinalPresentationResult(
                request.Damage,
                request.Defense,
                request.AttackCount,
                request.HealAmount,
                BuildFinalSummaryText(request));

            return new SlotPresentationResult(
                slotTurnResult.SpinResult,
                patternPresentations,
                relicPresentations,
                finalResult);
        }

        private static int[] BuildRetriggerRepeatCounts(
            RelicSpecResolveResult specResult,
            int matchCount)
        {
            var counts = new int[matchCount];
            IReadOnlyList<RelicPatternRepeat> repeats = specResult?.RetriggerPatternRepeats;
            if (repeats == null || repeats.Count == 0)
            {
                return counts;
            }

            for (int index = 0; index < repeats.Count; index++)
            {
                RelicPatternRepeat repeat = repeats[index];
                if (repeat.PatternIndex < 0 ||
                    repeat.PatternIndex >= counts.Length ||
                    repeat.Count <= 0)
                {
                    continue;
                }

                counts[repeat.PatternIndex] += repeat.Count;
            }

            return counts;
        }

        private static int Sum(IReadOnlyList<int> values)
        {
            int sum = 0;
            for (int index = 0; index < values.Count; index++)
            {
                sum += values[index];
            }

            return sum;
        }

        private SlotTurnResult BuildCurrentTurnResult(int spinCoinReward, int runCoinsAfterReward)
        {
            return new SlotTurnResult(
                _slotViewModel.CurrentSpinResult,
                _slotViewModel.CurrentPatternMatches,
                _slotViewModel.CurrentPatternResult,
                _slotViewModel.CurrentCombatRequest,
                spinCoinReward,
                runCoinsAfterReward,
                _slotViewModel.IsCurrentSpinResolved,
                CopyAgainMarks());
        }

        private SlotTurnResult BuildCurrentPreviewTurnResult(int spinCoinReward, int runCoinsAfterReward)
        {
            return new SlotTurnResult(
                _slotViewModel.CurrentSpinResult,
                _slotViewModel.PreviewCurrentPatternMatches(),
                _slotViewModel.PreviewCurrentPatternResult(),
                SlotCombatRequest.Empty,
                spinCoinReward,
                runCoinsAfterReward,
                isResolved: false,
                againMarks: CopyAgainMarks());
        }

        private bool[] CopyAgainMarks()
        {
            IReadOnlyList<bool> marks = _slotViewModel.CurrentAgainMarks;
            var copy = new bool[marks.Count];
            for (int index = 0; index < marks.Count; index++)
            {
                copy[index] = marks[index];
            }

            return copy;
        }

        private SlotRelicTriggerPresentationResult[] BuildRelicPresentations(
            RelicResolveResult relicResult,
            RunCombatRequestResult combatRequestResult,
            SlotCombatRequest baseRequest)
        {
            IReadOnlyList<RelicContributionDelta> contributions =
                CombineRelicContributions(relicResult, combatRequestResult);
            if (contributions == null || contributions.Count == 0)
            {
                return Array.Empty<SlotRelicTriggerPresentationResult>();
            }

            int attackCount = Math.Max(1, baseRequest?.AttackCount ?? 1);
            int attackPower = Math.Max(0, baseRequest?.Damage ?? 0) * attackCount;
            var results = new SlotRelicTriggerPresentationResult[contributions.Count];

            for (int index = 0; index < contributions.Count; index++)
            {
                RelicContributionDelta contribution = contributions[index];
                RelicDefinition definition = RelicCatalog.GetById(contribution.RelicId);
                int addedAttackPower = contribution.DamagePerHit * attackCount;
                int previousAttackPower = attackPower;
                attackPower += addedAttackPower;

                results[index] = new SlotRelicTriggerPresentationResult(
                    contribution.RelicId,
                    contribution.RelicName,
                    _relicIconResolver?.Invoke(contribution.RelicId),
                    definition?.Description ?? ResolveRelicDescription(
                        contribution.RelicId,
                        contribution.RelicName),
                    BuildRelicValueText(
                        previousAttackPower,
                        attackPower,
                        addedAttackPower,
                        contribution.Block,
                        contribution.Heal),
                    contribution.TriggerPatternIndex,
                    contribution.DamagePerHit,
                    contribution.Block,
                    contribution.Heal);
            }

            return results;
        }

        private static IReadOnlyList<RelicContributionDelta> CombineRelicContributions(
            RelicResolveResult relicResult,
            RunCombatRequestResult combatRequestResult)
        {
            IReadOnlyList<RelicContributionDelta> direct = relicResult?.Contributions;
            IReadOnlyList<RelicContributionDelta> derived =
                combatRequestResult?.DerivedHealContributions;

            int directCount = direct?.Count ?? 0;
            int derivedCount = derived?.Count ?? 0;

            if (directCount == 0)
            {
                return derivedCount == 0 ? Array.Empty<RelicContributionDelta>() : derived;
            }

            if (derivedCount == 0)
            {
                return direct;
            }

            var combined = new RelicContributionDelta[directCount + derivedCount];
            for (int index = 0; index < directCount; index++)
            {
                combined[index] = direct[index];
            }

            for (int index = 0; index < derivedCount; index++)
            {
                combined[directCount + index] = derived[index];
            }

            return combined;
        }

        private static string ResolveRelicDescription(string relicId, string relicName)
        {
            return $"{relicName} 발동";
        }

        private static string BuildRelicValueText(
            int previousAttackPower,
            int attackPower,
            int addedAttackPower,
            int block,
            int heal)
        {
            var values = new List<string>(3);

            if (addedAttackPower > 0)
            {
                values.Add($"공격력 {previousAttackPower} → {attackPower} (+{addedAttackPower})");
            }

            if (block > 0)
            {
                values.Add($"방어 +{block}");
            }

            if (heal > 0)
            {
                values.Add($"회복 +{heal}");
            }

            return values.Count > 0 ? string.Join(" / ", values) : "효과 발동";
        }

        private static string BuildFinalSummaryText(SlotCombatRequest request)
        {
            if (request == null)
            {
                return "ATK 0 / DEF 0 / HEAL 0";
            }

            string summary = $"ATK {request.Damage} / DEF {request.Defense} / HEAL {request.HealAmount}";

            if (request.AttackCount > 1)
            {
                summary += $" / HIT {request.AttackCount}";
            }

            return summary;
        }

        internal static SlotSpinResult CreateInitialSlotDisplayResult()
        {
            IReadOnlyList<SlotSymbolType> symbols = SlotSymbolPool.Symbols;
            var displaySymbols = new SlotSymbolType[SlotSpinResult.CellCount];

            for (int index = 0; index < displaySymbols.Length; index++)
            {
                displaySymbols[index] = symbols[index % symbols.Count];
            }

            return new SlotSpinResult(displaySymbols);
        }

        private static bool ShouldRaiseLeverBeforeEvent(CombatEvent combatEvent)
        {
            if (combatEvent.Kind == CombatEventKind.BattleEnded)
            {
                return true;
            }

            return combatEvent.Kind == CombatEventKind.PhaseChanged &&
                combatEvent.Phase != BattlePhase.Resolving;
        }

        private static bool IsLastPlayerAttackPresentation(
            CombatEvent combatEvent,
            int eventIndex,
            IReadOnlyList<CombatEvent> events)
        {
            if (!IsPlayerAttackPresentation(combatEvent))
            {
                return false;
            }

            for (int index = eventIndex + 1; index < events.Count; index++)
            {
                CombatEvent nextEvent = events[index];
                if (IsPlayerAttackPresentation(nextEvent))
                {
                    return false;
                }

                if (nextEvent.Kind == CombatEventKind.PhaseChanged &&
                    nextEvent.Phase != BattlePhase.Resolving)
                {
                    break;
                }
            }

            return true;
        }

        private static bool IsPlayerAttackPresentation(CombatEvent combatEvent)
        {
            return combatEvent.Kind == CombatEventKind.EffectApplied &&
                combatEvent.Phase == BattlePhase.Resolving &&
                !combatEvent.IsPlayerParticipant;
        }
    }

    internal readonly struct SlotTurnRequest
    {
        internal SlotTurnRequest(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        internal CancellationToken CancellationToken { get; }
    }

    internal sealed class SlotTurnResult
    {
        internal SlotTurnResult(
            SlotSpinResult spinResult,
            IReadOnlyList<SlotPatternMatch> patternMatches,
            SlotPatternResult patternResult,
            SlotCombatRequest baseCombatRequest,
            int spinCoinReward = 0,
            int runCoinsAfterReward = 0,
            bool isResolved = true,
            IReadOnlyList<bool> againMarks = null)
        {
            SpinResult = spinResult;
            PatternMatches = patternMatches;
            PatternResult = patternResult;
            BaseCombatRequest = baseCombatRequest;
            SpinCoinReward = Math.Max(0, spinCoinReward);
            RunCoinsAfterReward = Math.Max(0, runCoinsAfterReward);
            IsResolved = isResolved;
            AgainMarks = againMarks ?? Array.Empty<bool>();
        }

        internal SlotSpinResult SpinResult { get; }

        internal IReadOnlyList<SlotPatternMatch> PatternMatches { get; }

        /// <summary>현재 보드 셀별 "다시" 표식(5x3 셀 인덱스 순). 스왑을 반영한 최신 상태다.</summary>
        internal IReadOnlyList<bool> AgainMarks { get; }

        internal SlotPatternResult PatternResult { get; }

        internal SlotCombatRequest BaseCombatRequest { get; }

        internal int SpinCoinReward { get; }

        internal int RunCoinsAfterReward { get; }

        internal bool IsResolved { get; }
    }
}
