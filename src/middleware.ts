import * as builder from 'botbuilder'

/** 
 * Registers middleware for the given bot.
 * 
 * Currently, this is for debugging purposes such that arbitrary dialogs
 * can be started with the message syntax 'start:/dialogId' while skipping
 * any intent recognizers like LUIS.
 */
export function registerMiddleware (bot: builder.UniversalBot) {
    let debugDialogMiddleware: builder.IMiddlewareMap = {
        botbuilder: (session, next) => {
            let text = session.message.text
            let debugPrefix = 'start:'

            if (text === 'cancel') {
                session.endDialog()
            } else if (text.match(new RegExp(`^${debugPrefix}.+`))) {
                let dialogId = text.substr(debugPrefix.length)
                session.beginDialog(dialogId)
            } else {
                next()
            }
        }
    }

    bot.use(debugDialogMiddleware)
}