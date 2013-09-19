module Cell
open System
open ModelParameters

type PathwayLevel = Up | Down | Neutral
type CellState = Stem | NonStem | NonStemWithMemory | Death
type CellAction = AsymSelfRenew | SymSelfRenew | NonStemDivide | Die | NoAction

let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

let uniform_bool(prob: float) =
    let randgen = new Random(seed())
    randgen.NextDouble() < prob

type ExternalState() =
    let mutable egf = true
    let (_, maxo2) = ExternalState.O2Limits
    let mutable o2 = maxo2
    let mutable live_cells: int = 1
    let mutable stem_cells: int = 1
    
    static member O2Limits with get() = (float 0, float 100)

    member this.EGF with get() = egf and set(_egf) = egf <- _egf
    member this.O2 with get() = o2 and set(_o2: float) = o2 <- _o2
    member this.LiveCells with get() = live_cells and set(n) = live_cells <- n
    member this.StemCells with get() = stem_cells and set(n) = stem_cells <- n

type Cell (?s, ?gen) =
    let mutable ng2: PathwayLevel = Up
    let mutable cd133: PathwayLevel = Up
//    let mutable egfr: PathwayLevel = Up
    let mutable state: CellState = Stem
    let mutable next_state: CellState = Stem
    let mutable action: CellAction = NoAction
    let mutable time_in_state = 0
    let mutable generation = 0

    let set_state(s: CellState) =
        state <- s
        time_in_state <- 0

        if s = Stem then
            ng2 <- Up; cd133 <- Up
            (*let rand = random_bool()
            egfr <- if rand = 0 then Up else if rand = 1 then Down else Neutral*)
        else if s = NonStem then
            ng2 <- Down; cd133 <- Down
        else if s = NonStemWithMemory then
            ng2 <- Down; cd133 <- Up

    do
        set_state(defaultArg s Stem)
        generation <- defaultArg gen 0

    member this.NG2 with get() = ng2
    member this.CD133 with get() = cd133
//  member this.EGFR with get() = egfr
    member this.State with get() = state and set(s) = set_state(s)
    member this.StateAsStr with get() =
                            match state with
                            | Stem -> "Stem"
                            | NonStem -> "Non-stem"
                            | NonStemWithMemory -> "Non-stem with memory"
                            | Death -> "Dead"

    member this.NextState with get() = next_state and set(s) = next_state <- s
    member this.Action with get() = action and set(a) = action <- a
    member this.TimeInState with get() = time_in_state

    member this.ResetState(s) =
        time_in_state <- 0
        if (state <> s) then set_state(s)
    
    member this.Generation with get() = generation and set(g) = generation <- g

exception InnerError of string

type CellActivity() =
    // in assymetric cell division a stem cell produces
    // a new stem cell and a new non-stem cell
    static member asym_divide(cell: Cell) = 
             let non_stem_daughter = new Cell(NonStem, cell.Generation + 1)
             let stem_daughter = new Cell(Stem, cell.Generation + 1)
             [|non_stem_daughter; stem_daughter|]

    // in proliferation a non-stem cell produces
    // two identical non-stem cells
    static member sym_divide(cell: Cell) = 
        let daughter1 = new Cell(cell.State, cell.Generation + 1)
        let daughter2 = new Cell(cell.State, cell.Generation + 2)
        [|daughter1; daughter2|]

    // die function so far does nothing
    static member die(cell: Cell) =
        cell.State <- Death

    // determine if a cell can proliferate depending
    // on the amount of nutrients (currently oxygen: O2)
    static member can_divide(cell: Cell, ext: ExternalState) =
        let param = match cell.State with
                     | Stem -> ModelParameters.StemDivisionProbParam
                     | NonStem -> ModelParameters.NonStemDivisionProbParam
                     | _ -> raise(InnerError(sprintf "A cell in state %s can not divide" cell.StateAsStr))

        let prob = ModelParameters.logistic_func(ModelParameters.logistic_func_param(param))(ext.O2)
        uniform_bool(prob)

    // determine whether a stem cell will divide symmetrically
    static member should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.SymRenewProb)

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member should_die(cell: Cell, ext: ExternalState) = 
        let prob = float 1 - ModelParameters.logistic_func(ModelParameters.logistic_func_param(ModelParameters.DeathProbParam))(-ext.O2)
        uniform_bool(prob)

    // determine if a stem cell should go to a "non-stem with memory" state
    static member should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProbParam
        let randgen = Random(seed())
        randgen.NextDouble() < prob

    static member should_returnto_stem(cell: Cell, ext: ExternalState) =
        let prob = ModelParameters.exp_func(
                     ModelParameters.exp_func_param(
                        ModelParameters.NonStemToStemProbParam))
                        (float -ext.StemCells)
        uniform_bool(prob)


    // compute the action in the current state
    static member compute_action(ext: ExternalState) (cell: Cell) = 
        // no action is taken by default
        cell.Action <- NoAction

        // determine an action for a stem cell
        if cell.State = Stem then
            if ext.EGF && CellActivity.can_divide(cell, ext) then
                if CellActivity.should_divide_sym(cell) then
                    cell.Action <- SymSelfRenew
                else
                    cell.Action <- AsymSelfRenew
            else if CellActivity.should_goto_nonstem(cell) then
                cell.State <- NonStemWithMemory
        
        // determine an action for a non-stem cell
        else if cell.State = NonStem then
            if ext.EGF && CellActivity.can_divide(cell, ext) then
                cell.Action <- NonStemDivide
            else if CellActivity.should_die(cell, ext) then
                cell.Action <- Die

        // determine an action for a non-stem cell which can become stem again
        else if cell.State = NonStemWithMemory then
            if ext.EGF && CellActivity.should_returnto_stem(cell, ext) then
                cell.State <- Stem

    // recalculate the probabilistic events in the external system: EGF and O2
    static member recalculate_ext_state(ext:ExternalState, dt: int) =
        let randgen = new Random(seed())
        ext.EGF <- randgen.NextDouble() < ModelParameters.EGFProb
        let (c1, c2) = ModelParameters.O2Param

        let (minO2, maxO2) = ExternalState.O2Limits
        ext.O2 <- ext.O2 + (float dt)*c1 - (float dt)*c2*(float ext.LiveCells)
        if ext.O2 < minO2 then ext.O2 <- minO2
        else if ext.O2 > maxO2 then ext.O2 <- maxO2       