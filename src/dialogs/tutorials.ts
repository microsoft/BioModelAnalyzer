import * as builder from 'botbuilder'
import * as yaml from 'js-yaml'
import * as fs from 'fs'

import * as strings from './strings'
import {getTutorialImageAttachment, getBMAModelUrl, getTutorialModelUrl} from '../util'

/** 
 * All listed tutorials are offered to the user in that order. 
 * Strings are filenames in /data/tutorials/ without extension. 
 */
const TUTORIALS = [
    'exploring_the_interface',
    'boolean_operators',
    'temporal_operators',
    'advanced_temporal_operators',   
    'ecoli'
]

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

    /** The filename of the image that will be sent to the user. */
    image: string

    /** Name of the model that is sent to the user. */
    model: string
}

/** Reads all YAML tutorial files and dynamically creates dialogs from them, including the tutorial selection dialog. */
export function registerTutorialDialogs (bot: builder.UniversalBot) {
    // TODO make LUIS dialog available within tutorial dialogs

    // all available tutorials
    let tutorialPaths = TUTORIALS.map(name => `data/tutorials/${name}.yaml`)
    
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
                let message = new builder.Message(session)
                if (step.text) {
                    message.text(step.text)
                }
                if (step.image) {                    
                    message.addAttachment(getTutorialImageAttachment(step.image))
                }
                session.send(message)
                if (step.model) {
                    let bmaModelUrl = getBMAModelUrl(getTutorialModelUrl(step.model))
                    session.send(strings.OPEN_BMA_URL(bmaModelUrl))
                }
                next()
            })
        )

        let dialogId = `/tutorials/${tutorial.id}`
        bot.dialog(dialogId, waterfall)
    }
}