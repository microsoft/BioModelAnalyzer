// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

import * as strings from '../../src/dialogs/strings'
import {serveTestData, asAttachment, assertConversation, assertStartsWith} from '../helpers/util'

describe ('bot conversations: model storage', () => {
    serveTestData()

    it ('responds that no model uploaded yet if user requests model', () => {
        return assertConversation([
            { user: '!requestUploadedModel' },
            { bot: strings.NO_MODEL_FOUND }
        ])
    })

    it ('responds that no model uploaded yet if user tries to remove it', () => {
        return assertConversation([
            { user: '!removeUploadedModel' },
            { bot: strings.NO_MODEL_FOUND }
        ])
    })

    it ('responds with uploaded model if user requests it', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!requestUploadedModel' },
            { bot: assertStartsWith(strings.HERE_IS_YOUR_UPLOADED_MODEL('')) }
        ])
    })

    it ('responds that model got removed if user asks the to remove it', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!removeUploadedModel' },
            { bot: strings.MODEL_REMOVED },
            { user: '!requestUploadedModel' },
            { bot: strings.NO_MODEL_FOUND }
        ])
    })
})
