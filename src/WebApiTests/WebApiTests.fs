module ``LTL Queries`` 
    
open NUnit.Framework
open FsCheck
open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open LTLTests

let urlApi = "http://localhost:8223/api/"
//let url = "http://bmamathnew.cloudapp.net/api/lra"
let url = "http://localhost:8223/api/lra"

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
    let jobId = (Http.postFile (sprintf "%s/%s" url appId) job |> snd).Trim('"')
    while isSucceeded jobId |> not do
        System.Threading.Thread.Sleep(1000)
    
    let respCode, resp = Http.get (sprintf "%s/%s/result?jobId=%s" url appId jobId)
    match respCode with
    | 200 -> resp
    | 404 -> failwith "There is no job with the given job id and application id"
    | code -> failwithf "Unknown response code when getting status: %d" code

let performShortPolarity job = 
    try
        let code, result = Http.postFile (sprintf "%sAnalyzeLTLPolarity" urlApi) job
        match code with
        | 200 -> result
        | 504 -> raise (System.TimeoutException("Timeout while waiting for LTL polarity check"))
        | _ -> failwithf "Unexpected http status code %d" code
    with
    | :? System.Net.WebException as ex ->
        match ex.Response with
        | :? System.Net.HttpWebResponse as resp when resp.StatusCode = System.Net.HttpStatusCode.GatewayTimeout ->
            raise (System.TimeoutException("Timeout while waiting for LTL polarity check"))
        | _ -> raise ex   


[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Long-running LTL polarity checks``() =
    checkJob perform

[<Test; Timeout(600000)>]
[<Category("Deployment")>]
let ``Short-running LTL polarity checks``() =
    checkSomeJobs performShortPolarity ["LTLQueries/toymodel.request.json"]

[<Test; ExpectedException(typeof<System.TimeoutException>)>]
[<Category("Deployment")>]
let ``Short LTL polarity causes timeout if the check takes too long``() =
    performShortPolarity "LTLQueries/Epi-V9.request.json" |> ignore