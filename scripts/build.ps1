param(
    # Actions
    [switch]$build = $false,

    # Settings
    [switch]$ci = $false,
    [string]$config = "Release"
)

$ErrorActionPreference = "Stop"

[string]$rootDir = Join-Path $PSScriptRoot ".."
[string]$toolsDir = Join-Path $rootDir "tools"

# Toggle between human readable messages and Azure Pipelines messages based on 
# our current environment.
# https://docs.microsoft.com/en-us/azure/devops/pipelines/scripts/logging-commands?view=azure-devops&tabs=powershell
function Write-TaskError([string]$message) {
    if ($ci) {
        Write-Host "##vso[task.logissue type=error]$message"
    } else {
        Write-Host $message
    }
}

function Get-MSBuildPath() {
    $vsWhere = Join-Path $toolsDir "vswhere.exe"
    $vsInfo = Exec-Command $vsWhere "-latest -format json -requires Microsoft.Component.MSBuild" | Out-String | ConvertFrom-Json
  
    # use first matching instance
    $vsInfo = $vsInfo[0]
    $vsInstallDir = $vsInfo.installationPath
    $vsMajorVersion = $vsInfo.installationVersion.Split('.')[0]
    $msbuildVersionDir = if ([int]$vsMajorVersion -lt 16) { "$vsMajorVersion.0" } else { "Current" }
    return Join-Path $vsInstallDir "MSBuild\$msbuildVersionDir\Bin\msbuild.exe"
}

function Build-Solution() {
    $msbuild = Get-MSBuildPath
    Write-Host "Using MSBuild from $msbuild"

    Write-Host "Building HlslTools"
    
    $args = "/nologo /restore /v:m /p:Configuration=$config src/ShaderTools.sln"

    Exec-Console $msbuild $args
}

Push-Location $rootDir
try
{
    . "scripts\utils.ps1"

    if ($build)
    {
        Build-Solution
    }
}
catch
{
    Write-TaskError "Error: $($_.Exception.Message)"
    Write-TaskError $_.ScriptStackTrace
    exit 1
}
finally
{
    Pop-Location
}