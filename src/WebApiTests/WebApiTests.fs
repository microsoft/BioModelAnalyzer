module ``LTL Queries`` 
    
open NUnit.Framework
open FsCheck
open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open LTLTests

let url = "http://bmamathnew.cloudapp.net/api/lra"
//let url = "http://localhost:8223/api/lra"
let appId = "CF1B2F01-E2B7-4D34-88B6-9C9078C0D637"

let isSucceeded jobId =
    let respCode, resp = Http.get (sprintf "%s/%s?jobId=%s" url appId jobId)
    match respCode with
    | 200 -> true
    | 201 | 202 -> false
    | 203 -> failwithf "Job failed: %s" resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code
    

let perform job = 
    let jobId = (Http.postFile (sprintf "%s/%s" url appId) job).Trim('"')
    while isSucceeded jobId |> not do
        System.Threading.Thread.Sleep(1000)
    
    let respCode, resp = Http.get (sprintf "%s/%s/result?jobId=%s" url appId jobId)
    match respCode with
    | 200 -> resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code


[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Statuses for sample ltl queries are verified``() =
    checkJob perform