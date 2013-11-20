module Model

open Cell
open MolecularDynamics
open Automata
open System
open Geometry

type StatType = Live | Dividing | Dying | Dead | Stem | NonStem | NonStemWithMemory

type CellActivityStatistics() =
    let mutable time_between_divisions_sum = 0
    let mutable num_of_summands = 0
    let mutable maxtime_between_divisions = 0
    let mutable mintime_between_divisions = 0

    member this.AverageTimeBetweenDivisions with get() = float time_between_divisions_sum / float num_of_summands
    member this.MaxTimeBetweenDivisions with get() = maxtime_between_divisions
    member this.MinTimeBetweenDivisions with get() = mintime_between_divisions

    member this.AddData(interval: int) =
        time_between_divisions_sum <- time_between_divisions_sum + interval;
        num_of_summands <- num_of_summands + 1;
        if interval < mintime_between_divisions || mintime_between_divisions = 0 then mintime_between_divisions <- interval
        if interval > maxtime_between_divisions then maxtime_between_divisions <- interval

type Model() =

    let mutable t: int = 0
    let mutable dt: int = 1
    //let mutable max_stat_len: int = 100

    // SI: Explain invariant about disjointness of _cells. 
    //   live n dead = empty
    //   allcells = live U dead
    //   apop n necrotic = empty
    //   apop U necrotic = dead
    // all live_cells have state "live" 
    // ...
    let all_cells = new ResizeArray<Cell>()
    let live_cells = new ResizeArray<Cell>()
    let dead_cells = new ResizeArray<Cell>()
    let apoptotic_cells = new ResizeArray<Cell>()
    let necrotic_cells = new ResizeArray<Cell>()
    
    // we need below numbers for statistics
    let mutable live_cells_num_prev = 0
    let mutable dividing_cells_num = 0
    let mutable stem_cells_num = 0
    let mutable stem_cells_num_prev = 0
    let mutable nonstem_cells_num = 0
    let mutable nonstem_cells_num_prev = 0
    let mutable dying_cells_num = 0

    let ext_state = ref (new ExternalState())

    let mutable stem_cell_activity_stat = new CellActivityStatistics()
    let mutable nonstem_cell_activity_stat = new CellActivityStatistics()

    let init() =
        let first_cell = new Cell(cell_type = CellType.Stem, generation = 0, location = Point(), radius = 10., density = 1.)
        all_cells.Add(first_cell)
        live_cells.Add(first_cell)
        stem_cells_num <- stem_cells_num + 1
        CellActivity.calc_cellpackdensity(!ext_state, all_cells)
    
    let recalc_extstate() = 
        (!ext_state).LiveCells <- live_cells.Count
        (!ext_state).DividingCells <- dividing_cells_num
        (!ext_state).StemCells <- stem_cells_num
        CellActivity.recalculate_ext_state(!ext_state, all_cells, dt)

    let perform_division() =
        let stem_sym_div_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = SymSelfRenewal)
        let stem_asym_div_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = AsymSelfRenewal)
        let stem_div_cells = new ResizeArray<Cell>(stem_sym_div_cells.Count+stem_asym_div_cells.Count)
        stem_div_cells.AddRange(stem_sym_div_cells)
        stem_div_cells.AddRange(stem_asym_div_cells)

        stem_div_cells.ForEach(fun(c: Cell) -> stem_cell_activity_stat.AddData(c.StepsAfterLastDivision))
        stem_cells_num <- stem_cells_num + stem_sym_div_cells.Count

        let nonstem_div_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = NonStemDivision)
        nonstem_div_cells.ForEach(fun(c: Cell) -> stem_cell_activity_stat.AddData(c.StepsAfterLastDivision))
        nonstem_cells_num <- nonstem_cells_num + stem_asym_div_cells.Count + nonstem_div_cells.Count

        dividing_cells_num <- stem_div_cells.Count + nonstem_div_cells.Count

        let new_cells = new ResizeArray<Cell>(2*dividing_cells_num)
        let dividing_cells = new ResizeArray<Cell>(dividing_cells_num)
        dividing_cells.AddRange(stem_div_cells)
        dividing_cells.AddRange(nonstem_div_cells)

        for c in dividing_cells do
            new_cells.AddRange(CellActivity.divide(!ext_state)(c))
            all_cells.Remove(c) |> ignore
            live_cells.Remove(c) |> ignore

        new_cells

    let perform_death() =
        let start_apoptosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = StartApoptosis)
        for c in start_apoptosis_cells do
            CellActivity.start_apoptosis(c)
            dead_cells.Add(c)
            live_cells.Remove(c) |> ignore
            all_cells.Remove(c) |> ignore
            if c.Type = CellType.NonStem then nonstem_cells_num <- nonstem_cells_num-1

        let goto_necrosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = GotoNecrosis)
        for c in goto_necrosis_cells do
            CellActivity.goto_necrosis(c)
            dead_cells.Add(c)
            live_cells.Remove(c) |> ignore
            if c.Type = CellType.NonStem then nonstem_cells_num <- nonstem_cells_num-1

        dying_cells_num <- goto_necrosis_cells.Count + start_apoptosis_cells.Count

    let automata_step() = 
        // compute the action to take
        all_cells.ForEach(Action<Cell>(CellActivity.compute_action !ext_state))

        //remember the numbers of cells of different type at the beggining of the time frame - for statistics
        live_cells_num_prev <- live_cells.Count
        stem_cells_num_prev <- stem_cells_num
        nonstem_cells_num_prev <- nonstem_cells_num

        // take the action
        // 1. perform division
        let new_cells = perform_division()
        // 2. perform death
        perform_death()

        // update cells
        live_cells.ForEach(fun (c: Cell) -> CellActivity.do_step(c))
        new_cells.ForEach(fun (c: Cell) -> CellActivity.initialise_new_cell(c))
        // we don't want to execute the time step for new cells
        // and add these cells only after that
        live_cells.AddRange(new_cells) 
        all_cells.AddRange(new_cells)

        // recalculate the external state
        recalc_extstate()

    let recalculate_pos(dt: float) =

        // compute the next state (location, velocity and all forces) for each cell
        for c in all_cells do
            MolecularDynamics.compute_forces(all_cells)(c)

        // make the next state visible
        for c in all_cells do
            MolecularDynamics.move(dt)(c)
            //c.ApplyNewState()

    member this.T with get() = t and set(_t) = t <- _t
    member this.Dt with get() = dt and set(_dt) = dt <- _dt
    member this.LiveCells with get() = live_cells
    member this.AllCells with get() = all_cells
    member this.DeadCells with get() = dead_cells
    member this.ExtState with get() = !ext_state
    member this.ExtStateRef with get() = ext_state

    member this.CellActivityStatistics(t: CellType) =
        match t with
        | CellType.Stem -> stem_cell_activity_stat
        | CellType.NonStem -> nonstem_cell_activity_stat
        | _ -> raise (InnerError(sprintf "division statistics not supported for %s state" (Cell.TypeAsStr(t))));

    member this.init_simulation() =
        t <- 0
        init()

    member this.simulate(n: int) =
        for i = 0 to n-1 do
            recalculate_pos(float dt)
            automata_step()
            t <- t + dt

    member this.GetCellStatistics(stat_type: StatType) =
        match stat_type with
            | Live -> live_cells_num_prev
            | Dividing -> dividing_cells_num
            | Dying -> dying_cells_num
            | Dead -> dead_cells.Count
            | Stem -> stem_cells_num_prev
            | NonStem -> nonstem_cells_num_prev
            | NonStemWithMemory -> ExternalState.CountCells(live_cells, 
                                                            fun (c:Cell) -> c.Type = CellType.NonStemWithMemory)


    member this.Clear() =
        all_cells.Clear()
        live_cells.Clear()
        dead_cells.Clear()
        apoptotic_cells.Clear()
        necrotic_cells.Clear()

        live_cells_num_prev <- 0
        dividing_cells_num <- 0
        dying_cells_num <- 0
        stem_cells_num <- 0
        stem_cells_num_prev <- 0
        nonstem_cells_num <- 0
        nonstem_cells_num_prev <- 0

        (!ext_state).Peripheral.Clear()