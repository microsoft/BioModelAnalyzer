module MyMath

open System


exception InnerError of string

let pow2 x = x*x : float
let pow3 x = x*x*x: float
let pi = System.Math.PI

let int_pow(x: int, p: int) = 
    let res = ref x
    for j = 1 to p-1 do
        res := !res*x
    !res

let fact(x: int) =
    let mutable f = 1
    for k = 2 to x do
         f <- f*k
    f

module Geometry =
    type Point (xx: float, yy: float) = 
        member this.x = xx
        member this.y = yy

        new() = Point(0., 0.)
        new(p: Drawing.Point) = Point(float p.X, float p.Y)
        //override this.ToString() = "Pt(" + string xx + "," + string yy + ")"
        member this.ToDrawingPoint() = Drawing.Point(int xx, int yy)

    type Vector (x: float, y: float) =
        member this.x = x
        member this.y = y
        new() = Vector(0., 0.)
        new(p: Point) = Vector(p.x, p.y)
        new(center: Point, angle: float, radius: float) =
            let dx = radius*Math.Cos(angle)
            let dy = radius*Math.Sin(angle)
            Vector(center.x + dx, center.y - dy)

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

        member this.Normalise() =
            let len = Math.Sqrt(x*x + y*y)
            Vector(x/len, y/len)

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

    type FloatInterval(min: float, max: float) =
        member this.Min = min
        member this.Max = max
        new() = FloatInterval(Double.MinValue, Double.MaxValue)

    type IntInterval(min: int, max: int) = 
        member this.Min = min
        member this.Max = max
        new() = IntInterval(Int32.MinValue, Int32.MaxValue)

    type Rectangle(left: float, top: float, right: float, bottom: float) =
        member this.Left with get() = left
        member this.Right with get() = right
        member this.Top with get() = top
        member this.Bottom with get() = bottom

        member this.IsPointInside(p: Point) =
            p.x > left && p.x < right &&
                p.y > top && p.y < bottom

        member this.Area() = (right-left)*(bottom-top)

    type Circle(center: Point, r: float) =
        member this.Center with get() = center
        member this.R with get() = r

        member this.IsPointInside(p: Point) =
            let dx = p.x-center.x
            let dy = p.y-center.y
            dx*dx + dy*dy < r*r

    type RightAngledTriangle(p1: Point, p2: Point, p3: Point) =
        member this.P1 with get() = p1
        member this.P2 with get() = p2
        member this.P3 with get() = p3

        member this.Area() =
            let pp1 = p1
            let mutable pp2 = p2
            let mutable pp3 = p3

            // sort points so that pp1.x = pp2.x (then pp3.y = pp1.y or pp3.y = pp2.y)
            if pp2.x <> pp1.x then
                pp2 <- p3; pp3 <- p2

            0.5 * Math.Abs((pp2.y-pp1.y) * (pp3.x - pp2.x))

    type RectangularTrapezoid(p1: Point, p2: Point, p3: Point, p4: Point) =
        member this.P1 with get() = p1
        member this.P2 with get() = p2
        member this.P3 with get() = p3
        member this.P4 with get() = p4

        //member this.Area() =
            

    type CirclePart(circle: Circle, rect: Rectangle) =
        
        let circle_intersects_vline(x: float) =
            let yc = circle.Center.y
            let xc = circle.Center.x
            let r = circle.R

            let mutable solutions = QuadraticEquation(a = 1., b = -2.*yc,
                                                c = yc*yc + (x-xc)*(x-xc) - r*r).Solve() |>
                                    Array.filter(fun (y: float) -> y > rect.Top && y < rect.Bottom)

            let pairing_func = fun (x: float)(y: float) -> [|Point(x, y)|]
            Array.collect (pairing_func(x)) solutions

        let circle_intersects_hline(y: float) =
            let yc = circle.Center.y
            let xc = circle.Center.x
            let r = circle.R

            let mutable solutions = QuadraticEquation(a = 1., b = -2.*xc,
                                                c = xc*xc + (y-yc)*(y-yc) - r*r).Solve() |>
                                    Array.filter(fun (x: float) -> x > rect.Left && x < rect.Right)

            let pairing_func = fun (y: float)(x: float) -> [|Point(x, y)|]
            Array.collect (pairing_func(y)) solutions

        let intersection_points() = 
            Array.concat([circle_intersects_vline(rect.Left); circle_intersects_vline(rect.Right);
                circle_intersects_hline(rect.Top); circle_intersects_hline(rect.Bottom)])

        (*let close_figure(points: Point[]) =
            [||]*)

        (*member this.GetArea() =
            let points = intersection_points()
            let mutable area = 0.

            if points.Length = 0 then
                if mesh_inside_circle() then // the whole mesh is inside circle
                    area <- rect.Area()
                else // the mesh is completely outside circle
                    area <- 0.
            else if points.Length = 2 then
                let mesh_points = close_figure(points)

                if mesh_points.Length = 1 then // approximate the area by a triangle
                    area <- RightAngledTriangle(points.[0], points.[1], mesh_points.[0]).Area()
                else if mesh_points.Length = 2 // approximate the area by a tangular trapezoid*)

        member this.IsEmpty() =
            let points = intersection_points()
            // the intersection is empty if
            // there are no intersection points and the whole mesh is outside the circle
            points.Length = 0 && (not (circle.IsPointInside(Point(rect.Left, rect.Top)) || rect.IsPointInside(circle.Center)))

open Geometry

let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

let uniform_bool(prob: float) =
    let randgen = new Random(seed())
    randgen.NextDouble() < prob

let uniform_int(interval: IntInterval) =
    let randgen = new Random(seed())
    randgen.Next(interval.Min, interval.Max+1)

let uniform_float(interval: FloatInterval) =
    let randgen = new Random(seed())
    interval.Min + randgen.NextDouble() * (interval.Max - interval.Min)

// y(x) = ymin + 1 / (1/ymax + exp((mu - x)/s))
type LogisticFunc(min: Point, max: Point) as this =
    [<DefaultValue>] val mutable s: float
    [<DefaultValue>] val mutable mu: float

    // The function calculates the parameters mu and s from (x1, x2, min, max)
    // Because the logistic function converges at max and min but never reaches it,
    //   we calculate the parameters from the following two points:
    //   (x1, min+p*(max-min)) and (x2, max - (1-p)*(max-min)), where we define p to be 1% or 0.01
    let calc_param() =
    // we substitute x and y to the equation: 1 / (1/max + exp((mu - x)/s)) = y
    // and solve the system of two linear equations with two variables (mu and s)
        let p = 0.01 
        let l1 = Math.Log((1.-p) / (p*(max.y-min.y)))
        let l2 = Math.Log(p / ((1. - p)*(max.y-min.y)))
        this.s <- (max.x-min.x)/(l1-l2)
        this.mu <- this.s * l1 + min.x
    
    member this.Max with get() = max
    member this.Min  with get() = min
    member this.Mu with get() = this.mu
    member this.S with get() = this.s

    member this.Y(x: float) =
        min.y + float 1 / (float 1/(max.y-min.y) + exp((this.mu-x)/this.s))

    do
        calc_param()

// y(x) = ymin + A*exp(B/x)
type ExponentFunc(p1: Point, p2: Point, ymin: float) as this =
    [<DefaultValue>] val mutable x0: float
    [<DefaultValue>] val mutable b: float

    let calc_param() =
        // y(x) = ymin + A*exp(B/x)
        //this.b <- (p1.x*p2.x)/(p1.x - p2.x) * Math.Log(p2.y/(p1.y - ymin + 1.))
        //this.a <- (p1.y - ymin)/(Math.Exp(this.b/p1.x) - 1.)
        //this.a <- ymin - p2.y + Math.Exp(p1.x/p2.x)*(p1.y - ymin)
        //this.b <- p1.x * Math.Log((p1.y - ymin)/this.a + 1.)
        let L1 = Math.Log(p1.y - ymin + 1.)
        let L2 = Math.Log(p2.y - ymin + 1.)
        this.b <- (p1.x-p2.x)*(L2-L1)/(L1*L2)
        this.x0 <- p1.x - this.b/L1

    do
        calc_param()

    member this.P1 with get() = p1
    member this.P2 with get() = p2
    //member this.N with get() = this.n
    member this.X0 with get() = this.x0
    member this.B with get() = this.B
    member this.YMin with get() = ymin

    // the exponent function
    member this.Y(x: float) =
        ymin + Math.Exp(this.b/(x - this.x0))

// y(x) = ymin + A/B^x
// in contrast to ExponentFunc, ShiftExponentFunc can take non-infinite values at x=0
type ShiftExponentFunc(p1: Point, p2: Point, ymin: float) as this =
    [<DefaultValue>] val mutable a: float
    [<DefaultValue>] val mutable b: float

    let calc_param() =
    // the equation to be solved is as follows:
    // ymin + A/B^(x) = y
        this.b <- Math.Pow(((p1.y - ymin)/(p2.y - ymin)), 1./(p2.x - p1.x))
        this.a <- (p1.y - ymin) * Math.Pow(this.b, p1.x)

    do
        calc_param()

    member this.P1 with get() = p1
    member this.P2 with get() = p2
    member this.A with get() = this.A
    member this.B with get() = this.B
    member this.YMin with get() = ymin

    // the exponent function
    member this.Y(x: float) =
        ymin + this.a/Math.Pow(this.b, x)

//type Dimension = X | Y

type Derivative private(k: int, precision: int)  =
    // the equation to calculate higher-order partial derivative is:
    // Sum(m=l to r, m<>0)(a[m]*f[i+m]) - Sum(m=l to r, m<>0)(a[m]*f[i]) = 
    //      Sum(m=l to r, m<>0)(m*a[m])*dx/1! * f'(xi) +
    //      Sum(m=l to r, m<>0)(m^2*a[m])*dx^2/2! * f''(xi) +
    //      Sum(m=l to r, m<>0)(m^3*a[m])*dx^3/3! * f''(xi) + ...
    //
    // to get this equation we approximate the function with the Tailor series
    // f[i+m] = f[i] + (m*dx)/1!*f'(xi) + (m*dx)^2/2!*f''(xi) + (m*dx)^3/3!*f'''(xi) + ...
    // then we multiply each of this equation by a constant am and sum them up
    //
    // a detailed description of the method is given in
    // http://www.rsmas.miami.edu/personal/miskandarani/Courses/MSC321/lectfiniteDifference.pdf
    let matr_size = k+precision-1

    let mutable a = Array.create matr_size 0.
    let mutable b = Array.create matr_size 0.

    // points situated on the left edge of the grid: i+1, ..., i+matr_size
    let l_l = 1
    let mutable a_l = Array.create matr_size 0.
    // points situated on the right edge of the grid: i-matr_size, i-matr_size+1, ..., i-1
    let l_r = -matr_size
    let mutable a_r = Array.create matr_size 0.
    // points situated in the center of the grid: i - matr_size/2, i - matr_size/2 + 1, ..., i-1, i+1, ..., i + matr_size/2
    let l_c = -matr_size/2
    let mutable a_c = Array.create matr_size 0.

    static let derivative_2_1 = Derivative(2, 1)
    static let derivative_2_3 = Derivative(2, 3)
    
    // The coefficient of the k-th derivative is given by bk = Sum(m=l to r, m<>0)(m^k*a[m])
    // To determine the k-th derivative with precision p
    //      (where precision is the order of the truncated part: O(dx^p))
    //      we require that bq = 0 if q<>k and 1 if q=k for all q = 1 .. k+p-1
    //
    //      We neglect the derivatives of the order higher than k+p-1
    //      and in this way we need k+p-1 points
    let compute_coeff(order: int, precision: int) =
        // To compute vector a, we write the equation for bk in the matrix form and solve it      
        let mutable a_coeff_l = Array2D.create matr_size matr_size 0.
        let mutable a_coeff_r = Array2D.create matr_size matr_size 0.   
        let mutable a_coeff_c = Array2D.create matr_size matr_size 0.

        for i = 1 to matr_size do
            for j = 0 to matr_size-1 do
                a_coeff_l.[i, j] <- float (int_pow(l_l+j, i))
                a_coeff_r.[i, j] <- float (int_pow(l_r+j, i))
                a_coeff_c.[i, j] <- float (int_pow(l_c+j, i))

        for j = 0 to matr_size - 1 do
            b.[j] <- if j+1=order then 1. else 0.

        let mutable info = 0
        let mutable rep = new alglib.densesolverreport()
        alglib.rmatrixsolve(a_coeff_l, matr_size, b, ref info, ref rep, ref a_l)
        if info <> 1 then raise(InnerError("error when computing a_l"))
        alglib.rmatrixsolve(a_coeff_r, matr_size, b, ref info, ref rep, ref a_r)
        if info <> 1 then raise(InnerError("error when computing a_r"))
        alglib.rmatrixsolve(a_coeff_c, matr_size, b, ref info, ref rep, ref a_c)
        if info <> 1 then raise(InnerError("error when computing a_c"))

    // Since we computed a[m] in such a way that bq = 0 if q<>k and 1 if q=k for all q = 1 .. k+p-1,
    // we compute the k-th derivative as follows:
    //      fk(xi) = (Sum(m=l to r, m<>0)(a[m]*f[i+m]) - Sum(m=l to r, m<>0)(a[m]*f[i])) * k!/dx^k
    // or   fk(xi) = (Sum(m=l to r, m<>0)(a[m]*(f[i+m] - f[i]))) * k!/dx^k
    let compute_derivative(f: float[,], dimension: int, delta: float[], indices: int[]) =
        let mutable l = 0
        let mutable a = Array.create matr_size 0.
        
        let func_size = f.GetLength(dimension)
        let deriv_index = indices.[dimension]
        let i, j = indices.[0], indices.[1]

        if deriv_index < matr_size/2 then l <- l_l; a <- a_l
        else if deriv_index > func_size-1-(matr_size-matr_size/2) then l <- l_r; a <- a_r
        else l <- l_c; a <- a_c

        let mutable sum = 0.
        for jj = 0 to matr_size-1 do
            if jj = k-1 then ()
            else
                let m = if jj <= k-1 then jj else jj-1
                let fstep =
                    if dimension = 0 then f.[i+(l+m), j] // ddf/ddx
                    else f.[i, j+(l+m)]                  // ddf/ddy

                sum <- sum + a.[m]*(fstep - f.[i, j])

        sum <- sum * float (fact(k)) / Math.Pow(delta.[dimension], float k)
        sum

    // nabla square is the sum of second partial derivatives for each dimension
    member this.NablaSquare(f: float[,], delta: float[]) =
        let nabla_square_arr = Array2D.create (f.GetLength(0)) (f.GetLength(1)) 0.

        for i=0 to f.GetLength(0)-1 do
            for j=0 to f.GetLength(1)-1 do
                nabla_square_arr.[i, j] <- compute_derivative(f, 0, delta, [|i; j|]) + 
                                            compute_derivative(f, 1, delta, [|i; j|])

        nabla_square_arr

    static member Derivative_2_1 with get() = derivative_2_1
    static member Derivative_2_3 with get() = derivative_2_3