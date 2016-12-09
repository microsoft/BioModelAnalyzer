## Unit testing

### Web App scripts tests

### Back-end tests



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
Can be enabled in Basic plans and higher. 
See Azure Portal, Application Settings, Platform.

## Testing the deployment

### WebAPI tests

### E2E tests
