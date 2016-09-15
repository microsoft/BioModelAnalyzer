// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as strings from '../../src/dialogs/strings'
import {serveTestData, asAttachment, assertConversation, assertStartsWith} from '../helpers/util'

// Note that testing LUIS involves network requests.
describe ('bot conversations: natural language via LUIS', () => {
    serveTestData()

    it ('handles unknown messages', () => {
        return assertConversation([
            { user: 'Tell me a joke' }, 
            { bot: strings.UNKNOWN_INTENT }
        ])
    })

    it ('recognizes LTL queries', () => {
        return assertConversation([
            { user: 'show me a simulation where x is 1' },
            { bot: strings.MODEL_SEND_PROMPT }
        ])
    })

    it ('recognizes uploaded model request', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: 'which model did I upload?'},
            { bot: assertStartsWith(strings.HERE_IS_YOUR_UPLOADED_MODEL('')) }
        ])
    })
})
