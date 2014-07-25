module Geometry

open System
open System.Collections.Generic

exception InnerError of string
let float_error = 1.e-5

type FloatInterval(min: float, max: float) =
    member this.Min = min
    member this.Max = max
    new() = FloatInterval(Double.MinValue, Double.MaxValue)

type IntInterval(min: int, max: int) = 
    member this.Min = min
    member this.Max = max
    new() = IntInterval(Int32.MinValue, Int32.MaxValue)

type Point (x: float, y: float) = 
    member this.x = x
    member this.y = y

    new() = Point(0., 0.)
    new(p: Drawing.Point) = Point(float p.X, float p.Y)
    member this.ToDrawingPoint() = Drawing.Point(int x, int y)

    interface IComparable with
        member this.CompareTo(o: Object) =
            match o with
            | :? Point as p ->
                if Math.Abs(x - p.x) < float_error && Math.Abs(y - p.y) < float_error then 0
                else 1
            | _ -> raise(InnerError("Geometry.Point can not be compared to objects of other type"))

    override this.Equals(o: Object) = (this :> IComparable).Equals(o)
    override this.GetHashCode() = (this :> IComparable).GetHashCode()

type GridPoint(i: int, j: int) = 
    member this.I with get() = i
    member this.J with get() = j

type Vector (x: float, y: float) =
    member this.x = x
    member this.y = y
    new() = Vector(0., 0.)
    new(p: Point) = Vector(p.x, p.y)
    
    static member FromAngleAndRadius(angle: float, radius: float) =
        let dx = radius*Math.Cos(angle)
        let dy = radius*Math.Sin(angle)
        Vector(dx, -dy)
        
    new(coord: float[]) =
        if coord.Length <> 2 then raise(InnerError("Vector initialisation: invalid length of the array of coordinates"))
        Vector(coord.[1], coord.[0])

    static member (*) (k: float, v: Vector) =
        Vector(k*v.x, k*v.y)

    static member (*) (v: Vector, k:float) =
        Vector.op_Multiply(k, v)

    static member (/) (v: Vector, k: float) =
        Vector(v.x/k, v.y/k)

    static member (+) (v1: Vector, v2: Vector) =
        Vector(v1.x+v2.x, v1.y+v2.y)

    static member (-) (v1: Vector, v2: Vector) =
        Vector(v1.x-v2.x, v1.y-v2.y)

    static member (~-) (v1: Vector) =
        Vector(-v1.x, -v1.y)

    member this.ToPoint() =
        Point(x, y)

    member this.Length() =
        Math.Sqrt(x*x + y*y)

    member this.Normalise() =
        let len = this.Length()
        if len > float_error then
            Vector(x/len, y/len)
        else
            this

    member this.Perpendicular() =
        let mutable v = Vector()
        if Math.Abs(y) > float_error then
            v <- Vector(1., -x/y)
        else
            v <- Vector(0., 1.)
        v

    member this.IsNull() =
        this.Length() < float_error

// a convex polygon
type Grid2DRegion() =
    // the vertices of the polygon stored as a collection of
    // the horizontal intervals on the grid enclosed in the polygon
    //
    // storage format: the key is the row number in the grid
    // the value is interval of the the column numbers in the grid
    let points = new Dictionary<int, IntInterval>()


    // add a horizontal interval on the grid
    member this.AddSegment(i: int, j_interval: IntInterval) =
        let old_j_interval = ref (IntInterval())

        if not (points.TryGetValue(i, old_j_interval)) then
            points.Add(i, j_interval)
        else if j_interval.Min < (!old_j_interval).Min ||
                j_interval.Max > (!old_j_interval).Max then

            points.Remove(i) |> ignore
            points.Add(i, IntInterval(min j_interval.Min (!old_j_interval).Min,
                                        max j_interval.Max (!old_j_interval).Max))

    member this.IsPointStrictlyInside(i: int, j: int) =
        let j_interval = ref (IntInterval())
        points.TryGetValue(i, j_interval) &&
            j > (!j_interval).Min && j < (!j_interval).Max

    member this.IsPointInsideOrOnTheBorder(i: int, j: int) =
        let j_interval = ref (IntInterval())
        points.TryGetValue(i, j_interval) &&
            j >= (!j_interval).Min && j <= (!j_interval).Max

    member this.Clear() =
        points.Clear()

    member this.GetPoints() =
        points

let distance(p1: Point, p2: Point) =
    let dx = p1.x - p2.x
    let dy = p1.y - p2.y
    Math.Sqrt(dx*dx + dy*dy)

type QuadraticEquation(a: float, b: float, c: float) =
    member this.Solve() =
        let D = b*b - 4.*a*c
        if D < 0. then
            [||]
        else if D = 0. then
            let x = -b / (2.*a)
            [|x|]
        else
            let sqrtD = Math.Sqrt(D)
            let x1 = (-b + sqrtD) / (2.*a)
            let x2 = (-b - sqrtD) / (2.*a)
            [| x1; x2 |]

type Rectangle(left: float, top: float, right: float, bottom: float) =
    member this.Left with get() = left
    member this.Right with get() = right
    member this.Top with get() = top
    member this.Bottom with get() = bottom
    member this.Width with get() = right - left
    member this.Height with get() = bottom - top
    member this.Center with get() = Point(x = left + this.Width/2., y = top + this.Height/2.)

    member this.IsPointInside(p: Point) =
        p.x > left && p.x < right &&
            p.y > top && p.y < bottom

    member this.Area() = (right-left)*(bottom-top)
    new () = Rectangle(0., 0., 0., 0.)

type Circle(center: Point, r: float) =
    member this.Center with get() = center
    member this.R with get() = r

    member this.IsPointInside(p: Point) =
        let dx = p.x-center.x
        let dy = p.y-center.y
        dx*dx + dy*dy < r*r
    

// a part of the circle formed by the intersection of a circle with a rectangle
type CirclePart(circle: Circle, rect: Rectangle) =
  
    let circle_intersects_vline(x: float) =
        let yc = circle.Center.y
        let xc = circle.Center.x
        let r = circle.R

        let mutable solutions = QuadraticEquation(a = 1., b = -2.*yc,
                                            c = yc*yc + (x-xc)*(x-xc) - r*r).Solve()

        let pairing_func = fun (x: float)(y: float) -> [|Point(x, y)|]
        
        solutions |>
        Array.filter(fun (y: float) -> y > rect.Top && y < rect.Bottom) |>
        Array.collect (pairing_func(x))
        

    let circle_intersects_hline(y: float) =
        let yc = circle.Center.y
        let xc = circle.Center.x
        let r = circle.R

        let mutable solutions = QuadraticEquation(a = 1., b = -2.*xc,
                                            c = xc*xc + (y-yc)*(y-yc) - r*r).Solve()
        
        let pairing_func = fun (y: float)(x: float) -> [|Point(x, y)|]
                                            
        solutions |>
        Array.filter(fun (x: float) -> x > rect.Left && x < rect.Right) |>
        Array.collect (pairing_func(y))

    let intersection_points() = 
        Array.concat([circle_intersects_vline(rect.Left); circle_intersects_vline(rect.Right);
            circle_intersects_hline(rect.Top); circle_intersects_hline(rect.Bottom)])

    member this.NonEmpty() =

        // get the intersection points of the circle and the rectangle
        let points = intersection_points()

        // the intersection is not empty if
        // - there is more than zero intersection point
        // - or the whole mesh is inside the circle
        // - or the whole circle is inside the mesh
        points.Length > 0 || circle.IsPointInside(Point(rect.Left, rect.Top)) || rect.IsPointInside(circle.Center)

    static member IntersectionNotTriviallyEmpty(c: Circle, rect: Rectangle) =
        rect.Left < c.Center.x+c.R && rect.Right > c.Center.x-c.R &&
            rect.Top < c.Center.y+c.R && rect.Bottom > c.Center.y-c.R

type Size(width: float, height: float) =
    member this.Width with get() = width
    member this.Height with get() = height

    interface IComparable with
        member this.CompareTo(o: Object) =
            match o with
            | :? Size as size ->
                if width > size.Width && height > size.Height then 1
                else if Math.Abs(width - size.Width) < float_error && Math.Abs(height - size.Height) < float_error then 0
                else -1
            | _ -> raise(InnerError("Geometry.Size can not be compared to objects of other type"))

    override this.Equals(o: Object) = (this :> IComparable).Equals(o)
    override this.GetHashCode() = (this :> IComparable).GetHashCode()