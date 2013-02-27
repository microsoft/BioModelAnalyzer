using System;
using System.IO.IsolatedStorage;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel
{
    public class ApplicationSettings
    {
        /// <summary>
        /// Static - The name of the default model
        /// </summary>
        public const string DefaultModel = "Default Model";

        /// <summary>
        /// Static - The name of the directory in Isolated Storage where the models are stored.
        /// </summary>
        public const string DirectoryName = "Models";

        /// <summary>
        /// Static - The width of the grid cells, used to calculate the Left and Top positions of containers and variables
        /// </summary>
        public const int CellWidth = 360;

        /// <summary>
        /// Static - The height of the grid cells
        /// </summary>
        public const int CellHeight = 400;

        /// <summary>
        /// Static - default setting for EnableLogging
        /// </summary>
        public const bool DefaultLogging = false;
        

        /// <summary>
        /// Gets or sets the active model last opened by the user.
        /// </summary>
        /// <value>
        /// The active model.
        /// </value>
        public string ActiveModel
        {
            get { return GetValue<string>("ActiveModel", DefaultModel); }
            set { SetValue("ActiveModel", value); }
        }

        /// <summary>
        /// Gets or sets whether logging should be enabled or not.
        /// </summary>
        public bool EnableLogging
        {
            get { return GetValue<bool>("EnableLogging", DefaultLogging); }
            set { SetValue("EnableLogging", value); }
        }

        private T GetValue<T>(string name, T defaultValue)
        {
            T value;

            //try to get value from isolated storage
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(name, out value))
            {
                //not set yet
                value = defaultValue;
                IsolatedStorageSettings.ApplicationSettings[name] = value;
            }

            return value;
        }

        private void SetValue<T>(string name, T value)
        {
            //save value to isolated storage
            IsolatedStorageSettings.ApplicationSettings[name] = value;
        }

        public void Save()
        {
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
    }
}