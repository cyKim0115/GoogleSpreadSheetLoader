# system-crew (on-demand)

이 프로젝트에서는 system-crew가 **상시 적용되지 않는다**. 필요할 때만 호출한다.

## 호출

- 채팅: `system-crew`, `시스템 크루`, `Producer로`, `참고 재현해줘`, `아이디어 평가해줘`
- Skill: `.cursor/skills/system-crew`

## 업데이트

```powershell
git submodule update --remote .cursor/system-crew
powershell -File .cursor/system-crew/scripts/sync-to-project.ps1 -Mode OnDemand
```
