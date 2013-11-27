using MvvmFx.Common.ViewModels;
using System.Windows.Media;
using BioCheck.Helpers;
using System.Collections.Generic;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.ViewModel.Simulation
{
    public class VariableSimViewModel : ViewModelBase
    {
        private string name;
        private string cellname;
        private string range;
        private int rangeFrom;
        private int rangeTo;
        private int intialValue;
        private bool isGraphed;
        private Brush graphColor;
        private bool canModify;
        private bool isModified;

        private readonly DelegateCommand toggleGraphedCommand;
        private readonly DelegateCommand randomizeCommand;

        public VariableSimViewModel()
        {
            this.toggleGraphedCommand = new DelegateCommand(OnToggleGraphedExecuted);
            this.randomizeCommand = new DelegateCommand(OnRandomizedExecuted);

            Steps = new List<SimStepInfo>();
        }

        public List<SimStepInfo> Steps { get; set; }

        public DelegateCommand ToggleGraphedCommand
        {
            get { return this.toggleGraphedCommand; }
        }

        private void OnToggleGraphedExecuted()
        {
            if (this.IsGraphed)
            {
                this.GraphColor = GraphColours.Next();
            }
            else
            {
                this.GraphColor = null;
            }
        }

        public DelegateCommand RandomizeCommand
        {
            get { return this.randomizeCommand; }
        }

        private void OnRandomizedExecuted()
        {
            if (this.rangeFrom != this.rangeTo)
            {
                int randomValue = 0;
                do
                {
                    randomValue = RandomHelper.GetRandom(this.rangeFrom, this.rangeTo);
                }
                while (randomValue == this.intialValue);

                InitialValue = randomValue;
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Name"/> property.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        public string CellName
        {
            get { return this.cellname; }
            set
            {
                if (this.cellname != value)
                {
                    this.cellname = value;
                    OnPropertyChanged(() => CellName);
                }
            }
        }

        public string Range
        {
            get { return this.range; }
            set
            {
                if (this.range != value)
                {
                    this.range = value;
                    OnPropertyChanged(() => Range);
                }
            }
        }

        public int RangeFrom
        {
            get { return this.rangeFrom; }
            set
            {
                if (this.rangeFrom != value)
                {
                    this.rangeFrom = value;
                    OnPropertyChanged(() => RangeFrom);
                }
            }
        }

        public int RangeTo
        {
            get { return this.rangeTo; }
            set
            {
                if (this.rangeTo != value)
                {
                    this.rangeTo = value;
                    OnPropertyChanged(() => RangeTo);
                }
            }
        }

        public bool IsModified
        {
            get { return this.isModified; }
            set
            {
                if (this.isModified != value)
                {
                    this.isModified = value;
                    OnPropertyChanged(() => IsModified);
                }
            }
        }

        public bool IsGraphed
        {
            get { return this.isGraphed; }
            set
            {
                if (this.isGraphed != value)
                {
                    this.isGraphed = value;
                    OnPropertyChanged(() => IsGraphed);
                }
            }
        }

        public Brush GraphColor
        {
            get { return this.graphColor; }
            set
            {
                if (this.graphColor != value)
                {
                    this.graphColor = value;
                    OnPropertyChanged(() => GraphColor);
                }
            }
        }

        public int InitialValue
        {
            get { return this.intialValue; }
            set
            {
                if (this.intialValue != value)
                {
                    this.intialValue = value;
                    OnPropertyChanged(() => InitialValue);

                    // If we're in editable Modifiable mode, mark the variable as being Modified
                    if (canModify)
                    {
                        this.IsModified = true;

                        this.Steps.ForEach(s => s.IsModified = true);
                    }
                }
            }
        }

        public void RandomiseValue()
        {
            InitialValue = RandomHelper.GetRandom(this.rangeFrom, this.rangeTo);
        }

        public void ModifiableValue()
        {
            this.IsModified = false;
            this.canModify = true;

            this.Steps.ForEach(s => s.IsModified = false);
        }
    }
}