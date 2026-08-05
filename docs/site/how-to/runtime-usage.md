# 런타임에서 쓰기

생성물을 게임/툴 코드에서 읽는 최소 예시입니다. 프로젝트 아키텍처에 맞게 감싸 쓰세요.

## 테이블 (ScriptableObject)

Inspector에 생성된 테이블 에셋을 할당해 사용합니다.

```csharp
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public ItemTable itemTable;

    void Start()
    {
        foreach (var item in itemTable.dataList)
        {
            Debug.Log($"Item: {item.Name}, HP: {item.HP}");
        }
    }
}
```

실제 클래스·필드명은 시트 헤더와 코드젠 결과에 따릅니다.

## Localization (샘플)

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

{% hint style="info" %}
`LocalizeTable`은 패키지 샘플입니다. 소비 프로젝트의 `Util`/`LanguageUtil` 등과 결합할지는 프로젝트 규칙에 맡깁니다.  
키 추가는 항상 Localization 시트 → Sync 경로로 하세요.
{% endhint %}

## 관련 개념

- [생성물과 금지 사항](../concepts/generated-outputs.md)
- [Localization 시트](localization.md)
