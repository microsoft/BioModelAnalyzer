import * as builder from 'botbuilder'
import * as config from 'config'
import * as request from 'request'
import * as qs from 'querystring'
import {v4 as uuid} from 'node-uuid'
import * as strings from './strings'
import {default as NLParser, ParserResponseType } from '../NLParser/NLParser'
import {downloadAttachments} from '../attachments'
import {ModelStorage} from '../ModelStorage'

/**
 * Registers the LUIS dialog as root dialog. 
 */
export function registerLUISDialog (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    // Create LUIS recognizer that points at our model and add it as the root '/' dialog for our bot.
    let model = 'https://api.projectoxford.ai/luis/v1/application?id=' + config.get('LUIS_MODEL_ID') 
        + '&subscription-key=' + config.get('LUIS_KEY')
    let recognizer = new builder.LuisRecognizer(model)
    let intents = new builder.IntentDialog({ recognizers: [recognizer] })
    bot.dialog('/', intents)

    /**
     * All intent handlers here 
     */
    intents.matches('AboutBot', [function (session) {
            session.send(strings.ABOUT_BOT)
        }
    ])

    intents.matches('AboutSimulations', [function (session) {
            session.send(strings.ABOUT_SIMULATIONS)
        }
    ])

    intents.matches('UploadedModel', [function (session, args) {
            session.beginDialog('requestUploadedModel')
        }
    ])

    intents.matches('ListTutorial', [function (session) {
            session.beginDialog('/tutorials')
        }
    ])
    
    intents.matches('SelectTutorial', [function (session, args) {
        
    }])
    
    intents.matches('ExplainOp', [function (session, args) {
            var operator = builder.EntityRecognizer.findEntity(args.entities, 'Operator')
            var operatorName = operator.entity
            switch (operatorName)
            {
                case 'and':session.send(strings.EXPLAIN_AND)
                break;
                case 'or':session.send(strings.EXPLAIN_OR)
                break;
                case 'implies':session.send(strings.EXPLAIN_IMPLIES)
                break;
                case 'not':session.send(strings.EXPLAIN_NOT)
                break;
                case 'next':session.send(strings.EXPLAIN_NEXT)
                break;
                case 'always':session.send(strings.EXPLAIN_ALWAYS)
                break;
                case 'eventually':session.send(strings.EXPLAIN_EVENTUALLY)
                break;
                case 'upto':session.send(strings.EXPLAIN_UPTO)
                break;
                case 'weakuntil':session.send(strings.EXPLAIN_WEAKUNTIL)
                break;
                case 'until':session.send(strings.EXPLAIN_UNTIL)
                break;
                case 'release':session.send(strings.EXPLAIN_RELEASE)
                break;
            }   
        }
    ])

    function handleLTLQuery (session: builder.Session) {
        let text = session.message.text
        let result = NLParser.parse(text, session.conversationData.bmaModel)
        if (result.responseType !== ParserResponseType.SUCCESS) {
            session.send('I did not understand your query')
            return
        }
        session.send('Try this: ' + result.humanReadableFormula)
    }

    intents.matches('ExplainLTL', builder.DialogAction.send(strings.LTL_DESCRIPTION))
    intents.matches('LTLQuery', [
        (session, args, next) => {
            // check if JSON model has been uploaded already, otherwise prompt user
            if (!session.conversationData.bmaModel) {
                builder.Prompts.attachment(session, strings.MODEL_SEND_PROMPT)
            } else {
                handleLTLQuery(session)
                session.endDialog()
            }
        },
        (session, results, next) => receiveModelAttachmentStep(bot, modelStorage, session, results, next),
        (session, results, next) => {
            handleLTLQuery(session)
        }
    ])
    
    intents.onDefault(function (session, results, next) {
        let attachments = session.message.attachments
        if (attachments && attachments.length > 0) {
            receiveModelAttachmentStep(bot, modelStorage, session, results, next)
        } else {
            if (session.conversationData.hasSpellChecked) {
                // FIXME reset spellcheck state in other intents too!
                session.conversationData.hasSpellChecked = false
                session.send(strings.UNKNOWN_INTENT)
                return
            }

            let inputText = session.message.text
            let params = {
				// Request parameters, 
				text: inputText,
				mode: 'proof'
			}
            let spellUrl = 'https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?' + qs.stringify(params)
            request(spellUrl, {
                headers: {
                    'Ocp-Apim-Subscription-Key': config.get('BING_SPELLCHECK_KEY')
                },
                json: true
            }, (error, response, body) => {
                if (error || body.flaggedTokens.length === 0) {
                    session.send(strings.UNKNOWN_INTENT)
                    return
                }

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
                session.conversationData.hasSpellChecked = true
                
                session.send(strings.SPELLCHECK_ASSUMPTION(correctedText))

                let message = new builder.Message(session)
                message.text(correctedText)
                session.dispatch(session.sessionState, message.toMessage())
            })
        }
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
        let model
        try {
            model = JSON.parse(buf)
        } catch (e) {
            session.send(strings.INVALID_JSON(e))
            return
        }
        let modelId = session.conversationData.bmaModelId || uuid()
        modelStorage.storeUserModel(modelId, buf).then(() => {
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