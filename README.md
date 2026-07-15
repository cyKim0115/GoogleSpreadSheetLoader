# Google SpreadSheet Loader

Unity 에디터 확장 도구로, Google SpreadSheet에서 데이터를 다운로드하고 Unity에서 사용할 수 있는 형태로 자동 변환하는 도구입니다.

## 주요 기능

- 🔐 **서비스 계정 인증**: Google 서비스 계정(JSON 키)으로 OAuth2 토큰을 발급받아 인증하므로, 문서를 "링크가 있는 모든 사용자"로 공개할 필요 없이 **비공개 스프레드시트**도 다운로드
- 📥 **Google SpreadSheet 다운로드**: Google Sheets API v4를 사용하여 스프레드시트 데이터를 자동으로 다운로드
- 🔄 **자동 코드 생성**: 다운로드한 데이터를 기반으로 C# 클래스 및 ScriptableObject 자동 생성
- 📊 **테이블 데이터 관리**: 스프레드시트를 Unity의 ScriptableObject로 변환하여 게임 데이터로 활용
- 🌐 **다국어 지원**: Localization 시트를 JSON 형식으로 변환하여 다국어 시스템 구축
- 🔢 **Enum 자동 생성**: 스프레드시트 데이터를 기반으로 Enum 타입 자동 생성

## 설치 방법

1. 이 저장소를 클론하거나 Unity 패키지 파일(`GoogleSpreadSheetLoader.unitypackage`)을 다운로드합니다.
2. Unity 프로젝트에 패키지를 임포트합니다.
3. Unity 에디터에서 `Tools > GSSL > Open Window` 메뉴를 열어 설정을 시작합니다.

## 사용 방법

### 1. 서비스 계정 준비 (Google Cloud Console)

1. [Google Cloud Console](https://console.cloud.google.com/)에서 프로젝트를 생성하거나 선택합니다.
2. **Google Sheets API**를 활성화합니다.
3. `IAM 및 관리자 > 서비스 계정`에서 서비스 계정을 생성합니다.
4. 생성한 서비스 계정에서 **키 추가 > 새 키 만들기 > JSON**을 선택하여 JSON 키 파일을 내려받습니다.
5. 내려받은 JSON 파일에 있는 서비스 계정 이메일(`client_email`)을 확인합니다.
6. 다운로드할 각 스프레드시트를 이 서비스 계정 이메일과 **공유**합니다(뷰어 이상 권한).
   - 이 방식은 API 키와 달리 문서를 "링크가 있는 모든 사용자"로 공개할 필요가 없습니다.

### 2. Unity 에디터 설정

1. Unity 에디터에서 `Tools > GSSL > Open Window` 메뉴를 선택합니다.
2. `설정 편집`을 눌러 편집 모드로 진입합니다.
3. **서비스 계정 JSON 경로**에 내려받은 JSON 파일의 절대 경로를 지정합니다(`찾아보기` 버튼 사용).
   - 경로가 유효하면 서비스 계정 이메일이 표시됩니다. 아직 공유하지 않은 스프레드시트가 있다면 이 이메일과 공유하세요.
4. 다운로드할 스프레드시트 정보를 추가합니다.
   - 스프레드시트 이름과 ID를 입력합니다.
   - 스프레드시트 ID는 URL에서 확인할 수 있습니다: `https://docs.google.com/spreadsheets/d/{SPREADSHEET_ID}/edit`
5. `적용`을 눌러 설정을 저장합니다.

> ⚠️ 서비스 계정 JSON 키는 민감 정보입니다. 저장소에 커밋되지 않도록 프로젝트 밖의 안전한 경로에 두는 것을 권장합니다.

### 3. 시트 필터링 설정

- **시트 타겟 기준**: 포함 또는 제외 옵션 선택
- **시트 타겟 문자열**: 필터링할 문자열 지정 (기본값: `#`)
  - 포함 모드: 지정한 문자열을 포함하는 시트만 다운로드
  - 제외 모드: 지정한 문자열을 포함하는 시트는 제외

### 4. 데이터 다운로드 및 생성

1. 에디터 윈도우에서 다운로드 버튼을 클릭합니다.
2. 스프레드시트 데이터가 자동으로 다운로드됩니다.
3. 다운로드된 데이터를 기반으로 다음 항목들이 자동 생성됩니다:
   - **Enum**: `EnumDef` 타입으로 지정된 시트에서 Enum 클래스 생성
   - **TableData**: 테이블 데이터 클래스 및 ScriptableObject 생성
   - **Localization**: `Localization` 타입으로 지정된 시트에서 다국어 JSON 파일 생성

## 스프레드시트 형식

### 테이블 데이터 형식

첫 번째 행은 헤더로 사용되며, 다음 형식을 따릅니다:

```
변수명-타입
```

예시:
```
ID-int
Name-string
HP-float
Type-ItemType
```

지원하는 타입:
- `int`, `float`, `bool`, `long`, `double`, `string`
- 사용자 정의 Enum 타입

### Enum 생성 형식

Enum을 생성하려면 시트 이름에 `EnumDef` 문자열이 포함되어야 합니다.

- 헤더에 Enum 이름을 지정합니다.
- `-`가 포함된 헤더는 인덱스로 사용됩니다 (예: `ItemType-0`, `ItemType-1`)
- 각 행의 값이 Enum 항목으로 추가됩니다.

### Localization 형식

다국어 데이터를 생성하려면 시트 이름에 `Localization` 문자열이 포함되어야 합니다.

- 헤더는 테이블과 동일하게 `변수명-타입` 형식을 사용합니다.
- `id` 헤더를 가진 열이 키로 사용됩니다.
- 나머지 각 열(언어)마다 `Assets/Resources/Localize_{변수명}.json` 파일이 생성됩니다 (예: `id-string`, `Korean-string`, `English-string` → `Localize_Korean.json`, `Localize_English.json`).
- 저장 형식은 `{ "Key": ..., "Value": ... }` 항목의 배열이며, 런타임 `LocalizeTable`이 그대로 읽어 들입니다.

## 생성되는 파일 구조

```
Assets/
├── Generated/
│   ├── Enum/              # 생성된 Enum 클래스
│   ├── TableScript/       # 생성된 테이블 클래스
│   ├── DataScript/        # 생성된 데이터 클래스
│   └── TableData/         # 생성된 ScriptableObject 에셋
└── Resources/
    └── Localize_*.json    # 다국어 JSON 파일
```

## 사용 예시

### 생성된 테이블 데이터 사용

```csharp
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemTable itemTable; // Inspector에서 할당
    
    void Start()
    {
        foreach (var item in itemTable.dataList)
        {
            Debug.Log($"Item: {item.Name}, HP: {item.HP}");
        }
    }
}
```

### 다국어 시스템 사용

```csharp
using UnityEngine;

public class LocalizationExample : MonoBehaviour
{
    void Start()
    {
        LocalizeTable.Initialize(SystemLanguage.Korean);
        string text = "ITEM_NAME".GetLocalizeText();
        Debug.Log(text);
    }
}
```

## 요구사항

- Unity 6000.2 이상 (`Awaitable` API 사용)
- Google Cloud 서비스 계정 JSON 키 (Google Sheets API 활성화 필요)
- Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`, Unity Package Manager를 통해 설치)

