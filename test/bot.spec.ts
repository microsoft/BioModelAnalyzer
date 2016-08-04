import {setup as setupBot} from '../src/bot'
import * as builder from 'botbuilder'
import * as assert from 'assert'

interface BotReplyCallback {
    (message: builder.IMessage): any
}

function onBotReply (bot: builder.UniversalBot, cb: BotReplyCallback) {
    let testMiddleware: builder.IMiddlewareMap = {
        send: (event, next) => {
            if (event.type === 'message') {
                let message = <builder.IMessage> event
                cb(message)
            }
            next()
        }
    }
    bot.use(testMiddleware)
}

describe ('bot conversations', () => {
    let bot: builder.UniversalBot
    let connector: builder.ConsoleConnector

    beforeEach (() => {
        connector = new builder.ConsoleConnector().listen()
        bot = new builder.UniversalBot(connector)
        setupBot(bot)
    })

    afterEach (() => {
        // 'quit' is handled specially by the ConsoleConnector and will clean up
        connector.processMessage('quit')
    })

    it ('handles unknown messages', (done: Function) => {
        onBotReply(bot, message => {
            assert.equal(message.text, 'I did not understand you')            
            done()
        })

        connector.processMessage('Tell me a joke')
    })
})