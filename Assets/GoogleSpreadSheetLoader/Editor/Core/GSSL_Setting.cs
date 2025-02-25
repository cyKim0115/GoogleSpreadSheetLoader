using UnityEditor;
using UnityEngine;

namespace GoogleSpreadSheetLoader.Setting
{
    public class GSSL_Setting
    {
        #region Setting
        public static string SettingDataPath => _settingDataPath;
        public static SettingData SettingData => _settingData; 
        public static readonly string SettingDataAssetPath = "Assets/GoogleSpreadSheetLoader/Generated/SheetData";
        
        
        private static string _settingDataPath = $"Assets/GoogleSpreadSheetLoader/SettingData.asset";
        private static SettingData _settingData;
        #endregion
        
        [InitializeOnLoadMethod]
        private static void ResetStaticInstance()
        {
            _settingData = null;
        }
        
        public static bool CheckAndCreate()
        {
            if (_settingData == null)
            {
                // 없으면 파일 생성
                if (!AssetDatabase.AssetPathExists(_settingDataPath))
                {
                    SettingData obj = ScriptableObject.CreateInstance<SettingData>();
                    obj = new SettingData();

                    AssetDatabase.CreateAsset(obj, _settingDataPath);
                }

                _settingData =
                    AssetDatabase.LoadAssetAtPath<SettingData>(_settingDataPath);
            }

            return _settingData != null;
        }
    }
}