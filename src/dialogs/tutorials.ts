import * as builder from 'botbuilder'
import * as yaml from 'js-yaml'
import * as fs from 'fs'

import * as strings from './strings'

/** The object structure of a YAML tutorial file. */
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

/** The object structure of a single tutorial step in a YAML tutorial file. */
interface TutorialStep {
    /** The text that will be sent to the user. */
    text: string
}

/** Reads all YAML tutorial files and dynamically creates dialogs from them, including the tutorial selection dialog. */
export function registerTutorialDialogs (bot: builder.UniversalBot) {
    // TODO make LUIS dialog available within tutorial dialogs

    // all available tutorials
    let tutorialPaths = [
        '1_ltl_for_dummies'
        ].map(name => `data/tutorials/${name}.yaml`)
    
    let tutorials: Tutorial[] = tutorialPaths.map(path => fs.readFileSync(path, 'utf8')).map(yaml.safeLoad)

    // the tutorial selection dialog
    bot.dialog('/tutorials', [
        function (session) {
            builder.Prompts.choice(session, 
                strings.TUTORIAL_SELECT_PROMPT, 
                tutorials.map(tutorial => tutorial.title), 
                {
                    listStyle: builder.ListStyle.list,
                    maxRetries: 1,
                    retryPrompt: strings.TUTORIAL_UNKNOWN_SELECT
                })
        },
    
        function (session, results: builder.IPromptChoiceResult) {
            if (results.response) {
                let tutorialId = tutorials[results.response.index].id
                let dialogId = `/tutorials/${tutorialId}`
                session.beginDialog(dialogId)   
            } else {
                session.send(strings.TUTORIAL_SELECT_CANCELLED)
            }
        }
    ])

    // the individual tutorial dialogs
    for (let tutorial of tutorials) {
        // the first two fixed parts of each tutorial...
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

        // ...and the tutorial steps
        waterfall.push(...
            tutorial.steps.map(step => (session: builder.Session, results, next) => {
                session.send(step.text)
                next()
            })
        )

        let dialogId = `/tutorials/${tutorial.id}`
        bot.dialog(dialogId, waterfall)
        
        // for debugging
        // TODO remove at some point
        console.log(`[Tutorial '${tutorial.id}' registered, launch with "start:${dialogId}"]`)
    }
}