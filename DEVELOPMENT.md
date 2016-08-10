# Development guide

This project is developed as a Node.js package using TypeScript.
The following describes how to set up your local development environment.

## Prerequisites

- Install [Node.js](https://nodejs.org/en/download/).
- Install [Azure Storage Emulator](https://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/).
- (Optional) Install [Visual Studio Code](https://code.visualstudio.com/) for TypeScript IDE support. 

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
    "LUIS_KEY": "..."
}
```

By default, in development mode the bot is run inside the console.
To run it as an actual server, add `"USE_CONSOLE": false` to `config/local.json`.
In development mode, the server runs without authentication.

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

Whenever you change source code, you have to restart the server. Use Ctrl-C to stop the server.

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

Deployment logs can be found in the release logs of https://msrcapt.visualstudio.com/BMAChatBot.

#### Debugging

https://bmachatbot.scm.azurewebsites.net/

#### Articles

https://github.com/woloski/nodeonazure-blog/blob/master/articles/startup-task-to-run-npm-in-azure.markdown
https://azure.microsoft.com/en-us/documentation/articles/app-service-deploy-local-git/
http://epikia.eu/2016/07/developing_nodejs_on_azure/