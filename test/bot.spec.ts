// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as builder from 'botbuilder'
import * as express from 'express'
import * as http from 'http'
import * as strings from '../src/dialogs/strings'
import {assertConversation, assertStartsWith} from './helpers/util'

const PORT = 5678

function serveTestData () {
    let app: express.Express
    let server: http.Server
    before(() => {
        app = express()
        app.use('/', express.static('./test/data'))
        server = app.listen(PORT)        
    })

    after(() => {
        server.close()
    })
}

function asAttachment (filename): builder.IAttachment {
    return {
        contentType: 'application/octet-stream',
        contentUrl: `http://localhost:${PORT}/${filename}`
    }
}

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
