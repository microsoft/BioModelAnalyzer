// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

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
    getUserModel (id: string): Promise.IThenable<BMA.ModelFile>
    getUserModelUrl (id: string): string
    removeUserModel (id: string): Promise.IThenable<boolean>
    storeGeneratedModel (model: BMA.ModelFile): Promise.IThenable<string>
}

/**
 * Stores BMA models in Azure Blob Storage and generates public URLs for them.
 */
export class BlobModelStorage implements ModelStorage {
    private blobService

    constructor () {
        this.blobService = azure.createBlobService(config.get<string>('AZURE_STORAGE_ACCOUNT'), config.get<string>('AZURE_STORAGE_ACCESS_KEY'))

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

    storeUserModel (id: string, model: BMA.ModelFile) {
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

    getUserModel (id: string) {
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

    removeUserModel (id: string) {
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

    getUserModelUrl (id: string) {
        var url = this.blobService.getUrl(USER_MODELS, id)
        return url
    }

    storeGeneratedModel (model: BMA.ModelFile) {
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
