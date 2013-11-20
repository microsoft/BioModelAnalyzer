module Cell

open System
open ModelParameters
open Geometry
open NumericComputations

type CellType = Stem | NonStem | NonStemWithMemory

type CellState = Functioning | PreNecrosisState | ApoptosisState | NecrosisState
let any_live_state = [|Functioning; PreNecrosisState|]

type CellAction = AsymSelfRenewal | SymSelfRenewal | NonStemDivision |
                    GotoNecrosis | StartApoptosis | NoAction | NecrosisDesintegration

let any_nondeath_action = [|AsymSelfRenewal; SymSelfRenewal; NonStemDivision; NoAction|]
let sym_divide_action = [|SymSelfRenewal; NonStemDivision|]
let asym_divide_action = [|AsymSelfRenewal|]
let divide_action = [|AsymSelfRenewal; SymSelfRenewal; NonStemDivision|]

type SingleMassPoint(location: Point) =
    let mutable location = location
    let mutable repulsive_force = Vector()
    let mutable friction_force = Vector()
    let mutable velocity = Vector()

    member this.Location with get() = location and set(x) = location <- x
    member this.RepulsiveForce with get() = repulsive_force and set(x) = repulsive_force <- x
    member this.FrictionForce with get() = friction_force and set(x) = friction_force <- x
    member this.NetForce with get() = repulsive_force + friction_force
    member this.Velocity with get() = velocity and set(x) = velocity <- x

    member this.Summary() =
        sprintf "Location: r=(%.1f, %.1f)\n\
                Speed: v=(%.1f, %.1f)\n\
                Repulsive force=(%.1f, %.1f)\n\
                Friction force=(%.1f, %.1f)"
                location.x location.y
                velocity.x velocity.y
                repulsive_force.x repulsive_force.y
                friction_force.x friction_force.y

type PhysSphere(location: Point, radius: float, density: float) = 
     inherit SingleMassPoint(location)

     let mutable radius = radius
     let mutable density = density
     let mutable circle = Circle(location, radius) // this value is stored for performance reasons
    
     member this.Density with get() = density and set(x) = density <- x
     member this.R with get() = radius and set(x) = circle <- Circle(this.Location, x); radius <- x
     member this.Mass with get() = (density * 4./3.*Math.PI*(radius*radius*radius) / 1000.)
     member this.Location with get() = base.Location and set(x) = circle <- Circle(x, radius); base.Location <- x
     member this.Circle with get() = circle


type Cell (cell_type: CellType, generation: int, location: Point, radius: float, density: float) =
    inherit PhysSphere(location, radius, density)

    static let mutable counter = 0 // used for debugging purposes
    let mutable unique_number = 0  // used for debugging purposes
    let mutable cell_type: CellType = cell_type
    let mutable state: CellState = Functioning
    let mutable action: CellAction = NoAction   // the action to take in the current time step
    let mutable age = 0                         // the age of a cell in time steps
    let mutable generation = generation         // the number of ancestors of a cell
    let mutable wait_before_divide = 0          // after this amount of steps a cell can try again to proliferate
    let mutable wait_before_necrosis = 0        // after this amount of steps a cell in the Pre-necrosis state
                                                    // will transit either to the Necrosis state or back to the Functioning state
    let mutable wait_before_desintegration = 0  // after this amount of steps a cell in necrosis state will desintegrate
    let mutable steps_after_last_division = 0   // used for statistics

    let init() =
        unique_number <- counter
        counter <- counter + 1

    do
        init()

    member this.State with get() = state and set(s) = state <- s
    member this.Type with get() = cell_type and set(t) = cell_type <- t

    // SI: override toString for t
    static member TypeAsStr(t: CellType) =
        match t with
        | Stem -> "Stem"
        | NonStem -> "Non-stem"
        | NonStemWithMemory -> "Non-stem with memory"

    member this.TypeAsStr() =
        Cell.TypeAsStr(cell_type)

    static member StateAsStr(s: CellState) =
        match s with
        | Functioning -> "Functioning"
        | PreNecrosisState -> "Pre-necrotic state"
        | ApoptosisState -> "Apoptotic Death"
        | NecrosisState -> "Necrotic Death"

    member this.Action with get() = action and set(a) = action <- a
    
    member this.Generation with get() = generation and set(g) = generation <- g
    member this.WaitBeforeDivide with get() = wait_before_divide and set(x) = wait_before_divide <- x
    
    member this.WaitBeforeNecrosis with get() = wait_before_necrosis
                                    and set(x) = wait_before_necrosis <- x

    member this.WaitBeforeDesintegration with get() = wait_before_desintegration
                                            and set(x) = wait_before_desintegration <- x

    member this.StepsAfterLastDivision with get() = steps_after_last_division and set(x) = steps_after_last_division <- x
    member this.Age with get() = age and set(x) = age <- x

    member this.Summary() =
        sprintf "Type: %s, State: %s, Age: %d, Generation: %d\n\%s"
                (this.TypeAsStr()) (Cell.StateAsStr(state)) age generation
                (base.Summary())

// SI: move to Model, as it seems it's one ExternalState per Model
type ExternalState() as this =
    let mutable egf = true              // indicates whether EGF is Up (true) or Down (false)
    let mutable live_cells: int = 0     // the numbr of live cells in the model
    let mutable dividing_cells: int = 0 // the number of dividing cells in the model
    let mutable stem_cells: int = 0     // the number of stem cells in the model
    let mutable cell_concentration_area = Geometry.Rectangle()  // the smallest rectangle embraсing
                                                                // the locations of all the cells in the model

    // SI: use option type, rather than DefaultValue                                                                    
    [<DefaultValue>] val mutable private o2: GridFunction1D                 // a function which stores the concentration 
                                                                            // of o2 in the vertices of the grid

    [<DefaultValue>] val mutable private o2_nabla_square: GridFunction1D    // a grid function which stores nabla square
                                                                            // (the sum of second derivatives for each coordinate)
                                                                            // of the concentration of o2

    [<DefaultValue>] val mutable private cell_packing_density: GridFunction1D  // a grid function which stores the
                                                                               // cell packing density

    
    [<DefaultValue>] val mutable private cell_packing_density_gradient: ref<GridFunctionVector> // a grid function which stores
                                                                                                // the gradient of the cell packing density
                                                                                                // not currently used (see Automata.daughter_locations)

    [<DefaultValue>] val mutable private peripheral: Grid2DRegion  // a convex polygon embracing
                                                                   // the locations of all cells in the model

    let init_o2() =
        let grid = ModelParameters.O2Grid
        let f = GridFunction1D(grid)

        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                f.SetValue(grid.IndicesToPoint(i, j), 100.)
        
        f.ComputeInterpolant()
        f

    let init_o2_nabla_square() =
        let grid = ModelParameters.O2Grid
        let f = GridFunction1D(grid)

        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                f.SetValue(grid.IndicesToPoint(i, j), 0.)
        
        f.ComputeInterpolant()
        f

    let init_cell_pack_density() =
        let grid = ModelParameters.CellPackDensityGrid
        let f = GridFunction1D(grid)

        for i = 0 to grid.YLines-1 do
            for j = 0 to grid.XLines-1 do
                f.SetValue(grid.IndicesToPoint(i, j), 0.)
        
        f.ComputeInterpolant()
        f

    let init_cell_pack_density_grad() =
        let density_grid = ModelParameters.CellPackDensityGrid
        let f = ref (GridFunctionVector(grid = Grid(width = 2.*ModelParameters.AverageCellR,
                                           height = 2.*ModelParameters.AverageCellR,
                                           dx = 2.*ModelParameters.AverageCellR,
                                           dy = 2.*ModelParameters.AverageCellR),
                                        origin = Point(),
                                        max_size = Geometry.Size(density_grid.Width, density_grid.Height)))
        f

    do
        this.o2 <- init_o2()
        this.o2_nabla_square <- init_o2_nabla_square()
        this.cell_packing_density <- init_cell_pack_density()
        this.cell_packing_density_gradient <- init_cell_pack_density_grad()
        this.peripheral <- new Grid2DRegion()

    member this.EGF with get() = egf and set(x) = egf <- x
    member this.O2 with get() = this.o2
    member this.O2NablaSquare with get() = this.o2_nabla_square
    member this.Peripheral with get() = this.peripheral and set(x) = this.peripheral <- x
    member this.LiveCells with get() = live_cells and set(x) = live_cells <- x
    member this.DividingCells with get() = dividing_cells and set(x) = dividing_cells <- x
    member this.NonDividingLiveCells with get() = live_cells - dividing_cells

    member this.StemCells with get() = stem_cells and set(x) = stem_cells <- x
    member this.CellPackingDensity with get() = this.cell_packing_density

    member this.CellPackingDensityGradient with get() = this.cell_packing_density_gradient
                                            and set(x) = this.cell_packing_density_gradient <- x

    member this.CellConcentrationArea with get() = cell_concentration_area
                                        and set(x) = cell_concentration_area <- x

    (*static member GetNeighbours(cell: Cell, cells: Cell[], r: float) =
        Array.filter(fun (c: Cell) -> c <> cell && Geometry.distance(c.Location, cell.Location) < r) cells*)

    // determine if some part of the cell is inside the rectangle
    static member IsCellInMesh(rect: Rectangle)(c: Cell) =
        CirclePart.IntersectionNotTriviallyEmpty(c.Circle, rect) &&
                CirclePart(c.Circle, rect).NonEmpty()

    // count the number of the cells for which the predicate func yields true
    static member CountCells(cells: ResizeArray<Cell>, func: Cell->bool) =
        let mutable count = 0
        for c in cells do
            if func(c) then
                count <- count + 1

        count        

    member this.O2Summary(p: Point) =
        // values in the adjoining grid points from which the value is interpolated - displayed for debugging
        let o2s = Array.create 9 0.
        let o2_nabla2 = Array.create 9 0.
        let (i, j) = this.o2_nabla_square.Grid.PointToIndices(p)

        sprintf "Oxygen: o2(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f)\n\
                    \tnabla_square=%.3f\n\
                    \t(interpolated from cc=%.3f lb=%.3f lc=%.3f lt=%.3f ct=%.3f rt=%.3f rc=%.3f rb=%.3f cb=%.3f)"
                (this.o2.GetValue(p, o2s))
                (o2s.[0]) (o2s.[1]) (o2s.[2]) (o2s.[3]) (o2s.[4]) (o2s.[5]) (o2s.[6]) (o2s.[7]) (o2s.[8])
                (this.o2_nabla_square.GetValue(i, j, o2_nabla2))
                (o2_nabla2.[0]) (o2_nabla2.[1]) (o2_nabla2.[2]) (o2_nabla2.[3])
                (o2_nabla2.[4]) (o2_nabla2.[5]) (o2_nabla2.[6]) (o2_nabla2.[7]) (o2_nabla2.[8])


    member this.CellPackingDensitySummary(p: Point) =
        let densities = Array.create 9 0.

        let mutable grad_msg = ""

        // gradient is calculated only for the area where cells are concentrated
        if cell_concentration_area.IsPointInside(p) then
            let grad = (!this.CellPackingDensityGradient).GetValue(p)
            grad_msg <- sprintf ", gradient=(%.2f, %.2f)" grad.x grad.y

        sprintf "Cell pack density: density(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f\n\
                    %s)"
                    (this.cell_packing_density.GetValue(p, densities))
                    (densities.[0]) (densities.[1]) (densities.[2]) (densities.[3])
                    (densities.[4]) (densities.[5]) (densities.[6]) (densities.[7]) (densities.[8])
                    grad_msg