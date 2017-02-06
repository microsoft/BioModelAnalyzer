[onedrive]: https://microsoft-my.sharepoint.com/personal/t-mariec_microsoft_com/Documents/BMAChatBot%20shared

# Development guide

This project is developed as a Node.js package using TypeScript.
The following describes how to set up your local development environment.

## Prerequisites

- Install [Node.js](https://nodejs.org/en/download/).
- Install [Azure Storage Emulator](https://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/).
- (Optional) [Bot Framework Emulator](https://docs.botframework.com/en-us/tools/bot-framework-emulator/).
- (Optional) Install [Visual Studio Code](https://code.visualstudio.com/) for TypeScript IDE support.
- (Optional) Install the [TSLint extension](https://marketplace.visualstudio.com/items?itemName=eg2.tslint) for Visual Studio Code (checks code style).
- (Optional) Install the [CodeMetrics extension](https://marketplace.visualstudio.com/items?itemName=kisstkondoros.vscode-codemetrics) for Visual Studio Code (checks code complexity).

## Project set up

```sh
$ git clone https://msrcapt.visualstudio.com/DefaultCollection/_git/BMAChatBot
$ cd BMAChatBot
$ npm install
```

You have to run `npm install` whenever the dependencies (inside the `package.json` file) change.

The final step is to create a local configuration file `config/local.json` with the following contents:

```json
{
    "LUIS_MODEL_ID": "...",
    "LUIS_KEY": "...",
    "BING_SPELLCHECK_KEY": "..."
}
```

You can find all credentials in the [shared OneDrive folder][onedrive].
For access permissions ask [mailto:matjoh@microsoft.com](Matthew Johnson).

By default, in development mode the bot is run as a local server without authentication
and works out of the box with the Bot Framework Emulator and Azure Storage Emulator.

To run the bot inside a console without server, add `"USE_CONSOLE": "1"` to `config/local.json`.
Note that this mode has limited features (e.g. no attachment support).

### Development

As Node.js doesn't understand TypeScript natively, the project source (in `/src`) has to be transpiled to JavaScript.
The following long-running command automatically transpiles TypeScript source files whenever they get changed.
Run this command in a separate terminal:

```sh
$ npm run watch
```

Make sure the Azure Storage Emulator (see Prerequisites) is running before starting the bot server.

To start the bot server, run the following command in a new terminal:

```sh
$ npm start
```

If you don't use the console mode (see previous section), start the Bot Framework Emulator and talk to the bot,
otherwise talk to the bot directly in your console.

Whenever you change source code, you have to restart the server. Use Ctrl-C to stop the server.

### Running tests

Run all tests:

```sh
$ npm test
```

To get code coverage for the tests, run:

```sh
$ npm run coverage
```

The project has automated testing (continuous integration) set up in Visual Studio Team Services on every git push.
See [the VSTS project](https://msrcapt.visualstudio.com/BMAChatBot/_build) to view the test history.

### Adding dependencies

To make TypeScript happy, you need to supply it with typing definitions of all package dependencies.
A few libraries have those embedded, but most of them don't.
For the latter case, search for definitions on <http://microsoft.github.io/TypeSearch/> 
and install them with `npm install --save @types/...`.

### Deployment

The production chat bot is hosted on https://bmachatbot.azurewebsites.net/.
Deployment happens manually by creating a new "Production" release in https://msrcapt.visualstudio.com/BMAChatBot.

The dev version is hosted on https://bmachatbot-dev.azurewebsites.net/.
Deployment happens automatically each day at 8am or by manually creating a new "Development" release in https://msrcapt.visualstudio.com/BMAChatBot.

You can also deploy from the command-line with:

```sh
$ git remote add azure-dev https://bmabotdeploy@bmachatbot-dev.scm.azurewebsites.net:443/bmachatbot-dev.git
$ git remote add azure-prod https://bmabotdeploy@bmachatbot.scm.azurewebsites.net:443/bmachatbot.git
$ git push azure-dev master
$ # or: git push azure-prod +0.1.0~0:master
```

For git deployment, you need the deployment password which can be found in the [shared OneDrive folder][onedrive].

Deployment logs can be found in the release logs of https://msrcapt.visualstudio.com/BMAChatBot.

#### Debugging

If possible, debugging should be done locally, typically via logging or by attaching to the Node.js process in Visual Studio Code
and setting breakpoints appropriately.

If an error occurs only when deployed, then looking at live logs in the Azure Portal often helps,
as exceptions are visible there and other log output. The following describes how to do that:

1. Find the bmachatbot[dev] project on https://portal.azure.com/
2. Go to the "Monitoring" -> "Diagnostics" tab and enable "Application Logging (Filesystem)"
3. Go to the "Monitoring" -> "Log stream" tab and observe the live log output

A different way to explore the deployed app service is via [Kudu](https://github.com/projectkudu/kudu/wiki):

- https://bmachatbot.scm.azurewebsites.net/
- https://bmachatbotdev.scm.azurewebsites.net/

#### Articles

- [Local Git Deployment to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-deploy-local-git/)