using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace BioCheck.Web.Log
{
    public class UsuageDataSource
    {
        public const string UsuageTableName = "UsuageTable";

        private readonly TableServiceContext context;

        public UsuageDataSource()
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create the tables
            // In this case, just a single table.  
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.CreateTableIfNotExist(UsuageTableName);

            this.context = tableClient.GetDataServiceContext();
        }

        public IEnumerable<UsuageDataModel> Select()
        {
            var results = from ldm in context.CreateQuery<UsuageDataModel>(UsuageTableName)
                          select ldm;

            var query = results.AsTableServiceQuery<UsuageDataModel>();
            var queryResults = query.Execute();

            return queryResults;
        }

        public void Update(UsuageDataModel itemToUpdate)
        {
            context.AttachTo(UsuageTableName, itemToUpdate, "*");
            context.UpdateObject(itemToUpdate);
            context.SaveChanges();
        }

        public void Delete(UsuageDataModel itemToDelete)
        {
            context.AttachTo(UsuageTableName, itemToDelete, "*");
            context.DeleteObject(itemToDelete);
            context.SaveChanges();
        }

        public void Insert(UsuageDataModel newItem)
        {
            context.AddObject(UsuageTableName, newItem);
            context.SaveChanges();
        }
    }
}