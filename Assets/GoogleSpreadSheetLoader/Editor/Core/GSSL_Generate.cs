using System;
using System.Collections.Generic;
using System.IO;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Setting;
using TableData;
using UnityEditor;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace GoogleSpreadSheetLoader.Generate
{
    public partial class GSSL_Generate
    {
        private static readonly string tableScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Script/TableScript/";
        private static readonly string dataScriptSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Script/DataScript/";
        private static readonly string enumDefSavePath = "Assets/GoogleSpreadSheetLoader/Generated/Script/Enum/";
        private static readonly string dataSavePath = "Assets/GoogleSpreadSheetLoader/Generated/SerializeObject/TableData/";


        private static void CheckAndCreateDirectory()
        {
            if (!Directory.Exists(tableScriptSavePath))
            {
                Directory.CreateDirectory(tableScriptSavePath);
            }

            if (!Directory.Exists(dataScriptSavePath))
            {
                Directory.CreateDirectory(dataScriptSavePath);
            }

            if (!Directory.Exists(dataSavePath))
            {
                Directory.CreateDirectory(dataSavePath);
            }

            if (!Directory.Exists(enumDefSavePath))
            {
                Directory.CreateDirectory(enumDefSavePath);
            }
        }

        public static List<SheetData> GetSheetDataList()
        {
            var sheetDataAssetPath = GSSL_Path.GetPath(ePath.SheetData_Asset);
            var guids = AssetDatabase.FindAssets("", new[] { sheetDataAssetPath });

            if (guids.Length == 0)
            {
                return null;
            }

            var listSheetData = new List<SheetData>();

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                SheetData sheetData = AssetDatabase.LoadAssetAtPath<SheetData>(assetPath);
                listSheetData.Add(sheetData);
            }

            return listSheetData;
        }

        public static List<string> GetTableNameList()
        {
            var guids = AssetDatabase.FindAssets("", new[] { dataSavePath });

            if (guids.Length == 0)
            {
                Debug.LogError($"length 0");
                
                return null;
            }

            var listTable = new List<string>();

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                Debug.LogError(data.GetType().ToString());
                
                listTable.Add(data.GetType().ToString());
            }

            return listTable;
        }
    }
}