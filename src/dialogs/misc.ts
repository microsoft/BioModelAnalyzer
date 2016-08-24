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
        if (!modelId) {
            session.send(strings.NO_MODEL_FOUND)
            next()
            return
        }
        let url = getBMAModelUrl(modelStorage.getUserModelUrl(modelId))
        session.send(strings.HERE_IS_YOUR_UPLOADED_MODEL(url))
        next()
    })
    bot.dialog('/removeUploadedModel', (session, results, next) => {
        let id = session.conversationData.bmaModelId
        if (!id) {
            session.send(strings.NO_MODEL_FOUND)
            next()
            return
        }
        delete session.conversationData.bmaModelId
        delete session.conversationData.bmaModel
        modelStorage.removeUserModel(id)
        session.send(strings.MODEL_REMOVED)
        next()
    })
}