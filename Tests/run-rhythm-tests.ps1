$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourceFiles = @(
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEnums.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEvents.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameConfig.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Rhythm\RhythmManager.cs"),
    (Join-Path $PSScriptRoot "Core\UnityEngineStubs.cs"),
    (Join-Path $PSScriptRoot "Rhythm\RhythmManagerTests.cs")
)

foreach ($sourceFile in $sourceFiles) {
    if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) {
        throw "Required test source file not found: $sourceFile"
    }
}

Add-Type -Path $sourceFiles -ErrorAction Stop

try {
    $rhythmTests = [RhythmManagerTests]::RunAll()
    Write-Output "PASS: $rhythmTests rhythm tests completed successfully."
}
catch {
    Write-Error "Rhythm tests failed: $($_.Exception.Message)"
    exit 1
}
