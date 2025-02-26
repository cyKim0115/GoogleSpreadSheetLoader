using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleSpreadSheetLoader.Generate;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;

namespace GoogleSpreadSheetLoader
{
    public class GenerateView
    {
        private enum eGenerateState
        {
            None,
            Progress,
            Complete,
        }

        private DateTime _checkTime;
        private readonly List<SheetData> _listTableData = new();
        private readonly Dictionary<eTableStyle, Dictionary<SheetData, bool>> _dicTableDataGenerateCheck = new();
        private Vector2 _generateScrollPos = Vector2.zero;
        private readonly eGenerateState _generateScriptState = eGenerateState.None;
        private readonly string _generateScriptMessage = "";
        private readonly eGenerateState _generateDataState = eGenerateState.None;
        private readonly string _generateDataMessage = "";

        public void DrawGenerateView()
        {
            CheckAndLoad();

            DrawTableDataList();

            DrawGenerateButtons();
        }
        
        private void CheckAndLoad()
        {
            if ((DateTime.Now - _checkTime).TotalSeconds > 1)
            {
                _checkTime = DateTime.Now;

                if (!Directory.Exists(GSSL_Setting.SettingDataAssetPath))
                    return;

                var guids = AssetDatabase.FindAssets("", new[] { GSSL_Setting.SettingDataAssetPath });

                if (guids.Length == 0)
                {
                    CheckAndClearDictionary();
                    return;
                }

                _listTableData.Clear();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    SheetData sheetData = AssetDatabase.LoadAssetAtPath<SheetData>(assetPath);
                    _listTableData.Add(sheetData);
                }

                CheckAndClearDictionary();
            }
        }

        private void DrawTableDataList()
        {
            EditorGUILayout.Separator();

            if (_listTableData == null || _listTableData.Count == 0)
            {
                EditorGUILayout.LabelField("  다운로드된 시트 데이터가 하나도 없음.", EditorStyles.whiteLargeLabel);
                return;
            }

            EditorGUILayout.LabelField($"  변환시킬 시트들 선택)", EditorStyles.whiteLargeLabel);

            EditorGUILayout.Separator();

            // 요소 체크
            foreach (SheetData tableData in _listTableData)
            {
                _dicTableDataGenerateCheck.TryAdd(tableData.tableStyle, new Dictionary<SheetData, bool>());
                _dicTableDataGenerateCheck[tableData.tableStyle].TryAdd(tableData, false);
            }

            _generateScrollPos = EditorGUILayout.BeginScrollView(_generateScrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            foreach (KeyValuePair<eTableStyle, Dictionary<SheetData, bool>> categoryPair in _dicTableDataGenerateCheck)
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
        private void DrawGenerateButtons()
        {
            bool isGenerateable = _dicTableDataGenerateCheck.Count > 0 &&
                                  _dicTableDataGenerateCheck.Values.Any(x => x.Values.Any(y => y));

            if (!isGenerateable)
                return;

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            switch (_generateScriptState)
            {
                case eGenerateState.None:
                    if (GUILayout.Button("스크립트 생성", GUILayout.Width(150)))
                    {
                        var list = new List<SheetData>();
                        var enumTarget = _dicTableDataGenerateCheck[eTableStyle.None].Where(x => x.Value);

                        foreach (var pair in enumTarget)
                        {
                            list.Add(pair.Key);
                        }

                        GSSL_Generate.GenerateTableScripts(list);
                    }

                    break;
                case eGenerateState.Progress:
                case eGenerateState.Complete:
                {
                    EditorGUILayout.LabelField(_generateScriptMessage, GUILayout.Width(150));
                }
                    break;
            }

            switch (_generateDataState)
            {
                case eGenerateState.None:
                {
                    if (GUILayout.Button("테이블 데이터 생성", GUILayout.Width(150)))
                    {
                        var enumTarget = _dicTableDataGenerateCheck[eTableStyle.None].Where(x => x.Value);
                        var listSheet = new List<SheetData>();
                        foreach (var pair in enumTarget)
                        {
                            listSheet.Add(pair.Key);
                        }

                        GSSL_Generate.GenerateTableData(listSheet);
                    }
                }
                    break;
                case eGenerateState.Progress:
                case eGenerateState.Complete:
                {
                    EditorGUILayout.LabelField(_generateDataMessage, GUILayout.Width(150));
                }
                    break;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);
        }

        private void CheckAndClearDictionary()
        {
            if (_listTableData.Any(x => x == null))
            {
                _listTableData.Clear();
                _dicTableDataGenerateCheck.Clear();
            }
        }
    }
}