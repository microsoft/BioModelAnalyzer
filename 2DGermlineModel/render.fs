module Render
open Cells
//open Locations

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Media.Imaging



/////rendering of a single cell\\\\\
let renderCell (cell:Cell) =    
    let w,h = cell.Size,cell.Size   
    let drawEllipse (w,h) (brush:Brush) (p: Pt) =
        let p = Point(float p.x,float p.y)
        let geom = EllipseGeometry(p,w,h)
        let drawing = GeometryDrawing(brush,null,geom) :> Drawing
        drawing
    
    //definition of different brushes according to component levels and cell markers           
    let brush = if cell.GFP = On then Brushes.LimeGreen
                else if cell.Markers = Mitosis then 
                    if cell.NotchLevel = NotchMax then Brushes.DarkRed
                    else if cell.NotchLevel < NotchMax && cell.NotchLevel > (NotchMax/2.) then Brushes.OrangeRed 
                    else if cell.NotchLevel < (NotchMax/2.) && cell.NotchLevel > 0. then Brushes.Yellow
                    else Brushes.Black
                else if  cell.Markers = DTC then Brushes.Green
                else if cell.Markers = PreMeiosis then Brushes.DarkCyan
                else if cell.Markers = Meiosis then if cell.RasLevel < RasLimit1 then Brushes.DarkMagenta else if cell.RasLevel < RasLimit2 then Brushes.Yellow else Brushes.Magenta
                //BAH commented out as RasLimit1=RasLimit2
                //else if cell.Markers = Meiosis then if cell.RasLevel < RasLimit2 then Brushes.DarkMagenta else Brushes.Magenta
                else if cell.Markers = Dead then Brushes.Black
                else if cell.Markers = Fertilised then Brushes.Beige
                else if cell.Markers = Stopped then Brushes.Transparent
                else Brushes.DarkBlue      
    let cellDrawing = drawEllipse (w,h) brush cell.Location
    
    //drawings of nuclei and generations(not used at the moment)
    let nucleiDrawings = if cell.Markers = Stopped then [|drawEllipse (w/3.0,h/3.0) Brushes.Red cell.Location|] else [|drawEllipse (w/3.0,h/3.0) Brushes.Peru cell.Location|]
    let genDrawing i = 
        let theta = float i / 12.0 * 2.0 * 3.1415
        let p = Point(float cell.Location.x + 1.2 * w * cos theta,float cell.Location.y + 1.2 * h * sin theta)
        let geom = EllipseGeometry(p,w/4.0,h/4.0)
        GeometryDrawing(Brushes.DarkRed,null,geom) :> Drawing

    let drawings = Seq.concat  [Seq.append [cellDrawing] nucleiDrawings;
                                List.map genDrawing [(*1 .. cell.Generation*)] :> seq<_>;
                               ]
    DrawingGroup(Children = DrawingCollection(drawings)) :> Drawing


/////rendering all cells\\\\\
let renderCells (cells:Cell seq) =
    DrawingGroup(Children = DrawingCollection(Seq.map renderCell cells)) :> Drawing


/////rendering the background of the simulation\\\\\
let renderEnv =
    let geom   = RectangleGeometry(Rect(-1.0,-1.0,float (extentX+2.0),float (extentY+2.0)))
    let area   = GeometryDrawing(Brushes.AliceBlue,null,geom) :> Drawing    
    area


/////rendering areas of pathway activity and deathzone\\\\\    
let renderNotch =
    let stop   = gldB - 3.
    let geom   = RectangleGeometry(Rect(3.,7.7,float stop,float 15.))
    let area   = GeometryDrawing(Brushes.RosyBrown,null,geom) :> Drawing    
    area
    
let renderRas =
    let stop   = rasEnd - rasB
    let geom   = RectangleGeometry(Rect(rasB,7.7,float stop,float 15.))
    let area   = GeometryDrawing(Brushes.LightBlue,null,geom) :> Drawing    
    area

let renderDeathZone testborder testEnd =
    let stop   = testEnd - testborder
    let geom   = RectangleGeometry(Rect(testborder,7.7,float stop,float 15.))
    let area   = GeometryDrawing(Brushes.Black,null,geom) :> Drawing    
    area

//let lineDrawing testborder = GeometryDrawing(Brushes.CornflowerBlue,Pen(Brushes.CornflowerBlue,0.7),LineGeometry(Point(testborder,7.),Point(testborder,23.))) :> Drawing    
/////putting all drawings together\\\\\
let renderState cells =    
    let drawingA = renderEnv
    let drawingA2 = renderRas 
    let drawingA3 = renderNotch 
    let drawingA4 = renderDeathZone deathborder deathEnd
    let drawingB = renderCells cells
    //let drawingC = lineDrawing deathborder
    let drawing = DrawingGroup(Children = DrawingCollection([drawingA;drawingA2;drawingA3;drawingA4;drawingB]))    //;drawingCthe first drawings will be painted first!
    drawing.ClipGeometry <- RectangleGeometry(Rect(-10.0,-10.0,155.0,75.0))
    drawing :> Drawing