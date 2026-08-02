# UGS Leaderboards 설정 가이드

_Last updated: 2026-08-02_

이 문서는 SlotRogue 출시 빌드가 기대하는 Unity Gaming Services Leaderboards 설정값을 기록한다. 코드 쪽 계약은 [ADR-0012](../adr/0012-leaderboard-nickname-only-profile.md)를 따른다.

## 1. Unity 프로젝트 연결

- Unity Cloud Project ID: `b1aeb280-d45e-44b0-b0cb-0279b956f852`
- Project Settings 기준 `organizationId`: `danggunee`
- Project Settings 기준 `projectName`: `Slot Rogue`
- `cloudEnabled` 값은 로컬 설정상 `0`으로 보일 수 있다. UGS 런타임은 Unity Services 초기화와 Cloud Project 연결 상태를 별도로 확인한다.

## 2. 리더보드 리소스

UGS Dashboard에서 아래 리더보드가 있어야 한다.

| 항목 | 값 |
| --- | --- |
| Leaderboard ID | `Slot_Rogue_Leaderboard` |
| Ranking type | `Highest to lowest` |
| Update type | `Best score` |
| Score format | `Numeric` |
| Reset schedule | `Every week on Tuesday at 15:00 starting on August 4th, 2026 UTC` |
| KST 기준 리셋 | 매주 수요일 00:00 |
| Buckets | `None` |
| Tiers | `None` |

`Archive scores on reset`은 MVP 출시 기준 필수는 아니다. 과거 시즌 결과를 게임 안에서 다시 보여줄 계획이 생기면 켜고, 그 전에는 운영 부담을 줄이기 위해 꺼둬도 된다.

## 3. 런타임 코드 계약

코드 기준값:

- `LeaderboardConstants.Id`: `Slot_Rogue_Leaderboard`
- `LeaderboardConstants.MetadataSchemaVersion`: `3`
- `LeaderboardConstants.DisplayLimit`: `100`
- 최고 기록 저장 키: `SlotRogue.Leaderboard.BestWave`
- UGS 초기화 위치: `BootController` -> `SlotRogueLeaderboardService.InitializeAsync()`

점수는 현재 런에서 도달한 wave 값이다. 코드상 `Capture(...)`가 `RunProgress.CurrentBattleNumber`를 최소 `1`로 보정해 `score`와 metadata `wave`에 넣는다.

제출 metadata 구조:

| 필드 | 설명 |
| --- | --- |
| `schemaVersion` | metadata 스키마 버전. 현재 `3` |
| `wave` | 제출 점수와 같은 도달 wave |
| `relicIds` | 런 종료 시 보유 유물 ID 목록 |
| `symbolCounts` | 심볼별 가중치/카운트 스냅샷 |
| `profileIconId` | 플레이어가 고른 랭킹 아이콘 |
| `message` | 플레이어가 고른 랭킹 메시지 |

## 4. Mock 랭킹 주의

`SlotRogueLeaderboardService.UseMockEntries`는 `UNITY_EDITOR || DEVELOPMENT_BUILD`에서만 컴파일된다.

- 일반 릴리즈 빌드: mock 랭킹 코드가 빠진다.
- Editor / Development Build: `UseMockEntries`가 켜져 있으면 UGS 조회 대신 샘플 랭킹을 보여준다.
- 실서비스 검증: Editor 또는 Development Build에서 `UseMockEntries = false`로 바꾸고 제출/조회가 실제 UGS에 도달하는지 확인한다.

## 5. 출시 전 확인 절차

1. UGS Dashboard에서 위 리더보드 설정값을 다시 확인한다.
2. 닉네임 설정 후 한 런을 종료해 최고 기록 자동 제출을 확인한다.
3. Dashboard 또는 클라이언트 랭킹 UI에서 같은 점수가 조회되는지 확인한다.
4. 더 낮은 점수를 다시 제출했을 때 `Best score` 정책으로 최고 기록이 유지되는지 확인한다.
5. 2026-08-04 15:00 UTC 이후 첫 주간 리셋이 의도대로 동작하는지 운영 로그로 확인한다.
