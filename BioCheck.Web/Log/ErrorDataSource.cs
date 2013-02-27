using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace BioCheck.Web.Log
{
    public class ErrorDataSource
    {
        public const string ErrorTableName = "ErrorTable";

        private readonly TableServiceContext context;

        public ErrorDataSource()
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create the tables
            // In this case, just a single table.  
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.CreateTableIfNotExist(ErrorTableName);

            this.context = tableClient.GetDataServiceContext();
        }

        public IEnumerable<ErrorDataModel> Select()
        {
            var results = from ldm in context.CreateQuery<ErrorDataModel>(ErrorTableName)
                          select ldm;

            var query = results.AsTableServiceQuery<ErrorDataModel>();
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