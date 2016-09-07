// from https://github.com/Microsoft/BotBuilder/issues/662#issuecomment-232223965

import * as async from 'async'
import * as url from 'url'
import * as request from 'request'

export function downloadAttachments(connector, message, callback) {
    var attachments = []
    var containsSkypeUrl = false
    message.attachments.forEach(function (attachment) {
        if (attachment.contentUrl) {
            attachments.push({
                contentType: attachment.contentType,
                contentUrl: attachment.contentUrl
            })
            if (url.parse(attachment.contentUrl).hostname.substr(-'skype.com'.length) == 'skype.com') {
                containsSkypeUrl = true
            }
        }
    })
    if (attachments.length > 0) {
        async.waterfall([
            function (cb) {
                if (containsSkypeUrl) {
                    connector.getAccessToken(cb)
                }
                else {
                    cb(null, null)
                }
            }
        ], function (err, token) {
            if (!err) {
                var buffers = []
                async.forEachOf(attachments, function (item, idx, cb) {
                    var contentUrl = item.contentUrl
                    var headers = {}
                    if (url.parse(contentUrl).hostname.substr(-'skype.com'.length) == 'skype.com') {
                        headers['Authorization'] = 'Bearer ' + token
                        headers['Content-Type'] = 'application/octet-stream'
                    }
                    else {
                        headers['Content-Type'] = item.contentType
                    }
                    request({
                        url: contentUrl,
                        headers: headers,
                        encoding: null
                    }, function (err, res, body) {
                        if (!err && res.statusCode == 200) {
                            buffers.push(body)
                        }
                        cb(err)
                    })
                }, function (err) {
                    if (callback)
                        callback(err, buffers)
                })
            }
            else {
                if (callback)
                    callback(err, null)
            }
        })
    }
    else {
        if (callback)
            callback(null, null)
    }
}
