$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$sourceFiles = @(
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEnums.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameEvents.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Core\GameConfig.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Rhythm\RhythmManager.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Rhythm\NoteObject.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Rhythm\NoteSpawner.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Rhythm\PlayerInput.cs"),
    (Join-Path $repositoryRoot "Party Potato\Assets\Scripts\Gameplay\AIController.cs"),
    (Join-Path $PSScriptRoot "Core\UnityEngineStubs.cs"),
    (Join-Path $PSScriptRoot "Rhythm\RhythmManagerTests.cs"),
    (Join-Path $PSScriptRoot "Rhythm\NoteAndInputTests.cs"),
    (Join-Path $PSScriptRoot "Rhythm\AIControllerTests.cs")
)

foreach ($sourceFile in $sourceFiles) {
    if (-not (Test-Path -LiteralPath $sourceFile -PathType Leaf)) {
        throw "Required test source file not found: $sourceFile"
    }
}

Add-Type -Path $sourceFiles -ErrorAction Stop

try {
    $rmTests = [RhythmManagerTests]::RunAll()
    $noteTests = [NoteAndInputTests]::RunAll()
    $aiTests = [AIControllerTests]::RunAll()
    $total = $rmTests + $noteTests + $aiTests
    Write-Output "PASS: $total rhythm, input and AI tests completed successfully."
}
catch {
    Write-Error "Rhythm and input tests failed: $($_.Exception.Message)"
    exit 1
}
