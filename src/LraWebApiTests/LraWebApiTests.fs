module ``Deployment Tests`` 
    
open NUnit.Framework
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open CheckOperations

let urlApi = "http://localhost:8223/api/"
//let urlApi = "https://ossbmaapiserver.azurewebsites.net/api/"
let urlLra = sprintf "%slra" urlApi

let appId = "CF1B2F01-E2B7-4D34-88B6-9C9078C0D637"

let isSucceeded jobId =
    let respCode, resp = Http.get (sprintf "%s/%s?jobId=%s" urlLra appId jobId) "text/plain"
    match respCode with
    | 200 -> true
    | 201 | 202 -> false
    | 203 -> failwithf "Job failed: %A" resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code
    

let perform job = 
    let jobId = (Http.postJsonFile (sprintf "%s/%s" urlLra appId) job |> snd).Trim('"')
    while isSucceeded jobId |> not do
        System.Threading.Thread.Sleep(1000)
    
    let respCode, resp = Http.get (sprintf "%s/%s/result?jobId=%s" urlLra appId jobId) "application/json; charset=utf-8"
    match respCode with
    | 200 -> resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code


[<Test; Timeout(600000)>]
[<Category("Deployment"); Category("CloudService")>]
let ``LRA - Long-running LTL polarity checks``() =
    checkJob Folders.LTLQueries perform comparePolarityResults ""

[<Test>]
[<Category("Deployment"); Category("CloudService")>]
let ``LRA - Check get status response format``() =
    let job = "LTLQueries/toymodel.request.json"

    let t0 = System.DateTimeOffset.Now

    let jobs = 
        Array.init 5 (fun n -> (Http.postJsonFile (sprintf "%s/%s" urlLra appId) job |> snd).Trim('"'))
        
    let jobId = jobs.[jobs.Length-1]

    let rec check (lastPos:int, lastElapsed:int) =
        let next arg = 
            System.Threading.Thread.Sleep(100)
            check arg

        let respCode, resp = Http.get (sprintf "%s/%s?jobId=%s" urlLra appId jobId) "application/json; charset=utf-8"
        match respCode with
        | 200 -> ()
        | 201 -> 
            Trace.WriteLine(sprintf "Queued: %s, expected <= %d" resp lastPos)
            let pos = int resp
            Assert.GreaterOrEqual(pos, 0, "Position is zero-based")
            Assert.LessOrEqual(pos, lastPos, "Position must not increase")
            next(pos, lastElapsed)
        | 202 ->         
            Trace.WriteLine(sprintf "Executing: %s, expected >= %d" resp lastElapsed)
            
            let json = JObject.Parse(resp)

            let started = System.DateTimeOffset.Parse(json.["started"].ToString())
            Assert.IsTrue(started > t0, "Start time")
            Assert.IsTrue(started < System.DateTimeOffset.Now, "Start time (2)")
            
            let elapsed = int (json.["elapsed"])
            Assert.Greater(elapsed, lastElapsed, "Elapsed")

            next(lastPos, elapsed)
        | 404 -> failwith "There is no job with the given job id and application id"
        | code -> failwithf "Unexpected response code when getting status: %d" code
    
    check(jobs.Length-1, 0)


[<Test>]
[<Category("Deployment"); Category("CloudService")>]
let ``LRA - Check get failure status response format``() =
    let job = "incorrect.request.json"

    let jobId = (Http.postJsonFile (sprintf "%s/%s" urlLra appId) job |> snd).Trim('"')

    let rec check () =
        let next arg = 
            System.Threading.Thread.Sleep(100)
            check arg

        let respCode, resp = Http.get (sprintf "%s/%s?jobId=%s" urlLra appId jobId) "application/json; charset=utf-8"
        match respCode with
        | 203 -> 
            Trace.WriteLine("Ok, the job has failed: " + resp)
            StringAssert.Contains("Exception", resp)
        | 200 -> failwith "The job must fail"
        | 201 -> 
            Trace.WriteLine(sprintf "Queued: %s" resp)
            next()
        | 202 ->         
            Trace.WriteLine(sprintf "Executing: %s" resp)
            next()
        | 404 -> failwith "There is no job with the given job id and application id"
        | code -> failwithf "Unexpected response code when getting status: %d" code
    
    check ()
