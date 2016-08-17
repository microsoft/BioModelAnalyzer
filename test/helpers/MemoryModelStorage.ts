import * as Promise from 'promise'
import {ModelStorage} from '../../src/ModelStorage'

export default class MemoryModelStorage implements ModelStorage {
    models: { [id: string]: any }
    constructor () {
        this.models = {}
    }
    storeUserModel (id: string, content: string) {
        this.models[id] = JSON.parse(content)
        return Promise.resolve(true)
    }

    getUserModel (id: string) {
        return Promise.resolve(this.models[id])
    }

    getUserModelUrl (id: string) {
        let json = JSON.stringify(this.models[id])
        let base64 = new Buffer(json).toString('base64')
        return 'data:application/json;base64,' + base64
    }
}