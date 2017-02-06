# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
.\PrepareRepository.ps1
if (!$?) {
    Write-Error -Message 'ERROR: Repository preparation script (PrepareRepository.ps1) ended with an error'
    exit 1
}
.\build.ps1
if (!$?) {
    Write-Error -Message 'ERROR: Build script (build.ps1) ended with an error'
    exit 1
}
.\run.ps1