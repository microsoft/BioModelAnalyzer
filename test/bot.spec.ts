// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as builder from 'botbuilder'
import * as assert from 'assert'
import * as express from 'express'
import * as http from 'http'
import * as strings from '../src/dialogs/strings'
import {assertConversation} from './helpers/util'

const PORT = 5678

function asAttachment (filename): builder.IAttachment {
    return {
        contentType: 'application/octet-stream',
        contentUrl: `http://localhost:${PORT}/${filename}`
    }
}

describe ('bot conversations', () => {
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

    it ('start dialog directly skipping LUIS', () => {
        return assertConversation([
            { user: '!formulaHistory' }, 
            { bot: strings.FORMULA_HISTORY_EMPTY }
        ])
    })

    it ('start dialog directly with arguments skipping LUIS (debug purposes)', () => {
        return assertConversation([
            { user: '!removeFormula Foo' },
            { bot: msg => assert(msg.text.startsWith(strings.FORMULA_REFERENCE_INVALID(''))) }
        ])
    })

    it ('cancel out of any dialog by saying "cancel"', () => {
        return assertConversation([
            { user: '!tutorials' }, 
            { bot: msg => true },
            { user: 'cancel'},
            { bot: strings.OK }
        ])
    })

    it ('sends the first tutorial', () => {
        return assertConversation([
            { user: '!tutorials' }, 
            { bot: msg => true },
            { user: '1'},
            { bot: msg => assert(msg.text.startsWith(strings.TUTORIAL_INTRO(''))) },
            { bot: msg => true }, // tutorial description
            { bot: msg => msg.text.startsWith(strings.TUTORIAL_START_PROMPT) },
            { user: 'yes'},
            { bot: msg => assert(msg.text.startsWith('[1/'))}
        ])
    })

    it ('user can cancel tutorial selection', () => {
        return assertConversation([
            { user: '!tutorials' }, 
            { bot: msg => true },
            { user: 'the first'},
            { bot: msg => assert(msg.text.startsWith(strings.TUTORIAL_INTRO(''))) },
            { bot: msg => true }, // tutorial description
            { bot: msg => msg.text.startsWith(strings.TUTORIAL_START_PROMPT) },
            { user: 'no'},
            { bot: strings.OK }
        ])
    })

    it ('tutorial selection gets cancelled after two invalid inputs', () => {
        return assertConversation([
            { user: '!tutorials' },
            { bot: msg => true },
            { user: 'foo'},
            { bot: msg => assert(msg.text.startsWith(strings.TUTORIAL_UNKNOWN_SELECT)) },
            { user: 'bar'},
            { bot: strings.TUTORIAL_SELECT_CANCELLED }
        ])
    })

    it ('handles unknown messages', () => {
        return assertConversation([
            { user: 'Tell me a joke' }, 
            { bot: strings.UNKNOWN_INTENT }
        ])
    })

    it ('requests model file before answering LTL queries', () => {
        return assertConversation([
            { user: 'Show me a simulation where x=1' },
            { bot: strings.MODEL_SEND_PROMPT }
        ])
    })

    it ('accepts JSON model file after prompting for it', () => {
        return assertConversation([
            { user: 'Show me a simulation where x=1'},
            { bot: strings.MODEL_SEND_PROMPT }, 
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: 'Try this: x=1'}
        ])
    })

    it ('parse formula without LUIS', () => {
        return assertConversation([
            { user: '!formula x=1'},
            { bot: strings.MODEL_SEND_PROMPT }, 
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: 'Try this: x=1'}
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
            { user: 'which model did I upload?'},
            { bot: msg => assert(msg.text.startsWith('Here is the model you sent me:'), `Mismatch: "${msg.text}"`) }
        ])
    })
  
})
