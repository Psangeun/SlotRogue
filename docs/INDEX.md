# 문서 인덱스

SlotRogue의 모든 문서 진입점이다. 새 작업을 시작할 때는 이 파일과 [STATUS.md](./STATUS.md)를 먼저 확인한다.

> 모든 문서는 한국어로 작성한다. 코드, 식별자, 파일명은 영어를 사용한다. 자세한 언어 규칙은 [AGENTS.md](../AGENTS.md)를 따른다.

---

## 최상위

| 파일 | 목적 |
| --- | --- |
| [STATUS.md](./STATUS.md) | 프로젝트 상태 보드. 현재 포커스, 주차 마일스톤, active/completed 작업. |
| [GOVERNANCE.md](./GOVERNANCE.md) | ADR, design-doc, exec-plan, STATUS 운용 규칙의 허브. |

---

## 카테고리

### `adr/` - 결정 기록

결정 1건당 파일 1개를 둔다. 각 ADR은 Context / Decision / Alternatives / Consequences를 포함한다. design-doc은 ADR 번호를 인용하고, 결정의 상세 근거를 본문에 복사하지 않는다.

- [adr/INDEX.md](./adr/INDEX.md) - 모든 결정 목록, 현재 상태, 날짜
- [adr/TEMPLATE.md](./adr/TEMPLATE.md) - 새 ADR 작성 템플릿

### `design-docs/` - 설계 narrative

시스템 개요, 인터페이스, 데이터 흐름, 열린 질문을 기록한다. 각 파일은 `Status: draft | accepted | superseded`를 명시한다.

- [design-docs/INDEX.md](./design-docs/INDEX.md) - 기획/설계 문서 목록

명확하지 않은 결정은 구현 전에 ADR 또는 design-doc으로 먼저 고정한다.

### `exec-plans/` - 진행 상황

기능 단위 작업의 단계별 체크리스트를 기록한다. 현재 active plan은 [STATUS.md](./STATUS.md)가 미러링한다.

- [exec-plans/active/](./exec-plans/active/) - 진행 중 작업
- [exec-plans/completed/](./exec-plans/completed/) - 완료 작업

active에서 completed로 이동하는 절차는 [governance/exec-plans.md](./governance/exec-plans.md)를 따른다.

### `guides/` - How-to

design-doc이나 exec-plan이 아닌 실무 가이드다.

- [guides/unity-setup.md](./guides/unity-setup.md) - Unity 버전, 패키지, 프로젝트 설정, asmdef 기준
- [guides/coding-style.md](./guides/coding-style.md) - C# / Unity 코딩 규칙
- [guides/leaderboard-setup.md](./guides/leaderboard-setup.md) - UGS Leaderboards 설정값과 런타임 계약
- [guides/release-readiness.md](./guides/release-readiness.md) - Android 출시 전 감사 결과, Play Console 문구, Data safety 초안

필요해지면 `mobile-build.md`, `unity-profiling.md`, `addressables-workflow.md`, `package-setup.md`를 추가한다.

### `governance/` - 문서 운용 상세

[GOVERNANCE.md](./GOVERNANCE.md)에서 링크하는 세부 규칙을 둔다. 작업 단위 작성, 갱신, 완료 흐름과 커밋 메시지 규칙을 포함한다.

---

## 처음 기여자가 읽는 순서

1. 루트 [AGENTS.md](../AGENTS.md) - 규칙과 결정 인덱스
2. [STATUS.md](./STATUS.md) - 프로젝트 현재 위치
3. [GOVERNANCE.md](./GOVERNANCE.md) - 작업 추적 방식
4. 관련 시스템의 `design-docs/`와 인용된 `adr/NNNN-*.md`
5. 외부 자료가 필요할 때 [references/INDEX.md](../references/INDEX.md)

---

## 문서 컨벤션

- 파일명은 `kebab-case.md`.
- design-doc은 목적과 `Status` 라인으로 시작한다.
- exec-plan은 체크리스트와 `Completion` 섹션을 포함한다.
- 날짜는 절대 날짜 `YYYY-MM-DD`로 쓴다.
