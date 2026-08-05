# 생성물과 금지 사항

GSSL이 쓰는 산출물은 **재생성 대상**입니다. 데이터 수정은 Google Sheets에서만 하세요.

## 생성 경로

```text
Assets/
├── GoogleSpreadSheetLoader/
│   └── Generated/
│       ├── Cache/
│       ├── Script/
│       │   ├── Enum/
│       │   ├── TableScript/
│       │   └── DataScript/
│       └── SerializeObject/
│           └── TableData/
└── Resources/
    └── Localize_*.json
```

## 직접 수정 금지

| 대상 | 이유 |
|------|------|
| `Generated/Cache/**` | 다음 download/update가 덮어씀 |
| `Generated/Script/**`, ScriptableObject | 코드젠 결과 |
| `Assets/Resources/Localize_*.json` | Localization 시트에서 재생성 |

## 올바른 변경

```text
Google Sheets
  → Sync Pending (mode: update) 또는 에디터 다운로드
  → Cache + 스크립트 / SO / Localize JSON
  → 생성물 diff만 커밋
```

{% hint style="danger" %}
시트 수정 직후 `mode: regenerate`만으로 “최신화”를 대체하지 마세요.  
값은 Sheets → **update(다운로드)** 로 캐시에 들어와야 합니다.
{% endhint %}

## 동기화 실패 시

Cache나 JSON을 손으로 패치하지 마세요. `.cursor/gssl-result.json`과 Unity 콘솔을 보고 인증·네트워크·시트 공유·pending 형식을 고친 뒤 다시 sync 합니다.
