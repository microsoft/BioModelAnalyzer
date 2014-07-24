module Cell

open System
open ModelParameters
open Geometry
open NumericalComputations

type CellType =
    Stem | NonStem | NonStemWithMemory
    override this.ToString() =
        match this with
        | Stem -> "Stem"
        | NonStem -> "Non-stem"
        | NonStemWithMemory -> "Non-stem with memory"

type CellState =
    FunctioningState | PreNecrosisState | ApoptosisState | NecrosisState | DeathByRadiation
    override this.ToString() =
        match this with
        | FunctioningState -> "Functioning"
        | PreNecrosisState -> "Pre-necrotic state"
        | ApoptosisState -> "Apoptotic Death"
        | NecrosisState -> "Necrotic Death"
        | DeathByRadiation -> "Death by radiation"

type CellCycleStage =
    G0 | G1 | S | G2_M
    override this.ToString() =
        match this with
        | G0 -> "G0"
        | G1 -> "G1"
        | S -> "S"
        | G2_M -> "G2/M"

let live_states = [|FunctioningState; PreNecrosisState|]

type CellAction = NoAction | GotoG0 | GotoS | GotoG2M | GotoCytokinesis | GotoNecrosis | StartApoptosis | NecrosisDesintegration | DieByRadiation

type CellDivideAction = NoDivideAction | StemAsymmetricDivision | StemSymmetricDivision | NonStemDivision 

let divide_action = [|NoDivideAction; StemAsymmetricDivision; StemSymmetricDivision; NonStemDivision|]

type SingleMassPoint(location: Point) =
    let mutable location = location
    let mutable repulsive_force = Vector()
    let mutable friction_force = Vector()
    let mutable velocity = Vector()

    member this.Location with get() = location and set(x) = location <- x
    member this.RepulsiveForce with get() = repulsive_force and set(x) = repulsive_force <- x
    member this.FrictionForce with get() = friction_force and set(x) = friction_force <- x
    member this.NetForce with get() = repulsive_force + friction_force
    member this.Velocity with get() = velocity and set(x) = velocity <- x

    override this.ToString() =
        sprintf "Location: r=(%.1f, %.1f)\n\
                Speed: v=(%.1f, %.1f)\n\
                Repulsive force=(%.1f, %.1f)\n\
                Friction force=(%.1f, %.1f)"
                location.x location.y
                velocity.x velocity.y
                repulsive_force.x repulsive_force.y
                friction_force.x friction_force.y

type PhysSphere(location: Point, radius: float, density: float) = 
     inherit SingleMassPoint(location)

     let mutable radius = radius
     let mutable density = density
    
     member this.Density with get() = density and set(x) = density <- x
     member this.R with get() = radius and set(x) = radius <- x
     member this.Mass with get() = (density * 4./3.*Math.PI*(radius*radius*radius) / 1000.)


type Cell (cell_type: CellType, generation: int, location: Point, radius: float, density: float) =
    inherit PhysSphere(location, radius, density)

    let mutable unique_number = 0  // the unique (in the whole model) identifier of the cell, used for debugging purposes
    static let mutable counter = 0 // used along with unique number for debugging purposes

    let mutable cell_type: CellType = cell_type
    let mutable state: CellState = FunctioningState
    let mutable cell_cycle_stage: CellCycleStage = G1   //cells are initialised in G1
    let mutable action: CellAction = NoAction   // the action to take in the current time step
    let mutable divide_action: CellDivideAction = NoDivideAction // the action to take when the cell undergoes cytokinesis
    let mutable age = 0                         // the age of a cell in time steps
    let mutable generation = generation         // the number of ancestors of a cell
    let mutable wait_before_S = 0               // the number of steps (in a fixed interval) after which a cell can enter S phase (time spent in G1)
    let mutable wait_before_G2M = 0             // the number of steps (in a fixed interval) after which a cell can enter G2/M phase (time spent in S)
    let mutable wait_before_divide = 0          // the number of steps (in a fixed interval) after which a cell can undergo cytokinesis (time spent in G2/M)
    let mutable wait_before_necrosis = 0        // the number of steps (in a fixed interval) after which a cell in the Pre-necrosis state
                                                // will transit either to the Necrosis state or back to the Functioning state
    let mutable wait_before_desintegration = 0  // the number of steps (in a fixed interval) after which a cell in necrosis state will desintegrate
    let mutable steps_after_last_division = 0   // the number of steps which have passed after the last division of the cell, used for statistics

    let init() =
        unique_number <- counter
        counter <- counter + 1

    do
        init()

    member this.State with get() = state and set(s) = state <- s
    
    member this.Type with get() = cell_type and set(t) = cell_type <- t
    
    member this.CellCycleStage with get() = cell_cycle_stage and set(ccs) = cell_cycle_stage <- ccs
    
    member this.Action with get() = action and set(a) = action <- a

    member this.DivideAction with get() = divide_action and set(da) = divide_action <- da
    
    member this.Generation with get() = generation and set(g) = generation <- g
    
    member this.WaitBeforeS with get() = wait_before_S and set(x) = wait_before_S <- x

    member this.WaitBeforeG2M with get() = wait_before_G2M and set(x) = wait_before_G2M <- x

    member this.WaitBeforeDivide with get() = wait_before_divide and set(x) = wait_before_divide <- x
    
    member this.WaitBeforeNecrosis with get() = wait_before_necrosis
                                    and set(x) = wait_before_necrosis <- x

    member this.WaitBeforeDesintegration with get() = wait_before_desintegration
                                          and set(x) = wait_before_desintegration <- x

    member this.StepsAfterLastDivision with get() = steps_after_last_division and set(x) = steps_after_last_division <- x
    
    member this.Age with get() = age and set(x) = age <- x

    override this.ToString() =
        sprintf "Type: %s, State: %s, Cell Cycle Stage: %s, Age: %d, Generation: %d\n%s"
                (cell_type.ToString()) (state.ToString()) (cell_cycle_stage.ToString()) age generation
                (base.ToString())