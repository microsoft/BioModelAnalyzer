module Interface

open Physics
//open Vector
open Automata

//open Microsoft.FSharp.Quotations
//open Microsoft.FSharp.Quotations.Typed
//open Microsoft.FSharp.Quotations.Raw
type cellDev = Shrink of float<Physics.um> | RadialGrowth of float<Physics.um> | VolumeGrowth of float<Physics.um^3>
type growthType = RadialGrowthType of float<Physics.um/Physics.second> | VolumeGrowthType of float<Physics.um^3/Physics.second>
type particleModification = Death of string | Life of Particle*Map<QN.var,int> | Divide of string*(Particle*Map<QN.var,int>)*(Particle*Map<QN.var,int>) | Development of string*cellDev*Particle*Map<QN.var,int>
type clock = {Input:int; InputThreshold:int; OutputID:int; OutputState:int; TimeLimit:float<Physics.second>}
type interfaceTopology = {name:string; regions:((Vector.Cuboid<um>* int* int) list); clocks:(clock list); responses:((float<second>->Particle->Map<QN.var,int>->particleModification) list); randomMotors: (float<Physics.second>->Particle->Map<QN.var,int>->Map<QN.var,int>*Vector.Vector3D<Physics.zNewton>) list}
type floatMetric = Radius | Pressure | Age | Confluence | Force
type limitMetric = RadiusLimit of float<Physics.um> | PressureLimit of float<Physics.zNewton Physics.um^-2> | AgeLimit of float<Physics.second> | ConfluenceLimit of int | ForceLimit of float<Physics.zNewton> | NoLimit
type protectMetric =    RadiusMin of float<Physics.um>
                        | PressureMin of float<Physics.zNewton Physics.um^-2>
                        | AgeMin of float<Physics.second> 
                        | ConfluenceMin of int 
                        | ForceMin of float<Physics.zNewton> 
                        | RadiusMax of float<Physics.um>
                        | PressureMax of float<Physics.zNewton Physics.um^-2>
                        | AgeMax of float<Physics.second> 
                        | ConfluenceMax of int 
                        | ForceMax of float<Physics.zNewton> 
                        | Unprotected
type interfaceState = {Physical: Physics.Particle list ; Formal: Map<QN.var,int> list ; Register: string list}
type chance< [<Measure>] 'a> = Absolute of float | ModelledSingle of float*float<second> | ModelledMultiple of float*float<second>*float<'a>
//Counter type; for measuring the state of mutable responses
type counter = Unlimited | Instances of int
type probe = Contents | Fill of int
//Growth information
type growthInfo =       {
                        rate: growthType
                        varID: int
                        varState: int
                        varName: string
                        limit: limitMetric
                        }

//type chance = Certain | Random of System.Random*float*float

let cbrt2 = 2.**(1./3.)
let rsqrt2 = 1./(2. ** 0.5)

//pSlice is a function to return an appropriate probability per timestep based on an input probability, the time which that probability occurs in, and the timestep
//probability calculated is the probability of an even occuring at least once
//Memoized for speed
let pSlice =
    //Calculates the per timestep probability required to give the global probability requested over a larger number of steps
    let cache = System.Collections.Generic.Dictionary<_, _>()
    (fun probability (time:float<Physics.second>) (timestep:float<Physics.second>) ->
    if cache.ContainsKey((probability,timestep)) then cache.[(probability,timestep)] 
    else    let result = 1. - (1. - probability)**(timestep/time)
            cache.[(probability,timestep)] <- result
            //Todo: Cap at 1/0 instead?
            assert(result>0.&&result<1.) //Fail rather than give a bogus probability
            result )



let erf x =
    //Approximation of erf
    1. - (1./(1. + 0.278393*x + 0.230389*(x**2.) + 0.000972*(x**3.) + 0.078108*(x**4.) )**4.)

let sgn (x:float) = 
    x/abs(x)

let inverf x =
    let a = 0.147
    let b = sqrt( (2./(System.Math.PI*a) + (0.5 * System.Math.Log(1.-x**2.)) )**2. - System.Math.Log(1.-x**2.)/a )
    sgn(x) * sqrt( b - (2./(System.Math.PI*a) + 0.5 * System.Math.Log(1.-x**2.) ) )

//We know the total number of opportunities, and we calculate the necessary number of 'successes' from an input guestimate of the average amount of change
let multiStepProbability_nonmem (time:float<second>) (dt:float<second>) (rate:float<Physics.um second^-1>) (totalChange:float<Physics.um>) (totalProb:float) =
    let steps = floor(time/dt)
    let minimumStepsForSuccess = floor(totalChange / (dt * rate))
    let p50 = minimumStepsForSuccess / steps //This is the probability which will have half the cells die
    //printf "Steps: %A TotalSteps %A Prob50 %A " minimumStepsForSuccess steps p50
    assert(p50<1.&&p50>0.)//If the suggested probability is certainty fail
    let sd50 = sqrt(steps*p50*(1.-p50))
    //how do we scale p50 to get the appropriate probability?
    //We want to shift the mean by a number of standard deviations so that the population which survives is equal to the input probability
    //The population n SD from the mean (+/-) = erf (n/(2.**0.5))
    //So the pop n SD from the mean + xor - = ( erf (n/(2.**0.5)) ) / 2
    //ie Adding n standard deviations to the mean should increase the population over the threshold by ( erf (n/(2.**0.5)) ) / 2
    // ( inverf (d*2.) ) * sqrt(2)

    let d = abs(totalProb-0.5)
    let low_sigma = if (0.=d) then 0. else (inverf (d*2.) ) * sqrt(2.)
    let result = if (totalProb-0.5>0.) then (minimumStepsForSuccess+low_sigma*sd50)/steps else (minimumStepsForSuccess-low_sigma*sd50)/steps
    //Todo: Cap at 0/1?
    assert(result>0.&&result<1.) //Fail rather than giving a bogus probability
    result

let multiStepProbability = 
    let cache = System.Collections.Generic.Dictionary<_, _>()
    (fun (time:float<second>) (dt:float<second>) (rate:float<Physics.um second^-1>) (totalChange:float<Physics.um>) (totalProb:float) ->
    if cache.ContainsKey((time,dt,rate,totalChange,totalProb)) then cache.[(time,dt,rate,totalChange,totalProb)] 
    else    let result = multiStepProbability_nonmem time dt rate totalChange totalProb
            cache.[(time,dt,rate,totalChange,totalProb)] <- result
            result 
    )

let probabilisticBinaryMotor (maxQN:int) (rangeProb:float*float) (probVar:int) (motorVar:int) (rng:System.Random) (force:float<zNewton>) (pTime:float<Physics.second>) (dt:float<Physics.second>) (p:Particle) (m: Map<QN.var,int>)  =
    //Takes one variable which influences the probability, and one variable which represents the motor itself
    //Initial version is a binary motor. If the state of the motor is non-zero then the motor is considered to be on

    //We find the mapping from the probVar state to a probability by assuming that high=likely to be on, low=likely to be off
    //No timescaling at present
    let prob = pSlice ( ((fst rangeProb) + ((snd rangeProb)-(fst rangeProb))*(float probVar)/(float maxQN)) ) pTime dt
    assert(prob<=1.)
    //First alter the motor based on a random number and the state of the probVar
    let m' = match (m.[motorVar],(rng.NextDouble())) with
                | (0,rand) when rand < prob -> (Map.add motorVar 1 m)
                | (_,rand) when rand <(1.-prob) -> (Map.add motorVar 0 m)
                | (_,_) -> m
    //Then respond with a force dependent on the state of the motor
    let force = (if (m'.[motorVar]>0) then 1. else 0.) * p.details.orientation*force 
    //Now return a tuple of the QN and the force
    (m',force)

let cubeRoot (f:float<'m^3>) : float<'m> = 
    System.Math.Pow(float f, 1.0/3.0) |> LanguagePrimitives.FloatWithMeasure

let growCell style (p:Particle) dt =
    match style with 
    | RadialGrowthType(rate) -> new Particle(p.id,p.name,p.location,{p.details with radius=p.details.radius+rate*dt})//{p with radius = p.radius+rate*dt}
    | VolumeGrowthType(rate) -> let volume' = p.volume + rate*dt
                                let radius' = cubeRoot((3. * volume') / (4. * System.Math.PI))
                                //{p with radius = radius'}
                                new Particle(p.id,p.name,p.location,{p.details with radius=radius'})

let developmentEvent style t =
    match style with
    | RadialGrowthType(r) -> RadialGrowth(r*t)
    | VolumeGrowthType(r) -> VolumeGrowth(r*t)

let limitedLinearGrow (g:growthInfo) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    //Limit grow by an arbitrary property
    let overLimit = match g.limit with
                                        | RadiusLimit(n)     -> (p.details.radius > n)
                                        | AgeLimit(n)        -> (p.details.age > n)
                                        | PressureLimit(n)   -> (p.details.pressure > n)
                                        | ForceLimit(n)      -> (p.details.forceMag > n)
                                        | ConfluenceLimit(n) -> (p.details.confluence > n)
                                        | NoLimit            -> true
    match (m.[g.varID] = g.varState, overLimit) with
    | (_,true)  -> Life (p,m)
    | (false,_) -> Life (p,m)
    | (true,_)  ->   Development (g.varName,(developmentEvent g.rate dt), (growCell g.rate p dt) , m)
    //Life ({p with radius = p.radius+rate*dt},m)//(Particle(p.id,p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze),m)

//Todo: reimplement max(and sd) to be a limitMetric (to allow cells to divide against e.g. a volume limit)
//let linearGrowDivide (rate: growthType) (max: float<um>) (sd: float<um>) (varID: int) (varState: int) (varName: string )(rng: System.Random) (limit: limitMetric option) (variation: bool) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
let linearGrowDivide (g:growthInfo) (max: float<um>) (sd: float<um>) (rng: System.Random) (variation: bool) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let max = if variation then max+(sd*p.details.gRand*rsqrt2) else max
    match (m.[g.varID] = g.varState, g.limit) with
    | (false,_) -> Life (p,m) //Not in growth state
    | (true,NoLimit) ->
            match (p.details.radius < max) with
            | true -> Development (g.varName,(developmentEvent g.rate dt),(growCell g.rate p dt),m)// Growing if below limit  (Particle(p.id,p.name,p.location,p.velocity,p.orientation,p.Friction,(p.radius+rate*dt),p.density,p.age,p.gRand,p.freeze),m)
            //| false -> Divide (g.varName,({p with location = p.location+ p.orientation*(p.radius/(cbrt2)); velocity = p.velocity*rsqrt2; radius = (p.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng },m),({p with id = gensym(); location = p.location- p.orientation*(p.radius/(cbrt2)); velocity = p.velocity*rsqrt2; radius = (p.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng },m))
            | false -> Divide (g.varName,(new Particle(p.id,p.name,p.location + p.details.orientation*(p.details.radius/(cbrt2)),{p.details with velocity = p.details.velocity*rsqrt2; radius = (p.details.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng }),m),(new Particle(gensym(),p.name,p.location - p.details.orientation*(p.details.radius/(cbrt2)),{p.details with velocity = p.details.velocity*rsqrt2; radius = (p.details.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng }),m))
    | (true,_) ->
            let overLimit = match g.limit with
                                        | RadiusLimit(n)     -> (p.details.radius > n)
                                        | AgeLimit(n)        -> (p.details.age > n)
                                        | PressureLimit(n)   -> (p.details.pressure > n)
                                        | ForceLimit(n)      -> (p.details.forceMag > n)
                                        | ConfluenceLimit(n) -> (p.details.confluence > n)
                                        | NoLimit            -> true
            match ((p.details.radius < max), overLimit) with
            | (_,true) -> Life (p,m)
            | (true,_)  -> Development (g.varName,(developmentEvent g.rate dt),(growCell g.rate p dt),m)
            | (false,_) -> //Divide (g.varName,({p with location = p.location+ p.orientation*(p.radius/(cbrt2)); velocity = p.velocity*rsqrt2; radius = (p.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng },m),({p with id = gensym(); location = p.location- p.orientation*(p.radius/(cbrt2)); velocity = p.velocity*rsqrt2; radius = (p.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng },m))
                           Divide (g.varName,(new Particle(p.id,p.name,p.location + p.details.orientation*(p.details.radius/(cbrt2)),{p.details with velocity = p.details.velocity*rsqrt2; radius = (p.details.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng }),m),(new Particle(gensym(),p.name,p.location - p.details.orientation*(p.details.radius/(cbrt2)),{p.details with velocity = p.details.velocity*rsqrt2; radius = (p.details.radius/(cbrt2)); age = 0.<second>; gRand = PRNG.gaussianMargalisPolar' rng }),m))
let linearGrowDivideWithVectorDistanceDependence (origin: Vector.Vector3D<Physics.um>) (direction: Vector.Vector3D<1>) (gradient: float<Physics.second^-1>) (g:growthInfo) (max: float<um>) (sd: float<um>) (rng: System.Random) (variation: bool) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let projection =  (p.location - origin) * direction.norm
    let rate' = match g.rate with 
                | RadialGrowthType(rate) ->
                                let rate' = projection * gradient + rate  
                                let rate' = if (rate'< 0.<Physics.um/Physics.second>) then 0.<Physics.um/Physics.second> else rate' //Rate must be positive or zero
                                RadialGrowthType(rate')
                | VolumeGrowthType(rate) ->
                                let rate' = projection * gradient * 1.<um^2> + rate  
                                let rate' = if (rate'< 0.<Physics.um^3/Physics.second>) then 0.<Physics.um^3/Physics.second> else rate' //Rate must be positive or zero
                                VolumeGrowthType(rate')
    
    linearGrowDivide {g with rate=rate'} max sd rng variation dt p m

let testProtection (protection: protectMetric) (p: Physics.Particle) =
    match protection with 
                                        | RadiusMin(n)     -> (p.details.radius > n)
                                        | AgeMin(n)        -> (p.details.age > n)
                                        | PressureMin(n)   -> (p.details.pressure > n)
                                        | ForceMin(n)      -> (p.details.forceMag > n)
                                        | ConfluenceMin(n) -> (p.details.confluence > n)
                                        | RadiusMax(n)     -> (p.details.radius < n)
                                        | AgeMax(n)        -> (p.details.age < n)
                                        | PressureMax(n)   -> (p.details.pressure < n)
                                        | ForceMax(n)      -> (p.details.forceMag < n)
                                        | ConfluenceMax(n) -> (p.details.confluence < n)
                                        | Unprotected      -> true

let apoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    //dt doesn't do anything here- this is an 'instant death' function
    let allowDeath = testProtection protection p
    match (m.[varID] = varState && allowDeath ) with
    | false -> Life (p,m)
    | true -> Death (varName)
    // SI: consider switching to just if-then for these small expressions
    // if (m.[varID] = varState) then Death else Life (p,m)

let limitedApoptosis (limit: int) =
    //Only a limited number of cells are allowed to die
    let counter = ref 0
    (fun (varID: int) (varState: int) (varName: string) (protection: protectMetric) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) ->
        let result = apoptosis varID varState varName protection dt p m
        match (!counter<limit,result) with
        | (_,Life(p,m))         ->  Life(p,m)
        | (true,Death(v))       ->  incr counter
                                    Death (v)
        | (false,Death(v))      ->  Life(p,m)
        | (_,_)                 ->  failwith "Unexpected result from apoptosis" )


let shrinkingApoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (minSize: float<Physics.um>) (shrinkRate: float<Physics.um second^-1>) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let allowDeath = testProtection protection p
    //Note: an alternative way to do this would be to just prevent *death* ie allow the cells to shrink but not die
    match (m.[varID] = varState && allowDeath, p.details.radius <= minSize) with
    | (false,_)    -> Life (p,m)
    | (true,false) -> //Development (varName,Shrink(shrinkRate*dt),{p with radius = p.radius-shrinkRate*dt},m) //Life ({p with radius=p.radius-shrinkRate*dt},m)
                        Development (varName,Shrink(shrinkRate*dt),new Particle(p.id,p.name,p.location,{p.details with radius= p.details.radius-shrinkRate*dt}),m) //Life ({p with radius=p.radius-shrinkRate*dt},m)
    | (true,true)  -> Death (varName)

let shrinkingRandomApoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (minSize: float<Physics.um>) (shrinkRate: float<Physics.um second^-1>) (rng: System.Random) (pType:chance<Physics.um>) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let allowDeath = testProtection protection p
    let probability = match pType with
                        | Absolute(p) -> p
                        | ModelledMultiple(p,pTime,mean) -> multiStepProbability pTime dt shrinkRate mean p//pSlice probability pTime dt
                        | _ -> failwith "Illegal probability type for a multiple step random death"
    //Note: an alternative way to do this would be to just prevent *death* ie allow the cells to shrink but not die
    match (m.[varID] = varState, rng.NextDouble() < probability, p.details.radius <= minSize) with
    | (false,_,_)       -> Life (p,m)
    | (true,false,_)    -> Life (p,m)
    | (true,true,false) -> //Development (varName,Shrink(shrinkRate*dt),{p with radius = p.radius-shrinkRate*dt},m)//Life ({p with radius=p.radius-shrinkRate*dt},m)
                            Development (varName,Shrink(shrinkRate*dt),new Particle(p.id,p.name,p.location,{p.details with radius = p.details.radius-shrinkRate*dt}),m)
    | (true,true,true)  -> Death (varName)

let shrinkingBiasRandomApoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (minSize: float<Physics.um>) (shrinkRate: float<Physics.um second^-1>) (bias:floatMetric) (rng: System.Random) (sizePower:float) (refC:float<'a>) (refM:float) (pType:chance<Physics.um>) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let allowDeath = testProtection protection p
    let probability = match pType with
                        | Absolute(p) -> p
                        | ModelledMultiple(p,pTime,mean) -> multiStepProbability pTime dt shrinkRate mean p//pSlice probability pTime dt
                        | _ -> failwith "Illegal probability type for a multiple step random death"
    let biasMetric = match bias with
                        | Radius     -> float p.details.radius
                        | Age        -> float p.details.age
                        | Pressure   -> float p.details.pressure
                        | Force      -> float p.details.forceMag
                        | Confluence -> float p.details.confluence
    match (m.[varID] = varState && allowDeath, rng.NextDouble() < probability*refM*((biasMetric-(float refC))**sizePower), p.details.radius <= minSize) with
    | (false,_,_)       -> Life (p,m)
    | (true,false,_)    -> Life (p,m)
    | (true,true,false) -> Development (varName,Shrink(shrinkRate*dt),new Particle(p.id,p.name,p.location,{p.details with radius = p.details.radius-shrinkRate*dt}),m)//Life ({p with radius=p.radius-shrinkRate*dt},m)
    | (true,true,true)  -> Death (varName)

let certainDeath (varName) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    Death (varName)

let randomApoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (rng: System.Random) (pType: chance<_>) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let allowDeath = testProtection protection p
    let probability =   match pType with
                        | Absolute(p) -> p
                        | ModelledSingle(p, pTime) -> pSlice p pTime dt
                        | _ -> failwith "Illegal probability type for a single step random death"
    assert(probability<=1.)
    match (m.[varID] = varState && allowDeath, rng.NextDouble() < probability) with
    | (false,_)     -> Life (p,m)
    | (true,true)   -> Death (varName)
    | (true,false)  -> Life (p,m)

let randomBiasApoptosis (varID: int) (varState: int) (varName: string) (protection: protectMetric) (bias: floatMetric) (rng: System.Random) (sizePower:float) (refC:float<'a>) (refM:float) (pType: chance<_>) (dt: float<second>) (p: Particle) (m: Map<QN.var,int>) =
    let allowDeath = testProtection protection p
    //cells have a higher chance of dying based on some metric- the power determines the precise relationship
    let probability =   match pType with
                        | Absolute(p) -> p
                        | ModelledSingle(p, pTime) -> pSlice p pTime dt
                        | _ -> failwith "Illegal probability type for a single step random death"
    assert(probability<=1.)
    let biasMetric = match bias with
                        | Radius     -> float p.details.radius
                        | Age        -> float p.details.age
                        | Pressure   -> float p.details.pressure
                        | Force      -> float p.details.forceMag
                        | Confluence -> float p.details.confluence
    match (m.[varID] = varState && allowDeath, rng.NextDouble() < probability+dt*1.<second^-1>*refM*((biasMetric- (float refC) )**sizePower)) with
    | (false,_) -> Life (p,m)
    | (true,true) -> Death (varName)
    | (true,false) -> Life (p,m)

let rec dev (f : float<second>->Particle->Map<QN.var,int>->particleModification) (dT: float<second>) (pm: (Particle*Map<QN.var,int>) list) (acc: (Particle*Map<QN.var,int>) list) (birthDeathRegister: string list) (systemTime : float<second>)=
    let reporter (p:Particle) (modification:particleModification) (systemTime:float<second>) =
        match modification with
        | Death(cause) -> sprintf "Death by %s of P = %d at T = %f seconds" cause p.id (systemTime*1.<second^-1>)
        | Divide(cause,(p1,m1),(p2,m2)) -> sprintf "Division by %s of P = %d at T = %f seconds (birth of P&P: %d %d)" cause p.id (systemTime*1.<second^-1>) p1.id p2.id 
        | Development(cause,d,p1,m1) -> match d with
                                        | RadialGrowth(c) -> sprintf "Growth by %f um due to  %s of P = %d at T = %f seconds" (c*1.<Physics.um^-1>) cause p1.id (systemTime*1.<second^-1>)
                                        | VolumeGrowth(c) -> sprintf "Growth by %f um^3 due to  %s of P = %d at T = %f seconds" (c*1.<Physics.um^-3>) cause p1.id (systemTime*1.<second^-1>)
                                        | Shrink(c) -> sprintf "Shinkage by %f um due to  %s of P = %d at T = %f seconds" (c*1.<Physics.um^-1>) cause p1.id (systemTime*1.<second^-1>)
        | _ -> ""
    match pm with
    | head::tail -> 
                    let (p,m) = head
                    match (f dT p m) with
                    | Death(cause) -> dev f dT tail acc ((reporter p (Death (cause)) systemTime)::birthDeathRegister) systemTime
                    | Life(p1,m1) ->  dev f dT tail ((p1,m1)::acc) birthDeathRegister systemTime
                    | Development(cause,d,p1,m1) -> dev f dT tail ((p1,m1)::acc) birthDeathRegister systemTime
                    | Divide(cause,(p1,m1),(p2,m2)) -> dev f dT tail ((p1,m1)::((p2,m2)::acc)) ((reporter p (Divide (cause,(p1,m1),(p2,m2))) systemTime)::birthDeathRegister) systemTime
    | [] -> ((List.fold (fun acc elem -> elem::acc) [] acc),birthDeathRegister) //reverse the list which has been created by cons'ing

let rec devProcess (r : (float<second>->Particle->Map<QN.var,int>->particleModification) list) (dT:float<second>) (pm: (Particle*Map<QN.var,int>) list) (birthDeathRegister: string list) (systemTime:float<second>) = 
    match r with
    | head::tail -> let (pm',birthDeathRegister') = dev head dT pm [] birthDeathRegister systemTime
                    devProcess tail dT pm' birthDeathRegister' systemTime
    | [] -> (pm,birthDeathRegister)

let divideSystem system machineName =
    let f (p: Particle) ((name:string),(accS: Particle list),(accM: Particle list)) =
        match System.String.Equals(p.name,name) with
        | true ->  (name,accS,p::accM)
        | false -> (name,p::accS,accM)
    let (n,s,m) = List.foldBack f system (machineName,[],[])
    (s,m)

let interfaceUpdate (system: Particle array) (machineStates: Map<QN.var,int> array) (dT:float<second>) (intTop:interfaceTopology) (systemTime:float<second>) (birthDeathRegister: string list )  =
    let name = intTop.name
    let regions = intTop.regions
    let responses = intTop.responses
    let (machineSystem,staticSystem) = Array.partition (fun (p:Particle) -> not p.details.freeze) system
    (*
    Regions in the interface set a variable state of a machine based on the machines physical location. 
    If the machine is within a box, the variable is set to something unusual
    ie if I'm here, then I get a signals
    *)
    let withinCuboid (p: Particle) (c:Vector.Cuboid<um>) =
        let farCorner = c.origin+c.dimensions
        (p.location.x <= farCorner.x && p.location.y <= farCorner.y && p.location.z <= farCorner.z) && (p.location.x >= c.origin.x && p.location.y >= c.origin.y && p.location.z >= c.origin.z)
    let regionSwitch (r:Vector.Cuboid<um>*int*int) (p: Particle) (m: Map<QN.var,int>) =
        let (regionBox,vID,vState) = r
        match (withinCuboid p regionBox) with
        | true ->  m.Add(vID,vState)
        | false -> m
    let rec regionListSwitch (r: (Vector.Cuboid<um>*int*int) list) (p: Particle) (m: Map<QN.var,int>) =
        match r with
        | head::tail -> regionListSwitch tail p (regionSwitch head p m)
        | [] -> m
    let clockUpdate (clock:clock) (pm:Physics.Particle*Map<QN.var,int>) dt =
        let p,m = pm
        match (m.[clock.Input] >= clock.InputThreshold) with
        | false -> pm
        | true  ->  let time =   match p.details.variableClock.TryFind(clock.Input) with
                                    | None -> 0.<Physics.second>
                                    | Some(t) -> t + dt
                    let m = if time > clock.TimeLimit then m.Add(clock.OutputID,clock.OutputState) else m
                    let p = new Particle(p.id,p.name,p.location,{p.details with variableClock=p.details.variableClock.Add(clock.Input,time)})
                    (p,m)
        
    let rec clockListUpdate (clocks:clock list) (pm:Physics.Particle*Map<QN.var,int>) dt =
        match clocks with 
        | topClock::rest -> clockListUpdate rest (clockUpdate topClock pm dt) dt
        | [] -> pm
    let rec respond (r:  (float<second>->Particle->Map<QN.var,int>->Particle) list) (dT: float<second>) (p: Particle) (m: Map<QN.var,int>) =
        match r with
        | head::tail -> respond tail dT (head dT p m) m
        | [] -> p

    let rec getMotorForces (motorlist:(float<Physics.second>->Particle->Map<QN.var,int>->Map<QN.var,int>*Vector.Vector3D<Physics.zNewton>) list) system machines (dT:float<Physics.second>) (forceAcc:Vector.Vector3D<Physics.zNewton> list )= 
        match motorlist with
        | topMotor::otherMotors ->  let machineForces' = List.map2 (topMotor dT) system machines
                                    let forceAcc' = List.map2 (fun (f:Vector.Vector3D<Physics.zNewton>) (mf:Map<QN.var,int>*Vector.Vector3D<Physics.zNewton>) -> f + (snd mf)) forceAcc machineForces'
                                    let machines' = List.map (fun (mf:Map<QN.var,int>*Vector.Vector3D<Physics.zNewton>) -> fst mf) machineForces'
                                    getMotorForces otherMotors system machines' dT forceAcc'
        | [] -> (forceAcc,machines)
    let machineSystem = List.ofArray machineSystem
    let machineStates = List.ofArray machineStates
    //How do you fix the problem of physicsI update first or machineI update first?
    //Do I need to pass *both* new and old machines?
    //No, I'm going to update the physics first. The only way this could change things is by dividing across a region- this is not unreasonable
    let (pm,birthDeathRegister') = devProcess responses dT (List.zip machineSystem machineStates) birthDeathRegister systemTime
    //let m' = List.map (fun (p,m) -> regionListSwitch regions p m) pm 
    //let p' = List.map (fun (p,m) -> p) pm
    let pm' =   List.map (fun (p,m) -> (p,(regionListSwitch regions p m))) pm 
                |> List.map (fun pm -> clockListUpdate intTop.clocks pm dT)
    let p'= List.map (fun (p,m) -> p) pm' 
    let m'= List.map (fun (p,m) -> m) pm'
    let (f,m'') = getMotorForces intTop.randomMotors p' m' dT (List.map ( fun x -> new Vector.Vector3D<zNewton>() ) p')
    let p' = Array.ofList p'
    let m'' = Array.ofList m''
    let f = Array.ofList f
    (p', m'', f, birthDeathRegister')