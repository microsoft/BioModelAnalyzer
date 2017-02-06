# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
if ([Environment]::Is64BitOperatingSystem) {
    $selfhostdir = '.\src\bma.selfhost\bin\x64\Release\'
} else {
    $selfhostdir = '.\src\bma.selfhost\bin\Release\'
}
$selfhost = $selfhostdir + 'bma.selfhost.exe'
if (!(Test-Path $selfhost)) {
    echo ($selfhost + ' not found, running build.ps1')
    .\build.ps1
    if (!$?) {
        Write-Error -Message 'ERROR: Build script (build.ps1) ended with an error'
        exit 1
    }
}
$proc = Start-Process $selfhost '-b' -WorkingDirectory $selfhostdir -NoNewWindow -PassThru
$handle = $proc.Handle #workaround for not-working otherwise exit code
$proc.WaitForExit()
if ($proc.ExitCode -eq 0) {
    Start-Process 'http://localhost:8224/'
} else {
    Write-Error -Message 'ERROR: Failed to start bma.selfhost'
    exit 1
}