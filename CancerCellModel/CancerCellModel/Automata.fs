module Automata

open System
open Cell
open ModelParameters
open MyMath
open Geometry

type CellActivity() =
    static member private daughter_locations(cell: Cell, r1: float, r2: float) =
        // choose a random diagonal of cell division, which goes through the cell center
        let angle = uniform_float(FloatInterval(0., 2.*pi))
        let v1 = Vector(cell.Location, angle, r1)
        let v2 = Vector(cell.Location, angle+pi, r2)
        v1.ToPoint(), v2.ToPoint()

    // in assymetric cell division a stem cell produces
    // a new stem cell and a new non-stem cell
    static member asym_divide(cell: Cell) = 
        let r1, r2 = cell.R, cell.R
        let loc1, loc2 = CellActivity.daughter_locations(cell, r1, r2)
        let non_stem_daughter = new Cell(NonStem, cell.Generation + 1, loc1, r1, cell.Density)
        let stem_daughter = new Cell(Stem, cell.Generation + 1, loc2, r2, cell.Density)
        [|non_stem_daughter; stem_daughter|]

    // in syymetric cell division a cell produces two identical daughter cells
    static member sym_divide(cell: Cell) = 
        let r1, r2 = cell.R, cell.R
        let loc1, loc2 = CellActivity.daughter_locations(cell, r1, r2)
        let daughter1 = new Cell(cell.Type, cell.Generation + 1, loc1, r1, cell.Density)
        let daughter2 = new Cell(cell.Type, cell.Generation + 1, loc2, r2, cell.Density)
        [|daughter1; daughter2|]

    // die function so far does nothing
    static member die(cell: Cell) =
        cell.State <- Dead

    // determine if a cell can proliferate depending
    // on the amount of nutrients (currently oxygen: O2)
    static member private can_divide(cell: Cell, ext: ExternalState) =
        let prob_oxygen = match cell.Type with
                            | Stem -> ModelParameters.StemDivisionProbParam
                            | NonStem -> ModelParameters.NonStemDivisionProbParam
                            | _ -> raise(InnerError(sprintf "%s cell can not divide" (cell.TypeAsStr())))

        let prob_density = ModelParameters.DivisionProbDependOnCellDensity

        if (cell.WaitBeforeDivide > 0) then
            false
        else
            let prob = prob_oxygen.Y(ext.O2) * prob_density.Y(ext.CellDensity)
            uniform_bool(prob)

    // determine whether a stem cell will divide symmetrically
    static member private should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.SymRenewProb)

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member private should_die(cell: Cell, ext: ExternalState) = 
        let prob_oxygen = ModelParameters.DeathProbParam.Y(ext.O2)
        let prob_density = ModelParameters.DeathProbDependOnCellDensity.Y(ext.CellDensity)
        // we calculate the probability of death based on several parameters independently
        // i.e. based on the amount of oxygen and based on cell density
        //
        // we then take the maximum of these probabilities, which means that
        // if at least one critical event occurs (i.e. the oxygen is too small or there are too many cells)
        // then the cell decides to die
        let prob = max prob_oxygen prob_density
        uniform_bool(prob)

    // determine if a stem cell should go to a "non-stem with memory" state
    static member private should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProbParam
        uniform_bool(prob)

    static member private should_returnto_stem(cell: Cell, ext: ExternalState) =
        let prob = ModelParameters.NonStemToStemProbParam.Y(float -ext.StemCells)
        uniform_bool(prob)

    static member initialise_new_cell(cell: Cell) =
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

    (*static member private grid_point(i: int, j: int) =
        let size = ModelParameters.GridSize

    static member private calc_cellpackdensity(ext: ExternalState) =
        let k = 4.
        let imax = ext.CellPackDensity.GetLength(1)-1
        let jmax = ext.CellPackDensity.GetLength(2)-1

        for i = 0 to imax do
            for j = 0 to jmax do
                (*let point = grid_point(i, j)
                let neighbours = ExternalState.GetCellsInRegion(point, cells, k*cell.R)
                // we add one to the length to count the cell itself*)
                ext.CellPackDensity.[i,j] <- 0.//float (neighbours.Length+1) / (k*k*k)*)

    // recalculate the probabilistic events in the external system: EGF and O2
    static member recalculate_ext_state(ext:ExternalState, cells: Cell[], dt: int) =
        ext.EGF <- uniform_bool(ModelParameters.EGFProb)
        let (c1, c2, c3, _) = ModelParameters.O2Param

        let o2_limits = ExternalState.O2Limits
        ext.O2 <- ext.O2 + (float dt)*(c1 - c2*(float ext.DividingCells) - c3*float (ext.LiveCells - ext.DividingCells))
        if ext.O2 < o2_limits.Min then ext.O2 <- o2_limits.Min
        else if ext.O2 > o2_limits.Max then ext.O2 <- o2_limits.Max    