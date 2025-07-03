using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public static class GSSL_DownloadedSheet
    {
        // <SpreadSheetId, List<SheetData>>
        private static readonly Dictionary<string, List<SheetData>> _dicDownloadedSheet = new();

        private static bool _isInitialized;

        public static void Reset()
        {
            _isInitialized = false;
            _dicDownloadedSheet.Clear();
        }

        public static List<SheetData> GetAllSheetData()
        {
            RefreshIfNotInitialized();
            
            var allSheetData = new List<SheetData>();
            foreach (var sheetList in _dicDownloadedSheet.Values)
            {
                allSheetData.AddRange(sheetList);
            }
            
            return allSheetData;
        }

        public static List<SheetData> GetSheetData(string spreadSheetId)
        {
            RefreshIfNotInitialized();

            return _dicDownloadedSheet.TryGetValue(spreadSheetId, out var result)
                ? result
                : new();
        }

        public static void AddSheetData(SheetData sheetData)
        {
            _dicDownloadedSheet.TryAdd(sheetData.spreadSheetId, new());
            _dicDownloadedSheet[sheetData.spreadSheetId].Add(sheetData);
        }

        public static void ClearAllSheetData()
        {
            _dicDownloadedSheet.Clear();
        }

        private static void RefreshDownloadedSheet()
        {
            _dicDownloadedSheet.Clear();
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