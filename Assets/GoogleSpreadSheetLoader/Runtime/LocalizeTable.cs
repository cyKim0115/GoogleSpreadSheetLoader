using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global

public static class LocalizeTable
{
    private static Dictionary<string, string> dicLocalize = new();

    public static void Initialize(SystemLanguage language)
    {
        var assetName = $"Localize_";
        switch (language)
        {
            case SystemLanguage.Korean:
            case SystemLanguage.English:
                assetName += $"{language}";
                break;
            default:
                Debug.LogError($"해당 국가코드 ({language}) 는 정의 되지 않아 en으로 대체합니다.");
                assetName += $"{SystemLanguage.English}";
                break;
        }

        var obj = Resources.Load<TextAsset>(assetName);

        dicLocalize = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj.text);
    }

    public static string GetLocalizeText(this string key, params object[] param)
    {
        if (dicLocalize.TryGetValue(key, out var result))
        {
            return string.Format(result, param);
        }
            
        return "!" + key;
    }
}