module Physics

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
F = 120 um s^-1 * 0.5 pg / fric s^-1
  = 60 um pg s^-2
  = 60 * 10^-6 m * 10^-15 kg s^-2
  = 60 * 10^-21 m Kg s^-2
  = 60 zepto N

This seems low so we need a good guess for the friction coefficient...

From Berry et al the flagellar motor causes linear movement of velocity 200 um / s and a force of 15 pN (Theories of Rotary Motors, 2000, Philo Trans R Soc Lond B)

This indicates that the friction coefficient is roughly 10^-9 

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

type Particle(R:Vector.Vector3D<um>,V:Vector.Vector3D<um second^-1>,Friction: float<second>, radius: float<um>, density: float<pg um^-3>) = 
    member this.location = R
    member this.velocity = V
    member this.Friction = Friction
    member this.volume = 4. / 3. * System.Math.PI * radius * radius * radius //Ugly
    member this.radius = radius
    member this.density = density
    member this.mass = this.volume * this.density
    member this.frictioncoeff = this.mass / Friction

let bdUpdateNoThermal (cluster: Particle, F: Vector.Vector3D<aNewton>, T: float<Kelvin>, dT: float<second>) = 
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F
    let NewP = dT * NewV + cluster.location
    Particle(NewP,NewV,cluster.Friction, cluster.radius, cluster.density)

let bdUpdate (cluster: Particle, F: Vector.Vector3D<aNewton>, T: float<Kelvin>, dT: float<second>) = 
    let FrictionDrag = 1./cluster.frictioncoeff
    let NewV = FrictionDrag * F
    let NewP = dT * NewV + cluster.location
    Particle(NewP,NewV,cluster.Friction, cluster.radius, cluster.density)