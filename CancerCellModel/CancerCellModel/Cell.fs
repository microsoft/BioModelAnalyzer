module Cell
open System

type PathwayLevel = Up | Down | Neutral
type CellState = Stem | NonStem | NonStemWithMemory | Death
type CellAction = AsymSelfRenew | SymSelfRenew | NonStemDivide | Die | NoAction

let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

type ExternalState() =
    let mutable egf = true
    let mutable o2 = float ExternalState.MaxO2
    
    static member MaxO2 with get() = float 100
    static member MinO2 with get() = float 0

    member this.EGF with get() = egf and set(_egf) = egf <- _egf
    member this.O2 with get() = o2 and set(_o2: float) = o2 <- _o2
    
    //member this.NextState() = 

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

// model calibration parameters
type ModelParameters =
    // the probability of cell division is defined as 1 / (max + exp((mu - x)/s)),
    // this is logistic probability function, and there is no biological reason to choose it
    // but its shape and and the fact that it's between 0 and 1 are convenient for our purposes
    // we'll use s and mu as a calibration parameter (DivisionProbParam), x will mean O2,
    // and mu - the number of cells in the system
    static member StemDivisionProbParam = (float 60, float 100, float 0.015) // (mu, s, max)
    static member NonStemDivisionProbParam = (float 60, float 100, float 1) // (mu, s, max)

    // the probability that EGF is present
    static member EGFProb: float = 0.8

    // the probability of cell death is defined as c^(-x)
    // where ^ means "to the power of" and x will mean O2
    static member DeathProbParam = float 2 // c

    // the probability that a stem cell will divide symmetrically
    // rather than asymmetrically
    static member SymRenewProbParam = float 0.01

    // the level of O2 is modeled as the function
    // f(n, t+dt) = f(n, t) + dt*c1 - dt*c2*n
    // where dt*c1 - income of O2 in the system
    // dt*c2*n - consumption of O2 - linear in time and number of cells in the model (n)
    static member O2Param = (float 1, float 0.01) // (c1, c2)
    static member logistic_func(mu: float, s: float, max: float)(x: float) =
        float 1 / (float 1/max + exp((mu-x)/s))

    static member logistic_func_param(x1: float, x2: float, max: float) =
        let l1 = Math.Log(float 99/max)
        let l2 = Math.Log(float 1 - max)
        let s = (x2-x1)/(l1-l2)
        let mu = s * l1 + x1
        (mu, s, max)


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

        // if there is too little oxygen, a cell does not proliferate
        let prob = ModelParameters.logistic_func(param)(ext.O2)
        let randgen = Random(seed())
        randgen.NextDouble() < prob

    // determine whether a stem cell will divide symmetrically
    static member should_divide_sym(cell: Cell) =
        let randgen = Random(seed())
        randgen.NextDouble() < ModelParameters.SymRenewProbParam

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member should_die(cell: Cell, ext: ExternalState) = 
        let prob = Math.Pow(ModelParameters.DeathProbParam, -ext.O2)
        let randgen = Random(seed())
        randgen.NextDouble() < prob

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
            else
                cell.State <- NonStemWithMemory
        
        // determine an action for a non-stem cell
        else if cell.State = NonStem then
            if ext.EGF && CellActivity.can_divide(cell, ext) then
                cell.Action <- NonStemDivide
            else if CellActivity.should_die(cell, ext) then
                cell.Action <- Die

        // determine an action for a non-stem cell which can become stem again
        else if cell.State = NonStemWithMemory && ext.EGF = true then
            cell.State <- Stem

    // recalculate the probabilistic events in the external system: EGF and O2
    // n is the total number of live cells in the model
    static member recalculate_ext_state(ext:ExternalState, dt: int, n: int) =
        let randgen = new Random(seed())
        ext.EGF <- randgen.NextDouble() < ModelParameters.EGFProb
        let (c1, c2) = ModelParameters.O2Param

        ext.O2 <- ext.O2 + (float dt)*c1 - (float dt)*c2*(float n)
        if ext.O2 < ExternalState.MinO2 then ext.O2 <- ExternalState.MinO2
        else if ext.O2 > ExternalState.MaxO2 then ext.O2 <- ExternalState.MaxO2        