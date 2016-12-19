if ([Environment]::Is64BitOperatingSystem) {
    $pfiles = ${env:PROGRAMFILES(X86)}
} else {
    $pfiles = $env:PROGRAMFILES
}
$msbuild = $pfiles + '\MSBuild\14.0\Bin\MSBuild.exe'
$solution = '.\sln\bmaclient\bmaclient.sln'
$config = '/p:Configuration=Release'
$platform = '/p:Platform="Any CPU"'
$env:errorLevel = 0
$proc = Start-Process $msbuild $solution,$config,$platform -NoNewWindow -PassThru
$proc.WaitForExit()
if ($env:errorLevel -ne 0) {
    echo 'BUILD FAILED'
    exit 1
} else {
    echo 'BUILD SUCCEEDED'
    exit 0
}