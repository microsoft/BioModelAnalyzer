module Http

open System.Diagnostics
open System.Net
open System.IO
open System.Text
open Newtonsoft.Json.Linq

let traceHttp = TraceSwitch("http", "http requests", "Info")

let post url (content:byte[]) =
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Sending POST to %s..." url)

    let request = WebRequest.Create(url) :?> HttpWebRequest

    request.ContentType <- "application/x-www-form-urlencoded; charset=UTF-8"
    request.Accept <- "application/json, text/javascript, */*; q=0.01"
    request.UserAgent <- "Unit tests"
    request.Method <- "POST"
    request.KeepAlive <- false
    request.ContentLength <- int64(content.Length)
    
    // Get the request stream.
    use dataStream = request.GetRequestStream();

    // Write the data to the request stream.
    dataStream.Write(content, 0, content.Length);
    
    let response = request.GetResponse() :?> HttpWebResponse
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Response code %A" response.StatusCode)

    use respStream = response.GetResponseStream()
    let readStream = new StreamReader(respStream, Encoding.UTF8)
    let respContent = readStream.ReadToEnd()
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Response: %s" respContent)
    int(response.StatusCode), respContent

let postFile url filePath =
    let fileContent = File.ReadAllText filePath
    let byteArray = Encoding.UTF8.GetBytes fileContent
    post url byteArray
    
let get url =
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Sending GET to %s..." url)

    let request = WebRequest.Create(url) :?> HttpWebRequest

    request.ContentType <- "application/x-www-form-urlencoded; charset=UTF-8"
    request.Accept <- "application/json, text/javascript, */*; q=0.01"
    request.UserAgent <- "Unit tests"
    request.Method <- "GET"
    
    let response = request.GetResponse() :?> HttpWebResponse
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Response code %A" response.StatusCode)

    use respStream = response.GetResponseStream()
    let readStream = new StreamReader(respStream, Encoding.UTF8)
    let respContent = readStream.ReadToEnd()
    Trace.WriteLineIf (traceHttp.TraceVerbose, sprintf "Response: %s" respContent)

    int(response.StatusCode), respContent

let getJson url =
    let code, resp = get url
    code, JObject.Parse resp