using System;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Factories
{
    /// <summary>
    /// ViewModelFactory for creating and populating a new ModelViewModel
    /// </summary>
    public static class NewModelFactory 
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
                Name = NameFactory.Create("New Model"),
                Description = "This is a new model.",
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
