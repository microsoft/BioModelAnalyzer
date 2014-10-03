module Model

open Cell
open MolecularDynamics
open ModelParameters
open Automata
open System
open Geometry
open RadiationEvent


// CellDivisionStatistics is a container for statistics of cell division
type CellDivisionStatistics() =
    let mutable time_between_divisions_sum = 0
    let mutable num_of_summands = 0
    let mutable maxtime_between_divisions = 0
    let mutable mintime_between_divisions = 0

    member this.AverageTimeBetweenDivisions with get() = if num_of_summands > 0 then
                                                            float time_between_divisions_sum / float num_of_summands
                                                         else 0.

    member this.MaxTimeBetweenDivisions with get() = maxtime_between_divisions
    member this.MinTimeBetweenDivisions with get() = mintime_between_divisions

    member this.AddData(dt: int) =
        time_between_divisions_sum <- time_between_divisions_sum + dt
        num_of_summands <- num_of_summands + 1
        if dt < mintime_between_divisions || mintime_between_divisions = 0 then mintime_between_divisions <- dt
        if dt > maxtime_between_divisions then maxtime_between_divisions <- dt


// CellCycleStatistics contains information and statistics of cells in each phase of the cell cycle
type CellCycleStatistics() =
    let mutable live_cell_array = new ResizeArray<Cell>()
    let mutable all_cell_array = new ResizeArray<Cell>()
    let mutable total_functioning_cells = 0
    let mutable total_prenecrotic_cells = 0
    let mutable total_live_cells = 0
    let mutable total_apoptotic_cells = 0
    let mutable total_necrotic_cells = 0
    let mutable total_dead_cells = 0
    let mutable numofcells_G0 = 0
    let mutable numofcells_G1 = 0
    let mutable numofcells_S = 0
    let mutable numofcells_G2M = 0
    let mutable percentofcells_G0 = 0.
    let mutable percentofcells_G1 = 0.
    let mutable percentofcells_S = 0.
    let mutable percentofcells_G2M = 0.

    member this.TotalFunctioningCells with get() = total_functioning_cells
    member this.TotalPreNecroticCells with get() = total_prenecrotic_cells
    member this.TotalLiveCells with get() = total_live_cells
    member this.TotalApoptoticCells with get() = total_apoptotic_cells
    member this.TotalNecroticCells with get() = total_necrotic_cells
    member this.TotalDeadCells with get() = total_dead_cells

    member this.NumOfCellsG0 with get() = numofcells_G0 
    member this.NumOfCellsG1 with get() = numofcells_G1
    member this.NumOfCellsS with get() = numofcells_S
    member this.NumOfCellsG2M with get() = numofcells_G2M
    
    member this.PercentOfCellsG0 with get() = if total_live_cells > 0 then (float numofcells_G0 / float total_live_cells) * 100. else 0.
    member this.PercentOfCellsG1 with get() = if total_live_cells > 0 then (float numofcells_G1 / float total_live_cells) * 100. else 0.
    member this.PercentOfCellsS with get() = if total_live_cells > 0 then (float numofcells_S / float total_live_cells) * 100. else 0.
    member this.PercentOfCellsG2M with get() = if total_live_cells > 0 then (float numofcells_G2M / float total_live_cells) * 100. else 0.

    member this.UpdateArray(live_cells: ResizeArray<Cell>, all_cells: ResizeArray<Cell>) =
        live_cell_array <- live_cells
        all_cell_array <- all_cells
        total_live_cells <- live_cell_array.FindAll(fun (c: Cell) -> c.State = FunctioningState || c.State = PreNecrosisState).Count
        total_dead_cells <- all_cell_array.FindAll(fun (c: Cell) -> c.State = ApoptosisState || c.State = NecrosisState).Count
        total_functioning_cells <- live_cell_array.FindAll(fun (c: Cell) -> c.State = FunctioningState).Count
        total_prenecrotic_cells <- live_cell_array.FindAll(fun (c: Cell) -> c.State = PreNecrosisState).Count
        total_apoptotic_cells <- all_cell_array.FindAll(fun (c: Cell) -> c.State = ApoptosisState).Count
        total_necrotic_cells <- all_cell_array.FindAll(fun (c:Cell) -> c.State = NecrosisState).Count
        numofcells_G0 <- live_cell_array.FindAll(fun (c: Cell) -> (c.State = FunctioningState || c.State = PreNecrosisState) && c.CellCycleStage = G0).Count
        numofcells_G1 <- live_cell_array.FindAll(fun (c: Cell) -> (c.State = FunctioningState || c.State = PreNecrosisState) && c.CellCycleStage = G1).Count
        numofcells_S <- live_cell_array.FindAll(fun (c: Cell) -> (c.State = FunctioningState || c.State = PreNecrosisState) && c.CellCycleStage = S).Count
        numofcells_G2M <- live_cell_array.FindAll(fun (c: Cell) -> (c.State = FunctioningState || c.State = PreNecrosisState) && c.CellCycleStage = G2_M).Count


// the model of the biological system
// Model contains all the logic of the program
type Model() =

    let mutable t = 0  // the current time
    let mutable dt = 1 // the delta by which the time is increased each step

    let live_cells = new ResizeArray<Cell>() // live_cells contains all the live cells of the model
                                             // (the cells have state Functioning or PreNecrosis)

    let all_cells = new ResizeArray<Cell>()  // all_cells contains live as well as necrotic cells
                                             // (the cells have state Functioning, Prenecrotic or Necrotic)
    
    let mutable stem_cells_num_next = 0     // the number of stem cells after division
    let mutable nonstem_cells_num_next = 0  // the number of non-stem cells after division

    let glb = new GlobalState()             // the global state of the system

    let stem_cell_division_stat = new CellDivisionStatistics()      // the statistics of stem cell division
    let nonstem_cell_division_stat = new CellDivisionStatistics()   // the statistics of non-stem cell division

    let stem_cell_cycle_stat = new CellCycleStatistics()    // the cell cycle statistics for stem cells
    let nonstem_cell_cycle_stat = new CellCycleStatistics() // the cell cycle statistics for non-stem cells

    let radiation_event = new RadiationEvent()  // create radiation event, no cells will be killed until time of radiation event is reached

    // take the division action
    // returns the list of the new cells (offsprings)
    let perform_division() =

        let cytokinesis_cells = live_cells.FindAll(fun (c: Cell) -> c.Action = GotoCytokinesis)

        // perform the division of stem cells
        let stem_sym_div_cells = cytokinesis_cells.FindAll(fun (c:Cell) -> c.DivideAction = StemSymmetricDivision)
        let stem_asym_div_cells = cytokinesis_cells.FindAll(fun (c:Cell) -> c.DivideAction = StemAsymmetricDivision)
        let stem_div_cells = new ResizeArray<Cell>(stem_sym_div_cells.Count + stem_asym_div_cells.Count)
        stem_div_cells.AddRange(stem_sym_div_cells)
        stem_div_cells.AddRange(stem_asym_div_cells)

        stem_div_cells.ForEach(fun(c: Cell) -> stem_cell_division_stat.AddData(c.StepsAfterLastDivision))
        stem_cells_num_next <- stem_cells_num_next + stem_sym_div_cells.Count

        // perform the division of non-stem cells
        let nonstem_div_cells = cytokinesis_cells.FindAll(fun (c:Cell) -> c.DivideAction = NonStemDivision)
        nonstem_div_cells.ForEach(fun(c: Cell) -> nonstem_cell_division_stat.AddData(c.StepsAfterLastDivision))
        nonstem_cells_num_next <- nonstem_cells_num_next + stem_asym_div_cells.Count + nonstem_div_cells.Count

        // update the lists of cells and the global state
        glb.NumofDividingCells <- stem_div_cells.Count + nonstem_div_cells.Count
        let new_cells = new ResizeArray<Cell>(2 * glb.NumofDividingCells)
        let dividing_cells = new ResizeArray<Cell>(glb.NumofDividingCells)
        dividing_cells.AddRange(stem_div_cells)
        dividing_cells.AddRange(nonstem_div_cells)

        for c in dividing_cells do
            new_cells.AddRange(Automata.divide(glb)(c))    // call Automata.divide, will return two new cells
            all_cells.Remove(c) |> ignore
            live_cells.Remove(c) |> ignore

        new_cells

    // take the death action
    let perform_death() =
        let start_apoptosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = StartApoptosis)
        for c in start_apoptosis_cells do
            Automata.start_apoptosis(c)
            live_cells.Remove(c) |> ignore
            all_cells.Remove(c) |> ignore
            if c.Type = CellType.NonStem then nonstem_cells_num_next <- nonstem_cells_num_next-1

        let goto_necrosis_cells = live_cells.FindAll(fun (c:Cell) -> c.Action = GotoNecrosis)
        for c in goto_necrosis_cells do
            Automata.goto_necrosis(c)
            live_cells.Remove(c) |> ignore
            if c.Type = CellType.NonStem then nonstem_cells_num_next <- nonstem_cells_num_next-1

        let die_by_radiation_cells = live_cells.FindAll(fun (c: Cell) -> c.Action = DieByRadiation)
        for c in die_by_radiation_cells do
            Automata.die_by_radiation(c)
            live_cells.Remove(c) |> ignore
            all_cells.Remove(c) |> ignore
            if c.Type = CellType.NonStem then nonstem_cells_num_next <- nonstem_cells_num_next-1

        glb.NumofDyingCells <- goto_necrosis_cells.Count + start_apoptosis_cells.Count

    // progress through cell cycle
    let progress_cell_cycle() = 
        let toG0_cells = live_cells.FindAll(fun(c: Cell) -> c.Action = GotoG0)
        for c in toG0_cells do
            Automata.go_to_G0(c) 

        let toS_cells = live_cells.FindAll(fun (c: Cell) -> c.Action = GotoS)
        for c in toS_cells do
            Automata.go_to_S(c) 
        
        let toG2M_cells = live_cells.FindAll(fun (c: Cell) -> c.Action = GotoG2M)
        for c in toG2M_cells do
            Automata.go_to_G2M(c) 

    // run one step in the simulation of the model 
    let do_automata_step() = 

        // compute the action to take for each cell
        all_cells.ForEach(Action<Cell>(Automata.compute_action glb))

        // remember the numbers of cells of different type before taking any action
        //    (i.e. at the beginning of the time frame)
        glb.NumofLiveCells <- live_cells.Count
        glb.NumofStemCells <- stem_cells_num_next
        glb.NumofNonstemCells <- nonstem_cells_num_next
        glb.NumofNonstemWithmemoryCells <- GlobalState.CountCells(live_cells, fun (c: Cell) -> c.Type = NonStemWithMemory)
        
        // update the cell cycle statistics
        stem_cell_cycle_stat.UpdateArray(live_cells.FindAll(fun (c: Cell) -> c.Type = Stem), all_cells.FindAll(fun (c: Cell) -> c.Type = Stem))
        nonstem_cell_cycle_stat.UpdateArray(live_cells.FindAll(fun (c: Cell) -> c.Type = NonStem), all_cells.FindAll(fun (c: Cell) -> c.Type = NonStem))

        // take the action
        //   progress cell cycle
        progress_cell_cycle()
        //   perform division
        let new_cells = perform_division()

        if (t = ModelParameters.RadiationEventTimeStep) then
            for c in all_cells do
                if RadiationEvent.cell_will_die_by_radiation(c) then 
                    c.Action <- DieByRadiation

        //   perform death
        perform_death()

        // update the time-related fields of the old cells
        live_cells.ForEach(fun (c: Cell) -> Automata.do_step(c))
        // initialise the new cells
        new_cells.ForEach(fun (c: Cell) -> Automata.initialise_new_cell(c))

        // add the new cells to the live_cells after the live_cells are updated
        // because the new cells don't need to be updated        
        live_cells.AddRange(new_cells) 
        all_cells.AddRange(new_cells)

        // recalculate the external state
        Automata.recalc_global_state(glb, all_cells, dt)

    // recalculate the positions of the cells
    let recalculate_cell_positions(dt: float) =

        // compute the forces for each cell
        for c in all_cells do
            MolecularDynamics.compute_forces(all_cells)(c)

        // recalculate the location and velocity for each cell
        for c in all_cells do
            MolecularDynamics.move(dt)(c)

    member this.LiveCells with get() = live_cells
    member this.AllCells with get() = all_cells
    member this.GlobalState with get() = glb

    member this.StemCellDivisionStatistics with get() = stem_cell_division_stat
    member this.NonStemCellDivisionStatistics with get() = nonstem_cell_division_stat

    member this.StemCellCycleStatistics with get() = stem_cell_cycle_stat
    member this.NonStemCellCycleStatistics with get() = nonstem_cell_cycle_stat

    // initialise the model
    member this.init() =
        t <- 0
        let first_cell = new Cell(cell_type = CellType.Stem, generation = 0, location = Point(),
                                  radius = ModelParameters.AverageCellRadius, density = 1.)
        all_cells.Add(first_cell)
        live_cells.Add(first_cell)
        stem_cells_num_next <- 1
        nonstem_cells_num_next <- 0
        Automata.calc_cellpackdensity(glb, all_cells)

    // run the simulation of the model for n steps
    member this.simulate(n: int) =
        for i = 0 to n-1 do
            recalculate_cell_positions(float dt)
            do_automata_step()
            t <- t + dt

    // Clear() should be called before re-running the model
    member this.Clear() =
        all_cells.Clear()
        live_cells.Clear()

        stem_cells_num_next <- 0
        nonstem_cells_num_next <- 0

        glb.Clear()
