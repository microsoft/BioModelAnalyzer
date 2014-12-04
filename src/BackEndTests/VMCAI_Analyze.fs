namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq
open BioModelAnalyzer

[<TestClass>]
type VMCAIAnalyzeTests() = 

    [<TestMethod>]
    [<DeploymentItem("ToyModelStable.json")>]
    member x.``Stable model stabilizes`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelStable.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()

        // Create analyzer
        let vmcai = VMCAIAnalyzerAdapter(UIMain.Analyzer2())

        let result = vmcai.CheckStability(model, null)
        Assert.AreEqual(result.Status, StatusType.Stabilizing)

    [<TestMethod>]
    [<DeploymentItem("ToyModelUnstable.json")>]
    member x.``Unstable model does not stabilize`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelUnstable.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()

        // Create analyzer
        let vmcai = VMCAIAnalyzerAdapter(UIMain.Analyzer2())

        let result = vmcai.CheckStability(model, null)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)