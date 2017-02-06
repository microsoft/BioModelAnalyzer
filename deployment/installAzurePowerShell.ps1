# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE
if ((Get-Module -ListAvailable AzureRM* -Refresh).Length -eq 0) {
    # Install the Azure Resource Manager modules from the PowerShell Gallery
    Install-Module AzureRM
}
if ((Get-Module -ListAvailable Azure -Refresh).Length -eq 0) {
    # Install the Azure Service Management module from the PowerShell Gallery
    Install-Module Azure
}
