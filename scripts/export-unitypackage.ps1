<#
.SYNOPSIS
  Unity batchmode로 루트 GoogleSpreadSheetLoader.unitypackage를 생성합니다.

.DESCRIPTION
  대상 프로젝트를 Editor로 연 상태에서는 실행하지 마세요 (Library 경합).
  배포 시 에디터를 끈 뒤 이 스크립트를 사용합니다.

.PARAMETER UnityExe
  Unity.exe 절대 경로. 생략 시 ProjectSettings/ProjectVersion.txt + Hub 기본 경로에서 탐색.

.PARAMETER Commit
  생성 후 unitypackage를 스테이징하고 한글 커밋 메시지로 커밋.

.PARAMETER Push
  커밋 후 현재 브랜치를 git push (Commit과 함께 쓰는 것을 권장).

.EXAMPLE
  .\scripts\export-unitypackage.ps1

.EXAMPLE
  .\scripts\export-unitypackage.ps1 -Commit -Push
#>
[CmdletBinding()]
param(
    [string] $UnityExe = "",
    [switch] $Commit,
    [switch] $Push
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $RepoRoot

function Get-UnityVersionFromProject {
    $versionFile = Join-Path $RepoRoot "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) {
        throw "ProjectVersion.txt not found: $versionFile"
    }
    $line = Get-Content $versionFile | Where-Object { $_ -match '^m_EditorVersion:' } | Select-Object -First 1
    if (-not $line) { throw "m_EditorVersion not found in ProjectVersion.txt" }
    return ($line -replace '^m_EditorVersion:\s*', '').Trim()
}

function Resolve-UnityExe([string] $Version) {
    if ($UnityExe -and (Test-Path $UnityExe)) {
        return (Resolve-Path $UnityExe).Path
    }
    if ($env:UNITY_EDITOR) {
        $fromEnv = $env:UNITY_EDITOR.Trim('"')
        if (Test-Path $fromEnv) { return (Resolve-Path $fromEnv).Path }
    }

    $candidates = @(
        "C:\Program Files\Unity\Hub\Editor\$Version\Editor\Unity.exe",
        "${env:ProgramFiles}\Unity\Hub\Editor\$Version\Editor\Unity.exe"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { return $c }
    }

    throw @"
Unity $Version 을(를) 찾지 못했습니다.
- Unity Hub에 해당 버전을 설치하거나
- -UnityExe 'C:\...\Unity.exe' 또는 `$env:UNITY_EDITOR` 로 경로를 지정하세요.
"@
}

$version = Get-UnityVersionFromProject
$unity = Resolve-UnityExe $version
$logDir = Join-Path $RepoRoot "Logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logFile = Join-Path $logDir "gssl-export-unitypackage.log"
$resultFile = Join-Path $logDir "gssl-export-unitypackage-result.json"
$packagePath = Join-Path $RepoRoot "GoogleSpreadSheetLoader.unitypackage"
$method = "GoogleSpreadSheetLoader.Export.GSSL_ExportUnityPackage.Export"

Write-Host "Unity:  $unity"
Write-Host "Version: $version"
Write-Host "Method: $method"
Write-Host "Log:    $logFile"

if (Test-Path $resultFile) { Remove-Item -Force $resultFile }

$unityArgs = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", "$RepoRoot",
    "-executeMethod", $method,
    "-logFile", "$logFile"
)

$proc = Start-Process -FilePath $unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
$exitCode = $proc.ExitCode

if ($exitCode -ne 0) {
    Write-Host "Unity exit code: $exitCode" -ForegroundColor Red
    if (Test-Path $logFile) {
        Write-Host "---- log tail ----" -ForegroundColor Yellow
        Get-Content $logFile -Tail 80
    }
    exit $exitCode
}

if (-not (Test-Path $packagePath)) {
    Write-Host "Package file missing after export: $packagePath" -ForegroundColor Red
    if (Test-Path $logFile) { Get-Content $logFile -Tail 80 }
    exit 1
}

$size = (Get-Item $packagePath).Length
Write-Host "OK: $packagePath ($size bytes)" -ForegroundColor Green
if (Test-Path $resultFile) {
    Write-Host "Result: $resultFile"
    Get-Content $resultFile
}

if ($Push -and -not $Commit) {
    Write-Host "-Push 는 -Commit 과 함께 사용하세요." -ForegroundColor Yellow
}

if ($Commit) {
    git add -- "GoogleSpreadSheetLoader.unitypackage"
    $status = git status --porcelain -- "GoogleSpreadSheetLoader.unitypackage"
    if (-not $status) {
        Write-Host "unitypackage 내용 변화 없음 — 커밋 생략." -ForegroundColor Yellow
    }
    else {
        git commit -m @"
리소스 - GoogleSpreadSheetLoader.unitypackage 배치 Export 갱신
"@
        Write-Host "Committed." -ForegroundColor Green
        if ($Push) {
            git push origin HEAD
            Write-Host "Pushed." -ForegroundColor Green
        }
    }
}

exit 0
