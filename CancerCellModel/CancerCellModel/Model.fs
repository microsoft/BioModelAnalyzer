module Model
open System.Windows.Forms.DataVisualization.Charting
open System.Windows.Forms
open Cell
open ModelParameters
open MolecularDynamics
open Automata
open System
open MyMath
open Geometry

type StatType = Live | Dividing | Dying | Dead | Stem | NonStem | NonStemWithMemory

type CellStatistics() = //max_len: int) =

    let mutable live_stat: float[] = [||]
    let mutable dividing_stat: float[] = [||]
    let mutable dying_stat: float[] = [||]
    let mutable total_dead_stat: float[] = [||]
    let mutable stem_stat: float[] = [||]
    let mutable nonstem_stat: float[] = [||]
    let mutable nonstem_withmem_stat: float[] = [||]

    member private this.add_data(stat: float[] byref, data: int) =
        stat <- Array.append(stat)([|float data|])
        (*if stat.Length > max_len then
            stat <- Array.sub stat 1 max_len*)

    member this.LiveStat with get() = live_stat
    member this.DividingStat with get() = dividing_stat
    member this.DyingStat with get() = dying_stat
    member this.DeadStat with get() = total_dead_stat
    member this.StemStat with get() = stem_stat
    member this.NonStemStat with get() = nonstem_stat
    member this.NonStemWithMemStat with get() = nonstem_withmem_stat
 
    member this.AddData(live: int, dividing: int, dying: int, total_dead: int,
                                stem: int, non_stem: int, nonstem_withmem: int) =

        this.add_data(&live_stat, live)
        this.add_data(&dividing_stat, dividing)
        this.add_data(&dying_stat, dying)
        this.add_data(&total_dead_stat, total_dead)
        this.add_data(&stem_stat, stem)
        this.add_data(&nonstem_stat, non_stem)
        this.add_data(&nonstem_withmem_stat, nonstem_withmem)

    member this.Clear() =
        live_stat <- [||]
        dividing_stat <- [||]
        dying_stat <- [||]
        total_dead_stat <- [||]
        stem_stat <- [||]
        nonstem_stat <- [||]
        nonstem_withmem_stat <- [||]

type ExtStatistics() = //max_len: int) =
 
    let mutable o2_stat: float[] = [||]
    
    member private this.add_data(stat: float[] byref, data: float) =
        stat <- Array.append(stat)([|float data|])
        (*if stat.Length > max_len then
            stat <- Array.sub stat 1 max_len*)

    member this.O2 with get() = o2_stat
    member this.AddData(o2: float) =
        this.add_data(&o2_stat, o2)

    member this.Clear() =
        o2_stat <- [||]

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

    let all_cells = new ResizeArray<Cell>()
    let live_cells = new ResizeArray<Cell>()
    let dead_cells = new ResizeArray<Cell>()
    let apoptotic_cells = new ResizeArray<Cell>()
    let necrotic_cells = new ResizeArray<Cell>()
    let mutable dividing_cells_num = 0
    let mutable new_stem_cells_num = 0
    let mutable new_nonstem_cells_num = 0
    let mutable dying_cells_num = 0
    let mutable stem_cells_num = 0
    let mutable nonstem_cells_num = 0
    // we need dividing_cells and dying_cells only for statistics

    let ext_state = ref (new ExternalState())
    let cell_stat = new CellStatistics()
    let ext_stat = new ExtStatistics()

    // statistics for division is a tuple of four numbers, of which
    // the first is the sum of the number of steps between cell division
    // the second is the number of summands in the first element
    // (the idea is to update both and then to divide the first by the second)
    // the third and the fourth are the minimum and maximum resp. numbers of steps between cell division
    let mutable stem_cell_activity_stat = new CellActivityStatistics()
    let mutable nonstem_cell_activity_stat = new CellActivityStatistics()

    let init() =
        let first_cell = new Cell(cell_type = CellType.Stem, generation = 0, location = Point(), radius = 10., density = 1.)
        all_cells.Add(first_cell)
        live_cells.Add(first_cell)
        stem_cells_num <- stem_cells_num + 1
        CellActivity.calc_cellpackdensity(!ext_state, all_cells)
    
    let collect_statistics() =
        // collect the statistics
        cell_stat.AddData(live_cells.Count-dividing_cells_num, dividing_cells_num, dying_cells_num, dead_cells.Count,
            stem_cells_num - new_stem_cells_num, nonstem_cells_num - new_nonstem_cells_num,
            live_cells.FindAll(fun (c:Cell) -> c.Type = CellType.NonStemWithMemory).Count)

        //ext_stat.AddData((*CellActivity.oxygen_per_cell(ext_state)*)ext_state.O2)

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
        new_stem_cells_num <- stem_sym_div_cells.Count
        stem_cells_num <- stem_cells_num + new_stem_cells_num

        let nonstem_div_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = NonStemDivision)
        nonstem_div_cells.ForEach(fun(c: Cell) -> stem_cell_activity_stat.AddData(c.StepsAfterLastDivision))
        new_nonstem_cells_num <- stem_asym_div_cells.Count + nonstem_div_cells.Count
        nonstem_cells_num <- nonstem_cells_num + new_nonstem_cells_num

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
        let start_apoptosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = Apoptosis)
        for c in start_apoptosis_cells do
            CellActivity.start_apoptosis(c)
            live_cells.Remove(c) |> ignore
            all_cells.Remove(c) |> ignore

        dead_cells.AddRange(start_apoptosis_cells)

        let goto_necrosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = Necrosis)
        for c in goto_necrosis_cells do
            CellActivity.goto_necrosis(c)
            live_cells.Remove(c) |> ignore

        dead_cells.AddRange(goto_necrosis_cells)

        dying_cells_num <- goto_necrosis_cells.Count + start_apoptosis_cells.Count

    let automata_step() = 
        // compute the action to take
        live_cells.ForEach(Action<Cell>(CellActivity.compute_action !ext_state))

        // take the action
        // 1. perform division
        let new_cells = perform_division()
        // 2. perform death
        perform_death()
        collect_statistics()

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
        // TOFIX: we should store the cells in previous positions
        // then recalculate positions with old info
        // then update positions
        for c in all_cells do
            MolecularDynamics.compute_forces(all_cells)(c)
            MolecularDynamics.move(dt)(c)

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
        cell_stat.Clear()
        ext_stat.Clear()
        init()
        collect_statistics() // collect data for step 0

    member this.simulate(n: int) =
        for i = 0 to n-1 do
            recalculate_pos(float dt)
            automata_step()
            t <- t + dt

    member this.GetCellStatistics(stat_type: StatType) =
        let t_seq = seq {for x in 0 .. dt .. t -> float x}

        (match stat_type with
            | Live -> cell_stat.LiveStat
            | Dividing -> cell_stat.DividingStat
            | Dying -> cell_stat.DyingStat
            | Dead -> cell_stat.DeadStat
            | Stem -> cell_stat.StemStat
            | NonStem -> cell_stat.NonStemStat
            | NonStemWithMemory -> cell_stat.NonStemWithMemStat)
                :> seq<float> |> Seq.zip t_seq