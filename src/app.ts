import * as builder from 'botbuilder'
import * as restify from 'restify'
import CONFIG from './config'
import {setup as setupBot} from './bot'

let bot: builder.UniversalBot
if (CONFIG.USE_CONSOLE) {
    // Create bot and bind to console
    let connector = new builder.ConsoleConnector().listen()
    bot = new builder.UniversalBot(connector)
} else {
    // Setup Restify Server
    let server = restify.createServer()
    server.listen(CONFIG.PORT, function () {
        console.log('%s listening to %s', server.name, server.url)
    })
    
    // Create chat bot
    let connector = new builder.ChatConnector({
        appId: CONFIG.MICROSOFT_APP_ID, 
        appPassword: CONFIG.MICROSOFT_APP_PASSWORD
    })
    bot = new builder.UniversalBot(connector)
    server.post('/api/messages', connector.listen())
}

setupBot(bot)