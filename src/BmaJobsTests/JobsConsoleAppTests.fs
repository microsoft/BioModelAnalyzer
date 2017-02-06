// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module ``Console jobs``

open NUnit.Framework
open System.IO
open System
open CheckOperations
open JobsRunner

let performLTLPolarity timeout job = 
    Job.RunToCompletion("AnalyzeLTL.exe", File.ReadAllText job, timeout).Content

let performLTLSimulation timeout job = 
    Job.RunToCompletion("SimulateLTL.exe", File.ReadAllText job, timeout).Content

let performSimulation timeout job = 
    Job.RunToCompletion("Simulate.exe", File.ReadAllText job, timeout).Content

let performAnalysis timeout job = 
    Job.RunToCompletion("Analyze.exe", File.ReadAllText job, timeout).Content

let performFurtherTesting timeout job = 
    Job.RunToCompletion("FurtherTesting.exe", File.ReadAllText job, timeout).Content

[<Test; Timeout(600000)>]
[<Category("CI")>]
let ``Console app checks LTL Polarity``() =
    checkJob Folders.LTLQueries (performLTLPolarity -1) comparePolarityResults ""


[<Test; ExpectedException(typeof<TimeoutException>)>]
[<Category("CI")>]
let ``Timeout when running too long job for LTL polarity check``() =
    performLTLPolarity 1 "LTLQueries/Epi-V9.request.json" |> ignore

[<Test; ExpectedException(typeof<InvalidOperationException>)>]
[<Category("CI")>]
let ``Console app handles incorrect queries for LTL polarity check``() =
    Job.RunToCompletion("AnalyzeLTL.exe", "~~query is incorrect~~", -1) |> ignore

[<Test; Timeout(60000)>]
[<Category("CI")>]
let ``Console app makes a simulation for a model``() =
    checkJob Folders.Simulation (performSimulation -1) compareSimulationResults ""

[<Test; Timeout(60000)>]
[<Category("CI")>]
let ``Console app analyzes a model``() =
    checkJob Folders.Analysis (performAnalysis -1) compareAnalysisResults ""

[<Test; Timeout(60000)>]
[<Category("CI")>]
let ``Console app finds counter examples``() =
    checkJob Folders.CounterExamples (performFurtherTesting -1) compareFurtherTestingResults ""

[<Test; Timeout(60000)>]
[<Category("CI")>]
let ``Console app simulates LTL``() =
    checkJob Folders.LTLQueries (performLTLSimulation -1) compareLTLSimulationResults ".simulation"
