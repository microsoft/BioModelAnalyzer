module DataLoading

open FSharp.Data
open FSharp.Data.Csv
open FSharp.Data.Csv.Extensions

let [<Literal>] HOME_DIR = "C:/Users/Steven/" // remove

type EdgeChanges = CsvProvider<"cmpNewEdgeChanges.csv">

let readLines filePath = System.IO.File.ReadAllLines(filePath)

let private EDGES_FILENAME = System.Environment.GetCommandLineArgs().[2]

let geneTransitions =
    let csv = EdgeChanges.Load(EDGES_FILENAME)
    fun gene ->
        let rowsWhereGeneChanges = csv.Filter(fun row -> row.Gene = gene).Data
        let seen = System.Collections.Generic.HashSet<string>() // massive hack

        [| for row in rowsWhereGeneChanges do
            if not (seen.Contains row.StateB) then
                seen.Add(row.StateA) |> ignore
                yield (row.StateA, row.StateB) |]

let statesWithGeneTransitions =
    let csv = EdgeChanges.Load(EDGES_FILENAME)
    fun gene ->
        let rowsWhereGeneChanges = csv.Filter (fun row -> row.Gene = gene)
        Seq.map (fun (r : EdgeChanges.Row) -> r.StateA) rowsWhereGeneChanges.Data |> Set.ofSeq

// all loading should be moved to Program.fs

let getExpressionProfiles (statesFilename : string) nonTransitionEnforcedStates (geneNames : string []) =
    let csv = CsvFile.Load(statesFilename).Cache()

    fun gene ->
        let temp = csv.Headers
        let statesWithGeneTransitions = statesWithGeneTransitions geneNames.[gene - 2] // GET RID OF -2 EVERYWHERE
        let rowsWithTransitions (row : FSharp.Data.Csv.CsvRow) = Set.contains row.Columns.[0] statesWithGeneTransitions
        let rowsWithoutTransitions (row : FSharp.Data.Csv.CsvRow) =
            (not <| Set.contains (row.Columns.[0]) statesWithGeneTransitions) &&
            (Set.contains (row.Columns.[0]) nonTransitionEnforcedStates)

        let expressionProfilesWithGeneTransitions = csv.Filter rowsWithTransitions
        let expressionProfilesWithoutGeneTransitions = csv.Filter rowsWithoutTransitions

        let temp1 = expressionProfilesWithGeneTransitions.Data |> Seq.length
        let temp2 = expressionProfilesWithoutGeneTransitions.Data |> Seq.length

        expressionProfilesWithGeneTransitions, expressionProfilesWithoutGeneTransitions

let rowToArray (row : CsvRow) =
    let dropFirstColumn = Seq.skip 1
    dropFirstColumn row.Columns |> Seq.map (System.Boolean.Parse) |> Array.ofSeq

let printEdges sq = Seq.map (sprintf "%A; ") sq |> Seq.fold (+) ""

let tempLoadOFile filename =
    let lines = readLines filename

    let g (str : string) =
        let strs = str.Split(',')
        if Array.length strs <> 2 then failwith "incorrect length"
        (strs.[0].Trim(), strs.[1].Trim())

    let f (str : string) =
        let str = str.Replace("(", "")
        let str = str.Replace(")", "")
        let str = str.Replace("\"", "")
        //str.
        let str = str.TrimEnd(';', ' ')
        str.Split(';') |> List.ofArray |> List.map g
        
    Array.map f lines

let tempLoadPathFile filename =
    let lines = readLines filename
    
    let f (str : string) =
        let str = str.Replace("\"", "")
        let str = str.Replace(" ", "")
        let str = str.TrimEnd(';')
        str.Split(';') |> List.ofArray

    Seq.map f lines