import * as azure from 'azure-storage'
import * as config from 'config'

const USER_MODELS = 'user_models'

export default class Storage {
    blobService
    constructor () {
        this.blobService = azure.createBlobService(config.get('AZURE_STORAGE_ACCOUNT'), config.get('AZURE_STORAGE_ACCESS_KEY'))
    }

    /** Called on application start */
    init () {
        this.blobService.createContainerIfNotExists(USER_MODELS, {
            publicAccessLevel: 'blob'
        }, (error, result, response) => {
            if (error) {
                throw error
            }
        })
    }

    storeUserModel (id, stream, len) {
        // TODO handle errors, use Promise
        // see https://github.com/Azure/azure-storage-node/blob/master/lib/services/blob/blobservice.js
        this.blobService.createBlockBlobFromStream(USER_MODELS, id, stream, len, {})
    }

    getUserModel (id) {
        // TODO implement
    }

    getUserModelUrl (id) {
        var sasToken = this.blobService.generateSharedAccessSignature(USER_MODELS, id, {
            AccessPolicy: { Expiry: azure.date.minutesFromNow(60) }
        })
        var sasUrl = this.blobService.getUrl(USER_MODELS, id, sasToken, true)
        return sasUrl
    }
}
