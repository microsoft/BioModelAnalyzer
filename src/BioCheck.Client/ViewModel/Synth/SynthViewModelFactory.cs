using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Synth
{
    public static class SynthViewModelFactory
    {
        // Synth edit
        public static SynthViewModel Create(ModelViewModel modelVM)
        {
            var synthVM = new SynthViewModel();
            
            synthVM.ModelName = modelVM.Name;

            return synthVM;
        }
    }
}