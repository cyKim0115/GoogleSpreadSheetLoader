using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;
using static GoogleSpreadSheetLoader.Script.GSSL_Script;

namespace GoogleSpreadSheetLoader
{
    public partial class GSSL_EditorWindow
    {
        private enum eCreateState
        {
            None,
            Progress,
            Complete,
        }
        
        private DateTime _checkTime;
        private List<SheetData> _listTableData = new List<SheetData>();
        private Dictionary<eTableStyle, Dictionary<SheetData, bool>> _dicTableDataCreateCheck = new Dictionary<eTableStyle, Dictionary<SheetData, bool>>();
        private Vector2 _createScrollPos = Vector2.zero;
        private eCreateState _createScriptState = eCreateState.None;
        private string _createScriptMessage = "";
        private eCreateState _createDataState = eCreateState.None;
        private string _createDataMessage = "";

        public void DrawCreateView()
        {
            CreateView_CheckAndLoad();

            CreateView_DrawTableDataList();

            CreateView_DrawBtns();
        }

        private void CreateView_CheckAndLoad()
        {
            if ((DateTime.Now - _checkTime).TotalSeconds > 5)
            {
                _checkTime = DateTime.Now;

                if (!Directory.Exists(GSSL_Setting.SettingDataAssetPath))
                    return;

                var guids = AssetDatabase.FindAssets("", new[] { GSSL_Setting.SettingDataAssetPath });

                if (guids.Length == 0)
                    return;

                _listTableData.Clear();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    SheetData sheetData = AssetDatabase.LoadAssetAtPath<SheetData>(assetPath);
                    _listTableData.Add(sheetData);
                }
            }
        }

        private void CreateView_DrawTableDataList()
        {
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("  변환시킬 시트들 선택", EditorStyles.whiteLargeLabel);
            
            EditorGUILayout.Separator();

            // 요소 체크
            foreach (SheetData tableData in _listTableData)
            {
                _dicTableDataCreateCheck.TryAdd(tableData.tableStyle, new Dictionary<SheetData, bool>());
                _dicTableDataCreateCheck[tableData.tableStyle].TryAdd(tableData, false);
            }
            
            _createScrollPos = EditorGUILayout.BeginScrollView(_createScrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            foreach (KeyValuePair<eTableStyle,Dictionary<SheetData,bool>> categoryPair in _dicTableDataCreateCheck)
            {
                eTableStyle currCategory = categoryPair.Key;
                Dictionary<SheetData, bool> currDic = categoryPair.Value;

                string categoryName = "";
                switch (currCategory)
                {
                    case eTableStyle.None: categoryName = "일반"; break;
                    case eTableStyle.EnumType: categoryName = "Enum"; break;
                    case eTableStyle.Localization: categoryName = "Localization"; break;
                }
                
                // 전체 체크 부분
                bool isAllCehck = currDic.Count > 0 && currDic.All(x => x.Value);
                bool selected = EditorGUILayout.ToggleLeft($" {categoryName}", isAllCehck);

                if (selected != isAllCehck)
                {
                    SheetData[] keys = currDic.Keys.ToArray();

                    foreach (SheetData tableData in keys)
                    {
                        currDic[tableData] = selected;
                    }
                }

                // 요소들
                foreach (SheetData tableData in _listTableData)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                    currDic[tableData] =
                        EditorGUILayout.ToggleLeft(tableData.title, currDic[tableData]);
                    EditorGUILayout.EndHorizontal();
                }

            }
            
            EditorGUILayout.EndScrollView();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void CreateView_DrawBtns()
        {
            bool isCreateable = _dicTableDataCreateCheck.Count > 0 &&
                                _dicTableDataCreateCheck.Values.Any(x => x.Values.Any(y => y));

            if (!isCreateable)
                return;

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            switch (_createScriptState)
            {
                case eCreateState.None:
                    if (GUILayout.Button("스크립트 생성", GUILayout.Width(150)))
                    {
                        CreateScript();
                    }

                    break;
                case eCreateState.Progress:
                case eCreateState.Complete:
                {
                    EditorGUILayout.LabelField(_createScriptMessage,GUILayout.Width(150));
                }
                    break;
            }
            
            switch (_createDataState)
            {
                case eCreateState.None:
                {
                    if (GUILayout.Button("테이블 데이터 생성", GUILayout.Width(150)))
                    {
                    }
                }
                    break;
                case eCreateState.Progress:
                case eCreateState.Complete:
                {
                    EditorGUILayout.LabelField(_createDataMessage,GUILayout.Width(150));
                }
                    break;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
        }
    }
}