import * as builder from 'botbuilder'
import * as yaml from 'js-yaml'
import * as fs from 'fs'

import * as strings from './strings'

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

export function registerTutorialDialogs (bot: builder.UniversalBot) {
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