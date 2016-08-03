import * as builder from 'botbuilder'
import * as config from 'config'
import * as yaml from 'js-yaml'
import * as fs from 'fs'

export function setup (bot: builder.UniversalBot) {
    registerLUISDialog(bot)
    registerTutorialDialogs(bot)
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
        let text = session.message.text

        // TODO this doesn't work, LUIS picks up the debug command as another intent
        //      Is there a way to setup a global message router in front of LUIS but still inside the LUIS dialog?
        let debugPrefix = 'start:'
        if (text.match(new RegExp(`^${debugPrefix}.+`))) {
            let dialogId = text.substr(debugPrefix.length)
            console.log('starting dialog ' + dialogId)
            session.beginDialog(dialogId)
        } else {
            session.send('I did not understand you')
        }
    })
}

function registerTutorialDialogs (bot: builder.UniversalBot) {
    // TODO make LUIS dialog available within tutorial dialogs

    let tutorialPaths = [
        '1_ltl_for_dummies'
        ].map(name => `data/tutorials/${name}.yaml`)
    
    let tutorials = tutorialPaths.map(path => fs.readFileSync(path, 'utf8')).map(yaml.safeLoad)

    for (let tutorial of tutorials) {
        let waterfall: builder.IDialogWaterfallStep[] = tutorial.steps.map(step => (session: builder.Session, results, next) => {
            session.send(step.text)
        })

        let dialogId = `/tutorials/${tutorial.id}`
        bot.dialog(dialogId, waterfall)
        console.log(dialogId)
    }
}