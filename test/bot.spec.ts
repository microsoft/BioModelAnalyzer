import * as builder from 'botbuilder'
import * as assert from 'assert'
import * as restify from 'restify'
import * as strings from '../src/dialogs/strings'
import {assertConversation} from './util'

const PORT = 5678

function asAttachment (filename): builder.IAttachment {
    return {
        contentType: 'application/octet-stream',
        contentUrl: `http://localhost:${PORT}/${filename}`
    }
}

describe ('bot conversations', () => {
    let server: restify.Server
    before(() => {
        server = restify.createServer()
        server.listen(PORT)
        server.get(/\/?.*/, restify.serveStatic({
            directory: './test/data'
        }))
    })

    after(() => {
        server.close()
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
