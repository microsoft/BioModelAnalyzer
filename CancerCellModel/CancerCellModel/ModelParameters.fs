module ModelParameters
open System
open MyMath
open Geometry

type Grid(width: float, height: float, dx: float, dy: float) = 
    let x_lines = int (Math.Floor(width / dx)) + 1
    let y_lines = int (Math.Floor(height / dy)) + 1
    member this.Width with get() = width
    member this.Height with get() = height
    member this.Dx with get() = dx
    member this.Dy with get() = dy
    member this.XLines with get() = x_lines
    member this.YLines with get() = y_lines
    member this.Point(i: int, j: int) = Point(-width/2. + (float i)*dx, -height/2. + (float j)*dy)
    
    member this.CenteredRect(i: int, j: int) =
        let p = this.Point(i, j)
        Rectangle(left = p.x-width/2., right = p.x+width/2., top = p.y-height/2., bottom = p.y+height/2.)

// model calibration parameters
type ModelParameters() =
    static let mutable stem_division_prob_o2_param = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 0.015)))
    static let mutable nonstem_division_prob_o2_param = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 1.)))
    static let mutable division_prob_cell_density_param = ref (LogisticFunc(min = Point(0.95, 0.), max = Point(0.75, 1.)))
    static let mutable stemto_nonstem_prob_param = 0.05
    static let mutable nonstem_tostem_prob_param = ref (ShiftExponentFunc(p1 = Point(0., 1.), p2 = Point(1000., 0.01), ymin = 0.))
    static let mutable egf_prob: float = 0.8
    static let mutable death_prob_o2_param = ref (LogisticFunc(min = Point(20., 0.1), max = Point(10., 1.)))
    static let mutable death_prob_cell_density_param = ref (LogisticFunc(min = Point(0.5, 0.), max = Point(1., 1.)))
    static let mutable death_wait_interval = IntInterval(0, 10)
    static let mutable sym_renew_prob = float 0.01
    static let mutable o2_param = (1., 0.5, 0.25) // (c1, c2, c3)
    static let mutable stem_interval_between_divisions = IntInterval(4, 20)
    static let mutable nonstem_interval_between_divisions = IntInterval(4, 20)
    static let mutable max_numof_cells = 400
    static let mutable max_displacement = 0.6 // 0 < max_displacement < 1 - quantifies how much a cell can be deformed
    static let mutable average_radius = 6. // micrometers
    static let mutable repulsive_force_param = ExponentFunc(p1 = Point(max_displacement, 60.),
                                                            p2 = Point(1., 1.), ymin = 0.)
    static let mutable friction_coeff = 1.
    static let mutable diffusion_coeff = 1.
    static let mutable grid = Grid(width = 1000., height = 800., dx = 10., dy = 8.)
    static let mutable average_cell_r = 10.

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
    static member StemDivisionProbParam with get() = stem_division_prob_o2_param
                                        and set(p) = stem_division_prob_o2_param <- p

    static member NonStemDivisionProbParam with get() = nonstem_division_prob_o2_param
                                            and set(p) = nonstem_division_prob_o2_param <- p

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

    // the probability that EGF is present
    static member EGFProb with get() = egf_prob and set(p) = egf_prob <- p

    // the probability of cell death is defined a logistic function
    // (s must be negative, so that the function is decreasing with x):
    static member DeathProbOnO2Param with get() = death_prob_o2_param
                                      and set(p) = death_prob_o2_param <- p

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

    static member DivisionProbOnCellDensity with get() = division_prob_cell_density_param
                                                    and set(x) = division_prob_cell_density_param <- x

    static member DeathProbDependOnCellDensity with get() = death_prob_cell_density_param
                                                     and set(x) = death_prob_cell_density_param <- x

    static member RepulsiveForceParam with get() = repulsive_force_param
                                         and set(x) = repulsive_force_param <- x

    static member FrictionCoeff with get() = friction_coeff
                                        and set(x) = friction_coeff <- x
    
    static member DiffusionCoeff with get() = diffusion_coeff
                                        and set(x) = diffusion_coeff <- x

    static member GridParam with get() = grid and set(x) = grid <- x
    static member AverageCellR with get() = average_cell_r and set(x) = average_cell_r <- x