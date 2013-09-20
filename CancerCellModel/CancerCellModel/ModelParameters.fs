module ModelParameters
open System

// model calibration parameters
type ModelParameters() =
    static let mutable stem_division_prob_param = (float 60, float 100, float 0.015)  // (x1, x2, max)
    static let mutable nonstem_division_prob_param = (float 60, float 100, float 1) // (mu, s, max)
    static let mutable stemto_nonstem_prob_param = 0.05
    static let mutable nonstem_tostem_prob_param = (1000, float 1) //(x3, max)
    static let mutable egf_prob: float = 0.8
    static let mutable death_prob_param = (float 10, float 20, float 1) // (x1, x2, max)
    static let mutable sym_renew_prob = float 0.01
    static let mutable o2_param = (float 1, float 0.1) // (c1, c2)

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
    static member StemDivisionProbParam with get() = stem_division_prob_param
                                        and set(p) = stem_division_prob_param <- p

    static member NonStemDivisionProbParam with get() = nonstem_division_prob_param
                                            and set(p) = nonstem_division_prob_param <- p

    // a function to translate (x1, x2, max) to (mu, s, max)
    static member logistic_func_param(x1: float, x2: float, max: float) =
    // the equation to be solved is as follows:
    // 1 / (1/max + exp((mu - x)/s)) = y
    // we take two points: (x1, 0.01*max) and (x2, 0.99*max)
    // and solve the system of two linear equations with two variables (mu and s)
        let l1 = Math.Log(float 99/max)
        let l2 = Math.Log((float 1/0.99 - float 1) / max)
        let s = (x2-x1)/(l1-l2)
        let mu = s * l1 + x1
        (mu, s, max)
    
    // the logistic function
    static member logistic_func(mu: float, s: float, max: float)(x: float) =
        float 1 / (float 1/max + exp((mu-x)/s))

    // the probability of the transition from a stem to non-stem with memory state
    static member StemToNonStemProbParam with get() = stemto_nonstem_prob_param
                                            and set(p) = stemto_nonstem_prob_param <- p

    // the probability of the transition from a "non-stem with memory" state to stem
    // is defined as max* n^(-x), where x is the number of STEM cells
    // NonStemToStemProbParam is a tuple of x coordinate of a "nearly-zero" point (x3)
    // and the maximum of the function
    // nearly-zero means that the f(x3) = 0.01*max
    static member NonStemToStemProbParam with get() = nonstem_tostem_prob_param
                                            and set(p) = nonstem_tostem_prob_param <- p

    // a function to translate (x3, max) to (n, max)
    static member exp_func_param(x3: int, max: float) =
    // the equation to be solved is as follows:
    // max* n^(-x) = y
    // we take the point (x3, 0.01*max) and  solve a linear equation with one variable (n)
        let n = Math.Pow(0.01, float (-1) / float x3)
        (n, max)

    // the exponent function
    static member exp_func(n: float, max: float)(x: float) =
        max * Math.Pow(n, -x)

    // the probability that EGF is present
    static member EGFProb with get() = egf_prob and set(p) = egf_prob <- p

    // the probability of cell death is defined as an inverted logistic function:
    // 1 - 1 / (max + exp((mu - x)/s))
    static member DeathProbParam with get() = death_prob_param
                                    and set(p) = death_prob_param <- p

    // the probability that a stem cell will divide symmetrically
    // rather than asymmetrically
    static member SymRenewProb with get() = sym_renew_prob 
                                and set(p) = sym_renew_prob <- p

    // the level of O2 is modeled as the function
    // f(n, t+dt) = f(n, t) + dt*c1 - dt*c2*n
    // where dt*c1 - income of O2 in the system
    // dt*c2*n - consumption of O2 - linear in time and number of cells in the model (n)
    static member O2Param with get() = o2_param
                            and set(p) = o2_param <- p