import * as builder from 'botbuilder'

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