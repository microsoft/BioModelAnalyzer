module ShortestPaths

open System.Collections.Generic

type private Cursor = {
    Focus: string
    Visited: Set<string>
    Previous: Map<string, string> }

let private neighbours graph cursor =
    if not <| Map.containsKey cursor.Focus graph then Seq.empty
    else
        seq { for neighbour in Map.find cursor.Focus graph do
                if not <| Set.contains neighbour cursor.Visited then
                  yield { Focus = neighbour; Visited = Set.add neighbour cursor.Visited; Previous = Map.add neighbour cursor.Focus cursor.Previous } }

let private previousToPath target previous =
    let rec previousToPath target path =
        if not <| Map.containsKey target previous then
            path
        else
            previousToPath (Map.find target previous) (target :: path)

    previousToPath target []
  
// make total
let shortestPathMultiSink graph source sinks =
  let q = Queue<Cursor>([{Focus = source; Visited = Set.singleton source; Previous = Map.empty}])
  let visited = HashSet<string>()
  
  [ while q.Count > 0 && not <| visited.IsSupersetOf sinks do
      let u = q.Dequeue()
      for v in neighbours graph u do
          if not <| visited.Contains v.Focus then
              visited.Add(v.Focus) |> ignore
              q.Enqueue(v)
              if Set.contains v.Focus sinks then yield source :: previousToPath v.Focus v.Previous ]
              // definitely duplication with visited between here and 