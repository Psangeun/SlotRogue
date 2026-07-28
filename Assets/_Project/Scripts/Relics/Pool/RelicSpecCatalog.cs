using System.Collections.Generic;
using SlotRogue.Slot.Data;
using S = SlotRogue.Slot.Data.SlotSymbolType;

namespace SlotRogue.Relics.Pool
{
    /// <summary>
    /// 유물(별조각 상점) v30 최종안 55종을 부품 조합(<see cref="RelicSpec"/>)으로
    /// 정의하는 카탈로그. 숫자·조건은 데이터, 행동은 트리거별 핸들러가 담당한다.
    /// v30 최종안의 런타임 유물은 조합 가능한 효과로 표현하고, 실행 시점별 세부 처리는
    /// <see cref="RelicSpecRunner"/>와 UI/GameFlow 훅이 맡는다.
    /// v30엔 시작(Starter) 등급이 없다 — 전부 별조각 상점에서 구매하는 이번 런 전용 파츠.
    /// 수명: perm=상시 / euse=소멸·횟수(ConsumableUses) / ewave=소멸·웨이브(ConsumableWaves).
    /// </summary>
    public static class RelicSpecCatalog
    {
        public static IReadOnlyList<RelicSpec> All { get; } = new[]
        {
            // ── 재발동(retrig) ──────────────────────────────────────────
            R("R-01", "앙코르", "매 전투 첫 스핀의 최고 족보가 한 번 더 터진다.",
              RelicGrade.Uncommon, "retrig", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle)), lifetime: Life(RelicLifetimeKind.OncePerBattle, 1),
              devNote: "첫 스핀 최고 족보 재발동."),
            R("R-04", "7의 낙인", "<sprite index=1>세븐은 항상 [다시] 표식을 달고 나온다 — <sprite index=1>세븐 족보는 두 번 터진다!",
              RelicGrade.Rare, "retrig", 8, 1,
              RelicTrigger.OnSpinGenerate, Fx(E(RelicEffectKind.AddAgainMark, 1, s: Sy(S.Seven), chance: 1f)),
              devNote: "7 등장 시 항상 [다시]."),
            R("R-05", "메아리 부적", "자리 바꾸기를 아끼면, 첫 스핀의 모든 족보가 한 번 더 터진다.",
              RelicGrade.Rare, "retrig", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerAllPatterns)),
              Cx(C(RelicConditionKind.NoSwapThisSpin), C(RelicConditionKind.IsFirstSpinOfBattle)),
              lifetime: Life(RelicLifetimeKind.OncePerBattle, 1),
              devNote: "no-swap + 첫 스핀 전 족보 재발동."),
            R("R-06", "다시의 눈", "매 스핀 몇몇 칸에 무작위로 [다시] 표식이 뜬다 — 그 칸이 족보에 들면 한 번 더!",
              RelicGrade.Rare, "retrig", 8, 1,
              RelicTrigger.OnSpinGenerate, Fx(E(RelicEffectKind.AddAgainMark, 1, chance: 0.12f)),
              devNote: "매 스핀 각 칸 12% [다시]."),
            R("R-52", "완벽한 정렬", "<sprite name=\"icon_pattern_10\"> 한 줄을 같은 심볼로 꽉 채우면, 최고 족보를 한 번 더 터뜨린다!",
              RelicGrade.Rare, "retrig", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(C(RelicConditionKind.WholeLineSameSymbol)), devNote: "한 줄 5칸 동일 → 최고 족보 재발동."),
            R("R-53", "막판 뒤집기", "5턴째엔 모든 족보를 한 번 더 터뜨린다!",
              RelicGrade.Rare, "retrig", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerAllPatterns)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5)), devNote: "5턴째 전 족보 재발동."),
            R("R-40", "빌린 재발동", "다음 3웨이브 동안 최고 족보를 한 번 더 터뜨린다. 그 뒤 사라진다.",
              RelicGrade.Rare, "retrig", 6, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              lifetime: Life(RelicLifetimeKind.ConsumableWaves, 3), devNote: "소멸·3웨이브: 최고 족보 재발동."),
            R("R-57", "잭팟 엔진", "<sprite index=1>세븐 족보가 터진 스핀은, 최고 족보가 한 번 더 터진다!",
              RelicGrade.Legendary, "retrig", 13, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Seven)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "7 포함 족보 존재 시 최고 족보 재발동."),

            // ── 배율(mult) ──────────────────────────────────────────────
            R("R-09", "체리 증폭기", "<sprite index=0>체리가 들어간 족보가 50% 더 세게 터진다.",
              RelicGrade.Common, "mult", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.5f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Cherry)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "체리 포함 족보 Mult +0.5."),
            R("R-43", "과일 바구니", "<sprite index=0>체리·<sprite index=5>레몬이 들어간 족보가 30% 더 세게 터진다.",
              RelicGrade.Common, "mult", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.3f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Cherry, S.Lemon)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "체리·레몬 포함 족보 Mult +0.3."),
            R("R-51", "종·클로버 공명", "<sprite index=3>종·<sprite index=4>클로버가 들어간 족보가 30% 더 세게 터진다.",
              RelicGrade.Common, "mult", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.3f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Bell, S.Clover)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "종·클로버 포함 족보 Mult +0.3."),
            R("R-10", "세븐 뇌관", "<sprite index=1>세븐이 들어간 족보가 60% 더 세게 터진다.",
              RelicGrade.Uncommon, "mult", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.6f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Seven)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "7 포함 족보 Mult +0.6."),
            R("R-11", "패턴 학자", "<sprite name=\"icon_pattern_4\"> 이상 맞춘 족보가 50% 더 세게 터진다.",
              RelicGrade.Uncommon, "mult", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.5f)),
              Cx(C(RelicConditionKind.PatternSizeAtLeast, 4)), devNote: "4칸↑ 족보 Mult +0.5."),
            R("R-44", "큰 손", "<sprite name=\"icon_pattern_4\">을 꽉 채운 족보의 피해가 1.5배!",
              RelicGrade.Uncommon, "mult", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.5f)),
              Cx(C(RelicConditionKind.PatternSizeAtLeast, 5)), devNote: "5칸 족보 ×1.5."),
            R("R-12", "세븐 과충전", "<sprite index=1>세븐이 들어간 족보의 피해가 1.5배!",
              RelicGrade.Rare, "mult", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.5f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Seven)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "7 포함 족보 ×1.5."),
            R("R-50", "다이아 세공사", "<sprite index=2>다이아가 들어간 족보의 피해가 1.4배!",
              RelicGrade.Rare, "mult", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.4f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Diamond)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "다이아 포함 족보 ×1.4."),
            R("R-42", "콤보 왕관", "한 번에 족보를 3개 이상 터뜨리면 그 스핀 피해가 1.5배!",
              RelicGrade.Rare, "mult", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.5f)),
              Cx(C(RelicConditionKind.ActivePatternCountAtLeast, 3)), devNote: "족보 3개↑ 스핀 ×1.5."),
            R("R-13", "잭팟 코어", "<sprite name=\"icon_pattern_10\"> 한 줄을 같은 심볼로 꽉 채우면 피해가 3배!",
              RelicGrade.Rare, "mult", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FinalMultTimes, 3f)),
              Cx(C(RelicConditionKind.WholeLineSameSymbol)), devNote: "한 줄 5칸 동일 → 최종 ×3."),
            R("R-39", "폭죽 다발", "다음 2웨이브 동안 매 스핀 피해 +8. 그 뒤 사라진다.",
              RelicGrade.Uncommon, "mult", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 8f)),
              lifetime: Life(RelicLifetimeKind.ConsumableWaves, 2), devNote: "소멸·2웨이브: 스핀 피해 +8."),
            R("R-56", "별의 왕관", "모든 족보가 언제나 50% 더 세게 터진다!",
              RelicGrade.Legendary, "mult", 13, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.5f)),
              devNote: "전 족보 Mult +0.5."),

            // ── 스왑(swap) ──────────────────────────────────────────────
            R("R-16", "교환 장갑", "자리 바꾸기를 쓴 스핀은 피해 +4.",
              RelicGrade.Common, "swap", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 4f)),
              Cx(C(RelicConditionKind.SwapUsedThisSpin)), devNote: "swap 스핀 합산 피해 +4."),
            R("R-17", "스왑 장인", "자리 바꾸기를 웨이브마다 한 번 더 쓸 수 있다.",
              RelicGrade.Uncommon, "swap", 5, 1,
              RelicTrigger.RuleModifier, Fx(E(RelicEffectKind.SwapCountDelta, 1f)), devNote: "넛지(swap) +1회."),
            R("R-46", "무결의 일격", "자리 바꾸기를 아낀 스핀은 모든 족보가 50% 더 세게 터진다.",
              RelicGrade.Uncommon, "swap", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ComboMultAdd, 0.5f)),
              Cx(C(RelicConditionKind.NoSwapThisSpin)), devNote: "no-swap 스핀 전 족보 Mult +0.5."),
            R("R-18", "연쇄 교환", "자리 바꾸기로 새 족보를 완성하면, 그 스핀 최고 족보가 한 번 더 터진다!",
              RelicGrade.Rare, "swap", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(C(RelicConditionKind.PatternMadeBySwap)), devNote: "swap 완성 족보 존재 시 최고 족보 재발동."),
            R("R-19", "시간의 여유", "자리 바꾸기를 두 번 더 쓸 수 있다.",
              RelicGrade.Legendary, "swap", 13, 1,
              RelicTrigger.RuleModifier, Fx(E(RelicEffectKind.SwapCountDelta, 2f)), devNote: "넛지(swap) +2회."),

            // ── 경제/보유형(econ) ───────────────────────────────────────
            R("R-21", "별가루 주머니", "전투를 시작할 때 별조각 1개를 받는다.",
              RelicGrade.Common, "econ", 3, 1,
              RelicTrigger.OnBattleStart, Fx(E(RelicEffectKind.GainCoins, 1f)), devNote: "전투 시작 별조각 +1."),
            R("R-22", "별 수집가", "자리 바꾸기 없이 이기면 별조각을 1개 더 받는다.",
              RelicGrade.Uncommon, "econ", 5, 1,
              RelicTrigger.OnKill, Fx(E(RelicEffectKind.GainCoins, 1f)),
              Cx(C(RelicConditionKind.NoSwapThisBattle)), devNote: "no-swap 승리 시 별조각 +1."),
            R("R-23", "저축가의 반지", "별조각을 4개 넘게 갖고 있으면 매 스핀 피해 +2.",
              RelicGrade.Uncommon, "econ", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 2f)),
              Cx(C(RelicConditionKind.CoinsAtLeast, 5)), devNote: "보유 4+ → 스핀 피해 +2."),
            R("R-48", "저축 왕", "별조각을 8개 넘게 모으면 피해가 1.3배!",
              RelicGrade.Rare, "econ", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.3f)),
              Cx(C(RelicConditionKind.CoinsAtLeast, 8)), devNote: "보유 8+ → ×1.3."),
            R("R-54", "일확천금", "별조각을 10개 넘게 모으면 최종 피해가 1.5배!",
              RelicGrade.Rare, "econ", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FinalMultTimes, 1.5f)),
              Cx(C(RelicConditionKind.CoinsAtLeast, 10)), devNote: "보유 10+ → 최종 ×1.5."),

            // ── 상점/리롤(shop) ────────────────────────────────────────
            R("R-28", "단골 카드", "상점의 모든 유물이 1씩 싸진다.",
              RelicGrade.Uncommon, "shop", 5, 1,
              RelicTrigger.RuleModifier, Fx(E(RelicEffectKind.ShopDiscount, 1f)), devNote: "상점 가격 전부 -1 (최소 1)."),

            // ── 전투/생존(combat) ──────────────────────────────────────
            R("R-32", "작은 망원경", "매 전투 첫 스핀 피해 +3.",
              RelicGrade.Common, "combat", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 3f)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle)), devNote: "매 전투 첫 스핀 피해 +3."),
            R("R-49", "첫 끗발", "전투 첫 스핀의 피해가 1.5배!",
              RelicGrade.Common, "combat", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.5f)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle)), devNote: "전투 첫 스핀 ×1.5."),
            R("R-33", "응급 붕대", "몬스터를 잡을 때마다 체력을 4 회복한다.",
              RelicGrade.Common, "combat", 3, 1,
              RelicTrigger.OnKill, Fx(E(RelicEffectKind.Heal, 4f)), devNote: "처치 시 HP +4."),
            R("R-47", "재정비", "한 번에 족보를 4개 이상 터뜨리면 체력을 2 회복한다.",
              RelicGrade.Common, "combat", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.Heal, 2f)),
              Cx(C(RelicConditionKind.ActivePatternCountAtLeast, 4)), devNote: "족보 4개↑ → HP +2."),
            R("R-34", "마지막 탄환", "5턴째 스핀 피해 +10.",
              RelicGrade.Uncommon, "combat", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 10f)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5)), devNote: "5턴째 합산 피해 +10."),
            R("R-55", "큰 거 한 방", "<sprite name=\"icon_pattern_4\">을 꽉 채운 족보의 피해 +8.",
              RelicGrade.Uncommon, "combat", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 8f)),
              Cx(C(RelicConditionKind.PatternSizeAtLeast, 5)), devNote: "5칸 족보 +8."),
            R("R-35", "폭주 기관차", "오래 못 잡을수록 커진다 — 5턴째 스핀 피해 +15.",
              RelicGrade.Rare, "combat", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 15f)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5)), devNote: "5턴째 합산 피해 +15."),
            R("R-45", "최후의 불꽃", "5턴째 스핀의 피해가 2배로 터진다!",
              RelicGrade.Rare, "combat", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 2f)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5)), devNote: "5턴째 ×2."),

            // ── 속성(status) ────────────────────────────────────────────
            R("R-58", "성냥갑", "<sprite index=0>체리·<sprite index=5>레몬 족보가 터진 스핀, 적에게 화상 2.",
              RelicGrade.Common, "status", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 2f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Cherry, S.Lemon)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "과일 포함 3개↑ 족보 스핀 → 화상 2."),
            R("R-59", "기름통", "<sprite index=0>체리·<sprite index=5>레몬 족보가 터진 스핀, 적에게 화상 4.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 4f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Cherry, S.Lemon)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "과일 포함 3개↑ → 화상 4."),
            R("R-60", "독포자 주머니", "<sprite index=4>클로버·<sprite index=2>다이아 족보가 터진 스핀, 적에게 감염 3.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyInfection, 3f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Clover, S.Diamond)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "클로버/다이아 포함 3개↑ → 감염 3."),
            R("R-61", "낡은 종추", "<sprite index=3>종 족보가 터진 스핀, 적에게 취약 1과 약화 1.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyVulnerable, 1f), E(RelicEffectKind.ApplyWeaken, 1f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Bell)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "종 포함 3개↑ → 취약 1 + 약화 1."),
            R("R-62", "가시 갑옷", "<sprite index=4>클로버 족보가 터진 스핀, 가시 5가 돋는다.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.GainThorns, 5f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Clover)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3)), devNote: "클로버 포함 3개↑ → 가시 5(자신)."),
            R("R-70", "예열 장치", "전투가 시작되면 기계가 달아오른다 — 첫 스핀에 적에게 화상 3.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 3f)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle)), devNote: "첫 스핀 → 화상 3."),
            R("R-71", "포자 살포기", "전투가 시작되면 포자가 퍼진다 — 첫 스핀에 적에게 감염 4.",
              RelicGrade.Uncommon, "status", 5, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyInfection, 4f)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle)), devNote: "첫 스핀 → 감염 4."),
            R("R-64", "부지깽이", "<sprite name=\"staticon-Sheet_5\">불타는 적에게 주는 피해가 1.3배.",
              RelicGrade.Rare, "status", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.3f)),
              Cx(EnemyStatus(RelicEnemyStatusCondition.Burn)), devNote: "적 화상 보유 시 스핀 피해 ×1.3."),
            R("R-65", "병리학자", "<sprite name=\"staticon-Sheet_3\">감염된 적에게 주는 피해가 1.3배.",
              RelicGrade.Rare, "status", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.3f)),
              Cx(EnemyStatus(RelicEnemyStatusCondition.Infection)), devNote: "적 감염 보유 시 ×1.3."),
            R("R-66", "사형 집행인", "<sprite name=\"staticon-Sheet_9\">취약한 적에게 최종 피해가 1.25배.",
              RelicGrade.Rare, "status", 8, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FinalMultTimes, 1.25f)),
              Cx(EnemyStatus(RelicEnemyStatusCondition.Vulnerable)), devNote: "적 취약 보유 시 최종 ×1.25."),
            R("R-63", "초신성 뇌관", "5칸을 꽉 채운 스핀, 적에게 화상 5와 취약 2!",
              RelicGrade.Legendary, "status", 13, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 5f), E(RelicEffectKind.ApplyVulnerable, 2f)),
              Cx(C(RelicConditionKind.PatternSizeAtLeast, 5)), devNote: "5칸 족보 스핀 → 화상 5 + 취약 2."),
            R("R-67", "고통의 메아리", "상태이상에 걸린 적에겐 최고 족보가 한 번 더 터진다!",
              RelicGrade.Legendary, "status", 13, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(EnemyStatus(RelicEnemyStatusCondition.Any)), devNote: "적 상태이상 1개↑ → 최고 족보 재발동."),

            // ── 저주(curse) ─────────────────────────────────────────────
            R("R-72", "구두쇠의 저울", "상점의 모든 유물이 2 싸진다. 대신 자리 바꾸기를 쓸 수 없다.",
              RelicGrade.Curse, "shop", 2, 1,
              RelicTrigger.RuleModifier, Fx(E(RelicEffectKind.ShopDiscount, 2f), E(RelicEffectKind.SwapCountDelta, -1f)),
              devNote: "ShopDiscount +2 / swap -1회(기본 1→0)."),
            R("R-73", "성급한 태엽", "자리 바꾸기를 한 번 더 쓸 수 있다. 대신 상점의 모든 유물이 1 비싸진다.",
              RelicGrade.Curse, "swap", 2, 1,
              RelicTrigger.RuleModifier, Fx(E(RelicEffectKind.SwapCountDelta, 1f), E(RelicEffectKind.ShopDiscount, -1f)),
              devNote: "swap +1회 / ShopDiscount -1(가격 +1)."),
            R("R-74", "납 주사위", "매 스핀 피해 +8. 대신 모든 족보의 피해가 20% 줄어든다.",
              RelicGrade.Curse, "combat", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 8f), E(RelicEffectKind.FinalMultTimes, 0.8f)),
              devNote: "스핀 피해 +8 / 전 족보 ×0.8."),
            R("R-38", "유리 대포", "내 피해가 1.4배! 대신 받는 피해도 1.3배.",
              RelicGrade.Curse, "combat", 3, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.4f), E(RelicEffectKind.IncomingDamageMul, 1.3f)),
              devNote: "전 족보 ×1.4 / 받는 피해 ×1.3."),
        };

        private static readonly Dictionary<string, RelicSpec> ById = BuildIndex(All);

        /// <summary>id로 유물 명세 조회. 없으면 null.</summary>
        public static RelicSpec GetById(string id) =>
            !string.IsNullOrEmpty(id) && ById.TryGetValue(id, out RelicSpec spec) ? spec : null;

        // ── 제안(처치 보상) 엔진 효과 스펙 — 제안 카탈로그의 P-id와 매칭 ──────────
        // 제안은 픽하면 영구 누적되어 유물과 함께 전투 엔진이 소비한다(등급/가격은 미사용).
        public static IReadOnlyList<RelicSpec> Proposals { get; } = new[]
        {
            R("P-07", "3연격 훈련", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 1f)),
              Cx(C(RelicConditionKind.PatternSizeEquals, 3))),
            R("P-21", "4연격 훈련", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 3f)),
              Cx(C(RelicConditionKind.PatternSizeEquals, 4))),
            R("P-32", "완성된 문양", "", RelicGrade.Rare, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 8f)),
              Cx(C(RelicConditionKind.PatternSizeEquals, 5))),
            R("P-33", "문양 공명", "", RelicGrade.Rare, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 5f)),
              Cx(C(RelicConditionKind.ActivePatternCountAtLeast, 3))),
            R("P-41", "별의 축복", "", RelicGrade.Legendary, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 3f)),
              Cx(C(RelicConditionKind.PatternSizeEquals, 3))),
            R("P-35", "막타 정산", "", RelicGrade.Rare, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerHighestPattern)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5))),
            R("P-42", "황금 손", "", RelicGrade.Legendary, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.RetriggerAllPatterns)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle))),
            R("P-12", "첫 수의 감각", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 2f)),
              Cx(C(RelicConditionKind.IsFirstSpinOfBattle))),
            R("P-13", "절제 보상", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 1f)),
              Cx(C(RelicConditionKind.NoSwapThisSpin))),
            R("P-22", "연쇄 계산", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 2f)),
              Cx(C(RelicConditionKind.ActivePatternCountAtLeast, 2))),
            R("P-24", "교환 타격", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 2f)),
              Cx(C(RelicConditionKind.SwapUsedThisSpin))),
            R("P-26", "마무리 본능", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 5f)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5))),
            R("P-36", "저축 습관", "", RelicGrade.Rare, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.FlatDamageAdd, 2f)),
              Cx(C(RelicConditionKind.CoinsAtLeast, 5))),
            R("P-37", "최후의 별빛", "", RelicGrade.Rare, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 2f)),
              Cx(C(RelicConditionKind.TurnIndexEquals, 5))),

            // ── 속성(상태이상) 제안 ─────────────────────────────────────
            R("P-14", "붉은 성냥", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 2f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Cherry, S.Lemon)),
                 C(RelicConditionKind.PatternSizeAtLeast, 4))),
            R("P-15", "푸른 감염가루", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyInfection, 3f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Clover, S.Diamond)),
                 C(RelicConditionKind.PatternSizeAtLeast, 4))),
            R("P-16", "균열음", "", RelicGrade.Common, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyVulnerable, 1f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Bell)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3))),
            R("P-28", "가시 잎사귀", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.GainThorns, 3f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Clover)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3))),
            R("P-30", "약화 분말", "", RelicGrade.Uncommon, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyWeaken, 1f)),
              Cx(C(RelicConditionKind.PatternContainsSymbol, s: Sy(S.Bell, S.Clover)),
                 C(RelicConditionKind.PatternSizeAtLeast, 3))),
            R("P-47", "납덩이 계약", "", RelicGrade.Curse, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.SpecialMultTimes, 1.25f))),
            R("P-48", "그을린 계약", "", RelicGrade.Curse, "proposal", 0, 1,
              RelicTrigger.OnDamageResolve, Fx(E(RelicEffectKind.ApplyBurn, 2f)),
              Cx(C(RelicConditionKind.PatternSizeAtLeast, 4))),
        };

        private static readonly Dictionary<string, RelicSpec> ProposalById = BuildIndex(Proposals);

        /// <summary>제안 id로 엔진 효과 스펙 조회. 엔진 효과 제안이 아니면 null.</summary>
        public static RelicSpec GetProposalById(string id) =>
            !string.IsNullOrEmpty(id) && ProposalById.TryGetValue(id, out RelicSpec spec) ? spec : null;

        private static Dictionary<string, RelicSpec> BuildIndex(IReadOnlyList<RelicSpec> specs)
        {
            var map = new Dictionary<string, RelicSpec>(specs.Count);
            for (int index = 0; index < specs.Count; index++)
            {
                map[specs[index].Id] = specs[index];
            }

            return map;
        }

        // ── 작성 헬퍼(간결성) ──────────────────────────────────────────
        private static RelicSpec R(
            string id, string name, string desc, RelicGrade grade, string category, int price, int maxCopies,
            RelicTrigger trigger, RelicEffect[] effects, RelicCondition[] conditions = null,
            RelicLifetime lifetime = default, string devNote = null)
            => new(id, name, desc, grade, category, price, maxCopies, iconKey: "",
                trigger, effects, conditions, lifetime, unlock: default, devNote: devNote);

        private static RelicEffect E(
            RelicEffectKind kind, float value1 = 0f, float value2 = 0f,
            S[] s = null, float chance = 1f, string sr = null)
            => new(kind, value1, value2, s, chance, sr);

        private static RelicCondition C(RelicConditionKind kind, int value = 0, S[] s = null)
            => new(kind, value, s);

        private static RelicCondition EnemyStatus(RelicEnemyStatusCondition status)
            => C(RelicConditionKind.EnemyHasStatus, (int)status);

        private static RelicLifetime Life(RelicLifetimeKind kind, int amount) => new(kind, amount);

        private static RelicEffect[] Fx(params RelicEffect[] effects) => effects;

        private static RelicCondition[] Cx(params RelicCondition[] conditions) => conditions;

        private static S[] Sy(params S[] symbols) => symbols;
    }
}
