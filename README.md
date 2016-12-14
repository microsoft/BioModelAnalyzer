# Build and test

## Unit testing

### Web App scripts tests
 - Download `paket.bootstrapper.exe` into the folder `.paket`. Run `.paket/paket.bootstrapper.exe`. This will download the latest `paket.exe`.
 - Install all of the required packages from the specified sources: 
 
 `$ .paket/paket.exe install`
 
 - Build **bma.package** 
 - To run Web App scripts tests in Visual Studio download and install **Chutzpah Test Adapter for Test Explorer** and **Chutzpah Test Runner Context Menu Extension**.
 - Open solution **bma.client**. In this solution find the project **bma.package**. All Web App scripts tests are in the folder `test`.
 - Click on the file `Chutzpah.json` and run JS tests (`Chutzpah.json` is a test setting file which allows you to specify which files/folders to use as test files).

### Back-end tests


# Deployment

- Deployment to local IIS/IIS Express or Web Hosting
- Deployment on Azure App Service
- Deployment on Azure App Service and Cloud Service

## Setup OneDrive access


## Setup acitivity and failure logs locations

Either use local folder or storage account to store failure and activity logs.


## Deploy on App Service

Two projects should be deployed:

- **ApiServer** is API App that performs simulation and analysis for BMA.
- **bma.client** is Web App that servers static resources for a client and is an entry point for the browser.

1. Set up **ApiServer** configuration:


  * Update `unity.azure.config` so that 

       - it uses Storage Account-based logger.
       - optionally uses Storage Account for long-running task scheduler. This enables long-running LTL polarity checks, but not supported in the BMA client yet.

  * Update `web.config` so that it loads the `unity.azure.config`.
  
2. Set up **bma.client**:

  * Edit `web.config`: 

```xml
    <add key="LiveAppId" value="" /> <!--Live app ID goes here. Get it from the onedrive reg site-->
    <add key="RedirectUrl" value="https://bmainterface.azurewebsites.net/html/callback.html" />   
    <add key="BackEndUrl" value="https://ossbmaapiserver.azurewebsites.net" />
```

### Choosing the platform acrhitecture (32-bit or 64-bit)
The 64-bit platform can be enabled in Basic plans and higher. 
Open Azure Portal, find and click the deployed web application and then open its `Application Settings` and
check `Platform`.



## Testing the deployment

### WebAPI tests

### E2E tests
