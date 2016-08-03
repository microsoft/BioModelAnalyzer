import * as builder from 'botbuilder'
import * as config from 'config'
import * as yaml from 'js-yaml'
import * as fs from 'fs'

export function setup (bot: builder.UniversalBot) {
    registerMiddleware(bot)
    registerLUISDialog(bot)
    registerTutorialDialogs(bot)
}

function registerMiddleware (bot: builder.UniversalBot) {
    let debugDialogMiddleware: builder.IMiddlewareMap = {
        botbuilder: (session, next) => {
            let text = session.message.text

            let debugPrefix = 'start:'
            if (text.match(new RegExp(`^${debugPrefix}.+`))) {
                let dialogId = text.substr(debugPrefix.length)
                console.log('starting dialog ' + dialogId)
                session.beginDialog(dialogId)
            } else {
                next()
            }
        }
    }

    bot.use(debugDialogMiddleware)
}

function registerLUISDialog (bot: builder.UniversalBot) {
    // Create LUIS recognizer that points at our model and add it as the root '/' dialog for our bot.
    let model = 'https://api.projectoxford.ai/luis/v1/application?id=' + config.get('LUIS_MODEL_ID') + '&subscription-key=' + config.get('LUIS_KEY')
    let recognizer = new builder.LuisRecognizer(model)
    let dialog = new builder.IntentDialog({ recognizers: [recognizer] })
    bot.dialog('/', dialog)

    // Add intent handlers
    dialog.matches('ExplainLTL', builder.DialogAction.send('LTL means linear temporal logic'))
    dialog.matches('LTLQuery', [
        function (session, args, next) {
            // send file
            let message = new builder.Message(session)
            message.addAttachment({
                contentType: 'application/octet-stream',
                content: 'foo'
            })
            message.text('attachment coming')
            session.send(message)
        }
    ])
    dialog.onDefault(function (session, args) {
        session.send('I did not understand you')
    })
}

function registerTutorialDialogs (bot: builder.UniversalBot) {
    // TODO make LUIS dialog available within tutorial dialogs

    let tutorialPaths = [
        '1_ltl_for_dummies'
        ].map(name => `data/tutorials/${name}.yaml`)
    
    let tutorials = tutorialPaths.map(path => fs.readFileSync(path, 'utf8')).map(yaml.safeLoad)

    for (let tutorial of tutorials) {
        let waterfall: builder.IDialogWaterfallStep[] = 
            tutorial.steps.map(step => (session: builder.Session, results, next) => {
                session.send(step.text)
                next()
            })

        let dialogId = `/tutorials/${tutorial.id}`
        bot.dialog(dialogId, waterfall)
        console.log(`[Tutorial '${tutorial.id}' registered, launch with "start:${dialogId}"]`)
    }
}