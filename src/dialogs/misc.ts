import * as builder from 'botbuilder'

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot) {
    bot.dialog('/requestUploadedModel', session => {
        let model = session.userData.bmaModel
        if (!model) {
            // TODO fetch model from storage via .bmaModelId
        }
        // TODO rewrite
        let message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            content: model
        })
        message.text(`Here is the model you sent me (Name: ${model.Model.Name})`)
        session.send(message)
    })
}