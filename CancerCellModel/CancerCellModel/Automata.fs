module Automata

open System
open Cell
open ModelParameters
open MathFunctions
open Geometry
open NumericComputations

// SI: rename type to Automata
type CellActivity() =
    static let o2_vals = new ResizeArray<float>(ModelParameters.O2Grid.YLines*ModelParameters.O2Grid.XLines)
    static let o2_nabla_square_vals = new ResizeArray<float>(ModelParameters.O2Grid.YLines*ModelParameters.O2Grid.XLines)

    // get the smallest rectangle embracing the locations of the cells
    // extra is the amount of space added on each side
    static member get_border(cells: ResizeArray<Cell>, extra: float) =
        let xmin, xmax, ymin, ymax = ref 0., ref 0., ref 0., ref 0.

        let update_border = fun (c: Cell) -> 
            if !xmin > c.Location.x then xmin := c.Location.x;
            if !xmax < c.Location.x then xmax := c.Location.x;
            if !ymin > c.Location.y then ymin := c.Location.y;
            if !ymax < c.Location.y then ymax := c.Location.y;

        cells.ForEach(Action<Cell>(update_border))
        Geometry.Rectangle(!xmin-extra, !ymin-extra, !xmax+extra, !ymax+extra)

    static member compute_peripheral(ext: ExternalState, cells: ResizeArray<Cell>) =
        let border = CellActivity.get_border(cells, 3.*ModelParameters.AverageCellR)
        let grid = ext.O2.Grid
        let imin, jmin = grid.PointToIndices(Geometry.Point(border.Left, border.Top))
        let imax, jmax = grid.PointToIndices(Geometry.Point(border.Right, border.Bottom))
        ext.Peripheral.Clear()

        let xmin, xmax = ref 0., ref 0.
        let find_cells_horiz = fun(y: float, dy: float)(c: Cell) ->
            if c.Location.y+c.R > y && c.Location.y-c.R < y + dy then
                if c.Location.x-c.R < !xmin then xmin := c.Location.x-c.R
                if c.Location.x+c.R > !xmax then xmax := c.Location.x+c.R

        for i = imin to imax do
            let y = grid.IndicesToPoint(i, 0).y
            xmin := border.Right
            xmax := border.Left
            cells.ForEach(Action<Cell>(find_cells_horiz(y, grid.Dy)))
            
            if Math.Abs(border.Right - !xmin) > float_error &&
                Math.Abs(!xmax - border.Left) > float_error
            then
                let (_, jmin) = grid.PointToIndices(Geometry.Point(!xmin, y))            
                let (_, jmax) = grid.PointToIndices(Geometry.Point(!xmax, y))
                ext.Peripheral.AddSegment(i, IntInterval(jmin, jmax))

    static member private calc_cellpackdensity_gradient(ext: ExternalState, cells: ResizeArray<Cell>) =
        // calculate the area of cell concentration
        let border = CellActivity.get_border(cells, 4.*ModelParameters.AverageCellR)
        ext.CellConcentrationArea <- border

        let grid_global = ext.CellPackingDensity.Grid        
        //let k = 2.
        //let size =(*2.*k**)ModelParameters.AverageCellR
        let gradient_func = ext.CellPackingDensityGradient
        (!gradient_func).Resize(Geometry.Size(border.Width,border.Height), border.Center)
        // := GridFunctionVector(Grid(border.Width,border.Height, ModelParameters.AverageCellR, ModelParameters.AverageCellR), border.Center)
        let derivative = Derivative.Derivative_1_2
        let grid_local = (!gradient_func).Grid

        // mesh vertices surrounding the point p
        for i_local = 0 to grid_local.YLines-1 do
            for j_local = 0 to grid_local.XLines-1 do
                let point_local = grid_local.IndicesToPoint(i_local, j_local)
                // translate the coordinates of point p from grid2 to grid1
                let point_global =  (!gradient_func).translate_to_global_coord(point_local)
                let (i_global, j_global) = grid_global.PointToIndices(point_global)

                let gradient = derivative.Gradient(ext.CellPackingDensity.F, ext.CellPackingDensity.Delta, i_global, j_global)
                (!gradient_func).SetValue(point_global, gradient)

        // compute the (interpolation of) the gradient in the point p
        (!gradient_func).ComputeInterpolant()


    static member private daughter_locations(cell: Cell, r1: float, r2: float, ext: ExternalState) =
        let loc = cell.Location
        //let gradient = (!ext.CellPackingDensityGradient).GetValue(loc)
        let mutable v1, v2 = Vector(), Vector()

        // choose a diagonal of cell division, which goes through the cell center
        //if gradient.IsNull() then
        // choose a random diagonal
        let angle = uniform_float(FloatInterval(0., 2.*Math.PI))
        v1 <- Vector(cell.Location) + Vector.FromAngleAndRadius(angle, r1)
        v2 <- Vector(cell.Location) + Vector.FromAngleAndRadius(angle+Math.PI, r2)
        (* // the approach below does not work because the gradient is calculated badly
           // the reason is that the numeric approach used to calculate the derivative
           // requires that the density function and the derivative are smooth
           // which is not the case because the density is calculated with low precision for performance reasons
        else
            // place the diagonal of cell division
            // so that it's perpendicular to the gradient of cell density
            // because the gradient shows the direction of the largest growth of density
            let dir = gradient.Perpendicular().Normalise()
            v1 <- Vector(cell.Location) + dir*r1
            v2 <- Vector(cell.Location) - dir*r2
        *)

        v1.ToPoint(), v2.ToPoint()

    // in symmetric cell division a cell produces two identical daughter cells
    // in asymmetric cell division a stem cell produces
    // a new stem cell and a new non-stem cell
    static member divide(ext: ExternalState)(cell: Cell) = 
        let r1, r2 = cell.R, cell.R
        let loc1, loc2 = CellActivity.daughter_locations(cell, r1, r2, ext)
        let type1 = cell.Type
        let type2 = if cell.Action = AsymSelfRenewal then NonStem else cell.Type
        let non_stem_daughter = new Cell(type1, cell.Generation + 1, loc1, r1, cell.Density)
        let stem_daughter = new Cell(type2, cell.Generation + 1, loc2, r2, cell.Density)
        [|non_stem_daughter; stem_daughter|]

    // die function so far does nothing
    static member goto_necrosis(cell: Cell) =
        cell.State <- NecrosisState
        cell.WaitBeforeDesintegration <- uniform_int(ModelParameters.NecrosisDesintegrationInterval)

    static member start_apoptosis(cell: Cell) = 
        cell.State <- ApoptosisState

    // determine if a cell can proliferate depending
    // on the amount of nutrients (currently oxygen: O2)
    static member private can_divide(cell: Cell, ext: ExternalState) =
        if (cell.WaitBeforeDivide > 0) then
            false
        else
            let mutable prob_o2, prob_density = 0., 0.
            let loc = cell.Location
            let o2, density = ext.O2.GetValue(loc), ext.CellPackingDensity.GetValue(loc)

            match cell.Type with
            | Stem ->
                prob_o2 <- (!ModelParameters.StemDivisionProbO2).Y(o2);
                prob_density <- (!ModelParameters.StemDivisionProbDensity).Y(density)

            | NonStem ->
                prob_o2 <- (!ModelParameters.NonStemDivisionProbO2).Y(o2);
                prob_density <- (!ModelParameters.NonStemDivisionProbDensity).Y(density)

            | _ -> raise(InnerError(sprintf "%s cell can not divide" (cell.TypeAsStr())))

            let prob = prob_o2 * prob_density
            uniform_bool(prob)

    // determine whether a stem cell will divide symmetrically
    static member private should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.SymRenewProb)

    // determine if a cell should die depending
    // on the amount of nutrients (currently: O2)
    static member private should_goto_necrosis(cell: Cell, ext: ExternalState) = 
        if cell.Type <> NonStem then raise(InnerError(sprintf "%s cell can not die" (cell.TypeAsStr())))

        let prob_oxygen = (!ModelParameters.NonStemNecrosisProbO2).Y(ext.O2.GetValue(cell.Location))
        //let prob_density = (!ModelParameters.NonStemNecrosisProbDensity).Y(ext.CellPackingDensity.GetValue(cell.Location))
        // we calculate the probability of death based on several parameters independently
        // i.e. based on the amount of oxygen and based on cell density
        //
        // we then take the maximum of these probabilities, which means that
        // if at least one critical event occurs (i.e. the oxygen is too small or there are too many cells)
        // then the cell decides to die
        let prob = prob_oxygen //max prob_oxygen prob_density
        let decision = uniform_bool(prob)
        decision

    static member private should_start_apoptosis(cell: Cell) =
        if cell.Type <> NonStem then raise(InnerError(sprintf "%s cell can not die" (cell.TypeAsStr())))

        let prob = (!ModelParameters.NonStemApoptosisProbAge).Y(float cell.Age)
        let decision = uniform_bool(prob)
        decision

    // determine if a stem cell should go to a "non-stem with memory" state
    static member private should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProb
        uniform_bool(prob)

    static member private should_returnto_stem(cell: Cell, ext: ExternalState) =
        let prob = (!ModelParameters.NonStemToStemProb).Y(float -ext.StemCells)
        uniform_bool(prob)

    static member initialise_new_cell(cell: Cell) =
        match cell.Type with
        | Stem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.StemIntervalBetweenDivisions)
        | NonStem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.NonStemIntervalBetweenDivisions)
        | _ -> raise (InnerError(sprintf "Error: new cell in state %s" (cell.TypeAsStr())))

    // SI: add comments 
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

        if cell.State = PreNecrosisState && cell.WaitBeforeNecrosis > 0 then
            cell.WaitBeforeNecrosis <- (cell.WaitBeforeNecrosis - 1)
        
        cell.Age <- cell.Age + 1

    // compute the action in the current state
    static member compute_action(ext: ExternalState) (cell: Cell) = 
        try
            // no action is taken by default
            cell.Action <- NoAction

            if (cell.State = NecrosisState) then
                if cell.WaitBeforeDesintegration > 0 then
                    cell.WaitBeforeDesintegration <- cell.WaitBeforeDesintegration - 1
                    cell.Action <- NecrosisDesintegration

            else if (cell.State = PreNecrosisState) then
                if cell.WaitBeforeNecrosis = 0 then
                    if CellActivity.should_goto_necrosis(cell, ext) then
                        cell.Action <- GotoNecrosis
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
                    else if CellActivity.should_start_apoptosis(cell) then
                        cell.Action <- StartApoptosis
                    else if CellActivity.should_goto_necrosis(cell, ext) then
                        cell.WaitBeforeNecrosis <- uniform_int(ModelParameters.NecrosisWaitInterval)
                        cell.State <- PreNecrosisState

                // determine an action for a non-stem cell which can become stem again
                | NonStemWithMemory ->
                    if ext.EGF && CellActivity.should_returnto_stem(cell, ext) then
                        cell.Type <- Stem
        with
            | :? InnerError as error ->
                raise (InnerError(sprintf "%s\nCell summary:\n%s" (error.ToString()) (cell.Summary())))

    static member calc_cellpackdensity(ext: ExternalState, cells: ResizeArray<Cell>) =
//        let k = 8.
//        let d = k*ModelParameters.AverageCellR
        let grid = ext.CellPackingDensity.Grid

        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                let p = grid.IndicesToPoint(i, j)
                let rect = Rectangle(left = p.x-grid.Dx/2., right = p.x+grid.Dx/2., top = p.y-grid.Dy/2., bottom = p.y+grid.Dy/2.)
                let neighbours = ExternalState.CountCells(cells, ExternalState.IsCellInMesh(rect))
                let r = ModelParameters.AverageCellR
                let k = grid.Dx / (2.*r) //(grid.Dx*grid.Dy) / (pi*r*r)
                let density = (float neighbours) * k
                ext.CellPackingDensity.SetValue(p, density)
        ext.CellPackingDensity.ComputeInterpolant()
        
        //CellActivity.calc_cellpackdensity_gradient(ext, cells)

    static member private calc_o2(ext: ExternalState, cells: ResizeArray<Cell>, dt: int) =
        let (c1, c2, c3) = ModelParameters.O2Param
        let o2_limits = ModelParameters.O2Limits
        let grid = ext.O2.Grid
        
        // we get these values all at once because they are protected by a semaphore
        // and it's too costly to get them one by one
        // the semaphore is needed because the function values are updated in this thread
        // and read in another one (which renders the ExternalStateForm)
        ext.O2.GetAllValues(o2_vals)
        //let 02_vals' = ext.o2.GetAllValues' () // SI: don't pass buff, but return it. 
        ext.O2NablaSquare.GetAllValues(o2_nabla_square_vals)

        // here we bypass the semaphore for ext.O2NablaSquare.F but this function's values
        // are read and written in this thread only
        Derivative.Derivative_2_1.NablaSquare(ext.O2NablaSquare.F, ext.O2.F, ext.O2.Delta)
        CellActivity.compute_peripheral(ext, cells)

        for i=0 to grid.YLines-1 do
            for j=0 to grid.XLines-1 do
                let p = grid.IndicesToPoint(i, j)
                let rect = grid.CenteredRect(i, j)
                let found_cells = cells.FindAll(Predicate<Cell>(ExternalState.IsCellInMesh(rect)))
                //let live_cells = found_cells.FindAll(fun (c: Cell) -> (Array.exists(fun (s: CellState) -> s = c.State) any_live_state))
                //let dividing_cells = live_cells.FindAll(fun (c: Cell) -> (Array.exists(fun (a: CellAction) -> a = c.Action) divide_action))

                let live_cells_fun = fun (c: Cell) -> (Array.exists(fun (s: CellState) -> s = c.State) any_live_state)
                let numof_live_cells = ExternalState.CountCells(found_cells, live_cells_fun)

                let dividing_cells_fun = fun (c: Cell) -> (Array.exists(fun (a: CellAction) -> a = c.Action) divide_action)
                let numof_dividing_cells = ExternalState.CountCells(found_cells, dividing_cells_fun)
                            
                //let numof_dividing_cells = dividing_cells.Count
                let numof_nondividing_cells = (*live_cells.Count*) numof_live_cells - numof_dividing_cells

                // if the point is inside the tumor, there is no oxygen supply to it
                // (other than diffusion from the neighbouring points)
                let supply_rate = if ext.Peripheral.IsPointStrictlyInside(i, j) then 0. else c1
                let consumption_rate = c2*(float numof_dividing_cells) + c3*(float numof_nondividing_cells)

                let offset = i*grid.XLines+j
                let mutable new_value = o2_vals.[offset] +
                                        (float dt)*(ModelParameters.DiffusionCoeff * o2_nabla_square_vals.[offset] +
                                                    supply_rate - consumption_rate)

                if new_value < o2_limits.Min then new_value <- o2_limits.Min
                else if new_value > o2_limits.Max then new_value <- o2_limits.Max    
                o2_vals.[offset] <- new_value

        ext.O2.SetValues(o2_vals)
        ext.O2.ComputeInterpolant()

    // recalculate the probabilistic events in the external system: EGF and O2
    static member recalculate_ext_state(ext:ExternalState, cells: ResizeArray<Cell>, dt: int) =
        ext.EGF <- uniform_bool(ModelParameters.EGFProb)
        CellActivity.calc_cellpackdensity(ext, cells)
        CellActivity.calc_o2(ext, cells, dt)