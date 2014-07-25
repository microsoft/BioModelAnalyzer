using System;
using System.ServiceModel;
using BioCheck.LogService;

namespace BioCheck.ViewModel
{
    public class ApplicationLog
    {
        private LogServiceClient logClient;

        private string ipHash;
        private UsuageDataModel usuageDM;

        public ApplicationLog()
        {
            // Create the client proxy to the Log web service
            var serviceUri = new Uri("../Services/LogService.svc", UriKind.Relative);
            var endpoint = new EndpointAddress(serviceUri);
            logClient = new LogServiceClient("LogServiceCustom", endpoint);
            logClient.LogInCompleted += logClient_LogInCompleted;
        }

        public void Login()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (string.IsNullOrEmpty(ipHash))
            {
                //string userIP = Guid.NewGuid().ToString();
                string userIP = ApplicationViewModel.Instance.User.IPAddress;
                ipHash = userIP.GetHashCode().ToString();
            }

            this.logClient.LogInAsync(ipHash, Version.ToString());
        }

        void logClient_LogInCompleted(object sender, LogInCompletedEventArgs e)
        {
            this.usuageDM = e.Result;
        }

        public void Error(string message, string details)
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (string.IsNullOrEmpty(ipHash))
            {
                //string userIP = Guid.NewGuid().ToString();
                string userIP = ApplicationViewModel.Instance.User.IPAddress;
                ipHash = userIP.GetHashCode().ToString();
            }

            this.logClient.ErrorAsync(this.ipHash, Version.ToString(), message, details);
        }

        public void RunProof()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.RunProof++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }

        public void RunSimulation()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.RunSimulation++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }

        public void NewModel()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.NewModel++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }

        public void SaveModel()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.SaveModel++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }

        public void ImportModel()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.ImportModel++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }

        public void FurtherTesting()
        {
            if (App.IsRunningUnderDebugOrLocalhost)
                return;

            if (this.usuageDM != null)
            {
                this.usuageDM.FurtherTesting++;
                this.logClient.UpdateAsync(this.usuageDM);
            }
        }
    }
}