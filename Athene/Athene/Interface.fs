module Interface

open Physics
open Vector
open Automata

type particleModification = Death | Life of Particle*Map<QN.var,int> | Divide of (Particle*Map<QN.var,int>)*(Particle*Map<QN.var,int>)
type interfaceTopology = {name:string; regions:((Cuboid<um>* int* int) list); responses:((float<second>->Particle->Map<QN.var,int>->particleModification) list)}

let probabilisticMotor (min:int) (max:int) (state:int) (rng:System.Random) (force:float<zNewton>) (p:Particle) =
    //pMotor returns a force randomly depending on the state of the variable
    match rng.Next(min,max) with
    | x when x > state -> p.orientation*force
    | _ -> {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>}

let rlinearGrow (rate: float<um/second>) (max: float<um>) (varID: int) (varState: int) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    match (m.[varID] = varState) with
    | false -> p
    | true ->
            match (p.radius < max) with
            | true -> Particle(p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze)
            | false -> p

let linearGrow (rate: float<um/second>) (max: float<um>) (varID: int) (varState: int) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    match (m.[varID] = varState) with
    | false -> Life (p,m)
    | true ->
            match (p.radius < max) with
            | true -> Life (Particle(p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze),m)
            | false -> Life (p,m)

let linearGrowDivide (rate: float<um/second>) (max: float<um>) (varID: int) (varState: int) (rng: System.Random) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    match (m.[varID] = varState) with
    | false -> Life (p,m)
    | true ->
            match (p.radius < max) with
            | true -> Life (Particle(p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze),m)
            | false -> Divide ((Particle(p.name,p.location+(p.orientation*p.radius),p.velocity,p.orientation,p.Friction,(p.radius/2.),p.density,p.age,(PRNG.gaussianMargalisPolar' rng),p.freeze),m),(Particle(p.name,p.location-(p.orientation*p.radius),p.velocity,p.orientation,p.Friction,(p.radius/2.),p.density,0.<second>,(PRNG.gaussianMargalisPolar' rng),p.freeze),m))

let probabilisticGrowDivide (rate: float<um/second>) (max: float<um>) (sd: float<um>) (varID: int) (varState: int) (rng: System.Random) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let cbrt2 = 2.**(1./3.)
    match (m.[varID] = varState) with
    | false -> Life (p,m)
    | true ->
            match (p.radius < (max+sd*p.gRand)) with
            | true -> Life (Particle(p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze),m)
            | false -> Divide ((Particle(p.name,p.location+(p.orientation*p.radius),p.velocity,p.orientation,p.Friction,(p.radius/(cbrt2)),p.density,p.age,(PRNG.gaussianMargalisPolar' rng),p.freeze),m),(Particle(p.name,p.location-(p.orientation*p.radius),p.velocity,p.orientation,p.Friction,(p.radius/(cbrt2)),p.density,0.<second>,(PRNG.gaussianMargalisPolar' rng),p.freeze),m))

let apoptosis (varID: int) (varState: int) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    //dt doesn't do anything here- this is an 'instant death' function
    match (m.[varID] = varState) with
    | false -> Life (p,m)
    | true -> Death

let randomApoptosis (varID: int) (varState: int) (rng: System.Random) (probability: float) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    //dt doesn't do anything here- this is an 'instant death' function
    match (m.[varID] = varState) with
    | false -> Life (p,m)
    | true -> match (rng.NextDouble() < probability) with
                | true -> Death
                | false -> Life (p,m)

let rec dev (f : float<second>->Particle->Map<QN.var,int>->particleModification) (dT: float<second>) (pm: (Particle*Map<QN.var,int>) list) (acc: (Particle*Map<QN.var,int>) list) =
    match pm with
    | head::tail -> 
                    let (p,m) = head
                    match (f dT p m) with
                    | Death -> dev f dT tail acc
                    | Life(p1,m1) ->  dev f dT tail ((p1,m1)::acc)
                    | Divide((p1,m1),(p2,m2)) -> dev f dT tail ((p1,m1)::((p2,m2)::acc))
    | [] -> List.fold (fun acc elem -> elem::acc) [] acc //reverse the list which has been created by cons'ing

let rec devProcess (r : (float<second>->Particle->Map<QN.var,int>->particleModification) list) (dT:float<second>) (pm: (Particle*Map<QN.var,int>) list) = 
    match r with
    | head::tail -> devProcess tail dT (dev head dT pm [])
    | [] -> pm

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

let interfaceUpdate (system: Particle list) (machineStates: Map<QN.var,int> list) (dT:float<second>) (intTop:interfaceTopology)  =
    //let (name,regions,responses) = iTop
    let name = intTop.name
    let regions = intTop.regions
    let responses = intTop.responses
    let (staticSystem,machineSystem) = divideSystem system name
    (*
    Regions in the interface set a variable state of a machine based on the machines physical location. 
    If the machine is within a box, the variable is set to something unusual
    ie if I'm here, then I get a signals
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


    let rec respond (r:  (float<second>->Particle->Map<QN.var,int>->Particle) list) (dT: float<second>) (p: Particle) (m: Map<QN.var,int>) =
        match r with
        | head::tail -> respond tail dT (head dT p m) m
        | [] -> p

    //let nmSystem = [for (p,m) in (List.zip machineSystem machineStates) -> respond responses dT p m]



    //How do you fix the problem of physicsI update first or machineI update first?
    //Do I need to pass *both* new and old machines?
    //No, I'm going to update the physics first. The only way this could change things is by dividing across a region- this is not unreasonable
    let pm = devProcess responses dT (List.zip machineSystem machineStates)
    let nMachineStates = [for (p,m) in pm -> regionListSwitch regions p m]
    let nmSystem = [for (p,m) in pm -> p]
    let nSystem = List.foldBack (fun (p: Particle) acc -> p::acc) staticSystem nmSystem
    //let nMachineStates = machineStates
    let machineForces = [for p in nSystem -> {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>} ]
    (nSystem, nMachineStates, machineForces)