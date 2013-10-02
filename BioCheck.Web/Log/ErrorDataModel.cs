using System;
using System.Data.Services.Common;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace BioCheck.Web.Log
{
    // This originally derives from TableEntity but, as
    // http://stackoverflow.com/questions/14034699/tableservicecontext-cant-cast-to-unsupported-type-datetimeoffset-exception-w
    // points out, this seems to be mixing v1 and v2 storage access.
    // Since the caller is using v1, I've reverted this class.

    [DataServiceEntity]
    public class ErrorDataModel : TableServiceEntity
    {
        public ErrorDataModel(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {

        }

        public ErrorDataModel()
            : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {

        }

        public string UserId { get; set; }

        public DateTime Date { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }

        public string Version { get; set; }
    }
}
