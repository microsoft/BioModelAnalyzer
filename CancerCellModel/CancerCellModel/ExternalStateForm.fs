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

[<AbstractClass>]
type GridFuncForm (title) as this =
    inherit DrawingForm (Visible = false, Width = ModelParameters.GridSize.Width, Height = ModelParameters.GridSize.Height)
    
    (*let chart = new Chart(Dock=DockStyle.Fill)
    let series_o2 = new Series(ChartType = SeriesChartType.Line, name ="O2")*)
    let summary_tooltip = new ToolTip()
    let render = new GridFuncRender(base.Graphics)
    [<DefaultValue>] val mutable f: GridFunction1D
    let pause = ref true
    [<DefaultValue>] val mutable f_limits: FloatInterval

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
        this.Text <- title
        base.Graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        render.Size <- base.Graphics.ClipBounds
        let get_summary_curried = fun (p: Drawing.Point) -> this.get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, summary_tooltip, get_summary_curried, pause))

    abstract member plot_func_to_bitmap : unit -> unit

    default this.plot_func_to_bitmap() =
        render.PlotFunc(this.f, this.f_limits)

    member this.plot_func() = 
        this.plot_func_to_bitmap()
        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        graphics.DrawImage(base.Bitmap, Drawing.Point(0, 0))

    member this.AddPoints(newf: GridFunction1D, new_flimits: FloatInterval) =
        this.f <- newf
        this.f_limits <- new_flimits
        this.plot_func()

    override this.get_summary(p: Geometry.Point) =
        let grid = this.f.Grid
        let (i, j) = grid.PointToIndices(p)
        sprintf "Coordinate: (%.1f, %.1f), (i=%d, j=%d)"
                    p.x p.y i j

    override this.OnPaint(args: PaintEventArgs) =
        this.plot_func()

    member this.GetFValue(p: Geometry.Point) =
        this.f.GetValue(p)
    
    member this.Render with get() = render

type O2Form(ext: ref<ExternalState>) =
    inherit GridFuncForm("The concentration of O2")
    let ext = ext

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    ((!ext).O2ToStringVerbose(p))

type DensityForm(ext: ref<ExternalState>) =
    inherit GridFuncForm("Cell packing density")
    let ext = ext

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    ((!ext).DensityToStringVerbose(p))

    override this.plot_func_to_bitmap() =
        let grad = (!ext).CellPackDensityGrad
        let render = base.Render
        render.PlotFunc(this.f, this.f_limits, !grad)

    override this.OnPaint(args: PaintEventArgs) =
       this.plot_func()
