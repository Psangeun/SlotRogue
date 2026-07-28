# References 인덱스

외부 자료(Unity 매뉴얼 섹션, 게임 디자인 자료, SDK 문서)를 *언제 참조하는지*만 기록한다. **사본을 박제하지 않는다.**

> 사용 규칙은 [`../AGENTS.md`](../AGENTS.md) §4 참조.

---

## 현재 상태

### 슬롯 전투 로그라이트 유물 풀 v30 최종

- **링크**: `C:/Users/danggunee/Downloads/slot_roguelite_relics_v30_final.html` (2026-07-28 전달본)
- **언제 참조하나**: 별조각 상점 유물 55종의 ID·분류·등급·지속·가격·이름·플레이어 설명·개발 수치를 구현하거나 검증할 때
- **부가 메모**: HTML의 `relic-data` JSON 55종이 현재 `RelicSpecCatalog` 유물 데이터의 기준이다. 시작 유물은 없고 전부 런 중 상점 구매 유물이다.

### 슬롯 전투 로그라이트 유물 풀 v23

- **링크**: `relic_pool_v23_status_balance_patch.html` (2026-06-12 전달본)
- **언제 참조하나**: v23에서 v30으로 넘어온 유물 런타임 모델의 역사적 맥락이나 폐기된 시작 유물 경로를 확인할 때
- **부가 메모**: HTML의 `RELICS` 배열 80종과 시작 유물 S-01~S-06은 v30 상점형 카탈로그로 대체되었다.

### 슬롯 전투 속성 / 방해 설계표 v6

- **링크**: `C:/Users/binde_mt7hytl/OneDrive/문서/카카오톡 받은 파일/slot_battle_status_interference_design_v6.html` (2026-06-22 전달본)
- **언제 참조하나**: 유물과 연계된 화상·감염·흡혈·취약·약화·가시 구현, 몬스터/엘리트 상태 행동, 보스 슬롯 방해를 설계하거나 검증할 때
- **부가 메모**: 프로젝트 내부 재작성본은 [`../docs/design-docs/attribute-status-interference.md`](../docs/design-docs/attribute-status-interference.md)이다. v6은 가산 누적 허용 + 자연 감쇠, 곱연산 상한 원칙을 확정했다.

---

## 형식

```markdown
### <자료명>

- **링크**: <URL 또는 파일 경로>
- **언제 참조하나**: 한 줄 — 어떤 상황에서 이 자료가 필요한가
- **부가 메모**: (선택) 특정 챕터·페이지 포인터
```

---

## 추가 예정 후보 (참조 시 채움)

| 분류 | 자료 |
|------|------|
| Unity 매뉴얼 | Addressables, UniTask 사용 패턴, Mobile Optimization, Profiler |
| 게임 디자인 | 슬롯 RTP / volatility 이론, 로그라이크 메타 진행 사례 분석 |
| SDK / 도구 | Google Play 정책, AdMob / Unity Ads, IAP 통합 가이드 |
| 모바일 UX | Safe Area, 터치 타깃 사이즈, 한국 모바일 게이머 onboarding 패턴 |

위 표는 가이드일 뿐 — **실제로 참조할 때만** 항목으로 추가한다.

---

## 안티패턴

- 외부 repo / 책 전체를 `references/`에 복붙 → 라이선스·용량·stale 위험. 링크만.
- "언젠가 볼 것 같은" 자료를 미리 등록 → 노이즈. 실제 참조한 자료만.
- 외부 자료를 검색할 때 vendor 트리(`Assets/Plugins/<vendor>/`)를 통째로 읽기 → 항상 Grep으로 좁힌 뒤 필요한 부분만.
