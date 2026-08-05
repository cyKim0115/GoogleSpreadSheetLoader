# 시트 형식

Setting / 시트 이름에 들어 있는 타입 문자열로 GSSL이 처리 방식을 나눕니다. 기본 문자열은 설정에서 바꿀 수 있습니다.

## Common (일반 테이블)

첫 행은 스키마입니다. 셀 형식:

```text
변수명-타입
```

예:

```text
ID-int
Name-string
HP-float
Type-ItemType
```

지원 타입: `int`, `float`, `bool`, `long`, `double`, `string`, 사용자 정의 Enum.

헤더에 `-`가 **없으면** 코드젠 필드로 쓰지 않습니다. 코멘트/메모 열(`#` 또는 빈 헤더)에 적합합니다.

## EnumDef

시트 이름에 Enum 타입 문자열(기본 `EnumDef` 계열)이 포함되면 Enum을 생성합니다.

- 헤더에 Enum 이름
- `-`가 있는 헤더는 인덱스 (예: `ItemType-0`)
- 각 행 값이 Enum 멤버

EnumType 시트의 색 규칙은 Common과 다를 수 있습니다. 기존 시트 패턴을 따르는 것이 안전합니다.

## Localization

시트 이름에 localization 타입 문자열(기본 `Localization`)이 포함되면 다국어 JSON을 만듭니다.

- 키 열: `id-string` / `ID-string`
- 언어 열: `Korean-string` 등 → `Assets/Resources/Localize_Korean.json`
- 권장 행: row1 스키마, row2 테스트 행, row3+ 실키

실무 규칙은 [Localization 시트](../how-to/localization.md), 색·메모 열은 [시트 서식 맞추기](../how-to/sheet-formatting.md)를 보세요.
