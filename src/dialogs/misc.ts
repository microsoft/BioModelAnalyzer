import * as builder from 'botbuilder'
import {ModelStorage} from '../ModelStorage'
import {getBMAModelUrl} from '../util'
import * as strings from './strings'

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    bot.dialog('/requestUploadedModel', (session, results, next) => {
        let modelId = session.conversationData.bmaModelId
        let url = getBMAModelUrl(modelStorage.getUserModelUrl(modelId))
        session.send(strings.HERE_IS_YOUR_UPLOADED_MODEL(url))
        next()
    })
    bot.dialog('/removeUploadedModel', (session, results, next) => {
        //session.conversationData.bmaModelId = null
        session.conversationData.bmaModel = null
        session.send('Model removed')
        next()
    })
}