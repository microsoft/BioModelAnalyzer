module Cell
open System
open ModelParameters
open MyMath
open Geometry
open System.Threading

type PathwayLevel = Up | Down | Neutral
type CellType = Stem | NonStem | NonStemWithMemory

type CellState = Functioning | PreparingToDie | Dead
let any_state = [|Functioning; PreparingToDie|]

type CellAction = AsymSelfRenewal | SymSelfRenewal | NonStemDivision | Death | NoAction
let any_action = [|AsymSelfRenewal; SymSelfRenewal; NonStemDivision; NoAction|]
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
    let mutable wait_before_die = 0
    let mutable steps_after_last_division = 0

    let init() =
        match cell_type with
        | Stem -> ng2 <- Up; cd133 <- Up
            (*let rand = random_bool()
            egfr <- if rand = 0 then Up else if rand = 1 then Down else Neutral*)
        | NonStem -> ng2 <- Down; cd133 <- Down
        | NonStemWithMemory -> ng2 <- Down; cd133 <- Up

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

    member this.NextState with get() = next_state
                            and set(s) = next_state <- s;


    member this.Action with get() = action and set(a) = action <- a
    member this.TimeInState with get() = time_in_state
    
    member this.Generation with get() = generation and set(g) = generation <- g
    member this.WaitBeforeDivide with get() = wait_before_divide and set(x) = wait_before_divide <- x
    member this.WaitBeforeDie with get() = wait_before_die and set(x) = wait_before_die <- x
    member this.StepsAfterLastDivision with get() = steps_after_last_division and set(x) = steps_after_last_division <- x

type GridFunction(grid: Grid, f_limits: FloatInterval) =
    let x = Array.create grid.XLines 0.
    let y = Array.create grid.YLines 0.
    let f = Array2D.create grid.XLines grid.YLines 0.
    let interpolant = ref (new alglib.spline2dinterpolant())
    let mutex = new Mutex()

    do
        for i = 0 to grid.XLines - 1 do
            x.[i] <- grid.Point(i, 0).x

        for j = 0 to grid.YLines - 1 do
            y.[j] <- grid.Point(0, j).y

    member this.GetValue(p: Point) =
        if Math.Abs(p.x) > grid.Width/2. ||
            Math.Abs(p.y) > grid.Height/2. then
                raise(InnerError(sprintf "point (%.1f,%.1f) outside of managed grid area" p.x p.y))
        else
            mutex.WaitOne() |> ignore
            let fval = alglib.spline2dcalc(!interpolant, p.x, p.y)
            mutex.ReleaseMutex()
            fval

    member this.SetValue(p: Point, value: float) = 
        if Math.Abs(p.x) > grid.Width/2. ||
            Math.Abs(p.y) > grid.Height/2. then
                raise(InnerError(sprintf "point (%.1f,%.1f) outside of managed grid area" p.x p.y))
        else
            let i = int (Math.Ceiling((p.x + grid.Width/2.) / grid.Dx))
            let j = int (Math.Ceiling((p.y + grid.Height/2.) / grid.Dy))
            f.[i,j] <- value

    member this.ComputeInterpolant() =
        mutex.WaitOne() |> ignore
        alglib.spline2dbuildbilinear(x, y, f, grid.XLines, grid.YLines, interpolant)
        mutex.ReleaseMutex()

    member this.Grid with get() = grid
    member this.F with get() = f
    member this.FLimits with get() = f_limits

type ExternalState() =
    let mutable egf = true    
    let mutable live_cells: int = 0
    let mutable dividing_cells: int = 0
    let mutable stem_cells: int = 0

    let init_o2() =
        let grid = ModelParameters.GridParam
        let f = GridFunction(grid, ExternalState.O2Limits)

        for i = 0 to grid.XLines-1 do
            for j = 0 to grid.YLines-1 do
                f.SetValue(grid.Point(i, j), 100.)
        
        f.ComputeInterpolant()
        f

    let init_cell_pack_density() =
        let grid = ModelParameters.GridParam
        let f = GridFunction(grid, ExternalState.CellPackDensityLimits)

        for i = 0 to grid.XLines-1 do
            for j = 0 to grid.YLines-1 do
                f.SetValue(grid.Point(i, j), 0.)
        
        f.ComputeInterpolant()
        f

    let mutable o2 = init_o2()
    let mutable cell_pack_density = init_cell_pack_density()

    static member O2Limits with get() = FloatInterval(0., 100.)
    static member CellPackDensityLimits with get() = FloatInterval(0., 1.)
    member this.EGF with get() = egf and set(x) = egf <- x
    member this.O2 with get() = o2 //and set(x: GridFunction) = o2 <- x
    member this.LiveCells with get() = live_cells and set(x) = live_cells <- x
    member this.DividingCells with get() = dividing_cells and set(x) = dividing_cells <- x
    member this.NonDividingLiveCells with get() = live_cells - dividing_cells

    member this.StemCells with get() = stem_cells and set(x) = stem_cells <- x
    member this.CellPackDensity with get() = cell_pack_density
                                    //and set(x) = cell_pack_density <- x
    
    static member GetNeighbours(cell: Cell, cells: Cell[], r: float) =
        Array.filter(fun (c: Cell) -> c <> cell && Geometry.distance(c.Location, cell.Location) < r) cells

    static member GetCellsInMesh(cells: Cell[], rect: Rectangle, ?state: CellState[], ?action: CellAction[]) =
        let state = defaultArg state any_state
        let action = defaultArg action any_action

        Array.filter(fun (c: Cell) -> (not (CirclePart(Circle(c.Location, c.R), rect).IsEmpty())) &&
                                        //Geometry.distance(c.Location, point) < r &&
                                        Array.exists (fun (s: CellState) -> s = c.State) state &&
                                        Array.exists (fun (a: CellAction) -> a = c.Action) action) cells