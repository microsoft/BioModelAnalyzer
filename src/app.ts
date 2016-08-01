import * as builder from 'botbuilder'
import * as restify from 'restify'
import CONFIG from './config'

var useConsole = false

let bot: builder.UniversalBot
if (useConsole) {
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

// Create LUIS recognizer that points at our model and add it as the root '/' dialog for our Cortana Bot.
var model = 'https://api.projectoxford.ai/luis/v1/application?id=' + CONFIG.LUIS_MODEL_ID + '&subscription-key=' + CONFIG.LUIS_KEY
var recognizer = new builder.LuisRecognizer(model)
var dialog = new builder.IntentDialog({ recognizers: [recognizer] })
bot.dialog('/', dialog)

// Add intent handlers
dialog.matches('ExplainLTL', builder.DialogAction.send('LTL means linear temporal logic'))
dialog.matches('LTLQuery', [
    function (session, args, next) {
        // send file
        var message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            content: 'foo'
        })
        message.text('attachment coming')
        session.send(message)
    }
])
dialog.onDefault(builder.DialogAction.send('sorry, no idea what you are saying'))