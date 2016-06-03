﻿using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bma.LTLPolarity
{
    public static class Algorithms
    {
        public static Tuple<LTLAnalysisResult, LTLAnalysisResult> Check(LTLPolarityAnalysisInputDTO input)
        {
            try
            {
                string formula = input.Formula;
                string num_of_steps = input.Number_of_steps;
                FSharpOption<bool> polarity = FSharpOption<bool>.None;
                if (input.Polarity != LTLStatus.Unknown)
                {
                    polarity = new FSharpOption<bool>(input.Polarity == LTLStatus.True);
                }

                IAnalyzer analyzer = new UIMain.Analyzer();
                var model = (Model)input;
                var result = analyzer.checkLTLPolarity(input, formula, num_of_steps, polarity);
                var positive = new LTLAnalysisResult
                {
                    Error = result.Item1.Error,
                    Ticks = result.Item1.Ticks,
                    Status = result.Item1.Status ? LTLStatus.True : LTLStatus.False,
                    Loop = result.Item1.Loop,
                };

                LTLAnalysisResult negative = null;
                if (result.Item2 != null && !FSharpOption<LTLAnalysisResultDTO>.get_IsNone(result.Item2))
                {
                    negative = new LTLAnalysisResult
                    {
                        Error = result.Item2.Value.Error,
                        Ticks = result.Item2.Value.Ticks,
                        Status = result.Item2.Value.Status ? LTLStatus.True : LTLStatus.False,
                        Loop = result.Item2.Value.Loop,
                    };
                }

                return new Tuple<LTLAnalysisResult, LTLAnalysisResult>(positive, negative);
            }
            catch (Exception ex)
            {
                // Return an Unknown if fails
                return new Tuple<LTLAnalysisResult, LTLAnalysisResult>(new LTLAnalysisResult
                {
                    Error = ex.Message
                }, null);
            }
        }
    }
}
