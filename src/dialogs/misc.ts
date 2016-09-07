import * as builder from 'botbuilder'
import * as _ from 'underscore'
import {ModelStorage} from '../ModelStorage'
import {getBMAModelUrl} from '../util'
import * as BMA from '../BMA'
import {toHumanReadableString, NamedFormula} from '../NLParser/ASTUtils'
import * as strings from './strings'

/**
 * Registers dialogs that are not grouped into a specific theme yet.
 */
export function registerOtherDialogs (bot: builder.UniversalBot, modelStorage: ModelStorage) {
    bot.dialog('/requestUploadedModel', (session, args, next) => {
        let modelId = session.conversationData.bmaModelId
        if (!modelId) {
            session.send(strings.NO_MODEL_FOUND)
            next()
            return
        }
        let url = getBMAModelUrl(modelStorage.getUserModelUrl(modelId))
        session.send(strings.HERE_IS_YOUR_UPLOADED_MODEL(url))
        next()
    })
    bot.dialog('/removeUploadedModel', (session, args, next) => {
        let id = session.conversationData.bmaModelId
        if (!id) {
            session.send(strings.NO_MODEL_FOUND)
            next()
            return
        }
        delete session.conversationData.bmaModelId
        delete session.conversationData.bmaModel
        modelStorage.removeUserModel(id)
        session.send(strings.MODEL_REMOVED)
        next()
    })
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
    bot.dialog('/removeFormula', (session, args: string | number, next) => {
        let model: BMA.ModelFile = session.conversationData.bmaModel
        let formulas: NamedFormula[] = session.conversationData.formulas

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
    bot.dialog('/renameFormula', (session, args: {from: string, to: string}, next) => {
        let formulas: NamedFormula[] = session.conversationData.formulas
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

export function getFormattedFormulas (formulas: NamedFormula[], model: BMA.ModelFile) {
    return formulas.map(formula => `[${formula.name}] ${toHumanReadableString(formula.ast, model)}`).join(' \n\n ')
}
