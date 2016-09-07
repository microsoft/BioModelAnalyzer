import * as config from 'config'
import * as mime from 'mime'
import * as builder from 'botbuilder'

/** A-Z */
export const LETTERS: string[] = Array.apply(0, Array(26)).map((x, y) => String.fromCharCode(65 + y))

/** FA-ZZ, used as auto-generated names for stored formulas */
export const LETTERS_F: string[] = [].concat(...LETTERS.slice(5).map(a => LETTERS.map(b => a + b)))

const STATIC_URL_PREFIX = 'static/'

/**
 * Returns the public url of a file in the /public folder.
 * 
 * @param path Path relative to /public, e.g. 'tutorials/img/foo.png'
 */
function getPublicResourceUrl (path: string) {
    let host = config.get('HOSTNAME')
    let port = config.get<string>('PORT')
    // if deployed, then the port is the internal port which is a named pipe, so we ignore that
    if (isNaN(parseInt(port))) {
        port = null
    }
    let url = config.get('PROTOCOL') + '://' + host + (port ? ':' + port : '') + '/' + path
    return url
}

/**
 * Returns an IAttachment for a file in the /public/tutorials/img folder.
 * 
 * @param filename The image filename, e.g. 'foo.png'
 */
export function getTutorialImageAttachment (filename: string): builder.IAttachment {
    return {
        contentUrl: getPublicResourceUrl(STATIC_URL_PREFIX + 'tutorials/img/' + filename),
        contentType: mime.lookup(filename)
    }
}

/**
 * Returns the public URL of a tutorial model file.
 * 
 * @param filename The model's filename, e.g. 'ecoli.json'
 */
export function getTutorialModelUrl (filename: string): string {
    return getPublicResourceUrl(STATIC_URL_PREFIX + 'tutorials/model/' + filename)
}

/**
 * Returns a BMA URL which opens the given model URL.
 * 
 * @param modelUrl The URL of the model that BMA should open.
 */
export function getBMAModelUrl (modelUrl: string): string {
    return config.get('BMA_URL') + '?Model=' + modelUrl
}
