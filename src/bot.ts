import * as builder from 'botbuilder'

import {registerMiddleware} from './middleware'
import {registerLUISDialog} from './dialogs/luis'
import {registerTutorialDialogs} from './dialogs/tutorials'
import {registerOtherDialogs} from './dialogs/misc'
import {ModelStorage} from './ModelStorage'

/** Registers all dialogs and middlewares onto the given bot instance. */
export function setup (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    registerMiddleware(bot)
    registerLUISDialog(bot, modelStorage)
    registerTutorialDialogs(bot)
    registerOtherDialogs(bot, modelStorage)
}
