namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq
open BioModelAnalyzer

[<TestClass>]
type VMCAIFurtherTestingTests() = 

    [<TestMethod>]
    [<DeploymentItem("SimpleBifurcation.json")>]
    member x.``Bifurcating model bifurcates`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("SimpleBifurcation.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()
        model.Preprocess()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer () 
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let result = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExBifurcates(model, result)
        Assert.AreEqual(result.Variables.[0].Id, "3^0")
        Assert.AreEqual(result.Variables.[0].Fix1, 0)
        Assert.AreEqual(result.Variables.[0].Fix2, 1)

    [<TestMethod>]
    [<DeploymentItem("Race.json")>]
    member x.``Race model cycles`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("Race.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()
        model.Preprocess()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)

        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let result2 = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExCycles(model, result)
        Assert.AreEqual(result2.Variables.Length, 12)
        Assert.AreEqual(result2.Variables.[7].Id, "4^1")
        Assert.AreEqual(result2.Variables.[7].Value, 1)

    [<TestMethod>]
    [<DeploymentItem("ion channel.json")>]
    member x.``Ion channel has fixpoint`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ion channel.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<Model>()
        model.Preprocess()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 

        // Check stability
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let result2 = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExFixpoint(model, result)
        Assert.AreEqual(result2.Variables.Length, 2)
        Assert.AreEqual(result2.Variables.[0].Id, "3^0")
        Assert.AreEqual(result2.Variables.[1].Id, "2^0")
        Assert.AreEqual(result2.Variables.[0].Value, 0)
        Assert.AreEqual(result2.Variables.[1].Value, 0)

