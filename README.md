# Structure 

### `/sln` - Solutions 

* `bmaclient` is a solution containing 2 web applications:

  * `ApiServer` is a REST API App which performs simulation and analysis. It's documented using Swagger, 
  see `/docs/ApiServer.yaml`.
  * `bma.client` is a web site which exposes static resources such as html, scripts and images.
  
* `bmaclient-lra` is an experimental solution which contains Azure Cloud Service `ApiService`. 
  This is a worker role that performs long-running LTL polarity checks. 
  Note that this feature is unsupported by BioModelAnalyzer web client application.
  Also, this solution requires Microsoft Azure SDK fpr .NET 2.9 to be installed.

### `/src` - Projects

# Build and test

Once after cloning the repository, please **run the powershell script `dl-deps.ps1`**. 

It will download [paket](https://fsprojects.github.io/Paket/index.html) and run it in order to fetch the external dependencies. The rest of building, testing, and deployment processes heavily rely on this first step having been performed. After that code can be built using Visual Studio or msbuild.

Also it will create local file `.\src\ApiServer\unity.azure-appservice.config` with default Azure deployment configuration.
This file is added to `.gitignore` and can contain Azure Storage Account connection strings.

## Unit tests

### BackEndTests

### BmaJobsTests

### WebApiTests

### Web App scripts tests
 - To run Web App scripts tests in Visual Studio download and install **Chutzpah Test Adapter for Test Explorer** and **Chutzpah Test Runner Context Menu Extension**.
 - Open solution **bmaclient**. In this solution find and build the project **bma.package**. All Web App scripts tests are in the folder `test`.
 - Click on the file `Chutzpah.json` and run JS tests (`Chutzpah.json` is a test setting file which allows you to specify which files/folders to use as test files; this option requires **Chutzpah Test Runner Context Menu Extension**).
 - Alternatively you can just run these tests from Test Explorer (requires **Chutzpah Test Adapter for Test Explorer**)

### Back-end tests
 - Download and install **NUnit Test Adapter** extension for Visual Studio
 - Open solution **bmaclient** in Visual Studio
 - Build **BackEndTests** project
 - In Test Explorer run tests from **BackEndTests** project (requires **NUnit Test Adapter**)

# Deployment

- Deployment to local IIS/IIS Express or Web Hosting
- Deployment on Azure App Service
- Deployment on Azure App Service and Cloud Service

## Setup OneDrive access

If you want to enable OneDrive functionality (i.e. allow users to use their OneDrive accounts for model storage), you have to register your deployment with OneDrive and modify client app's `Web.config` file accordingly.
To do so,

* Go to [dev.onedrive.com](https://dev.onedrive.com)
* Go to App Registration
* Register your app for **OneDrive** (not _OneDrive for Business_) by following the corresponding instructions on the page
  * Add Web platform with redirect URI of the form "https://_\<domain>_/html/callback.html", where _\<domain>_ is the domain you're deploying the client app on (e.g. bmainterface.azurewebsites.net)
* Add Application Id and RedirectUrl to the ``Web.config`` of the project ``bma.client``. Note 
that it is required that `RedirectUrl` uses `https`.

```xml
<configuration>
  <appSettings>
    <add key="LiveAppId" value="..." /> <!--Live app ID goes here. Get it from the onedrive reg site-->
    <add key="RedirectUrl" value="https://<domain>/html/callback.html" />
    ...
  </appSettings>
  ...
</configuration>
``` 

There is no need to register a new application if you just want to move to a new domain.
You only need to update the redirect URI (both in the [settings of your OneDrive App](https://apps.dev.microsoft.com) and the `Web.config` file).


## Activity and failure logs

**ApiServer** exposes `/api/activitylog` endpoint to gather user activity statistics from BMA client. Browser sends
a message to this endpoint when a user closes the BMA page.  

Also, **ApiServer** stores information about failures during simulations and analysis performed for BMA client.
It saves the failed request and error message so it is possible to reproduce the error case.

Location and format of logs is determined by the Unity config files used by **ApiServer** and depend on
deployment type. See Unity configuration file samples in following sections.

* **Azure** deployment (both App Service or Cloud Service) should use Storage Account to store logs. To do that 
register `BMAWebApi.ActivityAzureLogger` and `BMAWebApi.FailureAzureLogger` types in the **ApiServer** 
Unity configuration file. You will get:
   
   - Activity logs are stored in the `ClientActivity` table of the given Storage Account.
   - Failure logs are stored in the `ServiceFailures` table of the given Storage Account. 
   Table rows reference blobs of the `failures` container which keep both failed request and response.

* **Web hosting** deployment (icnluding local IIS Express) should use local files to store logs. To do that 
register `BMAWebApi.ActivityFileLogger` and `BMAWebApi.FailureFileLogger` types in the **ApiServer** 
Unity configuration file. You will get:

  - Activity logs are stored as a CSV file `activity_*date*.csv` in a folder defined in the unity configuration file. 
  - Failure logs are stored as a CSV file `failures_*date*.csv` in a folder defined in the unity configuration file. 
  Along with the file, there is a folder
  `requests` with files keeping the failed requests. The CSV table rows reference these files by names.


## Deploy on App Service

Two projects should be deployed:

- **ApiServer** is API App that performs simulation and analysis for BMA.
- **bma.client** is Web App that servers static resources for a client and is an entry point for the browser.

### 1. Set up **ApiServer** configuration:


  * Set up activity and failure logs. Update `unity.azure-appservice.config` so that 

       - it uses `FailureAzureLogger` and `ActivityAzureLogger` to store failures and activities statistics to
       the given Storage Account. See `Activity and failure logs` section in this document for more details.
       - optionally uses Storage Account for long-running task scheduler. 
       This enables long-running LTL polarity checks to be run programmatically, 
       though it is not supported in the BMA client yet.

       **It is highly advised not to commit the configuration files containing Azure connection strings**. 
       Otherwise anyone might get an access to your Azure Storage Account. You should add such files to
       `.gitignore` and keep only locally.
              
       The file `unity.azure-appservice.config` is already added to `.gitignore` and
       can be safely used to store connection strings.       

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

### 3. Publish **ApiServer** and **bma.client** in Azure App Service. 

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
