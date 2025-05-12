using System;
using System.Collections.Generic;
using System.Linq;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.OneButton;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.GSSL_State;

// ReSharper disable SuggestVarOrType_DeconstructionDeclarations
// ReSharper disable InconsistentNaming

namespace GoogleSpreadSheetLoader.Editor.View
{
    public class IndividualView
    {
        private enum eGenerateState
        {
            None,
            Progress,
            Complete,
        }

        private readonly Dictionary<string, string> dicSpreadSheetName = new(); // SpreadSheetId => Name
        private readonly Dictionary<string, Dictionary<string, SheetData>> dicSheetDataGroup = new(); // SpreadSheetId =>  SheetTitle => SheetData
        private readonly Dictionary<SheetData, bool> dicSheetCheck = new();
        private readonly eGenerateState _generateScriptState = eGenerateState.None;
        private readonly string _generateScriptMessage = "";
        private readonly eGenerateState _generateDataState = eGenerateState.None;
        private readonly string _generateDataMessage = "";
        
        private DateTime _checkTime;
        private Vector2 _generateScrollPos = Vector2.zero;

        public void DrawGenerateView()
        {
            CheckAndReset();

            DrawTableDataList();

            DrawGenerateButtons();
        }

        private void CheckAndReset()
        {
            if ((DateTime.Now - _checkTime).TotalSeconds > 1)
            {
                _checkTime = DateTime.Now;
                
                var listData = GSSL_DownloadedSheet.GetAllSheetData();

                if (listData != null) ResetDict();
                else ClearDict();
            }
        }

        private void DrawTableDataList()
        {
            EditorGUILayout.Separator();

            if (dicSheetCheck.Count == 0)
            {
                EditorGUILayout.LabelField("  다운로드된 시트 데이터가 하나도 없음.", EditorStyles.whiteLargeLabel, GUILayout.Height(20));
                return;
            }

            EditorGUILayout.LabelField($"  최신화시킬 시트들 선택", EditorStyles.whiteLargeLabel, GUILayout.Height(20));

            EditorGUILayout.Separator();

            var boxStyle = new GUIStyle(GUI.skin.box);
            _generateScrollPos = EditorGUILayout.BeginScrollView(_generateScrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginVertical(boxStyle);
            GUILayout.Space(20);

            int spreadSheetCounter = 0; // 라벨 표기용
            foreach (var (spreadSheetId, dicSheetData) in dicSheetDataGroup)
            {
                spreadSheetCounter++;
                var spreadSheetName = dicSpreadSheetName[spreadSheetId];

                bool prevAllCheck = dicSheetData.Values.All(x => dicSheetCheck[x]);
                bool afterAllCheck = EditorGUILayout.ToggleLeft($"  {spreadSheetCounter}. {spreadSheetName}", prevAllCheck, EditorStyles.boldLabel);

                if (afterAllCheck != prevAllCheck)
                {
                    foreach (var sheetData in dicSheetData.Values)
                    {
                        dicSheetCheck[sheetData] = afterAllCheck;
                    }
                }

                foreach (var (sheetName, sheetData) in dicSheetData)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(20);
                    dicSheetCheck[sheetData] = EditorGUILayout.ToggleLeft(sheetName, dicSheetCheck[sheetData]);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Space(20);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void DrawGenerateButtons()
        {
            var isGenerateAble = dicSheetCheck?.Any(x => x.Value) ?? false;
            
            if (!isGenerateAble)
                return;
            
            if(CurrState == eGSSL_State.None)
            {
                GUILayout.FlexibleSpace();
            
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("다운로드 & 변환", GUILayout.Width(200)))
                {
                    var list = dicSheetCheck
                        .Where(x => x.Value)
                        .Select(x => new RequestInfo(x.Key.spreadSheetId, x.Key.title))
                        .ToList();

                    GSSL_OneButton.OneButtonProcessSheet(list).GetAwaiter().GetResult();
                }
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(ProgressText, GUILayout.Width(ProgressText.Length * 12));
                EditorGUILayout.Space(30);
                EditorGUILayout.EndHorizontal();

                if (GSSL_Setting.AdvanceMode)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("강제 초기화 (에러 났을때)", GUILayout.Width(180)))
                    {
                        SetProgressState(eGSSL_State.None);
                    }
                    EditorGUILayout.Space(30);
                    EditorGUILayout.EndHorizontal();
                }

                return;
            }
            
            EditorGUILayout.Space(20);
        }

        private void ClearDict()
        {
            dicSpreadSheetName.Clear();
            dicSheetDataGroup.Clear();
            dicSheetCheck.Clear();   
        }
        
        private void ResetDict()
        {
            foreach (var spreadSheetInfo in GSSL_Setting.SettingData.listSpreadSheetInfo)
            {
                var spreadSheetId = spreadSheetInfo.spreadSheetId;
                var spreadSheetName = spreadSheetInfo.spreadSheetName;

                dicSpreadSheetName.TryAdd(spreadSheetId, spreadSheetName);
                
                var listExistingSheetData = GSSL_DownloadedSheet.GetSheetData(spreadSheetId);

                if ((listExistingSheetData?.Count ?? 0) == 0)
                    continue;

                foreach (var sheetData in listExistingSheetData)
                {
                    dicSheetDataGroup.TryAdd(spreadSheetId, new());
                    dicSheetDataGroup[spreadSheetId].TryAdd(sheetData.title, sheetData);
                    dicSheetCheck.TryAdd(sheetData, false);
                }
            }
        }
    }
}