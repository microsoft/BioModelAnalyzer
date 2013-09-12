module Cell
open System

type PathwayLevel = Up | Down | Neutral
type CellState = AssymSelfRenewal | Proliferation | NonProliferation | NonStemWithMemory | Death

let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

let random_bool() =
    let randgen = Random(seed())
    randgen.Next() % 3

type ExternalState() =
    let mutable egf = false
    let mutable o2 = 100

    member this.EGF with get() = egf and set(_egf) = egf <- _egf
    member this.O2 with get() = o2 and set(_o2) = o2 <- _o2
    //member this.NextState() = 

type Cell (?s, ?gen) =
    let mutable ng2 = Up
    let mutable cd133 = Up
    let mutable egfr = Up
    let mutable state = AssymSelfRenewal
    let mutable time_in_state = 0
    let mutable generation = 0

    let set_state(s) =
        state <- s
        time_in_state <- 0

        if s = AssymSelfRenewal then
            ng2 <- Up; cd133 <- Up;
            let rand = random_bool();
            egfr <- if rand = 0 then Up else if rand = 1 then Down else Neutral
        else if s = Proliferation || s = NonProliferation then
            ng2 <- Down; cd133 <- Down
        else if s = NonStemWithMemory then
            ng2 <- Down; cd133 <- Up

    do
        set_state(defaultArg s AssymSelfRenewal)
        generation <- defaultArg gen 0

    member this.NG2 with get() = ng2
    member this.CD133 with get() = cd133
    member this.EGFR with get() = egfr
    member this.State with get() = state and set(s) = set_state(s)
    member this.TimeInState with get() = time_in_state
    member this.ResetState(s) =
        time_in_state <- 0
        if (state <> s) then set_state(s)
    member this.Generation with get() = generation and set(g) = generation <- g

module CellActivity =

    // in assymetric cell division a stem cell produces
    // a new stem cell and a new non-stem cell
    let assym_self_renew(cell: Cell) = 
        let non_stem_daughter = new Cell(Proliferation, cell.Generation + 1)
        let stem_daughter = new Cell(AssymSelfRenewal, cell.Generation + 1)
        [|non_stem_daughter; stem_daughter|]

    // in proliferation a non-stem cell produces
    // two identical non-stem cells
    let proliferate(cell: Cell) = 
        let daughter1 = new Cell(Proliferation, cell.Generation + 1)
        let daughter2 = new Cell(Proliferation, cell.Generation + 2)
        [|daughter1; daughter2|]

    // die function so far does nothing
    let die(cell: Cell) = cell

    // go to the next state in the state diagram
    let next_state(cell: Cell) =
        cell.State <- AssymSelfRenewal