# Build and test

## Unit testing

### Web App scripts tests

### Back-end tests


# Deployment

- Deployment to local IIS/IIS Express or Web Hosting
- Deployment on Azure App Service
- Deployment on Azure App Service and Cloud Service

## Setup OneDrive access

1. Register your instance of the BioModelAnalyzer application as OneDrive application at http://dev.onedrive.com. Open 
`` App Registration`` link and follow instructions.

1. Add Application Id and RedirectUrl to the ``Web.config`` of the project ``bma.client``. By default, 
you should redirect to the ``html/callback.html`` located at the root of the published ``bma.client``.

```xml
<configuration>
  <appSettings>
    <add key="LiveAppId" value="..." /> <!--Live app ID goes here. Get it from the onedrive reg site-->
    <add key="RedirectUrl" value="https://.../html/callback.html" />
    ...
  </appSettings>
  ...
</configuration>
``` 

There is no need to register a new application if you change to a new deployment site. 
Only need to update the redirect url (or add a new one at the http://dev.onedrive.com).


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
