# Chaîne locale : scènes J0, tests EditMode, build Web. Usage : powershell -File Tools/ci-local.ps1 [-ProjectPath <clone>] [-Dev]
param(
    [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\.."),
    [string]$Unity = "C:\Program Files\Unity\Hub\Editor\6000.3.25f1\Editor\Unity.exe",
    [switch]$Dev,
    [switch]$SkipScenes
)
$logs = Join-Path $ProjectPath "Logs\ci"
New-Item -ItemType Directory -Force $logs | Out-Null

function Run-Unity([string]$name, [string[]]$extra) {
    $log = Join-Path $logs "$name.log"
    $unityArgs = @('-batchmode', '-projectPath', "`"$ProjectPath`"", '-logFile', "`"$log`"") + $extra
    $p = Start-Process $Unity -ArgumentList $unityArgs -PassThru -WindowStyle Hidden
    $null = $p.Handle # garde le handle pour lire le code de sortie
    # -Wait attendrait aussi le client de licence que Unity laisse tourner
    $p.WaitForExit()
    Write-Host "[$name] code $($p.ExitCode)"
    Select-String -Path $log -Pattern "error CS|Exception:|\[Import\]|\[J0\]|\[Build\]|Shader error" | Select-Object -First 20 | ForEach-Object { Write-Host ("  " + $_.Line) }
    return $p.ExitCode
}

if (-not $SkipScenes -and (Run-Unity "scenes" @('-quit', '-executeMethod', 'StreetMythos.Editor.SceneBootstrapper.BuildAll')) -ne 0) { exit 1 }
if (-not $SkipScenes -and (Run-Unity "arena" @('-quit', '-executeMethod', 'StreetMythos.Editor.ArenaBuilder.Build')) -ne 0) { exit 1 }

$results = Join-Path $logs "editmode-results.xml"
$null = Run-Unity "tests" @('-runTests', '-testPlatform', 'EditMode', '-testResults', "`"$results`"")
if (Test-Path $results) {
    [xml]$x = Get-Content $results
    $r = $x.'test-run'
    "[tests] total $($r.total), réussis $($r.passed), échoués $($r.failed)"
    if ([int]$r.failed -gt 0) { exit 1 }
} else { "[tests] pas de résultats"; exit 1 }

$buildArgs = @('-quit', '-executeMethod', 'StreetMythos.Editor.BuildScript.BuildWeb')
if ($Dev) { $buildArgs += '-devBuild' }
if ((Run-Unity "build" $buildArgs) -ne 0) { exit 1 }
Get-Content (Join-Path $ProjectPath "Builds\web-size.txt")
