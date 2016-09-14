// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as strings from '../../src/dialogs/strings'
import {assertConversation, assertStartsWith} from '../helpers/util'

describe ('bot conversations: tutorials', () => {
    it ('sends the first tutorial', () => {
        return assertConversation([
            { user: '!tutorials' },
            { bot: assertStartsWith(strings.TUTORIAL_SELECT_PROMPT) },
            { user: '1'},
            { bot: assertStartsWith(strings.TUTORIAL_INTRO('')) },
            { bot: msg => true }, // tutorial description
            { bot: assertStartsWith(strings.TUTORIAL_START_PROMPT) },
            { user: 'yes' },
            { bot: assertStartsWith('[1/') }
        ])
    })

    it ('user can cancel tutorial selection', () => {
        return assertConversation([
            { user: '!tutorials' }, 
            { bot: assertStartsWith(strings.TUTORIAL_SELECT_PROMPT) },
            { user: 'the first'}, // this is not LUIS, just bot framework logic
            { bot: assertStartsWith(strings.TUTORIAL_INTRO('')) },
            { bot: msg => true }, // tutorial description
            { bot: assertStartsWith(strings.TUTORIAL_START_PROMPT) },
            { user: 'no'}, // this is not LUIS, just some clever bot framework logic
            { bot: strings.OK }
        ])
    })

    it ('tutorial selection gets cancelled after two invalid inputs', () => {
        return assertConversation([
            { user: '!tutorials' },
            { bot: assertStartsWith(strings.TUTORIAL_SELECT_PROMPT) },
            { user: 'foo'},
            { bot: assertStartsWith(strings.TUTORIAL_UNKNOWN_SELECT) },
            { user: 'bar'},
            { bot: strings.TUTORIAL_SELECT_CANCELLED }
        ])
    })
})
