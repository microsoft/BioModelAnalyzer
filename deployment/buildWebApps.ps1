# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
if ([Environment]::Is64BitOperatingSystem) {
    $pfiles = ${env:PROGRAMFILES(X86)}
} else {
    $pfiles = $env:PROGRAMFILES
}
$msbuild = $pfiles + '\MSBuild\14.0\Bin\MSBuild.exe'
if (!(Test-Path $msbuild)) {
    Write-Error -Message 'ERROR: Failed to locate MSBuild at ' + $msbuild
    exit 1
}
$solution = '.\sln\bmaclient\bmaclient.sln'
if (!(Test-Path $solution)) {
    Write-Error -Message 'ERROR: Failed to locate solution file at ' + $solution
    exit 1
}
$platform = '/p:Platform="x86"'
$config = '/p:Configuration=Release'
$env:errorLevel = 0
$proc = Start-Process $msbuild $solution,$config,$platform,'/t:"ApiServer:Rebuild";"bma_client:Rebuild"','/p:DeployApiServer=true','/p:DeployBmaClient=true','/p:PublishProfile=FileSystemPublishx86' -NoNewWindow -PassThru
$handle = $proc.Handle #workaround for not-working otherwise exit code
$proc.WaitForExit()
if ($env:errorLevel -ne 0 -or $proc.ExitCode -ne 0) {
    Write-Error -Message 'BUILD FAILED'
    exit 1
} else {
    echo 'BUILD SUCCEEDED'
    exit 0
}
