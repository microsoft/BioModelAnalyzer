module MainForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open StemCellParamForm
open NonStemCellParamForm
open ExternalStateForm
open Cell
open ModelParameters

type StatType = Live | Dead | Stem | NonStem | NonStemWithMemory

type CellStatisticsForm ((*cell_stat: CellStatistics, ext_stat: ExtStatistics*)) =
    inherit Form (Visible = false, Width = 1400, Height = 1000)

    let chart = new Chart(Dock=DockStyle.Fill)
    let series_live = new Series(ChartType = SeriesChartType.Line, name="Live cells")
    let series_dead = new Series(ChartType = SeriesChartType.Line, name="Dead cells")
    let series_stem = new Series(ChartType = SeriesChartType.Line, name="Stem cells")
    let series_non_stem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells")
    let series_nonstem_withmem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells \"with memory\"")

    let stem_prop_dialog = new StemCellParamForm()
    let nonstem_prop_dialog = new NonStemCellParamForm()
    let extstate_prop_dialog = new ExtStateParamForm()
    let ext_state_dialog = new ExtStatisticsForm()
    let mutable closed = false

    do
        base.Controls.Add(chart)
        let chart_area = new ChartArea()
        chart.Titles.Add("The statistics of the number of different kinds of cells") |> ignore
        chart.ChartAreas.Add (chart_area)
        //base.Controls.Add (chart)
        chart.Legends.Add(new Legend())
        chart.Location <- Drawing.Point(30, 30)
        //chart.Size <- Drawing.Size(int (float panel.ClientSize.Width * 0.8), int (float panel.ClientSize.Height * 0.8))
        chart_area.CursorX.IsUserSelectionEnabled <- true
        chart_area.CursorY.IsUserSelectionEnabled <- true

        // create the main chart (with statistics for different kinds of cells)
        chart_area.AxisX.Title <- "Time steps"
        chart_area.AxisY.Title <- "The number of cells"
        chart_area.AxisX.Minimum <- float 0
        chart_area.AxisX.ScaleView.Position <- float 0
        chart_area.AxisX.ScaleView.Size <- float 100
        chart.Series.Add (series_live)
        chart.Series.Add (series_dead)
        chart.Series.Add (series_stem)
        chart.Series.Add (series_non_stem)
        chart.Series.Add(series_nonstem_withmem)
        series_live.Color <- Drawing.Color.Red
        series_dead.Color <- Drawing.Color.Orange
        series_stem.Color <- Drawing.Color.DarkBlue
        series_non_stem.Color <- Drawing.Color.LightGreen
        series_nonstem_withmem.Color <- Drawing.Color.Chocolate

        // create the main menu
        base.Menu <- new MainMenu()
        let prob_func = new MenuItem("Probability functions")
        base.Menu.MenuItems.Add(prob_func) |> ignore
        let stem_cells = new MenuItem("Stem cells")
        prob_func.MenuItems.Add(stem_cells) |> ignore
        let nonstem_cells = new MenuItem("Non-stem cells")
        prob_func.MenuItems.Add(nonstem_cells) |> ignore
        let ext_state = new MenuItem("The external state")
        base.Menu.MenuItems.Add(ext_state) |> ignore

        stem_cells.Click.Add(fun args -> stem_prop_dialog.Visible <- true)
        nonstem_cells.Click.Add(fun args -> nonstem_prop_dialog.Visible <- true)
        ext_state.Click.Add(fun args -> ext_state_dialog.Visible <- true)

    member this.add_points(xs:seq<float*float>, series: Series) =
        series.Points.Clear()
        for t, n in xs do
            series.Points.AddXY(t, n) |> ignore

    member this.Chart with get() = chart
    member this.ExtStateForm with get() = ext_state_dialog

    member this.AddPoints(xs: seq<float*float>, stat_type: StatType) =
    // Add data to the series in a loop
        this.add_points(xs, 
           (match stat_type with
            | StatType.Live -> series_live
            | StatType.Dead -> series_dead
            | StatType.Stem -> series_stem
            | StatType.NonStem -> series_non_stem
            | StatType.NonStemWithMemory -> series_nonstem_withmem))

    member this.Clear() =
        series_live.Points.Clear()
        series_dead.Points.Clear()
        series_stem.Points.Clear()
        series_non_stem.Points.Clear()
        series_nonstem_withmem.Points.Clear()