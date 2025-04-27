using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleSpreadSheetLoader
{
    [Serializable]
    public class SettingData : ScriptableObject
    {
        public enum eSheetTargetStandard
        {
            포함,
            제외,
        }
        
        [Space(5)]
        [SerializeField] public string apiKey = "API Key Here";
        [SerializeField] public List<SpreadSheetInfo> listSpreadSheetInfo = new List<SpreadSheetInfo>();
        
        [Space(5)]
        [SerializeField] public eSheetTargetStandard sheetTarget = eSheetTargetStandard.제외;
        [SerializeField] public string sheetTargetStr = "#";
        
        [Space(5)]
        [SerializeField] public string sheet_enumTypeStr = "EnumDef";
        [SerializeField] public string sheet_localizationTypeStr = "Localization";
        
        [Space(5)]
        [SerializeField] public bool advanceMode = false;
    }

    [Serializable]
    public class SpreadSheetInfo
    {
        [SerializeField] public string spreadSheetName;
        [SerializeField] public string spreadSheetId;
    }
}