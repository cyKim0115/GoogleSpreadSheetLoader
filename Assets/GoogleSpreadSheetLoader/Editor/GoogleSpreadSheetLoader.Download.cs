using System.Collections.Generic;
using System.Linq;
using GoogleSpreadSheetLoader.Setting;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.Download.GSSL_Download;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable ConvertToConstant.Local

namespace GoogleSpreadSheetLoader.Download
{
    public class DownloadView
    {
        private Dictionary<int, bool> _dicDownloadSpreadSheetCheck = new();

        private Dictionary<string, Dictionary<string, bool>> _dicDownloadSheetCheck = new();

        // Key : SpreadSheetId , Value : SheetNames
        private Dictionary<string, List<string>> _dicSheetNames = new();


        private eDownloadState _spreadSheetDownloadState = eDownloadState.None;
        private eDownloadState _sheetDownloadState = eDownloadState.None;
        private string _spreadSheetDownloadMessage = "";
        private string _sheetDownloadMessage = "";
        private Vector2 _sheetDownloadScrollPos = new(0, 0);

        public void DrawDownloadView()
        {
            DrawSpreadSheetList();

            DrawDownloadSpreadSheetBtn();

            DrawSheetInfo();

            DrawDownloadSheetBtn();
        }

        private void DrawSpreadSheetList()
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("  정보를 다운로드할 스프레드 시트 선택", EditorStyles.whiteLargeLabel);
            EditorGUILayout.Separator();

            for (int i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                SpreadSheetInfo _info = GSSL_Setting.SettingData.listSpreadSheetInfo[i];

                EditorGUILayout.BeginHorizontal();

                if (!_dicDownloadSpreadSheetCheck.ContainsKey(i))
                {
                    _dicDownloadSpreadSheetCheck.Add(i, false);
                }

                _dicDownloadSpreadSheetCheck[i] =
                    EditorGUILayout.ToggleLeft("", _dicDownloadSpreadSheetCheck[i], GUILayout.Width(20));
                EditorGUILayout.LabelField($"{i + 1}. {_info.spreadSheetName}", GUILayout.Width(150));
                EditorGUILayout.LabelField(_info.spreadSheetId);

                EditorGUILayout.EndHorizontal();
            }
        }

        private async void DrawDownloadSpreadSheetBtn()
        {
            EditorGUILayout.Separator();

            if (_dicDownloadSpreadSheetCheck.Count == 0)
                return;

            switch (_spreadSheetDownloadState)
            {
                case eDownloadState.None:
                {
                    if (_dicDownloadSpreadSheetCheck.Values.Any(x => x))
                    {
                        if (GUILayout.Button("스프레드 시트 정보 다운로드"))
                        {
                            DownloadSpreadSheet(_dicDownloadSpreadSheetCheck,
                                _dicSheetNames,
                                state => { _spreadSheetDownloadState = state; },
                                message =>
                                {
                                    _spreadSheetDownloadMessage = message;
                                    EditorWindow.focusedWindow.Repaint();
                                });
                        }
                    }
                }
                    break;
                case eDownloadState.Complete:
                case eDownloadState.Downloading:
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(_spreadSheetDownloadMessage);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                    break;
            }
        }

        private void DrawSheetInfo()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("  다운로드 받은 시트 정보들", EditorStyles.whiteLargeLabel);
            _sheetDownloadScrollPos = EditorGUILayout.BeginScrollView(_sheetDownloadScrollPos,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            for (int index = 0; index < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; index++)
            {
                SpreadSheetInfo info = GSSL_Setting.SettingData.listSpreadSheetInfo[index];
                EditorGUILayout.Separator();


                if (!_dicSheetNames.ContainsKey(info.spreadSheetId) || _dicSheetNames[info.spreadSheetId].Count == 0)
                {
                    EditorGUILayout.LabelField($"{index + 1}. {info.spreadSheetName}");
                    EditorGUILayout.LabelField(" 저장된 시트 이름이 없습니다.");

                    continue;
                }
                else
                {
                    _dicDownloadSheetCheck.TryAdd(info.spreadSheetId, new Dictionary<string, bool>());

                    bool isAllCheck = _dicDownloadSheetCheck[info.spreadSheetId].Count > 0 &&
                                      _dicDownloadSheetCheck[info.spreadSheetId].All(x => x.Value);
                    bool select = EditorGUILayout.ToggleLeft($"{index + 1}. {info.spreadSheetName}", isAllCheck);

                    if (isAllCheck != select)
                    {
                        var keys = _dicDownloadSheetCheck[info.spreadSheetId].Keys.ToArray();
                        foreach (string key in keys)
                        {
                            _dicDownloadSheetCheck[info.spreadSheetId][key] = select;
                        }
                    }
                }

                _dicSheetNames.TryAdd(info.spreadSheetId, new List<string>());
                List<string> listTitleNames = _dicSheetNames[info.spreadSheetId];

                foreach (string sheetName in listTitleNames)
                {
                    _dicDownloadSheetCheck[info.spreadSheetId].TryAdd(sheetName, false);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(5));
                    _dicDownloadSheetCheck[info.spreadSheetId][sheetName] = EditorGUILayout.ToggleLeft($"  {sheetName}",
                        _dicDownloadSheetCheck[info.spreadSheetId][sheetName]);
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(50);
        }

        private async void DrawDownloadSheetBtn()
        {
            bool isDownloadable = _dicDownloadSheetCheck.Count > 0 &&
                                  _dicDownloadSheetCheck.Any(x => x.Value.Any(x => x.Value));

            if (!isDownloadable) return;

            GUILayout.FlexibleSpace();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            switch (_sheetDownloadState)
            {
                case eDownloadState.None:
                {
                    if (GUILayout.Button("시트 다운로드", GUILayout.Width(150)))
                    {
                        DownloadSheet(_dicDownloadSheetCheck,
                            state => _sheetDownloadState = state,
                            message =>
                            {
                                _sheetDownloadMessage = message;
                                GSSL_EditorWindow.focusedWindow.Repaint();
                            });
                    }
                }
                    break;
                case eDownloadState.Downloading:
                case eDownloadState.Complete:
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField(_sheetDownloadMessage);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                    break;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(30);
        }
    }
}