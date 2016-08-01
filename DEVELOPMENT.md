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

The last step is to copy the `src/config.sample.js` file to `src/config.js` and modify its contents accordingly.

### Development

As Node.js doesn't understand TypeScript natively, the project source (in /src) has to be transpiled to JavaScript.
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