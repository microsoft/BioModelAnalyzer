using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.SCM
{
    public static class SCMViewModelFactory
    {
        public static SCMViewModel Create(ModelViewModel modelVM)
        {
            var scmVM = new SCMViewModel();
            
            scmVM.ModelName = modelVM.Name;

            return scmVM;
        }
    }
}