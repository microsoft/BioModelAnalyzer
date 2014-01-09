using System;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Factories
{
    /// <summary>
    /// ViewModelFactory for creating and populating a new ModelViewModel
    /// </summary>
    public static class DefaultModelFactory 
    {
        /// <summary>
        /// Create, populate and return the new ModelViewModel object
        /// </summary>
        /// <returns></returns>
        public static ModelViewModel Create()
        {
            var date = DateTime.Now;

            var modelVM = new ModelViewModel()
            {
                Id = 1,
                Name = ApplicationSettings.DefaultModel,
                Description = "This is the default model.",
                // TODO - add author name
                //Author  = 1
                CreatedDate = date,
                ModifiedDate = date,
                IsLoaded = true
            };

            return modelVM;
        }
    }
}
