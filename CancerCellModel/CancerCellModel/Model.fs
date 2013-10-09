module Model
open System.Windows.Forms.DataVisualization.Charting
open System.Windows.Forms
open Cell
//open MainForm
open ModelParameters
open System

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

    member this.AddData(div_data: int[]) =
        div_data |> Array.iter (fun (interval: int) ->
                     time_between_divisions_sum <- time_between_divisions_sum + interval;
                     num_of_summands <- num_of_summands + 1;
                     if interval < mintime_between_divisions || mintime_between_divisions = 0 then mintime_between_divisions <- interval
                     if interval > maxtime_between_divisions then maxtime_between_divisions <- interval)

type Model() =

    let mutable t: int = 0
    let mutable dt: int = 1
    //let mutable max_stat_len: int = 100

    let mutable live_cells: Cell[] = [||]
    let mutable dead_cells: Cell[] = [||]
    let mutable dividing_cells: Cell[] = [||]
    let mutable dying_cells: Cell[] = [||]
    // we need dividing_cells and dying_cells only for statistics

    let mutable ext_state = new ExternalState()
    let cell_stat = new CellStatistics()//max_stat_len)
    let ext_stat = new ExtStatistics()//max_stat_len)

    // statistics for division is a tuple of four numbers, of which
    // the first is the sum of the number of steps between cell division
    // the second is the number of summands in the first element
    // (the idea is to update both and then to divide the first by the second)
    // the third and the fourth are the minimum and maximum resp. numbers of steps between cell division
    let mutable stem_cell_activity_stat = new CellActivityStatistics()
    let mutable nonstem_cell_activity_stat = new CellActivityStatistics()

    let init() =
        live_cells <- [|new Cell(CellType.Stem)|]
        dead_cells <- [||]
        ext_state <- new ExternalState()
    
    let collect_statistics() =
        // collect the statistics
        cell_stat.AddData(live_cells.Length, dividing_cells.Length, dying_cells.Length, dead_cells.Length,
            (Array.filter(fun (c: Cell) -> c.Type = CellType.Stem) live_cells) |> Array.length,
            (Array.filter(fun (c: Cell) -> c.Type = CellType.NonStem) live_cells) |> Array.length,
            (Array.filter(fun (c: Cell) -> c.Type = CellType.NonStemWithMemory) live_cells) |> Array.length)

        ext_stat.AddData((*CellActivity.oxygen_per_cell(ext_state)*)ext_state.O2)

    let recalc_extstate() = 
        ext_state.LiveCells <- live_cells.Length
        ext_state.DividingCells <- dividing_cells.Length
        ext_state.StemCells <- Array.length (Array.filter(fun (c:Cell) -> c.Type = CellType.Stem) live_cells)
        CellActivity.recalculate_ext_state(ext_state, dt)

    let do_step() = 
        // compute the action to take
        Array.iter (CellActivity.compute_action ext_state) live_cells    

        // take the action
        // 1. filter the cells which do nothing
        let old_cells = Array.filter(fun (c:Cell) -> c.Action = NoAction) live_cells
        dying_cells <- Array.filter(fun (c:Cell) -> c.Action = Death) live_cells

        // 2. perform division
        let asym_div_cells = Array.filter(fun (c:Cell) -> c.Action = AsymSelfRenewal) live_cells
        let sym_div_cells = Array.filter(fun (c:Cell) -> c.Action = SymSelfRenewal || c.Action = NonStemDivision) live_cells
        let stem_div_cells = Array.filter(fun (c:Cell) -> c.Action = SymSelfRenewal || c.Action = AsymSelfRenewal) live_cells
        let nonstem_div_cells = Array.filter(fun (c:Cell) -> c.Action = NonStemDivision) live_cells

        stem_cell_activity_stat.AddData(Array.map(fun (c: Cell) -> c.StepsAfterLastDivision) stem_div_cells)
        nonstem_cell_activity_stat.AddData(Array.map(fun (c: Cell) -> c.StepsAfterLastDivision) nonstem_div_cells)
                                    
        dividing_cells <- Array.concat([|stem_div_cells; nonstem_div_cells|])
        let new_cells_sym = (Array.collect CellActivity.sym_divide sym_div_cells)
        let new_cells_asym = (Array.collect CellActivity.asym_divide asym_div_cells)
        let new_cells = Array.concat([|new_cells_sym; new_cells_asym|])

        collect_statistics()

        // update old cells and initialise the new ones
        live_cells <- Array.concat([old_cells; new_cells_sym; new_cells_asym])
        old_cells |> Array.iter(fun (c: Cell) -> CellActivity.do_step(c))
        new_cells |> Array.iter(fun (c: Cell) -> CellActivity.initialise_new_cells(c))

        // 3. perform death
        Array.iter CellActivity.die dying_cells
        dead_cells <- Array.concat([dead_cells; dying_cells])

        // recalculate the external state
        recalc_extstate()

    member this.T with get() = t and set(_t) = t <- _t
    member this.Dt with get() = dt and set(_dt) = dt <- _dt
    member this.LiveCells with get() = live_cells
    member this.DeadCells with get() = dead_cells
    member this.ExtState with get() = ext_state

    member this.CellActivityStatistics with get(t: CellType) =
                                            match t with
                                            | CellType.Stem -> stem_cell_activity_stat
                                            | CellType.NonStem -> nonstem_cell_activity_stat
                                            | _ -> raise (InnerError(sprintf "division statistics not supported for %s state" (Cell.TypeAsStr(t))));



    member this.simulate(n: int) =
        t <- 0
        cell_stat.Clear()
        ext_stat.Clear()
        init()
        collect_statistics()
        for i = 0 to n-1 do
            //collect_statistics()
            do_step()
            t <- t + dt
            

(*        stat_forms 
        |> Seq.map runAsync
        |> Async.Parallel 
        |> Async.RunSynchronously
        |> ignore*)

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

    member this.GetO2Statistics() =
        let t_seq = seq {for x in 0 .. dt .. t -> float x}
        Seq.zip t_seq ext_stat.O2