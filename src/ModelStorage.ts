// Copyright (C) 2016 Microsoft - All Rights Reserved

import * as Promise from 'promise'
import * as azure from 'azure-storage'
import * as config from 'config'
import * as url from 'url'
import {v4 as uuid} from 'node-uuid'
import * as BMA from './BMA'

const USER_MODELS = 'usermodels'
const GENERATED_MODELS = 'genmodels'

export interface ModelStorage {
    storeUserModel (id: string, model: BMA.ModelFile): Promise.IThenable<boolean>
    getUserModel (id: string): Promise.IThenable<any>
    getUserModelUrl (id: string): string
    removeUserModel (id: string): Promise.IThenable<boolean>
    storeGeneratedModel (model: BMA.ModelFile): Promise.IThenable<string>
}

export class BlobModelStorage implements ModelStorage {
    blobService
    constructor () {
        this.blobService = azure.createBlobService(config.get('AZURE_STORAGE_ACCOUNT'), config.get('AZURE_STORAGE_ACCESS_KEY'))

        // enable CORS
        this.blobService.getServiceProperties((error, result, response) => {
            if (error) {
                throw error
            }
            // origin is http://biomodelanalyzer.research.microsoft.com
            // so BMA_URL without the path part at the end
            let bmaUrl = url.parse(config.get<string>('BMA_URL'))
            let bmaOrigin = bmaUrl.protocol + '//' + bmaUrl.host

            var serviceProperties = result
            serviceProperties.Cors = {
                CorsRule: [{
                    AllowedOrigins: [bmaOrigin],
                    AllowedMethods: ['GET'],
                    AllowedHeaders: [],
                    ExposedHeaders: [],
                    MaxAgeInSeconds: 3600
                }]
            }
            console.log('setting blob service properties:')
            console.log(JSON.stringify(serviceProperties, null, 4))
            this.blobService.setServiceProperties(serviceProperties, (error, result, response) => {  
                if (error) {
                    throw error
                }
            })
        })
        
        // create containers
        for (let container of [USER_MODELS, GENERATED_MODELS]) {
            this.blobService.createContainerIfNotExists(container, {
                publicAccessLevel: 'blob'
            }, (error, result, response) => {
                if (error) {
                    throw error
                }
            })
        }
    }

    storeUserModel (id, model) {
        // TODO think about expiration
        let content = JSON.stringify(model, null, 2)

        return new Promise((resolve, reject) => {
            this.blobService.createBlockBlobFromText(USER_MODELS, id, content, {}, (error, response) => {
                if (error) {
                    reject(error)
                } else {
                    console.log('User model stored: ' + id)
                    resolve(true)
                }
            })
        })        
    }

    getUserModel (id) {
        return new Promise((resolve, reject) => {
            this.blobService.getBlobToText(USER_MODELS, id, {}, (error, text) => {
                if (error) {
                    reject(error)
                } else {
                    let model = JSON.parse(text)
                    resolve(model)
                }
            })
        })
    }

    removeUserModel (id) {
        return new Promise((resolve, reject) => {
            this.blobService.deleteBlobIfExists(USER_MODELS, id, {}, (error, removed) => {
                if (error) {
                    reject(error)
                } else {
                    console.log('User model removed: ' + id)
                    resolve(removed)
                }
            })
        })
    }

    getUserModelUrl (id) {
        var url = this.blobService.getUrl(USER_MODELS, id)
        return url
    }

    storeGeneratedModel (model) {
        // TODO remove old models
        let content = JSON.stringify(model, null, 2)

        let id = uuid()
        return new Promise((resolve, reject) => {
            this.blobService.createBlockBlobFromText(GENERATED_MODELS, id, content, {}, (error, response) => {
                if (error) {
                    reject(error)
                } else {
                    console.log('Generated model stored: ' + id)
                    let url = this.blobService.getUrl(GENERATED_MODELS, id)
                    resolve(url)
                }
            })
        })
    }
}
