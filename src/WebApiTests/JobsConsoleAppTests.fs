module ``Console jobs``

open NUnit.Framework
open FsCheck
open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open System
open System.Text
open LTLTests
open JobsRunner

let performLTLPolarity timeout job = 
    Job.RunToCompletion("AnalyzeLTL.exe", File.ReadAllText job, timeout)

let performLTLSimulation timeout job = 
    Job.RunToCompletion("SimulateLTL.exe", File.ReadAllText job, timeout)

let performSimulation timeout job = 
    Job.RunToCompletion("Simulate.exe", File.ReadAllText job, timeout)

let performAnalysis timeout job = 
    Job.RunToCompletion("Analyze.exe", File.ReadAllText job, timeout)

[<Test; Timeout(600000)>]
let ``Console app checks LTL Polarity``() =
    checkJob Folders.LTLQueries (performLTLPolarity -1) comparePolarityResults ""


[<Test; ExpectedException(typeof<TimeoutException>)>]
let ``Timeout when running too long job``() =
    performLTLPolarity 1 "LTLQueries/Epi-V9.request.json" |> ignore


[<Test; ExpectedException(typeof<InvalidOperationException>)>]
let ``Handles incorrect queries``() =
    Job.RunToCompletion("AnalyzeLTL.exe", "~~query is incorrect~~", -1) |> ignore

[<Test; Timeout(60000)>]
let ``Console app simulates LTL, i.e. makes a proof``() =
    checkJob Folders.LTLQueries (performLTLSimulation -1) compareLTLSimulationResults ".simulation"

[<Test; Timeout(60000)>]
let ``Console app makes a simulation for a model``() =
    checkJob Folders.Simulation (performSimulation -1) compareSimulationResults ""

[<Test; Timeout(60000)>]
let ``Console app analyzes a model``() =
    checkJob Folders.Analysis (performAnalysis -1) compareAnalysisResults ""