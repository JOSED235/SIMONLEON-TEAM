$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourceFiles = @(
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEnums.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEvents.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameConfig.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\MatchManager.cs"),
    (Join-Path $PSScriptRoot "Core\UnityEngineStubs.cs"),
    (Join-Path $PSScriptRoot "Core\GameEnumsTests.cs"),
    (Join-Path $PSScriptRoot "Core\GameEventsTests.cs"),
    (Join-Path $PSScriptRoot "Core\MatchManagerTests.cs")
)

foreach ($sourceFile in $sourceFiles) {
    if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) {
        throw "Required test source file not found: $sourceFile"
    }
}

Add-Type -Path $sourceFiles -ErrorAction Stop

try {
    $enumTests = [GameEnumsTests]::RunAll()
    $eventTests = [GameEventsTests]::RunAll()
    $matchTests = [MatchManagerTests]::RunAll()
    $total = $enumTests + $eventTests + $matchTests
    Write-Output "PASS: $total core tests completed."
}
catch {
    Write-Error "Core tests failed: $($_.Exception.Message)"
    exit 1
}
