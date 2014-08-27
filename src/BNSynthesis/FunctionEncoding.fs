module FunctionEncoding

open FSharpx.Prelude
open Microsoft.Z3.FSharp.Common
open Microsoft.Z3.FSharp.Bool
open Microsoft.Z3.FSharp.BitVec
open FSharp.Data.Csv
open DataLoading

// needed?
let rec private delete x = function
  | [] -> []
  | h :: t when x = h -> t
  | h :: t -> h :: delete x t

// needed?
let inline private (/-/) xs ys = List.fold (flip delete) xs ys

let private numGenes = let statesFilename = System.Environment.GetCommandLineArgs().[1]
                       (CsvFile.Load(statesFilename).Headers |> Option.get |> Seq.length) - 1

// move to dataloading
let private numNonTransitionEnforcedStates = let nonTransitionEnforcedStatesFilename = System.Environment.GetCommandLineArgs().[5]
                                             readLines nonTransitionEnforcedStatesFilename |> Array.length

let [<Literal>] AND = 0
let [<Literal>] OR = 1
let NOTHING = numGenes + 2

let makeCircuitVar = let numBits = (uint32 << ceil <| System.Math.Log(float numGenes, 2.0)) + 1u // can remove the + 1, need to use the unsigned versions of operators
                     fun name -> BitVec (name, numBits)
                     
let makeEnforcedVar = let numBits = (uint32 << ceil <| System.Math.Log(float numNonTransitionEnforcedStates, 2.0)) + 1u // can remove the + 1, need to use the unsigned versions of operators
                      fun name -> BitVec (name, numBits)

let private variableDomains var lowerBound upperBound =
    (var >=. lowerBound) &&. (var <=. upperBound)
// >=~?

let private parentsOfNothingArentGates (a : BitVec []) (r : BitVec []) =
    let f c1 c2 p = Implies ((c1 =. NOTHING) ||. (c2 =. NOTHING),  (p <>. AND) &&. (p <>. OR))

    let aParents = And [| Implies ((a.[1] =. NOTHING) ||. (a.[2] =. NOTHING),  And [| a.[0] <>. AND; a.[0] <>. OR; a.[0] <>. NOTHING |]) // =>. rename
                          f a.[3] a.[4] a.[1]
                          f a.[5] a.[6] a.[2]
                          f a.[7] a.[8] a.[3]
                          f a.[9] a.[10] a.[4]
                          f a.[11] a.[12] a.[5]
                          f a.[13] a.[14] a.[6] |]

    let rParents = And [| f r.[1] r.[2] r.[0]
                          f r.[3] r.[4] r.[1]
                          f r.[5] r.[6] r.[2] |]
                              
    aParents &&. rParents

let private parentsOfRestAreGates (a : BitVec []) (r : BitVec []) =
    let f c1 c2 p = Implies ((c1 <>. NOTHING) ||. (c2 <>. NOTHING),  (p =. AND) ||. (p =. OR)) // =>. rename

    // duplicated with above
    let aParents = And [| f a.[1] a.[2] a.[0]
                          f a.[3] a.[4] a.[1]
                          f a.[5] a.[6] a.[2]
                          f a.[7] a.[8] a.[3]
                          f a.[9] a.[10] a.[4]
                          f a.[11] a.[12] a.[5]
                          f a.[13] a.[14] a.[6] |]

    // duplicated with above
    let rParents = And [| f r.[1] r.[2] r.[0]
                          f r.[3] r.[4] r.[1]
                          f r.[5] r.[6] r.[2] |]

    aParents &&. rParents

let private variablesDoNotAppearMoreThanOnce (gene : int) genes aVars rVars =
    let varDoesNotAppearMoreThanOnce symVars v = 
        let f a = Implies (a =. v, And (Array.map (fun a' -> a' <>. v) (Array.ofList ((List.ofArray symVars) /-/ [a])))) // sort of horrible..
        And <| Array.map f symVars

    varDoesNotAppearMoreThanOnce aVars gene &&. // refactor
    varDoesNotAppearMoreThanOnce rVars gene &&.
    (And <| Array.map (varDoesNotAppearMoreThanOnce <| aVars ++ rVars) (Set.toArray (genes - set [gene])))

let private enforceSiblingLexigraphicalOrdering (v1 : BitVec) (v2 : BitVec) =
    Implies (v1 <>. v2, v1 <. v2)
    //Implies (v1 <>. v2, v1 <~ v2)

let private enforceLexigraphicalOrderingBetweenBranches (p1 : BitVec) (p2 : BitVec) (c1 : BitVec) (c2 : BitVec) =
    Implies (p1 =. p2, c1 <=. c2)  
    //Implies (p1 =. p2, c1 <=~ c2)

// move
let circuitVars = set [ "a1"; "a2"; "a3"; "a4"; "a5"; "a6"; "a7"; "a8"; "a9"; "a10"; "a11"; "a12"; "a13"; "a14"; "a15"; "r1"; "r2"; "r3"; "r4"; "r5"; "r6"; "r7" ]

let private fixMaxActivators max =
    let v = "a"
    match max with
    | 0 -> makeCircuitVar (v + "1") =. NOTHING
    | 1 -> makeCircuitVar (v + "2") =. NOTHING
    | 2 -> makeCircuitVar (v + "4") =. NOTHING
    | 3 -> And [| makeCircuitVar (v + "6") =. NOTHING; makeCircuitVar (v + "8") =. NOTHING |]
    | 4 -> And [| makeCircuitVar (v + "8") =. NOTHING; makeCircuitVar (v + "10") =. NOTHING
                  makeCircuitVar (v + "12") =. NOTHING; makeCircuitVar (v + "14") =. NOTHING |]
    | 5 -> makeCircuitVar (v + "6") =. NOTHING
    | 6 -> And [| makeCircuitVar (v + "12") =. NOTHING; makeCircuitVar (v + "14") =. NOTHING |]
    | 7 ->  makeCircuitVar (v + "14") =. NOTHING
    | _ -> True

let private fixMaxRepressors max =
    let v = "r"
    match max with
    | 0 -> makeCircuitVar (v + "1") =. NOTHING
    | 1 -> makeCircuitVar (v + "2") =. NOTHING
    | 2 -> makeCircuitVar (v + "4") =. NOTHING
    | 3 -> makeCircuitVar (v + "6") =. NOTHING
    | _ -> True

let private notAnActivator (n : int) =
    [| for i in 1 .. 15 do 
            yield makeCircuitVar (sprintf "a%i" i) <>. n
    |] |> And
      
let private notARepressor (n : int) =
    [| for i in 1 .. 7 do
            yield makeCircuitVar (sprintf "r%i" i) <>. n
    |] |> And
      
let private isAnActivator (n : int) =
    [| for i in 1 .. 15 do
            yield makeCircuitVar (sprintf "a%i" i) =. n
    |] |> Or  
        
let private isARepressor (n : int) =
    [| for i in 1 .. 7 do
            yield makeCircuitVar (sprintf "r%i" i) =. n
    |] |> Or

let encodeUpdateFunction gene genes maxActivators maxRepressors (tempGeneNames : string []) =
    if not (Set.contains gene genes && maxActivators > 0 && maxActivators <= 8 && maxRepressors >= 0 && maxRepressors <= 4) then
        failwith "Incorrect arguments to encodeForUpdateFunction"

    let a = [| for i in 1..15 -> makeCircuitVar (sprintf "a%i" i) |]
    let r = [| for i in 1..7 -> makeCircuitVar (sprintf "r%i" i) |]
    let symVars = a ++ r

    let circuitEncoding = And <| [| variableDomains a.[0] 0 (NOTHING - 1)
                                    variableDomains r.[0] 0 NOTHING

                                    variableDomains a.[1] 0 NOTHING; variableDomains a.[2] 0 NOTHING; variableDomains a.[3] 0 NOTHING
                                    variableDomains a.[4] 0 NOTHING; variableDomains a.[5] 0 NOTHING; variableDomains a.[6] 0 NOTHING
              
                                    variableDomains r.[1] 0 NOTHING; variableDomains r.[2] 0 NOTHING

                                    variableDomains a.[7] 2 NOTHING; variableDomains a.[8] 2 NOTHING; variableDomains a.[9] 2 NOTHING; variableDomains a.[10] 2 NOTHING
                                    variableDomains a.[11] 2 NOTHING; variableDomains a.[12] 2 NOTHING; variableDomains a.[13] 2 NOTHING; variableDomains a.[14] 2 NOTHING

                                    variableDomains r.[3] 2 NOTHING; variableDomains r.[4] 2 NOTHING; variableDomains r.[4] 2 NOTHING; variableDomains r.[5] 2 NOTHING
                                    variableDomains r.[6] 2 NOTHING

                                    parentsOfNothingArentGates a r
                                    parentsOfRestAreGates a r
                                    variablesDoNotAppearMoreThanOnce gene genes a r
                                    
                                    enforceSiblingLexigraphicalOrdering a.[1] a.[2]
                                    enforceSiblingLexigraphicalOrdering a.[3] a.[4]
                                    enforceSiblingLexigraphicalOrdering a.[5] a.[6]
                                    enforceSiblingLexigraphicalOrdering a.[7] a.[8]
                                    enforceSiblingLexigraphicalOrdering a.[9] a.[10]
                                    enforceSiblingLexigraphicalOrdering a.[10] a.[11]
                                    enforceSiblingLexigraphicalOrdering a.[12] a.[13]
                                    
                                    enforceSiblingLexigraphicalOrdering r.[1] r.[2]
                                    enforceSiblingLexigraphicalOrdering r.[3] r.[4]
                                    enforceSiblingLexigraphicalOrdering r.[5] r.[6]

                                    enforceLexigraphicalOrderingBetweenBranches a.[1] a.[2] a.[3] a.[5]
                                    enforceLexigraphicalOrderingBetweenBranches a.[3] a.[4] a.[7] a.[9]
                                    enforceLexigraphicalOrderingBetweenBranches a.[5] a.[6] a.[10] a.[12]
                                    enforceLexigraphicalOrderingBetweenBranches r.[1] r.[2] r.[3] r.[5]
                                    
                                    fixMaxActivators maxActivators
                                    fixMaxRepressors maxRepressors |]
    (circuitEncoding, symVars)
 
let private evaluateUpdateFunction = 
    let counter = ref 0 // THIS IS LIKELY TO BE A PROBLEM, COUNTER IS INCREASED ACROSS ALL MY Z3 FUNCTIONS
    
    fun (symVars : BitVec []) (geneValues : bool []) ->
        let i = !counter
        counter := i + 1

        let geneValues = Array.map (fun b -> if b then True else False) geneValues

        let intermediateValueVariables = [| Bool <| sprintf "va1_%i" i; Bool <| sprintf "va2_%i" i; Bool <| sprintf "va3_%i" i; Bool <| sprintf "va4_%i" i; Bool <| sprintf "va5_%i" i; Bool <| sprintf "va6_%i" i; Bool <| sprintf "va7_%i" i
                                            Bool <| sprintf "va8_%i" i; Bool <| sprintf "va9_%i" i; Bool <| sprintf "va10_%i" i; Bool <| sprintf "va11_%i" i; Bool <| sprintf "va12_%i" i; Bool <| sprintf "va13_%i" i; Bool <| sprintf "va14_%i" i; Bool <| sprintf "va15_%i" i
                                            Bool <| sprintf "vr1_%i" i; Bool <| sprintf "vr2_%i" i; Bool <| sprintf "vr3_%i" i
                                            Bool <| sprintf "vr4_%i" i; Bool <| sprintf "vr5_%i" i; Bool <| sprintf "vr6_%i" i; Bool <| sprintf "vr7_%i" i |]

        let andConstraints pi c1i c2i =
            Implies (symVars.[pi] =. AND, intermediateValueVariables.[pi] =. (intermediateValueVariables.[c1i] &&. intermediateValueVariables.[c2i]))

        let orConstraints pi c1i c2i =
            Implies (symVars.[pi] =. OR, intermediateValueVariables.[pi] =. (intermediateValueVariables.[c1i] ||. intermediateValueVariables.[c2i]))
        
        let variableConstraints =
            let f i symVar =
                [| for v in 2 .. (NOTHING - 1) do
                        yield Implies (symVar =. v, intermediateValueVariables.[i] =. geneValues.[v - 2])
                |] |> And

            Array.mapi f symVars |> And

        let circuitValue =
            If (symVars.[15] =. NOTHING, // if no repressors, evaluate to the activating circuit, else evaluate to activating AND (NOT repressing)
                intermediateValueVariables.[0],
                And [| intermediateValueVariables.[0] =. True
                       intermediateValueVariables.[15] =. False
                    |])

        let circuitVal = Bool <| sprintf "circuit_%i" i
                        
        (And [| variableConstraints
                andConstraints 0 1 2
                andConstraints 1 3 4
                andConstraints 2 5 6
                andConstraints 3 7 8
                andConstraints 4 9 10
                andConstraints 5 11 12
                andConstraints 6 13 14

                orConstraints 0 1 2
                orConstraints 1 3 4
                orConstraints 2 5 6
                orConstraints 3 7 8
                orConstraints 4 9 10
                orConstraints 5 11 12
                orConstraints 6 13 14

                andConstraints 15 16 17
                andConstraints 16 18 19
                andConstraints 17 20 21
                orConstraints 15 16 17
                orConstraints 16 18 19
                orConstraints 17 20 21

                circuitVal =. circuitValue|], circuitVal)

let circuitEvaluatesToSame gene symVars (profile : bool []) =
    let b = (fun b -> if b then True else False) profile.[gene - 2]
    let evaluationEncoding, circuitVal = evaluateUpdateFunction symVars profile
    (evaluationEncoding, circuitVal =. b)
            
let circuitEvaluatesToDifferent gene symVars (profile : bool []) =
    let b = (fun b -> if b then True else False) profile.[gene - 2]
    let evaluationEncoding, circuitVal = evaluateUpdateFunction symVars profile
    (evaluationEncoding, circuitVal =. Not b)