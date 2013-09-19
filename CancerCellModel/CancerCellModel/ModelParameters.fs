module ModelParameters
open System

// model calibration parameters
type ModelParameters =
    // The probability of cell division is defined as 1 / (max + exp((mu - x)/s)).
    // This function is (a generalised version of) logistic probability function.
    // We use s, mu and max as calibration parameters (DivisionProbParam), x means the amount of oxygen.
    //
    // Note:
    // 1) There is no biological reason to choose this function, but its shape
    // and its limitedness in (0, 1) are convenient for our purposes.
    //
    // 2) To avoid confusion: the function has nothing to do with probability distribution.
    // It does not desribe the probability of taking value x but rather the probability
    // of some other event (cell division) where x is the amount of O2
    
    // StemDivisionProbParam is a tuple of (x coordinates of) two inflection points
    // and the maximum of the function
    static member StemDivisionProbParam = (float 60, float 100, float 0.015) // (x1, x2, max)
    static member NonStemDivisionProbParam = (float 60, float 100, float 1) // (mu, s, max)

    // a function to translate (x1, x2, max) to (mu, s, max)
    static member logistic_func_param(x1: float, x2: float, max: float) =
        let l1 = Math.Log(float 99/max)
        let l2 = Math.Log(float 1 - max)
        let s = (x2-x1)/(l1-l2)
        let mu = s * l1 + x1
        (mu, s, max)
    
    // the logistic function
    static member logistic_func(mu: float, s: float, max: float)(x: float) =
        float 1 / (float 1/max + exp((mu-x)/s))

    // the probability of the transition from a stem to non-stem with memory state
    static member StemToNonStemProbParam = 0.05

    // the probability of the transition from a "non-stem with memory" state to stem
    // is defined as max* n ^ (-x), where x is the number of STEM cells
    // NonStemToStemProbParam is a tuple of x coordinate of a "nearly-zero" point (x3)
    // and the maximum of the function
    // nearly-zero means that the f(x3) = 0.01*max
    static member NonStemToStemProbParam = (1000, float 1) //(x3, max)

    // a function to translate (x3, max) to (n, max)
    static member exp_func_param(x3: int, max: float) =
        let n = Math.Pow(0.01, float (-1) / float x3)
        (n, max)

    // the exponent function
    static member exp_func(n: float, max: float)(x: float) =
        max * Math.Pow(n, -x)

    // the probability that EGF is present
    static member EGFProb: float = 0.8

    // the probability of cell death is defined as an inverted logistic function:
    // 1 - 1 / (max + exp((mu - x)/s))
    static member DeathProbParam = (float 0.1, float 1, float 1) // (x1, x2, max)

    // the probability that a stem cell will divide symmetrically
    // rather than asymmetrically
    static member SymRenewProb = float 0.01

    // the level of O2 is modeled as the function
    // f(n, t+dt) = f(n, t) + dt*c1 - dt*c2*n
    // where dt*c1 - income of O2 in the system
    // dt*c2*n - consumption of O2 - linear in time and number of cells in the model (n)
    static member O2Param = (float 1, float 0.1) // (c1, c2)