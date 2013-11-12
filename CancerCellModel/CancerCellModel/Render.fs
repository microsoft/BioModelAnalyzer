module Render

open Cell
open System
open System.Windows.Forms
open System.Windows.Media
open System.Windows.Media.Imaging
open System.Drawing
open System.Drawing.Drawing2D
open ParamFormBase
open MyMath
open Geometry

type Render (graphics: Graphics)=
    let mutable size = RectangleF()
    member this.Size with get() = size and set(x) = size <- x

    member private this.color_to_argb(color: Color) =
        let argb = color.ToArgb()
        let mutable a, r, g, b = uint32(argb) >>> 24, ((uint32(argb) >>> 16) &&& 0xFFu), ((uint32(argb) >>> 8) &&& 0xFFu), (uint32(argb) &&& 0xFFu)
        a, r, g, b

    member this.translate_coord(point: Drawing.Point, ?model_to_screen: bool) =
        let model_to_screen = defaultArg model_to_screen true
        if model_to_screen then
            Drawing.Point(int(size.Width/2.f) + point.X, int(size.Height/2.f) + point.Y)
        else
            Drawing.Point(point.X - int(size.Width/2.f), point.Y - int(size.Height/2.f))

    member this.DrawGrid(grid: Grid) =
        let pen = new Pen(Color.LightGray)
        for i = 0 to grid.YLines-1 do
            graphics.DrawLine(pen, this.translate_coord(grid.IndicesToPoint(i, 0).ToDrawingPoint()),
                                   this.translate_coord(grid.IndicesToPoint(i, grid.XLines).ToDrawingPoint()))

        for j = 0 to grid.XLines-1 do
            graphics.DrawLine(pen, this.translate_coord(grid.IndicesToPoint(0, j).ToDrawingPoint()),
                                   this.translate_coord(grid.IndicesToPoint(grid.YLines, j).ToDrawingPoint()))

type CellRender (graphics: Graphics)=
    inherit Render(graphics)

    //definition of different brushes according to component levels and cell markers
    member private this.color_to_argb(color: Color) =
        let argb = color.ToArgb()
        let mutable a, r, g, b = uint32(argb) >>> 24, ((uint32(argb) >>> 16) &&& 0xFFu), ((uint32(argb) >>> 8) &&& 0xFFu), (uint32(argb) &&& 0xFFu)
        a, r, g, b
    
    member private this.choose_color (cell: Cell) = 
        let mutable color = 
                (match cell.Type with
                    | Stem -> Color.Green
                    | NonStem -> Color.Blue
                    | NonStemWithMemory -> Color.GreenYellow)

        if cell.State = PreparingToNecrosis then color <- Color.DarkGray
        else if cell.State = NecroticDeath then color <- Color.Black

        let mutable a, r, g, b = this.color_to_argb(color)
        let mutable da = 0u

        (*match cell.State with
            | Functioning -> ()
            | PreparingToDie -> da := 30u
            | Dead -> raise (InnerError(sprintf "Error: trying to display a dead cell"))

        match cell.Action with
            | AsymSelfRenewal| SymSelfRenewal | NonStemDivision -> da <- uint32(-50)
            | Necrosis | Apoptosis | NoAction -> ()*)

        a <- 125u + da
        if a < 10u then a <- 10u
        else if a > 255u then a <- 255u

        color <- Color.FromArgb(int(a), int(r), int(g), int(b))
        color

        (*if cell.GFP = On then Brushes.LimeGreen
                else if cell.Markers = Mitosis then 
                    if cell.NotchLevel = NotchMax then Brushes.DarkRed
                    else if cell.NotchLevel < NotchMax && cell.NotchLevel > (NotchMax/2.) then Brushes.OrangeRed 
                    else if cell.NotchLevel < (NotchMax/2.) && cell.NotchLevel > 0. then Brushes.Yellow
                    else Brushes.Black
                else if  cell.Markers = DTC then Brushes.Green
                else if cell.Markers = PreMeiosis then Brushes.DarkCyan
                else if cell.Markers = Meiosis then if cell.RasLevel < RasLimit1 then Brushes.DarkMagenta else if cell.RasLevel < RasLimit2 then Brushes.Yellow else Brushes.Magenta
                else if cell.Markers = Dead then Brushes.Black
                else if cell.Markers = Fertilised then Brushes.Beige
                else if cell.Markers = Stopped then Brushes.Transparent
                else Brushes.DarkBlue*)

    //drawings of nuclei and generations(not used at the moment)
    (*let nucleiDrawings = if cell.Markers = Stopped then [|drawEllipse (w/3.0,h/3.0) Brushes.Red cell.Location|] else
        [|drawEllipse (w/3.0,h/3.0) Brushes.Peru cell.Location|]*)

    member private this.cell_rect(cell: Cell) = 
        let x, y, r = cell.Location.x, cell.Location.y, cell.R
        let left_corner = this.translate_coord(Drawing.Point(int(x-r), int(y-r)))
        let size = Size(int(2.*r), int(2.*r))
        let rect = Drawing.Rectangle(left_corner, size)
        rect

    member private this.render_cell(cell: Cell) =
        let color = this.choose_color(cell)
        let brush = new SolidBrush(color)
        let pen = new Pen(color)
        let rect = this.cell_rect(cell)
        graphics.DrawEllipse(pen, rect)
        graphics.FillEllipse(brush, rect)
        (*Seq.concat  [(*Seq.append [cellDrawing] nucleiDrawings*) [cellDrawing];
                            List.map genDrawing [(*1 .. cell.Generation*)] :> seq<_>;
                            ]*)

    member this.RenderCells(cells: ResizeArray<Cell>) =
        graphics.Clear(Color.White)
        cells.ForEach(Action<Cell>(this.render_cell))


/////////////////// plot a function fun (x, y) -> z ///////////////
type GridFuncRender(graphics: Graphics) =
    inherit Render(graphics)

    member this.ComputeColor(f: float, f_limits: FloatInterval) =
        let f_norm = (f - f_limits.Min) / (f_limits.Max - f_limits.Min)
        let comp_limits = IntInterval(0, 255)
        let comp = comp_limits.Min + int (f_norm * float (comp_limits.Max - comp_limits.Min))

        //let mutable color = Color.Red
        (*color <- *)
        Color.FromArgb(255, comp, comp, comp)
        //color

    member this.DrawArrow(p: Drawing.Point, dir: Vector, size: Drawing.Size, pen: Pen) =
        //let pen1 = new Pen(Color.Black)//, float32 3)
        pen.StartCap <- LineCap.NoAnchor
        pen.EndCap <- LineCap.ArrowAnchor

        let x1, y1 = p.X, p.Y
        let x2, y2 = p.X + int (Math.Round(dir.x*(float size.Width))), p.Y + int (Math.Round(dir.y*(float size.Height)))
        graphics.DrawLine(pen, x1, y1, x2, y2)
                

    member this.PlotFunc(f: GridFunction1D, f_limits: FloatInterval, ?f_grad: GridFunctionND) =
        graphics.Clear(Color.White)
        let grid = f.Grid
        let pt_size = 2

        for i = 0 to grid.YLines - 1 do
            for j = 0 to grid.XLines - 1 do
                let p = grid.IndicesToPoint(i, j)
                let fval = f.GetValue(p)
                // speed up drawing - do not draw points with max value
                // because they are indistiguishable from the background
                if Math.Abs(fval - f_limits.Max) > float_error then
                    let screen_p = this.translate_coord(p.ToDrawingPoint())
                    let pen = new Pen(this.ComputeColor(fval, f_limits))
                    graphics.DrawEllipse(pen, Drawing.Rectangle(screen_p, Drawing.Size(pt_size, pt_size)))
                
        if f_grad <> None then
            let grad = f_grad.Value
            let grid2 = grad.Grid
            for i = 0 to grid2.YLines-1 do
                for j = 0 to grid2.XLines-1 do
                    let p_local = grid2.IndicesToPoint(i, j)
                    let p_global = grad.translate_to_global_coord(p_local)
                    let screen_p = this.translate_coord(p_global.ToDrawingPoint())
                    let pen = new Pen(this.ComputeColor(f.GetValue(p_global), f_limits))
                    this.DrawArrow(screen_p, Vector(grad.GetValue(p_local)), Drawing.Size(int grid.Dx, int grid.Dy), pen)

        //base.DrawGrid(grid)