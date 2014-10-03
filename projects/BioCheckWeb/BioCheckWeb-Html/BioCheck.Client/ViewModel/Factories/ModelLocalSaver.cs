using BioCheck.Services;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.XML;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Factories
{
    /// <summary>
    /// ViewModelSaver for saving a ModelViewModel object to a local XML data file in Isolated Storage
    /// </summary>
    public class ModelLocalSaver : ViewModelSaver<ModelViewModel>
    {
        protected override void OnSave(ModelViewModel modelVM)
        {
            // Convert the ModelViewModel to an XDocument
            var xdoc = ModelXmlSaver.SaveToXml(modelVM);

            string fileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, modelVM.Name);

            // Save to isolated storage
            IsolatedStorageHelper.SaveXDocument(fileName, xdoc);
        }
    }
}
