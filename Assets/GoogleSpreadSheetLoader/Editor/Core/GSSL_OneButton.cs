using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using GoogleSpreadSheetLoader.Download;
using GoogleSpreadSheetLoader.Generate;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.SheetData;
using static GoogleSpreadSheetLoader.GSSL_State;

namespace GoogleSpreadSheetLoader.OneButton
{
    public static class GSSL_OneButton
    {
        private static readonly string GenerateDataPrefsKey = "GenerateData";
        private static bool GenerateDataFlag => EditorPrefs.HasKey(GenerateDataPrefsKey);
        private static string GenerateDataString
        {
            get => EditorPrefs.HasKey(GenerateDataPrefsKey) ? EditorPrefs.GetString(GenerateDataPrefsKey) : string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                    EditorPrefs.DeleteKey(GenerateDataPrefsKey);
                else
                    EditorPrefs.SetString(GenerateDataPrefsKey, value);
            }
        }
        
        // 취소 관련 필드들
        private static CancellationTokenSource _cancellationTokenSource;
        public static bool IsProcessRunning => _cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested;

        public static bool TableLinkerFlag
        {
            get => EditorPrefs.HasKey("TableLinkerLink");
            set
            {
                if (value)
                {
                    EditorPrefs.SetString("TableLinkerLink", true.ToString());
                }
                else
                {
                    EditorPrefs.DeleteKey("TableLinkerLink");
                }
            }
        }

        /// <summary>
        /// 현재 진행중인 프로세스를 취소합니다.
        /// </summary>
        public static void CancelCurrentProcess()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                GSSL_Log.Log("프로세스 취소가 요청되었습니다.");
            }
            
            // 상태를 None으로 설정
            SetProgressState(eGSSL_State.None);
        }

        public static async Awaitable OneButtonProcessSpreadSheet(bool isClearGeneratedFolder = true)
        {
            // 이전 작업이 진행중이면 취소
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            
            // 새로운 취소 토큰 생성
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                if (isClearGeneratedFolder)
                {
                    GSSL_Path.ClearGeneratedFolder();
                }
                
                GSSL_Log.Log("Download SpreadSheet Start");
                var listDownloadInfo = await GSSL_Download.DownloadSpreadSheetAll(_cancellationTokenSource.Token);
                GSSL_Log.Log("Download SpreadSheet Done");

                GSSL_Log.Log("Download Sheet Start");
                await OneButtonProcessSheet(listDownloadInfo, _cancellationTokenSource.Token);
                GSSL_Log.Log("Download Sheet Done");
            }
            catch (OperationCanceledException)
            {
                GSSL_Log.Log("프로세스가 취소되었습니다.");
                SetProgressState(eGSSL_State.None);
            }
            catch (Exception ex)
            {
                GSSL_Log.LogError($"프로세스 중 에러가 발생했습니다: {ex.Message}");
                SetProgressState(eGSSL_State.None);
            }
            finally
            {
                // 리소스 정리
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        internal static async Awaitable OneButtonProcessSheet(List<RequestInfo> listRequestInfo, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await GSSL_Download.DownloadSheet(listRequestInfo, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var listSheetData = GSSL_DownloadedSheet.GetAllSheetData()
                .Where(x => listRequestInfo.Any(downloadInfo => downloadInfo.SheetName == x.title));

            var dicSheetData = new Dictionary<eTableStyle, List<SheetData>>();

            dicSheetData.TryAdd(eTableStyle.EnumType, new());
            dicSheetData.TryAdd(eTableStyle.Common, new());
            dicSheetData.TryAdd(eTableStyle.Localization, new());

            foreach (var sheetData in listSheetData)
            {
                dicSheetData[sheetData.tableStyle].Add(sheetData);
            }

            cancellationToken.ThrowIfCancellationRequested();

            SetProgressState(eGSSL_State.GenerateTableScript);
            foreach ((eTableStyle tableStyle, var list) in dicSheetData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                switch (tableStyle)
                {
                    case eTableStyle.Common:
                        GSSL_Generate.GenerateTableScripts(list);
                        break;
                    case eTableStyle.EnumType:
                        GSSL_Generate.GenerateEnumDef(list);
                        break;
                    case eTableStyle.Localization:
                        GSSL_Generate.GenerateLocalize(list);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            dicSheetData.Remove(eTableStyle.EnumType);
            dicSheetData.Remove(eTableStyle.Localization);
            var str = JsonConvert.SerializeObject(dicSheetData);
            GenerateDataString = str;
            TableLinkerFlag = true;

            cancellationToken.ThrowIfCancellationRequested();

            GSSL_Generate.GenerateTableLinkerScript();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CheckPrefsAndGenerateTableData();
        }

        private static async Awaitable GenerateTableLinkerAsync()
        {
            await Task.Delay(100);

            GSSL_Log.Log("Generate Table Start");
            GSSL_Generate.GenerateTableLinkerData();
            GSSL_Log.Log("Generate Table Done");
        }

        [InitializeOnLoadMethod]
        private static void CheckPrefsAndGenerateTableData()
        {
            GSSL_Log.Log($"Generate Data Check ({GenerateDataFlag})");

            if (GenerateDataFlag)
            {
                SetProgressState(eGSSL_State.GenerateTableData);

                var str = GenerateDataString;

                GSSL_Log.Log("Generate Data Start");
                GenerateData(str);
                GSSL_Log.Log("Generate Data Done");

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GSSL_Log.Log($"Generate TableLinker Check ({TableLinkerFlag})");
            if (TableLinkerFlag)
            {
                SetProgressState(eGSSL_State.GenerateTableLinker);

                GenerateTableLinkerAsync().GetAwaiter().GetResult();

                TableLinkerFlag = false;
            }

            GenerateDataString = string.Empty;

            Task.Run(async () =>
            {
                SetProgressState(eGSSL_State.Done);
                await Task.Delay(500);
                SetProgressState(eGSSL_State.None);
            });
        }

        private static void GenerateData(string str)
        {
            var dic = JsonConvert.DeserializeObject<Dictionary<eTableStyle, List<SheetData>>>(str);

            foreach ((eTableStyle tableStyle, var list) in dic)
            {
                switch (tableStyle)
                {
                    case eTableStyle.Common:
                        GSSL_Generate.GenerateTableData(list);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}