using System.Collections.Generic;
using System.Linq;
using GoogleSpreadSheetLoader.Setting;
using UnityEngine.Networking;

namespace GoogleSpreadSheetLoader.Download
{
    internal static partial class GSSL_Download
    {
        private static List<RequestInfo> GetRequestInfoList(Dictionary<string, List<string>> dicSpreadSheet)
        {
            var listDownloadInfo = new List<RequestInfo>();

            foreach (var (spreadSheetId, listTitle) in dicSpreadSheet)
            {
                listDownloadInfo.AddRange(listTitle.Select(title => new RequestInfo(spreadSheetId, title)));
            }

            return listDownloadInfo;
        }
    }
    
    public class RequestInfo
    {
        public RequestInfo(string spreadSheetId, string sheetName)
        {
            _spreadSheetId = spreadSheetId;
            _sheetName = sheetName;

            _url = string.Format(GSSL_URL.DownloadSheetUrl, _spreadSheetId, _sheetName,
                GSSL_Setting.SettingData.apiKey);
            _webRequest = UnityWebRequest.Get(_url);
        }

        public string SpreadSheetId => _spreadSheetId;
        private readonly string _spreadSheetId;
        public string SheetName => _sheetName;
        private readonly string _sheetName;
        public string URL => _url;
        private readonly string _url;

        public bool IsDone => _webRequest?.isDone ?? false;
        public string DownloadText => _webRequest?.downloadHandler?.text ?? "";
        
        // 에러 체크 기능 추가
        public bool HasError => _webRequest?.result != UnityWebRequest.Result.Success;
        public string ErrorMessage => _webRequest?.error ?? "";
        public UnityWebRequest.Result Result => _webRequest?.result ?? UnityWebRequest.Result.DataProcessingError;
        
        private readonly UnityWebRequest _webRequest;
        
        public void SendAndGetAsyncOperation()
        {
            _webRequest.SendWebRequest();
        }
        
        public void Dispose()
        {
            _webRequest?.Dispose();
        }
    }
}