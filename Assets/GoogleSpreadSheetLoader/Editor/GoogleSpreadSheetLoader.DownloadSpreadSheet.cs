using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleSpreadSheetLoader
{
    public partial class GoogleSpreadSheetLoaderWindow
    {
        private readonly string _downloadSpreadSheetUrl = "https://sheets.googleapis.com/v4/spreadsheets/{0}?key={1}";

        private async Awaitable DownloadSpreadSheet()
        {
            // 다운로드 대상 정리
            List<SpreadSheetInfo> listDownloadTarget = new List<SpreadSheetInfo>();

            for (int i = 0; i < _settingData.listSpreadSheetInfo.Count; i++)
            {
                if (!_dicDownloadSpreadSheetCheck[i])
                    continue;

                listDownloadTarget.Add(_settingData.listSpreadSheetInfo[i]);
            }

            _spreadSheetDownloadState = eDownloadState.Downloading;

            // 다운로드
            List<(SpreadSheetInfo info, UnityWebRequestAsyncOperation oper)> listInfoOperPair =
                new List<(SpreadSheetInfo, UnityWebRequestAsyncOperation)>();
            try
            {
                foreach (SpreadSheetInfo info in listDownloadTarget)
                {
                    string url = string.Format(_downloadSpreadSheetUrl, info.spreadSheetId, _settingData.apiKey);

                    UnityWebRequest webRequest = UnityWebRequest.Get(url);

                    UnityWebRequestAsyncOperation asyncOperator = webRequest.SendWebRequest();
                    listInfoOperPair.Add((info, asyncOperator));
                }

                do
                {
                    // Debug.Log($"({listOperator.Count(x=>x.webRequest.isDone)}/{listOperator.Count})");
                    _spreadSheetDownloadMessage =
                        $"다운로드 중 ({listInfoOperPair.Count(x => x.oper.isDone)}/{listInfoOperPair.Count})";
                    Repaint();
                    await Task.Delay(100);
                } while (listInfoOperPair.Any(x => !x.oper.isDone));
            }
            finally
            {
                _spreadSheetDownloadMessage = "다운로드 완료";
                _spreadSheetDownloadState = eDownloadState.Complete;
                await Task.Delay(1000);
                _dicDownloadSpreadSheetCheck.Clear();
                _spreadSheetDownloadState = eDownloadState.None;
            }

            // 다운로드 받은 데이터 정리
            foreach ((SpreadSheetInfo info, UnityWebRequestAsyncOperation oper) pair in listInfoOperPair)
            {
                JObject jObj = JObject.Parse(pair.oper.webRequest.downloadHandler.text);
                if (jObj.TryGetValue("sheets", out var sheetsJToken))
                {
                    _dicSheetNames.TryAdd(pair.info.spreadSheetId, new List<string>());
                    _dicSheetNames[pair.info.spreadSheetId].Clear();
                    
                    IEnumerable<JToken> enumTitle = sheetsJToken.Select(x => x["properties"]["title"]);
                    foreach (JToken title in enumTitle)
                    {
                        string titleString = title.ToString();

                        bool isContains = titleString.Contains(_settingData.sheetTargetStr);

                        // 포함 되어있는데 '제외'설정 되어있으면 추가되지 않음.
                        if (isContains && _settingData.sheetTarget == SettingData.eSheetTargetStandard.제외)
                            continue;

                        // 포함되어 있지 않은데 '포함'설정 되어있으면 추가되지 않음.
                        if (!isContains && _settingData.sheetTarget == SettingData.eSheetTargetStandard.포함)
                            continue;

                        if (_dicSheetNames[pair.info.spreadSheetId].Contains(titleString))
                        {
                            UnityEngine.Debug.LogError($"중복 시트 이름 : {pair.info.spreadSheetName}에서 {titleString}의 중복 이름이 존재!");
                            
                            continue;
                        }
                        
                        _dicSheetNames[pair.info.spreadSheetId].Add(titleString);
                    }
                }
            }
        }
    }
}
