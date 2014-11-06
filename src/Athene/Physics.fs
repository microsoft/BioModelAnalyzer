module Physics

//open Vector
open System
open System.Collections.Generic
open System.Linq
open System.Threading


(*
Some back of the envelope calculations to make it all easier
E.coli has a mass of 0.5 picograms
It is roughly cylindical with a height of 2 micrometers and a diameter of 0.5 micrometers
Therefore its volume is 2 * 0.25 * 0.25 * pi = 0.39 micrometers^3
Therefore the density is 1.3 picogram micrometer^-3

From wiki: 'some achieve roughly 60 cell lengths / second'

Therefore we might expect coli's top speed to me 120 um / s

Newton is Kg m s^-2

In a Brownian dynamics scheme (no inertia), with a friction coefficient of 1 s^-1 we need

V = fric / mass * F
F = V * mass / fric
F = 120 um s^-1 * 0.5 pg * fric s^-1
  = 60 um pg s^-2
  = 60 * 10^-6 m * 10^-15 kg s^-2
  = 60 * 10^-21 m Kg s^-2
  = 60 zepto N (10^-21)

This seems low so we need a good guess for the friction coefficient...

From Berry et al the flagellar motor causes linear movement of velocity 200 um / s and a force of 15 pN (Theories of Rotary Motors, 2000, Philo Trans R Soc Lond B)

Working with the BD integrator, it appears that 5 x 10^-9 <second> is  accurate for a 0.5 pg sphere experiencing a force of 15 pN

This works because

F= 15pN = 15*10^9zN

r(t+dt) = F * dT / gamma (+ noise)

gamma = 0.5/X pg second^-1

160 um = 15*10^9 zN * 1 second / gamma

160 um = 15*10^9 pg um second^-2 * 1 second * X second / 0.5 pg

Clear up the units

160 um = 15*10^9 * X /0.5 um

X = 80 * 10^-9 /15 = 5 * 10^-9 s

It also indicates that femto-pico Newtons may be typical for cellular systems
*)

[<Measure>]
type um

[<Measure>]
type second

[<Measure>]
type pg

[<Measure>]
type Kelvin

[<Measure>]
type zNewton = pg um second^-2 //F=ma zeptonewtons because pg um second^-2 = 10^-15 kg 10^-6 m second^-2

type NonBondedCutOff = Unlimited | Limit of float<um>

//[<Measure>]
//type FricCoeff = pg second^-1 //Friction coeffs for Brownian dynamics (AMU/ps in gromacs). v = 1/amu * F + noise. AMU is atomic mass unit(!)

//[<Measure>]
//type FricCon = second // g/s, so to calculate multiply mass by FricConstant

(*
Boltzmann constant is 
1.38 * 10^-23 m^2  kg second^-2 K^-1

1 kg = 10 ^ 15 pg 

1.38 * 10^-8 m^2  pg second^-2 K^-1

1 m ^ 2 = 10^12 um^2

1.38 * 10^4   um^2 pg second^-2 K^-1
*)

//[<Measure>]
//type Metre
//
//[<Measure>]
//type kg
//
//let metreToum d:float<'a> = 
//    d * 1000000.<um/Metre>
//
//let kgTopg m:float<'a> = 
//    m * 1000000000000000.<pg/kg>
//
//let wikiKb = 1.38 * (10.**(-23.))*1.<Metre^2  kg second^-2 Kelvin^-1>
//
//let tkb = kgTopg wikiKb
//            |> metreToum
//            |> metreToum

let Kb = (1.3806488 * 10.**4. )*1.<um^2 pg second^-2 Kelvin^-1>

let gensym =
    let x = ref 0
    (fun () -> incr x; !x)

[<Serializable>]
type Particle = { id:int; name:string; location:Vector.Vector3D<um>; velocity:Vector.Vector3D<um second^-1>; orientation: Vector.Vector3D<1>; Friction: float<second>; radius: float<um>; density: float<pg um^-3>; age: float<second>; pressure: float<zNewton um^-2>; forceMag: float<zNewton>; confluence: int; gRand:float; freeze: bool; variableClock: Map<int,float<second>>} with
    member this.volume = 4. / 3. * System.Math.PI * this.radius * this.radius * this.radius //Ugly
    member this.mass = this.volume * this.density
    member this.frictioncoeff = this.mass / this.Friction
    member this.ToString = sprintf "%d %s %f %f %f %f %f %f %f %f %f %f %f %f %f %f %b" this.id this.name (this.location.x*1.<um^-1>) (this.location.y*1.<um^-1>) (this.location.z*1.<um^-1>) (this.velocity.x*1.<second um^-1>) (this.velocity.y*1.<second um^-1>) (this.velocity.z*1.<second um^-1>) (this.orientation.x) (this.orientation.y) (this.orientation.z) (this.Friction*1.<second^-1>) (this.radius*1.<um^-1>) (this.density*1.<um^3/pg>) (this.age*1.<second^-1>) (this.gRand) this.freeze

let defaultParticle = { id=0;
                        name="X";
                        location= new Vector.Vector3D<um>(); //(){x=0.<um>;y=0.<um>;z=0.<um>};
                        velocity= new Vector.Vector3D<um/second>(); //{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>};
                        orientation= new Vector.Vector3D<1>(1.,0.,0.); //{x=1.;y=0.;z=0.};
                        Friction=1.<second>;
                        radius=0.<um>;
                        density=1.<pg/um^3>;
                        age=0.<second>;
                        pressure= 0.<zNewton um^-2>;//Lazy(fun () -> 0.<zNewton um^-2>);
                        forceMag= 0.<zNewton>; //Lazy(fun () -> 0.<zNewton>);
                        confluence= 0; //Lazy(fun () -> 0);
                        gRand=0.;
                        variableClock=Map.empty;
                        freeze=true}

type searchType = Grid | Simple

(*
SI: implement Particle as a record. then can write update more concisely.
type part = { id:int; Name:string; loc:int } 

let p = { id=0; Name="s"; loc=32 } 
let p' = { p with loc = p.loc + 1 } 
*)

let noForce (p1: Particle) (p2: Particle) = 
    new Vector.Vector3D<zNewton>()
    //{Vector.Vector3D.x=0.<zNewton>;Vector.Vector3D.y=0.<zNewton>;Vector.Vector3D.z=0.<zNewton>} 

let thermalReorientation (T: float<Kelvin>) (rng: System.Random) (dT: float<second>) (cluster: Particle) =
    (*
    Function for updating the orientation of a sphere by brownian motion
    To get this we model the sphere as two point masses at the locations (A and B), the centers of mass of the half spheres, in the direction of the unit vector cluster.orientation (o here)
    A' = Fa + A
    B' = Fb + B
    AB = A - B
    AB' = A' - B'
    AB' = AB + Fa - Fb

    Fa and Fb are the thermal noise for identical particles (now just F), and both functions give a Gaussian distribution.
    If we sum errors a&b with a normal distribution, we get a cumulative error of sqrt(a^2+b^2)
    
    So we approximate that 

    AB' = AB + sqrt(2)F

    The center of mass of a half sphere of radius r is 3/8 r from the circular plane

    AB = o * 6/8 r

    F = sqrt(2 * friction/(mass*0.5) * Kb * dT) * R

    (AB + sqrt(2)*F).norm

    *)
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    let FrictionDrag = 2./cluster.frictioncoeff //We are considering the mass of half spheres now
    let tV =  sqrt (2. * T * FrictionDrag * Kb * dT) * (new Vector.Vector3D<1>((List.nth rNum 0) , (List.nth rNum 1) , (List.nth rNum 2) ) )//{ Vector.Vector3D.x= (List.nth rNum 0) ; Vector.Vector3D.y= (List.nth rNum 1); Vector.Vector3D.z= (List.nth rNum 2)}
    (cluster.orientation*cluster.radius*(3./4.)+sqrt(2.)*tV).norm


let harmonicBondForce (optimum: float<um>) (forceConstant: float<zNewton>) (p1: Particle) (p2: Particle) =
    let ivec  = (p1.location - p2.location)
    let displacement = ivec.len - optimum
    forceConstant * displacement * (p1.location - p2.location).norm

let softSphereForce (repelPower: float) (repelConstant: float<zNewton>) ( attractPower:float ) (attractConstant: float<zNewton>) (attractCutOff: float<um>) (p1: Particle) (p2: Particle) :Vector.Vector3D<zNewton> =
    //Not as hard as a typical hard sphere force (n^-13, where n is less than 1)
    //Repulsion now scales with a power of the absolute distance overlap
    let ivec = (p1.location - p2.location)
    let mindist = p1.radius + p2.radius
    match ivec.len with 
    | d when d > (attractCutOff+mindist) -> ( new Vector.Vector3D<zNewton>() )// {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>}
    | d when d > mindist -> attractConstant * ((ivec.len - mindist)*1.<um^-1>)**attractPower * (p1.location - p2.location).norm
    | _ -> repelConstant * ((ivec.len-mindist)*1.<um^-1>)**repelPower * (p1.location - p2.location).norm

let hardSphereForce (repelForcePower: float) (repelConstant: float<zNewton> ) ( attractPower:float ) (attractConstant: float<zNewton>) (attractCutOff: float<um>) (p1: Particle) (p2: Particle) =
    //'Hard' spheres repel based on a (normalised overlap ** -n) Originally meant to be similar to lennard-jones potentials
    //the force felt by p2 due to collisions with p1 (relative distances), or harmonic adhesion (absolute distances)
    let ivec = (p1.location - p2.location)
    let mindist = p1.radius + p2.radius
    match ivec.len with 
    | d when d > (attractCutOff+mindist)-> ( new Vector.Vector3D<zNewton>() )//{Vector.Vector3D.x=0.<zNewton>;Vector.Vector3D.y=0.<zNewton>;Vector.Vector3D.z=0.<zNewton>} //can't see one another
    | d when d > mindist -> attractConstant * ((ivec.len - mindist)*1.<um^-1>)**attractPower * (p1.location - p2.location).norm
    | _ -> repelConstant * (-1./(ivec.len/mindist)**(repelForcePower)-1.) * (p1.location - p2.location).norm //overlapping

let rec gridFill (system: Particle list) (acc: Map<int*int*int,Particle list>) (minLoc:Vector.Vector3D<um>) (cutOff:float<um>) =
        match system with
        | head:: tail -> 
                            let dx = int ((head.location.x-minLoc.x)/cutOff)
                            let dy = int ((head.location.y-minLoc.y)/cutOff)
                            let dz = int ((head.location.z-minLoc.z)/cutOff)
                            let newValue = match acc.ContainsKey (dx,dy,dz) with
                                            | true  -> head::acc.[(dx,dy,dz)]
                                            | false -> [head]
                            
                            gridFill tail (acc.Add((dx,dy,dz),newValue)) minLoc cutOff
        | [] -> acc

let rec gridFillDict (system: Particle list) (acc: Dictionary<int*int*int,Particle list>) (minLoc:Vector.Vector3D<um>) (cutOff:float<um>) =
        match system with
        | head:: tail -> 
                            let dx = int ((head.location.x-minLoc.x)/cutOff)
                            let dy = int ((head.location.y-minLoc.y)/cutOff)
                            let dz = int ((head.location.z-minLoc.z)/cutOff)
                            let newValue = match acc.ContainsKey (dx,dy,dz) with
                                            | true  -> head::acc.[(dx,dy,dz)]
                                            | false -> [head]
                            
                            acc.[(dx,dy,dz)] <- newValue
                            gridFillDict tail acc minLoc cutOff
        | [] -> acc

let cube (n:int) = 
    let rec core (x:int) (y:int) (z:int) (n:int) (acc:(int*int*int) list) =
        match (x,y,z) with
        | (0,0,0) -> ((x,y,z)::acc)
        | (0,0,_) -> core n n (z-1) n ((x,y,z)::acc)
        | (0,_,_) -> core n (y-1) z n ((x,y,z)::acc)
        | (_,_,_) -> core (x-1) y z n ((x,y,z)::acc)
    core (n-1) (n-1) (n-1) (n-1) []

let existingNeighbourCells (box: int*int*int) (grid: Dictionary<int*int*int,Particle list>) =
        let (x,y,z) = box
        cube 5
        |> List.map (fun (i:int,j:int,k:int) ->     (x-2+i,y-2+j,z-2+k) )
        |> List.map (fun (key:int*int*int) ->   if grid.ContainsKey(key) then grid.[key] else [] )
//                                                match grid.TryFind(key) with
//                                                | Some res -> res
//                                                | None -> []  )

let rec quickJoin (l1: Particle list) (l2: Particle list) =
            match l2 with
            | head::tail -> quickJoin (head::l1) tail
            | [] -> l1

let collectGridNeighbours (p: Particle) (grid: Dictionary<int*int*int,Particle list>) (minLoc:Vector.Vector3D<um>) (cutOff:float<um>) =
         let rec quickJoinLoL (l: Particle list list) acc =
            match l with
            | head::tail -> quickJoinLoL tail (quickJoin acc head)
            | [] -> acc
         let dx = int ((p.location.x-minLoc.x)/cutOff)
         let dy = int ((p.location.y-minLoc.y)/cutOff)
         let dz = int ((p.location.z-minLoc.z)/cutOff)
         
         quickJoinLoL (existingNeighbourCells (dx,dy,dz) grid) [] 
         |> List.filter (fun (pcomp:Particle) -> not (p.id = pcomp.id))                     //Filter out self interactions
         |> List.filter (fun (pcomp:Particle) -> ((p.location-pcomp.location).len<=cutOff)) //Filter out interactions beyond the cutoff
         //|> List.sortBy (fun (p:Particle)     -> p.id)                                      //Test associativity induced numerical errors
let collectSimpleNeighbours (p: Particle) (system: Particle array) (cutOff:float<um>) = 
    system
    |> Array.filter (fun (pcomp:Particle) -> not (p.id = pcomp.id))                     //Filter out self interactions
    |> Array.filter (fun (pcomp:Particle) -> ((p.location-pcomp.location).len<=cutOff)) //Filter out interactions beyond the cutoff
    //|> List.sortBy (fun (p:Particle)     -> p.id)                                      //Test associativity induced numerical errors
let rec _updateGrid (accGrid: Map<int*int*int,Particle list>) (sOrigin:Vector.Vector3D<um>) (mobileSystem: Particle list) (cutOff: float<um>) = 
    match mobileSystem with
    | head :: tail -> 
                            let dx = int ((head.location.x-sOrigin.x)/cutOff)
                            let dy = int ((head.location.y-sOrigin.y)/cutOff)
                            let dz = int ((head.location.z-sOrigin.z)/cutOff)
                            let newValue = match accGrid.ContainsKey (dx,dy,dz) with
                                            | true  -> head::accGrid.[(dx,dy,dz)]
                                            | false -> [head]
                            _updateGrid (accGrid.Add((dx,dy,dz),newValue)) sOrigin tail cutOff

    | [] -> accGrid

//Note: Why don't I just reuse grid fill?
let updateGrid (accGrid: Map<int*int*int,Particle list>) (sOrigin:Vector.Vector3D<um>) (mobileSystem: Particle list) (cutOff: float<um>) =
    mobileSystem
    |> List.map (fun p -> (p, ((int ((p.location.x-sOrigin.x)/cutOff)),(int ((p.location.y-sOrigin.y)/cutOff)),(int ((p.location.z-sOrigin.z)/cutOff)))))
    |> Microsoft.FSharp.Collections.PSeq.fold (fun (acc:Map<int*int*int,Particle list>) (p,cart) -> if acc.ContainsKey cart then acc.Add(cart,p::acc.[cart]) else acc.Add(cart,[p])) accGrid

type forceEnv = { force: Vector.Vector3D<zNewton>; confluence: int; absForceMag: float<zNewton>; pressure: float<zNewton um^-2> ; interactors: (Particle*float<um>) list }
type nonBonded = {P: Particle ; Neighbours: Particle list ; Forces: forceEnv }

let rec joinList a b =
        match a with 
        | top::rest -> joinList rest (top::b)
        | [] -> b

let chunk n s = 
    let rec extractChunk s size lAcc gAcc =
        match s with
        | atom::rest -> let lAcc' = atom::lAcc
                        let gAcc' = if (List.length lAcc') = size then (List.rev lAcc')::gAcc else gAcc //Append to global
                        let lAcc' = if (List.length lAcc') = size then [] else lAcc'                    //clear buffer
                        extractChunk rest size lAcc' gAcc'
        | [] -> match (gAcc,lAcc) with
                | ([],_) -> []
                | (_,[]) -> List.rev gAcc
                | (oldLocalAcc::rest,_) ->  //let oldLocalAcc' = List.rev oldLocalAcc
                                        let lAcc' = List.rev lAcc
                                        List.rev (lAcc'::gAcc)
    let core n s =
        let l = List.length s
        let chunkSize = l/n
                        |> (fun f -> if f*n< l then f+1 else f)
        let leftovers = chunkSize*n
        extractChunk s chunkSize [] []
    if n = 0 then (List.map (fun i -> [i]) s) else core n s 

let unchunk s =
    let rec rejoin s acc =
        match s with
        | [] -> acc
        | top::rest ->  let acc' = joinList (List.rev top) acc
                        rejoin rest acc'
    let s' = List.rev s
    match s' with
    | [] -> []
    | last::first ->    rejoin first last
                        

// SI:: use more specific names than head, tail.
//      | top_particlar:: other_particles -> ... 
let forceUpdate (topology: Map<string,Map<string,Particle->Particle->Vector.Vector3D<zNewton>>>) (cutOff: float<um>) (system: Particle array) (search:searchType) (staticGrid:Dictionary<int*int*int,Particle list>) (staticSystem:Particle array) sOrigin (externalF: Vector.Vector3D<zNewton> array) threads = 
    let rec sumForces (p: Particle) (neighbours: Particle list) (acc: Vector.Vector3D<zNewton>) =
        match neighbours with
        | first_p::other_p -> sumForces p other_p (topology.[p.name].[first_p.name] first_p p) + acc
        | [] -> acc
    let sphereIntersectionArea (p:Particle) (first_p:Particle) = 
        let d = (first_p.location - p.location).len
        let intersectionRadiusSq = 1./(4.*d*d) * (-d + p.radius - first_p.radius) * (-d - p.radius + first_p.radius) * (-d + p.radius + first_p.radius) * (d + p.radius + first_p.radius)  
        2.*System.Math.PI* intersectionRadiusSq
    let rec populateForceEnvironment (p: Particle) (neighbours: Particle list) (acc: forceEnv) =
        (*
        This is expensive and only does one important thing. We want to have it do a few other cheap things on the way.
        The first version summed the vectors on a particle
        This calculates 4 related things as well
        The sum of the vector forces on a particle; the number of forces on a particle (confluence); the sum of the absolute scalar forces on a particle; the sum of the pressures
        *) 
        match neighbours with
        | first_p::other_p ->   let f = topology.[p.name].[first_p.name] first_p p
                                let d = (first_p.location - p.location).len
                                let fMag = f.len
                                let interactors' = if fMag <> 0.<zNewton> then (first_p,d)::acc.interactors else acc.interactors
                                let confluence = if (fMag>0.<zNewton> && not first_p.freeze) then 
                                                                                        acc.confluence+1 else 
                                                                                        acc.confluence
                                let pressure = if (d > (p.radius + first_p.radius)) then 
                                                                                        acc.pressure else 
                                                                                        acc.pressure + fMag/(sphereIntersectionArea p first_p)
                                let fMag' = acc.absForceMag + fMag;
                                let f' = acc.force + f
                                populateForceEnvironment p other_p {acc with 
                                                                        force = f'; 
                                                                        absForceMag = fMag' 
                                                                        confluence = confluence;
                                                                        pressure = pressure;
                                                                        interactors = interactors';
                                                                        }

        | [] -> acc
    let calculateGridNonBonded nonBondedGrid (nb: nonBonded) = 
        if nb.P.freeze then [] else (collectGridNeighbours nb.P nonBondedGrid sOrigin cutOff)   //Get neighbours
        |> (fun (non:nonBonded) (pl: Particle list) -> {nb with Neighbours=pl}) nb              //Update the record
        |> (fun x -> populateForceEnvironment x.P x.Neighbours x.Forces)                        //Caclulate the forces
    let calculateSimpleNonBonded system (nb: nonBonded) =
        if nb.P.freeze then [||] else (collectSimpleNeighbours nb.P system cutOff)                //Get neighbours
        |> List.ofArray
        |> (fun (non:nonBonded) (pl: Particle list) -> {nb with Neighbours=pl}) nb              //Update the record
        |> (fun x -> populateForceEnvironment x.P x.Neighbours x.Forces)                        //Caclulate the forces
    //add all the mobile particles to the staticGrid
    let mobileSystem = (Array.filter (fun (p:Particle) -> not p.freeze) system)
    //printf "ms %A" mobileSystem
    let nonBondedTerms = Array.map (fun x -> { force = x ; confluence=0 ; absForceMag = 0.<zNewton>; pressure= 0.<zNewton um^-2> ; interactors = [] }) externalF
                            |> Array.map2 (fun s f -> {P=s;Neighbours=[];Forces=f}) system  
    //printf "nb %A" nonBondedTerms
    //For testing purposes only- who is different?
//    let nonBondedGrid = updateGrid staticGrid sOrigin mobileSystem cutOff
//    let cSystem = quickJoin mobileSystem staticSystem
//    let p::rest = mobileSystem
//    let gridN = collectGridNeighbours p nonBondedGrid sOrigin cutOff
//    let simpleN = collectSimpleNeighbours p cSystem cutOff
//    let a = if (gridN.Length<>simpleN.Length) then printfn "GridN %A SimpleN %A" gridN.Length simpleN.Length else ()
//    let gridS = gridN |> Set.ofList
//    let simpleS = simpleN |> Set.ofList
//    let a  = if not (gridS=simpleS) then printfn "Sets differ!"
//    let gridF = populateForceEnvironment p gridN {force = {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>} ; confluence=0 ; absForceMag = 0.<zNewton>; pressure= 0.<zNewton um^-2> }
//    let simpleF = populateForceEnvironment p simpleN {force = {x=0.<zNewton>;y=0.<zNewton>;z=0.<zNewton>} ; confluence=0 ; absForceMag = 0.<zNewton>; pressure= 0.<zNewton um^-2> }
//    printfn "G: %A S: %A" gridF.force.len simpleF.force.len
    //End of testing section
    let completeGrid = new Dictionary<int*int*int,Particle list>(staticGrid)
    match search with
    | Grid ->       let nonBondedGrid = gridFillDict (List.ofArray mobileSystem) completeGrid sOrigin cutOff  //updateGrid staticGrid sOrigin (List.ofArray mobileSystem) cutOff
                    nonBondedTerms 
                                |> Array.Parallel.map (fun atom -> calculateGridNonBonded nonBondedGrid atom)
    | Simple ->     let cSystem = Array.init (Array.length mobileSystem + Array.length staticSystem) (fun index -> if index < Array.length mobileSystem then mobileSystem.[index] else staticSystem.[index - Array.length mobileSystem]) //quickJoin mobileSystem staticSystem
                    nonBondedTerms 
                                |> Array.Parallel.map (fun atom -> calculateSimpleNonBonded cSystem atom)

let bdAtomicUpdateNoThermal (cluster: Particle) (F: Vector.Vector3D<zNewton>) T (dT: float<second>) rng (maxMove: float<um>) = 
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F
    // SI: use V' style rather than NewV
    // let V' = FrictionDrag * F
    let NewP = dT * NewV + cluster.location
    { cluster with location = NewP; velocity=NewV; age=cluster.age+dT }


let bdAtomicUpdate (cluster: Particle) (F: Vector.Vector3D<zNewton>) (T: float<Kelvin>) (dT: float<second>) (rng: System.Random) (maxMove: float<um>)= 
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F //+ T * FrictionDrag * Kb
    let ThermalP = sqrt (2. * T * FrictionDrag * Kb * dT) * new Vector.Vector3D<1>(List.nth rNum 0, List.nth rNum 1, List.nth rNum 2) //{ Vector.Vector3D.x= (List.nth rNum 0) ; Vector.Vector3D.y= (List.nth rNum 1); Vector.Vector3D.z= (List.nth rNum 2)}  //integral of velocities over the time
    let NewP = dT * FrictionDrag * F + cluster.location + ThermalP
    { cluster with location = NewP; velocity=NewV; age=cluster.age+dT }

let bdOrientedAtomicUpdate (cluster: Particle) (F: forceEnv) (T: float<Kelvin>) (dT: float<second>) (rng: System.Random) (maxMove: float<um>) = 
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F.force
    let ThermalP = sqrt (2. * T * FrictionDrag * Kb * dT) * new Vector.Vector3D<1>(List.nth rNum 0, List.nth rNum 1, List.nth rNum 2) //{ Vector.Vector3D.x= (List.nth rNum 0) ; Vector.Vector3D.y= (List.nth rNum 1); Vector.Vector3D.z= (List.nth rNum 2)}  //integral of velocities over the time
    let dP = dT * FrictionDrag * F.force + ThermalP
    let NewP = cluster.location + dP
    let NewO = thermalReorientation T rng dT cluster
    { cluster with location = NewP; velocity=NewV; orientation=NewO; age=cluster.age+dT ; pressure=F.pressure; forceMag=F.absForceMag ; confluence=F.confluence}

let bdSystemUpdate (system: Particle array) (forces: forceEnv array) atomicIntegrator (T: float<Kelvin>) (dT: float<second>) (rng: System.Random) (maxMove: float<um>) =
    Array.map2 (fun (p:Particle) (f:forceEnv) -> if p.freeze then p else (atomicIntegrator p f T dT rng maxMove) ) system forces

let steep (system: Particle array) (forceEnv: forceEnv array) (maxlength: float<um>) = 
    let forces = Array.map (fun x->x.force) forceEnv
    let maxV = Array.max (Array.map (fun (f:Vector.Vector3D<zNewton>) ->f.len) forces)
    let modifier = maxlength/maxV
    if (modifier=infinity*1.0<um/zNewton>) then system
    else Array.map2 (fun p f -> if p.freeze then p else {p with location=p.location+(f*modifier)}) system forces

let rec integrate (system: Particle array) topology (searchType: searchType) staticGrid (staticSystem:Particle array) sOrigin (machineForces: Vector.Vector3D<zNewton> array) (T: float<Kelvin>) (dT: float<second>) maxMove (vdt_depth: int) (nbCutOff:float<um>) steps rand threads (F: forceEnv array option) = 
    //Use previously caclulated forces from failed integration step if available
    let F = match F with
            | Some(F) -> F
            | None -> forceUpdate topology nbCutOff system searchType staticGrid staticSystem sOrigin machineForces threads
    //From the update, calculate if the maximum move is broken
    let system' = bdSystemUpdate system F bdOrientedAtomicUpdate T dT rand maxMove
    let maxdP   = Array.map2 (fun (s: Particle) (s': Particle) -> (s'.location - s.location).len) system system'
                    |> Array.max
    match ((maxdP < maxMove),(steps=1),(vdt_depth>0)) with
    | (true,true,_)  -> system'
    | (true,false,_) -> integrate system' topology searchType staticGrid staticSystem sOrigin machineForces T dT maxMove vdt_depth nbCutOff (steps-1) rand threads None //in a single call we shouldn't exceed maxmove, even if done in parts
    | (false,_,true) -> //Maximum move is broken, but we have some variable dT depth left. Halve the timestep, double the steps, reduce the depth by 1 and repeat
                        //Avoid costly recalculation of the forces by supplying the forces calculated in the preceeding, too far step
                        //printf "Dropping down... NewDepth = %A " (vdt_depth-1) 
                        integrate system topology searchType staticGrid staticSystem sOrigin machineForces T (dT/2.) maxMove (vdt_depth-1) nbCutOff (steps*2) rand threads (Some F)
    | (false,_,false) -> //Maximum move is broken, and we have run out of variable dT depth. Fail and exit
                        printf "Max: %A Limit: %A Depth: %A" maxdP maxMove vdt_depth
                        failwith "Max move violated and run out of variable timestep depth. Reduce the timestep or increase the depth of the variability"
