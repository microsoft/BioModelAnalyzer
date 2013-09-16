module Locations

[<Struct>]
type Pt(xx:float,yy:float) =
    member this.x = xx
    member this.y = yy
    override this.ToString() = "Pt(" + string xx + "," + string yy + ")"

[<Struct>]
type GridRef(xx:int,yy:int) =
    member this.x = xx
    member this.y = yy
    override this.ToString() = "Grid(" + string xx + "," + string yy + ")"

open System.Collections.Generic

type LocationTable<'a>(d:float) =
    let table = Dictionary<GridRef,HashSet<'a>>()
    let mutable nhbrs = null // zap to null to flush cache, otherwise = Dictionary<GridRef,'a[]>()
    let pointGridRef (pt:Pt) = GridRef(int (pt.x / d),int (pt.y / d))
    let addItem (x:'a) pt =
        nhbrs <- null
        let gref = pointGridRef pt
        if table.ContainsKey(gref) then
            table.[gref].Add x |> ignore
        else
            table.[gref] <- new HashSet<'a>( [| x |] )
    let removeItem (x:'a) pt =
        nhbrs <- null
        let gref = pointGridRef pt
        if table.ContainsKey(gref) then
            table.[gref].Remove x |> ignore<bool>
        else
            failwith (sprintf "LocationTable.rmItem: item not located in table, likely an error, x=%A" x)    
    let moveItem x oldPt newPt =
        if pointGridRef oldPt <> pointGridRef newPt then
            nhbrs <- null
            removeItem x oldPt
            addItem    x newPt
    let lookup gref = if table.ContainsKey(gref) then table.[gref] :> seq<'a> else Seq.empty
    member this.Clear() = table.Clear(); nhbrs <- null
    member this.Add(x,pt)    = addItem x pt
    member this.Remove(x,pt) = removeItem x pt
    member this.Move(x,oldPt,newPt) = moveItem x oldPt newPt
    member this.PossibleNeighbours(pt:Pt) =
        lock this (fun () -> if nhbrs=null then nhbrs <- Dictionary<GridRef,'a[]>())
        let gref       = pointGridRef pt
        if nhbrs.ContainsKey(gref) then
            nhbrs.[gref] |> Seq.toArray
        else
            let localGrefs = seq { for dx in -1 .. 1 do
                                   for dy in -1 .. 1 do
                                   yield GridRef(gref.x + dx,gref.y + dy) }
            lock this (fun () -> 
                let localCells = Seq.collect lookup localGrefs |> Seq.toArray
                nhbrs.[gref] <- localCells
                localCells)
    member this.Table() = table |> Seq.map (fun (KeyValue (gref,items)) -> gref,items |> Seq.toArray) |> Seq.toArray
    member this.Count   = table |> Seq.map (fun (KeyValue (gref,items)) -> items.Count) |> Seq.sum (* not quick! *)
    
    
#if TESTING
let tab = LocationTable<string>(1.0)
let testAdd (x,y) = tab.Add(sprintf "%f-%f" x y,Pt(x,y))
let ran = System.Random()
for i = 0 to 100 do testAdd (ran.NextDouble() * 10.0,ran.NextDouble() * 10.0) 
tab.Table()
tab.PossibleNeighbours(Pt(2.2,2.1)) |> Seq.toArray
tab.Count
#endif


