# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
if ([Environment]::Is64BitOperatingSystem) {
    $pfiles = ${env:PROGRAMFILES(X86)}
    $platform = '/p:Platform="x64"'
} else {
    $pfiles = $env:PROGRAMFILES
    $platform = '/p:Platform="x86"'
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
$fsharpdll = $pfiles + '\Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.1.0\FSharp.Core.dll'
if (!(Test-Path $fsharpdll)) {
    Write-Warning ('WARNING: ' + $fsharpdll + ' is missing. Please, ensure, that F# tools are present on your machine.')
}
if (!(Test-Path '.\paket-files')) {
    echo 'paket-files folder was not found, running repository preparation script...'
    .\PrepareRepository.ps1
    if (!$?) {
        Write-Error -Message 'ERROR: Repository preparation script (PrepareRepository.ps1) ended with an error'
        exit 1
    }
}
$config = '/p:Configuration=Release'
$env:errorLevel = 0
$proc = Start-Process $msbuild $solution,$config,$platform,'/t:Rebuild' -NoNewWindow -PassThru
$handle = $proc.Handle #workaround for not-working otherwise exit code
$proc.WaitForExit()
if ($env:errorLevel -ne 0 -or $proc.ExitCode -ne 0) {
    Write-Error -Message 'BUILD FAILED'
    exit 1
} else {
    echo 'BUILD SUCCEEDED'
    exit 0
}