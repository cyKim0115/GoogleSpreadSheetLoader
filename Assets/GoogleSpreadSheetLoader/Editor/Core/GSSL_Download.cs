using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Setting;
using GoogleSpreadSheetLoader.Simple;
using NUnit.Framework;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException
#pragma warning disable CS0414 // 필드가 대입되었으나 값이 사용되지 않습니다

namespace GoogleSpreadSheetLoader.Download
{
    internal class GSSL_Download
    {
        public enum eDownloadState
        {
            None,
            Downloading,
            Complete,
        }

        public static List<GSSL_DownloadInfo> GetDownloadInfoList(Dictionary<string, Dictionary<string, bool>> sheetCheck)
        {
            var listDownloadInfo = new List<GSSL_DownloadInfo>();

            foreach (var (spreadSheetId, dicSheet) in sheetCheck)
            {
                listDownloadInfo.AddRange(from pair in dicSheet where pair.Value select new GSSL_DownloadInfo(spreadSheetId, pair.Key));
            }

            return listDownloadInfo;
        }

        public static List<GSSL_DownloadInfo> GetDownloadInfoList(Dictionary<string, List<string>> dicSpreadSheet)
        {
            var listDownloadInfo = new List<GSSL_DownloadInfo>();

            foreach (var (spreadSheetId, listTitle) in dicSpreadSheet)
            {
                listDownloadInfo.AddRange(listTitle.Select(title => new GSSL_DownloadInfo(spreadSheetId, title)));
            }

            return listDownloadInfo;
        }

        public static List<GSSL_DownloadInfo> GetAllSpreadSheet()
        {
            var listDownloadInfo = new List<GSSL_DownloadInfo>();

            listDownloadInfo.AddRange(from info in GSSL_Setting.SettingData.listSpreadSheetInfo
                                      select new GSSL_DownloadInfo(info.spreadSheetId, info.spreadSheetName));

            return listDownloadInfo;
        }

        #region 개별

        public static async Awaitable DownloadSpreadSheet()
        {
            var dicSheetName = new Dictionary<string, List<string>>();

            // 다운로드 대상 정리
            var listDownloadTarget = GSSL_Setting.SettingData.listSpreadSheetInfo;

            SimpleView.spreadSheetDownloadState = eDownloadState.Downloading;
            EditorWindow.focusedWindow.Repaint();

            // 다운로드
            var listInfoOperPair = new List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)>();
            try
            {
                listInfoOperPair.AddRange(from info in listDownloadTarget
                                          let url = string.Format(GSSL_URL.DownloadSpreadSheetUrl, info.spreadSheetId, GSSL_Setting.SettingData.apiKey)
                                          let webRequest = UnityWebRequest.Get(url)
                                          let asyncOperator = webRequest.SendWebRequest()
                                          select (info, asyncOperator));

                do
                {
                    SimpleView.spreadSheetDownloadMessage =
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                    EditorWindow.focusedWindow.Repaint();
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                SimpleView.spreadSheetDownloadMessage = "다운로드 완료";
                SimpleView.spreadSheetDownloadState = eDownloadState.Complete;
                EditorWindow.focusedWindow.Repaint();
                await Task.Delay(1000);
                SimpleView.spreadSheetDownloadMessage = "";
                SimpleView.spreadSheetDownloadState = (eDownloadState.None);
                EditorWindow.focusedWindow.Repaint();
            }

            // 다운로드 받은 데이터 정리
            foreach ((SpreadSheetInfo info, UnityWebRequestAsyncOperation oper) pair in listInfoOperPair)
            {
                JObject jObj = JObject.Parse(pair.oper.webRequest.downloadHandler.text);
                if (jObj.TryGetValue("sheets", out var sheetsJToken))
                {
                    dicSheetName.TryAdd(pair.info.spreadSheetId, new List<string>());
                    dicSheetName[pair.info.spreadSheetId].Clear();

                    IEnumerable<JToken> enumTitle = sheetsJToken.Select(x => x["properties"]["title"]);
                    foreach (JToken title in enumTitle)
                    {
                        var titleString = title.ToString();

                        var isContains = titleString.Contains(GSSL_Setting.SettingData.sheetTargetStr);

                        switch (isContains)
                        {
                            // 포함 되어있는데 '제외'설정 되어있으면 추가되지 않음.
                            case true when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.제외:
                            // 포함되어 있지 않은데 '포함'설정 되어있으면 추가되지 않음.
                            case false when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.포함:
                                continue;
                        }

                        if (dicSheetName[pair.info.spreadSheetId].Contains(titleString))
                        {
                            Debug.LogError(
                                $"중복 시트 이름 : {pair.info.spreadSheetName}에서 {titleString}의 중복 이름이 존재!");

                            continue;
                        }

                        dicSheetName[pair.info.spreadSheetId].Add(titleString);
                    }
                }
            }

            DownloadSheet(GetDownloadInfoList(dicSheetName));
        }

        public static async Awaitable<List<GSSL_DownloadInfo>> DownloadSpreadSheetAll()
        {
            // 다운로드 대상 정리
            var listDownloadTarget = GSSL_Setting.SettingData.listSpreadSheetInfo;
            List<GSSL_DownloadInfo> listResult = new();

            SimpleView.spreadSheetDownloadState = eDownloadState.Downloading;
            EditorWindow.focusedWindow.Repaint();

            // 다운로드
            var listInfoOperPair = new List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)>();
            try
            {
                listInfoOperPair.AddRange(from info in listDownloadTarget
                                          let url = string.Format(GSSL_URL.DownloadSpreadSheetUrl, info.spreadSheetId, GSSL_Setting.SettingData.apiKey)
                                          let webRequest = UnityWebRequest.Get(url)
                                          let asyncOperator = webRequest.SendWebRequest()
                                          select (info, asyncOperator));

                do
                {
                    SimpleView.spreadSheetDownloadMessage =
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                    EditorWindow.focusedWindow.Repaint();
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                SimpleView.spreadSheetDownloadMessage = "다운로드 완료";
                SimpleView.spreadSheetDownloadState = eDownloadState.Complete;
                EditorWindow.focusedWindow.Repaint();
                await Task.Delay(1000);
                SimpleView.spreadSheetDownloadMessage = "";
                SimpleView.spreadSheetDownloadState = (eDownloadState.None);
                EditorWindow.focusedWindow.Repaint();
            }

            // 다운로드 받은 데이터 정리
            foreach ((SpreadSheetInfo info, UnityWebRequestAsyncOperation oper) pair in listInfoOperPair)
            {
                JObject jObj = JObject.Parse(pair.oper.webRequest.downloadHandler.text);
                if (jObj.TryGetValue("sheets", out var sheetsJToken))
                {
                    IEnumerable<JToken> enumTitle = sheetsJToken.Select(x => x["properties"]["title"]);
                    foreach (JToken title in enumTitle)
                    {
                        var titleString = title.ToString();

                        var isContains = titleString.Contains(GSSL_Setting.SettingData.sheetTargetStr);

                        switch (isContains)
                        {
                            // 포함 되어있는데 '제외'설정 되어있으면 추가되지 않음.
                            case true when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.제외:
                            // 포함되어 있지 않은데 '포함'설정 되어있으면 추가되지 않음.
                            case false when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.포함:
                                continue;
                        }

                        if (listResult.Any(x => x.SheetName == titleString))
                        {
                            Debug.LogError(
                                $"중복 시트 이름 : {pair.info.spreadSheetName}에서 {titleString}의 중복 이름이 존재!");

                            continue;
                        }

                        listResult.Add(new GSSL_DownloadInfo(pair.info.spreadSheetId, titleString));
                    }
                }
            }

            return listResult;
        }

        public static async Awaitable DownloadSheet(List<GSSL_DownloadInfo> listDownloadInfo)
        {
            // 다운로드 대상 정리
            SimpleView.sheetDownloadState = eDownloadState.Downloading;

            // 다운로드
            try
            {
                foreach (var info in listDownloadInfo)
                {
                    info.SendAndGetAsyncOperation();
                }

                var totalCount = listDownloadInfo.Count;
                do
                {
                    await Task.Delay(100);
                    SimpleView.sheetDownloadMessage = $"다운로드 중 ({listDownloadInfo.Count(x => x.IsDone)}/{totalCount})";
                    EditorWindow.focusedWindow.Repaint();
                } while (listDownloadInfo.Any(x => !x.IsDone));
            }
            finally
            {
                SimpleView.sheetDownloadMessage = "다운로드 완료";
                SimpleView.sheetDownloadState = eDownloadState.Complete;
                EditorWindow.focusedWindow.Repaint();
                await Task.Delay(1000);
                SimpleView.sheetDownloadMessage = "";
                SimpleView.sheetDownloadState = eDownloadState.None;
                EditorWindow.focusedWindow.Repaint();
            }

            var sheetDataAssetPath = GSSL_Path.GetPath(ePath.SheetData);

            // 다운로드 받은 데이터 정리
            foreach (var info in listDownloadInfo)
            {
                SheetData sheetData = ScriptableObject.CreateInstance<SheetData>();
                sheetData.spreadSheetId = info.SpreadSheetId;
                sheetData.title = info.SheetName;

                if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_enumTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.EnumType;
                else if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_localizationTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.Localization;
                else
                    sheetData.tableStyle = SheetData.eTableStyle.Common;

                JObject jObj = JObject.Parse(info.DownloadText);

                if (!jObj.TryGetValue("values", out var values))
                {
                    Debug.LogError($"변환 실패 - {info.SheetName}\n"
                                   + $"{info.URL}\n"
                                   + $"{info.DownloadText}");

                    continue;
                }

                sheetData.data = values.ToString();

                AssetDatabase.CreateAsset(sheetData, $"{sheetDataAssetPath}/{sheetData.title}.asset");
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            GSSL_DownloadedSheet.Reset();
        }

        #endregion
    }
}