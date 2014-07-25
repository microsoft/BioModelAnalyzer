using System.Diagnostics;
using BioCheck.ViewModel.Cells;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.Controls
{
    public class RelationshipContextBarViewModel
    {
        private readonly RelationshipViewModel relationshipVM;

        public ActionCommand CutCommand { get; private set; }
        public ActionCommand CopyCommand { get; private set; }
        public ActionCommand PasteCommand { get; private set; }
        public ActionCommand DeleteCommand { get; private set; }

        public RelationshipContextBarViewModel(RelationshipViewModel relationshipVM)
        {
            this.relationshipVM = relationshipVM;

            CutCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);
            CopyCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);
            PasteCommand = new ActionCommand(arg => Debug.WriteLine(arg), arg => false);

            DeleteCommand = new ActionCommand(arg => relationshipVM.DeleteCommand.Execute());
        }
    }
}
