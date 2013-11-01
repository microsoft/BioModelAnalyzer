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
                            | Stem -> !ModelParameters.StemDivisionProbParam
                            | NonStem -> !ModelParameters.NonStemDivisionProbParam
                            | _ -> raise(InnerError(sprintf "%s cell can not divide" (cell.TypeAsStr())))

        let prob_density = !ModelParameters.DivisionProbOnCellDensity

        if (cell.WaitBeforeDivide > 0) then
            false
        else
            let loc = cell.Location
            let prob = prob_oxygen.Y(ext.O2.GetValue(loc)) * prob_density.Y(ext.CellPackDensity.GetValue(loc))
            uniform_bool(prob)

    // determine whether a stem cell will divide symmetrically
    static member private should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.SymRenewProb)

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member private should_die(cell: Cell, ext: ExternalState) = 
        let prob_oxygen = (!ModelParameters.DeathProbOnO2Param).Y(ext.O2.GetValue(cell.Location))
        let prob_density = (!ModelParameters.DeathProbDependOnCellDensity).Y(ext.CellPackDensity.GetValue(cell.Location))
        // we calculate the probability of death based on several parameters independently
        // i.e. based on the amount of oxygen and based on cell density
        //
        // we then take the maximum of these probabilities, which means that
        // if at least one critical event occurs (i.e. the oxygen is too small or there are too many cells)
        // then the cell decides to die
        let prob = max prob_oxygen prob_density
        let decision = uniform_bool(prob)
        decision

    // determine if a stem cell should go to a "non-stem with memory" state
    static member private should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProbParam
        uniform_bool(prob)

    static member private should_returnto_stem(cell: Cell, ext: ExternalState) =
        let prob = (!ModelParameters.NonStemToStemProbParam).Y(float -ext.StemCells)
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
        try
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
        with
            | :? InnerError as error -> raise error

    static member calc_cellpackdensity(ext: ExternalState, cells: Cell[]) =
//        let k = 8.
//        let d = k*ModelParameters.AverageCellR
        let grid = ext.CellPackDensity.Grid
        let imax = grid.XLines-1
        let jmax = grid.YLines-1

        for i = 0 to imax do
            for j = 0 to jmax do
                let p = grid.Point(i, j)
                let rect = Rectangle(left = p.x-grid.Dx/2., right = p.x+grid.Dx/2., top = p.y-grid.Dy/2., bottom = p.y+grid.Dy/2.)
                let neighbours = ExternalState.GetCellsInMesh(cells, rect)
                let r = ModelParameters.AverageCellR
                let k = (grid.Dx*grid.Dy) / (pi*r*r)
                let density = (float neighbours.Length) * k
                ext.CellPackDensity.SetValue(p, density)
        ext.CellPackDensity.ComputeInterpolant()

    static member private calc_o2(ext: ExternalState, cells: Cell[], dt: int) =
        let (supply_rate, c2, c3) = ModelParameters.O2Param
        let o2_limits = ExternalState.O2Limits
        let grid = ext.O2.Grid
        let nabla_square = Derivative.Derivative_2_3.NablaSquare(ext.O2.F, [|grid.Dx; grid.Dy|])

        for i=0 to grid.XLines-1 do
            for j=0 to grid.YLines-1 do

                let p = grid.Point(i, j)
                let rect = grid.CenteredRect(i, j)
                let live_cells = ExternalState.GetCellsInMesh(cells, rect).Length
                let dividing_cells = ExternalState.GetCellsInMesh(cells, rect, any_state, divide_action).Length
                let non_dividing_cells = live_cells - dividing_cells

                let consumption_rate = c2*(float dividing_cells) - c3*(float non_dividing_cells)
                let mutable new_value = ext.O2.GetValue(p) +
                                        (float dt)*(ModelParameters.DiffusionCoeff * nabla_square.[i, j] +
                                                    supply_rate - consumption_rate)

                if new_value < o2_limits.Min then new_value <- o2_limits.Min
                else if new_value > o2_limits.Max then new_value <- o2_limits.Max    

                ext.O2.SetValue(p, new_value)
        ext.O2.ComputeInterpolant()

    // recalculate the probabilistic events in the external system: EGF and O2
    static member recalculate_ext_state(ext:ExternalState, cells: Cell[], dt: int) =
        ext.EGF <- uniform_bool(ModelParameters.EGFProb)
        CellActivity.calc_cellpackdensity(ext, cells)
        CellActivity.calc_o2(ext, cells, dt)