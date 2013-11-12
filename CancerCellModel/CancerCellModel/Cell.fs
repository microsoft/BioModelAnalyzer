module Cell
open System
open ModelParameters
open MyMath
open Geometry
open System.Threading

type PathwayLevel = Up | Down | Neutral
type CellType = Stem | NonStem | NonStemWithMemory

type CellState = Functioning | PreparingToNecrosis | ApoptoticDeath | NecroticDeath
let any_live_state = [|Functioning; PreparingToNecrosis|]

type CellAction = AsymSelfRenewal | SymSelfRenewal | NonStemDivision | Necrosis | Apoptosis | NoAction
let any_nondeath_action = [|AsymSelfRenewal; SymSelfRenewal; NonStemDivision; NoAction|]
let sym_divide_action = [|SymSelfRenewal; NonStemDivision|]
let asym_divide_action = [|AsymSelfRenewal|]
let divide_action = [|AsymSelfRenewal; SymSelfRenewal; NonStemDivision|]

type PhysSphere(location: Point, radius: float, density: float) = 
    let mutable location = location
    let mutable repulsive_force = Vector()
    let mutable friction_force = Vector()
    let mutable net_force = Vector()
    let mutable velocity = Vector()
    let mutable radius = radius
    let mutable density = density
    //[<DefaultValue>] val mutable private interaction_energy: float

    member this.Location with get() = location and set(x) = location <- x
    member this.RepulsiveForce with get() = repulsive_force and set(x) = repulsive_force <- x
    member this.FrictionForce with get() = friction_force and set(x) = friction_force <- x
    member this.NetForce with get() = repulsive_force + friction_force //net_force and set(x) = net_force <- x
    member this.Velocity with get() = velocity and set(x) = velocity <- x
    member this.Density with get() = density and set(x) = density <- x
    member this.R with get() = radius and set(x) = radius <- x
    member this.Mass with get() = (density * 4./3.*pi*(pow3 radius) / 1000.)
    //member this.V with get() = this.interaction_energy and set(x) = this.interaction_energy <- x

type Cell (cell_type: CellType, generation: int, location: Point, radius: float, density: float) =
    inherit PhysSphere(location, radius, density)

    static let mutable counter = 0
    let mutable unique_number = 0
    let mutable ng2: PathwayLevel = Up
    let mutable cd133: PathwayLevel = Up
//    let mutable egfr: PathwayLevel = Up
    let mutable cell_type: CellType = cell_type
    let mutable state: CellState = Functioning
    let mutable next_state: CellState = Functioning
    let mutable action: CellAction = NoAction
    let mutable time_in_state = 0
    let mutable generation = generation
    let mutable wait_before_divide = 0
    let mutable wait_before_necrosis = 0
    let mutable steps_after_last_division = 0
    let mutable age = 0

    let init() =
        match cell_type with
        | Stem -> ng2 <- Up; cd133 <- Up
            (*let rand = random_bool()
            egfr <- if rand = 0 then Up else if rand = 1 then Down else Neutral*)
        | NonStem -> ng2 <- Down; cd133 <- Down
        | NonStemWithMemory -> ng2 <- Down; cd133 <- Up

        unique_number <- counter
        counter <- counter + 1

    do
        init()

    member this.NG2 with get() = ng2
    member this.CD133 with get() = cd133
//  member this.EGFR with get() = egfr
    member this.State with get() = state and set(s) = state <- s; time_in_state <- 0
    member this.Type with get() = cell_type and set(t) = cell_type <- t; time_in_state <- 0

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
        | PreparingToNecrosis -> "PreparingToNecrosis"
        | ApoptoticDeath -> "Apoptotic Death"
        | NecroticDeathStem -> "Necrotic Death"

    member this.NextState with get() = next_state
                            and set(s) = next_state <- s;


    member this.Action with get() = action and set(a) = action <- a
    member this.TimeInState with get() = time_in_state
    
    member this.Generation with get() = generation and set(g) = generation <- g
    member this.WaitBeforeDivide with get() = wait_before_divide and set(x) = wait_before_divide <- x
    member this.WaitBeforeNecrosis with get() = wait_before_necrosis and set(x) = wait_before_necrosis <- x
    member this.StepsAfterLastDivision with get() = steps_after_last_division and set(x) = steps_after_last_division <- x
    member this.Age with get() = age and set(x) = age <- x

    member this.Summary() =
        sprintf "State: %s, Age: %d, Generation: %d\n\
                Location: r=(%.1f, %.1f)\n\
                Speed: v=(%.1f, %.1f)\n\
                Repulsive force=(%.1f, %.1f)\n\
                Friction force=(%.1f, %.1f)"
                (Cell.StateAsStr(state)) age generation
                location.x location.y
                base.Velocity.x base.Velocity.y
                base.RepulsiveForce.x base.RepulsiveForce.y
                base.FrictionForce.x base.FrictionForce.y


type ExternalState() as this =
    let mutable egf = true    
    let mutable live_cells: int = 0
    let mutable dividing_cells: int = 0
    let mutable stem_cells: int = 0
    let mutable cell_concentration_area = Geometry.Rectangle()

    [<DefaultValue>] val mutable private o2: GridFunction1D
    [<DefaultValue>] val mutable private o2_nabla_square: GridFunction1D
    [<DefaultValue>] val mutable private cell_pack_density: GridFunction1D
    [<DefaultValue>] val mutable private cell_pack_density_grad: ref<GridFunctionVector>

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
        let f = ref (GridFunctionVector(grid = Grid(width = 2.*ModelParameters.AverageCellR,
                                           height = 2.*ModelParameters.AverageCellR,
                                           dx = 2.*ModelParameters.AverageCellR,
                                           dy = 2.*ModelParameters.AverageCellR)))
        f

    do
        this.o2 <- init_o2()
        this.o2_nabla_square <- init_o2_nabla_square()
        this.cell_pack_density <- init_cell_pack_density()
        this.cell_pack_density_grad <- init_cell_pack_density_grad()

    member this.EGF with get() = egf and set(x) = egf <- x
    member this.O2 with get() = this.o2
    member this.O2NablaSquare with get() = this.o2_nabla_square
    member this.LiveCells with get() = live_cells and set(x) = live_cells <- x
    member this.DividingCells with get() = dividing_cells and set(x) = dividing_cells <- x
    member this.NonDividingLiveCells with get() = live_cells - dividing_cells

    member this.StemCells with get() = stem_cells and set(x) = stem_cells <- x
    member this.CellPackDensity with get() = this.cell_pack_density

    member this.CellPackDensityGrad with get() = this.cell_pack_density_grad
                                     and set(x) = this.cell_pack_density_grad <- x

    member this.CellConcentrationArea with get() = cell_concentration_area
                                        and set(x) = cell_concentration_area <- x

    static member GetNeighbours(cell: Cell, cells: Cell[], r: float) =
        Array.filter(fun (c: Cell) -> c <> cell && Geometry.distance(c.Location, cell.Location) < r) cells

    static member GetCellsInMesh(all_cells: ResizeArray<Cell>, rect: Rectangle) =
        all_cells.FindAll(fun (c: Cell) -> let circle = Circle(c.Location, c.R)
                                           (not (CirclePart.IntersectionTriviallyEmpty(circle, rect) ||
                                                 CirclePart(circle, rect).IsEmpty()))) 

    member this.O2ToStringVerbose(p: Point) =
        // values in the adjoining grid points from which the value is interpolated - displayed for debugging
        let o2s = ref (Array.create 9 0.)
        let o2_nabla2 = ref (Array.create 9 0.)
        let (i, j) = this.o2_nabla_square.Grid.PointToIndices(p)

        sprintf "Oxygen: o2(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f)\n\
                    \tnabla_square=%.3f\n\
                    \t(interpolated from cc=%.3f lb=%.3f lc=%.3f lt=%.3f ct=%.3f rt=%.3f rc=%.3f rb=%.3f cb=%.3f)"
                (this.o2.GetValue(p, o2s))
                ((!o2s).[0]) ((!o2s).[1]) ((!o2s).[2]) ((!o2s).[3]) ((!o2s).[4]) ((!o2s).[5]) ((!o2s).[6]) ((!o2s).[7]) ((!o2s).[8])
                (this.o2_nabla_square.GetValue(i, j, o2_nabla2))
                ((!o2_nabla2).[0]) ((!o2_nabla2).[1]) ((!o2_nabla2).[2]) ((!o2_nabla2).[3])
                ((!o2_nabla2).[4]) ((!o2_nabla2).[5]) ((!o2_nabla2).[6]) ((!o2_nabla2).[7]) ((!o2_nabla2).[8])


    member this.DensityToStringVerbose(p: Point) =
        let densities = ref (Array.create 9 0.)

        let mutable grad_msg = ""

        // gradient is calculated only for the area where cells are concentrated
        if cell_concentration_area.IsPointInside(p) then
            let grad = (!this.CellPackDensityGrad).GetValue(p)
            grad_msg <- sprintf ", gradient=(%.2f, %.2f)" grad.x grad.y

        sprintf "Cell pack density: density(r)=%.1f \n\
                    \t(interpolated from cc=%.1f lb=%.1f lc=%.1f lt=%.1f ct=%.1f rt=%.1f rc=%.1f rb=%.1f cb=%.1f\n\
                    %s)"
                    (this.cell_pack_density.GetValue(p, densities))
                    ((!densities).[0]) ((!densities).[1]) ((!densities).[2]) ((!densities).[3])
                    ((!densities).[4]) ((!densities).[5]) ((!densities).[6]) ((!densities).[7]) ((!densities).[8])
                    grad_msg