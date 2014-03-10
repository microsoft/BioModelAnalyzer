module MainForm

open System
open System.Windows.Forms
open System.Threading
open StemCellParamForm
open NonStemCellParamForm
open ForcesParamForm
open GlobalStateParamForm
open GlobalStateForm
//open GeneExpressionParamForm
open CellDivisionStatisticsForm
open CellCycleStatisticsForm
open CellNumberStatisticsForm
//open RadiationForm
//open MoleculeInhibitorsForm
open Cell
open ModelParameters
open Model
open ParamFormBase
open Render
open Geometry

type ControlDelegate = delegate of unit -> unit

// the main window in the program
// starts the simulation of the model in a separate thread
type MainForm () as this =

    inherit DrawingForm (Visible = false, Width = ModelParameters.WindowSize.Width, Height = ModelParameters.WindowSize.Height)

    let mutable model = new Model()     // the model of the biological system (contains all the logic)
    let mutable step = 0                // the current step
    let mutable pause = false           // set to true when the simulation is stopped
    let mutable run_step = false        // set to true when exatly one step of the simulation should be run       
    let mutable restart_simulation = 0  // set to true when a new run of simulation should be started
    
    let render = new CellRender(base.Graphics)  // used to render the cells on the form
    static let mutable file_num = 0             // the number of the file to which the current screenshot will be saved

    let mutable cancellation_token = new CancellationTokenSource()  // used to stop all the simulation thread when the main window is closed
    
    // the dialogs which can be opened from the main window
    let stem_prop_dialog = new StemCellParamForm()          // the parameters of stem cells
    let nonstem_prop_dialog = new NonStemCellParamForm()    // the parameters of non-stem cells
    let forces_prop_dialog = new ForcesParamForm()          // the parameters of forces acting on the cells
    let glbstate_prop_dialog = new GlobalStateParamForm()   // the parameters of the global state
//    let gene_expression_dialog = new GeneExpressionParamForm()  // the parameters of gene expression (for EGFR and PDGFR)
//    let cell_cycle_dialog = new CellCycleParamForm()          // the parameters of the cell cycle

    let cellnumbers_stat_dialog = new CellNumberStatisticsForm()  // the statistics of the number of different types of cells in the model
    let division_statistics_dialog = new CellDivisionStatisticsForm(model.StemCellDivisionStatistics,   
                                                                        model.NonStemCellDivisionStatistics) // the statistics of cell division
    let cell_cycle_stat_dialog = new CellCycleStatisticsForm(model.StemCellCycleStatistics, model.NonStemCellCycleStatistics)  // the cell cycle statistics
    let o2_dialog = new O2Form(model.GlobalState, model.GlobalState.O2, ModelParameters.O2Limits) // the plot of the function of the concentration of oxygen
    let glucose_dialog = new GlucoseForm(model.GlobalState, model.GlobalState.Glucose, ModelParameters.GlucoseLimits)   // the plot of the function of the concentration of glucose
    
//    let radiation_dialog = new RadiationForm()  // the dialog for radiation
//    let molecule_inhibitors = new MoleculeInhibitorsForm()  // the dialog for molecule inhibitors

    let cell_tooltip = new ToolTip() // a pop-up tooltip, shows the summary of the system in a given point
    let status_bar = new StatusBar() // shows the state and the number of step of the simulation 
    let mainwindow_delegate = new ControlDelegate(this.UpdateMainWindow)    // a delegate used to update the main window from the simulation thread
    let dialogs_delegate = new ControlDelegate(this.PlotStatistics)         // a delegate used to update the dialogs from the simulation thread
    let mutable save_screenshots = false    // set to true if screenshots of the simulation of the model shold be saved

    let disable_close(form: ParamFormBase) =
        form.FormClosing.Add(ParamFormBase.hide_form form)

    // KeyPressEvent handler
    let key_press(args: KeyPressEventArgs) =
        // stop/continue the simulation of the model
        if args.KeyChar = 'p' then
            pause <- not pause
            this.UpdateMainWindow()
        // run exactly one step of the simulation
        else if args.KeyChar = 's' then
            run_step <- true

    // turn on/off saving screenshots of the model simulation
    let change_saving_screenshots(save_screenshots_menuitem: MenuItem) =
        save_screenshots_menuitem.Checked <- not save_screenshots_menuitem.Checked
        save_screenshots <- save_screenshots_menuitem.Checked
        if save_screenshots then
            let files = IO.Directory.GetFiles(IO.Directory.GetCurrentDirectory(), sprintf "%s*" output_file)
            for file in files do IO.File.Delete(file)

    do
        base.ClientSize <- FormDesigner.Scale(base.Size, (0.95, 0.95))
        
        // create the main menu
        base.Menu <- new MainMenu()

        let simulation_menuitem = new MenuItem("Simulation")
        let rerun_model_menuitem = new MenuItem("Rerun the simulation")
        rerun_model_menuitem.Click.Add(fun args ->
            Interlocked.Exchange(&restart_simulation, 1) |> ignore)
        let save_screenshots_menuitem = new MenuItem("Save the screenshots")
        save_screenshots_menuitem.Click.Add(fun args -> change_saving_screenshots(save_screenshots_menuitem))
        simulation_menuitem.MenuItems.AddRange([| rerun_model_menuitem; save_screenshots_menuitem |])

        let model_param_menuitem = new MenuItem("Model parameters")
        let stem_cell_param_menuitem = new MenuItem("Stem cells")
        stem_cell_param_menuitem.Click.Add(fun args -> stem_prop_dialog.Visible <- true)
        let nonstem_cell_param_menuitem = new MenuItem("Non-stem cells")
        nonstem_cell_param_menuitem.Click.Add(fun args -> nonstem_prop_dialog.Visible <- true)
        let global_state_param_menuitem = new MenuItem("The global state")
        global_state_param_menuitem.Click.Add(fun args -> glbstate_prop_dialog.Visible <- true)
        let forces_param_menuitem = new MenuItem("Forces")
        forces_param_menuitem.Click.Add(fun args -> forces_prop_dialog.Visible <- true)
        let gene_expression_param_menuitem = new MenuItem("Gene Expression")
//        gene_expression_menuitem.Click.Add(fun args -> gene_expression_dialog.Visible <- true)
        let cell_cycle_param_menuitem = new MenuItem("Cell Cycle")
//        cell_cycle_param_menuitem.Click.Add(fun args -> cell_cycle_dialog.Visible <- true)
        model_param_menuitem.MenuItems.AddRange([| stem_cell_param_menuitem; nonstem_cell_param_menuitem;
                                                   forces_param_menuitem; global_state_param_menuitem; 
                                                   gene_expression_param_menuitem; cell_cycle_param_menuitem |]) |> ignore

        let stat_menuitem = new MenuItem("Statistics")
        let cell_number_menuitem = new MenuItem("Statistics of the number of cells")
        cell_number_menuitem.Click.Add(fun args -> cellnumbers_stat_dialog.Visible <- true)
        let division_stat_menuitem = new MenuItem("Cell division statistics")
        division_stat_menuitem.Click.Add(fun args -> division_statistics_dialog.Visible <- true)
        let cell_cycle_stat_menuitem = new MenuItem("Cell Cycle Statistics")
        cell_cycle_stat_menuitem.Click.Add(fun args -> cell_cycle_stat_dialog.Visible <- true)
        let o2_menuitem = new MenuItem("Concentration of O2")
        o2_menuitem.Click.Add(fun args -> o2_dialog.Visible <- true)
        let glucose_menuitem = new MenuItem("Concentration of glucose")
        glucose_menuitem.Click.Add(fun args -> glucose_dialog.Visible <- true)
        stat_menuitem.MenuItems.AddRange([| cell_number_menuitem; division_stat_menuitem; cell_cycle_stat_menuitem; o2_menuitem; glucose_menuitem |])

        let perturbation_menuitem = new MenuItem("Perturbations")
        let radiation_menuitem = new MenuItem("Radiation")
//        radiation_menuitem.Click.Add(fun args -> radiation_dialog.Visible <- true)
        let molecule_inhibitors_menuitem = new MenuItem("Molecule Inhibitors")
//        molecule_inhibitors_menuitem.Click.Add(fun args -> molecule_inhibitors_dialog.Visible <- true)
        perturbation_menuitem.MenuItems.AddRange([| radiation_menuitem; molecule_inhibitors_menuitem |])

        base.Menu.MenuItems.AddRange([| simulation_menuitem; model_param_menuitem; stat_menuitem; perturbation_menuitem |]) |> ignore
        
        base.Controls.Add(status_bar)

        // show/hide the summary on mouse click
        this.KeyPress.Add(fun args -> key_press(args))
        let get_summary_curried = fun (p: Drawing.Point) -> this.get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, cell_tooltip, get_summary_curried))

        base.Graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        render.Size <- base.Graphics.ClipBounds
        base.FormBorderStyle <- FormBorderStyle.FixedSingle

        // hide the dialogs instead of closing them
        Array.iter disable_close [|cellnumbers_stat_dialog :> ParamFormBase;
            stem_prop_dialog :> ParamFormBase;
            nonstem_prop_dialog :> ParamFormBase;
            forces_prop_dialog :> ParamFormBase;
            cellnumbers_stat_dialog :> ParamFormBase;
            glbstate_prop_dialog :> ParamFormBase;
            o2_dialog :> ParamFormBase;
            glucose_dialog :> ParamFormBase;
            division_statistics_dialog :> ParamFormBase |]

        // start the simulation of the model
        Async.Start(this.run_simulation(), cancellation_token.Token)


   // render the cells on the form:
   // 1. draw the image on a bitmap
   // 2. copy the bitmap to the form (which is faster) - to avoid blinking
    member this.render_cells() = 
        // draw the image on a bitmap
        render.RenderCells(model.AllCells)
        //render.DrawGrid(model.GlobalState.O2.Grid)    // uncomment this line to display grid in the main window

        // copy the bitmap to the form
        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        graphics.DrawImage(base.Bitmap, Drawing.Point(0, 0))

        // save the bitmap to a file
        if save_screenshots then
            base.Bitmap.Save((sprintf "%s%04d.png" output_file file_num), Drawing.Imaging.ImageFormat.Png)
            file_num <- file_num + 1

    override this.OnPaint(args: PaintEventArgs) =
        this.render_cells()

    // returns a verbose summary of the system in the point p
    override this.get_summary(p: Geometry.Point) =
        // get all the cells located at the point p
        let cells = model.AllCells.FindAll(fun (cell: Cell) -> Geometry.distance(p, cell.Location) < cell.R)
        
        // then get the cell the center of which is closest the the point p
        let mutable i = -1
        if cells.Count = 1 then
            i <- 0
        else if cells.Count > 1 then
            let dist = ref (Geometry.distance(cells.[0].Location, p))
            for j in 1 .. cells.Count-1 do
                let dist' = Geometry.distance(cells.[j].Location, p)
                if dist' < !dist then
                    dist := dist'
                    i <- j

        // get the summary
        let mutable msg = ""
        let grid = model.GlobalState.O2.Grid
        let (i_grid, j_grid) = grid.PointToIndices(p)

        if i >= 0 then
            msg <- String.Concat( [| cells.[i].ToString(); "\n" |] )

        String.Concat( [| (sprintf "i=%d, j=%d\n" i_grid j_grid); msg;
                          (model.GlobalState.O2Summary(p)); "\n";
                          (model.GlobalState.GlucoseSummary(p)); "\n";
                          (model.GlobalState.CellPackingDensitySummary(p)) |] )

    member this.UpdateMainWindow() =
        let mutable msg = (sprintf "Simulating the model: Step %d" step)
        if pause then msg <- String.Concat(msg, " (pause)")
        status_bar.Text <- msg

    // add the statistics of the current step to all the dialogs
    member this.PlotStatistics() =
        let glb = model.GlobalState

        cellnumbers_stat_dialog.AddPoints(glb.NumofLiveCells,
            glb.NumofDividingCells, glb.NumofDyingCells,
            glb.NumofStemCells, glb.NumofNonstemCells,
            glb.NumofNonstemWithmemoryCells, step)

        o2_dialog.Refresh()
        glucose_dialog.Refresh()
        division_statistics_dialog.Refresh()

    // run the simulation of the model in a loop
    member this.run_simulation() = 
        async{
            let dstep = 1               // the number of steps to be simulated in every loop iteration
            let is_closing = ref false  // set to true if the main window is closed
            let start_time = ref (DateTime.Now) // the start time of the current time step
            let delay = 100             // the delay in milliseconds before executing the next model step

            model.init()
            step <- 0

            // the main loop
            while (not !is_closing) && step < max_steps do
                
                start_time := DateTime.Now

                // run the simulation for dstep number of steps
                if not pause || run_step then
                    model.simulate(dstep) |> ignore
                    step <- step + dstep
                    if run_step then run_step <- false

                    status_bar.Invoke(mainwindow_delegate) |> ignore
                    status_bar.Invoke(dialogs_delegate) |> ignore
                    this.render_cells()

                // make a delay before executing the next step
                let wait_before_next_step = delay - (DateTime.Now - !start_time).Milliseconds
                if wait_before_next_step > 0 then
                    do! Async.Sleep(wait_before_next_step)

                // determine if the simulation should terminate
                let! token = Async.CancellationToken
                if token.IsCancellationRequested then
                    is_closing := true

                // determine if the simulation should be restarted
                if Interlocked.CompareExchange(&restart_simulation, 0, 1) = 1 then
                    model.Clear()
                    model.init()
                    cellnumbers_stat_dialog.Clear()
                    step <- 0
        }