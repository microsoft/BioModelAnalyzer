module LTLTests

open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq

let bothContains (item1 : JToken, item2 : JToken) prop =
    item1.[prop] <> null && item2.[prop] <> null && item1.[prop].Type <> JTokenType.Null && item2.[prop].Type <> JTokenType.Null

let neitherContains (item1 : JToken, item2 : JToken) prop =
    (item1.[prop] = null || item1.[prop].Type = JTokenType.Null) || 
    (item2.[prop] = null || item2.[prop].Type = JTokenType.Null)

let equalOrMissing (item1 : JToken, item2 : JToken) prop =
    neitherContains (item1,item2) prop ||
    bothContains (item1,item2) prop && item1.[prop] = item2.[prop]

let compareSimulationResults (exp:JToken) (act:JToken) =
    equalOrMissing (exp, act) "Status" &&
    equalOrMissing (exp, act) "Error"

let comparePolarityResults (exp:JToken) (act:JToken) =
    let both = bothContains (exp, act)
    let neither = neitherContains (exp, act)
    (neither "Item1" ||
        both "Item1" &&
        equalOrMissing (exp.["Item1"], act.["Item1"]) "Status" &&
        equalOrMissing (exp.["Item1"], act.["Item1"]) "Error")
    &&
    (neither "Item2" ||
        both "Item2" &&
        equalOrMissing (exp.["Item2"], act.["Item2"]) "Status" &&
        equalOrMissing (exp.["Item2"], act.["Item2"]) "Error")


let checkSomeJobs (doJob : string -> string) (compare : JToken -> JToken -> bool) (responseSuffix : string) (jobs:string seq) =
    let outcome =
        jobs
        |> Seq.map(fun fileName ->
            let dir = Path.GetDirectoryName(fileName)
            let file = Path.GetFileNameWithoutExtension(fileName)
            let jobName = file.Substring(0, file.Length - ".request".Length)
            Trace.WriteLine(sprintf "Starting job %s..." jobName)
            try
                let resp_json = doJob fileName
                Trace.WriteLine(sprintf "Job %s is done." jobName)

                let resp = JObject.Parse(resp_json)
                let expected = JObject.Parse(File.ReadAllText(Path.Combine(dir, sprintf "%s%s.response.json" jobName responseSuffix)))               
                
                if compare expected resp then None
                else failwithf "Response status for the job %s differs from expected" jobName
            with 
            | exn ->
                Trace.WriteLine(sprintf "Job %s failed: %A" jobName exn)
                Some (System.Exception(sprintf "Job %s failed" jobName,  exn)))

    match outcome |> Seq.choose id |> Seq.toList with
    | [] -> () // ok
    | failed -> 
        raise (System.AggregateException("Some jobs have failed", failed))

let checkJob (doJob : string -> string) (compare : JToken -> JToken -> bool) (responseSuffix : string) =
    Directory.EnumerateFiles("LTLQueries", "*.request.json")
    |> checkSomeJobs doJob compare responseSuffix

