module MyMath

open System

let pow2 x = x*x : float
let pow3 x = x*x*x: float
let pi = System.Math.PI


module Geometry =
    type Point (xx: float, yy: float) = 
        member this.x = xx
        member this.y = yy

        new() = Point(0., 0.)
        new(p: Drawing.Point) = Point(float p.X, float p.Y)
        //override this.ToString() = "Pt(" + string xx + "," + string yy + ")"

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

    type FloatInterval(min: float, max: float) =
        member this.Min = min
        member this.Max = max
        new() = FloatInterval(Double.MinValue, Double.MaxValue)

    type IntInterval(min: int, max: int) = 
        member this.Min = min
        member this.Max = max
        new() = IntInterval(Int32.MinValue, Int32.MaxValue)

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