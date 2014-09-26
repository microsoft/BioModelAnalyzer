using System.Collections.Generic;
using System.Diagnostics;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Editing;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.Controls
{
    public class ContainerSiteContextBarViewModel
    {
        private readonly ContainerSite containerSite;
        private ContainerGrid containerGrid;

        public ActionCommand CutCommand { get; private set; }
        public ActionCommand CopyCommand { get; private set; }
        public ActionCommand PasteCommand { get; private set; }
        public ActionCommand DeleteCommand { get; private set; }

        public ContainerSiteContextBarViewModel(ContainerSite containerSite)
        {
            this.containerSite = containerSite;

            CutCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);
            CopyCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);
            DeleteCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);

            PasteCommand = new ActionCommand(arg => OnPaste(), arg => CanPaste());
        }

        private ContainerGrid Grid
        {
            get
            {
                // Get the parent ContainerGrid
                if (this.containerGrid == null)
                    this.containerGrid = SilverlightVisualTreeHelper.TryFindParent<ContainerGrid>(this.containerSite);

                return this.containerGrid;
            }
        }

        public bool CanPaste()
        {
            if (CopyPasteManager.CanPaste(null))
            {
                var containerVM = CopyPasteManager.Clipboard as ContainerViewModel;
                if (containerVM != null)
                {
                    bool canPaste = false;

                    if (containerVM.SizeOne)
                    {
                        canPaste = containerSite.IsEmpty();
                    }
                    else if (containerVM.SizeTwo)
                    {
                        // It's a 2x2 size container. Need to check the 3 cells around it
                        // If this cell is to be the top left, we need to set the drop states of the other 3 neighbouring sites
                        canPaste = CanPaste(new List<ContainerSite>
                                               {
                                                   Grid.GetContainerSite(containerSite.PositionX + 1, containerSite.PositionY),
                                                   Grid.GetContainerSite(containerSite.PositionX, containerSite.PositionY + 1),
                                                   Grid.GetContainerSite(containerSite.PositionX + 1, containerSite.PositionY + 1),
                                               });
                    }
                    else if (containerVM.SizeThree)
                    {
                        // It's a 3x3 size container. Need to treat this one as the middle cell and check the 8 cells around it
                        // If this cell is to be the centre, we need to set the drop states of the other 8 neighbouring sites
                        canPaste = CanPaste(new List<ContainerSite>
                                                   {
                                                       Grid.GetContainerSite(containerSite.PositionX - 1, containerSite.PositionY - 1),
                                                       Grid.GetContainerSite(containerSite.PositionX, containerSite.PositionY - 1),
                                                       Grid.GetContainerSite(containerSite.PositionX + 1, containerSite.PositionY - 1),
                                                       Grid.GetContainerSite(containerSite.PositionX - 1, containerSite.PositionY),
                                                       Grid.GetContainerSite(containerSite.PositionX + 1, containerSite.PositionY),
                                                       Grid.GetContainerSite(containerSite.PositionX - 1, containerSite.PositionY + 1),
                                                       Grid.GetContainerSite(containerSite.PositionX, containerSite.PositionY + 1),
                                                       Grid.GetContainerSite(containerSite.PositionX + 1, containerSite.PositionY + 1),
                                                   });
                    }

                    return canPaste;
                }
                else
                {
                    var variableVM = CopyPasteManager.Clipboard as VariableViewModel;
                    if (variableVM != null)
                    {
                        if (variableVM.Type == VariableTypes.Constant && !containerSite.HasContainerOverlapping())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool CanPaste(List<ContainerSite> neighbourCells)
        {
            bool canPaste = true;

            neighbourCells.ForEach(neighbour =>
            {
                if (neighbour == null)
                {
                    canPaste = false;
                }
                else
                {
                    if (!neighbour.IsEmpty())
                    {
                        canPaste = false;
                    }
                }
            });

            if (!containerSite.IsEmpty())
            {
                canPaste = false;
            }

            return canPaste;
        }

        private void OnPaste()
        {
            var containerVM = CopyPasteManager.Clipboard as ContainerViewModel;
            if (containerVM != null)
            {
                ContainerViewModel newContainerVM = null;

                ApplicationViewModel.Instance.DupActiveModel();

                if (containerVM.SizeOne)
                {
                    newContainerVM = ApplicationViewModel.Instance.ActiveModel.NewContainer(containerSite.PositionX, containerSite.PositionY);

                }
                else if (containerVM.SizeTwo)
                {
                    newContainerVM = ApplicationViewModel.Instance.ActiveModel.NewContainer(containerSite.PositionX, containerSite.PositionY);

                }
                else if (containerVM.SizeThree)
                {
                    newContainerVM = ApplicationViewModel.Instance.ActiveModel.NewContainer(containerSite.PositionX - 1, containerSite.PositionY - 1);
                }

                containerSite.Dispatcher.BeginInvoke(() => CopyPasteManager.Paste(newContainerVM));
            }
            else
            {
                var variableVM = CopyPasteManager.Clipboard as VariableViewModel;
                if (variableVM != null)
                {
                    if (variableVM.Type == VariableTypes.Constant && !containerSite.HasContainerOverlapping())
                    {
                        ApplicationViewModel.Instance.DupActiveModel();

                        var constantVM =
                                ApplicationViewModel.Instance.ActiveModel.NewConstant(containerSite.PositionX, containerSite.PositionY,
                                                                      variableVM.PositionX, variableVM.PositionY);
                        containerSite.Dispatcher.BeginInvoke(() => CopyPasteManager.Paste(constantVM));
                    }
                }
            }

            ApplicationViewModel.Instance.SaveActiveModel();
        }
    }
}
