module Simulation
open Cell

let mutable t = 0
let dt = 1

let mutable live_cells: Cell[] = [|new Cell()|]
let mutable dead_cells: Cell[] = [||]
let mutable ext_state = new ExternalState()

let DoStep(live_cells: Cell[] byref, dead_cells: Cell[] byref, ext_state: ExternalState, dt: int) = 
    let no_action_cells = Array.filter(fun (c:Cell) -> c.State = NonProliferation || c.State = NonStemWithMemory) live_cells
 
    let new_cells1 = Array.filter(fun (c:Cell) -> c.State = AssymSelfRenewal) live_cells |>
        Array.collect CellActivity.assym_self_renew 

    let new_cells2 = Array.filter(fun (c:Cell) -> c.State = Proliferation) live_cells |>
        Array.collect CellActivity.proliferate

    let new_dead = Array.filter(fun (c:Cell) -> c.State = Death) live_cells |>
        Array.collect CellActivity.die

    live_cells <- Array.concat([no_action_cells; new_cells1; new_cells2])
    dead_cells <- Array.concat([dead_cells; new_dead])
    Array.iter CellActivity.next_state live_cells
    //ext_state.NextState(dt)

let simulate(n: int) =
    for i = 1 to n do
        DoStep(&live_cells, &dead_cells, ext_state, dt)
        t <- t + dt