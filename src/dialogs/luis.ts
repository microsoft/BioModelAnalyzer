import * as builder from 'botbuilder'
import * as config from 'config'
import * as request from 'request'
import * as qs from 'querystring'
import {v4 as uuid} from 'node-uuid'
import * as strings from './strings'
import {default as NLParser, ParserResponseType } from '../NLParser/NLParser'
import {toStatesAndFormula, toHumanReadableString, clampVariables} from '../NLParser/ASTUtils'
import {downloadAttachments} from '../attachments'
import {ModelStorage} from '../ModelStorage'
import * as BMA from '../BMA'
import * as BMAApi from '../BMAApi'
import {getBMAModelUrl} from '../util'

/**
 * Registers the LUIS dialog as root dialog. 
 */
export function registerLUISDialog (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    // Create LUIS recognizer that points at our model and add it as the root '/' dialog for our bot.
    let model = 'https://api.projectoxford.ai/luis/v1/application?id=' + config.get('LUIS_MODEL_ID') 
        + '&subscription-key=' + config.get('LUIS_KEY')
    let recognizer = new builder.LuisRecognizer(model)
    let intents = new builder.IntentDialog({ 
        recognizers: [recognizer], 
        intentThreshold: 0.3 // default is 0.1
    })
    bot.dialog('/', intents)
    
    /** A wrapper around intents.matches() which handles resetting of the hasSpellChecked flag. See intents.onDefault below for more details. */
    function matches (intent: string, dialog: builder.IDialogWaterfallStep[]|builder.IDialogWaterfallStep, dialogArgs?: any): builder.IntentDialog {
        if (!Array.isArray(dialog)) {
            dialog = [dialog]
        }
        let firstStep = dialog[0]
        dialog[0] = function (session, args, next) {
            console.log(`Message: "${session.message.text}" [intent: ${args.intent} score: ${args.score}]`)
            if (session.conversationData.hasSpellChecked) {
                session.conversationData.hasSpellChecked = false
                session.save()
            }
            firstStep(session, args, next)
        }
        return intents.matches(intent, dialog, dialogArgs)
    }

    // All intent handlers here
    matches('AboutBot', (session) => {
        session.send(strings.ABOUT_BOT)
    })

    matches('AboutSimulations', (session) => {
        session.send(strings.ABOUT_SIMULATIONS)
    })

    matches('UploadedModel', (session, args) => {
        session.beginDialog('/requestUploadedModel')
    })

    matches('RemoveModel', (session, args) => {
        session.beginDialog('/removeUploadedModel')
    })

    matches('ListTutorial', (session) => {
        session.beginDialog('/tutorials')
    })

    matches ('OperatorInteractions', (session, args) => {
        let operatorInteraction = (builder.EntityRecognizer.findEntity(args.entities, 'interactions')).entity
        switch (operatorInteraction)
        {
            case 'always eventually':session.send(strings.ALWAYS_EVENTUALLY)
            break
            case 'always not':session.send(strings.ALWAYS_NOT)
            break
            case 'always next':session.send(strings.ALWAYS_NEXT)
            break
            case 'eventually always':session.send(strings.EVENTUALLY_ALWAYS)
            break
            case 'eventually not':session.send(strings.EVENTUALLY_NOT)
            break
            case 'eventually next':session.send(strings.EVENTUALLY_NEXT)
            break
            case 'next always':session.send(strings.NEXT_ALWAYS)
            break
            case 'next eventually':session.send(strings.NEXT_EVENTUALLY)
            break
            case 'next not':session.send(strings.NEXT_NOT)
            break
            case 'not eventually':session.send(strings.NOT_EVENTUALLY)
            break
            case 'not next':session.send(strings.NOT_NEXT)
            break
            case 'not always':session.send(strings.NOT_ALWAYS)
            break
        }
        //TODO : deal with user requests that ask to differentiate between interactions
    })
    
    matches('ExplainOp', (session, args) => {
        let operatorName = (builder.EntityRecognizer.findEntity(args.entities, 'Operator')).entity
        switch (operatorName)
        {
            case 'and':session.send(strings.EXPLAIN_AND)
            break
            case 'or':session.send(strings.EXPLAIN_OR)
            break
            case 'implies':session.send(strings.EXPLAIN_IMPLIES)
            break
            case 'not':session.send(strings.EXPLAIN_NOT)
            break
            case 'next':session.send(strings.EXPLAIN_NEXT)
            break
            case 'always':session.send(strings.EXPLAIN_ALWAYS)
            break
            case 'eventually':session.send(strings.EXPLAIN_EVENTUALLY)
            break
            case 'upto':session.send(strings.EXPLAIN_UPTO)
            break
            case 'weakuntil':session.send(strings.EXPLAIN_WEAKUNTIL)
            break
            case 'until':session.send(strings.EXPLAIN_UNTIL)
            break
            case 'release':session.send(strings.EXPLAIN_RELEASE)
            break
        }
    })

    matches('OperatorExample', (session, args) => {
        let operatorName = (builder.EntityRecognizer.findEntity(args.entities, 'Operator')).entity
        switch (operatorName)
        {
            case 'and':session.send(strings.EXAMPLE_AND)
            break
            case 'or':session.send(strings.EXAMPLE_OR)
            break
            case 'implies':session.send(strings.EXAMPLE_IMPLIES)
            break
            case 'not':session.send(strings.EXAMPLE_NOT)
            break
            case 'next':session.send(strings.EXAMPLE_NEXT)
            break
            case 'always':session.send(strings.EXAMPLE_ALWAYS)
            break
            case 'eventually':session.send(strings.EXAMPLE_EVENTUALLY)
            break
            case 'upto':session.send(strings.EXAMPLE_UPTO)
            break
            case 'weakuntil':session.send(strings.EXAMPLE_WEAKUNTIL)
            break
            case 'until':session.send(strings.EXAMPLE_UNTIL)
            break
            case 'release':session.send(strings.EXAMPLE_RELEASE)
            break
        }   
    })

    matches('Semantics', (session, args) => {
        let lookupSemantics = (builder.EntityRecognizer.findEntity(args.entities, 'Lookup')).entity
        switch (lookupSemantics)
        {
            case 'oscillation':
            case 'oscillations':session.send(strings.OSCILLATIONS)
            break
            case 'true state':session.send(strings.TRUE_STATE)
            break
            case 'selfloop':
            case 'self loop':session.send(strings.SELF_LOOP)
            break
            case 'steps':session.send(strings.STEPS)
            break
            case 'increase':session.send(strings.I_STEPS)
            break
            case 'decrease':session.send(strings.D_STEPS)
            break
        }   
    })

    function handleLTLQuery (session: builder.Session, text: string) {
        let bmaModel = session.conversationData.bmaModel
        let result = NLParser.parse(text, bmaModel)
        if (result.responseType !== ParserResponseType.SUCCESS) {
            session.send(strings.UNKNOWN_LTL_QUERY)
            return
        }

        let ast = result.AST

        // check if variable values in range and clamp if necessary
        let clampingResult = clampVariables(ast, bmaModel)
        if (clampingResult.clampings.length) {
            ast = clampingResult.AST
            let text = strings.VARIABLES_CLAMPED(clampingResult.clampings.map(c => `${c.variable.Name} -> ${c.clampedValue} (was ${c.originalValue})`).join(', '))
            session.send(text)
        }

        session.send(strings.TRY_THIS_FORMULA(toHumanReadableString(ast, bmaModel)))

        // merge formula into model copy and offer to user via URL
        let ltl = toStatesAndFormula(ast, bmaModel)
        let newBmaModel: BMA.ModelFile = JSON.parse(JSON.stringify(bmaModel))
        newBmaModel.ltl = ltl

        modelStorage.storeGeneratedModel(newBmaModel).then(url => {
            session.send(strings.OPEN_BMA_MODEL_LINK(getBMAModelUrl(url)))
        })

        let steps = 10
        let simulationOptions = {
            steps,
            timeout: 3
        }
        let expandedFormula = BMAApi.getExpandedFormula(bmaModel.Model, ltl.states, ltl.operations[0])
        BMAApi.runFastSimulation(bmaModel.Model, expandedFormula, simulationOptions).then(responseFast => {
            BMAApi.runThoroughSimulation(bmaModel.Model, expandedFormula, responseFast, simulationOptions).then(responseThorough => {
                if (responseThorough.Status) {
                    session.send(strings.SIMULATION_DUALITY(steps))
                } else if (responseFast.Status) {
                    session.send(strings.SIMULATION_ALWAYS_TRUE(steps))
                } else {
                    session.send(strings.SIMULATION_ALWAYS_FALSE(steps))
                }
            }).catch(e => {
                if (e.code === 'ETIMEDOUT') {
                    if (responseFast.Status) {
                        session.send(strings.SIMULATION_PARTIAL_TRUE(steps))
                    } else {
                        session.send(strings.SIMULATION_PARTIAL_FALSE(steps))
                    }
                } else {
                    throw e
                }
            })
        }).catch(e => {
            if (e.code === 'ETIMEDOUT') {
                session.send(strings.SIMULATION_CANCELLED)
            } else {
                throw e
            }
        })
    }

    matches('ExplainLTL', builder.DialogAction.send(strings.LTL_DESCRIPTION))
    matches('LTLQuery', [
        (session, args, next) => {
            // check if JSON model has been uploaded already, otherwise prompt user
            if (!session.conversationData.bmaModel) {
                session.conversationData.lastMessageText = session.message.text
                session.save()
                builder.Prompts.attachment(session, strings.MODEL_SEND_PROMPT)
            } else {
                handleLTLQuery(session, session.message.text)
                session.endDialog()
            }
        },
        (session, results, next) => receiveModelAttachmentStep(bot, modelStorage, session, results, next),
        (session, results, next) => {
            handleLTLQuery(session, session.conversationData.lastMessageText)
            delete session.conversationData.lastMessageText
        }
    ])
    
    intents.onDefault((session, results, next) => {
        let attachments = session.message.attachments
        if (attachments && attachments.length > 0) {
            receiveModelAttachmentStep(bot, modelStorage, session, results, next)
            return
        }

        console.log(`Message: "${session.message.text}" [not recognized]`)
        console.log(JSON.stringify(results, null, 2))

        if (config.get('ENABLE_SPELLCHECK') === '0' || session.conversationData.hasSpellChecked) {
            session.conversationData.hasSpellChecked = false
            session.save()
            session.send(strings.UNKNOWN_INTENT)
            return
        }

        let inputText = session.message.text
        let params = {
            text: inputText,
            mode: 'proof',
            mkt: 'en-GB'
        }
        let spellUrl = 'https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?' + qs.stringify(params)
        request(spellUrl, {
            headers: {
                'Ocp-Apim-Subscription-Key': config.get('BING_SPELLCHECK_KEY')
            },
            json: true
        }, (error, response, body) => {
            if (error || body.flaggedTokens.length === 0) {
                if (error) {
                    console.error(error)
                } else {
                    console.log('spellcheck response:')
                    console.log(JSON.stringify(body, null, 2))
                }
                session.send(strings.UNKNOWN_INTENT)
                return
            }
            console.log('spellcheck response:')
            console.log(JSON.stringify(body, null, 2))

            let inputOffset = 0
            let correctedText = ''
            for (let flaggedToken of body.flaggedTokens) {
                let offset = flaggedToken.offset
                if (inputOffset < offset) {
                    correctedText += inputText.substring(inputOffset, offset)
                }
                correctedText += flaggedToken.suggestions[0].suggestion
                inputOffset = offset + flaggedToken.token.length
            }
            if (inputOffset < inputText.length) {
                correctedText += inputText.substring(inputOffset)
            }

            session.conversationData.hasSpellChecked = true
            session.save()
            
            session.send(strings.SPELLCHECK_ASSUMPTION(correctedText))

            let message = new builder.Message(session)
            message.text(correctedText)
            session.dispatch(session.sessionState, message.toMessage())
        })
    })
}

function receiveModelAttachmentStep (bot: builder.UniversalBot, modelStorage: ModelStorage, session: builder.Session, results, next) {
    // check and store attachment
    let attachments = session.message.attachments
    if (attachments.length > 1) {
        session.send(strings.TOO_MANY_FILES)
        return
    }
    downloadAttachments(bot.connector('*'), session.message, (err, buffers) => {
        if (err) {
            session.send(strings.HTTP_ERROR(err))
            return
        }
        // TODO handle more than one attachment
        let buf = buffers[0].toString()
        let model: BMA.ModelFile
        try {
            model = JSON.parse(buf)
        } catch (e) {
            session.send(strings.INVALID_JSON(e))
            return
        }
        let modelId = session.conversationData.bmaModelId || uuid()
        modelStorage.storeUserModel(modelId, model).then(() => {
            session.conversationData.bmaModel = model
            session.conversationData.bmaModelId = modelId
            session.save()
            session.send(strings.MODEL_RECEIVED(model.Model.Name))
            next()
        }).catch(e => {
            session.send(strings.HTTP_ERROR(e))
            session.endDialog()
        })
    })
}