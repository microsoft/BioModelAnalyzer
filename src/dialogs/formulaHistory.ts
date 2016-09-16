// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as builder from 'botbuilder'
import * as _ from 'underscore'
import * as BMA from '../BMA'
import {toHumanReadableString, NamedFormula} from '../NLParser/ASTUtils'
import * as strings from './strings'

/**
 * Registers dialogs related to the formula history.
 */
export function registerFormulaHistoryDialogs (bot: builder.UniversalBot) {
    /*
     * Sends a list of all formulas in the history.
     */
    bot.dialog('/formulaHistory', (session, args, next) => {
        let model: BMA.ModelFile = session.conversationData.bmaModel
        let formulas: NamedFormula[] = session.conversationData.formulas
        if (!formulas) {
            session.send(strings.FORMULA_HISTORY_EMPTY)
            next()
            return
        }

        let text = strings.FORMULA_HISTORY(getFormattedFormulas(formulas, model))
        session.send(text)
        next()
    })

    /*
     * Removes all formulas in the history.
     */
    bot.dialog('/removeFormulas', (session, args, next) => {
        let formulas: NamedFormula[] = session.conversationData.formulas
        if (!formulas || !formulas.length) {
            session.send(strings.FORMULA_HISTORY_EMPTY)
            next()
            return
        }
        delete session.conversationData.formulas
        session.send(strings.FORMULA_HISTORY_CLEARED)
        next()
    })

    /*
     * Removes a specific formula from the history, specified either by formula name
     * or position in the history, starting from 1.
     */
    bot.dialog('/removeFormula', (session, args: string | number, next) => {
        let model: BMA.ModelFile = session.conversationData.bmaModel
        let formulas: NamedFormula[] = session.conversationData.formulas || []

        let formulaNumber: number
        if (!args) {
            formulaNumber = -1
        } else if (typeof args === 'number') {
            formulaNumber = args
        } else if (typeof args === 'string') {
            formulaNumber = _.findIndex(formulas, f => f.name.toLowerCase() === (<string>args).toLowerCase())
            if (formulaNumber !== -1) {
                formulaNumber += 1
            }
        }
        
        if (!formulas || formulaNumber < 0 || formulaNumber > formulas.length) {
            session.send(strings.FORMULA_REFERENCE_INVALID(getFormattedFormulas(formulas, model)))
            next()
            return
        }
        let formula: NamedFormula = formulas[formulaNumber - 1]
        formulas.splice(formulaNumber - 1, 1)
        session.send(strings.FORMULA_REMOVED_FROM_HISTORY(toHumanReadableString(formula.ast, model)))
        next()
    })

    /*
     * Renames a formula in the history by specifying the name of the formula to rename, and the new name.
     */
    bot.dialog('/renameFormula', (session, args: {from: string, to: string}, next) => {
        let formulas: NamedFormula[] = session.conversationData.formulas || []
        let from = args.from || ''
        let formula = _.find(formulas, f => f.name.toLowerCase() === from.toLowerCase())
        if (!formula) {
            let model: BMA.ModelFile = session.conversationData.bmaModel
            session.send(strings.FORMULA_REFERENCE_INVALID(getFormattedFormulas(formulas, model)))
            next()
            return
        }
        let oldName = formula.name
        let to = args.to ? args.to.trim().replace(' ', '') : null
        if (!to) {
            session.send(strings.FORMULA_RENAME_NAME_EMPTY)
            next()
            return
        }
        
        // check if name already exists
        if (formulas.some(f => f.name.toLowerCase() === to.toLowerCase())) {
            session.send(strings.FORMULA_RENAME_TO_EXISTS(to))
            next()
            return
        }

        formula.name = to
        session.send(strings.FORMULA_RENAMED(oldName, to))
        next()
    })
}

/**
 * Returns string representations of the given formulas in the format that the BMA UI undestands (via copy-pasting). 
 */
export function getFormattedFormulas (formulas: NamedFormula[], model: BMA.ModelFile) {
    return formulas.map(formula => `[${formula.name}] ${toHumanReadableString(formula.ast, model)}`).join(' \n\n ')
}
