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

let checkJob (doJob : string -> string) =
    let outcome =
        Directory.EnumerateFiles("LTLQueries", "*.request.json")
        |> PSeq.map(fun fileName ->
            let dir = Path.GetDirectoryName(fileName)
            let file = Path.GetFileNameWithoutExtension(fileName)
            let jobName = file.Substring(0, file.Length - ".request".Length)
            Trace.WriteLine(sprintf "Starting job %s..." jobName)
            try
                let resp_json = doJob fileName
                Trace.WriteLine(sprintf "Job %s is done." jobName)

                let resp = JObject.Parse(resp_json)
                let expected = JObject.Parse(File.ReadAllText(Path.Combine(dir, sprintf "%s.response.json" jobName)))               
                let both = bothContains (expected, resp)
                let neither = neitherContains (expected, resp)
                if 
                    (neither "Item1" ||
                     both "Item1" &&
                     equalOrMissing (expected.["Item1"], resp.["Item1"]) "Status" &&
                     equalOrMissing (expected.["Item1"], resp.["Item1"]) "Error")
                    &&
                    (neither "Item2" ||
                     both "Item2" &&
                     equalOrMissing (expected.["Item2"], resp.["Item2"]) "Status" &&
                     equalOrMissing (expected.["Item2"], resp.["Item2"]) "Error")
                then None
                else failwithf "Response status for the job %s differs from expected" jobName
            with 
            | exn ->
                Trace.WriteLine(sprintf "Job %s failed: %A" jobName exn)
                Some (System.Exception(sprintf "Job %s failed" jobName,  exn)))

    match outcome |> Seq.choose id |> Seq.toList with
    | [] -> () // ok
    | failed -> 
        raise (System.AggregateException("Some jobs have failed", failed))