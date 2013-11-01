module MainForm

open System
open System.Windows.Forms
open System.Threading
open StemCellParamForm
open NonStemCellParamForm
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

type CellStatisticsForm () as this =
    inherit Form (Visible = false, Width = 1400, Height = 1000)

    let stem_prop_dialog = new StemCellParamForm()
    let nonstem_prop_dialog = new NonStemCellParamForm()
    let extstate_prop_dialog = new ExternalStateParamForm()
    let extstate_dialog = new ExternalStateForm()
    let activity_stat_dialog = new CellActivityStatForm()
    let cellnumbers_stat_dialog = new CellNumbersStatForm(extstate_dialog)
    let model = new Model()
    let status_bar = new StatusBar()
    let mainwindow_delegate = new ControlDelegate(this.UpdateMainWindow)
    let dialogs_delegate = new ControlDelegate(this.PlotStat)
    let mutable step = 0
    let cancellation_token = new CancellationTokenSource()
    let pause = ref false
    let mutable run_step = false
    let cell_tooltip = new ToolTip()

    let width, height = this.ClientSize.Width, this.ClientSize.Height
    let bitmap = new System.Drawing.Bitmap(width, height)
    let graphics = System.Drawing.Graphics.FromImage(bitmap)
    let render = new CellRender(graphics)

    let render_cells() = 
        render.RenderCells(model.LiveCells)

        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, width, height))
        graphics.DrawImage(bitmap, Drawing.Point(0, 0))

    let run() = 
        async{
            model.init_simulation()
            let dstep = 1
            step <- 0
            let is_closing = ref false
            while (not !is_closing) && step < 4000 do
                if not !pause || run_step then
                    if (dstep = 1 || (step+1) % dstep <> 0) then
                        model.simulate(dstep) |> ignore

                    status_bar.Invoke(mainwindow_delegate) |> ignore
                    status_bar.Invoke(dialogs_delegate) |> ignore
                    render_cells()
                    step <- step + dstep
                    if run_step then run_step <- false

                do! Async.Sleep(100)

                let! token = Async.CancellationToken
                if token.IsCancellationRequested then
                    is_closing := true
        }

    let disable_close(form: ParamFormBase) =
        form.FormClosing.Add(ParamFormBase.hide_form form)

    let key_press(args: KeyPressEventArgs) =
        if args.KeyChar = 'p' then
            pause := not !pause
        else if args.KeyChar = 's' then
            run_step <- true

    let get_summary(point: Geometry.Point) =
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
        if i >= 0 then
            let cell = cells.[i]
            msg <- sprintf "Location: r=(%.1f, %.1f)\n\
                    Speed: v=(%.1f, %.1f)\n\
                    Repulsive force=(%.1f, %.1f)\n\
                    Friction force=(%.1f, %.1f)\n"
                    cell.Location.x cell.Location.y
                    cell.Velocity.x cell.Velocity.y
                    cell.RepulsiveForce.x cell.RepulsiveForce.y
                    cell.FrictionForce.x cell.FrictionForce.y

        msg <- String.Concat(msg,
            (sprintf "Oxygen: o2(r)=%.1f\n\
                    Cell pack density: density(r)=%.1f"
                    (model.ExtState.O2.GetValue(point))
                    (model.ExtState.CellPackDensity.GetValue(point))))

        msg

    do
        base.ClientSize <- FormDesigner.Scale(base.Size, (0.95, 0.95))
        
        // create the main menu
        base.Menu <- new MainMenu()

        let simulation = new MenuItem("Simulation")
        let rerun_model = new MenuItem("Rerun the simulation")
        rerun_model.Click.Add(fun args -> this.PlotStat())
        simulation.MenuItems.AddRange([|rerun_model|])

        let model_param = new MenuItem("Model parameters")
        let stem_cell_param = new MenuItem("Stem cells")
        stem_cell_param.Click.Add(fun args -> stem_prop_dialog.Visible <- true)
        let nonstem_cell_param = new MenuItem("Non-stem cells")
        nonstem_cell_param.Click.Add(fun args -> nonstem_prop_dialog.Visible <- true)
        let ext_state_param = new MenuItem("The external state")
        ext_state_param.Click.Add(fun args -> extstate_prop_dialog.Visible <- true)
        model_param.MenuItems.AddRange([|stem_cell_param; nonstem_cell_param; ext_state_param|]) |> ignore

        let stat = new MenuItem("Statistics")
        let cell_number = new MenuItem("Cell number")
        cell_number.Click.Add(fun args -> cellnumbers_stat_dialog.Visible <- true)
        let cell_activity = new MenuItem("Cell activity")
        cell_activity.Click.Add(fun args -> activity_stat_dialog.Visible <- true)
        let ext_state = new MenuItem("The external state")
        stat.MenuItems.AddRange([|cell_activity; ext_state|])
        ext_state.Click.Add(fun args -> extstate_dialog.Visible <- true)
        base.Menu.MenuItems.AddRange([|simulation; model_param; stat|]) |> ignore
        
        base.Controls.Add(status_bar)
        this.KeyPress.Add(fun args -> key_press(args))
        let get_summary_curried = fun (p: Drawing.Point) -> get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, cell_tooltip, get_summary_curried, pause))
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, width, height))
        render.Size <- graphics.ClipBounds

        Array.iter disable_close [|cellnumbers_stat_dialog :> ParamFormBase;
            stem_prop_dialog :> ParamFormBase;
            nonstem_prop_dialog :> ParamFormBase;
            extstate_prop_dialog :> ParamFormBase;
            extstate_dialog:> ParamFormBase;
            activity_stat_dialog :> ParamFormBase |]

        Async.Start(run(), cancellation_token.Token)

    override this.OnPaint(args: PaintEventArgs) =
        render_cells()

    member this.UpdateMainWindow() =
        status_bar.Text <- (sprintf "Simulating the model: Step %d" step)

    member this.PlotStat() =
        cellnumbers_stat_dialog.AddPoints(model.GetCellStatistics(Live),
            model.GetCellStatistics(Dividing), model.GetCellStatistics(Dying),
            model.GetCellStatistics(Stem), model.GetCellStatistics(NonStem),
            model.GetCellStatistics(NonStemWithMemory))
        
        cellnumbers_stat_dialog.Refresh()

        extstate_dialog.AddPoints(model.ExtState.O2)
        extstate_dialog.Refresh()

        activity_stat_dialog.StemDivisionStatistics <- model.CellActivityStatistics(CellType.Stem)
        activity_stat_dialog.NonStemDivisionStatistics <- model.CellActivityStatistics(CellType.NonStem)