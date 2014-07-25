module NumericalComputations

open MathFunctions
open Geometry
open System
open System.Threading

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
    
    
    (*let solve_sle(a: float[,], b: float[]) =
        if a.GetLength(1) <> b.Length then
            raise(InnerError("the number of matrix columns differs from the length vector in sle"))

        let a_mn = DenseMatrix.OfArray(a)
        let b_mn = DenseVector.Create(b.Length, fun (i:int) -> b.[i])
        let x_mn = a_mn.LU().Solve(b_mn)
        x_mn.ToArray()*)

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

        (*a_l <- solve_sle(a_coeff_l, b)
        a_c <- solve_sle(a_coeff_c, b)
        a_r <- solve_sle(a_coeff_r, b)*)

        let info = ref 0
        let rep = ref (alglib.densesolverreport())
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

    // nabla squared is the sum of second partial derivatives for each dimension
    member this.NablaSquared(nabla_squared_arr: float[,], f: float[,], delta: float[]) =
        if k <> 2 then raise(InnerError("The object was initialised with k!=2 (where k is the order of derivative)"))

        for i=0 to f.GetLength(0)-1 do
            for j=0 to f.GetLength(1)-1 do
                nabla_squared_arr.[i, j] <- compute_derivative(f, 1, delta, [|i; j|]) + 
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

type GridFunction internal(initial_grid: Grid, ?origin: Geometry.Point) as this =
    let mutable grid = initial_grid
    let mutable origin = defaultArg origin (Geometry.Point())

    let mutable x: float[] = [||]
    let mutable y: float[] = [||]
    let mutable interpolant = ref (new alglib.spline2dinterpolant())
    let mutex = new Mutex()

    do
        this.init()

    member private this.init() =
        x <- Array.create grid.XLines 0.
        for j = 0 to grid.XLines - 1 do
            x.[j] <- grid.IndicesToPoint(0, j).x

        y <- Array.create grid.YLines 0.
        for i = 0 to grid.YLines - 1 do
            y.[i] <- grid.IndicesToPoint(i, 0).y

    member this.Grid with get() = grid and private set(x) = grid <- x
    member this.Origin with get() = origin and private set(x) = origin <- x
    member this.Delta with get() = [|grid.Dy; grid.Dx|]
    member this.FSize with get() = [|this.Grid.YLines; this.Grid.XLines|]

    member internal this.Mutex with get() = mutex
    member internal this.Interpolant with get() = interpolant
    member internal this.X with get() = x
    member internal this.Y with get() = y
    member this.translate_to_global_coord(plocal: Point) = (Geometry.Vector(plocal) + Geometry.Vector(origin)).ToPoint()
    member this.translate_to_local_coord(pglobal: Point) = (Geometry.Vector(pglobal) - Geometry.Vector(origin)).ToPoint()

    member internal this.CheckPoint(p: Point) =
        if not (Math.Abs(p.x) < this.Grid.Width/2. + float_error &&
                Math.Abs(p.y) < this.Grid.Height/2. + float_error) then
                    raise(InnerError(sprintf "point (%.1f,%.1f) is outside of the managed grid area" p.x p.y))

    member internal this.CheckPoint(i: int, j: int) =
        if not (i >= 0 && i <= this.Grid.YLines-1 &&
                j >= 0 && j <= this.Grid.XLines-1) then
                    raise(InnerError(sprintf "point (i=%d, j=%d) is outside of the managed grid area" i j))

type GridFunction1D (grid: Grid, ?origin: Geometry.Point) =
    inherit GridFunction(grid, (defaultArg origin (Point())))
    let f = Array2D.create grid.YLines grid.XLines 0.

    member this.FSize with get() = [|f.GetLength(0); f.GetLength(1)|]
    member this.F with get() = f
    
    member private this.GetAdjoiningValues(vals: float[], i: int, j: int) =
        vals.[0] <- f.[i, j]
        vals.[1] <- f.[i+1, j-1]
        vals.[2] <- f.[i, j-1]
        vals.[3] <- f.[i-1, j-1]
        vals.[4] <- f.[i-1, j]
        vals.[5] <- f.[i-1, j+1]
        vals.[6] <- f.[i, j+1]
        vals.[7] <- f.[i+1, j+1]
        vals.[8] <- f.[i+1, j]

    member this.GetValue(pglobal: Point, ?vals: float[]) =
        let p = base.translate_to_local_coord(pglobal)
        base.CheckPoint(p)

        base.Mutex.WaitOne() |> ignore
        let fval = alglib.spline2dcalc(!base.Interpolant, p.x, p.y)
            
        // store values in the adjoining grid points (from which the value is interpolated)
        if vals <> None then
            let i, j = grid.PointToIndices(p)
            this.GetAdjoiningValues(vals.Value, i, j)

        base.Mutex.ReleaseMutex()
        fval

    member this.GetValue(i: int, j: int, ?vals: float[]) =
        base.Mutex.WaitOne() |> ignore
        let fval = f.[i, j]
            
        // store values in the adjoining grid points (from which the value is interpolated)
        if vals <> None then
            this.GetAdjoiningValues(vals.Value, i, j)

        base.Mutex.ReleaseMutex()
        fval

    member this.SetValue(pglobal: Point, value: float) = 
        let p = base.translate_to_local_coord(pglobal)
        let i, j = grid.PointToIndices(p)
        base.CheckPoint(i, j)

        base.Mutex.WaitOne() |> ignore
        f.[i,j] <- value
        base.Mutex.ReleaseMutex()
        

    member this.GetAllValues() =
        let vals = Array2D.create (f.GetLength(0)) (f.GetLength(1)) 0.

        base.Mutex.WaitOne() |> ignore
        for i=0 to grid.YLines-1 do
            for j=0 to grid.XLines-1 do
                vals.[i, j] <- f.[i, j]

        base.Mutex.ReleaseMutex()
        vals

    member this.SetValues(vals: float[,]) =
        if vals.GetLength(0) <> grid.YLines ||
            vals.GetLength(1) <> grid.XLines then
                raise(InnerError("the number of grid points differs from the number of function values"))

        base.Mutex.WaitOne() |> ignore
        for i=0 to grid.YLines-1 do
            for j=0 to grid.XLines-1 do
                f.[i, j] <- vals.[i, j]

        base.Mutex.ReleaseMutex()   

    // ComputeInterpolant() must be called before getting the values of the interpolated function
    member this.ComputeInterpolant() =
        base.Mutex.WaitOne() |> ignore
        alglib.spline2dbuildbilinear(base.X, base.Y, f, base.Y.Length, base.X.Length, base.Interpolant)
        base.Mutex.ReleaseMutex()