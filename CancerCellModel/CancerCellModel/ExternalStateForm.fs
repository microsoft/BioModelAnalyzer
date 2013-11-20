module ExternalStateForm

open System
open System.Windows.Forms
open Cell
open ModelParameters
open ParamFormBase
open Geometry
open Render
open NumericComputations

[<AbstractClass>]
type GridFuncForm (title) as this =
    inherit DrawingForm (Visible = false, Width = ModelParameters.GridSize.Width, Height = ModelParameters.GridSize.Height)
    
    let summary_tooltip = new ToolTip()
    let render = new GridFuncRender(base.Graphics)
    [<DefaultValue>] val mutable f: GridFunction1D
    let pause = ref true
    [<DefaultValue>] val mutable f_limits: FloatInterval

    do
        this.Text <- title
        base.Graphics.Clip <- new Drawing.Region(Drawing.Rectangle(0, 0, base.Width, base.Height))
        render.Size <- base.Graphics.ClipBounds
        let get_summary_curried = fun (p: Drawing.Point) -> this.get_summary(Geometry.Point(render.translate_coord(p, false)))
        this.MouseClick.Add(ParamFormBase.show_summary2(this, summary_tooltip, get_summary_curried, pause))

    abstract member plot_func_to_bitmap : unit -> unit

    default this.plot_func_to_bitmap() =
        render.PlotFunc(this.f, this.f_limits)

    abstract member plot_func : unit -> unit
    default this.plot_func() = 
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
                    ((!ext).O2Summary(p))

    override this.plot_func_to_bitmap() =
        base.plot_func_to_bitmap()
        base.Render.DrawGrid2DRegion((!ext).Peripheral, (!ext).O2.Grid)

type DensityForm(ext: ref<ExternalState>) =
    inherit GridFuncForm("Cell packing density")
    let ext = ext
    //[<DefaultValue>] val mutable grad: ref<GridFunctionVector>

    override this.get_summary(p: Geometry.Point) =
        sprintf "%s\n%s" (base.get_summary(p))
                    ((!ext).CellPackingDensitySummary(p))

    override this.plot_func_to_bitmap() =
        let render = base.Render
        render.PlotFunc(this.f, this.f_limits)//, !this.grad)

    (*member this.AddPoints(newf: GridFunction1D, new_flimits: FloatInterval, newgrad: ref<GridFunctionVector>) =
        this.grad <- newgrad
        base.AddPoints(newf, new_flimits)*)