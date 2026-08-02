# 출시 준비 체크리스트

_Last updated: 2026-08-02_

이 문서는 SlotRogue Android 출시 후보를 Play Console 트랙에 올리기 전 확인해야 할 항목과 현재 로컬 감사 결과를 기록한다.

## 1. 현재 로컬 스냅샷

| 항목 | 현재 값 / 결과 |
| --- | --- |
| Unity | `6000.3.10f1` |
| Git | `main`, `origin/main`보다 1커밋 앞섬, 미커밋 변경 있음 |
| Android package name | `com.DevRaptor.SlotRogue` |
| Version / versionCode | `1.0` / `7` |
| Min / Target SDK | `29` / `36` |
| Scripting Backend | IL2CPP |
| Android architecture | `AndroidTargetArchitectures: 3` - ARM64 포함 |
| Build scenes | `00_TitleScene`, `10_LobbyScene`, `20_RunGameScene` 활성화 |
| Icon | 기본 아이콘 `Assets/_Project/Art/UI/appicon2.png`, Android 플랫폼별 아이콘 슬롯은 비어 있음 |
| Keystore | `user.keystore`, alias `raptor` |
| `.meta` 감사 | 누락 `.meta` 0개, 고아 `.meta` 0개 |
| 코드 빌드 | `dotnet build SlotRogue.slnx --no-restore` 성공, warning 0 / error 0 |
| 테스트 프로젝트 | `dotnet test`는 종료 코드 0이나 Unity Test Framework 테스트 개수는 dotnet 출력에서 확인되지 않음 |

## 2. 출시 차단 / 확인 필요

- LevelPlay 런타임 ID가 비어 있다. `AdsManager`의 production `appKey`, `rewardedAdUnitId`, `productionAdsEnabled`를 릴리즈 빌드에서 실제 값으로 설정해야 한다.
- `remove_ads` IAP 상품은 코드와 Codeless IAP Button에 연결되어 있으나 Play Console 상품 상태, 라이선스 테스트 구매, 복원 흐름은 별도 확인이 필요하다.
- UGS 리더보드는 Dashboard 계약이 확인되었지만, mock을 끈 실제 제출/조회 실기기 검증이 아직 출시 게이트다.
- AAB 빌드와 서명 검증이 필요하다. Google Play 신규 앱/업데이트는 2026-08-31부터 Android 16/API 36 타겟 요구를 적용받는다.
- Android 플랫폼별 adaptive icon 슬롯이 비어 있다. 기본 아이콘만으로 런처/스토어/설정 화면이 의도대로 보이는지 확인한다.
- `user.keystore`가 저장소에 추적 중이다. 비공개 저장소라도 release/upload key로 쓰는 파일은 백업과 접근 권한을 분리하고, 비밀번호가 커밋되지 않았는지 확인한다. 공개 저장소라면 키 교체를 검토한다.
- `Assets/_Recovery/0.unity`가 추적 중이다. 빌드 씬에는 없지만, 출시 브랜치에서 유지할 이유가 있는지 확인한다.
- 기본 랭킹 메시지 `허접ㅋ`가 스토어 연령 등급과 공개 랭킹 UX에 맞는지 출시 전에 다시 검토한다.
- 개인정보처리방침 URL, Data safety, 광고 포함 여부, 콘텐츠 등급, 앱 액세스, 인앱 상품 정보를 Play Console에 입력해야 한다.

## 3. UGS 리더보드 설정

현재 출시 계약:

- Leaderboard ID: `Slot_Rogue_Leaderboard`
- Ranking type: `Highest to lowest`
- Update type: `Best score`
- Score format: `Numeric`
- Reset schedule: `Every week on Tuesday at 15:00 starting on August 4th, 2026 UTC`
- KST 기준: 매주 수요일 00:00
- Buckets / Tiers: 없음

`Archive scores on reset`은 MVP 기준 필수가 아니며, 과거 시즌 결과를 클라이언트에서 보여줄 때 켠다.

## 4. 광고 / IAP 연결 점검

광고 코드 연결:

- `00_TitleScene`에 `AdsManager`가 직렬화되어 있다.
- `AdsManager`는 LevelPlay 9.4.1을 `LevelPlay.Init(...)`으로 초기화하고 `LevelPlayRewardedAd`로 보상형 광고를 로드한다.
- 배치 ID 상수는 `revive`, `reward_reroll`, `reward_extra`, `reward_double`, `shop_star_fragment`다.
- 2026-08-02 기준 production `appKey`와 `rewardedAdUnitId`가 비어 있고 `productionAdsEnabled`가 꺼져 있다. 실제 출시 빌드는 이 상태로 광고가 초기화되지 않는다.

IAP 코드 연결:

- `Assets/Resources/IAPProductCatalog.json`에 `remove_ads` 상품이 있다.
- 코드 기준 상품 ID는 `AdsRemoveState.ProductId = "remove_ads"`다.
- `10_LobbyScene`의 `CodelessIAPButton` productId가 `remove_ads`로 연결되어 있다.
- Inspector 이벤트는 `IapFulfillmentHandler.OnPurchaseFetched`, `IapFulfillmentHandler.OnOrderPending`으로 연결되어 있다.
- 복원 흐름은 `IapStoreConnectionCallbacks.OnRestoredProduct(...)`가 fulfillment로 전달한다. Play Console 라이선스 테스트에서 재설치/복원 시나리오를 확인한다.
- 로컬 entitlement 캐시는 `slotrogue.iap.remove_ads` PlayerPrefs 키를 사용한다.

## 5. Play Console 문구 초안

짧은 설명:

> 슬롯을 돌리고 유물을 모아 끝없는 우주 전투를 버티는 로그라이크

긴 설명:

> SlotRogue는 슬롯 결과로 전투를 풀어가는 모바일 로그라이크입니다. 매 턴 슬롯을 돌려 공격, 방어, 회복, 자원을 만들고, 런 중 획득한 유물 조합으로 더 깊은 웨이브에 도전하세요.
>
> 핵심 요소
> - 슬롯 조합으로 달라지는 전투 흐름
> - 런마다 바뀌는 유물 선택과 성장
> - 짧게 즐길 수 있는 모바일 전투 템포
> - 최고 wave를 겨루는 주간 랭킹
> - 보상형 광고와 광고 제거 구매 지원
>
> 이 게임은 실제 현금 베팅이나 환전을 제공하지 않습니다.

릴리즈 노트:

> 첫 Android 출시입니다.
> - 튜토리얼과 메인 런 전투 추가
> - 유물, 상점, 보상 선택 흐름 추가
> - 주간 최고 wave 랭킹 추가
> - 보상형 광고 및 광고 제거 구매 추가
> - 모바일 알림과 기본 품질 개선 적용

## 6. 개인정보 / Data safety 초안

최종 제출 전 Unity SDK와 Google Play SDK Index 기준으로 다시 대조한다.

수집 또는 처리 가능 데이터:

- 닉네임: UGS Authentication Player Name으로 저장되며 랭킹 표시명으로 공개될 수 있다.
- 게임 플레이 기록: 최고 wave, 유물 ID 목록, 심볼 카운트, 프로필 아이콘, 랭킹 메시지가 UGS Leaderboards metadata로 제출된다.
- 구매 정보: `remove_ads` 구매와 복원은 Google Play Billing / Unity IAP를 통해 처리하며, 앱은 광고 제거 보유 여부를 로컬에 캐시한다.
- 광고 관련 데이터: Unity LevelPlay와 연결된 광고 네트워크가 광고 ID, 기기 정보, 진단 정보, 대략적 위치 등을 처리할 수 있다. 실제 Data safety 항목은 활성화한 광고 네트워크 기준으로 확정한다.
- 알림: 로컬 알림만 사용한다. 서버 push token은 현재 사용하지 않는다.

현재 코드 기준 앱이 직접 저장하지 않는 항목:

- 이메일
- 전화번호
- 주소
- 국가 선택값
- 연락처
- 사진 또는 동영상

개인정보처리방침에 반드시 포함할 내용:

- UGS Authentication / Leaderboards 사용
- Google Play Billing 사용
- Unity LevelPlay 및 광고 네트워크 사용
- 보상형 광고 시청 보상과 광고 제거 구매
- 공개 랭킹에 표시되는 닉네임/점수/metadata 범위
- 데이터 삭제 또는 닉네임 변경 요청 방법

## 7. 실기기 최종 체크

1. Release 또는 non-development 빌드로 AAB 생성.
2. 서명된 AAB를 Play Console 내부 테스트 트랙에 업로드.
3. 신규 설치, 튜토리얼, 한 런 완료, 패배 결과, 랭킹 제출을 확인.
4. 보상형 광고 로드/시청/보상 지급/실패 처리를 확인.
5. `remove_ads` 테스트 구매 후 광고 버튼 상태와 보상형 광고 우회 동작을 확인.
6. 구매 복원 또는 재설치 후 entitlement 복구를 확인.
7. 주간 리셋 알림 예약과 권한 요청 UX를 확인.
8. 네트워크 끊김 상태에서 랭킹, 광고, IAP 실패 처리가 막히지 않는지 확인.

## 8. 릴리즈 직전 Git 체크

- 의도한 변경만 남아 있는지 `git status --short --branch`로 확인한다.
- Unity 자동 생성물, 성능 테스트 임시 JSON, 복구 씬 복사본을 커밋하지 않는다.
- 새 에셋과 `.meta`가 항상 같이 들어갔는지 확인한다.
- keystore, 광고 key, IAP 비밀값 등 민감 정보가 새로 추가되지 않았는지 확인한다.
- 릴리즈 커밋은 `docs/STATUS.md`와 관련 guide 변경을 같은 커밋에 포함한다.

## 9. 참고 링크

- [Google Play 대상 API 수준 요구사항](https://support.google.com/googleplay/android-developer/answer/11926878)
- [Google Play Data safety](https://support.google.com/googleplay/android-developer/answer/10787469)
- [Google Play Android App Bundle 안내](https://support.google.com/googleplay/android-developer/answer/9844679)
- [Unity Leaderboards 문서](https://docs.unity.com/en-us/leaderboards/get-started)
- [Unity LevelPlay Rewarded Ads 문서](https://docs.unity.com/en-us/grow/levelplay/sdk/unity/rewarded-ad-integration-package)
