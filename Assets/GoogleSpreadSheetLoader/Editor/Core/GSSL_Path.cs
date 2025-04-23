using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public enum ePath
    {
        None,
        SettingData,
        SheetData_Asset,
    }
        
    public static class GSSL_Path
    {
        private static readonly Dictionary<ePath, string> _dicPath = new()
        {
            { ePath.SettingData, "Assets/GoogleSpreadSheetLoader/"},
            { ePath.SheetData_Asset, "Assets/GoogleSpreadSheetLoader/Generated/SerializeObject/Sheet"},
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