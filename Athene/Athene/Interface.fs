module Interface

open Physics
open Vector
open Automata

let probabilisticMotor (min:int) (max:int) (state:int) (rng:System.Random) (force:float<zNewton>) (p:Particle) =
    //pMotor returns a force randomly depending on the state of the variable
    match rng.Next(min,max) with
    | x when x > state -> p.orientation*force
    | _ -> {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>}

let divideSystem system machineName =
//    let rec f (sys: Particle list) (accS: Particle list) (accM: Particle list) (name:string) =
//        match sys with
//        | head::tail when System.String.Equals(head.name,name) -> f tail accS head::accM name
//        | head::tail -> f tail head::accS accM name
//        | [] -> (accS,accM)
//     f system [] [] machineName
    let f (p: Particle) ((name:string),(accS: Particle list),(accM: Particle list)) =
        match System.String.Equals(p.name,name) with
        | true ->  (name,accS,p::accM)
        | false -> (name,p::accS,accM)
    let (n,s,m) = List.foldBack f system (machineName,[],[])
    (s,m)

let interfaceUpdate (system: Particle list) (machineStates: Map<QN.var,int> list) ((name:string),(regions: (Cuboid<um>* int* int) list)) =
    let (staticSystem,machineSystem) = divideSystem system name
    (*
    Regions in the interface set a variable state of a machine based on the machines physical location. 
    If the machine is within a box, the variable is set to something unusual
    ie if I'm here, then I get a signal
    *)
    let withinCuboid (p: Particle) (c:Cuboid<um>) =
        let farCorner = c.origin+c.dimensions
        (p.location.x <= farCorner.x && p.location.y <= farCorner.y && p.location.z <= farCorner.z) && (p.location.x >= c.origin.x && p.location.y >= c.origin.y && p.location.z >= c.origin.z)
    let regionSwitch (r:Cuboid<um>*int*int) (p: Particle) (m: Map<QN.var,int>) =
        let (regionBox,vID,vState) = r
        match (withinCuboid p regionBox) with
        | true ->  m.Add(vID,vState)
        | false -> m
    let rec regionListSwitch (r: (Cuboid<um>*int*int) list) (p: Particle) (m: Map<QN.var,int>) =
        match r with
        | head::tail -> regionListSwitch tail p (regionSwitch head p m)
        | [] -> m
    let nMachineStates = [for (p,m) in (List.zip machineSystem machineStates) -> regionListSwitch regions p m]
    let nSystem = system
    //let nMachineStates = machineStates
    let machineForces = [for p in system -> {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>} ]
    (nSystem, nMachineStates, machineForces)