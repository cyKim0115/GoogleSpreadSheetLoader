using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Setting;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace GoogleSpreadSheetLoader.Download
{
    public class GSSL_Download
    {
        public enum eDownloadState
        {
            None,
            Downloading,
            Complete,
        }
        
        private static readonly string _downloadSheetUrl =
            "https://sheets.googleapis.com/v4/spreadsheets/{0}/values/{1}?key={2}";
        private static readonly string _downloadSpreadSheetUrl = "https://sheets.googleapis.com/v4/spreadsheets/{0}?key={1}";
        
        private string _spreadSheetOpenUrl = "https://docs.google.com/spreadsheets/d/{0}/edit?key={1}";

        public static async Awaitable DownloadSpreadSheet(
            Dictionary<int, bool> dicCheck,
            Dictionary<string, List<string>> dicSheetName,
            UnityAction<eDownloadState> OnStateChangeAction,
            UnityAction<string> OnMessageAction)
        {
            // 다운로드 대상 정리
            List<SpreadSheetInfo> listDownloadTarget = new List<SpreadSheetInfo>();

            for (int i = 0; i < GSSL_Setting.SettingData.listSpreadSheetInfo.Count; i++)
            {
                if (!dicCheck[i])
                    continue;

                listDownloadTarget.Add(GSSL_Setting.SettingData.listSpreadSheetInfo[i]);
            }

            OnStateChangeAction?.Invoke(eDownloadState.Downloading);

            // 다운로드
            List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)> listInfoOperPair =
                new List<(SpreadSheetInfo, UnityWebRequestAsyncOperation)>();
            try
            {
                foreach (SpreadSheetInfo info in listDownloadTarget)
                {
                    string url = string.Format(_downloadSpreadSheetUrl, info.spreadSheetId,
                        GSSL_Setting.SettingData.apiKey);

                    UnityWebRequest webRequest = UnityWebRequest.Get(url);

                    UnityWebRequestAsyncOperation asyncOperator = webRequest.SendWebRequest();
                    listInfoOperPair.Add((info, asyncOperator));
                }

                do
                {
                    OnMessageAction?.Invoke(
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})");
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                OnMessageAction?.Invoke("다운로드 완료");
                OnStateChangeAction?.Invoke(eDownloadState.Complete);
                await Task.Delay(1000);
                dicCheck.Clear();
                OnStateChangeAction?.Invoke(eDownloadState.None);
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
                        string titleString = title.ToString();

                        bool isContains = titleString.Contains(GSSL_Setting.SettingData.sheetTargetStr);

                        // 포함 되어있는데 '제외'설정 되어있으면 추가되지 않음.
                        if (isContains && GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.제외)
                            continue;

                        // 포함되어 있지 않은데 '포함'설정 되어있으면 추가되지 않음.
                        if (!isContains && GSSL_Setting.SettingData.sheetTarget == SettingData.eSheetTargetStandard.포함)
                            continue;

                        if (dicSheetName[pair.info.spreadSheetId].Contains(titleString))
                        {
                            UnityEngine.Debug.LogError(
                                $"중복 시트 이름 : {pair.info.spreadSheetName}에서 {titleString}의 중복 이름이 존재!");

                            continue;
                        }

                        dicSheetName[pair.info.spreadSheetId].Add(titleString);
                    }
                }
            }
        }
        
        public static async Awaitable DownloadSheet(
            Dictionary<string, Dictionary<string, bool>> dicCheck,
            UnityAction<eDownloadState> OnStateChangeAction,
            UnityAction<string> OnMessageAction)
        {
            // 다운로드 대상 정리
            List<(string spreadsheetId, string sheetName)> listDownloadTarget =
                new List<(string spreadsheetId, string sheetName)>();

            foreach (KeyValuePair<string, Dictionary<string, bool>> spreadSheetPair in dicCheck)
            {
                string spreadsheetId = spreadSheetPair.Key;

                foreach (KeyValuePair<string, bool> sheetPair in spreadSheetPair.Value)
                {
                    if (!sheetPair.Value)
                        continue;

                    string sheetName = sheetPair.Key;

                    listDownloadTarget.Add((spreadsheetId, sheetName));
                }
            }

            OnStateChangeAction?.Invoke(eDownloadState.Downloading);

            // 다운로드
            List<((string spreadSheetId, string sheetName), UnityWebRequestAsyncOperation oper)> listInfoOperPair = new();
            try
            {
                foreach ((string spreadSheetId, string sheetName) info in listDownloadTarget)
                {
                    string url = string.Format(_downloadSheetUrl, info.spreadSheetId, info.sheetName,
                        GSSL_Setting.SettingData.apiKey);

                    UnityWebRequest webRequest = UnityWebRequest.Get(url);

                    UnityWebRequestAsyncOperation asyncOperator = webRequest.SendWebRequest();
                    listInfoOperPair.Add((info, asyncOperator));
                }

                do
                {
                    OnMessageAction?.Invoke(
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})");
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                OnMessageAction?.Invoke("다운로드 완료");
                OnStateChangeAction?.Invoke(eDownloadState.Complete);
                await Task.Delay(1000);
                dicCheck.Clear();
                OnStateChangeAction?.Invoke(eDownloadState.None);
            }

            if (!Directory.Exists(GSSL_Setting.SettingDataAssetPath))
            {
                Directory.CreateDirectory(GSSL_Setting.SettingDataAssetPath);
            }

            // 다운로드 받은 데이터 정리
            foreach (((string spreadSheetId, string sheetName) info, UnityWebRequestAsyncOperation oper) pair in
                     listInfoOperPair)
            {
                SheetData sheetData = ScriptableObject.CreateInstance<SheetData>();

                sheetData.title = pair.info.sheetName;

                if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_enumTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.EnumType;
                else if (sheetData.title.Contains(GSSL_Setting.SettingData.sheet_localizationTypeStr))
                    sheetData.tableStyle = SheetData.eTableStyle.Localization;
                else
                    sheetData.tableStyle = SheetData.eTableStyle.None;

                JObject jObj = JObject.Parse(pair.oper.webRequest.downloadHandler.text);

                if (!jObj.TryGetValue("values", out var values))
                {
                    Debug.LogError($"변환 실패 - {pair.info.sheetName} \n{pair.oper.webRequest.downloadHandler.text}");

                    continue;
                }

                sheetData.data = values.ToString();

                AssetDatabase.CreateAsset(sheetData, $"{GSSL_Setting.SettingDataAssetPath}/{sheetData.title}.asset");
            }
        }

    }
}