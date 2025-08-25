using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleSpreadSheetLoader.Setting;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using static GoogleSpreadSheetLoader.GSSL_State;

namespace GoogleSpreadSheetLoader.Download
{
    internal static partial class GSSL_Download
    {
        private const int MaxRetryAttempts = 2; // 전체 재시도 횟수
        
        public static async Awaitable DownloadSheet(List<RequestInfo> listDownloadInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                for (int attempt = 0; attempt <= MaxRetryAttempts; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (attempt > 0)
                    {
                        Debug.LogWarning($"전체 다운로드 재시도 중... ({attempt}/{MaxRetryAttempts})");
                        await Task.Delay(2000, cancellationToken); // 재시도 간격
                    }
                    
                    if (await TryDownloadSheet(listDownloadInfo, cancellationToken))
                    {
                        // 성공하면 루프 종료
                        break;
                    }
                    
                    if (attempt == MaxRetryAttempts)
                    {
                        // 마지막 시도까지 실패한 경우
                        var errorInfos = listDownloadInfo.Where(x => x.HasError && !x.IsDisposed).ToList();
                        var errorMessage = $"시트 다운로드 실패 (최대 재시도 횟수 초과):\n";
                        foreach (var errorInfo in errorInfos)
                        {
                            errorMessage += $"• {errorInfo.SheetName}: {errorInfo.ErrorMessage} (재시도: {errorInfo.RetryCount}회)\n";
                        }
                        
                        Debug.LogError(errorMessage);
                        throw new System.Exception($"시트 다운로드 실패: {errorInfos.Count}개 시트에서 에러 발생 (최대 재시도 횟수 초과)");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("시트 다운로드가 취소되었습니다.");
                throw; // 취소 예외는 다시 던져서 상위에서 처리하도록 함
            }
            finally
            {
                // 리소스 정리
                foreach (var info in listDownloadInfo)
                {
                    info.Dispose();
                }
            }
        }
        
        private static async Awaitable<bool> TryDownloadSheet(List<RequestInfo> listDownloadInfo, CancellationToken cancellationToken = default)
        {
            // 실패한 요청들만 재시도
            var failedRequests = listDownloadInfo.Where(x => x.HasError && x.CanRetry).ToList();
            
            if (failedRequests.Any())
            {
                Debug.LogWarning($"실패한 시트 {failedRequests.Count}개 재시도 중...");
                
                foreach (var request in failedRequests)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await request.RetryAsync(cancellationToken);
                }
                
                // 재시도된 요청들이 완료될 때까지 대기
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    string progressString = $"({listDownloadInfo.Count(x => x.IsDone)}/{listDownloadInfo.Count})";
                    SetProgressState(eGSSL_State.DownloadingSheet, progressString + " (재시도 중)");
                    EditorWindow.focusedWindow?.Repaint();
                    await Task.Delay(100, cancellationToken);
                } while (listDownloadInfo.Any(x => !x.IsDone && !x.IsDisposed));
            }
            else
            {
                // 모든 요청 시작
                foreach (var info in listDownloadInfo)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if (!info.IsDisposed)
                    {
                        info.SendAndGetAsyncOperation();
                    }
                }

                var totalCount = listDownloadInfo.Count;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    string progressString = $"({listDownloadInfo.Count(x => x.IsDone)}/{totalCount})";
                    SetProgressState(eGSSL_State.DownloadingSheet, progressString);
                    EditorWindow.focusedWindow?.Repaint();
                    await Task.Delay(100, cancellationToken);
                } while (listDownloadInfo.Any(x => !x.IsDone && !x.IsDisposed));
            }

            {
                string progressString = $"(Done)";
                SetProgressState(eGSSL_State.DownloadingSheet, progressString);
                EditorWindow.focusedWindow?.Repaint();
                await Task.Delay(500, cancellationToken);
            }

            // 다운로드 완료 후 에러 체크
            var errorInfos = listDownloadInfo.Where(x => x.HasError && !x.IsDisposed).ToList();
            if (errorInfos.Any())
            {
                var errorMessage = "시트 다운로드 중 에러가 발생했습니다:\n";
                foreach (var errorInfo in errorInfos)
                {
                    errorMessage += $"• {errorInfo.SheetName}: {errorInfo.ErrorMessage} (재시도: {errorInfo.RetryCount}회)\n";
                }
                
                Debug.LogError(errorMessage);
                return false; // 실패
            }

            GSSL_DownloadedSheet.ClearAllSheetData();

            foreach (var info in listDownloadInfo)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (info.IsDisposed) continue;
                
                try
                {
                    SheetData sheetData = new SheetData();
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

                    GSSL_DownloadedSheet.AddSheetData(sheetData);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"시트 데이터 처리 중 에러 발생 - {info.SheetName}: {ex.Message}");
                    return false; // 실패
                }
            }
            
            return true; // 성공
        }
    }
}