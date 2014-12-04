namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open Newtonsoft.Json.Linq

[<TestClass>]
type JsonPreprocessTests() = 

    [<TestMethod>]
    [<DeploymentItem("SkinModel.json")>]
    member x.``Replaces variable names with their IDs`` () = 
        let jobj = JObject.Parse(System.IO.File.ReadAllText("SkinModel.json"))

        // Extract model from json
        let model = (jobj.["model"] :?> JObject).ToObject<BioModelAnalyzer.Model>()
        model.Preprocess()

        // Find variable with Id = 5
        let var5 = model.Variables |> Array.pick (fun v -> match v.Id with
                                                            | 5 -> Some(v)
                                                            | _ -> None)        
        Assert.AreEqual(var5.Function, "min(var(12),var(45))")
