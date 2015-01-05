namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open System.Xml.Serialization

open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq

open BioCheckAnalyzerCommon
open BioModelAnalyzer

[<TestClass>]
type VMCAIAnalyzeTests() = 

    [<TestMethod>]
    [<DeploymentItem("ToyModelStable.json")>]
    member x.``Stable model stabilizes`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelStable.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()
        model.Preprocess()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer () 
        let result = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)

        Assert.AreEqual(result.Status, StatusType.Stabilizing)


    [<TestMethod>]
    [<DeploymentItem("ToyModelUnstable.json")>]
    member x.``Unstable model does not stabilize`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelUnstable.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()
        model.Preprocess() 

        // Create analyzer
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 
        let result = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model) 
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)