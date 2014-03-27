module Automata

open System
open Cell
open ModelParameters
open MathFunctions
open Geometry
open NumericalComputations


// a single object of type GlobalState is created in the program
// GlobalState type contains
//   - the values of the concentration of o2, glucose, their second derivative
//       and the cell density
//   - the number of different type of cells in the model
//   - the border of the tumor represented with a convex polygon
type GlobalState() =

    let mutable egf = true              // indicates whether EGF is Up (true) or Down (false)
    let mutable o2 = GridFunction1D(ModelParameters.O2Grid)     // a grid function which stores the concentration of o2

    let mutable o2_nabla_squared = GridFunction1D(ModelParameters.O2Grid)    // a grid function which stores nabla squared
                                                                             // (the sum of second derivatives for each coordinate)
                                                                             // of the concentration of o2

    let mutable glucose = GridFunction1D(ModelParameters.GlucoseGrid)   // a grid function which stores the concentration of glucose

    let mutable glucose_nabla_squared = GridFunction1D(ModelParameters.GlucoseGrid)    // a grid function which stores nabla squared
                                                                                       // (the sum of second derivatives for each coordinate)
                                                                                       // of the concentration of glucose

    let mutable cell_packing_density =
        GridFunction1D(ModelParameters.CellPackingDensityGrid)  // a grid function which stores the
                                                                // cell packing density

    let mutable peripheral = new Grid2DRegion ()                // a convex polygon embracing
                                                                // the locations of all cells in the model

    let mutable live_cells_num = 0      // the number of cells in state Functioning or PreNecrosis
    let mutable dividing_cells_num = 0  // the number of the cells taking action
                                        // StemAsymmetricDivision, StemSymmetricDivision or NonStemDivision 
    let mutable dying_cells_num = 0     // the number of the cells taking action GotoNecrosis or StartApoptosis

    let mutable stem_cells_num = 0      // the number of stem cells
    let mutable nonstem_cells_num = 0   // the number of non-stem cells
    let mutable nonstem_withmemory_cells_num = 0    // the number of "non-stem with memory" cells

    do
        let grid = o2.Grid

        // initialise the functions of the concentration of 02 and its nabla squared
        for i = 0 to o2.Grid.YLines-1 do
            for j = 0 to o2.Grid.XLines-1 do
                o2.SetValue(grid.IndicesToPoint(i, j), 100.)
                o2_nabla_squared.SetValue(grid.IndicesToPoint(i, j), 0.)
        
        o2.ComputeInterpolant()
        o2_nabla_squared.ComputeInterpolant()

        let grid = glucose.Grid

        // initialise the functions of the concentration of glucose and its nabla squared
        for i = 0 to glucose.Grid.YLines-1 do
            for j = 0 to glucose.Grid.XLines-1 do
                glucose.SetValue(grid.IndicesToPoint(i, j), 100.)
                glucose_nabla_squared.SetValue(grid.IndicesToPoint(i, j), 0.)

        glucose.ComputeInterpolant()
        glucose_nabla_squared.ComputeInterpolant()

        let grid = ModelParameters.CellPackingDensityGrid

        // initialise the function of the cell packing density
        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                cell_packing_density.SetValue(grid.IndicesToPoint(i, j), 0.)
        
        cell_packing_density.ComputeInterpolant()

    member this.EGF with get() = egf and set(x) = egf <- x
    member this.O2 with get() = o2
    member this.O2NablaSquared with get() = o2_nabla_squared
    member this.Glucose with get() = glucose
    member this.GlucoseNablaSquared with get() = glucose_nabla_squared
    member this.Peripheral with get() = peripheral and set(x) = peripheral <- x
    member this.CellPackingDensity with get() = cell_packing_density

    (*static member GetNeighbours(cell: Cell, cells: Cell[], r: float) =
        Array.filter(fun (c: Cell) -> c <> cell && Geometry.distance(c.Location, cell.Location) < r) cells*)

    // determine if some part of the cell is inside the rectangle
    static member IsCellInMesh(rect: Rectangle)(c: Cell) =
        let circle = Geometry.Circle(c.Location, c.R)
        CirclePart.IntersectionNotTriviallyEmpty(circle, rect) &&
                CirclePart(circle, rect).NonEmpty()

    // count the number of the cells for which the predicate func yields true
    static member CountCells(cells: ResizeArray<Cell>, func: Cell->bool) =
        let mutable count = 0
        for c in cells do
            if func(c) then
                count <- count + 1
        count        

    // get verbose information about the concentration of o2 in point p
    // returns a string
    member this.O2Summary(p: Point) =
        // values in the adjoining grid points from which the value is interpolated - displayed for debugging
        let o2s = Array.create 9 0.
        let o2_nabla2 = Array.create 9 0.
        let (i, j) = o2_nabla_squared.Grid.PointToIndices(p)

        sprintf "Oxygen: o2(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f)\n\
                    \tnabla_square=%.3f\n\
                    \t(interpolated from cc=%.3f lb=%.3f lc=%.3f lt=%.3f ct=%.3f rt=%.3f rc=%.3f rb=%.3f cb=%.3f)"
                (o2.GetValue(p, o2s))
                (o2s.[0]) (o2s.[1]) (o2s.[2]) (o2s.[3]) (o2s.[4]) (o2s.[5]) (o2s.[6]) (o2s.[7]) (o2s.[8])
                (o2_nabla_squared.GetValue(i, j, o2_nabla2))
                (o2_nabla2.[0]) (o2_nabla2.[1]) (o2_nabla2.[2]) (o2_nabla2.[3])
                (o2_nabla2.[4]) (o2_nabla2.[5]) (o2_nabla2.[6]) (o2_nabla2.[7]) (o2_nabla2.[8])

    // get verbose information about the concentration of glucose in point p
    // returns a string
    member this.GlucoseSummary(p: Point) =
        //values in the adjoining grid points from which the value is interpolated - displayed for debugging
        let glucoses = Array.create 9 0.
        let glucose_nabla2 = Array.create 9 0.
        let (i, j) = glucose_nabla_squared.Grid.PointToIndices(p)

        sprintf "Gluocose: glucose(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f)\n\
                    \tnabla_square=%.3f\n\
                    \t(interpolated from cc=%.3f lb=%.3f lc=%.3f lt=%.3f ct=%.3f rt=%.3f rc=%.3f rb=%.3f cb=%.3f)"
                (glucose.GetValue(p, glucoses))
                (glucoses.[0]) (glucoses.[1]) (glucoses.[2]) (glucoses.[3]) (glucoses.[4]) (glucoses.[5]) (glucoses.[6]) (glucoses.[7]) (glucoses.[8])
                (glucose_nabla_squared.GetValue(i, j, glucose_nabla2))
                (glucose_nabla2.[0]) (glucose_nabla2.[1]) (glucose_nabla2.[2]) (glucose_nabla2.[3])
                (glucose_nabla2.[4]) (glucose_nabla2.[5]) (glucose_nabla2.[6]) (glucose_nabla2.[7]) (glucose_nabla2.[8])

    // get verbose information about the cell packing density in point p
    // returns a string
    member this.CellPackingDensitySummary(p: Point) =
        let densities = Array.create 9 0.

        sprintf "Cell pack density: density(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f)"
                    (cell_packing_density.GetValue(p, densities))
                    (densities.[0]) (densities.[1]) (densities.[2]) (densities.[3])
                    (densities.[4]) (densities.[5]) (densities.[6]) (densities.[7]) (densities.[8])

    member this.NumofLiveCells with get() = live_cells_num and set(x) = live_cells_num <- x
    member this.NumofDividingCells with get() = dividing_cells_num and set(x) = dividing_cells_num <- x
    member this.NumofDyingCells with get() = dying_cells_num and set(x) = dying_cells_num <- x
    member this.NumofStemCells with get() = stem_cells_num and set(x) = stem_cells_num <- x
    member this.NumofNonstemCells with get() = nonstem_cells_num and set(x) = nonstem_cells_num <- x
    member this.NumofNonstemWithmemoryCells with get() = nonstem_withmemory_cells_num and set(x) = nonstem_withmemory_cells_num <- x

    // Clear() should be called before re-running the simulation
    member this.Clear() = 
        live_cells_num <- 0
        dividing_cells_num <- 0
        dying_cells_num <- 0
        stem_cells_num <- 0
        nonstem_cells_num <- 0
        nonstem_withmemory_cells_num <- 0

        peripheral.Clear()


// A probabilistic discrete-time automata encoding the life cycle of a cell
type Automata() =

    // return the smallest rectangle embracing the locations of the cells
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

    
    // return the smallest convex polygon embracing the locations of the cells
    static member compute_peripheral(glb: GlobalState, cells: ResizeArray<Cell>) =
        
        // get a rude approximation of the tumour border by a rectangle 
        let border = Automata.get_border(cells, 3.*ModelParameters.AverageCellRadius)
        let grid = glb.O2.Grid
        let imin, jmin = grid.PointToIndices(Geometry.Point(border.Left, border.Top))
        let imax, jmax = grid.PointToIndices(Geometry.Point(border.Right, border.Bottom))
        glb.Peripheral.Clear()

        let xmin, xmax = ref 0., ref 0.
        let find_cells_horiz = fun(y: float, dy: float)(c: Cell) ->
            if c.Location.y+c.R > y && c.Location.y-c.R < y + dy then
                if c.Location.x-c.R < !xmin then xmin := c.Location.x-c.R
                if c.Location.x+c.R > !xmax then xmax := c.Location.x+c.R

        // refine the border to a convex polygon
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
                glb.Peripheral.AddSegment(i, IntInterval(jmin, jmax))

    // return the locations of the offspring of a cell
    // located on a random diagonal of division
    static member private daughter_locations(cell: Cell, r1: float, r2: float, glb: GlobalState) =
        let loc = cell.Location
        let mutable v1, v2 = Vector(), Vector()

        // choose a random diagonal of cell division, which goes through the cell center
        let angle = uniform_float(FloatInterval(0., 2.*Math.PI))
        v1 <- Vector(cell.Location) + Vector.FromAngleAndRadius(angle, r1)
        v2 <- Vector(cell.Location) + Vector.FromAngleAndRadius(angle+Math.PI, r2)
        v1.ToPoint(), v2.ToPoint()

    // in symmetric cell division a cell produces two identical daughter cells
    // in asymmetric cell division a stem cell produces
    //      a new stem cell and a new non-stem cell
    static member divide(glb: GlobalState)(cell: Cell) = 
        let r1, r2 = cell.R, cell.R
        let loc1, loc2 = Automata.daughter_locations(cell, r1, r2, glb)
        let type1 = cell.Type
        let type2 = if cell.DivideAction = StemAsymmetricDivision then NonStem else cell.Type
        let non_stem_daughter = new Cell(type1, cell.Generation + 1, loc1, r1, cell.Density)    
        let stem_daughter = new Cell(type2, cell.Generation + 1, loc2, r2, cell.Density)       
        [|non_stem_daughter; stem_daughter|]

    // perform the necrosis action
    static member goto_necrosis(cell: Cell) =
        cell.State <- NecrosisState
        cell.WaitBeforeDesintegration <- uniform_int(ModelParameters.NecrosisDesintegrationInterval)

    // perform the apoptosis action
    static member start_apoptosis(cell: Cell) = 
        cell.State <- ApoptosisState

    static member die_by_radiation(cell: Cell) =
        cell.State <- DeathByRadiation

    // progress through cell cycle
    static member go_to_G0(cell: Cell) = 
        cell.CellCycleStage <- G0

    static member go_to_S(cell: Cell) =
        cell.CellCycleStage <- S
        match cell.Type with
            |Stem -> cell.WaitBeforeG2M <- uniform_int(ModelParameters.StemWaitBeforeG2MInterval)
            |NonStem -> cell.WaitBeforeG2M <- uniform_int(ModelParameters.NonStemWaitBeforeG2MInterval)
            |_ -> raise(InnerError(sprintf "Error: new cell in state %s" (cell.Type.ToString())))
        
    static member go_to_G2M(cell: Cell) =
        cell.CellCycleStage <- G2_M
        match cell.Type with
            |Stem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.StemWaitBeforeDivisionInterval)
            |NonStem -> cell.WaitBeforeDivide <- uniform_int(ModelParameters.NonStemWaitBeforeDivisionInterval)
            |_ -> raise(InnerError(sprintf "Error: new cell in state %s" (cell.Type.ToString())))

    // returns true if the cell can proliferate, which depends on 
    //   - the amount of nutrients (currently oxygen & glucose)
    //   - and cell packing density
    static member private can_divide(cell: Cell, glb: GlobalState) =

        // a cell must wait after division before it can commit to divide again (enter S phase again)
        if (cell.WaitBeforeS > 0) then
            false
        else
            let mutable prob_o2, prob_glucose, prob_density = 0., 0., 0.
            let loc = cell.Location
            let o2, glucose, density = glb.O2.GetValue(loc), glb.Glucose.GetValue(loc), glb.CellPackingDensity.GetValue(loc)

            match cell.Type with
            | Stem ->
                prob_o2 <- (!ModelParameters.StemDivisionProbabilityO2).Y(o2);
                prob_glucose <- (!ModelParameters.StemDivisionProbabilityGlucose).Y(glucose)
                prob_density <- (!ModelParameters.StemDivisionProbabilityDensity).Y(density)

            | NonStem ->
                prob_o2 <- (!ModelParameters.NonStemDivisionProbabilityO2).Y(o2);
                prob_glucose <- (!ModelParameters.NonStemDivisionProbabilityGlucose).Y(glucose)
                prob_density <- (!ModelParameters.NonStemDivisionProbabilityDensity).Y(density)

            | _ -> raise(InnerError(sprintf "%s cell can not divide" (cell.Type.ToString())))

            // the probability of cell division is calculated
            // as the product of two functions (both range between 0 and 1)
                // one of each takes as its argument the concentration of oxygen
                // another one - the cell packing density
            (*let prob = prob_o2 * prob_density
            uniform_bool(prob)*)

            // ADDING GLUCOSE CONCENTRATION IN THE CALCULATION OF CELL DIVISION PROBABILITY
            // the probability of cell division is calculated
            // as the product of two functions (both range between 0 and 1)
                // one takes as its argument the max probability of division based on levels of oxygen and glucose
                // the second takes the cell packing density
            let prob = (max prob_o2 prob_glucose) * prob_density
            uniform_bool(prob)

    // returns true if the stem cell should divide symmetrically
    // and false if it should divide asymmetrically
    static member private should_divide_sym(cell: Cell) =
        uniform_bool(ModelParameters.StemSymmetricDivisionProbability)

    // returns true if a cell should undergo necrosis 
    // which depends on the amount of nutrients (currently: oxygen & glucose)   // 
    static member private should_goto_necrosis(cell: Cell, glb: GlobalState) = 
        if cell.Type <> NonStem then raise(InnerError(sprintf "%s cell can not die" (cell.Type.ToString())))

        let prob_oxygen = (!ModelParameters.NonStemNecrosisProbabilityO2).Y(glb.O2.GetValue(cell.Location))
        let prob_glucose = (!ModelParameters.NonStemNecrosisProbabilityGlucose).Y(glb.Glucose.GetValue(cell.Location))
        //let prob_density = (!ModelParameters.NonStemNecrosisProbDensity).Y(glb.CellPackingDensity.GetValue(cell.Location))
        // we calculate the probability of death based on two parameters independently
        // i.e. based on the concentration of oxygen and glucose
        // we then take the minimum of these probabilities, which means that
        // if at least one critical event occurs (i.e. if either O2 or glucose concentration is suffcient)
        // then the cell decides to survive
        let prob = min prob_oxygen prob_glucose
        let decision = uniform_bool(prob)
        decision

    // returns true if the cell should start apoptosis
    static member private should_start_apoptosis(cell: Cell) =
        if cell.Type <> NonStem then raise(InnerError(sprintf "%s cell can not die" (cell.Type.ToString())))

        let prob = (!ModelParameters.NonStemApoptosisProbabilityAge).Y(float cell.Age)
        uniform_bool(prob)
        
    // returns true if a stem cell should go to a "non-stem with memory" state
    static member private should_goto_nonstem(cell: Cell) =
        let prob = ModelParameters.StemToNonStemProbability
        uniform_bool(prob)

    // returns true if "a non-stem with memory" cell should return to the stem state
    static member private should_returnto_stem(cell: Cell, glb: GlobalState) =
        let prob = (!ModelParameters.NonStemToStemProbability).Y(float glb.NumofStemCells)
        uniform_bool(prob)

    // initialise_new_cell() must be called for each new cell
    static member initialise_new_cell(cell: Cell) =
        match cell.Type with
        | Stem -> 
            cell.WaitBeforeS <- uniform_int(ModelParameters.StemWaitBeforeCommitToDivideInterval)
            cell.StepsAfterLastDivision <- 0
        | NonStem -> 
            cell.WaitBeforeS <- uniform_int(ModelParameters.NonStemWaitBeforeCommitToDivideInterval)
            cell.StepsAfterLastDivision <- 0
        | _ -> raise(InnerError(sprintf "Error: new cell in state %s" (cell.Type.ToString())))


    // compute the action for a cell to take in the current step
    static member compute_action(glb: GlobalState) (cell: Cell) = 
        try
            // no action is taken by default
            cell.Action <- NoAction

            if (cell.State = NecrosisState) then
                if cell.WaitBeforeDesintegration > 0 then
                    cell.WaitBeforeDesintegration <- cell.WaitBeforeDesintegration - 1
                else cell.Action <- NecrosisDesintegration
            else if (cell.State = PreNecrosisState) then
                if cell.WaitBeforeNecrosis = 0 then
                    if Automata.should_goto_necrosis(cell, glb) then
                        cell.Action <- GotoNecrosis
                    else cell.State <- FunctioningState
            
            else if (cell.CellCycleStage = G1 || cell.CellCycleStage = G0) then
                if cell.WaitBeforeS > 0 then
                    cell.WaitBeforeS <- cell.WaitBeforeS - 1
                else 
                    match cell.Type with
                        // determine an action for a stem cell
                        | Stem ->
                            if glb.EGF && Automata.can_divide(cell, glb) then
                                cell.Action <- GotoS
                                if Automata.should_divide_sym(cell) then
                                    cell.DivideAction <- StemSymmetricDivision
                                else
                                    cell.DivideAction <- StemAsymmetricDivision
                            else if Automata.should_goto_nonstem(cell) then
                                cell.Type <- NonStemWithMemory
        
                        // determine an action for a non-stem cell
                        | NonStem ->
                            if glb.EGF && Automata.can_divide(cell, glb) then
                                cell.Action <- GotoS
                                cell.DivideAction <- NonStemDivision
                            else if Automata.should_start_apoptosis(cell) then
                                cell.Action <- StartApoptosis
                            else if Automata.should_goto_necrosis(cell, glb) then
                                cell.WaitBeforeNecrosis <- uniform_int(ModelParameters.NecrosisWaitInterval)
                                cell.State <- PreNecrosisState
                            else cell.Action <- GotoG0

                        // determine an action for a non-stem cell which can become stem again
                        | NonStemWithMemory ->
                            if glb.EGF && Automata.should_returnto_stem(cell, glb) then
                                cell.Type <- Stem

            else if (cell.CellCycleStage = S) then
                if cell.WaitBeforeG2M > 0 then
                    cell.WaitBeforeG2M <- cell.WaitBeforeG2M - 1
                else cell.Action <- GotoG2M

            else if (cell.CellCycleStage = G2_M) then
                if cell.WaitBeforeDivide > 0 then
                    cell.WaitBeforeDivide <- cell.WaitBeforeDivide - 1
                else cell.Action <- GotoCytokinesis

        with
            | :? InnerError as error ->
                raise (InnerError(sprintf "%s\nCell summary:\n%s" (error.ToString()) (cell.ToString())))


    // do_step() must be called in every step of the simulation
    // updates the time-related fields of the cell (waiting intervals, age etc)
    static member do_step(cell: Cell) =
        cell.StepsAfterLastDivision <- cell.StepsAfterLastDivision + 1
        cell.Age <- cell.Age + 1
        if cell.State = PreNecrosisState && cell.WaitBeforeNecrosis > 0 then
            cell.WaitBeforeNecrosis <- (cell.WaitBeforeNecrosis - 1)
        

    // calculate the cell packing density in the whole model
    static member calc_cellpackdensity(glb: GlobalState, cells: ResizeArray<Cell>) =
        let grid = glb.CellPackingDensity.Grid

        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                let p = grid.IndicesToPoint(i, j)
                let rect = grid.CenteredRect(i, j)
                // count the cells in the rectangle
                // centered in the grid vertex (i, j) and with size of a grid mesh 
                let cells_in_rect = GlobalState.CountCells(cells, GlobalState.IsCellInMesh(rect))
                // the number of the cells is weighted by the coefficient k
                // which equals to the ratio of the size of a grid mesh to the cell size
                let k = grid.Dx / (2.*ModelParameters.AverageCellRadius)
                let density = (float cells_in_rect) * k
                glb.CellPackingDensity.SetValue(p, density)

        glb.CellPackingDensity.ComputeInterpolant()

        
    // calculate the concentration of oxygen in the whole model
    static member private calc_o2(glb: GlobalState, cells: ResizeArray<Cell>, dt: int) =
        let (c1, c2, c3) = ModelParameters.O2Param
        let o2_limits = ModelParameters.O2Limits
        let grid = glb.O2.Grid
        
        // get these values all at once because they are protected by a semaphore
        // and it's too costly to get them one by one
        // the semaphore is needed because the function values are updated in this thread
        // and read in another one (which renders the ExternalStateForm)
        let o2_vals = glb.O2.GetAllValues()
        let o2_nabla_squared_vals = glb.O2NablaSquared.GetAllValues()

        // here we bypass the semaphore for glb.O2NablaSquare.F but this function's values
        // are read and written in this thread only
        Derivative.Derivative_2_1.NablaSquared(glb.O2NablaSquared.F, glb.O2.F, glb.O2.Delta)

        Automata.compute_peripheral(glb, cells)
        let find_live_cells = fun (c: Cell) -> Array.Exists(live_states, fun (s: CellState) -> s = c.State)
        let find_dividing_cells = fun (c: Cell) -> Array.Exists(divide_action, fun (a: CellDivideAction) -> a = c.DivideAction)

        for i=0 to grid.YLines-1 do
            for j=0 to grid.XLines-1 do
                let p = grid.IndicesToPoint(i, j)
                let rect = grid.CenteredRect(i, j)
                // get all the cells in the rectangle
                // centered in the grid vertex (i, j) and with size of a grid mesh 
                let cells_in_rect = cells.FindAll(Predicate<Cell>(GlobalState.IsCellInMesh(rect)))

                // compute the number of dividing and non-dividing cells 
                let numof_live_cells = GlobalState.CountCells(cells_in_rect, find_live_cells)
                let numof_dividing_cells = GlobalState.CountCells(cells_in_rect, find_dividing_cells)
                let numof_nondividing_cells = numof_live_cells - numof_dividing_cells

                // if the point is inside the tumor, there is no oxygen supply to it
                // (other than diffusion from the neighbouring points)
                let supply_rate = if glb.Peripheral.IsPointStrictlyInside(i, j) then 0. else c1
                let consumption_rate = c2*(float numof_dividing_cells) + c3*(float numof_nondividing_cells)

                let mutable new_value = o2_vals.[i, j] +
                                        (float dt)*(ModelParameters.OxygenDiffusionCoeff * o2_nabla_squared_vals.[i, j] +
                                                    supply_rate - consumption_rate)

                if new_value < o2_limits.Min then new_value <- o2_limits.Min
                else if new_value > o2_limits.Max then new_value <- o2_limits.Max    
                o2_vals.[i, j] <- new_value

        glb.O2.SetValues(o2_vals)
        glb.O2.ComputeInterpolant()

    // calculate the concentration of glucose in the whole model
    static member private calc_glucose(glb: GlobalState, cells: ResizeArray<Cell>, dt: int) =
        let (c1, c2, c3) = ModelParameters.GlucoseParam
        let glucose_limits = ModelParameters.GlucoseLimits
        let grid = glb.Glucose.Grid

        let glucose_vals = glb.Glucose.GetAllValues()
        let glucose_nabla_squared_vals = glb.GlucoseNablaSquared.GetAllValues()

        Derivative.Derivative_2_1.NablaSquared(glb.GlucoseNablaSquared.F, glb.Glucose.F, glb.Glucose.Delta)

        Automata.compute_peripheral(glb, cells)
        let find_live_cells = fun (c: Cell) -> Array.Exists(live_states, fun (s: CellState) -> s = c.State)
        let find_dividing_cells = fun (c:Cell) -> Array.Exists(divide_action, fun (a: CellDivideAction) -> a = c.DivideAction)

        for i=0 to grid.YLines-1 do
            for j=0 to grid.XLines-1 do
                let p = grid.IndicesToPoint(i, j)
                let rect = grid.CenteredRect(i, j)
                // get all the cells in the rectangle
                // centered in the grid vertex (i, j) and with size of a grid mesh
                let cells_in_rect = cells.FindAll(Predicate<Cell>(GlobalState.IsCellInMesh(rect)))

                // compute the number of dividing and non-dividing cells
                let numof_live_cells = GlobalState.CountCells(cells_in_rect, find_live_cells)
                let numof_dividing_cells = GlobalState.CountCells(cells_in_rect, find_dividing_cells)
                let numof_nondividing_cells = numof_live_cells - numof_dividing_cells

                // if the point is inside the tumor, there is no glucose supply to it
                // (other than diffusion from the neighboring points)
                let supply_rate = if glb.Peripheral.IsPointStrictlyInside(i, j) then 0. else c1
                let consumption_rate = c2*(float numof_dividing_cells) + c3*(float numof_nondividing_cells)

                let mutable new_value = glucose_vals.[i, j] + 
                                        (float dt)*(ModelParameters.GlucoseDiffusionCoeff * glucose_nabla_squared_vals.[i, j] +
                                                    supply_rate - consumption_rate)

                if new_value < glucose_limits.Min then new_value <- glucose_limits.Min
                else if new_value > glucose_limits.Max then new_value <- glucose_limits.Max
                glucose_vals.[i, j] <- new_value

        glb.Glucose.SetValues(glucose_vals)
        glb.Glucose.ComputeInterpolant()

    // recalculate the global state of the model
    static member recalc_global_state(glb:GlobalState, cells: ResizeArray<Cell>, dt: int) =
        glb.EGF <- uniform_bool(ModelParameters.EGFProb)
        Automata.calc_cellpackdensity(glb, cells)
        Automata.calc_o2(glb, cells, dt)
        Automata.calc_glucose(glb, cells, dt)