module Synthesis

open FSharp.Data
open Microsoft.Z3
open Microsoft.Z3.FSharp.Common
open Microsoft.Z3.FSharp.Bool
open Microsoft.Z3.FSharp.BitVec
open DataLoading
open FunctionEncoding
open ShortestPaths
open FSharpx.Collections
open FSharp.Data
open FSharp.Data.CsvExtensions

type NumNonTransitionsEnforced = All | Num of int | DropFraction of int

let private constraintsBool (m : Model) (d : FuncDecl)  =
    let x = System.Boolean.Parse(m.[d].ToString())
    (Bool (d.Name.ToString())) =. not x

let private addConstraintsBool (solver : Solver) (m : Model) (ds : FuncDecl []) =
    let constraints = Or <| Array.map (constraintsBool m) ds
    solver.Add(constraints)

let private constraintsBitVec ctor (m : Model) (d : FuncDecl) =
    let x = System.Int32.Parse(m.[d].ToString())
    (ctor (d.Name.ToString())) <>. x

let private addConstraintsEnforcedVar (solver : Solver) (m : Model) (ds : FuncDecl []) =
    let constraints = Or <| Array.map (constraintsBitVec makeEnforcedVar m) ds
    solver.Add(constraints)

let addConstraintsCircuitVar (solver : Solver) (m : Model) (ds : FuncDecl []) =
    let constraints = Or <| Array.map (constraintsBitVec makeCircuitVar m) ds
    solver.Add(constraints)

// the below functions share a lot by copy and paste
// probably should take things as parameters

let private buildGraph edges =
    // REFACTOR!
    let mutable adjacency = Map.empty
    for (u, v) in edges do
        let something = if Map.containsKey u adjacency then v :: Map.find u adjacency else [v]
        adjacency <- Map.add u something adjacency
    adjacency

let private askNonTransition gene symVars =
    let counter = ref 0
        
    fun (profile : bool []) ->
        let nonTransitionEnforced = makeEnforcedVar (sprintf "enforced_%i" !counter)
        counter := !counter + 1

        let encoding, same = circuitEvaluatesToSame gene symVars profile
        (encoding &&. If (same, nonTransitionEnforced =. 1, nonTransitionEnforced =. 0),
            nonTransitionEnforced)

let private manyNonTransitionsEnforced gene symVars nonCloudExpressionProfilesWithoutGeneTransitions numNonTransitionsEnforced =
    if numNonTransitionsEnforced = 0 then True
    else
        let askNonTransitions, enforceVars = Array.unzip << Array.ofSeq <| Seq.map (askNonTransition gene symVars) nonCloudExpressionProfilesWithoutGeneTransitions
        let askNonTransitions = And askNonTransitions
        let manyEnforced = Array.reduce (+) enforceVars >=. numNonTransitionsEnforced

        askNonTransitions &&. manyEnforced

let private findAllowedEdges (solver : Solver) gene genes (geneNames : string []) maxActivators maxRepressors numNonTransitionsEnforced
                             (expressionProfilesWithGeneTransitions : Runtime.CsvFile<CsvRow>) (nonCloudExpressionProfilesWithoutGeneTransitions : Runtime.CsvFile<CsvRow>) =
    let circuitEncoding, symVars = encodeUpdateFunction gene genes maxActivators maxRepressors geneNames
    let nonCloudExpressionProfilesWithoutGeneTransitions = Seq.map rowToArray nonCloudExpressionProfilesWithoutGeneTransitions.Rows

    let numNonTransitionsEnforced =
        match numNonTransitionsEnforced with
        | All -> nonCloudExpressionProfilesWithoutGeneTransitions |> Seq.length
        | Num i -> i
        | DropFraction i -> let max = nonCloudExpressionProfilesWithoutGeneTransitions |> Seq.length
                            max - max / i
    let undirectedEdges = geneTransitions geneNames.[gene - 2] // GET RID OF -2 EVERYWHERE    
    let manyNonTransitionsEnforced = manyNonTransitionsEnforced gene symVars nonCloudExpressionProfilesWithoutGeneTransitions numNonTransitionsEnforced

    let encodeTransition (stateA, stateB) =
        let profile s = expressionProfilesWithGeneTransitions.Filter(fun row -> row.Columns.[0] = s).Rows |> Seq.head |> rowToArray // not efficent
        let differentA = (let e, v = circuitEvaluatesToDifferent gene symVars (profile stateA) in e &&. v)

        differentA

    let checkEdge (a, b) =
        solver.Reset()
        solver.Add (circuitEncoding,
                    manyNonTransitionsEnforced,
                    encodeTransition (a, b))

        solver.Check() = Status.SATISFIABLE

    set [ for (a, b) in undirectedEdges do
              if checkEdge (a, b) then yield (a, b)
              if checkEdge (b, a) then yield (b, a) ]

let private findFunctions (solver : Solver) gene genes (geneNames : string []) maxActivators maxRepressors numNonTransitionsEnforced shortestPaths
                          (expressionProfilesWithGeneTransitions  : Runtime.CsvFile<CsvRow>) (nonCloudExpressionProfilesWithoutGeneTransitions : Runtime.CsvFile<CsvRow>) =
    let circuitEncoding, symVars = encodeUpdateFunction gene genes maxActivators maxRepressors geneNames
    let nonCloudExpressionProfilesWithoutGeneTransitions = Seq.map rowToArray nonCloudExpressionProfilesWithoutGeneTransitions.Rows
    let undirectedEdges = geneTransitions geneNames.[gene - 2] |> Set.ofArray // GET RID OF -2 EVERYWHERE
    
    let numNonTransitionsEnforced =
        match numNonTransitionsEnforced with
        | All -> nonCloudExpressionProfilesWithoutGeneTransitions |> Seq.length
        | Num i -> i
        | DropFraction i -> let max = nonCloudExpressionProfilesWithoutGeneTransitions |> Seq.length
                            max - max / i

    let encodeTransition (stateA, stateB) =
        if not (Set.contains (stateA, stateB) undirectedEdges || Set.contains (stateB, stateA) undirectedEdges)
        then // remove these 4 lines
            True
        else
        let profile s = expressionProfilesWithGeneTransitions.Filter(fun row -> row.Columns.[0] = s).Rows |> Seq.head |> rowToArray // not efficent
        let differentA = (let e, v = circuitEvaluatesToDifferent gene symVars (profile stateA) in e &&. v)

        differentA

    let encodePath (path : string list) =
        let f (formula, u) v = (And [| formula; encodeTransition (u, v) |], v)
        List.fold f (True, List.head path) (List.tail path) |> fst
    
    let pathsEncoding = if Seq.isEmpty shortestPaths then True else
                        And [| for paths in shortestPaths do
                                   yield Or [| for path in paths do
                                                   yield encodePath path |] |]

    solver.Reset()
    solver.Add (circuitEncoding,
                pathsEncoding, // ENCODED THE SAME TRANSITION MULTIPLE TIMES, could make slower. THE UNSAT CODE HAD A NICER WAY OF DOING THIS
                // implement percentages and the better enforcelexicalordering
                manyNonTransitionsEnforced gene symVars nonCloudExpressionProfilesWithoutGeneTransitions numNonTransitionsEnforced)

    let intToName i = if i = AND then "And"
                      elif i = OR then "Or"
                      elif i = NOTHING then "Nothing"
                      else geneNames.[i - 2]

    set [ while solver.Check() = Status.SATISFIABLE do
                let m = solver.Model

                let circuitDecls = Array.filter (fun (d : FuncDecl) -> Set.contains (d.Name.ToString()) circuitVars) m.ConstDecls // refactor top level
                addConstraintsCircuitVar solver m circuitDecls

                let enforceDecls = Array.filter (fun (d : FuncDecl) -> d.Name.ToString().StartsWith "enforced") m.ConstDecls // refactor top level 
                let numEnforced = List.sum <| [ for d in enforceDecls do yield System.Int32.Parse (string m.[d]) ]

// TODO: GIVE THE PERCENTAGE TOO
                yield ("numEnforced", string numEnforced) :: [ for d in circuitDecls do
                                                                    let value = System.Int32.Parse(m.[d].ToString())
                                                                    if value <> NOTHING then
                                                                        yield (sprintf "%O" d.Name, intToName value) ] ]

let synthesise geneIds geneNames statesFilename initialStates targetStates nonTransitionEnforcedStates =
    let f n = 2 + (Seq.findIndex ((=) n) geneNames) // + 2 BECAUSE OF AND, OR

    let geneParameters = Map.ofList ["Gata2", (1, 3)
                                     "Gata1", (3, 1)
                                     "Fog1", (1, 0)
                                     "EKLF", (1, 1)
                                     "Fli1", (1, 1)
                                     "Scl", (1, 1)
                                     "Cebpa", (1, 3)
                                     "Pu.1", (2, 2)
                                     "cJun", (1, 1)
                                     "EgrNab", (2, 1)
                                     "Gfi1", (1, 1)]

    let getExpressionProfiles = getExpressionProfiles statesFilename nonTransitionEnforcedStates geneNames
    let solver = Solver()

    let allowedEdges = geneNames |> Array.map (fun g -> let a, r = Map.find g geneParameters
                                                        let expressionProfilesWithGeneTransitions, nonCloudExpressionProfilesWithoutGeneTransitions = getExpressionProfiles (f g)
                                                        let temp = findAllowedEdges solver (f g) geneIds geneNames a r All expressionProfilesWithGeneTransitions nonCloudExpressionProfilesWithoutGeneTransitions
                                                        // SI: Pass output dir as command line arg. Change literal "\\" to use Dir.Combine
                                                        System.IO.File.WriteAllText (HOME_DIR + "Desktop\\Cmp\\Edges\\" + g + ".txt", printEdges temp)
                                                        temp) |> Set.unionMany

    let reducedStateGraph = buildGraph allowedEdges

    let shortestPaths = initialStates |> Array.map (fun initial ->
        let targetStates = targetStates |> Set.ofArray |> Set.remove initial
        shortestPathMultiSink reducedStateGraph initial targetStates) 

    let invertedPaths = [| for i in 0 .. Array.length targetStates - 1 do
                               yield [ for j in 0 .. Array.length initialStates - 1 do
                                           for path in shortestPaths.[j] do
                                               match path with
                                               | [] -> ()
                                               | l -> if List.nth l (List.length l - 1) = targetStates.[i] then yield l ] |] // not efficent

    geneNames |> Array.iter (fun gene -> let numAct, numRep = Map.find gene geneParameters
                                         let expressionProfilesWithGeneTransitions, nonCloudExpressionProfilesWithoutGeneTransitions = getExpressionProfiles (f gene)
                                         let circuits = findFunctions solver (f gene) geneIds geneNames numAct numRep All invertedPaths expressionProfilesWithGeneTransitions nonCloudExpressionProfilesWithoutGeneTransitions
                                         // SI: Pass output dir as command line arg. Change literal "\\" to use Dir.Combine
                                         System.IO.File.WriteAllLines (HOME_DIR + "Desktop\\Cmp\\" + gene + ".txt", Seq.map (sprintf "%A") circuits))