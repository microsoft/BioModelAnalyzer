Invoke-WebRequest https://github.com/fsprojects/Paket/releases/download/3.31.2/paket.bootstrapper.exe -OutFile .\.paket\paket.bootstrapper.exe
.\.paket\paket.bootstrapper.exe --run install

copy .\src\ApiServer\unity.azure-appservice.template.config .\src\ApiServer\unity.azure-appservice.config
copy .\src\ApiService\ServiceConfiguration.Cloud.template.cscfg .\src\ApiService\ServiceConfiguration.Cloud.cscfg
