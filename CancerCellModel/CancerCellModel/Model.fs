module Model
open Cell
open Visualisation
open System

type CellStatistics(max_len: int) =

    let mutable live_stat: float[] = [||]
    let mutable dead_stat: float[] = [||]
    let mutable stem_stat: float[] = [||]
    let mutable non_stem_stat: float[] = [||]

    member this.LiveStat with get() = live_stat
    member this.DeadStat with get() = dead_stat
    member this.StemStat with get() = stem_stat
    member this.NonStemStat with get() = non_stem_stat

    member this.add_data(stat: float[] byref, data: int) =
        stat <- Array.append(stat)([|float data|])
        if stat.Length > max_len then
            stat <- Array.sub stat 1 max_len

    member this.add_data(live: int, dead: int, stem: int, non_stem: int) =
        this.add_data(&live_stat, live)
        this.add_data(&dead_stat, dead)
        this.add_data(&stem_stat, stem)
        this.add_data(&non_stem_stat, non_stem)

type ExtStatistics(max_len: int) =
 
    let mutable o2_stat: float[] = [||]
    member this.O2 with get() = o2_stat

    member this.add_data(stat: float[] byref, data: float) =
        stat <- Array.append(stat)([|float data|])
        if stat.Length > max_len then
            stat <- Array.sub stat 1 max_len

    member this.add_data(o2: float) =
        this.add_data(&o2_stat, o2)

type Model() =

    let mutable t: int = 0
    let mutable dt: int = 1
    let mutable max_stat_len: int = 100

    let mutable live_cells: Cell[] = [|new Cell()|]
    let mutable dead_cells: Cell[] = [||]
    let mutable ext_state = new ExternalState()

    let cell_stat = new CellStatistics(max_stat_len)
    let ext_stat = new ExtStatistics(max_stat_len)

    let cell_form = new CellStatisticsForm("Cell statistics")
    let ext_state_form = new ExtStatisticsForm("Nutrition statistics")
    
    let stat_forms = [ cell_form :> LineChartForm ]
                       //ext_state_form :> LineChartForm ]

    let do_step() = 
    
        Array.iter (CellActivity.compute_action ext_state) live_cells    
        let old_cells = Array.filter(fun (c:Cell) -> c.Action = NoAction) live_cells

        let new_cells1 = (Array.filter(fun (c:Cell) -> c.Action = AsymSelfRenew) live_cells |>
                            Array.collect CellActivity.asym_divide)

        let new_cells2 = (Array.filter(fun (c:Cell) -> c.Action = SymSelfRenew || c.Action = NonStemDivide)
                             live_cells |> Array.collect CellActivity.sym_divide)

        let newly_dead = Array.filter(fun (c:Cell) -> c.Action = Die) live_cells
        newly_dead |> Array.iter CellActivity.die

        live_cells <- Array.concat([old_cells; new_cells1; new_cells2])
        dead_cells <- Array.concat([dead_cells; newly_dead])

        //recalculate the external state
        CellActivity.recalculate_ext_state(ext_state, dt, live_cells.Length)
        t <- t + dt

    let get_statistics() =
        cell_stat.add_data(live_cells.Length, dead_cells.Length,
            (Array.filter(fun (c: Cell) -> c.State = CellState.Stem) live_cells) |> Array.length,
            (Array.filter(fun (c: Cell) -> c.State = CellState.NonStem) live_cells) |> Array.length)

        ext_stat.add_data(ext_state.O2)

    let draw_statistics() =
        let t0 = ref (t-dt* (max_stat_len-1))
        if t0.Value < 0 then t0.Value <- 0
        let t_seq = seq {for x in t0.Value .. dt .. t -> float x}
        
        cell_form.AddPoints(cell_stat.LiveStat :> seq<float> |> Seq.zip t_seq, StatType.Live)
        cell_form.AddPoints(cell_stat.DeadStat :> seq<float> |> Seq.zip t_seq, StatType.Dead)
        cell_form.AddPoints(cell_stat.StemStat :> seq<float> |> Seq.zip t_seq, StatType.Stem)
        cell_form.AddPoints(cell_stat.NonStemStat :> seq<float> |> Seq.zip t_seq, StatType.NonStem)

        ext_state_form.AddPoints(Seq.zip t_seq ext_stat.O2)

    let runAsync(form: LineChartForm) =
     async { 
        try 
            form.ShowDialog() |> ignore
        with
            | ex -> printfn "%s" (ex.Message);
    }


    member this.T with get() = t and set(_t) = t <- _t
    member this.Dt with get() = dt and set(_dt) = dt <- _dt
    member this.LiveCells with get() = live_cells
    member this.DeadCells with get() = dead_cells
    member this.ExtState with get() = ext_state

    member this.simulate(n: int) =
        //f.Show()//Dialog() |> ignore
        for i = 1 to n do
            //cell_form.Show();
 //           f.Enabled <- false;
            
            //do your stuff here
            get_statistics()
            //cell_form.Clear()
            //draw_statistics()
            //cell_form.Update()
            do_step()
            //Async.Sleep (1000) |> ignore

            //f.Hide();
            //f.Enabled <- true;
        draw_statistics()
        stat_forms 
        |> Seq.map runAsync
        |> Async.Parallel 
        |> Async.RunSynchronously
        |> ignore

 (*       cell_form.Show()
        cell_form.Enabled <- false
        //cell_form.ShowDialog() |> ignore
        ext_state_form.Show()
        cell_form.Enabled <- true*)