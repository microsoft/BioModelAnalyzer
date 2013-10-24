module Physics

open Vector
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

Working with the BD integrator, it appears that 0.00002<second> is  accurate for a 0.5 pg sphere experiencing a force of 15 pN

This works because

F= 15pN = 15*10^6aN

r(t+dt) = F * dT / gamma (+ noise)

gamma = 0.5/X pg second^-1

160 um = 15*10^6 aN * 1 second / gamma

160 um = 15*10^6 pg um second^-2 * 1 second * X second / 0.5 pg

Clear up the units

160 um = 15*10^6 * X /0.5 um

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
type aNewton = pg um second^-2 //F=ma attonewtons because pg * um = 10^-12 kg * 10^-6 m

//[<Measure>]
//type FricCoeff = pg second^-1 //Friction coeffs for Brownian dynamics (AMU/ps in gromacs). v = 1/amu * F + noise. AMU is atomic mass unit(!)

//[<Measure>]
//type FricCon = second // g/s, so to calculate multiply mass by FricConstant

let Kb = 13.806488<um^2 pg second^-2 Kelvin^-1>

type Particle(Name:string, R:Vector.Vector3D<um>,V:Vector.Vector3D<um second^-1>,Friction: float<second>, radius: float<um>, density: float<pg um^-3>, freeze: bool) = 
    member this.name = Name
    member this.location = R
    member this.velocity = V
    member this.Friction = Friction
    member this.volume = 4. / 3. * System.Math.PI * radius * radius * radius //Ugly
    member this.radius = radius
    member this.density = density
    member this.mass = this.volume * this.density
    member this.frictioncoeff = this.mass / Friction
    member this.freeze = freeze

let hardSphereForce (p1: Particle) (p2: Particle) (forceConstant: float<aNewton> ) =
    //the force felt by p2 due to collisions with p1 (relative distances)
    let ivec = (p1.location - p2.location)
    let mindist = p1.radius + p2.radius
    match ivec.len with 
    | d when mindist <= d -> {x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>}
    | _ -> forceConstant * (-1./(ivec.len/mindist)**(13.)-1.) * (p1.location - p2.location).norm

let hardStickySphereForce (p1: Particle) (p2: Particle) (repelConstant: float<aNewton> ) (attractConstant: float<aNewton um^-1>) (attractCutOff: float<um>) =
    //the force felt by p2 due to collisions with p1 (relative distances), or harmonic adhesion (absolute distances)
    let ivec = (p1.location - p2.location)
    let mindist = p1.radius + p2.radius
    match ivec.len with 
    | d when attractCutOff <= d -> {x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>} //can't see one another
    | d when mindist > d -> repelConstant * (-1./(ivec.len/mindist)**(13.)-1.) * (p1.location - p2.location).norm //overlapping
    | _ -> attractConstant * (ivec.len - mindist) * (p1.location - p2.location).norm

let nonBondedPairList (system: Particle list) (cutOff: float<um>) = 
    let getNeighbours (p:Particle) (system: Particle list) (cutOff: float<um>) =
        [for i in system do match (i.location-p.location).len with
                            | x when i=p -> ()
                            | x when x < cutOff-> yield i
                            | _ -> () ]
    [for i in system -> getNeighbours i system cutOff] 

let forceUpdate (system: Particle list) (cutOff: float<um>) = 
    let rec sumForces (p: Particle) (neighbours: Particle list) (acc: Vector.Vector3D<aNewton>) =
        match neighbours with
        //| head::tail -> sumForces p tail (hardSphereForce head p 1.<aNewton>)+acc //Arbitrary 1aN force constant
        | head::tail -> sumForces p tail (hardStickySphereForce head p 1.<aNewton> 1000000.<aNewton/um> 2.<um>)+acc //Arbitrary 10aN force constant
        | [] -> acc
    let nonBonded = nonBondedPairList (system: Particle list) cutOff
    [for item in (List.zip system nonBonded) -> sumForces (fst item) (snd item) {x=0.<aNewton>;y=0.<aNewton>;z=0.<aNewton>}]


let bdAtomicUpdateNoThermal (cluster: Particle) (F: Vector.Vector3D<aNewton>) (dT: float<second>) = 
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F
    let NewP = dT * NewV + cluster.location
    Particle(cluster.name, NewP,NewV,cluster.Friction, cluster.radius, cluster.density, false)

let bdAtomicUpdate (cluster: Particle) (F: Vector.Vector3D<aNewton>) (T: float<Kelvin>) (dT: float<second>) (rng: System.Random) = 
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    let FrictionDrag = 1./cluster.frictioncoeff
    //let ThermalV =  2. * Kb * T * dT * FrictionDrag * { x= (List.nth rNum 0) ; y= (List.nth rNum 1); z= (List.nth rNum 2)} //instantanous velocity from thermal motion
    let NewV = FrictionDrag * F //+ T * FrictionDrag * Kb
    let ThermalP = sqrt (2. * T * FrictionDrag * Kb * dT) * { x= (List.nth rNum 0) ; y= (List.nth rNum 1); z= (List.nth rNum 2)}  //integral of velocities over the time
    let NewP = dT * FrictionDrag * F + cluster.location + ThermalP
    //let NewV = NewP * (1. / dT)
    //printfn "Force %A %A %A" F.x F.y F.z
    Particle(cluster.name, NewP,NewV,cluster.Friction, cluster.radius, cluster.density, false)

let bdSystemUpdate (system: Particle list) forces atomicIntegrator (T: float<Kelvin>) (dT: float<second>) (rng: System.Random) =
    [for (p,f) in List.zip system forces -> 
        match p.freeze with
        | false -> atomicIntegrator p f T dT rng
        | true  -> p ]

