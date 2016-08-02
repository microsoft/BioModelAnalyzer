# Development guide

This project is developed as a Node.js package using TypeScript.
The following describes how to set up your local development environment.

## Prerequisites

- Install [Node.js](https://nodejs.org/en/download/).
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
The default bot's app id and password are `foo` and `bar`, respectively. Those are not needed in console mode.

### Development

As Node.js doesn't understand TypeScript natively, the project source (in `/src`) has to be transpiled to JavaScript.
The following long-running command automatically transpiles TypeScript source files whenever they get changed.
Run this command in a separate terminal:

```sh
$ npm run watch
```

To start the bot server, run the following command in a new terminal:

```sh
$ npm start
```

Whenever you change source code, you have to restart the server. Use Ctrl-C to stop the server.

### Adding dependencies

To make TypeScript happy, you need to supply it with typing definitions of all package dependencies.
A few libraries have those embedded, but most of them don't.
For the latter, install the [TypeScript Definition Manager](https://www.npmjs.com/package/typings) with `npm install typings --global`
and then search for definitions with `typings search apackage`. Installation can be done with `typings install ...`, see the documentation for more details.