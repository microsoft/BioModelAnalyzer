import * as assert from 'assert'
import * as Promise from 'promise'
import * as builder from 'botbuilder'
import {setup as setupBot} from '../src/bot'
import TestConnector from './TestConnector'

/**
 * Returns a fresh bot for testing.
 */
function createBot () {
    let connector = new TestConnector()
    let bot = new builder.UniversalBot(connector)
    setupBot(bot)
    return bot
}

export interface DirectedMessage {
    /** A message from the user */
    user?: string | builder.IAttachment

    /** A message from the bot */
    bot?: string | builder.IAttachment
}

/**
 * Verifies bot responses in pre-defined conversations.
 * Currently only supports text messages. 
 */
export function assertConversation (messages: DirectedMessage[]) {
    return new Promise(resolve => {
        let bot = createBot()
        let connector = bot.connector('test') as TestConnector

        let currentMsgIndex = -1
        let next = () => {
            currentMsgIndex++
            if (currentMsgIndex === messages.length) {
                resolve(true)
            } else if ('user' in messages[currentMsgIndex]) {
                // if the message originates from the user, send it to the bot
                connector.processMessage(messages[currentMsgIndex].user)
            }
        }

        // handle messages that the user sends to the bot
        bot.on('incoming', (actualMessage: builder.IMessage) => {
            let refMessage = messages[currentMsgIndex]
            assert('user' in refMessage)
            assert.equal(actualMessage.text, connector.toMessage(refMessage.user).text)
            next()
        })

        // handle messages that the bot sends to the user
        bot.on('send', (actualMessage: builder.IMessage) => {
            let refMessage = messages[currentMsgIndex]
            assert('bot' in refMessage)
            let msg = connector.toMessage(refMessage.bot)
            if (msg.text) {
                assert.equal(actualMessage.text, msg.text)
            }
            if (msg.attachments && msg.attachments.length > 0) {
                if (msg.attachments.length > 1) {
                    throw new Error('not implemented yet')
                }
                let refAttachment = msg.attachments[0]
                let actualAttachment = actualMessage.attachments[0]
                

                assert.deepEqual(actualAttachment.content, refAttachment.content)
            }
            next()
        })

        // start the conversation
        next()
    })
}
