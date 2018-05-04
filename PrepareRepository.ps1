# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
mkdir -Force .paket
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest https://github.com/fsprojects/Paket/releases/download/3.31.2/paket.bootstrapper.exe -OutFile .\.paket\paket.bootstrapper.exe
if (!$?) {
    Write-Error -Message 'ERROR: Failed to download paket bootstrapper.'
    exit 1
}
.\.paket\paket.bootstrapper.exe --run install
if (!$?) {
    Write-Error -Message 'ERROR: Failed install dependencies with paket.'
    exit 1
}

if (-not (Test-Path .\src\ApiServer\unity.azure-appservice.config)) {
    copy .\src\ApiServer\unity.azure-appservice.template.config .\src\ApiServer\unity.azure-appservice.config
}
if (-not (Test-Path .\src\ApiService\ServiceConfiguration.Cloud.cscfg)) {
    copy .\src\ApiService\ServiceConfiguration.Cloud.template.cscfg .\src\ApiService\ServiceConfiguration.Cloud.cscfg
}
