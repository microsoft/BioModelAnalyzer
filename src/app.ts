import * as builder from 'botbuilder'
import * as restify from 'restify'
import * as config from 'config'
import {setup as setupBot} from './bot'

let bot: builder.UniversalBot
if (config.get('USE_CONSOLE')) {
    // Create bot and bind to console
    let connector = new builder.ConsoleConnector().listen()
    bot = new builder.UniversalBot(connector)
} else {
    // Setup Restify Server
    let server = restify.createServer()
    server.listen(config.get('PORT'), function () {
        console.log('%s listening to %s', server.name, server.url)
    })
    
    // Create chat bot
    let connector = new builder.ChatConnector({
        appId: config.get('APP_ID'), 
        appPassword: config.get('APP_PASSWORD')
    })
    bot = new builder.UniversalBot(connector)
    server.post('/api/messages', connector.listen())
}

setupBot(bot)