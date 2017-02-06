// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

import * as strings from '../../src/dialogs/strings'
import {serveTestData, asAttachment, assertConversation, assertStartsWith} from '../helpers/util'

// Note that the following tests do *not* invoke the BMA backend as this is disabled for the tests.
// See the setupBot() call in helpers/util#createBot().
describe ('bot conversations: formula history', () => {
    serveTestData()

    it ('stores formula in history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: '!formulaHistory' },
            { bot: msg => msg.text.indexOf('[FA] x=1') !== -1 }
        ])
    })

    it ('allow to clear the formula history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: '!removeFormulas' },
            { bot: strings.FORMULA_HISTORY_CLEARED }
        ])
    })

    it ('inform user that formula history is empty on clearing request', () => {
        return assertConversation([
            { user: '!removeFormulas' },
            { bot: strings.FORMULA_HISTORY_EMPTY }
        ])
    })

    it ('allow to remove a single formula from the history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: '!removeFormula FA' },
            { bot: strings.FORMULA_REMOVED_FROM_HISTORY('x=1') }
        ])
    })

    it ('allow to rename a formula in the history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: '!renameFormula {"from": "FA", "to": "foo"}' },
            { bot: strings.FORMULA_RENAMED('FA', 'foo') },
            { user: '!formulaHistory' },
            { bot: msg => msg.text.indexOf('[foo] x=1') !== -1 }
        ])
    })

})
