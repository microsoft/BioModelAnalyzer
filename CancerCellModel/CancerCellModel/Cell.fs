module Cell
open System
open ModelParameters

type PathwayLevel = Up | Down | Neutral
type CellType = Stem | NonStem | NonStemWithMemory
type CellState = Functioning | PreparingToDie | Dead
type CellAction = AsymSelfRenewal | SymSelfRenewal | NonStemDivision | Death | NoAction

exception InnerError of string

let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

let uniform_bool(prob: float) =
    let randgen = new Random(seed())
    randgen.NextDouble() < prob

let uniform_int(xmin, xmax) =
    let randgen = new Random(seed())
    randgen.Next(xmin, xmax+1)

type ExternalState() =
    let mutable egf = true
    let (_, maxo2) = ExternalState.O2Limits
    let mutable o2 = maxo2
    let mutable live_cells: int = 0
    let mutable dividing_cells: int = 0
    let mutable stem_cells: int = 0
    
    static member O2Limits with get() = (float 0, float 100)

    member this.EGF with get() = egf and set(_egf) = egf <- _egf
    member this.O2 with get() = o2 and set(_o2: float) = o2 <- _o2
    member this.LiveCells with get() = live_cells and set(n) = live_cells <- n
    member this.DividingCells with get() = dividing_cells and set(x) = dividing_cells <- x
    member this.NonDividingLiveCells with get() = live_cells - dividing_cells
    member this.StemCells with get() = stem_cells and set(n) = stem_cells <- n
    member this.CellDensity with get() = float live_cells / float ModelParameters.MaxNumOfCells

type Point (xx: float, yy: float, zz: float) = 
    member this.x = xx
    member this.y = yy
    member this.z = zz

    new() = Point(0., 0., 0.)
    //override this.ToString() = "Pt(" + string xx + "," + string yy + ")"

type Cell (t, ?gen) =
    let mutable ng2: PathwayLevel = Up
    let mutable cd133: PathwayLevel = Up
//    let mutable egfr: PathwayLevel = Up
    let mutable cell_type: CellType = t
    let mutable state: CellState = Functioning
    let mutable next_state: CellState = Functioning
    let mutable action: CellAction = NoAction
    let mutable time_in_state = 0
    let mutable generation = 0
    let mutable wait_before_divide = 0
    let mutable wait_before_die = 0
    let mutable steps_after_last_division = 0
    let mutable position = Point()

    let init() =
        match t with
        | Stem -> ng2 <- Up; cd133 <- Up
            (*let rand = random_bool()
            egfr <- if rand = 0 then Up else if rand = 1 then Down else Neutral*)
        | NonStem -> ng2 <- Down; cd133 <- Down
        | NonStemWithMemory -> ng2 <- Down; cd133 <- Up

    do
        init()
        generation <- defaultArg gen 0

    member this.NG2 with get() = ng2
    member this.CD133 with get() = cd133
//  member this.EGFR with get() = egfr
    member this.State with get() = state and set(s) = state <- s; time_in_state <- 0
    member this.Type with get() = cell_type and set(t) = cell_type <- t; time_in_state <- 0
    member this.TypeAsStr() =
                            match cell_type with
                            | Stem -> "Stem"
                            | NonStem -> "Non-stem"
                            | NonStemWithMemory -> "Non-stem with memory"

    static member TypeAsStr(t: CellType) =
        let cell = new Cell(t)
        cell.TypeAsStr()

    member this.NextState with get() = next_state
                            and set(s) = next_state <- s;


    member this.Action with get() = action and set(a) = action <- a
    member this.TimeInState with get() = time_in_state
    
    member this.Generation with get() = generation and set(g) = generation <- g
    member this.WaitBeforeDivide with get() = wait_before_divide and set(x) = wait_before_divide <- x
    member this.WaitBeforeDie with get() = wait_before_die and set(x) = wait_before_die <- x
    member this.StepsAfterLastDivision with get() = steps_after_last_division and set(x) = steps_after_last_division <- x

type CellActivity() =
    // in assymetric cell division a stem cell produces
    // a new stem cell and a new non-stem cell
    static member asym_divide(cell: Cell) = 
             let non_stem_daughter = new Cell(NonStem, cell.Generation + 1)
             let stem_daughter = new Cell(Stem, cell.Generation + 1)
             [|non_stem_daughter; stem_daughter|]

    // in syymetric cell division a cell produces two identical daughter cells
    static member sym_divide(cell: Cell) = 
        let daughter1 = new Cell(cell.Type, cell.Generation + 1)
        let daughter2 = new Cell(cell.Type, cell.Generation + 2)
        [|daughter1; daughter2|]

    // die function so far does nothing
    static member die(cell: Cell) =
        cell.State <- Dead

    // determine if a cell can proliferate depending
    // on the amount of nutrients (currently oxygen: O2)
    static member can_divide(cell: Cell, ext: ExternalState) =
        let param = match cell.Type with
                     | Stem -> ModelParameters.StemDivisionProbParam
                     | NonStem -> ModelParameters.NonStemDivisionProbParam
                     | _ -> raise(InnerError(sprintf "%s cell can not divide" (cell.TypeAsStr())))

        if (cell.WaitBeforeDivide > 0) then
            false
        else
            let prob = ModelParameters.logistic_func(ModelParameters.logistic_func_param(param))
                                        ((*CellActivity.oxygen_per_cell(ext)*)ext.O2) *
                        ModelParameters.logistic_func(ModelParameters.logistic_func_param(ModelParameters.DivisionProbDependOnCellDensity))
                                        (ext.CellDensity)
            uniform_bool(prob)

    // determine whether a stem cell will divide symmetrically
    static member should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.SymRenewProb)

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member should_die(cell: Cell, ext: ExternalState) = 
        let prob1 = ModelParameters.logistic_func(ModelParameters.logistic_func_param(ModelParameters.DeathProbParam))
                                     ((*CellActivity.oxygen_per_cell(ext)*)ext.O2)

        let prob2 = ModelParameters.logistic_func(ModelParameters.logistic_func_param(ModelParameters.DeathProbDependOnCellDensity))
                                                                                        (ext.CellDensity)

        let prob = max prob1 prob2
        uniform_bool(prob)

    // determine if a stem cell should go to a "non-stem with memory" state
    static member should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProbParam
        uniform_bool(prob)

    static member should_returnto_stem(cell: Cell, ext: ExternalState) =
        let prob = ModelParameters.exp_func(
                     ModelParameters.exp_func_param(
                        ModelParameters.NonStemToStemProbParam))
                        (float -ext.StemCells)
        uniform_bool(prob)

    static member initialise_new_cells(cell: Cell) =
        match cell.Type with
        | Stem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.StemIntervalBetweenDivisions)
        | NonStem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.NonStemIntervalBetweenDivisions)
        | _ -> raise (InnerError(sprintf "Error: new cell in state %s" (cell.TypeAsStr())))


    static member do_step(cell: Cell) =
        if (cell.Action = SymSelfRenewal || cell.Action = AsymSelfRenewal) then
            cell.WaitBeforeDivide <- uniform_int(ModelParameters.StemIntervalBetweenDivisions)
            cell.StepsAfterLastDivision <- 0
        else if cell.Action = NonStemDivision then
            cell.WaitBeforeDivide <- uniform_int(ModelParameters.NonStemIntervalBetweenDivisions)
            cell.StepsAfterLastDivision <- 0
        else
            cell.StepsAfterLastDivision <- (cell.StepsAfterLastDivision + 1)
            if cell.WaitBeforeDivide > 0
                then cell.WaitBeforeDivide <- (cell.WaitBeforeDivide - 1)

        if cell.State = PreparingToDie && cell.WaitBeforeDie > 0 then
            cell.WaitBeforeDie <- (cell.WaitBeforeDie - 1)

    // compute the action in the current state
    static member compute_action(ext: ExternalState) (cell: Cell) = 
        // no action is taken by default
        cell.Action <- NoAction

        if (cell.State = PreparingToDie) then
            if cell.WaitBeforeDie = 0 then
                if CellActivity.should_die(cell, ext) then
                    cell.Action <- Death
                else cell.State <- Functioning
        else
            // determine an action for a stem cell
            match cell.Type with
            | Stem ->
                if ext.EGF && CellActivity.can_divide(cell, ext) then
                    if CellActivity.should_divide_sym(cell) then
                        cell.Action <- SymSelfRenewal
                    else
                        cell.Action <- AsymSelfRenewal
                else if CellActivity.should_goto_nonstem(cell) then
                    cell.Type <- NonStemWithMemory
        
            // determine an action for a non-stem cell
            | NonStem ->
                if ext.EGF && CellActivity.can_divide(cell, ext) then
                    cell.Action <- NonStemDivision
                else if CellActivity.should_die(cell, ext) then
                    cell.WaitBeforeDie <- uniform_int(ModelParameters.DeathWaitInterval)
                    cell.State <- PreparingToDie

            // determine an action for a non-stem cell which can become stem again
            | NonStemWithMemory ->
                if ext.EGF && CellActivity.should_returnto_stem(cell, ext) then
                    cell.Type <- Stem

    // recalculate the probabilistic events in the external system: EGF and O2
    static member recalculate_ext_state(ext:ExternalState, dt: int) =
        ext.EGF <- uniform_bool(ModelParameters.EGFProb)
        let (c1, c2, c3, _) = ModelParameters.O2Param

        let (minO2, maxO2) = ExternalState.O2Limits
        ext.O2 <- ext.O2 + (float dt)*(c1 - c2*(float ext.DividingCells) - c3*float (ext.LiveCells - ext.DividingCells))
        if ext.O2 < minO2 then ext.O2 <- minO2
        else if ext.O2 > maxO2 then ext.O2 <- maxO2       