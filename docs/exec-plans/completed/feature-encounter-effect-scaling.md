# Encounter 효과 수치 스케일링

**Status**: completed
**Started**: 2026-07-25
**Owner**: Codex
**Related design-docs**: [`combat-core.md`](../../design-docs/combat-core.md), [ADR-0022](../../adr/0022-encounter-shared-effect-scaling.md)

## Goal

기존 Encounter 난이도 배율을 적 최대 HP와 적 행동의 Damage, Shield, Heal, StatusEffect Amount에 공용 적용한다. 원본 몬스터 패턴 SO와 LockSlot 수치는 변경하지 않는다.

## Checklist

- [x] 공용 Encounter 배율 결정과 전투 설계를 문서화한다.
- [x] Encounter 설정·계산·적 행동 조립 경로에 공용 배율을 연결한다.
- [x] HP·피해·보호막·회복·상태이상 및 LockSlot 제외를 EditMode 테스트로 추가하고 solution compile로 검증한다.
- [x] plan과 STATUS를 완료 상태로 갱신한다.

## Notes

- 양수 Amount는 HP와 동일한 AwayFromZero 반올림, 0은 유지, 상한 없음으로 확정했다.
- `dotnet build SlotRogue.sln --no-restore`는 경고·오류 없이 통과했다. Unity EditMode Test Runner의 실제 실행은 Unity Editor에서 후속 확인한다.
- 2026-07-25 후속 결정: 취약·약화·흡혈 Amount는 남은 적용 횟수이므로 공용 스케일에서 제외했다.
- 이 후속 변경의 재컴파일은 Windows SDK 접근 권한 요청이 거절되어 실행하지 못했다. Unity Editor Test Runner에서 `EnemyCombatantFactoryTests`를 실행해 확인한다.

## Completion

(`completed/`로 옮길 때 채움.)
- **Finished**: 2026-07-25
- **Outcome**: 기존 Encounter 난이도 배율을 적 HP와 Damage·Shield·Heal 및 화상·감염·가시·동결 상태 수치에 공용 적용했다. 취약·약화·흡혈 적용 횟수는 고정했다.
- **Follow-ups**: Unity EditMode Test Runner에서 EncounterScalingTests와 EnemyCombatantFactoryTests를 실행한다.
