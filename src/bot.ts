import * as builder from 'botbuilder'
import * as config from 'config'
import * as yaml from 'js-yaml'
import * as fs from 'fs'
import * as strings from './strings'

export function setup (bot: builder.UniversalBot) {
    registerMiddleware(bot)
    registerLUISDialog(bot)
    registerTutorialDialogs(bot)
    registerOtherDialogs(bot)
}

function registerMiddleware (bot: builder.UniversalBot) {
    let debugDialogMiddleware: builder.IMiddlewareMap = {
        botbuilder: (session, next) => {
            let text = session.message.text

            let debugPrefix = 'start:'
            if (text.match(new RegExp(`^${debugPrefix}.+`))) {
                let dialogId = text.substr(debugPrefix.length)
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
    let model = 'https://api.projectoxford.ai/luis/v1/application?id=' + config.get('LUIS_MODEL_ID') 
        + '&subscription-key=' + config.get('LUIS_KEY')
    let recognizer = new builder.LuisRecognizer(model)
    let dialog = new builder.IntentDialog({ recognizers: [recognizer] })
    bot.dialog('/', dialog)

    // Add intent handlers
    dialog.matches('ExplainLTL', builder.DialogAction.send(strings.LTL_DESCRIPTION))
    dialog.matches('LTLQuery', [
        (session, args, next) => {
            // check if JSON model has been uploaded already, otherwise prompt user
            if (!session.userData.bmaModel) {
                builder.Prompts.attachment(session, strings.MODEL_SEND_PROMPT)
            } else {
                // invoke LTL parser
                session.send('Try this: ...')
            }
        },
        (session, results, next) => {
            // check and store attachment
            let attachments: builder.IAttachment[] = results.response
            if (attachments.length > 1) {
                session.send(strings.TOO_MANY_FILES)
                return
            }
            let json = attachments[0].content
            let model: any
            if (typeof json === 'string') {
                // TODO does that ever happen except in test cases?!
                // -> who parses incoming JSON? bot framework, or we?
                try {
                    model = JSON.parse(json)
                } catch (e) {
                    session.send(strings.INVALID_JSON(e.message))
                    return
                }
            } else {
                model = json
            }

            session.userData.bmaModel = model
            session.send(strings.MODEL_RECEIVED)
        }
    ])
    dialog.onDefault(function (session, args) {
        session.send(strings.UNKNOWN_INTENT)
    })
}

function registerTutorialDialogs (bot: builder.UniversalBot) {
    // TODO make LUIS dialog available within tutorial dialogs

    let tutorialPaths = [
        '1_ltl_for_dummies'
        ].map(name => `data/tutorials/${name}.yaml`)
    
    // an array of tutorial objects
    let tutorials: Tutorial[] = tutorialPaths.map(path => fs.readFileSync(path, 'utf8')).map(yaml.safeLoad)

    // TODO add tutorial selection dialog via builder.Prompts.choice
    // see https://docs.botframework.com/en-us/node/builder/chat/prompts/#promptschoice
    // see https://docs.botframework.com/en-us/node/builder/chat-reference/classes/_botbuilder_d_.prompts.html#choice
    // the dialogId could be /tutorials

    bot.dialog('/tutorials', [
        function (session) {
            builder.Prompts.choice(session, "Which tutorial", tutorials.map(tutorial => tutorial.title))
        },
    
        function (session, results: builder.IPromptChoiceResult) {
            if (results.response) {
                let tutorialId = tutorials[results.response.index].id
                let dialogId = `/tutorials/${tutorialId}`
                session.beginDialog(dialogId)   
            } else {
                //session.send("you haven't selected a tutorial")
            }
        }
    ])


    for (let tutorial of tutorials) {
        let waterfall: builder.IDialogWaterfallStep[] = [
            (session: builder.Session) => {
                session.send(`Tutorial: ${tutorial.title}`)
                session.send(tutorial.description)
                builder.Prompts.confirm(session, strings.TUTORIAL_START_PROMPT)
            },
            (session: builder.Session, results: builder.IPromptConfirmResult, next) => {
                if (results.response) {
                    next()
                } else {
                    session.endDialog()
                }
            }
        ]

        waterfall.push(...
            tutorial.steps.map(step => (session: builder.Session, results, next) => {
                session.send(step.text)
                next()
            })
        )

        let dialogId = `/tutorials/${tutorial.id}`
        bot.dialog(dialogId, waterfall)
        console.log(`[Tutorial '${tutorial.id}' registered, launch with "start:${dialogId}"]`)
    }
}

function registerOtherDialogs (bot: builder.UniversalBot) {
    let dialogId = '/requestUploadedModel'
    bot.dialog(dialogId, session => {
        let message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            content: session.userData.bmaModel
        })
        message.text('Here is the model you sent me')
        session.send(message)
    })
}

interface Tutorial {
    /** The internal tutorial ID, unique amongst all tutorials. */
    id: string

    /** A short one-line tutorial title. */
    title: string

    /** A longer description of the tutorial. */
    description: string

    /** Tutorial steps. */
    steps: TutorialStep[]
}

interface TutorialStep {
    /** The text that will be sent to the user. */
    text: string
}