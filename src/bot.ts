import * as builder from 'botbuilder'
import * as config from 'config'

import {registerMiddleware} from './middleware'
import {registerLUISDialog} from './dialogs/luis'
import {registerTutorialDialogs} from './dialogs/tutorials'
import {registerOtherDialogs} from './dialogs/misc'

/** Registers all dialogs and middlewares onto the given bot instance. */
export function setup (bot: builder.UniversalBot) {
    registerMiddleware(bot)
    registerLUISDialog(bot)
    registerTutorialDialogs(bot)
    registerOtherDialogs(bot)
}
