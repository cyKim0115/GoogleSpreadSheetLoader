using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public enum ePath
    {
        None,
        SettingData,
        SheetData,
        TableScript,
        DataScript,
        Enum,
        TableData,
        TableLinkerScript,
        TableLinkerData,
    }
        
    public static class GSSL_Path
    {
        private static readonly Dictionary<ePath, string> _dicPath = new()
        {
            { ePath.SettingData, "Assets/GoogleSpreadSheetLoader/"},
            { ePath.SheetData, "Assets/GoogleSpreadSheetLoader/Generated/SerializeObject/Sheet"},
            {ePath.TableScript, "Assets/GoogleSpreadSheetLoader/Generated/Script/TableScript/"},
            {ePath.DataScript, "Assets/GoogleSpreadSheetLoader/Generated/Script/DataScript/"},
            {ePath.Enum, "Assets/GoogleSpreadSheetLoader/Generated/Script/Enum/"},
            {ePath.TableData, "Assets/GoogleSpreadSheetLoader/Generated/SerializeObject/TableData/"},
            {ePath.TableLinkerScript, "Assets/GoogleSpreadSheetLoader/Generated/Script/"},
            {ePath.TableLinkerData, "Assets/Resources/"},
        };

        public static string GetPath(ePath path)
        {
            if (_dicPath.TryGetValue(path, out var result))
            {
                if (!Directory.Exists(result))
                    Directory.CreateDirectory(result);
                
                return result;
            }
            
            Debug.LogError($"GSSL_Path : 정의 되지 않은 패스 ePath({path})");
            return "";
        }
    }
}