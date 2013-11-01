module CellNumbersStatForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Cell
open ParamFormBase
open ExternalStateForm
open Model

type CellNumbersStatForm(extstate_dialog: ExternalStateForm) =
    inherit ParamFormBase (Width=1200, Height = 900)

    let chart = new Chart(Dock=DockStyle.Fill)
    let series_live = new Series(ChartType = SeriesChartType.Line, name="Live cells")
    let series_dividing = new Series(ChartType = SeriesChartType.Line, name="Dividing cells")
    let series_dying = new Series(ChartType = SeriesChartType.Line, name="Dying cells")
    let series_total_dead = new Series(ChartType = SeriesChartType.Line, name="Dead cells (total number)")
    let series_stem = new Series(ChartType = SeriesChartType.Line, name="Stem cells")
    let series_nonstem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells")
    let series_nonstem_withmem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells \"with memory\"")
    let summary_tooltip = new ToolTip()

    let add_points(xs:seq<float*float>, series: Series) =
        series.Points.Clear()
        for t, n in xs do
            series.Points.AddXY(t, n) |> ignore

    let get_summary(x: float) =
        sprintf "Time: %d\n\
                The number of live cells: %d\n\
                The number of stem cells: %d\n\
                The number of non-stem cells: %d\n\
                The number of non-stem cells with memory: %d\n\
                The number of dividing cells: %d\n\
                The number of dying cells: %d" //\n\n\
                //The amount of oxygen per cell: %.1f%%"
                (int (round(x)))
                (int (ParamFormBase.get_chart_yvalue(series_live, x)))
                (int (ParamFormBase.get_chart_yvalue(series_stem, x)))
                (int (ParamFormBase.get_chart_yvalue(series_nonstem, x)))
                (int (ParamFormBase.get_chart_yvalue(series_nonstem_withmem, x)))
                (int (ParamFormBase.get_chart_yvalue(series_dividing, x)))
                (int (ParamFormBase.get_chart_yvalue(series_dying, x)))
                //(extstate_dialog.GetYValue(x))

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

    member this.AddPoints(live: StatSequence, dividing: StatSequence,
                            dying: StatSequence, stem: StatSequence,
                            nonstem: StatSequence, nonstem_withmem: StatSequence) =
        add_points(live, series_live)
        add_points(dividing, series_dividing)
        add_points(dying, series_dying)
        //add_points(model.GetCellStatistics(TotalDead), series_total_dead)
        add_points(stem, series_stem)
        add_points(nonstem, series_nonstem)
        add_points(nonstem_withmem, series_nonstem_withmem)
        chart.Refresh()

    member this.Clear() =
        series_live.Points.Clear()
        series_dividing.Points.Clear()
        series_dying.Points.Clear()
        series_total_dead.Points.Clear()
        series_stem.Points.Clear()
        series_nonstem.Points.Clear()
        series_nonstem_withmem.Points.Clear()

    override this.Refresh() =
        base.Refresh()
        chart.Refresh()