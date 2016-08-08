import * as builder from 'botbuilder'
import * as config from 'config'

import {registerMiddleware} from './middleware'
import {registerLUISDialog} from './dialogs/luis'
import {registerTutorialDialogs} from './dialogs/tutorials'
import {registerOtherDialogs} from './dialogs/misc'

export function setup (bot: builder.UniversalBot) {
    registerMiddleware(bot)
    registerLUISDialog(bot)
    registerTutorialDialogs(bot)
    registerOtherDialogs(bot)
}
