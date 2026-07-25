# ADR-0022: Encounter 난이도 배율을 HP와 적 행동 Amount에 공용 적용한다

**Status**: accepted
**Date**: 2026-07-25
**Supersedes**: none
**Superseded by**: none
**Related design-docs**: [`docs/design-docs/combat-core.md`](../design-docs/combat-core.md)

---

## Context

무한모드는 전투 번호와 ThemeSection에 따라 적 최대 HP만 증가한다. 적 행동의 피해, Shield, 회복, 상태이상 수치는 MonsterTurnPatternDefinition에 작성한 값 그대로여서, 같은 몬스터는 후반에도 HP만 높아지는 구조다. 기획서는 구간 난이도에 공격과 상태이상 강화를 포함한다.

상태이상 요청의 Amount는 상태마다 피해 강도, 누적량, 남은 적용 횟수 또는 지속 턴처럼 다른 의미를 가진다. 이 의미는 ADR-0021의 StatusEffectSpec 변환 계약을 유지해야 한다.

## Decision

EncounterBalanceSettings의 전투별 증가율, ThemeSection별 증가율, 등급 배율을 공용 Encounter 난이도 배율로 정의한다. 이 배율은 적 최대 HP와 적 행동의 Damage, Shield, Heal, StatusEffect Amount에 적용한다. 단, `Vulnerable`, `Weaken`, `Lifesteal`의 Amount는 남은 적용 횟수이므로 원본 값으로 고정한다.

양수 Amount는 HP와 같이 `MidpointRounding.AwayFromZero` 반올림 후 최소 1로 적용한다. 0은 0으로 유지하며, 1차 구현에는 상한을 두지 않는다. LockSlot의 잠금 칸 수와 지속 턴은 공용 배율 적용 대상에서 제외한다.

## Alternatives considered

- **HP와 행동에 별도 증가율을 둔다** — 초기 튜닝 가짓수가 늘고 HP와 같은 구조라는 결정에 맞지 않아 거절한다. 필요해지면 후속 ADR로 분리한다.
- **피해만 강화한다** — Shield, 회복, 상태이상 행동이 후반에 상대적으로 무의미해져 기획의 다축 난이도와 맞지 않아 거절한다.
- **모든 상태이상 Amount를 스케일한다** — 취약·약화·흡혈의 Amount는 피해 강도가 아니라 남은 적용 횟수여서, 후반에 행동 제약이 과도하게 길어질 수 있으므로 거절한다.
- **상태이상과 LockSlot을 함께 스케일한다** — LockSlot은 칸 수와 지속 턴이 플레이 가능성을 급격히 낮추므로 1차 범위에서 제외한다.

## Consequences

- 적 행동 수치는 전투 시작 시 원본 SO를 수정하지 않고, 런타임 EnemyActionPlan으로 변환할 때만 스케일한다.
- 화상·감염·가시와 동결은 상태 Amount를 스케일한다. 취약·약화·흡혈의 남은 적용 횟수는 스케일하지 않는다.
- 무한 진행 시 모든 대상 수치가 계속 증가하므로, 플레이테스트 후 개별 효과 상한이나 별도 배율이 필요하면 새 결정을 기록한다.

## Notes

공용 배율은 기존 EncounterBalanceSettingsDefault.asset의 값과 직렬화 호환성을 유지한다.
