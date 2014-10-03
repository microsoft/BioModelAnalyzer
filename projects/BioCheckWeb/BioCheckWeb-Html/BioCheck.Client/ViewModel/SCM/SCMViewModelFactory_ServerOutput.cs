using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;

namespace BioCheck.ViewModel.SCM
{
    public static class SCMViewModelFactory_ServerOutput
    {
        public static ProofViewModel Create(AnalysisInputDTO input, AnalysisOutput output)
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var proofVM = new ProofViewModel(input, output);

            proofVM.ModelName = modelVM.Name;
            proofVM.State = output.Status == "SingleStablePoint" ? ProofViewState.Stable : ProofViewState.NotStable;
            return proofVM;
        }
    }
}