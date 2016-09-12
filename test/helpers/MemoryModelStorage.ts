// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as Promise from 'promise'
import {ModelStorage} from '../../src/ModelStorage'

export default class MemoryModelStorage implements ModelStorage {
    models: { [id: string]: any }
    constructor () {
        this.models = {}
    }
    storeUserModel (id, model) {
        this.models[id] = model
        return Promise.resolve(true)
    }

    getUserModel (id) {
        return Promise.resolve(this.models[id])
    }

    getUserModelUrl (id) {
        return toDataUrl(this.models[id])
    }

    removeUserModel (id) {
        let existed = id in this.models
        delete this.models[id]
        return Promise.resolve(existed)
    }

    storeGeneratedModel (model) {
        return Promise.resolve(toDataUrl(model))
    }
}

function toDataUrl (obj: any) {
    let json = JSON.stringify(obj)
    let base64 = new Buffer(json).toString('base64')
    return 'data:application/json;base64,' + base64
}
