module ModelParameters

open System
open MathFunctions
open Geometry
open NumericalComputations

let max_steps = 4000
let output_file = "bitmap"

// the calibration parameters of the model
type ModelParameters() =

    // the probability of cell division is calculated in Automata.can_divide()
    // the probability function of a stem cell division which takes as argument the concentration of oxygen
    static let mutable stem_division_prob_o2 = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 0.015)))
    // the probability function of a stem cell divicion which takes as argument the concentration of glucose
    static let mutable stem_division_prob_glucose = ref (LogisticFunc(min = Point(10., 0.001), max = Point(100., 0.015)))
    // the probability function of a stem cell division which takes as argument the cell packing density
    // (please note that min.x > max.x, this is because s must be negative, so that the function is decreasing with x)
    static let mutable stem_division_prob_cell_density = ref (LogisticFunc(min = Point(3., 0.), max = Point(2., 1.)))
    // the probability (out of 1) that a dividing stem cell will divide symmetrically 
    static let mutable stem_symmetric_division_probability = 0.01

    // TIME PARAMETERS //
    // the time interval for a stem cell to stay in G1 phase and then commit to divide, set Cell.wait_before_S
    static let mutable stem_wait_before_commit_to_divide_interval = IntInterval(6, 8) 
    // the time interval for a non-stem cell to stay in G1 phase and then commit to divide, set Cell.wait_before_S
    static let mutable nonstem_wait_before_commit_to_divide_interval = IntInterval(24, 33)  
    // the time interval for a stem cell to stay in S before entering G2/M phase, set Cell.wait_before_G2M
    static let mutable stem_wait_before_G2M_interval = IntInterval(16, 20)
    // the time interval for a non-stem cell to stay in S before entering G2/M phase, set Cell.wait_before_G2M
    static let mutable nonstem_wait_before_G2M_interval = IntInterval(8, 18)
    // the time interval for a stem cell to stay in G2/M before undergoing cytokinesis, set Cell.wait_before_divide
    static let mutable stem_wait_before_divide_interval = IntInterval(6, 8)
    // the time interval for a non-stem cell to stay in G2/M before undergoing cytokinesis, set Cell.wait_before_divide
    static let mutable nonstem_wait_before_divide_interval = IntInterval(6, 10)
    // the interval in which a random value is taken to set Cell.wait_before_divide - for stem cells
//    static let mutable stem_wait_before_division_interval = IntInterval(0, 20)
    // the interval in which a random value is taken to set Cell.wait_before_divide - for non-stem cells
//    static let mutable nonstem_wait_before_division_interval = IntInterval(30, 60)
    // the interval in which a random value is taken to set Cell.wait_before_necrosis
    static let mutable necrosis_wait_interval = IntInterval(0, 32)   // original interval (0, 20)
    // the interval in which a random value is taken to set Cell.wait_before_desintegration
    static let mutable necrosis_desintergration_interval = IntInterval(240, 480)    // original interval (150, 300)
    // the interval for a complete cell cycle for stem and non-stem cells
    static let mutable stem_cell_cycle_interval = IntInterval(28, 36)
    static let mutable nonstem_cell_cycle_interval = IntInterval(38, 61)

    // the probability function of a non-stem cell division which takes as argument the concentration of oxygen
    static let mutable nonstem_division_prob_o2 = ref (LogisticFunc(min = Point(20., 0.001), max = Point(100., 1.)))
    // the probability function of a non-stem cell division which takes as argument the concentration of glucose
    static let mutable nonstem_division_prob_glucose = ref (LogisticFunc(min = Point(10., 0.001), max = Point(100., 1.)))
    // the probability function of a non-stem cell division which takes as argument the cell packing density
    static let mutable nonstem_division_prob_cell_density = ref (LogisticFunc(min = Point(4.5, 0.), max = Point(3., 1.)))

    // the probability of the transition of a stem cell to the "non-stem with memory" state
    static let mutable stemto_nonstem_prob = 0.05

    // the probability of returning from a "non-stem with memory" cell to the stem cell type
    // which takes as argument the number of stem cells in the whole model
    // the point p2 is a "nearly-zero" point, because zero is never reached
    static let mutable nonstem_tostem_prob_stemcells = ref (ShiftExponentFunc(p1 = Point(0., 0.99), p2 = Point(1000., 0.01), ymin = 0.))

    // the probability of the transition of a non-stem cell to the necrosis state which takes as argument the concentration of oxygen
    static let mutable nonstem_necrosis_prob_o2 = ref (LogisticFunc(min = Point(20., 0.), max = Point(7., 1.)))
    // the probability of the transition of a non-stem cell to the necrosis state which takes as argument the concentration of glucose
    static let mutable nonstem_necrosis_prob_glucose = ref (LogisticFunc(min = Point(10., 0.), max = Point(1., 1.)))
    // the probability of the transition of a non-stem cell to the apoptosis state which takes as argument the age of the cell
    static let mutable nonstem_apoptosis_prob_age = ref (LogisticFunc(min = Point(150., 0.), max = Point(200., 1.)))
    
    // the probability of EGF being Up
    static let mutable egf_probability = 0.8

    // NUTRIENT PARAMETERS //
    // the concentration of oxygen is calculated in Automata.calc_o2()
    // the interval in which the concentration of oxygen can take values
    static let o2_limits = FloatInterval(0., 100.)
    // the parameters for calculating oxygen supply and consumption
    // c1 is oxygen supply in one grid mesh (outside the tumor mass) per time unit
    // c2 is oxygen consumption per one dividing cell per time unit
    // c3 is oxygen consumption per one non-dividing cell per time unit
    static let mutable o2_param = (1., 0.3, 0.15) // (c1, c2, c3)
    // the coefficient of oxygen diffusion
    static let mutable oxygen_diffusion_coeff = 10.

    // the concentration of glucose is calculated in Automata.calc_glucose()
    static let glucose_limits = FloatInterval(0., 100.)
    // the parameters for calculating glucose supply and consumption
    // c1 is glucose supply in one grid mesh (outside the tumor mass) per time unit
    // c2 is glucose consumption per one dividing cell per time unit
    // c3 is gluocse consumption per one non-dividing cell per time unit
    static let mutable glucose_param = (1., 0.45, 0.22) // (c1, c2, c3)
    // the coefficient of glucose difusion
    static let mutable glucose_diffusion_coeff = 0.25

    // RADIATION PARAMETERS //
    static let mutable g0g1_radiation_param = (0.351, 0.04) // (alpha, beta)
    static let mutable s_radiation_param = (0.1235, 0.0285)
    static let mutable g2m_radiation_param = (0.793, 0.)
    static let mutable radiation_dose = 2.
    static let mutable radiation_event_time_step = 500

    // the interval in which the cell packing density can take values
    // the cell packing density is calculated in Automata.calc_cellpackdensity()
    // with this method of calculation, the density is not limited from above, so the upper limit is made up
    // however, in practice cell packing density should not go above this limit
    // given that the other model parameters are not changed too much
    static let cell_packing_density_limits = FloatInterval(0., 10.) 
   
    // the repulsive force acting on a cell is calculated in MolecularDynamics.repulsive_force()
    // the maximum repulsive force
    static let mutable max_repulsive_force = 20.
    // cell displacement quantifies how much a cell can be deformed, 0 < max_displacement < 1
    // the repulsive force is a function of cell displacement
    static let mutable repulsive_force = ref (ShiftExponentFunc(p1 = Point(0., max_repulsive_force),
                                                            p2 = Point(0.9, max_repulsive_force*0.01), ymin = 0.))

    // the friction force acting on a cell is calculated in MolecularDynamics.compute_forces()
    // the coefficient of viscosity
    static let mutable viscosity_coeff = 2.

    // average radius of a cell
    static let mutable average_cell_radius = 10.

     // the size of the window, the same as the size of the grid
    static let window_size = Drawing.Size(width = 1400, height = 1000)

    // the grid for the 2-dimensional function of the cell packing density
    static let mutable cell_packing_density_grid = Grid(width = float window_size.Width, height = float window_size.Height,
                                                    dx = average_cell_radius, dy = average_cell_radius)
    
    // the grid for the 2-dimensional function of oxygen concentration
    static let mutable o2_grid = Grid(width = float window_size.Width, height = float window_size.Height,
                                            dx = average_cell_radius, dy = average_cell_radius)
    // the grid for the 2-dimensional function of glucose concentration
    static let mutable glucose_grid = Grid(width = float window_size.Width, height = float window_size.Height,
                                            dx = average_cell_radius, dy = average_cell_radius)

    
    
    static member StemDivisionProbabilityO2 with get() = stem_division_prob_o2
                                             and set(p) = stem_division_prob_o2 <- p

    static member StemDivisionProbabilityGlucose with get() = stem_division_prob_glucose
                                                  and set(p) = stem_division_prob_glucose <- p

    static member NonStemDivisionProbabilityO2 with get() = nonstem_division_prob_o2
                                                and set(p) = nonstem_division_prob_o2 <- p

    static member NonStemDivisionProbabilityGlucose with get() = nonstem_division_prob_glucose
                                                     and set(p) = nonstem_division_prob_glucose <- p

    static member StemToNonStemProbability with get() = stemto_nonstem_prob
                                            and set(p) = stemto_nonstem_prob <- p

    static member NonStemToStemProbability with get() = nonstem_tostem_prob_stemcells
                                            and set(p) = nonstem_tostem_prob_stemcells <- p

    static member EGFProb with get() = egf_probability and set(p) = egf_probability <- p

    static member NonStemNecrosisProbabilityO2 with get() = nonstem_necrosis_prob_o2
                                                and set(p) = nonstem_necrosis_prob_o2 <- p

    static member NonStemNecrosisProbabilityGlucose with get() = nonstem_necrosis_prob_glucose
                                                     and set(p) = nonstem_necrosis_prob_glucose <- p

    static member StemSymmetricDivisionProbability with get() = stem_symmetric_division_probability 
                                                    and set(p) = stem_symmetric_division_probability <- p

    static member StemWaitBeforeCommitToDivideInterval with get() = stem_wait_before_commit_to_divide_interval
                                                        and set(x) = stem_wait_before_commit_to_divide_interval <- x

    static member NonStemWaitBeforeCommitToDivideInterval with get() = nonstem_wait_before_commit_to_divide_interval
                                                           and set(x) = nonstem_wait_before_commit_to_divide_interval <- x

    static member StemWaitBeforeG2MInterval with get() = stem_wait_before_G2M_interval
                                             and set(x) = stem_wait_before_G2M_interval <- x

    static member NonStemWaitBeforeG2MInterval with get() = nonstem_wait_before_G2M_interval
                                                and set(x) = nonstem_wait_before_G2M_interval <- x

    static member StemWaitBeforeDivisionInterval with get() = stem_wait_before_divide_interval
                                                  and set(x) = stem_wait_before_divide_interval <- x

    static member NonStemWaitBeforeDivisionInterval with get() = nonstem_wait_before_divide_interval
                                                    and set(x) = nonstem_wait_before_divide_interval <- x

    static member StemCellCycleInterval with get() = stem_cell_cycle_interval and set(x) = stem_cell_cycle_interval <- x

    static member NonStemCellCycleInterval with get() = nonstem_cell_cycle_interval and set(x) = nonstem_cell_cycle_interval <- x

    static member NecrosisWaitInterval with get() = necrosis_wait_interval
                                        and set(x) = necrosis_wait_interval <- x

    static member NecrosisDesintegrationInterval with get() = necrosis_desintergration_interval
                                                  and set(x) = necrosis_desintergration_interval <- x

    static member O2Param with get() = o2_param and set(p) = o2_param <- p

    static member GlucoseParam with get() = glucose_param and set(p) = glucose_param <- p

    static member G0G1RadiationParam with get() = g0g1_radiation_param and set(x) = g0g1_radiation_param <- x

    static member SRadiationParam with get() = s_radiation_param and set(x) = s_radiation_param <- x

    static member G2MRadiationParam with get() = g2m_radiation_param and set(x) = g2m_radiation_param <- x

    static member RadiationDose with get() = radiation_dose and set(x) = radiation_dose <- x

    static member RadiationEventTimeStep with get() = radiation_event_time_step and set(x) = radiation_event_time_step <- x

    static member StemDivisionProbabilityDensity with get() = stem_division_prob_cell_density
                                                  and set(x) = stem_division_prob_cell_density <- x

    static member NonStemDivisionProbabilityDensity with get() = nonstem_division_prob_cell_density
                                                     and set(x) = nonstem_division_prob_cell_density <- x

    static member NonStemApoptosisProbabilityAge with get() = nonstem_apoptosis_prob_age
                                                  and set(x) = nonstem_apoptosis_prob_age <- x

    static member RepulsiveForce with get() = repulsive_force
                                  and set(x) = repulsive_force <- x
    
    static member DisplacementInterval with get() = FloatInterval(0., 1.)

    static member ViscosityCoeff with get() = viscosity_coeff
                                  and set(x) = viscosity_coeff <- x

    static member OxygenDiffusionCoeff with get() = oxygen_diffusion_coeff
                                        and set(x) = oxygen_diffusion_coeff <- x

    static member GlucoseDiffusionCoeff with get() = glucose_diffusion_coeff
                                         and set(x) = glucose_diffusion_coeff <- x

    static member WindowSize with get() = window_size
    
    static member O2Grid with get() = o2_grid and set(x) = o2_grid <- x
    
    static member GlucoseGrid with get() = glucose_grid and set(x) = glucose_grid <- x
    
    static member CellPackingDensityGrid with get() = cell_packing_density_grid and set(x) = cell_packing_density_grid <- x
    
    static member AverageCellRadius with get() = average_cell_radius and set(x) = average_cell_radius <- x
    
    static member O2Limits with get() = o2_limits
    
    static member GlucoseLimits with get() = glucose_limits
    
    static member CellPackingDensityLimits with get() = cell_packing_density_limits