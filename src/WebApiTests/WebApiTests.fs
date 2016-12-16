module ``Deployment Tests`` 
    
open NUnit.Framework
open CheckOperations

let urlApi = "http://localhost:8223/api/"
//let urlApi = "https://ossbmaapiserver.azurewebsites.net/api/"

let appId = "CF1B2F01-E2B7-4D34-88B6-9C9078C0D637"

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
