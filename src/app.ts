import * as builder from 'botbuilder'
import * as restify from 'restify'
import * as config from 'config'
import Storage from './storage'
import {setup as setupBot} from './bot'

let storage = new Storage()
storage.init()

let botSettings = {
    // this is false by default but we need to access data between unrelated dialogs
    persistConversationData: true
}

let bot: builder.UniversalBot
if (config.get('USE_CONSOLE')) {
    // Create bot and bind to console
    let connector = new builder.ConsoleConnector().listen()
    bot = new builder.UniversalBot(connector, botSettings)
} else {
    // Setup Restify Server
    let server = restify.createServer()
    server.listen(config.get('PORT'), function () {
        console.log('%s listening to %s', server.name, server.url)
    })
    
    // Create chat bot
    let connector = new builder.ChatConnector({
        appId: config.get<string>('APP_ID'), 
        appPassword: config.get<string>('APP_PASSWORD')
    })
    bot = new builder.UniversalBot(connector, botSettings)
    server.post('/api/messages', connector.listen())
}

setupBot(bot)