import * as builder from 'botbuilder'

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot) {
    let dialogId = '/requestUploadedModel'
    bot.dialog(dialogId, session => {
        let message = new builder.Message(session)
        message.addAttachment({
            contentType: 'application/octet-stream',
            content: session.userData.bmaModel
        })
        message.text('Here is the model you sent me')
        session.send(message)
    })
}