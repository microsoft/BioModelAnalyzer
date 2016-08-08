import * as builder from 'botbuilder'

export function registerMiddleware (bot: builder.UniversalBot) {
    let debugDialogMiddleware: builder.IMiddlewareMap = {
        botbuilder: (session, next) => {
            let text = session.message.text

            let debugPrefix = 'start:'
            if (text.match(new RegExp(`^${debugPrefix}.+`))) {
                let dialogId = text.substr(debugPrefix.length)
                session.beginDialog(dialogId)
            } else {
                next()
            }
        }
    }

    bot.use(debugDialogMiddleware)
}