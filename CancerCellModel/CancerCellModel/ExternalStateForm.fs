module ExternalStateForm

open System
//open System.Windows.Controls
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Cell
open ModelParameters
open StemCellParamForm

type ExtStatisticsForm () =
    inherit Form (Visible = false, Width = 1400, Height = 1000)
    
    let chart = new Chart(Dock=DockStyle.Fill)
    let series_o2 = new Series(ChartType = SeriesChartType.Line, name ="O2")

    do
        let chart_area = new ChartArea()
        chart.ChartAreas.Add (chart_area)
        base.Controls.Add (chart)
        chart.Legends.Add(new Legend())

        // create the main chart (with statistics for different kinds of cells)
        chart_area.AxisX.Title <- "Time steps"
        chart_area.AxisY.Title <- "Amount of oxygen (%)"
        chart_area.CursorX.IsUserSelectionEnabled <- true
        chart.Series.Add (series_o2)
        series_o2.Color <- Drawing.Color.Red

    member this.AddPoints(xs: seq<float*float>(*, stat_type: StatType*)) =
    // Add data to the series in a loop
        series_o2.Points.Clear()
        for t, n in xs do
            series_o2.Points.AddXY(t, n) |> ignore

    override this.Refresh() =
        base.Refresh()
        chart.Refresh()