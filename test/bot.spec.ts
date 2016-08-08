import * as builder from 'botbuilder'
import * as assert from 'assert'
import * as strings from '../src/strings'
import {assertConversation} from './util'

describe ('bot conversations', () => {
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
        let attachment: builder.IAttachment = {
            contentType: 'application/octet-stream',
            content: '{}'
        }
        return assertConversation([
            { user: 'Show me a simulation where x=1'},
            { bot: strings.MODEL_SEND_PROMPT }, 
            { user: attachment },
            { bot: strings.MODEL_RECEIVED }
        ])
    })

    it ('accepts JSON model file without prompting for it', () => {
        let attachment: builder.IAttachment = {
            contentType: 'application/octet-stream',
            content: '{}'
        }
        return assertConversation([
            { user: attachment },
            { bot: strings.MODEL_RECEIVED }
        ])
    })

    it ('sends JSON model file back to the user when requested', () => {
        let attachment: builder.IAttachment = {
            contentType: 'application/octet-stream',
            content: '{}'
        }
        return assertConversation([
            { user: attachment },
            { bot: strings.MODEL_RECEIVED },
            { user: 'Please send me my model file'},
            { bot: attachment}
        ])
    })
    
})
