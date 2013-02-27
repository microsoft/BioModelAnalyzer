using System;
using BioCheck.Services;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.XML;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using Microsoft.Practices.Unity;
using MvvmFx.Common.ViewModels.Factories;
using System.Linq;

namespace BioCheck.ViewModel.Factories
{
    /// <summary>
    /// ViewModelFactory for creating and populating a LibraryViewModel from
    /// local data in isolated storage for offline/local mode.
    /// </summary>
    public class LibraryLocalFactory : ViewModelFactory<LibraryViewModel>
    {
        /// <summary>
        /// Create, populate and return the LibraryViewModel object
        /// </summary>
        /// <returns></returns>
        protected override LibraryViewModel OnCreate()
        {
            // Get all the model XML files in the local data directory in Isolated Storage
            var fileNames = IsolatedStorageHelper.GetFileNames(ApplicationSettings.DirectoryName);

            // For each file name in the Models directory, load the xml to an XDocument
            var xmls = (from f in fileNames
                              let name = f.Replace(".xml", "")
                              let path = string.Format(@"{0}\{1}", ApplicationSettings.DirectoryName, f)
                              let xdoc = IsolatedStorageHelper.LoadXDocument(path)
                              select new { XDoc = xdoc, Path = path })
                        .ToList();

            var libraryVM = new LibraryViewModel();

            if (xmls.Count == 0)
            {
                // Create a default model
                var defaultModelVM = DefaultModelFactory.Create();
                libraryVM.Models.Add(defaultModelVM);

                // Save it
                ApplicationViewModel.Instance.Container
                            .Resolve<IViewModelSaver<ModelViewModel>>()
                            .Save(defaultModelVM);

                return libraryVM;
            }

            xmls.ForEach(xml =>
                              {
                                  try
                                  {
                                      var modelVM = ModelXmlFactory.Create(xml.XDoc);
                                      libraryVM.Models.Add(modelVM);
                                  }
                                  catch (Exception)
                                  {
                                      string details = xml.XDoc.ToString();

                                          ApplicationViewModel.Instance.Container
                                            .Resolve<IInvalidModelWindowService>()
                                             .Show(details, shouldDelete =>
                                             {
                                                 if (shouldDelete)
                                                 {
                                                     IsolatedStorageHelper.DeleteFile(xml.Path);
                                                 }

                                                 // Log the error to the Log web service
                                                 ApplicationViewModel.Instance.Log.Error("There was an error opening an invalid model", details);
                                             });
                                  }
                              });

            return libraryVM;
        }
    }
}
