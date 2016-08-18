import * as builder from 'botbuilder'
import * as restify from 'restify'
import * as config from 'config'
import { BlobModelStorage } from './ModelStorage'
import { setup as setupBot } from './bot'
import NLParser from './NLParser/NLParser'

var model = { "Model": { "Name": "model 1", "Variables": [{ "Name": "x", "Id": 1, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "y", "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "z", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }] } }
var sentence = "show me a simulation where if k is 1 then t is 1"
var parserResponse = NLParser.parse(sentence, model)

let port = config.get('PORT')
console.log('starting on port:', port)

let server = restify.createServer()
server.listen(port, () => {
    console.log('%s listening to %s', server.name, server.url)
})

// in development, static files are served directly via restify (instead of IIS)
if (config.get('SERVE_STATIC_VIA_RESTIFY') === '1') {
    // enable CORS so that the BMA tool can open our tutorial model URLs
    server.use(restify.CORS())

    server.get(/\/?.*/, restify.serveStatic({
        directory: './public'
    }))
}

let botSettings = {
    // this is false by default but we need to access data between unrelated dialogs
    persistConversationData: true
}

let bot: builder.UniversalBot
if (config.get('USE_CONSOLE') === '1') {
    // Create console bot
    let connector = new builder.ConsoleConnector().listen()
    bot = new builder.UniversalBot(connector, botSettings)
} else {
    // Create server bot
    let connector = new builder.ChatConnector({
        appId: config.get<string>('APP_ID'),
        appPassword: config.get<string>('APP_PASSWORD')
    })
    bot = new builder.UniversalBot(connector, botSettings)
    server.post('/api/messages', connector.listen())
}

let modelStorage = new BlobModelStorage()
setupBot(bot, modelStorage)