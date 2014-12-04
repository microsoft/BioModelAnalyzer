namespace BackEndTests

open System
open System.Linq
open System.Xml.Linq
open Newtonsoft.Json
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type JsonToQnTests() = 

    let assertQNsAreEqual (q1:QN.node list) (q2:QN.node list) =
        Assert.AreEqual(List.length q1, List.length q2)
        let areQNEqual (qn1 : QN.node, qn2: QN.node) =
            qn1.defaultF = qn2.defaultF &&
            Expr.str_of_expr(qn1.f) = Expr.str_of_expr(qn2.f) &&
            qn1.inputs = qn2.inputs &&
            qn1.name = qn2.name &&
            qn1.nature = qn2.nature &&
            qn1.number = qn2.number &&
            qn1.range = qn2.range &&
            qn1.tags = qn2.tags && 
            qn1.var = qn2.var
        Assert.AreEqual(List.length q1, List.length q2)
        Assert.IsTrue(List.zip q1 q2 |> List.forall areQNEqual)

    [<TestMethod>]
    [<DeploymentItem("Skin2D_5X2_AI.xml")>]
    member x.``QNs support stuctural equality`` () = 
        // Import two QNs from one document ... 
        let xdoc = XDocument.Load("Skin2D_5X2_AI.xml")
        let xqn1 = Marshal.model_of_xml(xdoc)
        let xqn2 = Marshal.model_of_xml(xdoc) 
        // ... and they should be equal       
        assertQNsAreEqual xqn1 xqn2

//    [<TestMethod>]
//    [<DeploymentItem("Skin2D_5X2_AI.xml")>]
//    [<DeploymentItem("Skin2D_5X2_Analysis.json")>]
//    member x.``Converts JSON to QN for Skin2D_5X2`` () = 
//        let xqn = Marshal.model_of_xml(XDocument.Load("Skin2D_5X2_AI.xml"))
//        let reader = new System.IO.StreamReader("Skin2D_5X2_Analysis.json")        
//        let input = JsonSerializer.Create().Deserialize(reader, typedefof<bma.client.AnalysisInput>)
//        reader.Close()
//        let jqn = List.empty<QN.node> // Temporary solution
//        assertQNsAreEqual xqn jqn
