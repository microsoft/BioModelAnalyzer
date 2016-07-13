module JobsConsoleAppTests

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

let perform timeout job = 
    Job.RunToCompletion("AnalyzeLTL.exe", File.ReadAllText job, timeout)

[<Test; Timeout(600000)>]
let ``Console app checks LTL Polarity``() =
    checkJob (perform -1)


[<Test; ExpectedException(typeof<TimeoutException>)>]
let ``Timeout when running too long job``() =
    perform 1 "LTLQueries/Epi-V9.request.json" |> ignore


[<Test; ExpectedException(typeof<InvalidOperationException>)>]
let ``Handles incorrect queries``() =
    Job.RunToCompletion("AnalyzeLTL.exe", "~~query is incorrect~~", -1) |> ignore