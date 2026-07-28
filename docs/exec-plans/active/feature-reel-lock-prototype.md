# 심볼 스왑 프로토타입
**Status**: active  
**Started**: 2026-06-29  
**Owner**: Codex  
**Contributors**: _(없음)_  
**Related design-docs**: [`slot-core.md`](../../design-docs/slot-core.md), [`game-flow.md`](../../design-docs/game-flow.md)

## Goal

전투 시스템 본체를 갈아엎지 않고, 플레이어 턴의 개입감을 검증하기 위한 `스핀 후 1회 인접 심볼 스왑` 프로토타입을 `codex/reel-lock-prototype` 브랜치에서 분리 구현한다. 릴 잠금 실험은 재미 검증에서 폐기하고, 상점/별조각 축은 v30 제안/유물 최종안 기준으로 검증한다.

## Prototype Rules

- 플레이어 턴에는 스핀을 1회만 수행한다.
- 스핀 결과가 나온 뒤, 플레이어는 인접한 두 심볼을 드래그해서 최대 1회 서로 바꿀 수 있다.
- 스왑 횟수는 플레이어 턴이 돌아올 때마다 1회로 리필되며, 남은 횟수는 누적되지 않는다.
- 스왑은 가로/세로 인접 셀만 허용한다. 대각선과 비인접 셀은 불가능하다.
- 스왑을 쓰지 않아도 `ATTACK`으로 현재 결과를 공격에 사용한다.
- PC 테스트 편의를 위해 첫 칸 클릭 후 인접 칸 클릭으로도 같은 스왑을 수행할 수 있다.
- 스핀 1회마다 기본 별조각을 지급한다. 단, 스왑을 사용한 턴은 별조각 보상을 지급하지 않는다.
- 릴 잠금 UI/로직은 플레이어 기능에서 제거한다. 씬에 남은 `LockBtn` 5개는 런타임에서 숨긴다.
- 일반/튜토리얼 런은 시작 유물을 지급하지 않고 바로 첫 전투로 진입하며, 튜토리얼 승리 후에는 일반 런으로 전환해 `RewardPanel 1` 제안 화면을 검증한다.
- 전투 승리 후 `RewardPanel 1`에는 v30 제안 최종안 34종 중 3개를 제시한다.
- v30 제안 중 심볼 가중치, 심볼 기본 피해, 별조각 즉시 지급, 배율, 다시, 상태 계열은 현재 런 상태와 전투 수식에 직접 적용한다.
- 전투 중 별도 상점 버튼을 누르면 `ShopPanel`이 열리고, `ShopPanel` 하위 `GameFlowOptionView` 유물 카드 3개와 `RerollButton`으로 별조각을 소비한다.
- `ShopPanel`의 `Ad Button`은 wave당 최대 2회 광고 보상으로 별조각을 1개씩 지급하고 `2/2 → 1/2 → 0/2`로 남은 횟수를 표시한다.
- 상점 버튼이 씬에 없으면 런타임 fallback `ShopButton`을 생성해 `ShopPanel` 토글에 연결한다.
- `Top Panel/Star HUD`와 `ShopPanel/StarPanel`에는 현재 별조각을 표시하며, 씬에 별도 텍스트가 없으면 런타임 텍스트를 생성한다.
- 상점 가격은 v30 최종 HTML의 유물별 `price` 값을 사용한다. 기본 가격은 Common 3, Uncommon 5, Rare 8, Legendary 13이지만 소멸형/저주 예외(`R-39`, `R-40`, `R-72`, `R-73`)는 원본 값을 따른다. 리롤은 별조각 1개다.
- 보유 유물 슬롯은 기본 5칸이며, 제안 효과로 늘어나도 최대 7칸까지만 허용한다.
- v30 유물 중 즉시 지급, 기본 피해 증가, 전투당 스왑 횟수 증가, 전투 시작 별조각 지급, 배율, 다시, 저주, 상태 부여, 선택 대상 상태 조건, 받는 피해 배율은 현재 런 상태와 전투 수식에 직접 적용한다.

## Checklist

- [x] `RelicPresentationDirector`가 하이어라키 Icon 하나만 활성화·비활성화하며 유물 발동을 순차 연출
- [x] `codex/reel-lock-prototype` 브랜치 분리
- [x] 릴 잠금 프로토타입 구현
- [x] `ShopPanel` 유물 3칸, 구매 버튼 3개, `RerollButton` 연결
- [x] `ShopPanel` 별조각 표시와 유물 카드 fallback 렌더링 보강
- [x] 일반/튜토리얼 런 시작 유물 선택 제거
- [x] 전투 후 3택 제안을 유물 제외 보상풀로 재구성
- [x] 릴 잠금 플레이어 로직 제거
- [x] 스핀 후 1회 인접 심볼 드래그/클릭 스왑 상태/입력 구현
- [x] 슬롯 보드/레버 클릭으로 `SPIN` 요청 연결
- [x] 스왑 후 매칭 preview 갱신, `ATTACK` 시 패턴/전투 요청 확정 계산
- [x] 튜토리얼 안내를 스핀 결과 확인 단계에 맞춰 재배치하고 SWAP 별조각 미지급 규칙 반영
- [x] 스왑 대기 중 Addressable 하이라이트 심볼과 tilt pulse cue 표시
- [x] 스왑 확정 전까지 상점 입력 비활성화
- [x] `RewardPanel 1` 자동 탐색과 전투 승리 후 v30 제안 3개 표시 연결
- [x] `ShopPanel`을 상점 버튼으로 열고 구매/리롤/닫기 입력 연결
- [x] `ShopPanel`의 `GameFlowOptionView` 카드 직접 렌더링과 튜토리얼 첫 승리 후 `RewardPanel 1` 진입 보강
- [x] `RewardPanel 1` 이름 정확 매칭과 상점 버튼 fallback 생성 보강
- [x] 전투 HUD `Relic Panel`에 현재 보유 유물 아이콘 표시
- [x] `Top Panel/Star HUD`와 `ShopPanel/StarPanel` 별조각 숫자 표시 연결
- [x] `ShopPanel` 3개 유물 카드 직접 연결 기준 렌더링, 상점 open 중 spin 비활성화, 별조각 HUD 직접 연결 보강
- [x] `ShopPanel` 광고 버튼 wave당 2회 별조각 지급, 잔여 횟수 표시·소진 비활성화·새 wave 초기화
- [x] 전투 `Swap HUD` 전용 View와 현재/최대 횟수(`0/1`, `1/1`) 표시 연결
- [x] `ShopArtifactOptionView` 등급 표시는 tint 컴포넌트 대신 부모 `RunBattleShopView`의 capsule sheet 1회 연결로 Sprite 교체
- [x] `FinalResultDirector` 초기 표시를 0/0/0으로 세팅하고 impact flash duration 경고 제거
- [x] 보유 유물 기본 5칸/최대 7칸 용량 제한과 상점 구매 차단 적용
- [x] 기존 유물 카탈로그를 v30 유물 55종으로 교체
- [x] 기존 보상 제안을 v30 제안 34종으로 교체
- [x] 슬롯 회전 중 짧은 반복 haptic pulse와 릴 정지 tick 연결
- [x] 심볼 가중치 제안을 v30 값(체리/레몬 `+0.3f`, 종/클로버/다이아 `+0.2f`, 세븐 `+0.1f`)과 `×0.5f` 절반 순서 누적으로 정리하고 저장 복원 검증
- [x] 1회성 제안 `P-35`/`P-37`/`P-42`/`P-47`/`P-48` 획득 후 보상풀 제거
- [x] 유물 조건 `EnemyHasStatus`와 받는 피해 배율 `IncomingDamageMul` 전투 훅 연결
- [x] v30 최종 HTML 55종과 `RelicSpecCatalog` ID/이름/가격/주요 효과 재검증, 새 `icon-Sheet_300` 유물 아이콘 Addressable 전환
- [x] `dotnet build SlotRogue.UI.csproj`, `dotnet build SlotRogue.UI.Tests.csproj` 컴파일 검증
- [x] EditMode 테스트 갱신
- [ ] Unity Editor에서 RunGame 전투 UI 수동 플레이테스트
- [ ] 재미검증 후 main 반영 또는 폐기 결정

## Notes

- 2026-07-28: `slot_roguelite_relics_v30_final.html`의 55종과 `RelicSpecCatalog`를 재대조해 누락/초과/이름 불일치를 0개로 맞췄다. R-56/R-57/R-58~R-62/R-70/R-71의 ID 밀림, R-39/R-40/R-72/R-73 가격 예외, 상태/저주 수치를 최종 HTML 기준으로 갱신했다. 유물 아이콘은 84x84 Sprite 56개 시트 `icon-Sheet_300.png`를 Addressable `Relic Sheet 300`으로 등록하고, v30 55종이 앞 55개 아이콘을 카탈로그 순서대로 사용하게 했다.
- 2026-07-16: v30 최종안 기준으로 `RelicSpecCatalog`를 유물 55종과 제안 34종으로 정리했다. P2 백로그/삭제 유물은 상점 풀에서 제거했고, 신규 상태 부여/저주/전설 유물과 `P-47`/`P-48` 계약 제안을 데이터로 추가했다.
- 2026-07-16: 심볼 기본 가중치는 체리/레몬 1.3, 종/클로버 1.0, 다이아 0.8, 7 0.5의 float 값을 유지한다. v30 심볼 가중치 제안은 Amount 3/3/2/2/2/1을 0.1 단위로 적용해 체리/레몬 `+0.3f`, 종/클로버/다이아 `+0.2f`, 세븐 `+0.1f`를 더하고, 절반 제안은 현재 가중치에 0.5를 곱한다.
- 2026-07-16: 1회성 제안은 획득 ID를 런 저장 데이터에 보존하고 이후 보상풀에서 제외한다. `EnemyHasStatus` 조건은 선택 대상의 화상/감염/취약/약화 상태를 보고 평가하며, 유리 대포의 `IncomingDamageMul`은 적 행동 피해 계산 직전에 소비한다.
- 2026-07-16: 스왑 HUD의 남은 횟수 보충 시점을 전투 정산 뒤 다음 스핀 입력이 아니라 레버 복귀 연출 시작 시점으로 앞당겼다. 스왑 입력은 기존처럼 스핀 결과가 나온 뒤에만 활성화된다.
- 2026-07-16: 로비 Play 입력 후 RunGame 씬 전환을 동기 `LoadScene`에서 async 로드로 바꾸고, 로비 상태 텍스트에 `게임 준비 중...`을 먼저 렌더해 시작 딜레이가 멈춤처럼 보이지 않게 했다.
- 2026-07-10: `SlotMachineFrameView`의 레거시 단일 `Slot Machine Animation` 스프라이트 경로와 `_slotMachineSprites` 직렬화 필드를 제거하고, 분리 릴 프레임 전용으로 정리했다.
- 2026-07-10: `Slot Machine Reel Frame Animation` 루트 RectTransform은 프리팹 작성값을 보존하고, 런타임에서는 하위 Reel Frame 이미지 참조와 배치만 갱신하도록 조정했다.
- 2026-07-10: `ShopPanel/Alert Text`를 구매 실패 경고 토스트로 연결했다. 배치된 TMP를 우선 재사용하고, 누락된 런타임 패널은 `RunBattleShopView`가 1회 생성한 fallback TMP를 재사용한다.
- 2026-07-10: `Swap HUD`는 계속 표시하되, 스왑 결정 단계 종료 시 남은 횟수를 0으로 리셋하지 않도록 조정했다. 실제 스왑을 사용했을 때만 `0 / 1`이 된다.
- 2026-07-10: `Slot Machine Panel/Result Text`를 `RunBattleSlotBoardView` 인스펙터 필드로 연결하고, 매칭 족보 중심에 공격력/상태이상 결과 TMP 토큰을 표시하도록 했다.
- 2026-07-10: `Result Text`는 현재 슬롯 패턴 연출 단계에서만 단일 TMP 인스턴스를 재사용하고, 족보 가운데에서 즉시 둥실둥실 떠 있으며 공격 아이콘 파티클이 릴 프레임 위에서 위로 터졌다가 아래로 떨어지도록 수정했다.

- 인벤토리는 유물 전용으로 열고, 심볼/패턴 정보는 별도 설명 패널 탭으로 유지한다.
- 유물 인벤토리 row는 아이콘, 유물 이름, 설명 텍스트를 각각 바인딩한다.

- `BattleSceneHost`의 `ShopDescriptionView`는 하위 검색으로 보강하지 않고 인스펙터 필드와 RunGame 씬 직렬화 참조로 직접 연결한다.

- 전투 피해 적용, 적 턴, Replay 이벤트 타임라인은 유지한다.
- 2026-07-09: 상점 제안을 현재 prefab의 3칸 구성에 맞추고, `shop_star_fragment` Rewarded placement로 wave당 별조각 2개를 1개씩 획득하도록 연결했다. `remove_ads` 구매자는 광고만 건너뛰며 같은 2회 제한을 유지한다.
- 스왑은 전투 계산 확정 전의 보드 편집 단계다. 스핀/스왑 중에는 매칭 셀 preview만 갱신하고, `ATTACK` 이후에 패턴 계산 → 유물/보너스 → 전투 적용 순서를 따른다.
- `RunGame` 씬을 Title Boot 없이 직접 실행해도 스왑 대기 하이라이트가 보이도록 전투 씬 조립 단계에서 `Symbol Sheet Highlight`를 로드해 슬롯 보드에 주입한다.
- 슬롯 회전 haptic은 Android에서 짧은 저강도 `VibrationEffect` pulse를 반복해 "돌돌돌" 느낌을 낸다. Editor와 기본 fallback에서는 단발 `Handheld.Vibrate()`를 쓰지 않아 긴 진동으로 떨어지지 않게 한다.
- 실패 시 브랜치 단위로 자기장 없이 버릴 수 있도록 기존 main 흐름과 분리한다.
