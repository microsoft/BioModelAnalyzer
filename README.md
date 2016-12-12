# Build and test

## Unit testing

### Web App scripts tests

### Back-end tests


# Deployment

- Deployment to local IIS/IIS Express or Web Hosting
- Deployment on Azure App Service
- Deployment on Azure App Service and Cloud Service

## Setup OneDrive access

1. Register your instance of the BioModelAnalyzer application as OneDrive application at [http://dev.onedrive.com](http://dev.onedrive.com). 
For this, open ``App Registration`` link at the site and follow instructions.

1. Add Application Id and RedirectUrl to the ``Web.config`` of the project ``bma.client``. By default, 
it should redirect to the ``html/callback.html`` located at the root of the published ``bma.client``. Note 
that it is required that `RedirectUrl` uses `https`.

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
Only need to update the redirect url or add a new one at the 
[My applications](https://apps.dev.microsoft.com) portal.


## Activity and failure logs

**ApiServer** exposes `/api/activitylog` endpoint to gather user activity statistics from BMA client. Browser sends
a message to this endpoint when a user closes the BMA page.  

Also, **ApiServer** stores information about failures during simulations and analysis performed for BMA client.
It saves the failed request and error message so it is possible to reproduce the error case.

Location and format of logs is determined by the unity config files used by **ApiServer** and depend on
deployment type.

* **Azure** deployment (both App Service or Cloud Service) should use Storage Account to store logs.
   
   - Activity logs are stored in the `ClientActivity` table of the given Storage Account.
   - Failure logs are stored in the `ServiceFailures` table of the given Storage Account. 
   Table rows reference blobs of the `failures` container which keep both failed request and response.

* **Web hosting** deployment (icnluding local IIS Express) should use local files to store logs.

  - Activity logs are stored as a CSV file `activity_*date*.csv` in a folder defined in the web.config. 
  - Failure logs are stored as a CSV file `failures_*date*.csv` in a folder defined in the web.config. Along with the file, there is a folder
  `requests` with files keeping the failed requests and referenced from the table.

## Deploy on App Service

Two projects should be deployed:

- **ApiServer** is API App that performs simulation and analysis for BMA.
- **bma.client** is Web App that servers static resources for a client and is an entry point for the browser.

### 1. Set up **ApiServer** configuration:


  * Update `unity.azure-appservice.config` so that 

       - it uses `FailureAzureLogger` and `ActivityAzureLogger` to store failures and activities statistics to
       the given Storage Account. See `Activity and failure logs` section in this document for more details.
       - optionally uses Storage Account for long-running task scheduler. 
       This enables long-running LTL polarity checks to be run programmatically, 
       though it is not supported in the BMA client yet.

```xml
<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
    <container>
      <register type="BMAWebApi.IFailureLogger, BMAWebApi"
              mapTo="BMAWebApi.FailureAzureLogger, BMAWebApi">
        <constructor>
          <param name="connectionString"
                 value="..." />
        </constructor>
      </register>
      
      <register type="BMAWebApi.IActivityLogger, BMAWebApi"
                mapTo="BMAWebApi.ActivityAzureLogger, BMAWebApi">
        <constructor>
          <param name="connectionString"
                 value="..." />
        </constructor>
      </register>
    </container>
</unity>
```       

  * Update `web.config` so that it loads the `unity.azure-appservice.config`.

```xml
<configuration>
  ...
  <unity configSource="unity.azure-appservice.config"/>
  ...
</configuration>
```  
  
### 2. Set up **bma.client**:

  * Edit `web.config` to setup OneDrive access and provide ApiServer address. `BackEndUrl` should be URI
  of the ApiServer, e.g. `https://ossbmaapiserver.azurewebsites.net` or `http://localhost:8223`. 

```xml
    <add key="LiveAppId" value="" /> <!--Live app ID goes here. Get it from the onedrive reg site-->
    <add key="RedirectUrl" value="..." />
    <add key="BackEndUrl" value="..." />
```
See  `Setup OneDrive access` in this document for more details. 

## Choosing the platform acrhitecture (32-bit or 64-bit)
In Visual Studio, change current platform for the solution to either `Any CPU` or `x64`. 
If you deploy it then, this selection must correspond to settings of web hosting or Azure application settings,
otherwise you will get `Internal Server Error` when trying to access the services. This error is caused by
`BadImageFormatException` thrown by the web application.

In Azure App Service, the 64-bit platform can be enabled in Basic plans and higher. 
Open Azure Portal, find and click the deployed web application and then open its `Application Settings` and
check `Platform`.

To run the applications using IIS Express in Visual Studio, open `Tools/Options/Projects and Solutions/Web Projects` 
and check or uncheck the option `Use the 64 bit version of IIS Express for web sites and projects`.




## Testing the deployment

### WebAPI tests

### E2E tests
