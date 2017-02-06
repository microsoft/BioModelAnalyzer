// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

import * as strings from '../src/dialogs/strings'
import {assertConversation, assertStartsWith} from './helpers/util'

describe ('bot conversations: debug/command mode (skipping LUIS)', () => {
    it ('start dialog by ID', () => {
        return assertConversation([
            { user: '!formulaHistory' }, 
            { bot: strings.FORMULA_HISTORY_EMPTY }
        ])
    })

    it ('start dialog by ID with arguments', () => {
        return assertConversation([
            { user: '!removeFormula Foo' },
            { bot: assertStartsWith(strings.FORMULA_REFERENCE_INVALID('')) }
        ])
    })

    it ('cancel out of any dialog by saying "cancel"', () => {
        return assertConversation([
            { user: '!tutorials' }, 
            { bot: assertStartsWith(strings.TUTORIAL_SELECT_PROMPT) },
            { user: 'cancel'},
            { bot: strings.OK }
        ])
    })
})
