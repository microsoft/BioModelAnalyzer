namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq
open BioModelAnalyzer
open System.Text.RegularExpressions
open System.ComponentModel

[<TestClass>]
[<DeploymentItem("libz3.dll")>]
type VMCAIFurtherTestingTests() = 

    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("SimpleBifurcation.json")>]
    member x.``Bifurcating model bifurcates`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("SimpleBifurcation.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer () 
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let resulto = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExBifurcates(model, result)
        let result = resulto.Value
//        Assert.AreEqual(result.Variables.[0].Id, "3^0")
//        Assert.AreEqual(result.Variables.[0].Fix1, 0)
//        Assert.AreEqual(result.Variables.[0].Fix2, 1)
        let var3_0 = result.Variables |> Seq.pick (fun v -> if v.Id = "3^0" then Some(v) else None) 
        Assert.AreEqual(var3_0.Id, "3^0")
        Assert.IsTrue(var3_0.Fix1 = 0 && var3_0.Fix2 = 1 || var3_0.Fix1 = 1 && var3_0.Fix2 = 0, "Bifuraction fixpoints")


    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("Race.json")>]
    member x.``Race model cycles`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("Race.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)

        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let result2o = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExCycles(model, result)
        let result2 = result2o.Value
        Assert.AreEqual(result2.Variables.Length, 12)
        let var4_1 = result2.Variables |> Seq.pick (fun v -> if v.Id = "4^1" then Some(v) else None) 
        Assert.AreEqual(var4_1.Value, 1)


    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("ion channel.json")>]
    member x.``Ion channel has fixpoint`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ion channel.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 

        // Check stability
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find bifurcations
        let result2o = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExFixpoint(model, result)
        let result2 = result2o.Value
        Assert.AreEqual(result2.Variables.Length, 2)
        // Check that var(2^0) exists and has value 0. 
        let var2_0 = result2.Variables |> Seq.pick (fun v -> if v.Id = "2^0" then Some(v) else None) 
        Assert.AreEqual(var2_0.Value, 0)
        // Check that var(3^0) exists and has value 0. 
        let var3_0 = result2.Variables |> Seq.pick (fun v -> if v.Id = "3^0" then Some(v) else None) 
        Assert.AreEqual(var3_0.Value, 0)

    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("ceilFunc.json")>]
    member x.``Find Counter Examples correctly handles ceil and floor functions`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ceilFunc.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 

        // Check stability
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing)

        // Find fix points
        let result2o = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExFixpoint(model, result)
        let result2 = result2o.Value
        // Check that var(2^0) exists and has value 0. 
        let var2_0 = result2.Variables |> Seq.filter (fun v -> Regex.IsMatch(v.Id, "\d*\^\d*") |> not ) |> Seq.length
        Assert.AreEqual(var2_0, 0)

    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("RestingNeuron.json")>]
    member x.``RestingNeuron.json is processed correctly`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("RestingNeuron.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer. 
        // Have to static cast to get IAnalyzer functions.   
        let analyzer = UIMain.Analyzer() 

        // Check stability
        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkStability(model)
        Assert.AreEqual(result.Status, StatusType.NotStabilizing, "not stabilizing")

        // Find fix points
        let result2o = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExFixpoint(model, result)
        let result2 = result2o.Value
        // Check that var(2^0) exists and has value 0. 
        let var2_0 = result2.Variables |> Seq.filter (fun v -> Regex.IsMatch(v.Id, "\d*\^\d*") |> not ) |> Seq.length
        Assert.AreEqual(var2_0, 0, "zero fixpoint")

        let resultBF = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExBifurcates(model, result)
        Assert.AreEqual(resultBF, null, "bifurcation")

        let resultC = (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).findCExCycles(model, result)
        Assert.AreEqual(resultC, null, "cycle")

//    [<TestMethod; TestCategory("CI")>]
//    [<DeploymentItem("ToyModelUnstable.json")>]
//    member x.``LTL check`` () = 
//        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelUnstable.json"))
//
//        // Extract model from json
//        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()
//
//        // Create analyzer. 
//        // Have to static cast to get IAnalyzer functions.   
//        let analyzer = UIMain.Analyzer () 
//        let result =  (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).checkLTL(model, "(< var(2) 1)", "10")
//        Assert.AreEqual(result.Status, StatusType.True)
//
//        let tick0 = result.Ticks.[0]
//        let vara = tick0.Variables.[0]
//        Assert.AreEqual(vara.Id, 1)
//        Assert.AreEqual(vara.Hi, 2)
//        Assert.AreEqual(vara.Lo, 2)
//
//        let varb = tick0.Variables.[1]
//        Assert.AreEqual(varb.Id, 2)
//        Assert.AreEqual(varb.Hi, 0)
//        Assert.AreEqual(varb.Lo, 0)
//
//        let varc = tick0.Variables.[2]
//        Assert.AreEqual(varc.Id, 3)
//        Assert.AreEqual(varc.Hi, 1)
//        Assert.AreEqual(varc.Lo, 1)



