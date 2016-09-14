// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as assert from 'assert'
import * as Promise from 'promise'
import * as builder from 'botbuilder'
import * as express from 'express'
import * as http from 'http'
import {setup as setupBot} from '../../src/bot'
import TestConnector from './TestConnector'
import MemoryModelStorage from './MemoryModelStorage'

/**
 * Returns a fresh bot for testing.
 */
function createBot () {
    let connector = new TestConnector()
    let bot = new builder.UniversalBot(connector, {
        // this is false by default but we need to access data between unrelated dialogs
        persistConversationData: true
    })
    let modelStorage = new MemoryModelStorage()
    setupBot(bot, modelStorage, true)
    return bot
}

const PORT = 5678

export function serveTestData () {
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

export function asAttachment (filename): builder.IAttachment {
    return {
        contentType: 'application/octet-stream',
        contentUrl: `http://localhost:${PORT}/${filename}`
    }
}

export interface DirectedMessage {
    /** A message from the user */
    user?: string | builder.IAttachment

    /** A message from the bot */
    bot?: string | builder.IAttachment | ((msg: builder.IMessage) => void)
}

/**
 * Verifies bot responses in pre-defined conversations.
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
            if (!refMessage) {
                console.log('Ignoring additional message from bot: "' + actualMessage.text.substr(0, 30) + ' [...]"')
                return
            }
            assert('bot' in refMessage)
            if (typeof refMessage.bot === 'function') {
                refMessage.bot(actualMessage)
            } else {
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
            }
            next()
        })

        // start the conversation
        next()
    })
}

/**
 * Asserts whether a message starts with the given string. To be used as user/bot value in assertConversation().
 */
export function assertStartsWith (text: string) {
    return (msg: builder.IMessage) => assert(msg.text.startsWith(text), `Mismatch: "${msg.text}"`)
}
