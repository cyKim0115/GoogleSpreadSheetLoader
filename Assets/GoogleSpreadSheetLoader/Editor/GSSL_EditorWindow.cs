using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    public partial class GSSL_EditorWindow : EditorWindow
    {
        private readonly SettingView _settingView = new();
        private readonly DownloadView _downloadView = new();
        
        private static int _selectedToolbar = 0;

        [MenuItem("Tools/Google Spread Sheet Loader")]
        public static void ShowWindow()
        {
            var window = GetWindow<GSSL_EditorWindow>(true, "Google Spread Sheet Loader");
            var editorWindow = (window as EditorWindow);
            editorWindow.minSize = new Vector2(530, 600);

            _selectedToolbar = LoadSelectedToolbarNum();
            
            window.ShowUtility();
        }

        private void OnGUI()
        {
            int prevSelected = _selectedToolbar;
            _selectedToolbar = GUILayout.Toolbar(_selectedToolbar, new[] { "Settings", "Download", "Create" });
            
            // 기존 번호랑 다르면 세이브
            if (prevSelected != _selectedToolbar)
                SaveSelectedToolbarNum();

            if (!GSSL_Setting.CheckAndCreate())
                return;
            
            switch (_selectedToolbar)
            {
                case 0:
                    _settingView.DrawSettingView();
                    break;
                case 1:
                    _downloadView.DrawDownloadView();
                    break;
                case 2:
                    DrawCreateView();
                    break;
            }
        }

        private static int LoadSelectedToolbarNum()
        {
            return EditorPrefs.GetInt("GSSL_SelectToolbarNum", 0);
        }
        private static void SaveSelectedToolbarNum()
        {
            EditorPrefs.SetInt("GSSL_SelectToolbarNum", _selectedToolbar);
        }
    }
}