import * as builder from 'botbuilder'

const TestConversationID = 'Convo1'

/**
 * A mock connector usable for unit testing. Inspired by ConsoleConnector.
 */
export default class TestConnector implements builder.IConnector {
    handler: (events: builder.IEvent[], callback?: (err: Error) => void) => void

    /** Part of IConnector interface */
    onEvent (handler) {
        this.handler = handler
    }

    /** Part of IConnector interface; not needed as we don't physically deliver messages to the user in tests */
    send (messages, cb) {
    }

    /** Part of IConnector interface */
    startConversation (address, cb) {
        let adr = Object.create(address) // shallow copy
        adr.conversation = { id: TestConversationID }
        cb(null, adr)
    }

    /** Custom method used by tests to send messages to the bot. */
    processMessage (message: string | builder.IAttachment) {
        if (this.handler) {
            this.handler([this.toMessage(message)])
        }
        return this
    }

    /** Custom convenience method for converting message-like objects to real IMessage's */
    toMessage (message: string | builder.IAttachment): builder.IMessage {
        // adapted from ConsoleConnector
        let msg = 
            new builder.Message().address({
                channelId: 'test',
                user: { id: 'user', name: 'User1' },
                bot: { id: 'bot', name: 'Bot' },
                conversation: { id: TestConversationID }
            }).timestamp()
            
        if (typeof message === 'string') {
            msg.text(message)
        } else {
            msg.text('').addAttachment(message)
        }
        return msg.toMessage()
    }
}
