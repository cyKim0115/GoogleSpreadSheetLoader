using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public static class GSSL_DownloadedSheet
    {
        // <SpreadSheetName, <SheetName, SheetData>>
        private static readonly Dictionary<string, List<SheetData>> _dicDownloadedSheet = new();

        private static bool _isInitialized;

        public static void Reset()
        {
            _isInitialized = false;

            _dicDownloadedSheet.Clear();
        }

        public static List<SheetData> GetAllSheetData()
        {
            var sheetDataAssetPath = GSSL_Path.GetPath(ePath.SheetData);
            var guids = AssetDatabase.FindAssets("", new[] { sheetDataAssetPath });

            return guids.Length == 0
                ? null
                : guids.Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<SheetData>).ToList();

        }

        public static List<SheetData> GetSheetData(string spreadSheetId)
        {
            RefreshIfNotInitialized();

            return _dicDownloadedSheet.TryGetValue(spreadSheetId, out var result)
                ? result
                : new();
        }

        private static void RefreshDownloadedSheet()
        {
            _dicDownloadedSheet.Clear();

            var sheetPath = GSSL_Path.GetPath(ePath.SheetData);

            var guids = AssetDatabase.FindAssets("", new[] { sheetPath });

            if (guids.Length == 0)
                return;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(SheetData));
                var sheetData = asset as SheetData;

                if (sheetData == null)
                {
                    Debug.LogError($"SheetData 형식이 아닙니다. ({asset.name})");
                    continue;
                }

                _dicDownloadedSheet.TryAdd(sheetData.spreadSheetId, new());
                _dicDownloadedSheet[sheetData.spreadSheetId].Add(sheetData);
            }
        }

        [InitializeOnLoadMethod]
        private static void RefreshIfNotInitialized()
        {
            if (_isInitialized)
                return;

            RefreshDownloadedSheet();

            _isInitialized = true;
        }
    }
}
