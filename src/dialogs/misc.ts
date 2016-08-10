import * as builder from 'botbuilder'
import Storage from '../storage'

const storage = new Storage()

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot) {
    bot.dialog('/requestUploadedModel', session => {
        let modelId = session.conversationData.bmaModelId
        let url = storage.getUserModelUrl(modelId)

        let message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            contentUrl: url
        })

        let model = session.conversationData.bmaModel
        message.text(`Here is the model you sent me (Name: ${model.Model.Name})`)
        session.send(message)
    })
}