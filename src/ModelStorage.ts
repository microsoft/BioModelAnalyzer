import * as Promise from 'promise'
import * as azure from 'azure-storage'
import * as config from 'config'

const USER_MODELS = 'usermodels'

export interface ModelStorage {
    storeUserModel (id: string, content: string): Promise.IThenable<boolean>
    getUserModel (id: string): Promise.IThenable<any>
    getUserModelUrl (id: string): string
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
            var serviceProperties = result
            serviceProperties.Cors = {
                CorsRule: [{
                    AllowedOrigins: [config.get('BMA_HOST')],
                    AllowedMethods: ['GET'],
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
        
        // create container
        this.blobService.createContainerIfNotExists(USER_MODELS, {
            publicAccessLevel: 'blob'
        }, (error, result, response) => {
            if (error) {
                throw error
            }
        })
    }

    storeUserModel (id, content) {
        // TODO think about expiration

        return new Promise((resolve, reject) => {
            this.blobService.createBlockBlobFromText(USER_MODELS, id, content, {}, (error, response) => {
                if (error) {
                    reject(error)
                } else {
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

    getUserModelUrl (id) {
        /*
        var sasToken = this.blobService.generateSharedAccessSignature(USER_MODELS, id, {
            AccessPolicy: { Expiry: azure.date.minutesFromNow(60) }
        })
        var sasUrl = this.blobService.getUrl(USER_MODELS, id, sasToken, true)
        */
        var sasUrl = this.blobService.getUrl(USER_MODELS, id)
        return sasUrl
    }
}
