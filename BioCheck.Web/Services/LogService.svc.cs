using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using BioCheck.Web.Log;

namespace BioCheck.Web.Services
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class LogService
    {
        [OperationContract]
        public UsuageDataModel LogIn(string userId, string version)
        {
            var dataSource = new UsuageDataSource();

            var logDM = new UsuageDataModel()
            {
                UserId = userId,
                LogInTime = DateTime.Now,
                LogOutTime = DateTime.Now,
                Version = version,
            };

            dataSource.Insert(logDM);

            return logDM;
        }

        [OperationContract]
        public void Update(UsuageDataModel logDM)
        {
            logDM.LogOutTime = DateTime.Now;
            logDM.Duration = Convert.ToInt32((logDM.LogOutTime - logDM.LogInTime).TotalMinutes);

            var dataSource = new UsuageDataSource();
            dataSource.Update(logDM);
        }

        [OperationContract]
        public void Error(string userId, string version, string message, string details)
        {
            var dataSource = new ErrorDataSource();

            var errorDM = new ErrorDataModel()
            {
                UserId = userId,
                Date = DateTime.Now,
                Message = message,
                Details = details,
                Version = version,
            };

            dataSource.Insert(errorDM);
        }
    }
}
