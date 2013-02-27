using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using System.Xml.Linq;
using BioCheck.Services;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.XML;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using MvvmFx.Common.ViewModels.Factories;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Models
{
    public class LibraryViewModel : ObservableViewModel
    {
        private readonly ObservableViewModelCollection<ModelViewModel> models;
        private ModelViewModel selectedModel;

        private readonly DelegateCommand newCommand;
        private readonly DelegateCommand openCommand;
        private readonly DelegateCommand duplicateCommand;
        private readonly DelegateCommand deleteCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand exportXmlCommand;
        private readonly DelegateCommand importXmlCommand;

        public LibraryViewModel()
        {
            this.models = new ObservableViewModelCollection<ModelViewModel>();

            this.newCommand = new DelegateCommand(OnNewExecuted);
            this.openCommand = new DelegateCommand(OnOpenExecuted);
            this.duplicateCommand = new DelegateCommand(OnDuplicateExecuted);
            this.deleteCommand = new DelegateCommand(OnDeleteExecuted);
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.exportXmlCommand = new DelegateCommand(OnExportXmlExecuted);
            this.importXmlCommand = new DelegateCommand(OnImportXmlExecuted);
        }

        /// <summary>
        /// Gets the value of the <see cref="Models"/> property.
        /// </summary>
        public ObservableViewModelCollection<ModelViewModel> Models
        {
            get { return models; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedModel"/> property.
        /// </summary>
        public ModelViewModel SelectedModel
        {
            get { return this.selectedModel; }
            set
            {
                if (this.selectedModel != value)
                {
                    this.selectedModel = value;
                    OnPropertyChanged(() => SelectedModel);
                    OnPropertyChanged(() => HasSelectedModel);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="HasSelectedModel"/> property.
        /// </summary>
        public bool HasSelectedModel
        {
            get { return this.selectedModel != null; }
        }

        /// <summary>
        /// Gets the value of the <see cref="NewCommand"/> property.
        /// </summary>
        public DelegateCommand NewCommand
        {
            get { return this.newCommand; }
        }

        private void PromptToSave(Action onContinue)
        {
            // TODO - turn on Prompt To Save once an IsModified property has been set
            // Save the changes and continue
            ApplicationViewModel.Instance.ToolbarViewModel.SaveCommand.Execute();
            onContinue.Invoke();

            //// Prompt the user and save the model if they confirm
            //string message = "Do you want to save the current model?" + Environment.NewLine + Environment.NewLine;
            //message += "Click Yes to save any current changes and continue. " + Environment.NewLine +
            //          "Click No to lose any current changes and continue." + Environment.NewLine +
            //          "Or click Cancel to keep any current changes and not continue.";

            //ApplicationViewModel.Instance.Container
            //                            .Resolve<IMessageWindowService>()
            //                            .Show(message, MessageType.YesNoCancel, result =>
            //                            {
            //                                if (result == MessageResult.Yes)
            //                                {
            //                                    // Save the changes and continue
            //                                    ApplicationViewModel.Instance.ToolbarViewModel.SaveCommand.Execute();
            //                                    onContinue.Invoke();
            //                                }
            //                                else if (result == MessageResult.No)
            //                                {
            //                                    // Just continue
            //                                    onContinue.Invoke();
            //                                }
            //                            });
        }

        private void OnNewExecuted()
        {
            // Save changes in the current model
            if (ApplicationViewModel.Instance.HasActiveModel)
            {
                PromptToSave(() =>
                                 {
                                     ApplicationViewModel.Instance.ToolbarViewModel.ClearProofCommand.Execute();
                                     ApplicationViewModel.Instance.ActiveVariable = null;
                                     ApplicationViewModel.Instance.ActiveContainer = null;
                                     ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                     // Unload the main data
                                     DoUnload(ApplicationViewModel.Instance.ActiveModel);

                                     // Create the new one
                                     DoNew();
                                 });
            }
            else
            {
                DoNew();
            }

        }

        private void DoNew()
        {
            // Create the new ModelVM
            var modelVM = NewModelFactory.Create();
            this.models.Add(modelVM);

            // Save it
            ApplicationViewModel.Instance.Container
                                    .Resolve<IViewModelSaver<ModelViewModel>>()
                                    .Save(modelVM);

            // Set it to the selected model
            this.SelectedModel = modelVM;

            // Activate it
            ApplicationViewModel.Instance.ActiveModel = modelVM;

            // Log the creation of the new model to the Log web service
            ApplicationViewModel.Instance.Log.NewModel();
        }

        /// <summary>
        /// Gets the value of the <see cref="OpenCommand"/> property.
        /// </summary>
        public DelegateCommand OpenCommand
        {
            get { return this.openCommand; }
        }

        private void DoLoad(ModelViewModel modelVM)
        {
            // Open the Model ViewModel for the current model
            var loadedModelVM = ApplicationViewModel.Instance.Container
                                .Resolve<IViewModelFactory<ModelViewModel>>()
                                .Create(new CreateCriteria { Key = modelVM.Name });

            if (loadedModelVM != modelVM)
            {
                throw new Exception("Error loading model");
            }

            modelVM.IsLoaded = true;
        }

        private void DoUnload(ModelViewModel modelVM)
        {
            modelVM.RelationshipViewModels.RemoveAll();

            foreach (var containerVM in modelVM.ContainerViewModels)
            {
                containerVM.VariableViewModels.RemoveAll();
            }

            modelVM.ContainerViewModels.RemoveAll();
            modelVM.VariableViewModels.RemoveAll();

            modelVM.IsLoaded = false;
        }

        private void OnOpenExecuted()
        {
            if (!this.HasSelectedModel)
            {
                ApplicationViewModel.Instance.Container
                  .Resolve<IMessageWindowService>()
                  .Show("Please select a model to open.");
                return;
            }

            if (this.SelectedModel == ApplicationViewModel.Instance.ActiveModel)
            {
                return;
            }

            // Save changes in the current model
            if (ApplicationViewModel.Instance.HasActiveModel)
            {
                PromptToSave(() =>
                                 {
                                     ApplicationViewModel.Instance.ToolbarViewModel.ClearProofCommand.Execute();
                                     ApplicationViewModel.Instance.ActiveVariable = null;
                                     ApplicationViewModel.Instance.ActiveContainer = null;
                                     ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                     // Unload the main data
                                     DoUnload(ApplicationViewModel.Instance.ActiveModel);

                                     DoOpen();
                                 });
            }
            else
            {
                DoOpen();
            }
        }

        private void DoOpen()
        {
            DoLoad(this.selectedModel);
            ApplicationViewModel.Instance.ActiveModel = this.selectedModel;
        }

        /// <summary>
        /// Gets the value of the <see cref="CloseCommand"/> property.
        /// </summary>
        public DelegateCommand CloseCommand
        {
            get { return this.closeCommand; }
        }

        private void OnCloseExecuted()
        {
            ApplicationViewModel.Instance.ToolbarViewModel.ShowLibrary = false;
        }

        /// <summary>
        /// Gets the value of the <see cref="DuplicateCommand"/> property.
        /// </summary>
        public DelegateCommand DuplicateCommand
        {
            get { return this.duplicateCommand; }
        }

        private void OnDuplicateExecuted()
        {
            if (!this.HasSelectedModel)
            {
                ApplicationViewModel.Instance.Container
                  .Resolve<IMessageWindowService>()
                  .Show("Please select a model to duplicate.");
                return;
            }

            // Prompt to save changes in the current model
            // Save changes in the current model
            if (ApplicationViewModel.Instance.HasActiveModel)
            {
                PromptToSave(() =>
                                 {

                                     ApplicationViewModel.Instance.ToolbarViewModel.ClearProofCommand.Execute();
                                     ApplicationViewModel.Instance.ActiveVariable = null;
                                     ApplicationViewModel.Instance.ActiveContainer = null;
                                     ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                     // Unload the main data
                                     DoUnload(ApplicationViewModel.Instance.ActiveModel);

                                     DoDuplicate();
                                 });
            }
            else
            {
                DoDuplicate();
            }
        }

        private void DoDuplicate()
        {
            // Get the source and new duplicate file names
            string sourceFileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, this.SelectedModel.Name);
            string duplicateName = NameFactory.Create(this.SelectedModel.Name);
            string destinationFileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, duplicateName);

            // Copy the source file in isolated storage
            // TODO - this assumes it's offline/local
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.CopyFile(sourceFileName, destinationFileName);
            }

            // Create the new model
            var xdoc = IsolatedStorageHelper.LoadXDocument(destinationFileName);
            var modelVM = ModelXmlFactory.Create(xdoc);

            // Set its new properties
            modelVM.Name = duplicateName;
            modelVM.CreatedDate = DateTime.Now;
            modelVM.ModifiedDate = modelVM.CreatedDate;

            this.models.Add(modelVM);

            // Load the model
            DoLoad(modelVM);

            // Save it
            ApplicationViewModel.Instance.Container
                        .Resolve<IViewModelSaver<ModelViewModel>>()
                        .Save(modelVM);

            // Select it
            this.SelectedModel = modelVM;

            // Open the model
            ApplicationViewModel.Instance.ActiveModel = modelVM;
        }

        /// <summary>
        /// Gets the value of the <see cref="DeleteCommand"/> property.
        /// </summary>
        public DelegateCommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        private void OnDeleteExecuted()
        {
            if (!this.HasSelectedModel)
            {
                ApplicationViewModel.Instance.Container
                  .Resolve<IMessageWindowService>()
                  .Show("Please select a model to delete.");
                return;
            }

            // Prompt the user and clear the model if they confirm
            ApplicationViewModel.Instance.Container
                                        .Resolve<IMessageWindowService>()
                                        .Show(string.Format("Are you sure you want to delete the '{0}' model?", this.selectedModel.Name), MessageType.YesCancel, result =>
                                        {
                                            if (result == MessageResult.Yes)
                                            {
                                                // Remove the active model if this is it
                                                if (ApplicationViewModel.Instance.HasActiveModel && ApplicationViewModel.Instance.ActiveModel == this.selectedModel)
                                                {
                                                    // Clear everything first
                                                    ApplicationViewModel.Instance.ToolbarViewModel.ResetStability(false);

                                                    var modelVM = ApplicationViewModel.Instance.ActiveModel;

                                                    ApplicationViewModel.Instance.ActiveVariable = null;
                                                    ApplicationViewModel.Instance.ActiveContainer = null;
                                                    ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                                    modelVM.Reset();

                                                    // Remove the active model
                                                    ApplicationViewModel.Instance.ActiveModel = null;
                                                }

                                                string sourceFileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, this.selectedModel.Name);

                                                // This assumes it's offline/local
                                                using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
                                                {
                                                    if (storage.FileExists(sourceFileName))
                                                    {
                                                        storage.DeleteFile(sourceFileName);
                                                    }
                                                }

                                                this.models.Remove(this.selectedModel);
                                                this.SelectedModel = null;
                                            }
                                        });

        }

        /// <summary>
        /// Gets the value of the <see cref="ExportXmlCommand"/> property.
        /// </summary>
        public DelegateCommand ExportXmlCommand
        {
            get { return this.exportXmlCommand; }
        }

        private void OnExportXmlExecuted()
        {
            if (!this.HasSelectedModel)
            {
                ApplicationViewModel.Instance.Container
                   .Resolve<IMessageWindowService>()
                   .Show("Please select a model to export.");
                return;
            }

            var modelVM = this.selectedModel;
            if (!modelVM.IsLoaded)
                DoLoad(modelVM);
            ModelXmlSaver.Save(modelVM);
        }

        /// <summary>
        /// Gets the value of the <see cref="ImportXmlCommand"/> property.
        /// </summary>
        public DelegateCommand ImportXmlCommand
        {
            get { return this.importXmlCommand; }
        }

        private void OnImportXmlExecuted()
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            openFileDialog.Multiselect = false;

            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult.Value)
            {
                // Save and close the current model
                if (ApplicationViewModel.Instance.HasActiveModel)
                {
                    PromptToSave(() =>
                                     {
                                         ApplicationViewModel.Instance.ToolbarViewModel.ClearProofCommand.Execute();
                                         ApplicationViewModel.Instance.ActiveVariable = null;
                                         ApplicationViewModel.Instance.ActiveContainer = null;
                                         ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                         // Unload the main data
                                         DoUnload(ApplicationViewModel.Instance.ActiveModel);

                                         DoImport(openFileDialog.File);
                                     });
                }
                else
                {
                    DoImport(openFileDialog.File);
                }
            }
        }

        private void DoImport(FileInfo file)
        {
            var container = ApplicationViewModel.Instance.Container;
            var busyService = container.Resolve<IBusyIndicatorService>();
            busyService.Show("Importing model...");

            using (var stream = file.OpenRead())
            {
                XDocument xdoc = null;
                ModelViewModel importedVM;

                try
                {
                    xdoc = XDocument.Load(stream);
                    importedVM = ModelXmlFactory.Create(xdoc);
                    ModelXmlFactory.Load(xdoc, importedVM);
                }
                catch (Exception)
                {
                    string details = xdoc.ToString();

                    busyService.Close();

                    ApplicationViewModel.Instance.Container
                                .Resolve<IInvalidModelWindowService>()
                                .ShowImportError(details);

                    // Log the error to the Log web service
                    ApplicationViewModel.Instance.Log.Error("There was an error importing an invalid model", details);

                    return;
                }

                string currentName = file.Name.Replace(file.Extension, "");

                importedVM.Name = NameFactory.Create(currentName);
                models.Add(importedVM);

                // Set the Created and Modified dates
                importedVM.CreatedDate = DateTime.Now;
                importedVM.ModifiedDate = importedVM.CreatedDate;

                // Set it to the selected model
                this.SelectedModel = importedVM;

                // Open it
                ApplicationViewModel.Instance.ActiveModel = importedVM;

                // Save it
                DispatcherHelper.DoubleBeginInvoke(() =>
                {
                    var saver = container.Resolve<IViewModelSaver<ModelViewModel>>();
                    saver.Save(importedVM);
                });

                // Log the import of the model to the Log web service
                ApplicationViewModel.Instance.Log.ImportModel();

                busyService.Close();
            }
        }
    }
}