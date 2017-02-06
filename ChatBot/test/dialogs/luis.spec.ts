// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

import * as strings from '../../src/dialogs/strings'
import {serveTestData, asAttachment, assertConversation, assertStartsWith} from '../helpers/util'

// Note that testing LUIS involves network requests.
describe ('bot conversations: natural language via LUIS', () => {
    serveTestData()

    it ('handles unknown messages', () => {
        return assertConversation([
            { user: 'Tell me a joke' }, 
            { bot: strings.UNKNOWN_INTENT }
        ])
    })

    it ('bot introduces itself', () => {
        return assertConversation([
            { user: 'hello' },
            { bot: strings.ABOUT_BOT }
        ])
    })

    it ('bot explains LTL', () => {
        return assertConversation([
            { user: 'what does LTL mean?' },
            { bot: strings.LTL_DESCRIPTION }
        ])
    })

    it ('bot explains simulations', () => {
        return assertConversation([
            { user: 'What kind of simulations can you find?' },
            { bot: strings.ABOUT_SIMULATIONS }
        ])
    })

    it ('bot lists available tutorials', () => {
        return assertConversation([
            { user: 'which tutorials can i do?' },
            { bot: assertStartsWith(strings.TUTORIAL_SELECT_PROMPT) }
        ])
    })

    it ('bot explains operator interactions', () => {
        return assertConversation([
            { user: 'how can i use always eventually' },
            { bot: strings.ALWAYS_EVENTUALLY }
        ])
    })

    it ('bot explains operator', () => {
        return assertConversation([
            { user: 'what does eventually mean?' },
            { bot: strings.EXPLAIN_EVENTUALLY }
        ])
    })

    it ('bot gives operator example', () => {
        return assertConversation([
            { user: 'show me an example of the next operator' },
            { bot: assertStartsWith(strings.EXAMPLE_NEXT) }
        ])
    })

    it ('bot explains semantics', () => {
        return assertConversation([
            { user: 'What happens if I increase the number of steps?' },
            { bot: strings.INCREASE_STEPS },
            { user: 'what is an oscillation?' },
            { bot: strings.OSCILLATIONS },
            { user: 'how does a self loop work?' },
            { bot: strings.SELF_LOOP }
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

    it ('recognizes model removal request', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: 'remove my uploaded model'},
            { bot: strings.MODEL_REMOVED }
        ])
    })

    it ('recognizes formula history request', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: 'show me the formula history' }, // this is what we test
            { bot: msg => msg.text.indexOf('[FA] x=1') !== -1 }
        ])
    })

    it ('allow to remove a single formula from the history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: 'forget the FA formula' }, // this is what we test
            { bot: strings.FORMULA_REMOVED_FROM_HISTORY('x=1') }
        ])
    })

    it ('recognizes formula history clearing request', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: 'clear all formulas' }, // this is what we test
            { bot: strings.FORMULA_HISTORY_CLEARED }
        ])
    })

    it ('allow to rename a formula in the history', () => {
        return assertConversation([
            { user: asAttachment('testmodel.json') },
            { bot: strings.MODEL_RECEIVED('model 1') },
            { user: '!formula x is 1'},
            { bot: strings.FORMULA_HISTORY_FIRST_NOTICE },
            { bot: strings.TRY_THIS_FORMULA('x=1')},
            { bot: assertStartsWith(strings.OPEN_BMA_MODEL_LINK('')) },
            { user: 'please rename the FA formula to foo' }, // this is what we test
            { bot: strings.FORMULA_RENAMED('FA', 'foo') },
            { user: '!formulaHistory' },
            { bot: msg => msg.text.indexOf('[foo] x=1') !== -1 }
        ])
    })
})
