module ModelParameters
open System
open MyMath
open Geometry

// model calibration parameters
type ModelParameters() =
    static let o2_limits = FloatInterval(0., 100.)
    static let density_limits = FloatInterval(0., 10.)

    static let mutable stem_division_prob_o2 = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 0.015)))
    static let mutable stem_division_prob_cell_density = ref (LogisticFunc(min = Point(0.65*density_limits.Max, 0.), max = Point(0.45*density_limits.Max, 1.)))
    static let mutable stemto_nonstem_prob = 0.05

    static let mutable nonstem_division_prob_o2 = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 1.)))
    static let mutable nonstem_division_prob_cell_density = ref (LogisticFunc(min = Point(0.45*density_limits.Max, 0.), max = Point(0.3*density_limits.Max, 1.)))
    static let mutable nonstem_tostem_prob_stemcells = ref (ShiftExponentFunc(p1 = Point(0., 1.), p2 = Point(1000., 0.01), ymin = 0.))
    static let mutable nonstem_necrosis_prob_o2 = ref (LogisticFunc(min = Point(20., 0.), max = Point(10., 1.)))
    //static let mutable nonstem_necrosis_prob_density_param = ref (LogisticFunc(min = Point(0.75*density_limits.Max, 0.), max = Point(density_limits.Max, 1.)))
    static let mutable nonstem_apoptosis_prob_age = ref (LogisticFunc(min = Point(150., 0.), max = Point(200., 1.)))
    
    static let mutable egf_prob = 0.8
    static let mutable necrosis_wait_interval = IntInterval(0, 10)
    static let mutable sym_renew_prob = 0.01
    static let mutable o2_param = (1., 1., 0.5) // (c1, c2, c3)
    static let mutable stem_interval_between_divisions = IntInterval(0, 20)
    static let mutable nonstem_interval_between_divisions = IntInterval(20, 40)
    //static let mutable max_numof_cells = 400
    static let mutable max_displacement = 0. // 0 < max_displacement < 1 - quantifies how much a cell can be deformed
    static let mutable max_repulsive_force = 30.
    static let mutable average_cell_radius = 6. // micrometers
    static let mutable repulsive_force_param = ref (ShiftExponentFunc(p1 = Point(max_displacement, max_repulsive_force),
                                                            p2 = Point(0.9, max_repulsive_force*0.01), ymin = 0.))
    static let mutable friction_coeff = 2.
    static let mutable max_friction_coeff = 10.
    static let mutable diffusion_coeff = 20.
    static let grid_size = Drawing.Size(width = 1400, height = 1000)
    static let mutable cell_pack_density_grid = Grid(width = float grid_size.Width, height = float grid_size.Height,
                                                    dx = 2.*average_cell_radius, dy = 2.*average_cell_radius)
    static let mutable o2_grid = Grid(width = float grid_size.Width, height = float grid_size.Height,
                                                    dx = 2.*average_cell_radius, dy = 2.*average_cell_radius)

    // StemDivisionProbParam is a tuple of (x coordinates of) two inflection points
    // and the maximum of the function. The function argument is the concentration of O2
    static member StemDivisionProbO2 with get() = stem_division_prob_o2
                                          and set(p) = stem_division_prob_o2 <- p

    static member NonStemDivisionProbO2 with get() = nonstem_division_prob_o2
                                              and set(p) = nonstem_division_prob_o2 <- p

    // the probability of the transition from a stem to non-stem with memory state
    static member StemToNonStemProb with get() = stemto_nonstem_prob
                                            and set(p) = stemto_nonstem_prob <- p

    // the probability of the transition from a "non-stem with memory" state to stem
    // is defined as min + max* n^(-x), where x is the number of STEM cells
    // NonStemToStemProbParam is a tuple of x coordinate of a "nearly-zero" point (x3)
    // and the maximum of the function
    // nearly-zero means that the f(x3) = min + p*(max-min) where we define p to be 1% or 0.01
    static member NonStemToStemProb with get() = nonstem_tostem_prob_stemcells
                                     and set(p) = nonstem_tostem_prob_stemcells <- p

    // the probability that EGF is present
    static member EGFProb with get() = egf_prob and set(p) = egf_prob <- p

    // the probability of cell death is defined a logistic function
    // (s must be negative, so that the function is decreasing with x):
    static member NonStemNecrosisProbO2 with get() = nonstem_necrosis_prob_o2
                                        and set(p) = nonstem_necrosis_prob_o2 <- p

    static member NecrosisWaitInterval with get() = necrosis_wait_interval
                                        and set(x) = necrosis_wait_interval <- x

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

    (*static member MaxNumOfCells with get() = max_numof_cells
                                and set(x) = max_numof_cells <- x*)

    static member StemDivisionProbDensity with get() = stem_division_prob_cell_density
                                                    and set(x) = stem_division_prob_cell_density <- x

    static member NonStemDivisionProbDensity with get() = nonstem_division_prob_cell_density
                                                    and set(x) = nonstem_division_prob_cell_density <- x

    (*static member NonStemDeathProbDensity with get() = nonstem_death_prob_density_param
                                                     and set(x) = nonstem_death_prob_density_param <- x*)

    static member NonStemApoptosisProbAge with get() = nonstem_apoptosis_prob_age
                                           and set(x) = nonstem_apoptosis_prob_age <- x

    static member RepulsiveForceParam with get() = repulsive_force_param
                                         and set(x) = repulsive_force_param <- x
    static member DisplacementInterval with get() = FloatInterval(0., 1.)

    static member FrictionCoeff with get() = friction_coeff
                                        and set(x) = friction_coeff <- x

    static member MaxFrictionCoeff with get() = max_friction_coeff
    
    static member DiffusionCoeff with get() = diffusion_coeff
                                        and set(x) = diffusion_coeff <- x

    static member GridSize with get() = grid_size
    static member O2Grid with get() = o2_grid and set(x) = o2_grid <- x
    static member CellPackDensityGrid with get() = cell_pack_density_grid and set(x) = cell_pack_density_grid <- x
    static member AverageCellR with get() = average_cell_radius and set(x) = average_cell_radius <- x
    
    static member O2Limits with get() = o2_limits
    static member CellPackDensityLimits with get() = density_limits