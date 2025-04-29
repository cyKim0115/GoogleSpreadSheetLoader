using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace GoogleSpreadSheetLoader.Generate
{
    public static partial class GSSL_Generate
    {
        public static List<SheetData> GetSheetDataList()
        {
            var sheetDataAssetPath = GSSL_Path.GetPath(ePath.SheetData);
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
            var dataSavePath = GSSL_Path.GetPath(ePath.TableData);
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