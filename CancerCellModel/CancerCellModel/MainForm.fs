module MainForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open StemCellParamForm
open NonStemCellParamForm
open ExternalStateParamForm
open ExternalStateForm
open CellActivityStatForm
open Cell
open ModelParameters
open Model
open ParamFormBase

type CellStatisticsForm ((*cell_stat: CellStatistics, ext_stat: ExtStatistics*)) =
    inherit Form (Visible = false, Width = 1400, Height = 1000)

    let chart = new Chart(Dock=DockStyle.Fill)
    let series_live = new Series(ChartType = SeriesChartType.Line, name="Live cells")
    let series_dividing = new Series(ChartType = SeriesChartType.Line, name="Dividing cells")
    let series_dying = new Series(ChartType = SeriesChartType.Line, name="Dying cells")
    let series_total_dead = new Series(ChartType = SeriesChartType.Line, name="Dead cells (total number)")
    let series_stem = new Series(ChartType = SeriesChartType.Line, name="Stem cells")
    let series_nonstem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells")
    let series_nonstem_withmem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells \"with memory\"")

    let stem_prop_dialog = new StemCellParamForm()
    let nonstem_prop_dialog = new NonStemCellParamForm()
    let extstate_prop_dialog = new ExternalStateParamForm()
    let extstate_dialog = new ExtStatisticsForm()
    let activity_stat_dialog = new CellActivityStatForm()
    let summary_tooltip = new ToolTip()
    let model = new Model()

    let hide_form(form: Form)(args: FormClosingEventArgs) =
        args.Cancel <- true
        form.Visible <- false

    let add_points(xs:seq<float*float>, series: Series) =
        series.Points.Clear()
        for t, n in xs do
            series.Points.AddXY(t, n) |> ignore

    let plot_stat() = 
        model.simulate(1000) |> ignore
        add_points(model.GetCellStatistics(Live), series_live)
        add_points(model.GetCellStatistics(Dividing), series_dividing)
        add_points(model.GetCellStatistics(Dying), series_dying)
        //add_points(model.GetCellStatistics(TotalDead), series_total_dead)
        add_points(model.GetCellStatistics(Stem), series_stem)
        add_points(model.GetCellStatistics(NonStem), series_nonstem)
        add_points(model.GetCellStatistics(NonStemWithMemory), series_nonstem_withmem)
        chart.Refresh()

        extstate_dialog.AddPoints(model.GetO2Statistics())
        extstate_dialog.Refresh()

        activity_stat_dialog.StemDivisionStatistics <- model.CellActivityStatistics(CellType.Stem)
        activity_stat_dialog.NonStemDivisionStatistics <- model.CellActivityStatistics(CellType.NonStem)

    let get_summary(x: float) =
        sprintf "Time: %d\n\
                The number of live cells: %d\n\
                The number of stem cells: %d\n\
                The number of non-stem cells: %d\n\
                The number of non-stem cells with memory: %d\n\
                The number of dividing cells: %d\n\
                The number of dying cells: %d\n\n\
                The amount of oxygen: %d"
                (int (round(x))) (ParamFormBase.get_chart_yvalue(series_live, x))
                (ParamFormBase.get_chart_yvalue(series_stem, x))
                (ParamFormBase.get_chart_yvalue(series_nonstem, x))
                (ParamFormBase.get_chart_yvalue(series_nonstem_withmem, x))
                (ParamFormBase.get_chart_yvalue(series_dividing, x))
                (ParamFormBase.get_chart_yvalue(series_dying, x))
                (extstate_dialog.GetYValue(x))

    do
        base.Controls.Add(chart)
        let chart_area = new ChartArea()
        chart.Titles.Add("The statistics of the number of different kinds of cells") |> ignore
        chart.ChartAreas.Add (chart_area)
        //base.Controls.Add (chart)
        chart.Legends.Add(new Legend())
        chart.Location <- Drawing.Point(30, 30)
        chart.MouseClick.Add(ParamFormBase.show_summary(chart, summary_tooltip, get_summary))
        //chart.MouseMove.Add(fun args -> summary_tooltip.Hide(chart))
        //chart.Size <- Drawing.Size(int (float panel.ClientSize.Width * 0.8), int (float panel.ClientSize.Height * 0.8))
        chart_area.CursorX.IsUserSelectionEnabled <- true
        chart_area.CursorY.IsUserSelectionEnabled <- true

        // create the main chart (with statistics for different kinds of cells)
        chart_area.AxisX.Title <- "Time steps"
        chart_area.AxisY.Title <- "The number of cells"
        chart_area.AxisX.Minimum <- float 0
        chart_area.AxisX.ScaleView.Position <- float 0
        chart_area.AxisX.ScaleView.Size <- float 100
        chart.Series.Add(series_live)
        chart.Series.Add(series_dividing)
        chart.Series.Add(series_dying)
        chart.Series.Add(series_total_dead)
        chart.Series.Add(series_stem)
        chart.Series.Add(series_nonstem)
        chart.Series.Add(series_nonstem_withmem)
        series_live.Color <- Drawing.Color.Red
        series_dividing.Color <- Drawing.Color.DeepPink
        series_dying.Color <- Drawing.Color.DarkViolet
        series_total_dead.Color <- Drawing.Color.Orange
        series_stem.Color <- Drawing.Color.DarkBlue
        series_nonstem.Color <- Drawing.Color.LightGreen
        series_nonstem_withmem.Color <- Drawing.Color.DarkTurquoise

        // create the main menu
        base.Menu <- new MainMenu()

        let rerun_model = new MenuItem("Rerun the simulation")
        rerun_model.Click.Add(fun args -> plot_stat())

        let model_param = new MenuItem("Model parameters")
        let stem_cell_param = new MenuItem("Stem cells")
        stem_cell_param.Click.Add(fun args -> stem_prop_dialog.Visible <- true)
        let nonstem_cell_param = new MenuItem("Non-stem cells")
        nonstem_cell_param.Click.Add(fun args -> nonstem_prop_dialog.Visible <- true)
        let ext_state_param = new MenuItem("The external state")
        ext_state_param.Click.Add(fun args -> extstate_prop_dialog.Visible <- true)
        model_param.MenuItems.AddRange([|stem_cell_param; nonstem_cell_param; ext_state_param|]) |> ignore

        let stat = new MenuItem("Statistics")
        let cell_activity = new MenuItem("Cell activity")
        cell_activity.Click.Add(fun args -> activity_stat_dialog.Visible <- true)
        let ext_state = new MenuItem("The external state")
        stat.MenuItems.AddRange([|cell_activity; ext_state|])
        ext_state.Click.Add(fun args -> extstate_dialog.Visible <- true)
        base.Menu.MenuItems.AddRange([|rerun_model; model_param; stat|]) |> ignore

        stem_prop_dialog.FormClosing.Add(hide_form stem_prop_dialog)
        nonstem_prop_dialog.FormClosing.Add(hide_form nonstem_prop_dialog)
        extstate_prop_dialog.FormClosing.Add(hide_form extstate_prop_dialog)
        extstate_dialog.FormClosing.Add(hide_form extstate_dialog)
        activity_stat_dialog.FormClosing.Add(hide_form activity_stat_dialog)
        plot_stat()

    member this.Clear() =
        series_live.Points.Clear()
        series_dividing.Points.Clear()
        series_dying.Points.Clear()
        series_total_dead.Points.Clear()
        series_stem.Points.Clear()
        series_nonstem.Points.Clear()
        series_nonstem_withmem.Points.Clear()