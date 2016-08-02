module ``Deployment Tests`` 
    
open NUnit.Framework
open FsCheck
open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open LTLTests

let urlApi = "http://localhost:8223/api/"
let url = "http://localhost:8223/api/lra"
//let url = "http://bmamathnew.cloudapp.net/api/lra"
//let urlApi = "http://bmamathnew.cloudapp.net/api/"

let appId = "CF1B2F01-E2B7-4D34-88B6-9C9078C0D637"

let isSucceeded jobId =
    let respCode, resp = Http.get (sprintf "%s/%s?jobId=%s" url appId jobId) "text/plain"
    match respCode with
    | 200 -> true
    | 201 | 202 -> false
    | 203 -> failwithf "Job failed: %A" resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code
    

let perform job = 
    let jobId = (Http.postJsonFile (sprintf "%s/%s" url appId) job |> snd).Trim('"')
    while isSucceeded jobId |> not do
        System.Threading.Thread.Sleep(1000)
    
    let respCode, resp = Http.get (sprintf "%s/%s/result?jobId=%s" url appId jobId) "application/json; charset=utf-8"
    match respCode with
    | 200 -> resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code

let performSR endpoint job =
    let code, result = Http.postJsonFile (sprintf "%s%s" urlApi endpoint) job
    match code with
    | 200 -> result
    | 204 -> raise (System.TimeoutException("Timeout while waiting for job to complete"))
    | _ -> failwithf "Unexpected http status code %d" code

let performShortPolarity = performSR "AnalyzeLTLPolarity"
let performLTLSimulation = performSR "AnalyzeLTLSimulation"
let performSimulation = performSR "Simulate"
let performAnalysis = performSR "Analyze"
let performFurtherTesting = performSR "FurtherTesting"


[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Long-running LTL polarity checks``() =
    checkJob Folders.LTLQueries perform comparePolarityResults ""

[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Short-running LTL polarity checks``() =
    checkSomeJobs performShortPolarity comparePolarityResults "" ["LTLQueries/toymodel.request.json"]

[<Test; ExpectedException(typeof<System.TimeoutException>)>]
[<Category("Deployment")>]
let ``Short LTL polarity causes timeout if the check takes too long``() =
    performShortPolarity "LTLQueries/Epi-V9.request.json" |> ignore

[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Simulate LTL``() =
    checkSomeJobs performLTLSimulation compareLTLSimulationResults "" ["LTLQueries/toymodel.request.json"]

[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Simulate model``() =
    checkJob Folders.Simulation performSimulation compareSimulationResults ""
    
[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Analyze model``() =
    checkJob Folders.Analysis performAnalysis compareAnalysisResults ""
    
[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Find counter examples for a model``() =
    checkJob Folders.CounterExamples performFurtherTesting compareFurtherTestingResults ""

