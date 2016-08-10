import * as builder from 'botbuilder'
import * as config from 'config'
import * as request from 'request'
import {v4 as uuid} from 'node-uuid'
import * as strings from './strings'
import Storage from '../storage'
import {downloadAttachments} from '../attachments'

const storage = new Storage()

/**
 * Registers the LUIS dialog as root dialog. 
 */
export function registerLUISDialog (bot: builder.UniversalBot) {
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
                case 'and':session.send(strings.EXPLAIN_ALWAYS)
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

    intents.matches('ExplainLTL', builder.DialogAction.send(strings.LTL_DESCRIPTION))
    intents.matches('LTLQuery', [
        (session, args, next) => {
            // check if JSON model has been uploaded already, otherwise prompt user
            if (!session.conversationData.bmaModel) {
                builder.Prompts.attachment(session, strings.MODEL_SEND_PROMPT)
            } else {
                // invoke LTL parser
                session.send('Try this: ...')
            }
        },
        (session, results, next) => receiveModelAttachmentStep(bot, session, results, next),
        (session, results, next) => {
            // invoke LTL parser
            session.send('Try this: ...')
        }
    ])
    
    intents.onDefault(function (session, results, next) {
        let attachments = session.message.attachments
        if (attachments.length > 0) {
            receiveModelAttachmentStep(bot, session, results, next)
        } else {
            session.send(strings.UNKNOWN_INTENT)
        }
    })
}

function receiveModelAttachmentStep (bot: builder.UniversalBot, session: builder.Session, results, next) {
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
        storage.storeUserModel(modelId, buf).then(() => {
            session.conversationData.bmaModel = model
            session.conversationData.bmaModelId = modelId
            session.send(strings.MODEL_RECEIVED(model.Model.Name))
            next()
        }).catch(e => {
            session.send(strings.HTTP_ERROR(e))
        })
    })
}