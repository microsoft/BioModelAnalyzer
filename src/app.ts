import * as builder from 'botbuilder'
import * as restify from 'restify'
import * as config from 'config'
import {BlobModelStorage} from './ModelStorage'
import {setup as setupBot} from './bot'

let port = config.get('PORT')
console.log('starting on port:', port)

let server = restify.createServer()
server.listen(port, () => {
    console.log('%s listening to %s', server.name, server.url)
})

// in development, static files are served directly via restify (instead of IIS)
if (config.get('SERVE_STATIC_VIA_RESTIFY')) {
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

let modelStorage = new BlobModelStorage()
setupBot(bot, modelStorage)