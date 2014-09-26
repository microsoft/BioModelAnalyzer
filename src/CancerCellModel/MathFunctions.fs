module MathFunctions

open Geometry
open System

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
// f(x) not describe the probability of taking value x but rather the probability
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

// ymin + 1/B^(x-x0) = y
type ShiftExponentFunc(p1: Point, p2: Point, ymin: float) as this =
    [<DefaultValue>] val mutable x0: float
    [<DefaultValue>] val mutable b: float

    let calc_param() =
    // the system of equations to be solved is as follows:
    // 1/B^(x1-x0) + ymin = y1
    // 1/B^(x2-x0) + ymin = y2
        if Math.Abs(p1.y-ymin) < float_error then
            raise(InnerError("Parameters of the exponent function can not be calculated because of division by zero"))

        let l = Math.Log(p2.y-ymin, p1.y-ymin)
        this.x0 <- (p2.x-l*p1.x)/(1.-l)
        this.b <- Math.Pow(p1.y - ymin, 1./(this.x0 - p1.x))

    do
        calc_param()

    member this.P1 with get() = p1
    member this.P2 with get() = p2
    member this.X0 with get() = this.x0
    member this.B with get() = this.b
    member this.YMin with get() = ymin

    // the function value
    member this.Y(x: float) =
        ymin + 1./Math.Pow(this.b, (x-this.x0))
