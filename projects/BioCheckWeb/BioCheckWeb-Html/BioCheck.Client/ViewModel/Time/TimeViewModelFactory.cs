using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Time
{
    public static class TimeViewModelFactory
    {
        // Time edit
        public static TimeViewModel Create(ModelViewModel modelVM)
        {
            var timeVM = new TimeViewModel();
            
            timeVM.ModelName = modelVM.Name;

            return timeVM;
        }
    }
}