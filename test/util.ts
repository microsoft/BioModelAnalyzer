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
                connector.processMessage(messages[currentMsgIndex].user)
            }
        }

        bot.on('incoming', (actualMessage: builder.IMessage) => {
            let refMessage = messages[currentMsgIndex]
            assert('user' in refMessage)
            assert.equal(actualMessage.text, connector.toMessage(refMessage.user).text)
            next()
        })
        bot.on('send', (actualMessage: builder.IMessage) => {
            let refMessage = messages[currentMsgIndex]
            assert('bot' in refMessage)
            assert.equal(actualMessage.text, connector.toMessage(refMessage.bot).text)
            next()
        })

        next()
    })
}
