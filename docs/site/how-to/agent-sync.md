# Agent 동기화

Unity Editor를 연 채로, AI/에이전트(또는 스크립트)가 **일부 시트만** 갱신할 때 쓰는 흐름입니다.

## 전제

- 해당 Unity 프로젝트가 Editor에 열려 있음
- Unity Skills 서버 실행 (`Window > UnitySkills > Start Server`) — 메뉴 자동 실행 시
- Google Sheets 읽기/쓰기 수단 (예: MCP gsheets)
- SettingData에 서비스 계정 JSON 절대 경로, 대상 시트는 서비스 계정과 공유됨
- 대상 시트가 이미 `Generated/Cache/cache_index.json`에 등록됨

## 절차

{% stepper %}
{% step %}
## 스키마 확인 (읽기만)

`Assets/GoogleSpreadSheetLoader/Generated/Cache/cache_index.json`과 `Generated/Cache/<Sheet>.txt`를 봅니다. **편집하지 않습니다.**
{% endstep %}

{% step %}
## Google Sheets 수정

값·서식을 시트에 반영합니다.
{% endstep %}

{% step %}
## pending 파일 작성

경로: `.cursor/gssl-pending.json`

```json
{
  "mode": "update",
  "sheets": ["MyTableSheet"]
}
```

`mode: "update"`는 해당 시트를 다시 다운로드한 뒤 생성합니다.  
`mode: "regenerate"`는 **이미 받은 캐시**로 코드만 다시 뽑을 때 쓰며, 시트 수정 직후 대체용으로 쓰지 마세요.
{% endstep %}

{% step %}
## Unity 메뉴 실행

`Tools/GSSL/Sync Pending Sheets` (update) 또는 `Tools/GSSL/Regenerate Pending Sheets` (regenerate).
{% endstep %}

{% step %}
## 결과 확인

`.cursor/gssl-result.json`의 `status`가 `success`인지 확인합니다.  
성공 시 pending 파일이 정리되는 것이 정상입니다. 생성물 diff만 커밋합니다.
{% endstep %}
{% endstepper %}

## 하지 말 것

- Cache / `Localize_*.json` 손편집으로 “일단 반영”
- 시트 수정 직후 regenerate만으로 최신화
- Editor가 열린 상태에서 Unity CLI `-batchmode`로 같은 프로젝트 sync

에이전트용 상세 원문(비공개): 레포 `.cursor/skills/project-workflows/gssl-agent-workflow/`
