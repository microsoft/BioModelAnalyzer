import * as builder from 'botbuilder'
import CONFIG from './config'

export function setup (bot: builder.UniversalBot) {
    registerLUISDialog(bot)
}

function registerLUISDialog (bot: builder.UniversalBot) {
    // Create LUIS recognizer that points at our model and add it as the root '/' dialog for our bot.
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
}