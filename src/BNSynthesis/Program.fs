module Program

open DataLoading
open FSharp.Data
open FSharpx.Collections

let synth (statesFilename : string) edgesFilename initialStatesFilename targetStatesFilename nonTransitionsNodesFilename =
    let geneNames = CsvFile.Load(statesFilename).Headers |> Option.get |> Seq.skip 1 |> Array.ofSeq

    let f n = 2 + (Seq.findIndex ((=) n) geneNames) // + 2 BECAUSE OF AND, OR
    let geneIds = geneNames |> Seq.map f |> Set.ofSeq

    let initialStates = readLines initialStatesFilename
    let targetStates = readLines targetStatesFilename
    let nonTransitionEnforcedStates = readLines nonTransitionsNodesFilename |> Set.ofArray

    Synthesis.synthesise geneIds geneNames statesFilename initialStates targetStates nonTransitionEnforcedStates
    
[<EntryPoint>]
let main args =
    synth args.[0] args.[1] args.[2] args.[3] args.[4]

    0