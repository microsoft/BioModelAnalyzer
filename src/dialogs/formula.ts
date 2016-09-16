// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as builder from 'botbuilder'
import * as _ from 'underscore'
import * as BMA from '../BMA'
import * as BMAApi from '../BMAApi'
import {ModelStorage} from '../ModelStorage'
import {default as NLParser, ParserResponseType } from '../NLParser/NLParser'
import {NamedFormula, embedNamedFormulas, toStatesAndFormula, toHumanReadableString, clampVariables} from '../NLParser/ASTUtils'
import {getBMAModelUrl, LETTERS_F} from '../util'
import {receiveModelAttachmentStep} from './modelStorage'
import * as strings from './strings'

/**
 * Registers dialogs related to the formula history.
 */
export function registerFormulaDialog (bot: builder.UniversalBot, modelStorage: ModelStorage, skipBMAAPI: boolean) {
    bot.dialog('/formula', [
        (session, args, next) => {
            let text = args
            // check if JSON model has been uploaded already, otherwise prompt user
            if (!session.conversationData.bmaModel) {
                session.conversationData.lastMessageText = text
                session.save()
                builder.Prompts.attachment(session, strings.MODEL_SEND_PROMPT)
            } else {
                processFormulaText(session, text, modelStorage, skipBMAAPI)
                session.endDialog()
            }
        },
        (session, results, next) => receiveModelAttachmentStep(bot, modelStorage, session, results, next),
        (session, results, next) => {
            processFormulaText(session, session.conversationData.lastMessageText, modelStorage, skipBMAAPI)
            delete session.conversationData.lastMessageText
        }
    ])
}

/**
 * Tries to parse a given formula in natural language into a structured LTL formula,
 * stores it into the formula history, sends the formula back to the user in BMA string format
 * and as a BMA model link, and also tests the formula using the BMA backend.
 * 
 * Note that testing a formula is done on a best-efforts basis in regards to time spent.
 * If the BMA backend takes too long, then the user is informed about that and directed to the BMA tool
 * to test the formula himself.  
 */
function processFormulaText (session: builder.Session, text: string, modelStorage: ModelStorage, skipBMAAPI: boolean) {
    // fetch some session state
    let bmaModel: BMA.ModelFile = session.conversationData.bmaModel

    if (!session.conversationData.formulas) {
        session.conversationData.formulas = []
    }
    let namedFormulas: NamedFormula[] = session.conversationData.formulas

    // parse formula
    let result = NLParser.parse(text, bmaModel, namedFormulas)
    if (result.responseType !== ParserResponseType.SUCCESS) {
        session.send(strings.UNKNOWN_LTL_QUERY)
        return
    }
    let ast = result.AST

    let formulaCount = session.userData.totalFormulaCount = (session.userData.totalFormulaCount || 0) + 1
    if (formulaCount === 1) {
        session.send(strings.FORMULA_HISTORY_FIRST_NOTICE)
    } else if (formulaCount === 5) {
        let randomVariable = bmaModel.Model.Variables.length > 0 ? bmaModel.Model.Variables[0].Name : 'Notch'
        session.send(strings.FORMULA_SHORTCUT(randomVariable))
    } else if (formulaCount === 100) {
        session.send(strings.FORMULA_COUNT_100)
    }

    // embed all named formulas
    ast = embedNamedFormulas(ast, bmaModel, namedFormulas)

    // check if variable values are in range and clamp if necessary
    let clampingResult = clampVariables(ast, bmaModel)
    if (clampingResult.clampings.length) {
        ast = clampingResult.AST
        let text = strings.VARIABLES_CLAMPED(clampingResult.clampings.map(c => `${c.variable.Name} -> ${c.clampedValue} (was ${c.originalValue})`).join(', '))
        session.send(text)
    }

    // store formula in history for later composition if not in history yet
    let astStr = JSON.stringify(ast)
    let isInHistory = namedFormulas.some(historyAst => JSON.stringify(historyAst.ast) === astStr)
    if (!isInHistory) {
        let unusedId = _.isEmpty(namedFormulas) ? 0 : namedFormulas[namedFormulas.length - 1].id + 1
        let unusedFormulaName = _.find(LETTERS_F, letter => !namedFormulas.some(f => f.name.toLowerCase() === letter.toLowerCase()))
        if (!unusedFormulaName) {
            session.send(strings.FORMULA_HISTORY_FULL)
        } else {
            namedFormulas.push({
                id: unusedId,
                name: unusedFormulaName,
                ast
            })
        }
    }

    // send human readable version of formula
    session.send(strings.TRY_THIS_FORMULA(toHumanReadableString(ast, bmaModel)))

    // merge formula into model copy and offer to user via URL
    let ltl = toStatesAndFormula(ast, bmaModel)
    let newBmaModel: BMA.ModelFile = JSON.parse(JSON.stringify(bmaModel))
    newBmaModel.ltl = ltl

    modelStorage.storeGeneratedModel(newBmaModel).then(url => {
        session.send(strings.OPEN_BMA_MODEL_LINK(getBMAModelUrl(url)))
    })

    if (!skipBMAAPI) {
        // run formula on BMA backend and send result back to user
        let steps = 10
        let simulationOptions = {
            steps,
            timeout: 3
        }
        let expandedFormula = BMAApi.getExpandedFormula(bmaModel.Model, ltl.states, ltl.operations[0])
        BMAApi.runFastSimulation(bmaModel.Model, expandedFormula, simulationOptions).then(responseFast => {
            BMAApi.runThoroughSimulation(bmaModel.Model, expandedFormula, responseFast, simulationOptions).then(responseThorough => {
                if (responseThorough.Status) {
                    session.send(strings.SIMULATION_DUALITY(steps))
                } else if (responseFast.Status) {
                    session.send(strings.SIMULATION_ALWAYS_TRUE(steps))
                } else {
                    session.send(strings.SIMULATION_ALWAYS_FALSE(steps))
                }
            }).catch(e => {
                if (e.code === 'ETIMEDOUT') {
                    if (responseFast.Status) {
                        session.send(strings.SIMULATION_PARTIAL_TRUE(steps))
                    } else {
                        session.send(strings.SIMULATION_PARTIAL_FALSE(steps))
                    }
                } else {
                    throw e
                }
            })
        }).catch(e => {
            if (e.code === 'ETIMEDOUT') {
                session.send(strings.SIMULATION_CANCELLED)
            } else {
                throw e
            }
        })
    }
}
