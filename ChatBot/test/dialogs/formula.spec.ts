// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

import * as strings from '../../src/dialogs/strings'
import {serveTestData, asAttachment, assertConversation, assertStartsWith} from '../helpers/util'

// Note that the following tests do *not* invoke the BMA backend as this is disabled for the tests.
// See the setupBot() call in helpers/util#createBot().
describe ('bot conversations: model and formula', () => {
    serveTestData()

    it ('requests model file before answering LTL queries', () => {
        return assertConversation([
            { user: '!formula x=1' },
            { bot: strings.MODEL_SEND_PROMPT }
        ])
    })

    it ('parses formula correctly', () => {
        return assertConversation([
            { user: '!formula x=1'},
            { bot: strings.MODEL_SEND_PROMPT }, 
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')}
        ])
    })

    it ('informs user if formula cannot be parsed', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula the world is not flat'},
            { bot: strings.UNKNOWN_LTL_QUERY }
        ])
    })

    it ('accepts JSON model file after prompting for it', () => {
        return assertConversation([
            { user: '!formula x=1'},
            { bot: strings.MODEL_SEND_PROMPT }, 
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')}
        ])
    })

    it ('accepts JSON model file without prompting for it', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') }
        ])
    })

    it ('sends JSON model file back to the user when requested', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!requestUploadedModel'},
            { bot: assertStartsWith(strings.HERE_IS_YOUR_UPLOADED_MODEL('')) }
        ])
    })
})
