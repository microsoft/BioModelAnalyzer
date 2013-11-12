module MyMath

open System
open System.Threading

exception InnerError of string

let pow2 x = x*x : float
let pow3 x = x*x*x: float
let pi = System.Math.PI
let float_error = 1.e-5

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

type FloatInterval(min: float, max: float) =
    member this.Min = min
    member this.Max = max
    new() = FloatInterval(Double.MinValue, Double.MaxValue)

type IntInterval(min: int, max: int) = 
    member this.Min = min
    member this.Max = max
    new() = IntInterval(Int32.MinValue, Int32.MaxValue)


module Geometry =
    type Point (x: float, y: float) = 
        member this.x = x
        member this.y = y

        new() = Point(0., 0.)
        new(p: Drawing.Point) = Point(float p.X, float p.Y)
        //override this.ToString() = "Pt(" + string xx + "," + string yy + ")"
        member this.ToDrawingPoint() = Drawing.Point(int x, int y)

    type Vector (x: float, y: float) =
        member this.x = x
        member this.y = y
        new() = Vector(0., 0.)
        new(p: Point) = Vector(p.x, p.y)
        new(center: Point, angle: float, radius: float) =
            let dx = radius*Math.Cos(angle)
            let dy = radius*Math.Sin(angle)
            Vector(center.x + dx, center.y - dy)
        
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

        member this.IsEmpty() =
            let points = intersection_points()
            // the intersection is empty if
            // there are no intersection points and the whole mesh is outside the circle
            points.Length = 0 && (not (circle.IsPointInside(Point(rect.Left, rect.Top)) || rect.IsPointInside(circle.Center)))

        static member IntersectionTriviallyEmpty(c: Circle, rect: Rectangle) =
            rect.Left > c.Center.x+c.R || rect.Right < c.Center.x-c.R ||
                rect.Top > c.Center.y+c.R || rect.Bottom < c.Center.y-c.R

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

// (Generalised) logistic function is defined as
//      y(x) = ymin + 1 / (1/ymax + exp((mu - x)/s))
//
// Note:
// 1) There is no biological reason to choose this function, but its shape
// and its limitedness between min and max are convenient for our purposes.
//
// 2) To avoid confusion: the function has nothing to do with probability distribution.
// f(x) not desribe the probability of taking value x but rather the probability
// of some other event (cell division) (where x is e.g. the amount of O2)
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

// y(x) = ymin + exp(B/(x-x0))-1
type ExponentFunc(p1: Point, p2: Point, ymin: float) as this =
    [<DefaultValue>] val mutable x0: float
    [<DefaultValue>] val mutable b: float

    let calc_param() =
        // y(x) = ymin + exp(B/(x-x0))
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
        ymin + Math.Exp(this.b/(x - this.x0))-1.

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

type Derivative private(k: int, p: int)  =
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
    let matr_size = k+p-1

    let mutable a = Array.create matr_size 0.
    let mutable b = Array.create matr_size 0.

    // points situated on the left edge of the grid: i+1, ..., i+matr_size
    let l_l = 1
    let mutable a_l = ref (Array.create matr_size 0.)
    // points situated on the right edge of the grid: i-matr_size, i-matr_size+1, ..., i-1
    let l_r = -matr_size
    let mutable a_r = ref (Array.create matr_size 0.)
    // points situated in the center of the grid: i - matr_size/2, i - matr_size/2 + 1, ..., i-1, i+1, ..., i + matr_size/2
    let l_c = -matr_size/2
    let mutable a_c = ref (Array.create matr_size 0.)

    static let derivative_1_1 = Derivative(1, 1)
    static let derivative_1_2 = Derivative(1, 2)
    static let derivative_1_3 = Derivative(1, 3)
    static let derivative_1_4 = Derivative(1, 4)
    static let derivative_2_1 = Derivative(2, 1)
    static let derivative_2_2 = Derivative(2, 2)
    static let derivative_2_3 = Derivative(2, 3)
    static let derivative_2_4 = Derivative(2, 4)
    
    
    // The coefficient of the k-th derivative is given by bk = Sum(m=l to r, m<>0)(m^k*a[m])
    // To determine the k-th derivative with precision p
    //      (where precision is the order of the truncated part: O(dx^p))
    //      we require that bq = 0 if q<>k and 1 if q=k for all q = 1 .. k+p-1
    //
    //      We neglect the derivatives of the order higher than k+p-1
    //      and in this way we need k+p-1 points
    let compute_coeff(k: int, p: int) =
        // To compute vector a, we write the equation for bk in the matrix form and solve it      
        let mutable a_coeff_l = Array2D.create matr_size matr_size 0.
        let mutable a_coeff_r = Array2D.create matr_size matr_size 0.   
        let mutable a_coeff_c = Array2D.create matr_size matr_size 0.

        for i = 0 to matr_size-1 do
            for j = 0 to matr_size-1 do
                a_coeff_l.[i, j] <- float (int_pow(l_l+j, i+1))
                a_coeff_r.[i, j] <- float (int_pow(l_r+j, i+1))
                let mutable ind_c = l_c+j
                if ind_c >= 0 then ind_c <- ind_c + 1
                a_coeff_c.[i, j] <- float (int_pow(ind_c, i+1))

        for j = 0 to matr_size - 1 do
            b.[j] <- if j+1=k then 1. else 0.

        let mutable info = ref 0
        let mutable rep = ref (new alglib.densesolverreport())
        alglib.rmatrixsolve(a_coeff_l, matr_size, b, info, rep, a_l)
        if !info <> 1 then raise(InnerError("error when computing a_l"))
        alglib.rmatrixsolve(a_coeff_r, matr_size, b, info, rep, a_r)
        if !info <> 1 then raise(InnerError("error when computing a_r"))
        alglib.rmatrixsolve(a_coeff_c, matr_size, b, info, rep, a_c)
        if !info <> 1 then raise(InnerError("error when computing a_c"))

    // Since we computed a[m] in such a way that bq = 0 if q<>k and 1 if q=k for all q = 1 .. k+p-1,
    // we compute the k-th derivative as follows:
    //      fk(xi) = (Sum(m=l to r, m<>0)(a[m]*f[i+m]) - Sum(m=l to r, m<>0)(a[m]*f[i])) * k!/dx^k
    // or   fk(xi) = (Sum(m=l to r, m<>0)(a[m]*(f[i+m] - f[i]))) * k!/dx^k
    let compute_derivative(f: float[,], dimension: int, delta: float[], indices: int[]) =
        let mutable l = 0
        let mutable a = ref null
        
        let func_size = f.GetLength(dimension)
        let deriv_index = indices.[dimension]
        let i, j = indices.[0], indices.[1]

        if deriv_index < matr_size/2 then l <- l_l; a <- a_l
        else if deriv_index > func_size-1-(matr_size-matr_size/2) then l <- l_r; a <- a_r
        else l <- l_c; a <- a_c

        let mutable sum = 0.
        for jj = 0 to matr_size-1 do
            let m = if jj < -l then jj else jj+1
            let fstep =
                if dimension = 0 then f.[i+(l+m), j] // ddf/ddx
                else f.[i, j+(l+m)]                  // ddf/ddy

            sum <- sum + (!a).[jj]*(fstep - f.[i, j])

        sum <- sum * float (fact(k)) / Math.Pow(delta.[dimension], float k)
        sum

    do
        compute_coeff(k, p)

    // nabla square is the sum of second partial derivatives for each dimension
    member this.NablaSquare(nabla_square_arr: float[,], f: float[,], delta: float[]) =
        if k <> 2 then raise(InnerError("The object was initialised with k!=2 (where k is the order of derivative)"))
        //let nabla_square_arr = Array2D.create (f.GetLength(0)) (f.GetLength(1)) 0.

        for i=0 to f.GetLength(0)-1 do
            for j=0 to f.GetLength(1)-1 do
                //if j >= 42 && j <= 44 && i >= 57 && i <= 59 then
                    //()

                nabla_square_arr.[i, j] <- compute_derivative(f, 1, delta, [|i; j|]) + 
                                            compute_derivative(f, 0, delta, [|i; j|])

    // gradient is a vector with components equal to first derivatives in the corresponding dimension (multiplied by minus one)
    member this.Gradient(f: float[,], delta: float[], i: int, j: int) =
        if k <> 1 then raise(InnerError("The object was initialised with k!=1 (where k is the order of derivative)"))
        
        if i < 0 || i >= f.GetLength(0) ||
            j < 0 || j >= f.GetLength(1) then
                raise(InnerError("i or j is outside the size of f"))

        let df_dx = compute_derivative(f, 1, delta, [|i; j|])
        let df_dy = compute_derivative(f, 0, delta, [|i; j|])
        (Vector(df_dx, df_dy).Normalise())

    static member Derivative_1_1 with get() = derivative_1_1
    static member Derivative_1_2 with get() = derivative_1_2
    static member Derivative_1_3 with get() = derivative_1_3
    static member Derivative_1_4 with get() = derivative_1_4
    static member Derivative_2_1 with get() = derivative_2_1
    static member Derivative_2_2 with get() = derivative_2_2
    static member Derivative_2_3 with get() = derivative_2_3
    static member Derivative_2_4 with get() = derivative_2_4

type Grid(width: float, height: float, dx: float, dy: float) = 
    let x_lines = int (Math.Floor(width / dx)) + 1
    let y_lines = int (Math.Floor(height / dy)) + 1
    member this.Width with get() = width
    member this.Height with get() = height
    member this.Dx with get() = dx
    member this.Dy with get() = dy
    member this.XLines with get() = x_lines
    member this.YLines with get() = y_lines

    member this.IndicesToPoint(i: int, j: int) = Point(-width/2. + (float j)*dx, -height/2. + (float i)*dy)
    
    member this.PointToIndices(p: Point) =
        let i = int (Math.Round((p.y + height/2.) / dy))
        let j = int (Math.Round((p.x + width/2.) / dx))
        i, j
    
    member this.CenteredRect(i: int, j: int) =
        let p = this.IndicesToPoint(i, j)
        Rectangle(left = p.x-dx/2., right = p.x+dx/2., top = p.y-dy/2., bottom = p.y+dy/2.)

    member this.SurroundingMeshVertices(p: Point) = 
        let i = (p.y + height/2.) / dy
        let j = (p.x + width/2.) / dx
        let mutable imin, imax = int (Math.Floor(i)), int (Math.Ceiling(i))
        let mutable jmin, jmax = int (Math.Floor(j)), int (Math.Ceiling(j))
        
        if imin = imax then
            if imax+1 <= x_lines then
                imax <- imax+1
            else
                imin <- imin-1

        if jmin = jmax then
            if jmax+1 <= y_lines then
                jmax <- jmax+1
            else
                jmin <- jmin - 1

        [| this.IndicesToPoint(imax, jmin); this.IndicesToPoint(imin, jmin); this.IndicesToPoint(imin, jmax); this.IndicesToPoint(imax, jmax) |]
        //IntInterval(imin, imax), IntInterval(jmin, jmax)

type GridFunction internal(grid: Grid, ?origin: Geometry.Point) =
    let x = Array.create grid.XLines 0.
    let y = Array.create grid.YLines 0.
    let origin = defaultArg origin (Geometry.Point())
    
    let mutable interpolant: ref<alglib.spline2dinterpolant> = ref null
    let mutex = new Mutex()

    do
        for j = 0 to grid.XLines - 1 do
            x.[j] <- grid.IndicesToPoint(0, j).x

        for i = 0 to grid.YLines - 1 do
            y.[i] <- grid.IndicesToPoint(i, 0).y

    member this.Grid with get() = grid
    member this.Delta with get() = [|grid.Dy; grid.Dx|]
    
    member internal this.Mutex with get() = mutex
    member internal this.Interpolant with get() = interpolant
    member internal this.X with get() = x
    member internal this.Y with get() = y
    member this.translate_to_global_coord(plocal: Point) = (Vector(plocal) + Vector(origin)).ToPoint()
    member this.translate_to_local_coord(pglobal: Point) = (Vector(pglobal) - Vector(origin)).ToPoint()

    member this.IsPointInside(p: Point) =
        Math.Abs(p.x) < grid.Width/2. + float_error &&
            Math.Abs(p.y) < grid.Height/2. + float_error

    member internal this.CheckPoint(p: Point) =
        if not (this.IsPointInside(p)) then
                raise(InnerError(sprintf "point (%.1f,%.1f) outside of managed grid area" p.x p.y))

type GridFunction1D (grid: Grid, ?origin: Geometry.Point) = //, f_limits: FloatInterval) =
    inherit GridFunction(grid, (defaultArg origin (Point()))) //, f_limits)
    let f = Array2D.create grid.YLines grid.XLines 0.

    member this.FSize with get() = [|f.GetLength(0); f.GetLength(1)|]
    member this.F with get() = f

    member this.GetValue(pglobal: Point, ?vals: ref<float[]>) =
        let vals = defaultArg vals (ref null)
        let p = base.translate_to_local_coord(pglobal)
        base.CheckPoint(p)

        base.Mutex.WaitOne() |> ignore
        let fval = alglib.spline2dcalc(!(base.Interpolant), p.x, p.y)
            
        // store values in the adjoining grid points (from which the value is interpolated)
        if !vals <> null then
            let i, j = grid.PointToIndices(p)
            (!vals).[0] <- f.[i, j]
            (!vals).[1] <- f.[i+1, j-1]
            (!vals).[2] <- f.[i, j-1]
            (!vals).[3] <- f.[i-1, j-1]
            (!vals).[4] <- f.[i-1, j]
            (!vals).[5] <- f.[i-1, j+1]
            (!vals).[6] <- f.[i, j+1]
            (!vals).[7] <- f.[i+1, j+1]
            (!vals).[8] <- f.[i+1, j]

        base.Mutex.ReleaseMutex()
        fval

    member this.GetValue(i: int, j: int, ?vals: ref<float[]>) =
        let vals = defaultArg vals (ref null)
        let p = grid.IndicesToPoint(i, j)
        base.CheckPoint(p)

        base.Mutex.WaitOne() |> ignore
        let fval = f.[i, j]
            
        // store values in the adjoining grid points (from which the value is interpolated)
        if !vals <> null then
            let i, j = grid.PointToIndices(p)
            (!vals).[0] <- f.[i, j]
            (!vals).[1] <- f.[i+1, j-1]
            (!vals).[2] <- f.[i, j-1]
            (!vals).[3] <- f.[i-1, j-1]
            (!vals).[4] <- f.[i-1, j]
            (!vals).[5] <- f.[i-1, j+1]
            (!vals).[6] <- f.[i, j+1]
            (!vals).[7] <- f.[i+1, j+1]
            (!vals).[8] <- f.[i+1, j]

        base.Mutex.ReleaseMutex()
        fval

    member this.SetValue(pglobal: Point, value: float) = 
        let p = base.translate_to_local_coord(pglobal)
        let i, j = grid.PointToIndices(p)

        if i < 0 || i > grid.YLines-1 ||
            j < 0 || j > grid.XLines-1 then
                raise(InnerError(sprintf "point (%.1f,%.1f) outside of managed grid area" p.x p.y))
        else
            f.[i,j] <- value

    member this.ComputeInterpolant() =
        base.Mutex.WaitOne() |> ignore
        alglib.spline2dbuildbilinear(base.X, base.Y, f, base.Y.Length, base.X.Length, base.Interpolant)
        base.Mutex.ReleaseMutex()

type GridFunctionND (grid: Grid, D: int, ?origin: Geometry.Point) =
    inherit GridFunction(grid)
    let f = Array.create (grid.YLines*grid.XLines*D) 0.

    member this.FSize with get() = [|grid.YLines; grid.XLines|]
    member this.F with get() = f

    member private this.IndicesToOffset(i: int, j: int) =
        D*(i*grid.XLines + j)

    member this.GetValue(pglobal: Point, ?vals: ref<float[]>) =
        let p = base.translate_to_local_coord(pglobal)
        let vals = defaultArg vals (ref null)
        base.CheckPoint(p)

        base.Mutex.WaitOne() |> ignore
        let fval = ref ([|0.; 0.|])
        alglib.spline2dcalcv(!(base.Interpolant), p.x, p.y, fval)
            
        // store values in the adjoining grid points (from which the value is interpolated)
        if !vals <> null then
            let i, j = grid.PointToIndices(p)
            (!vals).[0] <- f.[this.IndicesToOffset(i, j)]
            (!vals).[1] <- f.[this.IndicesToOffset(i+1, j-1)]
            (!vals).[2] <- f.[this.IndicesToOffset(i, j-1)]
            (!vals).[3] <- f.[this.IndicesToOffset(i-1, j-1)]
            (!vals).[4] <- f.[this.IndicesToOffset(i-1, j)]
            (!vals).[5] <- f.[this.IndicesToOffset(i-1, j+1)]
            (!vals).[6] <- f.[this.IndicesToOffset(i, j+1)]
            (!vals).[7] <- f.[this.IndicesToOffset(i+1, j+1)]
            (!vals).[8] <- f.[this.IndicesToOffset(i+1, j)]

        base.Mutex.ReleaseMutex()

        !fval

    member this.SetValue(pglobal: Point, value: float[]) = 
        let p = base.translate_to_local_coord(pglobal)
        let i, j = grid.PointToIndices(p)

        if i < 0 || i > grid.YLines-1 ||
            j < 0 || j > grid.XLines-1 then
                raise(InnerError(sprintf "point (%.1f,%.1f) outside of managed grid area" p.x p.y))
        else
            let offset = this.IndicesToOffset(i, j)
            for ii = 0 to D-1 do
                f.[offset+ii] <- value.[ii]

    member this.ComputeInterpolant() =
        if base.X.Length <> grid.XLines || base.Y.Length <> grid.YLines ||
           f.Length <> base.Y.Length*base.X.Length*D then
            raise (InnerError("x/y dimenstion is not equal to grid.XLines/grid.YLines"))

        base.Mutex.WaitOne() |> ignore
        alglib.spline2dbuildbilinearv(base.X, base.X.Length, base.Y, base.Y.Length, f, D, base.Interpolant)
        base.Mutex.ReleaseMutex()

type GridFunctionVector(grid: Grid, ?origin: Geometry.Point) =
    inherit GridFunctionND(grid, 2, if origin = None then Point() else origin.Value)

    member this.GetValue(pglobal: Point, ?vals: ref<float[]>) =
        let arr = base.GetValue(pglobal, if vals = None then ref null else vals.Value)
 
        let mutable v = Vector()
        if (arr.Length <> 2) then
            raise(InnerError("the array with coordinates of gradient contains number of elements other than 2"))
        else
            v <- Vector(arr.[1], arr.[0])

        v

    member this.SetValue(pglobal: Point, v: Vector) =
        base.SetValue(pglobal, [|v.y; v.x|])