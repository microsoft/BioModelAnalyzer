using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table.DataServices;

namespace BioCheck.Web.Log
{
    public class ErrorDataSource
    {
        public const string ErrorTableName = "ErrorTable";

        private readonly TableServiceContext context;

        public ErrorDataSource()
        {
            //var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=biocheckstorage;AccountKey=G8QXrBKlXVjA6j8iT8nAeMRgAUCQyTfMoETLIcRQVvvfdMy+qga16iRU7LG4GYLLnHjzdLUp1miJPiD3IAFo/A==");
            
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create the table if it doesn't exist.
            var tableClient = storageAccount.CreateCloudTableClient();
            var cloudTable = tableClient.GetTableReference(ErrorTableName);
            cloudTable.CreateIfNotExists();

            this.context = tableClient.GetTableServiceContext();
        }

        public IEnumerable<ErrorDataModel> Select()
        {
            var results = from ldm in context.CreateQuery<ErrorDataModel>(ErrorTableName)
                          select ldm;

            var query = results.AsTableServiceQuery(context);
            var queryResults = query.Execute();

            return queryResults;
        }

        public void Update(ErrorDataModel itemToUpdate)
        {
            context.AttachTo(ErrorTableName, itemToUpdate, "*");
            context.UpdateObject(itemToUpdate);
            context.SaveChanges();
        }

        public void Delete(ErrorDataModel itemToDelete)
        {
            context.AttachTo(ErrorTableName, itemToDelete, "*");
            context.DeleteObject(itemToDelete);
            context.SaveChanges();
        }

        public void Insert(ErrorDataModel newItem)
        {
            context.AddObject(ErrorTableName, newItem);
            context.SaveChanges();
        }
    }
}