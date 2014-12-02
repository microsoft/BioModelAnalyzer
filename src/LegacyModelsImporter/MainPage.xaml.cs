using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LegacyModelsImporter
{
    public partial class MainPage : UserControl
    {
        private string[] modelFiles;

        public MainPage()
        {
            InitializeComponent();

            try 
            { 
			    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
				    if (storage.DirectoryExists("Models"))
				    {
					    modelFiles = storage.GetFileNames("Models\\*.xml");
					    if (modelFiles.Length > 0) 
					    {
                            errorMessage.Visibility = System.Windows.Visibility.Collapsed;
                            LayoutRoot.Children.Remove(errorMessage);
                            importModelsLink.Visibility = System.Windows.Visibility.Visible;
					    }
				    }                
		    }
            catch(Exception exc)
            {
                errorMessage.Text = String.Format("Legacy models error: {0}", exc.Message);
            }
        }

        private void importModelsLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog zipDialog;
                InitializeComponent();
                zipDialog = new SaveFileDialog();
                zipDialog.Filter = "Zip Files | *.zip";
                zipDialog.DefaultExt = "zip";
                zipDialog.DefaultFileName = "models.zip";

                bool? result = zipDialog.ShowDialog();
                if (result == true)
                {
                    System.IO.Stream fileStream = zipDialog.OpenFile();
                    Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile();
    			    using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                        foreach (var fileName in modelFiles)
                        {
                            using (var modelStream = storage.OpenFile(String.Concat("Models\\", fileName), FileMode.Open)) {
                                var modelReader = new BinaryReader(modelStream);
                                z.AddEntry(fileName, modelReader.ReadBytes((int)modelStream.Length));
                            }
                        }
                    z.Save(fileStream);
                    fileStream.Flush();
                    fileStream.Close();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(String.Format("Error extracting legacy models {0}", exc.Message), "Error", MessageBoxButton.OK);
            }
        }
    }
}
