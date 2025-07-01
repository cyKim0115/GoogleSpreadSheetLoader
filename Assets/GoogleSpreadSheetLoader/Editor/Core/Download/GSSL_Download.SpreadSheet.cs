using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Setting;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static GoogleSpreadSheetLoader.GSSL_State;

// ReSharper disable PossibleNullReferenceException

namespace GoogleSpreadSheetLoader.Download
{
    internal partial class GSSL_Download
    {
        internal static async Awaitable DownloadSpreadSheetOnly()
        {
            var dicSheetName = new Dictionary<string, List<string>>();

            var listDownloadTarget = GSSL_Setting.SettingData.listSpreadSheetInfo;

            SetProgressState(eGSSL_State.Prepare);
            EditorWindow.focusedWindow?.Repaint();

            var listInfoOperPair = new List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)>();

            listInfoOperPair.AddRange(from info in listDownloadTarget
                                      let url = string.Format(GSSL_URL.DownloadSpreadSheetUrl, info.spreadSheetId, GSSL_Setting.SettingData.apiKey)
                                      let webRequest = UnityWebRequest.Get(url)
                                      let asyncOperator = webRequest.SendWebRequest()
                                      select (info, asyncOperator));

            do
            {
                string progressString = $"({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                SetProgressState(eGSSL_State.DownloadingSpreadSheet, progressString);
                EditorWindow.focusedWindow?.Repaint();
                await Task.Delay(100);
            } while (listInfoOperPair.Any(x => !x.oper.isDone));

            SetProgressState(eGSSL_State.Done);
            EditorWindow.focusedWindow?.Repaint();
            await Task.Delay(1000);
            SetProgressState(eGSSL_State.None);
            EditorWindow.focusedWindow?.Repaint();

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
                            case true when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.제외:
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

            await DownloadSheet(GetRequestInfoList(dicSheetName));
        }

        public static async Awaitable<List<RequestInfo>> DownloadSpreadSheetAll()
        {
            var listDownloadTarget = GSSL_Setting.SettingData.listSpreadSheetInfo;
            List<RequestInfo> listResult = new();

            SetProgressState(eGSSL_State.Prepare);
            EditorWindow.focusedWindow?.Repaint();

            var listInfoOperPair = new List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)>();

            listInfoOperPair.AddRange(from info in listDownloadTarget
                                      let url = string.Format(GSSL_URL.DownloadSpreadSheetUrl, info.spreadSheetId, GSSL_Setting.SettingData.apiKey)
                                      let webRequest = UnityWebRequest.Get(url)
                                      let asyncOperator = webRequest.SendWebRequest()
                                      select (info, asyncOperator));

            do
            {
                string progressString = $"({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                SetProgressState(eGSSL_State.DownloadingSpreadSheet, progressString);
                EditorWindow.focusedWindow?.Repaint();
                await Task.Delay(100);
            } while (listInfoOperPair.Any(x => !x.oper.isDone));
            
            {
                string progressString = $"(Done)";
                SetProgressState(eGSSL_State.DownloadingSpreadSheet, progressString);
                EditorWindow.focusedWindow?.Repaint();
                await Task.Delay(500);
            }

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
                            case true when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.제외:
                            case false when GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.포함:
                                continue;
                        }

                        if (listResult.Any(x => x.SheetName == titleString))
                        {
                            Debug.LogError(
                                $"중복 시트 이름 : {pair.info.spreadSheetName}에서 {titleString}의 중복 이름이 존재!");

                            continue;
                        }

                        listResult.Add(new RequestInfo(pair.info.spreadSheetId, titleString));
                    }
                }
            }

            return listResult;
        }
    }
}