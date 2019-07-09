param(
    # Actions
    [switch]$build = $false,
    [switch]$test = $false,
    [switch]$package = $false,

    # Settings
    [switch]$ci = $false,
    [string]$config = "Release"
)

$ErrorActionPreference = "Stop"

[string]$rootDir = Join-Path $PSScriptRoot ".."
[string]$toolsDir = Join-Path $rootDir "tools"
[string]$binariesDir = Join-Path $rootDir "binaries"

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

function Test-UnitTests-NetCore() {
    Write-Host "Running .NET Core unit tests"
    $resultsDir = Join-Path $binariesDir "xunitResults"
    Create-Directory $resultsDir

    $all =
        "src\ShaderTools.CodeAnalysis.Tests\ShaderTools.CodeAnalysis.Tests.csproj",
        "src\ShaderTools.CodeAnalysis.Hlsl.Tests\ShaderTools.CodeAnalysis.Hlsl.Tests.csproj",
        "src\ShaderTools.CodeAnalysis.Hlsl.Features.Tests\ShaderTools.CodeAnalysis.Hlsl.Features.Tests.csproj",
        "src\ShaderTools.CodeAnalysis.Hlsl.EditorFeatures.Tests\ShaderTools.CodeAnalysis.Hlsl.EditorFeatures.Tests.csproj",
        "src\ShaderTools.CodeAnalysis.ShaderLab.Tests\ShaderTools.CodeAnalysis.ShaderLab.Tests.csproj"
    $anyFailed = $false
    
    foreach ($filePath in $all) { 
        $filePath = Join-Path $rootDir $filePath
        $arg = "test $filePath --no-build --configuration $config --results-directory $resultsDir"
        try {
            Exec-Console "dotnet" $arg
        }
        catch {
            $anyFailed = $true
        }
    }
    
    if ($anyFailed) {
        throw "Unit tests failed"
    }
}

function Test-UnitTests-NetFramework() { 
    Write-Host "Running .NET Framework unit tests"
    $resultsDir = Join-Path $binariesDir "xunitResults"
    Create-Directory $resultsDir

    $all =
        "src\ShaderTools.Editor.VisualStudio.Tests\bin\Release\ShaderTools.Editor.VisualStudio.Tests.dll"
    $xunit = Join-Path $rootDir "tools\xunit\xunit.console.x86.exe"
    $anyFailed = $false
  
    foreach ($filePath in $all) { 
        $filePath = Join-Path $rootDir $filePath
        $fileName = [IO.Path]::GetFileNameWithoutExtension($filePath)
        $logFilePath = Join-Path $resultsDir "$($fileName).xml"
        $arg = "$filePath -xml $logFilePath"
        try {
            Exec-Console $xunit $arg
        }
        catch {
            $anyFailed = $true
        }
    }
  
    if ($anyFailed) {
      throw "Unit tests failed"
    }
}

function Package-LanguageServer() {
    $runtimes =
        "win10-x64"
        #"osx-x64"

    Write-Host "Packaging Language Server"

    foreach ($runtime in $runtimes) {
        # TODO: Change filename depending on platform
        $outputDir = Join-Path $rootDir "src/ShaderTools.VSCode/bin/$runtime"
        Create-Directory $outputDir
        $outputBinary = "$outputDir/ShaderTools.LanguageServer.exe"
        $args = "warp src/ShaderTools.LanguageServer/ShaderTools.LanguageServer.csproj --output $outputBinary --verbose"
        Exec-Console "dotnet" $args
    }

    $vscePath = Join-Path $binariesDir "vscode"
    Create-Directory $vscePath

    $vsceArgs = "package"
    Exec-Console "vsce" $vsceArgs
}

Push-Location $rootDir
try {
    . "scripts\utils.ps1"

    if ($build) {
        Build-Solution
    }

    if ($test) {
        Test-UnitTests-NetCore
        Test-UnitTests-NetFramework
    }

    if ($package) {
        Package-LanguageServer
    }
} catch {
    Write-TaskError "Error: $($_.Exception.Message)"
    Write-TaskError $_.ScriptStackTrace
    exit 1
} finally {
    Pop-Location
}