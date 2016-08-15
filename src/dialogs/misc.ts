import * as builder from 'botbuilder'
import {ModelStorage} from '../ModelStorage'

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    bot.dialog('/requestUploadedModel', (session, results, next) => {
        let modelId = session.conversationData.bmaModelId
        let url = modelStorage.getUserModelUrl(modelId)

        let message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            contentUrl: url
        })

        let model = session.conversationData.bmaModel
        message.text(`Here is the model you sent me (Name: ${model.Model.Name})`)
        session.send(message)
        next()
    })
    bot.dialog('/removeUploadedModel', (session, results, next) => {
        //session.conversationData.bmaModelId = null
        session.conversationData.bmaModel = null
        session.send('Model removed')
        next()
    })
}