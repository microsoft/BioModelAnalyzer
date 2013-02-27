using System;
using System.Windows.Input;
using BioCheck.ViewModel.Cells;

namespace BioCheck.Views
{
    public class RelationshipEventArgs : EventArgs
    {
        private readonly RelationshipTypes type;
        private readonly VariableViewModel variableViewModel;
        private readonly MouseButtonEventArgs mouseArgs;

        public RelationshipEventArgs(RelationshipTypes type, VariableViewModel variableViewModel, MouseButtonEventArgs mouseArgs)
        {
            this.type = type;
            this.variableViewModel = variableViewModel;
            this.mouseArgs = mouseArgs;
        }

        public MouseButtonEventArgs MouseArgs
        {
            get { return mouseArgs; }
        }

        public VariableViewModel VariableViewModel
        {
            get { return variableViewModel; }
        }

        public RelationshipTypes Type
        {
            get { return type; }
        }
    }
}