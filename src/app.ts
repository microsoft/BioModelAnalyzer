import * as builder from 'botbuilder'
import * as restify from 'restify'
import * as config from 'config'
import Storage from './storage'
import {setup as setupBot} from './bot'
import NLParser from './NLParser/NLParser'


var model = { "Model": { "Name": "model 1", "Variables": [{ "Name": "a", "Id": 2, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "c", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }, { "Name": "d", "Id": 3, "RangeFrom": 0, "RangeTo": 1, "Formula": "" }] } }
var sentence = "if a=1 and c=1 then d=1 eventually"
var parserResponse = NLParser.parse(sentence, model)


let storage = new Storage()
storage.init()

let server = restify.createServer()
server.listen(config.get('PORT'), () => {
    console.log('%s listening to %s', server.name, server.url)
})
if (config.get('SERVE_STATIC_VIA_RESTIFY')) {
    server.get(/\/?.*/, restify.serveStatic({
        directory: './public'
    }))
}

let botSettings = {
    // this is false by default but we need to access data between unrelated dialogs
    persistConversationData: true
}

let bot: builder.UniversalBot
if (config.get('USE_CONSOLE')) {
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

setupBot(bot)