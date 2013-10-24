module Render

open Cell
open System
open System.Windows.Forms
open System.Windows.Media
open System.Windows.Media.Imaging
open System.Drawing
open ParamFormBase
open MyMath

type Render private ()=
    static let instance = Render()
    [<DefaultValue>] val mutable private graphics: Graphics

    static member Instance() = instance
    member this.Graphics with get() = this.graphics and set(x) = this.graphics <- x

    //definition of different brushes according to component levels and cell markers           
    member private this.choose_color (cell: Cell) = 
        let color = 
                ref (match cell.Type with
                    | Stem -> Color.Green
                    | NonStem -> Color.Blue
                    | NonStemWithMemory -> Color.GreenYellow)

        if cell.State = PreparingToDie then
            color := Color.Black

        let argb = (!color).ToArgb()
        let a, r, g, b = ref(uint32(argb) >>> 24), ((uint32(argb) >>> 16) &&& 0xFFu), ((uint32(argb) >>> 8) &&& 0xFFu), (uint32(argb) &&& 0xFFu)

        let da = ref 0u

        (*match cell.State with
            | Functioning -> ()
            | PreparingToDie -> da := 30u
            | Dead -> raise (InnerError(sprintf "Error: trying to display a dead cell"))*)

        match cell.Action with
            | AsymSelfRenewal| SymSelfRenewal | NonStemDivision -> da := uint32(-30)
            | Death -> da := 60u
            | NoAction -> ()


        a := 125u + !da
        if !a < 10u then a := 10u
        else if !a > 255u then a:= 255u

        color := Color.FromArgb(int(!a), int(r), int(g), int(b))
        !color

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

    member this.translate_coord((*graphics: Graphics,*) point: Point, ?direction: bool) =
        let model_to_screen = defaultArg direction true
        let size = this.graphics.ClipBounds
        if model_to_screen then
            Point(int(size.Width/2.f) + point.X, int(size.Height/2.f) + point.Y)
        else
            Point(point.X - int(size.Width/2.f), point.Y - int(size.Height/2.f))

    member private this.cell_rect((*graphics: Graphics,*) cell: Cell) = 
        let x, y, r = cell.Location.x, cell.Location.y, cell.R
        let left_corner = this.translate_coord((*this.graphics,*) Point(int(x-r), int(y-r)))
        let size = Size(int(2.*r), int(2.*r))
        let rect = Rectangle(left_corner, size)
        rect

    member private this.render_cell((*graphics: Graphics)( *)cell: Cell) =
        let color = this.choose_color(cell)
        let brush = new SolidBrush(color)
        let pen = new Pen(color)
        let rect = this.cell_rect((*graphics, *)cell)
        this.graphics.DrawEllipse(pen, rect)
        this.graphics.FillEllipse(brush, rect)
        (*Seq.concat  [(*Seq.append [cellDrawing] nucleiDrawings*) [cellDrawing];
                            List.map genDrawing [(*1 .. cell.Generation*)] :> seq<_>;
                            ]*)

    member this.RenderCells((*graphics: Graphics,*) cells: Cell[]) =
        this.graphics.Clear(Color.White)
        Array.iter(this.render_cell (*graphics*)) cells