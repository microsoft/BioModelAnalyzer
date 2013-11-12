module MolecularDynamics
open MyMath
open Geometry
open Cell
open ModelParameters
open System

type MolecularDynamics() = 
    static member move(dt: float)(cell: Cell) =
        // the coordinate is approximated by Tailor series:
        // r(t+dt) = r(t) + r'(t) * dt + 1/2*r''(t)*dt^2 + O(dt^3)
        // where r' and r'' are first and second time derivatives resp. of the coordinate
        let mod_f = Math.Sqrt(cell.NetForce.x*cell.NetForce.x+cell.NetForce.y*cell.NetForce.y)
        let r = Vector(cell.Location) + cell.Velocity*dt + 0.5*dt*dt*cell.NetForce/cell.Mass
        cell.Location <- r.ToPoint()

        // v(t+dt) = v(t) + r''(t)*dt + O(dt^2)
        cell.Velocity <- cell.Velocity + dt*cell.NetForce/cell.Mass

    static member interaction_energy(cell1: Cell, cell2: Cell) =
        // we approximate the interaction energy by the following function:
        //          A/r^12  if dr < dr_min
        // V(dr) =  -B/r^2  if dr_min < dr < dr_max
        //          0       if dr > dr_max
        let max_compression = 0.4
        let optimal_compression = 1.
        let r_sum = (cell1.R + cell2.R)
        let dr_min = r_sum*max_compression
        let dr_max = r_sum*optimal_compression
        let dr = Geometry.distance(cell1.Location, cell2.Location)
        let A = 2700.

        (*if (dr < dr_min) then
            1./(Math.Pow(dr, 12.))
        else if (dr < dr_max) then
            -1./(dr*dr)*)
        if (dr < r_sum) then
            A/Math.Pow(dr, 4.)
        else
            0.

    static member interaction_energy_total(cell: Cell, cells: Cell[]) =
        let neighbours = ExternalState.GetNeighbours(cell, cells, 1.8*cell.R)

        let V = ref 0.
        for i = 0 to neighbours.Length-1 do
            V := !V + MolecularDynamics.interaction_energy(cell, neighbours.[i])
        !V

    static member repulsive_force(cell1: Cell, cell2: Cell) =
        let Fdir =  (Vector(cell1.Location) - Vector(cell2.Location)).Normalise()
        
        let dr = Geometry.distance(cell1.Location, cell2.Location) / (cell1.R+cell2.R)
        let fmod = if (dr < 1.) then
                        (!ModelParameters.RepulsiveForceParam).Y(dr)
                    else
                        0.

        fmod * Fdir

    static member repulsive_force_total(cell: Cell, cells: ResizeArray<Cell>) =
        let neighbours = cells.FindAll(fun (c: Cell) -> c <> cell &&
                                                        Geometry.distance(cell.Location, c.Location) < cell.R + c.R) 

        let F = ref (Vector())
        for i = 0 to neighbours.Count-1 do
            F := !F + MolecularDynamics.repulsive_force(cell, neighbours.[i])
        !F

    static member compute_forces(cells: ResizeArray<Cell>)(cell: Cell) =
        // interaction force
        // compute the interaction energy at the current location
        (*let V = MolecularDynamics.interaction_energy_total(cell, cells)
        // move a cell to a new location to compute the gradient of interaction energy
        let step = cell.R/100. // length of a trial step
        let angle = uniform_float(FloatInterval(0., 2.*pi))
        let new_r = Vector(cell.Location, angle, step)
        let loc = cell.Location // save the current location
        cell.Location <- new_r.ToPoint() 
        // compute the interaction energy at the new location
        let new_V = MolecularDynamics.interaction_energy_total(cell, cells) 
        let dV = (new_V - V)
        let dr = new_r - Vector(loc)
        cell.Location <- loc // restore the location
        
        // compute the gradient of interaction energy
        let interaction_force = -Vector(dV / dr.x, dV / dr.y)*)

        (*let interaction_force =*)
        cell.RepulsiveForce <- MolecularDynamics.repulsive_force_total(cell, cells)

        // friction force
        (*let friction_force =*)
        cell.FrictionForce <- -cell.Velocity * ModelParameters.FrictionCoeff

        // net force is the sum of interaction force and friction force
        //cell.NetForce <- interaction_force + friction_force