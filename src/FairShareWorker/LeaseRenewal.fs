namespace bma.Cloud

open Microsoft.WindowsAzure.Storage.Queue

module Leasing =
    let renewLease (queue : CloudQueue) (message : CloudQueueMessage) visibilityTimeout =
        queue.UpdateMessage(message, visibilityTimeout, MessageUpdateFields.Visibility)

