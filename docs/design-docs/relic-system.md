# 유물 시스템

**Status**: draft
**Last updated**: 2026-07-28

## Purpose

v30 최종 별조각 상점 유물 55종을 하나의 카탈로그로 식별하고, 상점·런 인벤토리·스핀 효과 계산·전투 상태 요청을 같은 데이터 모델로 연결한다. 현재 플레이에는 시작 유물이 없으며, `RelicSpecCatalog`가 v30 데이터의 단일 출처이고 `RelicCatalog`는 기존 소비처가 쓰는 `RelicDefinition` 형태로 변환해 제공한다.

## Decisions

| # | 결정 | 요약 |
|---|------|------|
| R1 | [ADR-0005](../adr/0005-relic-v23-runtime-model.md) | 런타임 유물 소비처는 `RelicCatalog`와 `RelicDefinition`으로 단일화한다. v30 데이터 원본은 `RelicSpecCatalog`가 담당한다. |
| R2 | 카탈로그와 보상 풀 | `All`과 `RewardPool`은 v30 상점 유물 55종 전체를 제공한다. `Starters`는 v30에서 항상 빈 목록이다. |
| R3 | 전투 코어 비침범 | `RelicSpecRunner`는 이번 턴의 피해·회복·배율·재발동·상태이상 요청만 만들고 실제 적용은 기존 슬롯/전투 파이프라인이 담당한다. |
| R4 | [ADR-0009](../adr/0009-relic-icon-addressable-keys.md) | 유물 아이콘은 `IconKey`로 식별하고 SceneRoot가 Addressables에서 로드한다. |
| R5 | [`attribute-status-interference.md`](./attribute-status-interference.md) | 속성 유물은 v6의 6속성 및 정산 단위 기준으로 구현한다. |

## Source data

기획 원본은 `slot_roguelite_relics_v30_final.html`의 `relic-data` JSON이다. 총 55종이며 등급별 개수는 일반 10, 고급 18, 희귀 18, 전설 5, 저주 4다. 분류별 개수는 재발동 8, 배율 12, 스왑 6, 경제/보유형 5, 상점/리롤 2, 전투/생존 10, 속성 12다.

v30에는 시작 유물이 없다. 런 시작 선택 단계는 비활성이고, 유물은 전투 중 `ShopPanel`에서 별조각으로 구매한다.

## Runtime flow

```text
RelicShopModel
→ RelicCatalog.RewardPool 중 3종 추첨
→ GameFlowSession.OwnedRelics
→ SlotMachineViewModel.Spin()
→ RelicSpecRunner.ResolveAgainMarks() / ResolveDamageTurn()
→ CombatTurnRequestBuilder.Build()
→ SlotCombatRequestToCombatEffectsConverter.Convert()
→ BattleSystem
```

`RelicSpecRunner`는 보유 유물, 이번 스핀의 패턴 요약, 스왑 사용 여부, 별조각 보유량, 턴 수, 선택 적 상태 스냅샷을 받아 계산한다. 여러 유물이 동시에 발동하면 기본 족보 피해 배율, 정수 피해, 회복, 재발동 요청, 상태이상 요청을 합산해 상위 전투 연결 계층에 전달한다.

상태이상 요청은 `CombatTargetMode`로 적용 대상을 명시한다. 화상·감염·취약·약화처럼 적에게 적용하는 효과는 `SelectedEnemy`, 가시·흡혈처럼 자신에게 적용하는 효과는 `Self`를 사용한다. 요청 병합은 상태 종류와 대상이 모두 같은 경우에만 수행하며, 최종 `CombatEffectTarget` 생성은 UI 전투 연결 계층의 Converter가 담당한다.

전투 중 상점은 3칸 랜덤 제안을 제공하며, 가격은 v30 원본의 개별 `price` 값을 그대로 사용한다. `R-39`/`R-40` 같은 소멸·웨이브 유물과 `R-72`/`R-73` 저주의 가격 예외도 등급 기본값으로 덮어쓰지 않는다.

## Icon flow

```text
RelicDefinition.IconKey
→ RunBattleRelicShopOfferState / RunInventoryRelicItemState
→ BattleSceneHost / RunRelicInventoryView
→ AddressableSpriteProvider
→ GameFlowImageSlot
```

현재 시트는 `Assets/_Project/Art/Relics/icon-Sheet_300.png`이며 84 x 84 Sprite 56개를 가진다. Addressable 주소는 `Relic Sheet 300`이고, 키는 `RelicIconKeys.Slot00`~`Slot55`다. v30 유물 55종은 카탈로그 순서대로 앞 55개 아이콘을 사용하며, 마지막 1개는 여분이다. 아이콘 로드 실패 시 `RelicIconKeys.Default`를 사용한다.

새 시트나 개별 Sprite는 `Resources`가 아닌 Addressables 그룹에 등록한다. View와 ViewModel에는 `Addressables.Load*` 호출을 추가하지 않는다.

## Legacy boundary

- 2026-06-12 정적 참조 검증 후 `ArtifactDefinitionSO`, `RelicDataSO`, 관련 Runtime 코드와 Editor builder를 삭제했다.
- 참조가 끊긴 `Assets/_Project/Data/_Legacy/`의 구 Artifact/Relic 자산도 함께 삭제했다.
- 현재 유물 런타임의 단일 진입점은 `RelicSpecCatalog`, `RelicCatalog`, `RelicDefinition`, `RelicSpecRunner`다.

## Current implementation

v30 상점 유물 55종은 모두 `RelicSpecCatalog.All`에 등록되어 `RelicCatalog.RewardPool`로 노출된다. 현재 실행 엔진은 다음 계약을 직접 처리한다.

- 피해: 정수 가산, 족보별 콤보 배율, 특수 배율, 최종 배율, 받는 피해 배율.
- 재발동: 최고 족보 재발동, 전체 족보 재발동, `[다시]` 표식 부여.
- 런 규칙: 스왑 횟수 증감, 상점 가격 할인/할증, 전투 시작/처치 별조각.
- 생존/상태: 처치·조건부 회복, 화상·감염·취약·약화·가시 요청.
- 수명: `ConsumableWaves` 유물은 웨이브 종료 시 수명을 감소시키고 만료 시 제거한다.

상태 전투 계약은 [`attribute-status-interference.md`](./attribute-status-interference.md)를 기준으로 하며, 유물 계층은 상태 적용 요청만 만들고 실제 틱/피해/감쇠는 전투 코어가 처리한다.

## 전투 담당 요청사항

아래 항목은 전투 코어의 변경이 필요하므로 유물 계층에서 우회 구현하지 않는다. 상세 명세와 현재 코드 차이는 [`attribute-status-interference.md`](./attribute-status-interference.md)에 둔다.

1. 화상은 부여 즉시 피해 + 대상 턴 종료 1회 피해로 구현해야 한다.
2. 감염은 턴 종료 피해 후 수치가 1 감소해야 하며, 총 스택 상한은 두지 않는다.
3. 취약과 약화는 유물 발동 건별이 아니라 정산 1회 기준으로 적용한다. 약화는 직접 공격 피해를 20% 감소시키고 행동당 적용 횟수 1을 소모한다.
4. 흡혈은 실제 HP 피해 기반 회복과 턴당 회복 상한이 필요하다.
5. 가시는 피격 후 반사 피해와 라운드 종료 제거, 턴당 반사 횟수 상한이 필요하다.
6. 유물 판정 계층이 화상·감염·취약·약화 상태를 구분해 조회할 수 있는 읽기 전용 상태 계약이 필요하다.
7. 피해 효과 발동, 상태 부여, 감염 피해, 방어도 획득, 가시 피해, 흡혈 회복을 구분하는 전투 이벤트가 필요하다.

## Open questions

| ID | 질문 | 비고 |
|----|------|------|
| Q1 | 취약/약화 기본 수치 | v6 상세표와 일부 유물 설명의 수치가 다르므로 구현 전 기준값을 확정한다. |
| Q2 | 카탈로그 외부 데이터화 | v30 HTML처럼 표 기반 편집이 잦아지면 ScriptableObject/JSON/CSV 중 하나를 ADR로 결정한다. |
| Q3 | 실제 릴 확률 기반 기대값 보정 | 기획 원본도 p(3+), p(4+) 계산 후 재보정을 요구한다. 슬롯 확률표 확정 뒤 검증한다. |
| Q4 | 구 16종 유물 시트 정리 | 새 v30 시트로 런타임 키는 전환했다. 구 `Relic Sheet Normal`/`Relic Sheet Highlight` Addressable 엔트리 제거 여부는 Prefab fallback 시각 검증 뒤 결정한다. |

## Alternatives considered

구 모델 병행과 v23 전체 ScriptableObject 재생성은 [ADR-0005](../adr/0005-relic-v23-runtime-model.md)에 기록한다.
