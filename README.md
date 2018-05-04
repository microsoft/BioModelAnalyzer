The BioModelAnalyzer is a tool that allows biologists to easily and quickly build complex models 
of biological behaviour, and to analyse them using techniques derived from the field of formal 
verification. Its backend is written in F#, and its graphical frontend is an HTML5 application. 
It uses the SMT solver Z3.

This consists of 
*	An HTML5 user interface designed for rapid model construction and analysis, and
corresponding REST API application performing analysis and simulation (solution `bmaclient`). 
*	A command line tool for access to a wide range of analysis algorithms (solution `BioCheckConsole`)
*	A command line hybrid physical/executable simulator (solution `Athene`), 
as used in [“Emergent stem cell homeostasis in the C. elegans germline is revealed by hybrid modeling”](https://dx.doi.org/10.1016/j.bpj.2015.06.007).
*	A chat bot intended for user education in linear temporal logic (folder `ChatBot`)
*	A set of related tools for formally verifying biological models

The goal of the project is to provide access to biologists to powerful, newly developed algorithms 
without requiring expertise in the underlying computer science. This is achieved by bespoke user 
interfaces and novel methods of interaction. The aims of the project are to increase the range of 
modelling and analysis approaches made available through the tool, and to extend the interface 
to increase the ease of user adoption. 

The user interface is considered as production quality, whilst all other tools are regarded as 
prototypes in different stages of readiness.

The three goals on the project roadmap are to add more advanced library and comparison functions 
to the user interface, to expand the range of concurrency types available in the tool, and 
to add support for alternative model formats. This are intended to be addressed over the next 
2-3 years.

Contributions are welcome! Bugs or feature requests should be reported to the team, 
whilst code contributions should follow the instructions in `CONTRIBUTING.md`.

# Contents

- [Structure of repository](#structure-of-repository)
- [Build and test](#build-and-test)
    - [Build requirements](#build-requirements)
    - [How to Build](#how-to-build)
    - [How to Test and Validate](#how-to-test-and-validate)
- [Run and deploy](#run-and-deploy)
    - [Local run using IIS Express](#local-run-using-iis-express)
    - [Self-hosting application using OWIN](#self-hosting-application-using-owin)
    - [Deployment in Azure App Services](#deployment-in-azure-app-services)
    - [Choosing the platform architecture (32-bit or 64-bit)](#choosing-the-platform-architecture-32-bit-or-64-bit)
    - [Activity and failure logs](#activity-and-failure-logs)
- [How to release](#how-to-release)

# Structure of repository

Powershell scripts in the root of repository ([PowerShell 5.0](https://www.microsoft.com/en-us/download/details.aspx?id=50395) or above is required):

* `PrepareRepository.ps1` prepares freshly cloned repository for the first use. See [Build and test](#build-and-test).
* `build.ps1` builds `bmaclient` solution mentioned below.
* `run.ps1` starts BioModelAnalyzer (both API and client web applications) on local machine using [OWIN](https://www.asp.net/aspnet/overview/owin-and-katana).
 See [Self-hosting application using OWIN](#self-hosting-application-using-owin).
* `BuildAndRun.ps1` consecutively runs `PrepareRepository.ps1`, `build.ps1`, and `run.ps1`
  in order to start the BMA app on local machine right after cloning the repository.
* `DeployAzure.ps1` deploys BMA in Azure. See [Deployment in Azure App Services](#deployment-in-azure-app-services).

`/sln` - Visual Studio solutions:

* `bmaclient` contains 2 web applications:

  * `bma.client` is a web site designed for rapid biological model construction and analysis.
  It uses `ApiServer` to perform simulation and analysis of models.
  * `ApiServer` is a REST API App which performs simulation and analysis of biological models. 
  It is documented using [Swagger](http://editor.swagger.io/), see `/docs/ApiServer.yaml`.  

* `BioCheckConsole` is a command line tool for access to a wide range of analysis algorithms.

* `Athene` is a command line hybrid physical/executable simulator, as used in
[“Emergent stem cell homeostasis in the C. elegans germline is revealed by hybrid modeling”](https://dx.doi.org/10.1016/j.bpj.2015.06.007).

* `ClientStat` is a command line tool that reads user activity statistics from Azure Storage Account,
   writes to CSV files and displays as charts.

* `bmaclient-lra` contains Azure Cloud Service `ApiService`. 
  This is a worker role that performs long-running LTL polarity checks. 
  Note that this feature is yet unsupported by the BioModelAnalyzer web client application.
  This solution requires Microsoft Azure SDK for .NET 2.9 to be installed.

* `fs-scheduler` contains implementation of task scheduler based on Azure Storage Account.
  The scheduler enables fair sharing of computation resources between multiple applications. 
  An Azure Worker is 
  used to perform tasks. This scheduler is used in `bmaclient-lra` to perform long-running operations.

`/ChatBot` - A chat bot intended for user education in linear temporal logic.

`/src` - Visual Studio projects. A project can be shared between multiple solutions.

`/docs` - Project documentation.
 
* `ApiServer.yaml` describing `ApiServer` REST API. It follows [Swagger](http://editor.swagger.io/) 2.0 specs.

* `BMA Deployment Overview.pptx` describes architecture of BioModelAnalyzer backend and frontent.

`/ext/FParsec` contains third party source code of [FParsec](http://www.quanttec.com/fparsec/), a parser combinator library for F#.
BioModelAnalyzer depends on this library.

`/ext/CUDD` contains third party source code of [CUDD](http://vlsi.colorado.edu/~fabio/CUDD/html/), a binary decision diagram library for C++.
Some functionality of BioModelAnalyzer depends on this library.

`/Models` contains biological models that can be imported from the BioModelAnalyzer application.

# Build and test

- **Run the powershell script `PrepareRepository.ps1`** once after cloning the repository.
[PowerShell 5.0](https://www.microsoft.com/en-us/download/details.aspx?id=50395) or above is required.

The rest of building, testing, and deployment processes heavily rely on this first step having been performed. 
The script downloads [paket](https://fsprojects.github.io/Paket/index.html) and runs it in order to fetch the external dependencies. 
Also the script creates local files `/src/ApiServer/unity.azure-appservice.config` and 
`/src/ApiService/ServiceConfiguration.Cloud.cscfg` with default Azure deployment configurations.
These files are configured to be ignored by git and will not be committed to the repository, thus
they may contain Azure Storage Account connection strings.

## Build requirements

1. **Visual Studio 2015/2017.**
If you don't have Visual Studio 2015/2017, you can install the free [Visual Studio 2015/2017 Community](http://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx).
Currently the build process relies on tools that come with Visual Studio 2015/2017 such as
Microsoft .NET Framework 4.5, Microsoft Build Tools, Web Applications Build Targets,
Visual F# Tools, TypeScript compiler and Visual C++.

2. **Visual F# Tools 4.0.**
The Visual F# Tools are installed automatically when you first create or open an F# project in Visual Studio.
If you run the build script without using Visual Studio IDE
you can install it [directly as a separate download](https://www.microsoft.com/en-us/download/details.aspx?id=48179).

3. **TypeScript 1.8.**
Certain versions of Visual Studio can install different versions of TypeScript by default, 
so you need to [install TypeScript 1.8](http://download.microsoft.com/download/6/D/8/6D8381B0-03C1-4BD2-AE65-30FF0A4C62DA/TS1.8.6-TS-release-1.8-nightly.20160229.1/TypeScript_Dev14Full.exe)
from [Microsoft web site](https://www.microsoft.com/en-us/download/details.aspx?id=48593).

## How to Build

After the repository is prepared using the script `PrepareRepository.ps1`,
you can build the solutions using Visual Studio or msbuild.

Also there are helpful build-related scripts located in the root of repository:
 * `build.ps1` builds `bmaclient` solution which contains BioModelAnalyzer Web API and Web client applications.
 * `run.ps1` starts both BioModelAnalyzer Web API and Web client applications on local machine using OWIN.
 * `BuildAndRun.ps1` consecutively runs `PrepareRepository.ps1`, `build.ps1`, and `run.ps1`
  in order to start the BMA applications on local machine.

## How to Test and Validate

### Regression tests

Solution `/sln/bmaclient` contains following test projects:

- `BackEndTests` checks that simulation and analysis of corner cases work correctly. 
It performs same checks twice: by calling appropriate methods directly and 
by sending HTTP requests to self-hosted ApiServer controllers.

- `BmaJobsTests` performs simulations and analysis for requests represented as files in 
`/src/BmaTests.Common/Simulation`, `/src/BmaTests.Common/Analysis`,
`/src/BmaTests.Common/CounterExamples` and `/src/BmaTests.Common/LTLQueries`. Results
are checked against response files in the same folders.

- `bma.package` contains [Jasmine](https://jasmine.github.io/)-based unit tests for `bma.client` code.
All Web App scripts tests are in the folder `test` of project `bma.package`.


### Deployment tests

Solution `/sln/bmaclient` contains test project `WebApiTests` that sends requests
represented as files in 
`/src/BmaTests.Common/Simulation`, `/src/BmaTests.Common/Analysis`,
`/src/BmaTests.Common/CounterExamples` and `/src/BmaTests.Common/LTLQueries`
to the deployed `ApiServer` and checks the responses. 
Server url is configured in the WebApiTests.fs.

This allows to check if the deployed server correctly performs operations.

### End-to-end tests

End-to-end tests for `bma.client` web application are located in `/src/CodedUITests`. 
They are based on [Protractor](http://www.protractortest.org/) framework.


### How to run tests in Visual Studio

- Install following extensions to Visual Studio (menu `Tools/Extensions and Updates`):
  - NUnit Test Adapter 2.0.0 (*NOT NUnit 3 Test Adapter*)
  - Chutzpah Test Adapter for Test Explorer
- Open and build solution `/sln/bmaclient`. 
- Open Test Explorer from menu `Test/Windows` and run the tests. Note that `WebApiTests` require
url of running `ApiServer` to be configured in the WebApiTests.fs; otherwise the tests fail.

Note that running tests for x64 requires setting default processor architecture for tests to x64.
In Visual Studio 2015/2017, use menu `Test/Test Settings/Default Processor Architecture`.

# Run and deploy

Here we describe how to run and deploy BioModelAnalyzer which consists of two web applications: `ApiServer` and `bma.client`.
- Local run using IIS Express.
- Self-hosting standalone application using OWIN.
- Deployment in Azure App Services.

Please see `/docs/BMA Deployment Overview.pptx` for details about BioModelAnalyzer architecture.

In the followings guidelines we will use Visual Studio 2015/2017.
The solution that produces the web applications is located in `/sln/bmaclient`. 
Please make sure that you have run the powershell script `./PrepareRepository.ps1` as described
in the [Build and test](#build-and-test) section to prepare the repository. 


## Local run using IIS Express

This deployment is useful when developing the applications. It allows easily debugging the applications 
using Visual Studio.

### 1. Choose the platform for the solution.

Use Visual Studio interface to select the solution platform, either x86 or x64.

In Visual Studio menu, click `Tools/Options/Projects and Solutions/Web Projects` and check or uncheck the option 
`Use the 64 bit version of IIS Express for web sites and projects`.

### 2. Update activity and failure logs configurations for `APIServer`.

Open `ApiServer/Web.config` and uncomment the unity configuration source `unity.web.config`
to write the activity and failure logs to local CSV files 
or `unity.trace-loggers.config` to write logs to `System.Diagnostics.TraceSource`:

```xml
<unity configSource="unity.web.config"/>
```

OR

```xml
<unity configSource="unity.trace-loggers.config"/>
```

Make sure that only one `<unity />` configuration is uncommented. See more details in
the [Activity and failure logs](#activity-and-failure-logs) section of this document.

### 3. Choose the IIS Express to be used as server.

Open `Properties` for the project `ApiServer` and go to section `Web`.
In the group `Servers`, choose `IIS Express` and check the `Project Url` text which contains
url of the ApiServer application. By default, it is `http://localhost:8223`.

### 4. Set `ApiServer` as start-up project.

In the context menu for the `ApiServer`, click on the `Set as StartUp Project` command. The project name should
become bold in the `Solution Explorer`.

### 5. Run `ApiServer`.

In menu `Debug` click either `Start Debugging` or `Start Without Debugging`.
After the project is built and run, a browser window should open with the project url.
Most browsers normally show `HTTP Error 403.14 - Forbidden` because the root of the site is forbidden
and no default document is configured.

Now the `ApiServer` is available at `http://localhost:8223`. Next we will run `bma.client` web site which
allows using the BioModelAnalyzer HTML application to build and analyze models.

### 6. Update `BackEndUrl` for the `bma.client` application.

We should specify url of the `ApiServer` for the `bma.client` to send user requests, such as model analysis 
and simulation. Edit the `bma.client/Web.config` so the `<appSettings>` contains key `BackEndUrl`:

```xml
  <appSettings>   
    <add key="BackEndUrl" value="http://localhost:8223" />    
  </appSettings> 
```

Note that other two app settings, `LiveAppId` and `RedirectUrl`, are for integration with OneDrive which is unavailable
for local host and thus can be ignored here.

### 7. Choose the IIS Express to be used as server for `bma.client`.

Open `Properties` for the project `bma.client` and go to section `Web`.
In the group `Servers`, choose `IIS Express`.

### 8. Set `bma.client` as start-up project.

In the context menu for the `bma.client`, click on the `Set as StartUp Project` command. The project name should
become bold in the `Solution Explorer`.

### 9. Run `bma.client`.

After the project is built and run, a browser window should open with the project url.
Normally you see the start-up screen with logo and `Launch Tool` button.
Also you can click `Help` link at the top and load one of the examples.

You can run `WebApiTests` for the deployed `ApiServer` to check whether it works correctly;
see section [Deployment tests](#deployment-tests) in this document.

## Self-hosting application using OWIN

BMA can be hosted using
[OWIN](https://www.asp.net/aspnet/overview/owin-and-katana).
This is convenient for development and debugging, and it allows to use BMA standalone.

For those purposes the repository contains `bma.selfhost` application.
It hosts BMA using OWIN making both API and UI available at [http://localhost:8224/](http://localhost:8224/) and it stores logs in
local files (see [Activity and failure logs](#activity-and-failure-logs) for details).

You can start it either by running `run.ps1` powershell script located in the root of repository
(in which case your system's [architecture](#choosing-the-platform-architecture-32-bit-or-64-bit) will be used)
or you can find it within `/sln/bmaclient` solution. The script will also open a browser window with BMA UI
for you as soon as it is available. If you choose not to use the script and to
run the application directly instead, then, in order to access BMA UI you'll have to
open [http://localhost:8224/](http://localhost:8224/) in your browser.

You can run `WebApiTests` with server url set to `http://localhost:8224/` to check if it works correctly;
see  [Deployment tests](#deployment-tests) for details.

## Deployment in Azure App Services

This deployment allows to publish BMA web application for public use. Azure allows scaling the applications
and distributes demand between multiple copies of an application.

## Quick deployment using included script

Included powershell script `DeployAzure.ps1` quickly deploys BMA using a free tier App Service plan to host API and UI and a locally-redundant storage account for logging.
Simply run in powershell console

`.\DeployAzure.ps1 <name>`

and log into Azure when prompted to deploy UI to https://\<name\>.azurewebsites.net and API to
https://\<name\>api.azurewebsites.net.

It's possible to customize the names of deployed resources using script arguments. Run

`Get-Help .\DeployAzure.ps1 -detailed`

for details.

## Custom deployment using Visual Studio

### 1. Update activity and failure logs configurations for `ApiServer`.

- Open `ApiServer/Web.config` and uncomment the unity configuration source `unity.azure-appservice.config`
to write the activity and failure logs to Azure Storage Account:

```xml
<unity configSource="unity.azure-appservice.config"/>
```
Make sure that only one `<unity />` configuration is uncommented. 

- Edit `ApiServer/unity.azure-appservice.config` and add connection strings for
Azure Storage Account where the acitivities and failure logs will be accumulated. Read 
the [Activity and failure logs](#activity-and-failure-logs) section of this document for details.

### 2. Publish `ApiServer`

**The first time** you publish the application you need to create an Azure App Service,
choose a resource group and a service plan. All these settings will be saved as a publish profile file
in the folder `/src/ApiServer/Properties/PublishProfiles`, so next deployments will be very simple.

1. In Visual Studio, in the context menu for the `ApiServer` project, click `Publish...`
2. Select `Microsoft Azure App Service` as a publish target.
3. In the `App Service` window, you should either create new app service (if you do this first time),
or choose previously created app service. For the first time, click `New`.
4. Click `Change Type` and choose `API App`.
5. Enter `API App Name`, for example, `BioModelAnalyzerAPI`.
6. Choose your Azure Subscription.
7. Choose existing or create new Resource Group. In Azure, related resources such as app services,
storage accounts and service plans are logically grouped to be managed as a single entity. So it makes sense
to create a Resource Group for BMA.
8. Choose existing or create new App Service Plan. The plan determines power and cost of the services.
If you choose free plan, you cannot use x64 platform.
9. Click `Create` and wait until all deployment steps completed.
10. In the `Publish` window, make sure that `Publish method` is `Web Deploy`. Check site name and click `Next`. 
11. Select configuration, e.g. `Release - x86`. If you need x64, please read section 
[Choosing the platform architecture (32-bit or 64-bit)](#choosing-the-platform-architecture-32-bit-or-64-bit) 
of this document, because there are 
certain requirements for service plan. 
12. Click `Next`, then `Publish`. Visual Studio will start building the solution for the selected
configuration. If build succeeds, the application will be deployed in Azure and Visual Studio
will open a browser for the application's url, e.g. `http://biomodelanalyzerapi.azurewebsites.net/`.

Since **the publish profile exists**, deployment update is very simple:

1. In Visual Studio, in the context menu for the `ApiServer` project, click `Publish...`.
2. In the `Publish` window, click `Publish`. 
3. If the password wasn't cached on your machine, Visual Studio will ask your for the password.
To find the password, 
    1. Open [https://portal.azure.com](https://portal.azure.com).
    2. Click `App Services`.
    3. Click the published Api App, for example, `BioModelAnalyzerAPI`.
    4. Click `...More` button and then `Get publish profile`.
    5. In the downloaded file, find `userPWD` and use it as the password.


In the `Publish` window it is possible to click `Profile` button and create another publish profile.
You can then choose between existing publish profiles.

So far the API App is published. Next we should publish web application `bma.client`.

### 3. Setup OneDrive access for `bma.client`

If you want to enable OneDrive functionality (allow users to use their OneDrive as model storage), 
you have to register your deployment at OneDrive and modify `bma.client`'s `Web.config` file accordingly.
To do so,

* Go to [dev.onedrive.com](https://dev.onedrive.com)
* Go to App Registration
* Register your app for **OneDrive** (not _OneDrive for Business_) by following the corresponding instructions on the page
  * Add Web platform with redirect URI of the form "https://_\<domain>_/html/callback.html", where _\<domain>_ is the domain you're deploying the client app on (e.g. bmainterface.azurewebsites.net)
* Add Application Id and RedirectUrl to the ``Web.config`` of the project ``bma.client``. Note 
that it is required that `RedirectUrl` uses `https` and BMA web site is also opened using `https`.

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

There is no need to register new application if you just want to move to a new domain.
You only need to update the redirect URI (both in the [settings of your OneDrive App](https://apps.dev.microsoft.com) and the `Web.config` file).

### 4. Update `BackEndUrl` for the `bma.client` application

Edit `Web.config` to provide url of the published `ApiServer`. `BackEndUrl` should be URI
  of the ApiServer, e.g. `http://biomodelanalyzerapi.azurewebsites.net/`. 

```xml
<configuration>
  <appSettings>    
    <add key="BackEndUrl" value="--- url of the ApiServer ---" />
    ...
  </appSettings>
  ...
</configuration>
```

### 5. Update application version

If you publish new version of BioModelAnalyzer application, you need to increase its version number.
To do that, edit `bma.client/version.txt`.

### 6. Publish `bma.client`

**The first time** you publish the application you need to create an Azure App Service,
choose a resource group and a service plan. All these settings will be saved as a publish profile file
in the folder `/src/bma.client/Properties/PublishProfiles`, so next deployments will be very simple.

1. In Visual Studio, in the context menu for the `bma.client` project, click `Publish...`
2. Select `Microsoft Azure App Service` as a publish target.
3. In the `App Service` window, you should either create new app service (if you do this first time),
or choose previously created app service. For the first time, click `New`.
4. Click `Change Type` and choose `Web App`.
5. Enter `Web App Name`, for example, `BioModelAnalyzerClient`.
6. Choose same Azure Subscription as for `ApiServer`.
7. Choose the Resource Group you previously selected for the `ApiServer`.
8. Choose the App Service Plan you previously selected for the `ApiServer`.
9. Click `Create` and wait until all deployment steps completed.
10. In the `Publish` window, make sure that `Publish method` is `Web Deploy`. Check site name and click `Next`. 
11. Select configuration. It is not significant for this application, so you can choose `Release - x86`.
12. Click `Next`, then `Publish`. Visual Studio will start building the solution for the selected
configuration. If build succeeds, the application will be deployed in Azure and Visual Studio
will open a browser for the application's url.
If you enabled OneDrive, you must use `https`, e.g. `http://biomodelanalyzerclient.azurewebsites.net/`, 
otherwise OneDrive will fails.


Since **the publish profile exists**, deployment update is very simple:

1. In Visual Studio, in the context menu for the `bma.client` project, click `Publish...`
2. In the `Publish` window, click `Publish`. 
3. If the password wasn't cached on your machine, Visual Studio will ask your for the password.
To find the password, 
    1. Open [https://portal.azure.com](https://portal.azure.com).
    2. Click `App Services`.
    3. Click the published Web App, for example, `BioModelAnalyzerClient`.
    4. Click `...More` button and then `Get publish profile`.
    5. In the downloaded file, find `userPWD` and use it as the password.


In the `Publish` window it is possible to click `Profile` button and create another publish profile.
You can then choose between existing publish profiles.


After the project is deployed, a browser window should open with the project url.
Normally you see the start-up screen with logo and `Launch Tool` button.
Also you can click `Help` link at the top and load one of the examples.

You can run `WebApiTests` for the deployed `ApiServer` to check whether it works correctly;
see section [Deployment tests](#deployment-tests) in this document.

## Scale up and Scale out

In Azure, a scale up operation allows to choose bigger or smaller server depending on your needs.
A scale out operation creates multiple copies of a web application and adds a load balancer to
distribute demand between them. The article
[Scale up an app in Azure](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-scale)
shows how to scale an application in Azure App Service.


## Choosing the platform architecture (32-bit or 64-bit)
In Visual Studio, change current platform for the solution to either x86 or x64. 
Note that applications built for x86 work on x64 platform, but not vice versa. 
If you deploy it then, this selection must correspond to settings of web hosting or Azure application settings, 
otherwise you will get `Internal Server Error` when trying to access the services. 
This error is caused by `BadImageFormatException` thrown by the web application.

In Azure App Service, the 64-bit platform can be enabled in Basic plans and higher. 
Open Azure Portal, find and click the deployed web application and then open its `Application Settings` and check `Platform`.

To run the applications using IIS Express in Visual Studio, 
open `Tools/Options/Projects and Solutions/Web Projects` and check or uncheck the option 
`Use the 64 bit version of IIS Express for web sites and projects`.

## Activity and failure logs

**ApiServer** exposes `/api/activitylog` endpoint to gather *user activity statistics* from BMA client. Browser sends
a message to this endpoint when a user closes the BMA page.  

Also, **ApiServer** stores information about *failures* during simulations and analysis performed for BMA client.
It saves the failed request and error message so it is possible to reproduce the error case.

Location and format of logs is determined by the Unity config files used by **ApiServer** and depend on
deployment type. See Unity configuration file samples in following sections.

* **Azure** deployment (both App Service or Cloud Service) should use Storage Account to store logs. To do that 
register `BMAWebApi.ActivityAzureLogger` and `BMAWebApi.FailureAzureLogger` types in the **ApiServer** 
Unity configuration file. You will get:
   
   - Activity logs are stored in the `ClientActivity` table of the given Storage Account.
   - Failure logs are stored in the `ServiceFailures` table of the given Storage Account. 
   Table rows reference blobs of the `failures` container which keep both failed request and response.

   The `/sln/ClientStat` solution builds a command line tool that allows to retrieve statistics from
   a Storage Account and display it as charts.

  **It is highly advised not to commit the configuration files containing Azure connection strings to a public repository**. 
```xml
<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
    <container>
      <register type="BMAWebApi.IFailureLogger, BMAWebApi"
              mapTo="BMAWebApi.FailureAzureLogger, BMAWebApi">
        <constructor>
          <param name="connectionString"
                 value="-- Storage Account connection string --" />
        </constructor>
      </register>
      
      <register type="BMAWebApi.IActivityLogger, BMAWebApi"
                mapTo="BMAWebApi.ActivityAzureLogger, BMAWebApi">
        <constructor>
          <param name="connectionString"
                 value="-- Storage Account connection string --" />
        </constructor>
      </register>
    </container>
</unity>
```


* **Web hosting** deployment (including local IIS Express) should use local files to store logs. To do that 
register `BMAWebApi.ActivityFileLogger` and `BMAWebApi.FailureFileLogger` types in the **ApiServer** 
Unity configuration file. You will get:

  - Activity logs are stored as a CSV file `activity_*date-time*.csv` in a folder defined in the unity configuration file. 
  - Failure logs are stored as a CSV file `failures_*date-time*.csv` in a folder defined in the unity configuration file. 
  Along with the file, there is a folder
  `requests` with files keeping the failed requests. The CSV table rows reference these files by names.

  The folders are created in the Web application root directory.

```xml
<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
  <container>
    <register type="BMAWebApi.IFailureLogger, BMAWebApi"
              mapTo="BMAWebApi.FailureFileLogger, BMAWebApi">
      <constructor>
        <param name="dir"
               value="Failures" />
        <param name="tryServerPath"
               value="True" />
      </constructor>
    </register>

    <register type="BMAWebApi.IActivityLogger, BMAWebApi"
              mapTo="BMAWebApi.ActivityFileLogger, BMAWebApi">
      <constructor>
        <param name="dir"
               value="ActivityLog" />
        <param name="tryServerPath"
               value="True" />
      </constructor>
    </register>
  </container>
</unity>
```

* Alternatively, it is possible to write activity and failure logs to `System.Diagnostics.TraceSource`. 
To do that register `BMAWebApi.ActivityTraceLogger` and `BMAWebApi.FailureTraceLogger` types in the **ApiServer** 
Unity configuration file. The loggers will use `TraceSource.TraceData()` method with the given `eventId` and
event type `Information`.

```xml
<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
  <container>
    <register type="BMAWebApi.IFailureLogger, BMAWebApi"
              mapTo="BMAWebApi.FailureTraceLogger, BMAWebApi">
      <constructor>
        <param name="traceSourceName"
               value="BioModelAnalyzer" />
        <param name="eventId"
               value="1" />
      </constructor>
    </register>

    <register type="BMAWebApi.IActivityLogger, BMAWebApi"
              mapTo="BMAWebApi.ActivityTraceLogger, BMAWebApi">
      <constructor>
        <param name="traceSourceName"
               value="BioModelAnalyzer" />
        <param name="eventId"
               value="2" />
      </constructor>
    </register>
  </container>
</unity>
```

# How to release

Here we describe how to release new version of the BioModelAnalyzer application which is
represented by the solution `/sln/bmaclient`.

1. Update `/src/bma.client/version.txt` so it contains new version number for the BioModelAnalyzer release.
2. Add new section to `/RELEASE_NOTES.md` which describes the releasing version and contains summary,
new features and bug fixes lists.
3. Build the bmaclient solution in the Release configuration both for x86 and x64 platforms. 
See [How to Build](#how-to-build).
4. Run regression tests for both platforms. All tests must pass. See [How to Test and Validate](#how-to-test-and-validate).
5. Run BioModelAnalyzer locally or make test deployment then run deployment and end-to-end tests. 
See [Run and deploy](#run-and-deploy).
6. Create a GitHub release. See [Creating Releases](https://help.github.com/articles/creating-releases/).
7. Deploy new version of BioModelAnalyzer to a production server.
8. Test the production deployment using [deployment tests](#deployment-tests).
