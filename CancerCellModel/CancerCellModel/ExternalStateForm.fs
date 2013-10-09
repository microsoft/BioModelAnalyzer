module ExternalStateForm

open System
//open System.Windows.Controls
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Cell
open ModelParameters
open StemCellParamForm
open ParamFormBase

type ExtStatisticsForm () as this =
    inherit Form (Visible = false, Width = 1400, Height = 1000)
    
    let chart = new Chart(Dock=DockStyle.Fill)
    let series_o2 = new Series(ChartType = SeriesChartType.Line, name ="O2")
    let summary_tooltip = new ToolTip()

    let show_summary(args: MouseEventArgs) =
        if args.Button = MouseButtons.Right then
            let x = (chart.ChartAreas.Item(0).AxisX.PixelPositionToValue(float args.X))
            let message = (sprintf "Time: %d\n\
                                   The amount of oxygen:%.1f%%"
                                   (int (round(x))) (this.GetYValue(x)))

            summary_tooltip.Show(message, chart, Drawing.Point(args.X + 15, args.Y - 15), 20000)
        else if args.Button =  MouseButtons.Left then
            summary_tooltip.Hide(chart)


    do
        let chart_area = new ChartArea()
        chart.ChartAreas.Add (chart_area)
        base.Controls.Add (chart)
        chart.Legends.Add(new Legend())

        // create the main chart (with statistics for different kinds of cells)
        chart_area.AxisX.Title <- "Time steps"
        chart_area.AxisY.Title <- "Amount of oxygen (%)"
        chart_area.CursorX.IsUserSelectionEnabled <- true
        chart_area.AxisX.Minimum <- float 0
        chart.Series.Add (series_o2)
        chart.MouseClick.Add(show_summary)
        series_o2.Color <- Drawing.Color.Red

    member this.AddPoints(xs: seq<float*float>(*, stat_type: StatType*)) =
    // Add data to the series in a loop
        series_o2.Points.Clear()
        for t, n in xs do
            series_o2.Points.AddXY(t, n) |> ignore

    member this.GetYValue(x: float) =
        ParamFormBase.get_chart_yvalue(series_o2, x)

    override this.Refresh() =
        base.Refresh()
        chart.Refresh()