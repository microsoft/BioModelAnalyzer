<#
 .SYNOPSIS
    Builds and deploys BMA.

 .DESCRIPTION
    Builds BMA and deploys it to Azure using a free-tier app service plan and a locally-redundant storage account for logging.

 .PARAMETER serviceName
    Name to use as domain for front-end application (so, it will be deployed to <serviceName>.azurewebsites.net)

 .PARAMETER serviceLocation
    Location of azure datacenter to use. Default is "North Europe"

 .PARAMETER resourceGroupName
    The resource group where BMA will be deployed. Can be the name of an existing or a new resource group. If not specified, serviceName will be used instead.

 .PARAMETER deploymentName
    The deployment name. Default is <serviceName>Deployment

 .PARAMETER storageAccountName
    Name of the storage account to use in this deployment. Default is <serviceName>storage.

 .PARAMETER apiServiceName
    Name to use as domain for API sevice. Default is <serviceName>api.

 .PARAMETER servicePlanName
    Name of app service plan to use in this deployment. Default is <serviceName>Plan.
#>

# Copyright (c) Microsoft Research 2016
# License: MIT. See LICENSE

param(
 [Parameter(Mandatory=$True, Position=1)]
 [string]
 $serviceName,

 [string]
 $serviceLocation = "North Europe",

 [string]
 $resourceGroupName,

 [string]
 $deploymentName,

 [string]
 $storageAccountName,

 [string]
 $apiServiceName,

 [string]
 $servicePlanName
)
$ErrorActionPreference = "stop"
$exitcode = 0

if ($serviceName.Length -ge 60) {
    Write-Error -Message 'ERROR: service name is too long (more than 60 characters).'
    exit 1
}
# default values for empty parameters
if ([System.String]::IsNullOrEmpty($resourceGroupName)) {
    $resourceGroupName = $serviceName
    if ($resourceGroupName.Length -gt 60) {
        Write-Error -Message "ERROR: resource group name defaulted to an incorrect value ('$resourceGroupName') - it's longer than 60 characters. Either provide a different service name or specify a correct resource group name explicitly (via -resourceGroupName parameter)."
        exit 1
    }
}
if ([System.String]::IsNullOrEmpty($storageAccountName)) {
    $storageAccountName = $serviceName + "storage"
    if ($storageAccountName -notmatch "^[a-z1-9]{3,24}$") {
        Write-Error -Message "ERROR: storage account name defaulted to an incorrect value ('$storageAccountName') - only lowercase letters and digits are allowed, must be between 3 and 24 characters long. Either provide a different service name or specify a correct storage account name explicitly (via -storageAccountName parameter)."
        exit 1
    }
}
if ([System.String]::IsNullOrEmpty($apiServiceName)) {
    $apiServiceName = $serviceName + "api"
    if ($apiServiceName.Length -gt 60) {
        Write-Error -Message "ERROR: api service name defaulted to an incorrect value ('$apiServiceName') - it's longer than 60 characters. Either provide a different service name or specify a correct api service name explicitly (via -apiServiceName parameter)."
        exit 1
    }
}
if ([System.String]::IsNullOrEmpty($servicePlanName)) {
    $servicePlanName = $serviceName + "Plan"
    if ($servicePlanName.Length -gt 40) {
        Write-Error -Message "ERROR: service plan name defaulted to an incorrect value ('$servicePlanName') - it's longer than 40 characters. Either provide a different service name or specify a correct service plan name explicitly (via -servicePlanName parameter)."
        exit 1
    }
}
if ([System.String]::IsNullOrEmpty($deploymentName)) {
    $deploymentName = $serviceName + "Deployment"
}

if ($storageAccountName -notmatch "^[a-z1-9]{3,24}$") {
    Write-Error -Message 'ERROR: storage account name is incorrect (only lowercase letters and digits are allowed, must be between 3 and 24 characters long).'
    exit 1
}
if ($resourceGroupName.Length -gt 60) {
    Write-Error -Message 'ERROR: resource group name is too long (more than 60 characters).'
    exit 1
}
if ($apiServiceName.Length -gt 60) {
    Write-Error -Message 'ERROR: api service name is too long (more than 60 characters).'
    exit 1
}
if ($servicePlanName.Length -gt 40) {
    Write-Error -Message 'ERROR: service plan name is too long (more than 40 characters).'
    exit 1
}

$cpath = Get-Location
$cdir = $cpath.Path

$resourceGroupLocation = $serviceLocation
$templateFilePath = Join-Path $cdir "deployment\template.json"
$parametersTemplateFilePath = Join-Path $cdir "deployment\parameters.template.json"
$parametersFilePath = Join-Path $cdir "deployment\parameters.json"
Copy-Item $parametersTemplateFilePath $parametersFilePath

.\deployment\installAzurePowerShell.ps1

.\PrepareRepository.ps1

Import-Module .\packages\publish-module\tools\publish-module.psm1


Write-Host "Building web apps..."
.\deployment\buildWebApps.ps1
if (!$?) {
    Write-Error -Message 'ERROR: Build script (deployment\buildWebApps.ps1) ended with an error'
    exit 1
}

Try {
# sign in
Write-Host "Logging in...";
Login-AzureRmAccount;

$subscriptions = Get-AzureRmSubscription

if ($subscriptions.Length -eq 1) {
    $subscriptionId = $subscriptions[0].SubscriptionId
    $subscriptionName = $subscriptions[0].SubscriptionName
} elseif ($subscriptions.Length -eq 0) {
    Write-Error "No Azure subscriptions found."
    exit 1;
} else {
    Write-Host "Multiple subscriptions are found:"
    for ($i = 1; $i -le $subscriptions.Length; ++$i) {
        Write-Host "$i) $($subscriptions[$i-1].SubscriptionName)"
    }
    $ans = Read-Host -Prompt "Type the number corresponding to the subscription you'd like to use"
    $num = 0
    $succ = [System.Int32]::TryParse($ans, [ref] $num)
    while (-not $succ -or $num -lt 1 -or $num -gt $subscriptions.Length) {
        $ans = Read-Host -Prompt "Please, type a number between 1 and $($subscriptions.Length)"
        $succ = [System.Int32]::TryParse($ans, [ref] $num)
    }
    $subscriptionId = $subscriptions[$num - 1].SubscriptionId
    $subscriptionName = $subscriptions[$num - 1].SubscriptionName
}

# select subscription
Write-Host "Selecting subscription '$subscriptionName' with id '$subscriptionId'";
Select-AzureRmSubscription -SubscriptionID $subscriptionId;

# preparing parameters.json
$parameters = Get-Content $parametersFilePath | ConvertFrom-Json
$parameters.parameters.storageAccounts_bmastorage_name.value = $storageAccountName
$parameters.parameters.serverfarms_bmaplan_name.value = $servicePlanName
$parameters.parameters.sites_bmafrontend_name.value = $serviceName
$parameters.parameters.sites_bmaapi_name.value = $apiServiceName
$parameters.parameters.location.value = $serviceLocation
$parameters | ConvertTo-Json | set-content $parametersFilePath

Write-Host "Deploying resource group..."
.\deployment\deployResourceGroup.ps1 $resourceGroupName $resourceGroupLocation $deploymentName $templateFilePath $parametersFilePath
if (!$?) {
    Write-Error -Message 'ERROR: Failed to create a resource group for the deployment'
    exit 1
}

Write-Host "Retrieving storage and publishing credentials..."
$frontendSiteConf = Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $serviceName/publishingcredentials -Action list -ApiVersion 2015-08-01 -Force
$apiSiteConf = Invoke-AzureRmResourceAction -ResourceGroupName $resourceGroupName -ResourceType Microsoft.Web/sites/config -ResourceName $apiServiceName/publishingcredentials -Action list -ApiVersion 2015-08-01 -Force
$storageSiteConf = Get-AzureRmStorageAccountKey -ResourceGroupName $resourceGroupName -AccountName $storageAccountName
$storageKey = $storageSiteConf[0].Value
$connectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageKey"

Write-Host "Configuring ApiServer..."
$unityConfPath = Join-Path $cdir "\deployment\ApiServer\unity.azure-appservice.config"
$unityConf = [xml] (Get-Content $unityConfPath)
$unityConf.unity.container.register[0].constructor.param.value = $connectionString
$unityConf.unity.container.register[1].constructor.param.value = $connectionString
$unityConf.Save($unityConfPath)
$apiConfPath = Join-Path $cdir "\deployment\ApiServer\Web.config"
$apiConf = [xml] (Get-Content $apiConfPath)
$apiConf.configuration.unity.SetAttribute("configSource", "unity.azure-appservice.config")
$apiConf.Save($apiConfPath)

Write-Host "Configuring bma.client..."
$clientConfPath = Join-Path $cdir "\deployment\bma.client\Web.config"
$clientConf = [xml] (Get-Content $clientConfPath)
($clientConf.configuration.appSettings.add | where {$_.key -eq 'BackEndUrl'}).SetAttribute("value", "https://$apiServiceName.azurewebsites.net")
$clientConf.Save($clientConfPath)

Write-Host "Publishing ApiServer..."
$publishProperties = @{'WebPublishMethod' = 'MSDeploy';
                        'MSDeployServiceUrl' = "$apiServiceName.scm.azurewebsites.net:443";
                        'DeployIisAppPath' = $apiServiceName;
                        'Username' = $apiSiteConf.properties.publishingUserName 
                        'Password' = $apiSiteConf.properties.publishingPassword}

Publish-AspNet -packOutput (Join-Path $cdir "deployment\ApiServer") -publishProperties $publishProperties

Write-Host "Publishing bma.client..."
$publishProperties = @{'WebPublishMethod' = 'MSDeploy';
                        'MSDeployServiceUrl' = "$serviceName.scm.azurewebsites.net:443";
                        'DeployIisAppPath' = $serviceName;
                        'Username' = $frontendSiteConf.properties.publishingUserName;
                        'Password' = $frontendSiteConf.properties.publishingPassword}

Publish-AspNet -packOutput (Join-Path $cdir "deployment\bma.client") -publishProperties $publishProperties

}
Catch
{
Write-Host "An error occured during deployment. Error details:"
Write-Host $_
$exitcode = 1
}
Finally
{
Write-Host "Deleting Temporary Files"
Remove-Item (Join-Path $cdir "\deployment\ApiServer") -Recurse
Remove-Item (Join-Path $cdir "\deployment\bma.client") -Recurse
$objpath = Join-Path $cdir "\deployment\obj"
if (Test-Path $objpath) {
    Remove-Item $objpath -Recurse
}
exit $exitcode
}
