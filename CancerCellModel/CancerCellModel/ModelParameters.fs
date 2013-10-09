module ModelParameters
open System

// model calibration parameters
type ModelParameters() =
    static let mutable stem_division_prob_param = (float 20, float 100, float 0.001, float 0.015)  // (x1, x2, min, max)
    static let mutable nonstem_division_prob_param = (float 20, float 100, float 0.001, float 1) // (mu, s, min, max)
    static let mutable stemto_nonstem_prob_param = 0.05
    static let mutable nonstem_tostem_prob_param = (1000, float 0, float 1) //(x3, min, max)
    static let mutable egf_prob: float = 0.8
    static let mutable death_prob_param = (float 20, float 10, float 0.1, float 1) // (x1, x2, min, max)
    static let mutable death_wait_interval = (0, 10)
    static let mutable sym_renew_prob = float 0.01
    static let mutable o2_param = (float 1, float 0.01, float 0.005, float 0.001) // (c1, c2, c3, k)
    static let mutable stem_interval_between_divisions = (4, 20)
    static let mutable nonstem_interval_between_divisions = ( 4, 20)
    static let mutable max_numof_cells = 400
    static let mutable division_prob_cell_density_param = (0.75, 0.5, float 0, float 1)
    static let mutable death_prob_cell_density_param = (0.5, float 1, float 0.1, float 1)

    // The probability of cell division is defined as min + 1 / (1/(max-min) + exp((mu - x)/s)).
    // This function is a generalised version of logistic function.
    // We use s, mu and max as calibration parameters (DivisionProbParam), x means the amount of oxygen.
    //
    // Note:
    // 1) There is no biological reason to choose this function, but its shape
    // and its limitedness between min and max are convenient for our purposes.
    //
    // 2) To avoid confusion: the function has nothing to do with probability distribution.
    // f(x) not desribe the probability of taking value x but rather the probability
    // of some other event (cell division) where x is the amount of O2

    // StemDivisionProbParam is a tuple of (x coordinates of) two inflection points
    // and the maximum of the function
    static member StemDivisionProbParam with get() = stem_division_prob_param
                                        and set(p) = stem_division_prob_param <- p

    static member NonStemDivisionProbParam with get() = nonstem_division_prob_param
                                            and set(p) = nonstem_division_prob_param <- p

    // The function calculates the parameters mu and s from (x1, x2, min, max)
    // Because the logistic function converges at max and min but never reaches it,
    //   we calculate the parameters from the following two points:
    //   (x1, min+p*(max-min)) and (x2, max - (1-p)*(max-min)), where we define p to be 1% or 0.01
    static member logistic_func_param(x1: float, x2: float, ymin: float, ymax: float) =
    // we substitute x and y to the equation: 1 / (1/max + exp((mu - x)/s)) = y
    // and solve the system of two linear equations with two variables (mu and s)
        let p = 0.01 
        let l1 = Math.Log((float 1-p) / (p*(ymax-ymin)))
        let l2 = Math.Log(p / ((float 1 - p)*(ymax-ymin)))
        let s = (x2-x1)/(l1-l2)
        let mu = s * l1 + x1
        (mu, s, ymin, ymax)
    
    // the logistic function
    static member logistic_func(mu: float, s: float, ymin: float, ymax: float)(x: float) =
        ymin + float 1 / (float 1/(ymax-ymin) + exp((mu-x)/s))

    // the probability of the transition from a stem to non-stem with memory state
    static member StemToNonStemProbParam with get() = stemto_nonstem_prob_param
                                            and set(p) = stemto_nonstem_prob_param <- p

    // the probability of the transition from a "non-stem with memory" state to stem
    // is defined as min + max* n^(-x), where x is the number of STEM cells
    // NonStemToStemProbParam is a tuple of x coordinate of a "nearly-zero" point (x3)
    // and the maximum of the function
    // nearly-zero means that the f(x3) = min + p*(max-min) where we define p to be 1% or 0.01
    static member NonStemToStemProbParam with get() = nonstem_tostem_prob_param
                                            and set(p) = nonstem_tostem_prob_param <- p

    // a function to translate (x, min, max) to (n, min, max)
    static member exp_func_param(x: int, ymin: float, ymax: float) =
    // the equation to be solved is as follows:
    // min + (max-min)* n^(-x) = y
    // we take the point (x, min + p*(max-min)) and  solve a linear equation with one variable (n)
        let p = 0.01
        let n = Math.Pow(p, float (-1) / float x)
        (n, ymin, ymax)

    // the exponent function
    static member exp_func(n: float, ymin: float, ymax: float)(x: float) =
        ymin + (ymax-ymin) * Math.Pow(n, -x)

    // the probability that EGF is present
    static member EGFProb with get() = egf_prob and set(p) = egf_prob <- p

    // the probability of cell death is defined a logistic function
    // (s must be negative, so that the function is decreasing with x):
    static member DeathProbParam with get() = death_prob_param
                                    and set(p) = death_prob_param <- p

    static member DeathWaitInterval with get() = death_wait_interval
                                     and set(x) = death_wait_interval <- x

    // the probability that a stem cell will divide symmetrically
    // rather than asymmetrically
    static member SymRenewProb with get() = sym_renew_prob 
                                and set(p) = sym_renew_prob <- p

    static member StemIntervalBetweenDivisions with get() = stem_interval_between_divisions
                                                and set(x) = stem_interval_between_divisions <- x

    static member NonStemIntervalBetweenDivisions with get() = nonstem_interval_between_divisions
                                                    and set(x) = nonstem_interval_between_divisions <- x

    // the level of O2 is modeled as the function
    // f(n, t+dt) = f(n, t) + dt*(c1 - c2*NumberOfDividingCells(t) - c3*NumberOfNonDividingLiveCells(t))
    // where c1 is the supply of the oxygen per time step,
    // c2 is the consumption of oxygen by dividing cells per time step per cell
    // and c3 is the consumption of oxygen by non-dividing live cells per time step per cell
    //
    // then oxygen per cell is calculated as follows:
    // O2(t)/(k*(NumberIfNonDividingLiveCells(t) + (c2/c3) * NumberOfDividingCells(t))
    static member O2Param with get() = o2_param
                            and set(p) = o2_param <- p

    static member MaxNumOfCells with get() = max_numof_cells
                                and set(x) = max_numof_cells <- x

    static member DivisionProbDependOnCellDensity with get() = division_prob_cell_density_param
                                                    and set(x) = division_prob_cell_density_param <- x

    static member DeathProbDependOnCellDensity with get() = death_prob_cell_density_param
                                                     and set(x) = death_prob_cell_density_param <- x