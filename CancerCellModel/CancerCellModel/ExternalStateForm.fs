module ExternalStateForm

open System
//open System.Windows.Controls
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Cell
open ModelParameters
open StemCellParamForm
open ParamFormBase
open MyMath
open Geometry
open Render

type ExternalStateForm () as this =
    inherit ParamFormBase (Visible = false, Width = 1400, Height = 1000)
    
    (*let chart = new Chart(Dock=DockStyle.Fill)
    let series_o2 = new Series(ChartType = SeriesChartType.Line, name ="O2")*)
    let summary_tooltip = new ToolTip()
    let width, height = this.ClientSize.Width, this.ClientSize.Height
    let bitmap = new System.Drawing.Bitmap(width, height)
    let graphics = System.Drawing.Graphics.FromImage(bitmap)
    let render = new GridFuncRender(graphics)
    let mutable f = GridFunction(ModelParameters.GridParam, ExternalState.O2Limits)
    let pause = ref true

    let plot_func(f: GridFunction) = 
        render.PlotFunc(f)

        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, width, height))
        graphics.DrawImage(bitmap, Drawing.Point(0, 0))

    let get_summary(p: Geometry.Point) =
        sprintf "Coordinate: (%.1f, %.1f)\n\
                    The amount of oxygen:%.1f%%"
                    p.x p.y (this.GetFValue(p.x, p.y))

    do
        
        (*let chart_area = new ChartArea()
        chart.ChartAreas.Add (chart_area)
        base.Controls.Add (chart)
        chart.Legends.Add(new Legend())

        // create the main chart (with statistics for different kinds of cells)
        chart_area.AxisX.Title <- "Time steps"
        chart_area.AxisY.Title <- "Amount of oxygen (%)"
        chart_area.CursorX.IsUserSelectionEnabled <- true
        chart_area.AxisX.Minimum <- float 0
        chart.Series.Add (series_o2)
        chart.MouseClick.Add(ParamFormBase.show_summary(chart, summary_tooltip, get_summary))
        series_o2.Color <- Drawing.Color.Red*)
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, width, height))
        render.Size <- graphics.ClipBounds
        let get_summary_curried = fun (p: Drawing.Point) -> get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, summary_tooltip, get_summary_curried, pause))

    member this.AddPoints(newf: GridFunction) =
    // Add data to the series in a loop
        (*series_o2.Points.Clear()
        for t, n in xs do
            series_o2.Points.AddXY(t, n) |> ignore*)
        f <- newf
        plot_func(f)

    override this.OnPaint(args: PaintEventArgs) =
        plot_func(f)

    member this.GetFValue(x: float, y: float) =
        //ParamFormBase.get_chart_yvalue(series_o2, x)
        f.GetValue(Point(x, y))

    override this.Refresh() =
        base.Refresh()
        //chart.Refresh()