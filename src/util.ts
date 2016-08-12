import * as config from 'config'
import * as mime from 'mime'
import * as builder from 'botbuilder'

/**
 * Returns the public url of a file in the /public folder.
 * 
 * @param path Path relative to /public, e.g. 'tutorials/img/foo.png'
 */
export function getPublicResourceUrl (path: string) {
    let host = config.get('HOSTNAME')
    let port = config.get<string>('PORT')
    if (isNaN(parseInt(port))) {
        port = null
    }
    let url = 'https://' + host + (port ? ':' + port : '') + '/' + path
    return url
}

/**
 * Returns an IAttachment for a file in the /public/tutorials/img folder.
 * 
 * @param filename The image filename, e.g. 'foo.png'
 */
export function getTutorialImageAttachment (filename: string): builder.IAttachment {
    return {
        contentUrl: getPublicResourceUrl('tutorials/img/' + filename),
        contentType: mime.lookup(filename)
    }
}

/**
 * Returns an IAttachment for a file in the /public/tutorials/model folder.
 * 
 * @param filename The models filename, e.g. 'ecoli.json'
 */
export function getTutorialModelAttachment (filename: string): builder.IAttachment {
    return {
        contentUrl: getPublicResourceUrl('tutorials/model/' + filename),
        contentType: 'application/octet-stream'
    }
}