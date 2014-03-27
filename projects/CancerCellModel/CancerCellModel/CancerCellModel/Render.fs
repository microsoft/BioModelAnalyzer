module Render

open Cell
open System
open System.Drawing
open System.Drawing.Drawing2D
open Geometry
open NumericalComputations
open ModelParameters

type Render (graphics: Graphics)=
    let mutable size = RectangleF()
    member this.Size with get() = size and set(x) = size <- x

    member internal this.color_to_argb(color: Color) =
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

    member private this.choose_color (cell: Cell) = 
        let mutable color = 
                (match cell.Type with
                    | Stem -> Color.Green
                    | NonStem -> Color.Blue
                    | NonStemWithMemory -> Color.GreenYellow)

        if cell.State = PreNecrosisState then color <- Color.Brown
        else if cell.State = NecrosisState then color <- Color.Black
        else if cell.CellCycleStage = G0 then color <- Color.Yellow
        else if cell.State = DeathByRadiation then color <- Color.Crimson
        color <- Color.FromArgb(125, color)
        color

     member private this.cell_rect(cell: Cell) = 
        let x, y, r = cell.Location.x, cell.Location.y, cell.R
        let left_corner = this.translate_coord(Drawing.Point(int(x-r), int(y-r)))
        let size = Drawing.Size(int(2.*r), int(2.*r))
        let rect = Drawing.Rectangle(left_corner, size)
        rect

    member private this.render_cell(cell: Cell) =
        let color = this.choose_color(cell)
        let brush = new SolidBrush(color)
        let pen = new Pen(color)
        let rect = this.cell_rect(cell)
        graphics.DrawEllipse(pen, rect)
        graphics.FillEllipse(brush, rect)

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

        Color.FromArgb(255, comp, comp, comp)

    member this.DrawArrow(p: Drawing.Point, dir: Vector, scale: float, pen: Pen) =
        if not (dir.IsNull()) then
            pen.StartCap <- LineCap.NoAnchor
            pen.EndCap <- LineCap.ArrowAnchor

            let x1, y1 = p.X, p.Y
            let x2, y2 = p.X + int (Math.Round(dir.x*scale)), p.Y + int (Math.Round(dir.y*scale))
            graphics.DrawLine(pen, x1, y1, x2, y2)
                

    member this.PlotFunc(f: GridFunction1D, f_limits: FloatInterval) =
        graphics.Clear(Color.White)
        let grid = f.Grid
        let pt_size = 2
        let threshold = 0.01*(f_limits.Max-f_limits.Min)
        let pen = new Pen(new SolidBrush(Color.White))
        let mutable rect = Drawing.Rectangle()
        rect.Size <- Drawing.Size(pt_size, pt_size)

        let fvals = f.GetAllValues()

        for i = 0 to grid.YLines - 1 do
            for j = 0 to grid.XLines - 1 do
                let fval = fvals.[i, j]
                // speed up drawing - do not draw points with max value
                // because they are indistiguishable from the background
                if Math.Abs(f_limits.Max-fval) > threshold then
                    pen.Color <- this.ComputeColor(fval, f_limits)
                    rect.Location <- this.translate_coord(grid.IndicesToPoint(i, j).ToDrawingPoint())
                    graphics.DrawRectangle(pen, rect)


    member this.DrawGrid2DRegion(region: Geometry.Grid2DRegion, grid: Grid) =
        let mutable color = Color.LightBlue
        color <- Color.FromArgb(100, color)
        let pen = new Pen(color)
        let brush = new SolidBrush(color)
        let size = Drawing.Size(int grid.Dx, int grid.Dy)
        for pair in region.GetPoints() do
            let i = pair.Key
            let j_interval = pair.Value
            for j = pair.Value.Min to pair.Value.Max do
                let p1 = this.translate_coord(point = grid.IndicesToPoint(i, j).ToDrawingPoint(), model_to_screen=true)
                let rect = Drawing.Rectangle(p1, size)
                graphics.DrawRectangle(pen, rect)
                graphics.FillRectangle(brush, rect)