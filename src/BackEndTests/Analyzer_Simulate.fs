namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq
open BioModelAnalyzer

[<TestClass>]
type VMCAISimulateTests() = 

    [<TestMethod; TestCategory("CI")>]
    [<DeploymentItem("ToyModelUnstable.json")>]
    member x.``Unstable model simulated for 10 steps`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("ToyModelUnstable.json"))

        // Extract model from json
        let model = (jobj.["Model"] :?> JObject).ToObject<Model>()

        // Create analyzer
        let analyzer = UIMain.Analyzer()

        let v1 = SimulationVariable()
        v1.Id <- 1
        v1.Value <- 1
        let v2 = SimulationVariable()
        v2.Id <- 2
        v2.Value <- 2
        let v3 = SimulationVariable()
        v3.Id <- 3
        v3.Value <- 3
        let mutable state = [| v1; v2; v3 |]
        for i in [0..9] do
             state <- (analyzer :> BioCheckAnalyzerCommon.IAnalyzer).simulate_tick(model, state)

        let pickId id (v : SimulationVariable) = if v.Id = id then Some(v) else None

        Assert.AreEqual(state.Length, 3);
        Assert.AreEqual((state |> Array.pick (pickId 1)).Value, 3);
        Assert.AreEqual((state |> Array.pick (pickId 2)).Value, 3);
        Assert.AreEqual((state |> Array.pick (pickId 3)).Value, 2);

