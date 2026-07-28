# 달토끼 일반 몬스터 SO 작성 계획

**Status**: draft  
**Last updated**: 2026-07-28

## Purpose

첫 10전의 일반전 EncounterTable에서 무작위로 선택할 달토끼 4종의 전투 데이터를 authoring하기 전에, 만들고 갱신할 ScriptableObject, 참조 관계, 행동 순서와 기본 수치를 고정한다. `MoonRabbit-4`는 보스 전용으로 예약하며, 그 Pattern/Definition 데이터는 별도로 확정한다. 이 문서는 실제 밸런스 데이터의 원본이 아니며, 구현 후 수치는 `Assets/_Project/Data/Monsters/MoonRabbit/`의 SO가 source of truth가 된다.

## Decisions

| 결정 | 근거 |
|------|------|
| 행동 Amount는 base 값으로 SO에 저장한다. | [ADR-0022](../adr/0022-encounter-shared-effect-scaling.md)의 공용 Encounter 배율이 HP와 Damage/Shield/Heal Amount에 적용된다. 약화·흡혈의 적용 횟수는 스케일하지 않는다. |

## Current constraints

- 몬스터 턴은 기존 `MonsterTurnPatternDefinition` → `FixedSequenceEnemyActionPlanner` 계약으로 순환한다. 순환 index는 SO가 아니라 런타임 planner가 소유한다.
- 현재 달토끼 combat visual은 `EnemyCommon.controller`의 `Common` 상태만 확실히 사용할 수 있다. 이번 SO의 행동 이름은 모두 `Common`으로 유지하며, 새 Animator state는 만들지 않는다.

## Scope

### 생성 또는 갱신하는 SO

기존 파일을 재사용한다. 새 파일명을 만들거나 `MonsterVisualDefinition`을 복제하지 않는다.

| 역할 | Pattern SO | Definition SO | 기존 Visual SO |
|------|------------|---------------|--------------|
| 정찰 수비병 | `MoonRabbit-1-Pattern.asset` | `MoonRabbit-1.asset` | `MoonRabbit-1-Visual.asset` |
| 주술사 | `MoonRabbit-2-Pattern.asset` | `MoonRabbit-2.asset` | `MoonRabbit-2-Visual.asset` |
| 회복병 | `MoonRabbit-3-Pattern.asset` | `MoonRabbit-3.asset` | `MoonRabbit-3-Visual.asset` |
| 광전병 | `MoonRabbit-5-Pattern.asset` | `MoonRabbit-5.asset` | `MoonRabbit-5-Visual.asset` |
| 보스 예약 | 이번 범위에서 변경하지 않음 | 이번 범위에서 변경하지 않음 | `MoonRabbit-4-Visual.asset` |

### 만들지 않는 SO

- `MonsterVisualDefinition`: 초상화와 combat visual prefab이 연결된 기존 5개를 그대로 사용한다.
- `EncounterTable`: 어느 전투에서 어떤 가중치로 등장하는지는 이 문서 범위가 아니다.
- 새 EffectDefinition ScriptableObject: 적 행동 효과는 `MonsterTurnPatternDefinition.turns` 안의 직렬화된 `EnemyEffectDefinition`으로 저장한다.
- 새로운 Animator Controller 또는 행동 Animator State: 별도 연출 작업에서 다룬다.

## 참조 관계

```text
MoonRabbit-N.asset (MonsterDefinition)
 ├─ _visual ────────> MoonRabbit-N-Visual.asset (기존)
 ├─ maxHp ──────────> 아래 표의 base HP
 └─ turnPattern ────> MoonRabbit-N-Pattern.asset
                             └─ turns[0..2]
                                  └─ actions[0..n]
                                       ├─ ActionName: Common
                                       ├─ IntentIcon: 효과 종류별 기존 Intent sprite
                                       └─ Effect: Damage / Shield / Heal / Status
```

`MonsterDefinition`의 `maxHp`는 base HP다. 실제 전투 HP와 Damage/Shield/Heal Amount는 웨이브·등급 공용 배율을 적용한 런타임 값으로 확인한다.

## Pattern SO authoring 규칙

- 각 Pattern SO의 `turns` 길이는 3으로 고정한다. 3번째 턴 뒤에는 런타임 planner가 다시 첫 번째 턴을 사용한다.
- 피해 효과의 대상은 플레이어, Shield/Heal/Lifesteal 상태의 대상은 자신으로 설정한다.
- 같은 턴의 복수 행동은 배열 순서대로 실행한다. 상태 부여가 먼저여야 하면 상태 행동을 Damage보다 앞에 둔다.
- 모든 `EnemyActionDefinition.ActionName`은 `Common`으로 설정한다. 지금은 각 action이 같은 공격 애니메이션을 재생하며, Intent와 적용 효과로만 차이를 표시한다.
- Intent icon은 기존 `Assets/_Project/Art/UI/staticon-Sheet.png`의 아래 sub-sprite를 사용한다. `MonsterIntent_Status.png`와 `MonsterIntent_Speicial.png`는 이번 Pattern SO에 사용하지 않는다.

| 효과 | Intent sprite |
|------|---------------|
| Damage | `staticon-Sheet_1` |
| Shield | `staticon-Sheet_2` |
| Heal | `staticon-Sheet_4` |
| Lifesteal | `staticon-Sheet_8` |
| Weaken | `staticon-Sheet_10` |

## Definition 및 Pattern 데이터

| 역할 | `maxHp` | Turn 1 | Turn 2 | Turn 3 |
|------|---:|---|---|---|
| 정찰 수비병 | 18 | Shield(Self) 3 → Damage 2 | Damage 5 | Shield(Self) 3 → Damage 6 |
| 주술사 | 17 | Damage 2 | Weaken(Player) 1 → Damage 2 | Damage 6 |
| 회복병 | 17 | Damage 3 | Heal(Self) 3 → Damage 2 | Damage 6 |
| 광전병 | 14 | Lifesteal(Self) 3 → Damage 3 | Damage 5 | Damage 8 |

광전병의 Turn 1은 Lifesteal 상태를 먼저 적용한 뒤 피해를 준다. 따라서 해당 피해가 실제 HP에 들어가면 흡혈 사용 횟수 중 하나를 바로 소비한다. Shield에 전부 막혀 실제 HP 피해가 없으면 흡혈 회복도 발생하지 않는다.

### 턴별 Intent 배열

| 역할 | Turn 1 | Turn 2 | Turn 3 |
|------|--------|--------|--------|
| 정찰 수비병 | Shield, Damage | Damage | Shield, Damage |
| 주술사 | Damage | Weaken, Damage | Damage |
| 회복병 | Damage | Heal, Damage | Damage |
| 광전병 | Lifesteal, Damage | Damage | Damage |

## 구현 전 확인 항목

- `Common` animation state가 이번 범위의 달토끼 1·2·3·5 combat visual prefab에 존재하는지 확인한다.
- Pattern의 복수 action이 Intent UI에 순서대로 표시되고, 상태 action 뒤 Damage action이 같은 적 턴에서 순서대로 재생되는지 확인한다.
- 광전병의 Lifesteal이 자신에게 적용되고, 첫 Damage부터 실제 HP 피해 기준으로 회복하는지 확인한다.
- Encounter 공용 배율을 적용한 1·5·10전의 실전 HP와 Damage/Shield/Heal 수치가 기획 목표 범위를 벗어나지 않는지 확인한다.

## Out of scope

- 4종의 등장 가중치와 1~10전 EncounterTable authoring
- `MoonRabbit-4` 보스 SO와 5전 엘리트 SO
- 행동별 전용 애니메이션과 Intent 아트 추가
- 실제 밸런스 플레이테스트 결과에 따른 수치 조정
