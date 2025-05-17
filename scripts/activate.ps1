$scriptsDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptFiles = @(
    #e.g. "$scriptsDir/env.ps1"
)

foreach ($script in $scriptFiles) {
    if (Test-Path $script) {
        . $script
    }
    else {
        Write-Error "Script not found: $script"
    }
}

Write-Host "Project profile successfully loaded." 