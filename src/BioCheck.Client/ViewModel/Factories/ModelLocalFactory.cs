using System;
using BioCheck.Services;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.XML;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Factories;
using System.Linq;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Factories
{
    /// <summary>
    /// ViewModelFactory for creating and populating a ModelViewModel from
    /// local data in isolated storage for offline/local mode.
    /// </summary>
    public class ModelLocalFactory : ViewModelFactory<ModelViewModel>
    {
        /// <summary>
        /// Create, populate and return the ModelViewModel object
        /// </summary>
        /// <returns></returns>
        protected override ModelViewModel OnCreate(ICreateCriteria criteria)
        {
            string modelName = criteria.Key;

            try
            {
                var library = ApplicationViewModel.Instance.Library;
                var modelVM = library.Models.First(mvm => mvm.Name == modelName);

                if (modelVM == null)
                {
                    // Throw an exception as we shouldn't get here
                    throw new Exception("Missing Model: " + modelName);
                }

                string fileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, modelName);

                var xdoc = IsolatedStorageHelper.LoadXDocument(fileName);
                try
                {
                    ModelXmlFactory.Load(xdoc, modelVM);
                }
                catch (Exception)
                {
                    string details = xdoc.ToString();

                    ApplicationViewModel.Instance.Container
                        .Resolve<IInvalidModelWindowService>()
                        .Show(details, shouldDelete =>
                                           {
                                               if(shouldDelete)
                                               {
                                                   IsolatedStorageHelper.DeleteFile(fileName);
                                               }
                                           });
                }
                return modelVM;
            }
            finally
            {
                ApplicationViewModel.Instance.IsLoading = false;
            }
        }
    }
}
