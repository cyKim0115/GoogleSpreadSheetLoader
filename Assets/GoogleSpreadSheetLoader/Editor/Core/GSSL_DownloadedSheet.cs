using System.Collections.Generic;
using UnityEditor;

namespace GoogleSpreadSheetLoader.Download
{
    public static class GSSL_DownloadedSheet
    {
        private static readonly Dictionary<string, Dictionary<string, SheetData>> _dicDownloadedSheet = new();

        private static bool _isInitialized;

        private static void RefreshDownloadedSheet()
        {
            _dicDownloadedSheet.Clear();
        }
        
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            if (_isInitialized)
                return;

            RefreshDownloadedSheet();

            _isInitialized = true;
        }
    }
}
