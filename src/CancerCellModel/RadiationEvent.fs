module RadiationEvent

open System
open ModelParameters
open Cell
open MathFunctions

type RadiationEvent() =
    static let mutable (g0g1_alpha, g0g1_beta) = ModelParameters.G0G1RadiationParam
    static let mutable (s_alpha, s_beta) = ModelParameters.SRadiationParam
    static let mutable (g2m_alpha, g2m_beta) = ModelParameters.G2MRadiationParam
    static let mutable radiation_dose = ModelParameters.RadiationDose

    // calculate surving fraction for cells in each phase of the cell cycle
    static let mutable sf_g0g1 = exp( (-g0g1_alpha*radiation_dose) + (-g0g1_beta*(radiation_dose*2.)) )
    static let mutable sf_s = exp( (-s_alpha*radiation_dose) + (-s_beta*(radiation_dose**2.)) )
    static let mutable sf_g2m = exp( (-g2m_alpha*radiation_dose) + (-g2m_beta*(radiation_dose**2.)) )

    static member G0G1PhaseSF with get() = sf_g0g1
    static member SPhaseSF with get() = sf_s 
    static member G2MPhaseSF with get() = sf_g2m

    static member cell_will_die_by_radiation(cell: Cell) = 
        // returns true if the cell should die by radiation event
        let mutable surviving_prob = 0.
        match cell.CellCycleStage with
            |G0 -> surviving_prob <- RadiationEvent.G0G1PhaseSF
            |G1 -> surviving_prob <- RadiationEvent.G0G1PhaseSF
            |S -> surviving_prob <- RadiationEvent.SPhaseSF
            |G2_M -> surviving_prob <- RadiationEvent.G2MPhaseSF

        let decision = uniform_bool(1.-surviving_prob)
        decision