import * as builder from 'botbuilder'
import * as express from 'express'
import * as cors from 'cors'
import * as config from 'config'
import { BlobModelStorage } from './ModelStorage'
import { setup as setupBot } from './bot'
import { default as NLParser, ParserResponseType, FormulaPointer } from './NLParser/NLParser'
import { ModelFile, Ltl } from './BMA'
import * as ASTUtils from './NLParser/ASTUtils'
let testModel: ModelFile = require('../test/data/testmodel.json')

var sentence = "show me a simulation where a is maximally active"
var parserResponse = NLParser.parse(sentence, testModel)
var expected = ""
ASTUtils.toHumanReadableString(parserResponse.AST, testModel)

let port = config.get('PORT')
console.log('starting server on port:', port)

let server = express()
server.listen(port)

// in development, static files are served directly via express (instead of IIS)
if (config.get('SERVE_STATIC_VIA_EXPRESS') === '1') {
    // enable CORS so that the BMA tool can open our tutorial model URLs
    server.use(cors())
    console.log('serving static files via express')
    server.use('/static', express.static('public'))
}

let botSettings = {
    // this is false by default but we need to access data between unrelated dialogs
    persistConversationData: true
}

let bot: builder.UniversalBot
if (config.get('USE_CONSOLE') === '1') {
    // Create console bot
    let connector = new builder.ConsoleConnector().listen()
    bot = new builder.UniversalBot(connector, botSettings)
} else {
    // Create server bot
    let connector = new builder.ChatConnector({
        appId: config.get<string>('APP_ID'),
        appPassword: config.get<string>('APP_PASSWORD')
    })
    bot = new builder.UniversalBot(connector, botSettings)
    server.post('/api/messages', connector.listen())
}

let modelStorage = new BlobModelStorage()
setupBot(bot, modelStorage)