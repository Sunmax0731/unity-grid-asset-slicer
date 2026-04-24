param(
    [string]$UnityPath = "C:\Program Files\Unity\6000.3.13f1\Editor\Unity.exe",
    [string]$ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OutputRoot = "",
    [switch]$SkipUnityPackage,
    [switch]$SkipEditModeTests
)

$ErrorActionPreference = "Stop"

$projectPath = (Resolve-Path $ProjectPath).Path
$packageRoot = Join-Path $projectPath "Packages\com.sunmax0731.grid-asset-slicer"
$packageManifestPath = Join-Path $packageRoot "package.json"
$toolName = "UnityGridAssetSlicer"

if ((-not $SkipUnityPackage -or -not $SkipEditModeTests) -and -not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity executable was not found: $UnityPath"
}

if (-not (Test-Path -LiteralPath $packageManifestPath)) {
    throw "package.json was not found: $packageManifestPath"
}

$packageManifest = Get-Content -LiteralPath $packageManifestPath -Raw -Encoding utf8 | ConvertFrom-Json
$version = $packageManifest.version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Package version could not be read from package.json."
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $projectPath "ReleaseBuilds"
}

$outputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
$stageRoot = Join-Path $outputRoot "$toolName-$version"
$samplesRoot = Join-Path $stageRoot "Samples"
$unityPackagePath = Join-Path $stageRoot "$toolName-$version.unitypackage"
$zipPath = Join-Path $outputRoot "$toolName-$version-release.zip"
$manifestPath = Join-Path $stageRoot "release-manifest.json"
$packageSamplesRoot = Join-Path $packageRoot "Samples~"
$packageDocsRoot = Join-Path $packageRoot "Documentation~"

function Assert-FileExists {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required file was not found: $Path"
    }
}

function Assert-DirectoryHasFiles {
    param(
        [string]$Path,
        [string]$Filter
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "Required directory was not found: $Path"
    }

    $files = @(Get-ChildItem -LiteralPath $Path -Recurse -File -Filter $Filter)
    if ($files.Count -le 0) {
        throw "Required directory did not contain any $Filter files: $Path"
    }
}

function Copy-DirectoryContents {
    param(
        [string]$SourceRoot,
        [string]$DestinationRoot
    )

    New-Item -ItemType Directory -Path $DestinationRoot -Force | Out-Null

    foreach ($sourceFile in Get-ChildItem -LiteralPath $SourceRoot -Recurse -File | Sort-Object FullName) {
        if ($sourceFile.Name.EndsWith(".meta", [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $sourceRootFullPath = [System.IO.Path]::GetFullPath($SourceRoot).TrimEnd('\', '/')
        $relativePath = $sourceFile.FullName.Substring($sourceRootFullPath.Length).TrimStart('\', '/')
        $destinationFile = Join-Path $DestinationRoot $relativePath
        $destinationDirectory = Split-Path -Parent $destinationFile
        if (-not [string]::IsNullOrWhiteSpace($destinationDirectory)) {
            New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        }

        Copy-Item -LiteralPath $sourceFile.FullName -Destination $destinationFile -Force
    }
}

function Get-ZipEntries {
    param([string]$Path)

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
    try {
        return @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    }
    finally {
        $zip.Dispose()
    }
}

$requiredPackageFiles = @(
    (Join-Path $packageRoot "README.md"),
    (Join-Path $packageRoot "CHANGELOG.md"),
    (Join-Path $packageRoot "LICENSE.md"),
    (Join-Path $packageDocsRoot "Manual.ja.md"),
    (Join-Path $packageDocsRoot "Manual.md"),
    (Join-Path $packageDocsRoot "TermsOfUse.md"),
    (Join-Path $packageDocsRoot "ReleaseNotes.md"),
    (Join-Path $packageDocsRoot "ValidationChecklist.md")
)

foreach ($requiredFile in $requiredPackageFiles) {
    Assert-FileExists -Path $requiredFile
}

Assert-DirectoryHasFiles -Path $packageSamplesRoot -Filter "*.png"
Assert-DirectoryHasFiles -Path $packageSamplesRoot -Filter "*.session.json"

if (-not $SkipEditModeTests) {
    $validationScript = Join-Path $projectPath "tools\validation\run-editmode-tests.ps1"
    Assert-FileExists -Path $validationScript

    & $validationScript `
        -UnityPath $UnityPath `
        -ProjectPath $projectPath `
        -ResultsPath (Join-Path $outputRoot "Validation\editmode-results.xml") `
        -LogPath (Join-Path $outputRoot "Validation\editmode-tests.log")
}

if (Test-Path -LiteralPath $stageRoot) {
    Remove-Item -LiteralPath $stageRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $stageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $samplesRoot -Force | Out-Null

if (-not $SkipUnityPackage) {
    $env:UNITY_GRID_ASSET_SLICER_UNITYPACKAGE_OUTPUT = $unityPackagePath
    try {
        $unityPackageLogPath = Join-Path $outputRoot "$toolName-$version-unitypackage.log"
        if (Test-Path -LiteralPath $unityPackageLogPath) {
            Remove-Item -LiteralPath $unityPackageLogPath -Force
        }

        $process = Start-Process -FilePath $UnityPath `
            -WorkingDirectory $projectPath `
            -ArgumentList @(
                "-batchmode",
                "-quit",
                "-nographics",
                "-projectPath", $projectPath,
                "-executeMethod", "Sunmax.GridAssetSlicer.Editor.Release.GridAssetSlicerReleaseBuilder.BuildUnityPackageBatch",
                "-logFile", $unityPackageLogPath
            ) `
            -Wait `
            -PassThru

        if ($process.ExitCode -ne 0) {
            throw "Unity batch build failed with exit code $($process.ExitCode). See $unityPackageLogPath"
        }
    }
    finally {
        Remove-Item Env:UNITY_GRID_ASSET_SLICER_UNITYPACKAGE_OUTPUT -ErrorAction SilentlyContinue
    }

    Assert-FileExists -Path $unityPackagePath
}

Copy-Item -LiteralPath (Join-Path $packageRoot "README.md") -Destination (Join-Path $stageRoot "README.md") -Force
Copy-Item -LiteralPath (Join-Path $packageDocsRoot "Manual.ja.md") -Destination (Join-Path $stageRoot "Manual.ja.md") -Force
Copy-Item -LiteralPath (Join-Path $packageDocsRoot "Manual.md") -Destination (Join-Path $stageRoot "Manual.md") -Force
Copy-Item -LiteralPath (Join-Path $packageDocsRoot "TermsOfUse.md") -Destination (Join-Path $stageRoot "TermsOfUse.md") -Force
Copy-Item -LiteralPath (Join-Path $packageDocsRoot "ReleaseNotes.md") -Destination (Join-Path $stageRoot "ReleaseNotes.md") -Force
Copy-Item -LiteralPath (Join-Path $packageDocsRoot "ValidationChecklist.md") -Destination (Join-Path $stageRoot "ValidationChecklist.md") -Force
Copy-Item -LiteralPath (Join-Path $packageRoot "CHANGELOG.md") -Destination (Join-Path $stageRoot "CHANGELOG.md") -Force
Copy-Item -LiteralPath (Join-Path $packageRoot "LICENSE.md") -Destination (Join-Path $stageRoot "LICENSE.md") -Force
Copy-DirectoryContents -SourceRoot $packageSamplesRoot -DestinationRoot $samplesRoot

$contentEntries = @(
    "README.md",
    "Manual.ja.md",
    "Manual.md",
    "TermsOfUse.md",
    "ReleaseNotes.md",
    "ValidationChecklist.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "Samples/"
)

if (-not $SkipUnityPackage) {
    $contentEntries += "$(Split-Path -Leaf $unityPackagePath)"
}

$manifest = [ordered]@{
    tool = $toolName
    version = $version
    generatedUtc = [System.DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
    unityPackage = if ($SkipUnityPackage) { $null } else { Split-Path -Leaf $unityPackagePath }
    zip = Split-Path -Leaf $zipPath
    contents = $contentEntries
}

$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $manifestPath -Encoding utf8

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $stageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal
Assert-FileExists -Path $zipPath

$zipEntries = Get-ZipEntries -Path $zipPath
$expectedZipEntries = @(
    "README.md",
    "Manual.ja.md",
    "Manual.md",
    "TermsOfUse.md",
    "ReleaseNotes.md",
    "ValidationChecklist.md",
    "CHANGELOG.md",
    "LICENSE.md",
    "release-manifest.json"
)

foreach ($expectedEntry in $expectedZipEntries) {
    if ($zipEntries -notcontains $expectedEntry) {
        throw "Release zip is missing expected entry: $expectedEntry"
    }
}

if (-not ($zipEntries | Where-Object { $_ -like "Samples/*" })) {
    throw "Release zip is missing Samples contents."
}

if (-not $SkipUnityPackage -and $zipEntries -notcontains (Split-Path -Leaf $unityPackagePath)) {
    throw "Release zip is missing unitypackage: $(Split-Path -Leaf $unityPackagePath)"
}

Write-Output "Release stage: $stageRoot"
Write-Output "Release zip: $zipPath"
if (-not $SkipUnityPackage) {
    Write-Output "UnityPackage: $unityPackagePath"
}
