module GlobalStateForm

open System
open System.Windows.Forms
open Cell
open ModelParameters
open ParamFormBase
open Geometry
open Render
open NumericalComputations
open Automata

[<AbstractClass>]
type GridFuncForm (title: string, f: GridFunction1D, f_limits: FloatInterval) as this =
    inherit DrawingForm (Visible = false, Width = ModelParameters.WindowSize.Width, Height = ModelParameters.WindowSize.Height)
    
    let summary_tooltip = new ToolTip()
    let render = new GridFuncRender(base.Graphics)

    do
        this.Text <- title
        base.Graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        render.Size <- base.Graphics.ClipBounds
        let get_summary_curried = fun (p: Drawing.Point) -> this.get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, summary_tooltip, get_summary_curried))

    abstract member plot_func_to_bitmap : unit -> unit

    default this.plot_func_to_bitmap() =
        render.PlotFunc(f, f_limits)

    abstract member plot_func : unit -> unit
    default this.plot_func() = 
        this.plot_func_to_bitmap()
        let graphics = this.CreateGraphics()
        graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        graphics.DrawImage(base.Bitmap, Drawing.Point(0, 0))

    override this.Refresh() =
        this.plot_func()

    override this.get_summary(p: Geometry.Point) =
        let grid = f.Grid
        let (i, j) = grid.PointToIndices(p)
        sprintf "Coordinate: (%.1f, %.1f), (i=%d, j=%d)"
                    p.x p.y i j

    override this.OnPaint(args: PaintEventArgs) =
        this.plot_func()

    member this.GetFValue(p: Geometry.Point) =
        f.GetValue(p)
    
    member this.Render with get() = render

type O2Form(glb: GlobalState, f: GridFunction1D, f_limits: FloatInterval) =
    inherit GridFuncForm("The concentration of O2", f, f_limits)

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    (glb.O2Summary(p))

    override this.plot_func_to_bitmap() =
        base.plot_func_to_bitmap()
        base.Render.DrawGrid2DRegion(glb.Peripheral, glb.O2.Grid)

type GlucoseForm(glb: GlobalState, f: GridFunction1D, f_limits: FloatInterval) =
    inherit GridFuncForm("The concentration of glucose", f, f_limits)

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    (glb.GlucoseSummary(p))

    override this.plot_func_to_bitmap() =
        base.plot_func_to_bitmap()
        base.Render.DrawGrid2DRegion(glb.Peripheral, glb.Glucose.Grid)

type DensityForm(glb: GlobalState, f, f_limits) =
    inherit GridFuncForm("Cell packing density", f, f_limits)

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    (glb.CellPackingDensitySummary(p))

    override this.plot_func_to_bitmap() =
        let render = base.Render
        render.PlotFunc(f, f_limits)
