module MainForm

open System
open System.Windows.Forms
open System.Threading
open StemCellParamForm
open NonStemCellParamForm
open ForcesParamForm
open ExternalStateParamForm
open ExternalStateForm
open CellActivityStatForm
open CellNumbersStatForm
open Cell
open ModelParameters
open Model
open ParamFormBase
open Render
open MyMath

type ControlDelegate = delegate of unit -> unit

type MainForm () as this =
    inherit DrawingForm (Visible = false, Width = ModelParameters.GridSize.Width, Height = ModelParameters.GridSize.Height)

    let stem_prop_dialog = new StemCellParamForm()
    let nonstem_prop_dialog = new NonStemCellParamForm()
    let forces_prop_dialog = new ForcesParamForm()
    let extstate_prop_dialog = new ExternalStateParamForm()
    let activity_stat_dialog = new CellActivityStatForm()
    let cellnumbers_stat_dialog = new CellNumbersStatForm()
    let model = new Model()
    let o2_dialog = new O2Form(model.ExtStateRef)
    let cell_density_dialog = new DensityForm(model.ExtStateRef)
    let status_bar = new StatusBar()
    let mainwindow_delegate = new ControlDelegate(this.UpdateMainWindow)
    let dialogs_delegate = new ControlDelegate(this.PlotStat)
    let mutable step = 0
    let mutable cancellation_token = new CancellationTokenSource()
    let pause = ref false
    let mutable run_step = false
    let cell_tooltip = new ToolTip()

    let render = new CellRender(base.Graphics)

    let disable_close(form: ParamFormBase) =
        form.FormClosing.Add(ParamFormBase.hide_form form)

    let key_press(args: KeyPressEventArgs) =
        if args.KeyChar = 'p' then
            pause := not !pause
        else if args.KeyChar = 's' then
            run_step <- true

    do
        base.ClientSize <- FormDesigner.Scale(base.Size, (0.95, 0.95))
        
        // create the main menu
        base.Menu <- new MainMenu()

        let simulation = new MenuItem("Simulation")
        let rerun_model = new MenuItem("Rerun the simulation")
        
        rerun_model.Click.Add(fun args ->
            cancellation_token.Cancel()
            cancellation_token <- new CancellationTokenSource()
            Async.Start(this.run(), cancellation_token.Token))

        simulation.MenuItems.AddRange([|rerun_model|])

        let model_param = new MenuItem("Model parameters")
        let stem_cell_param = new MenuItem("Stem cells")
        stem_cell_param.Click.Add(fun args -> stem_prop_dialog.Visible <- true)
        let nonstem_cell_param = new MenuItem("Non-stem cells")
        nonstem_cell_param.Click.Add(fun args -> nonstem_prop_dialog.Visible <- true)
        let ext_state_param = new MenuItem("The external state")
        ext_state_param.Click.Add(fun args -> extstate_prop_dialog.Visible <- true)
        let forces_param = new MenuItem("Forces")
        forces_param.Click.Add(fun args -> forces_prop_dialog.Visible <- true)
        model_param.MenuItems.AddRange([|stem_cell_param; nonstem_cell_param; forces_param; ext_state_param|]) |> ignore

        let stat = new MenuItem("Statistics")
        let cell_number = new MenuItem("Cell number")
        cell_number.Click.Add(fun args -> cellnumbers_stat_dialog.Visible <- true)
        let cell_activity = new MenuItem("Cell activity")
        cell_activity.Click.Add(fun args -> activity_stat_dialog.Visible <- true)
        let o2 = new MenuItem("The concentration of O2")
        o2.Click.Add(fun args -> o2_dialog.Visible <- true)
        let cell_density = new MenuItem("Cell density")
        cell_density.Click.Add(fun args -> cell_density_dialog.Visible <- true)

        stat.MenuItems.AddRange([|cell_number; cell_activity; o2; cell_density|])
        base.Menu.MenuItems.AddRange([|simulation; model_param; stat|]) |> ignore
        
        base.Controls.Add(status_bar)
        this.KeyPress.Add(fun args -> key_press(args))
        let get_summary_curried = fun (p: Drawing.Point) -> this.get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, cell_tooltip, get_summary_curried, pause))
        base.Graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        render.Size <- base.Graphics.ClipBounds
        base.FormBorderStyle <- FormBorderStyle.FixedSingle

        Array.iter disable_close [|cellnumbers_stat_dialog :> ParamFormBase;
            stem_prop_dialog :> ParamFormBase;
            nonstem_prop_dialog :> ParamFormBase;
            forces_prop_dialog :> ParamFormBase;
            cellnumbers_stat_dialog :> ParamFormBase;
            extstate_prop_dialog :> ParamFormBase;
            o2_dialog :> ParamFormBase;
            cell_density_dialog :> ParamFormBase;
            activity_stat_dialog :> ParamFormBase |]

        Async.Start(this.run(), cancellation_token.Token)

    member this.render_cells() = 
        render.RenderCells(model.LiveCells)
        render.DrawGrid(model.ExtState.o2.Grid)

        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        graphics.DrawImage(base.Bitmap, Drawing.Point(0, 0))

    override this.OnPaint(args: PaintEventArgs) =
        this.render_cells()

    override this.get_summary(point: Geometry.Point) =
        let cells = Array.filter(fun (cell: Cell) -> Geometry.distance(point, cell.Location) < cell.R) model.LiveCells
        
        let mutable i = -1
        if cells.Length = 1 then
            i <- 0
        else if cells.Length > 1 then
            let dist = ref (Geometry.distance(cells.[0].Location, point))
            for j in 1 .. cells.Length-1 do
                let dist' = Geometry.distance(cells.[j].Location, point)
                if dist' < !dist then
                    dist := dist'
                    i <- j

        let mutable msg = ""
        let grid = model.ExtState.o2.Grid
        let (i_grid, j_grid) = grid.PointToIndices(point)

        if i >= 0 then
            msg <- String.Concat( [| cells.[i].Summary(); "\n" |] )

        String.Concat( [| (sprintf "i=%d, j=%d\n" i_grid j_grid); msg; (model.ExtState.O2ToStringVerbose(point)); "\n"; (model.ExtState.DensityToStringVerbose(point)) |] )

    member this.UpdateMainWindow() =
        let mutable msg = (sprintf "Simulating the model: Step %d" step)
        if !pause then msg <- String.Concat(msg, " (pause)")
        status_bar.Text <- msg

    member this.PlotStat() =
        cellnumbers_stat_dialog.AddPoints(model.GetCellStatistics(Live),
            model.GetCellStatistics(Dividing), model.GetCellStatistics(Dying),
            model.GetCellStatistics(Stem), model.GetCellStatistics(NonStem),
            model.GetCellStatistics(NonStemWithMemory))

        o2_dialog.AddPoints(model.ExtState.O2, ModelParameters.O2Limits)
        cell_density_dialog.AddPoints(model.ExtState.CellPackDensity, ModelParameters.CellPackDensityLimits)

        activity_stat_dialog.StemDivisionStatistics <- model.CellActivityStatistics(CellType.Stem)
        activity_stat_dialog.NonStemDivisionStatistics <- model.CellActivityStatistics(CellType.NonStem)

    member this.run() = 
        async{
            model.init_simulation()
            let dstep = 1
            step <- 0
            let is_closing = ref false
            let start_time = ref (DateTime.Now)
            let delay = 100 // delay in milliseconds between executing model steps
            while (not !is_closing) && step < 4000 do
                start_time := DateTime.Now
                if not !pause || run_step then
                    if (dstep = 1 || (step+1) % dstep <> 0) then
                        model.simulate(dstep) |> ignore

                    status_bar.Invoke(mainwindow_delegate) |> ignore
                    status_bar.Invoke(dialogs_delegate) |> ignore
                    this.render_cells()
                    step <- step + dstep
                    if run_step then run_step <- false

                let wait_time = delay - (DateTime.Now - !start_time).Milliseconds
                if wait_time > 0 then
                    do! Async.Sleep(wait_time)

                let! token = Async.CancellationToken
                if token.IsCancellationRequested then
                    is_closing := true
        }