(*  Module IO: Read and Write files
    Does some checking of file format and tests input values
 *)

module IO

open System
open System.Collections.Generic
open System.IO
open Physics
open Vector
open Interface
open System.Xml.Linq
open System.Runtime.Serialization.Formatters.Binary

(*
'Spherical E. coli' particle
Particle('E',{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 1.,  0.7<um>, 1.3<pg um^-3>, false)
Particle("E",{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 0.7<um>, 1.3<pg um^-3>, 1.<second>, 1., false)
*)
[<Serializable>]
type systemState = {Physical: Physics.Particle list; Formal: Map<QN.var,int> list}
[<Serializable>]
type systemStorage = {Physical: Physics.Particle list; LFormal: (QN.var*int) list list}

let convertObjectToByteArray(data)=
    let bf = new BinaryFormatter()
    use mstream = new MemoryStream()
    bf.Serialize(mstream,data)
    mstream.ToArray()

let readCheckpoint (filename) = 
    use fileStream = new FileStream(filename, FileMode.Open)
    let bf = new BinaryFormatter()
    let result = bf.Deserialize(fileStream)
    let state' = (result :?> systemStorage)
    let mapped = state'.LFormal |> List.map (fun m -> Map.ofList m)
    let p = state'.Physical
    //Read the maximum gensym and sync the current gensym to that
    let maxG = p |> List.map (fun i -> i.id) |> List.max
    let rec updateGensym (max:int) =
        let g' = Physics.gensym ()
        if (g' < max) then updateGensym max else ()
    ignore (updateGensym maxG)
    {Formal=mapped;Physical=p;}

let dropFrame (system: Physics.Particle list) =
    ()

let dropStates (machines: Map<QN.var,int> list) =
    ()

let dropEvents (events: string list) = 
    ()

let getBias (metric:string) = 
    match metric with
    | "Age" -> Age
    | "Size" -> Radius
    | "Confluence" -> Confluence
    | "Force" -> Force
    | "Pressure" -> Pressure
    | _ -> failwith "Incorrect bias type"


let cart2Particle ((name:string), (xr:float), (yr:float), (zr:float), (rng:System.Random)) = 
    //Particle(gensym(),name,{x=(xr*1.<um>);y=(yr*1.<um>);z=(zr*1.<um>)},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 1.<second>, 1.<um>, 1.<pg um^-3>, 0.<second>, (PRNG.gaussianMargalisPolar' rng), true)
    {Physics.defaultParticle with id=gensym();name=name;location={x=(xr*1.<um>);y=(yr*1.<um>);z=(zr*1.<um>)};velocity={x=0.<um/second>;y=0.<um/second>;z=0.<um/second>};orientation={x=1.;y=0.;z=0.};Friction= 1.<second>;radius=1.<um>;density=1.<pg um^-3>;age=0.<second>;gRand=(PRNG.gaussianMargalisPolar' rng);freeze=true} 

let interfaceEventWriteFrame (file:StreamWriter) (register : string list) = 
    let rec clear_buffer (file:StreamWriter) (events:string list) =
        match events with
        | last_event::rest ->   file.WriteLine(last_event)
                                clear_buffer file rest
        | [] -> ()
    //use file = new StreamWriter(filename, true)
    register
    |> List.rev //Reverse the list order to ensure that backlogged events are correctly ordered. Need to test 150514
    |> clear_buffer file 
    //file.Close()
    
    
let xyzWriteFrame (file:StreamWriter) (machName: string) (system: Physics.Particle list)  =
        //use file = new StreamWriter(filename, true)
//        let mSystem = [for p in system do match System.String.Equals(p.name,machName) with 
//                                            | true -> yield p
//                                            | false -> ()
//                                            ]
        let mSystem = List.filter (fun (p:Physics.Particle) -> p.name=machName) system
        file.WriteLine(sprintf "%A" mSystem.Length)
        file.WriteLine("Athene")
        ignore [for p in mSystem -> file.WriteLine(sprintf "%s %A %A %A %A %A %A %A %A" p.name p.location.x p.location.y p.location.z p.radius p.age (p.pressure/1000000000.) p.confluence (p.forceMag/1000000000.))] 
//                
//        async { file.WriteLine(sprintf "%A" mSystem.Length)
//                file.WriteLine("Athene")
//                ignore [for p in mSystem -> file.WriteLine(sprintf "%s %A %A %A %A %A %A %A %A" p.name p.location.x p.location.y p.location.z p.radius p.age (p.pressure/1000000000.) p.confluence (p.forceMag/1000000000.))] 
//                }
//        |> Async.StartImmediate
        //file.Close()

let csvWriteStates (file:StreamWriter) (machines: Map<QN.var,int> list) = 
        //use file = new StreamWriter(filename, true)
        //[for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        //ignore [for p in system -> file.WriteLine(sprintf "%s %A %A %A" p.name p.location.x p.location.y p.location.z)]
        file.WriteLine(String.concat "," (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better
        //file.Close()

let dumpSystem (filename: string) (state: systemState) = 
        use file = new FileStream(filename, FileMode.Create)//new StreamWriter(filename, false)
        //file.WriteLine(machines.Length)
        //file.WriteLine(String.concat "," (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better
        //file.WriteLine(String.concat "," (seq { for p in particles -> p.ToString }) )
        //Create a mapless system state
        //This is because BinaryFormatter cannot serialize maps http://fpish.net/topic/None/59723
        let mapless = state.Formal |> List.map (fun m -> Map.toList m)
        let state' = {Physical=state.Physical;LFormal=mapless}
        let byteArray = convertObjectToByteArray(state')
        file.Write(byteArray,0,byteArray.Length)
        file.Close()
        ()

let pdbRead (filename: string) (rng: System.Random) =
    let atomParse (line: string) = 
        let name =    (line.Substring (11,5)).Trim()
        let x = float (line.Substring (30,8))
        let y = float (line.Substring (38,8))
        let z = float (line.Substring (46,8))
        cart2Particle (name, x,y,z, rng)
    [for line in File.ReadLines(filename) do match line with
                                                            | atom when atom.StartsWith("ATOM") -> yield atomParse line
                                                            | _ -> () ]

type runParameters = {
                        steps:int
                        temperature:float<Physics.Kelvin>
                        timestep:float<Physics.second>
                        pg:int
                        mg:int
                        ig:int
                        report:int
                        nonBond:float<Physics.um>
                        vdt:int
                        checkpointReport:int
                        }

let getProtection protectType protectValue =
    match protectType with
    | "LargerThan"              ->  let size = (float) protectValue
                                    RadiusMin(1.<Physics.um>*size)
    | "SmallerThan"             ->  let size = (float) protectValue
                                    RadiusMax(1.<Physics.um>*size)
    | "OlderThan"               ->  let age = (float) protectValue
                                    AgeMin(1.<Physics.second>*age)
    | "YoungerThan"             ->  let age = (float) protectValue
                                    AgeMax(1.<Physics.second>*age)
    | "PressureGreaterThan"     ->  let pressure = (float) protectValue
                                    PressureMin(1.<Physics.zNewton/Physics.um^2>*pressure)
    | "PressureLessThan"        ->  let pressure = (float) protectValue
                                    PressureMax(1.<Physics.zNewton/Physics.um^2>*pressure)
    | "ConfluenceGreaterThan"   ->  let conf = (int) protectValue
                                    ConfluenceMin(conf)
    | "ConfluenceLessThan"      ->  let conf = (int) protectValue
                                    ConfluenceMax(conf)
    | "ForceGreaterThan"        ->  let pressure = (float) protectValue
                                    PressureMin(1.<Physics.zNewton/Physics.um^2>*pressure)
    | "ForceLessThan"           ->  let pressure = (float) protectValue
                                    PressureMax(1.<Physics.zNewton/Physics.um^2>*pressure)
    | "None"                    ->  Unprotected
    | _                         ->  printf("Unrecognised apoptosis protection mechanism. Proceeding unprotected\n")
                                    Unprotected

let xmlTopRead (filename: string) =
    let xn s = XName.Get(s)
    let xd = XDocument.Load(filename)
    let maxMove = try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "MaxMove").Value) with _ -> failwith "Set a maximum move distance"
    let steps = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Steps").Value) with _ -> failwith "Set number of steps"
    let seed = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Seed").Value) with _ -> failwith "Set random seed"
    let temperature = try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Temperature").Value) with _ -> failwith "Set temperature"
    let timestep = try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Timestep").Value) with _ -> failwith "Set timestep"
    let pg = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "PG").Value) with _ -> failwith "Set physical time granularity"
    let mg = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "MG").Value) with _ -> failwith "Set machine time granularity"
    let ig = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "IG").Value) with _ -> failwith "Set interface time granularity"
    let vdt = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "VariabledTDepth").Value) with _ -> 0
    let report = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Report").Value) with _ -> 1
    let checkpoint = try (int) (xd.Element(xn "Topology").Element(xn "System").Element(xn "CheckpointFreq").Value) with _ -> 100 * report
    let nonBond = try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "NonBondedCutoff").Value) with _ -> failwith "Set non bonded cutoff"
    
    let rng = System.Random(seed)

    let rp = {steps=steps;temperature=(temperature*1.<Physics.Kelvin>);timestep=(timestep*1.<Physics.second>);pg=pg;mg=mg;ig=ig;vdt=vdt;report=report;nonBond=(nonBond*1.<Physics.um>);checkpointReport=checkpoint}

    let sOrigin = {x=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "X").Value) with _ -> failwith "Set an x origin")*1.<um>;y=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Y").Value) with _ -> failwith "Set a y origin")*1.<um>;z=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Z").Value) with _ -> failwith "Set a z origin")*1.<um>}
//    let PBC = 
//                { x=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "PBC").Attribute(xn "X").Value) with _ -> failwith "Set an x PBC dimension")*1.<um>;
//                y=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "PBC").Attribute(xn "Y").Value) with _ -> failwith "Set a y PBC dimension")*1.<um>;
//                z=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "PBC").Attribute(xn "Z").Value) with _ -> failwith "Set a z PBC dimension")*1.<um>}
    let pTypes = [ for t in xd.Element(xn "Topology").Element(xn "Types").Elements(xn "Particle") do 
                    let tName = try t.Attribute(xn "Name").Value with _ -> failwith "Cannot read name"
                    let tDensity = try (float) (t.Element(xn "Density").Value) with _ -> failwith "Cannot read density"
                    let tFric = try (float) (t.Element(xn "FrictionCoeff").Value) with _ -> failwith "Cannot read friction coefficient"
                    let tRadius = try (float) (t.Element(xn "Radius").Value) with _ -> failwith "Cannot read radius"
                    let tFreeze = match (t.Element(xn "Freeze").Value) with
                                  | "true" -> true
                                  | "false" -> false
                                  | _ -> failwith "Cannot read freeze"
                    //yield Particle(tName,{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},tFric*1.<second>, tRadius*1.<um>, tDensity*1.<pg um^-3>, tFreeze) ]
                    yield (tName,(tFric*1.<second>, tRadius*1.<um>, tDensity*1.<pg um^-3>, tFreeze)) ]
                    |> Map.ofList
    let nbTypes = [ for bi in xd.Element(xn "Topology").Element(xn "NonBonded").Elements(xn "Interaction") do
                    let biName = try bi.Attribute(xn "Name").Value with _ -> failwith "Cannot read type"
                    let biMap =  [ for bj in bi.Elements(xn "jInteraction") do 
                                    let bjName = try bj.Attribute(xn "Name").Value with _ -> failwith "Cannot read type"
                                    //let bond = try (int) (bj.Element(xn "Type").Value) with _ -> failwith "Missing bond type"
                                    let bond = match (try (int) (bj.Element(xn "Type").Value) with _ -> failwith "Missing bond type") with
                                        // SI: consider storing descriptive text here, rather than 0-4.     
                                                |0 -> noForce
                                                |1 -> 
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    hardSphereForce rP (rC*1.<zNewton>) 1. 0.<zNewton> 0.<um>
                                                |2 ->
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let aC = try (float) (bj.Element(xn "AttractCoeff").Value) with _ -> failwith "Missing attract constant"
                                                    let aP = try (float) (bj.Element(xn "AttractPower").Value) with _ -> failwith "Missing attract power"
                                                    let aCO = try (float) (bj.Element(xn "AttractCutOff").Value) with _ -> failwith "Missing attract cutoff"
                                                    hardSphereForce rP (rC*1.<zNewton>) aP (aC*1.<zNewton>) (aCO*1.<um>)
                                                |3 -> 
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    //(repelPower: float) (repelConstant: float<zNewton>) ( attractPower:float ) (attractConstant: float<zNewton>) (attractCutOff: float<um>)
                                                    softSphereForce rP (rC*1.<zNewton>) 1. 0.<zNewton> 0.<um>
                                                |4 ->
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let aC = try (float) (bj.Element(xn "AttractCoeff").Value) with _ -> failwith "Missing attract constant"
                                                    let aP = try (float) (bj.Element(xn "AttractPower").Value) with _ -> failwith "Missing attract power"
                                                    let aCO = try (float) (bj.Element(xn "AttractCutOff").Value) with _ -> failwith "Missing attract cutoff"
                                                    softSphereForce rP (rC*1.<zNewton>) aP (aC*1.<zNewton>) (aCO*1.<um>)
                                                |_ -> failwith "Incorrect type of nonbonded interaction"
                                    yield (bjName,bond) ] 
                                    |> Map.ofList
                    yield (biName,biMap)
                        ]
                    |> Map.ofList
    

    let regions = [ for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Region") do 
                         let oX = try (float) (r.Element(xn "Location").Attribute(xn "X").Value) with _ -> failwith "Missing box origin X"
                         let oY = try (float) (r.Element(xn "Location").Attribute(xn "Y").Value) with _ -> failwith "Missing box origin Y"
                         let oZ = try (float) (r.Element(xn "Location").Attribute(xn "Z").Value) with _ -> failwith "Missing box origin Z"
                         let dX = try (float) (r.Element(xn "Dimensions").Attribute(xn "X").Value) with _ -> failwith "Missing box dim X"
                         let dY = try (float) (r.Element(xn "Dimensions").Attribute(xn "Y").Value) with _ -> failwith "Missing box dim Y"
                         let dZ = try (float) (r.Element(xn "Dimensions").Attribute(xn "Z").Value) with _ -> failwith "Missing box dim Z"
                         let varID = try (int) (r.Element(xn "Var").Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                         let varState = try (int) (r.Element(xn "Var").Attribute(xn "State").Value) with _ -> failwith "Missing variable state"
                         yield ({origin={x=oX*1.<um>;y=oY*1.<um>;z=oZ*1.<um>};dimensions={x=dX*1.<um>;y=dY*1.<um>;z=dZ*1.<um>}},varID,varState)
                         ]
    let clocks = [for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Clock") do
                        let inputId = try (int) (r.Element(xn "Input").Attribute(xn "ID").Value) with _ -> failwith "Missing clock input ID"
                        let inputThreshold = try (int) (r.Element(xn "Input").Attribute(xn "Threshold").Value) with _ -> failwith "Missing clock threshold"
                        let inputTime = try (float) (r.Element(xn "Input").Attribute(xn "Time").Value) with _ -> failwith "Missing clock time limit"
                        
                        let OutputId = try (int) (r.Element(xn "Output").Attribute(xn "ID").Value) with _ -> failwith "Missing clock output ID"
                        let OutputState = try (int) (r.Element(xn "Output").Attribute(xn "State").Value) with _ -> failwith "Missing clock output state"
                        
                        yield ({Input=inputId;InputThreshold=inputThreshold;OutputID=OutputId;OutputState=OutputState;TimeLimit=inputTime*1.<Physics.second>})
                        
                         ]
    let responses = [ for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Response") do
                        let f = match r.Attribute(xn "Function").Value with
                                | ("LinearGrow" | "PressureLinearGrow" | "AgeLinearGrow" | "ForceLinearGrow" | "ConfluenceLinearGrow" ) as growth ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    match growth with
                                        | "ConfluenceLinearGrow" -> 
                                            let max  = try (int) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing maximum"
                                            limitedLinearGrow (rate*1.<um/second>) (ConfluenceLimit (max)) varID varState varName
                                        | _ ->
                                            let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing maximum"
                                            match growth with
                                                | "LinearGrow" ->
                                                    limitedLinearGrow (rate*1.<um/second>) (RadiusLimit (max*1.<um>)) varID varState varName
                                                | "PressureLinearGrow" ->
                                                    limitedLinearGrow (rate*1.<um/second>) (PressureLimit (max*1.<zNewton/um^2>)) varID varState varName
                                                | "AgeLinearGrow" ->
                                                    limitedLinearGrow (rate*1.<um/second>) (AgeLimit (max*1.<second>)) varID varState varName
                                                | "ForceLinearGrow" ->
                                                    limitedLinearGrow (rate*1.<um/second>) (ForceLimit (max*1.<zNewton>)) varID varState varName
                                                | _ -> failwith "Incorrect growth type"
                                //Linear, synchronous division process
                                | ( "LinearGrowDivide"| "PressureLimitedLinearGrowDivide" | "AgeLimitedLinearGrowDivide" | "ConfluenceLimitedLinearGrowDivide" | "ForceLimitedLinearGrowDivide" ) as growth ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    match growth with
                                        | "LinearGrowDivide" ->
                                            linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng None false                              
                                        | "ConfluenceLimitedLinearGrowDivide" ->
                                                    let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (ConfluenceLimit (limit))) false   
                                        | _ ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (PressureLimit (limit * 1.<zNewton/um^2>))) false                            
                                                | "AgeLimitedLinearGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (AgeLimit (limit * 1.<second>))) false                         
                                                | "ForceLimitedLinearGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (ForceLimit (limit * 1.<zNewton>))) false   
                                                | _ -> failwith "Incorrect growth type"
                                 //Unbiased, random division process
                                 | ("ProbabilisticGrowDivide" | "PressureLimitedProbabilisticGrowDivide" | "AgeLimitedProbabilisticGrowDivide" | "ConfluenceLimitedProbabilisticGrowDivide" | "ForceLimitedProbabilisticGrowDivide") as growth ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let sd = try (float) (r.Attribute(xn "SD").Value)  with _ -> failwith "Missing standard deviation of cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    match growth with
                                        | "ProbabilisticGrowDivide" ->
                                            linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng None true                            
                                        | "ConfluenceLimitedProbabilisticGrowDivide" ->
                                            let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing confluence limit"
                                            linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (ConfluenceLimit (limit))) true
                                        | _ as growth -> 
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (PressureLimit (limit * 1.<zNewton/um^2>))) true                            
                                                | "AgeLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (AgeLimit (limit * 1.<second>))) true                                                  
                                                | "ForceLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (ForceLimit (limit * 1.<zNewton>))) true    
                                                | _ -> failwith "Incorrect growth type"                   
                                //Linear, varying growth rates based on vector distance from a location
                                | ( "LinearGrowDivideVectorRate"| "PressureLimitedLinearGrowDivideVectorRate" | "AgeLimitedLinearGrowDivideVectorRate" | "ConfluenceLimitedLinearGrowDivideVectorRate" | "ForceLimitedLinearGrowDivideVectorRate" ) as growth ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    let gradient = try (float) (r.Attribute(xn "Gradient").Value)  with _ -> failwith "Missing variation of growth rate"
                                                    |> (fun x -> x*1.<Physics.second^-1>)
                                    let xOrigin = try (float) (r.Attribute(xn "xOrigin").Value)  with _ -> failwith "No origin- X"
                                    let yOrigin = try (float) (r.Attribute(xn "yOrigin").Value)  with _ -> failwith "No origin- Y"
                                    let zOrigin = try (float) (r.Attribute(xn "zOrigin").Value)  with _ -> failwith "No origin- Z"
                                    let origin = {x=(xOrigin*1.<Physics.um>);y=(yOrigin*1.<Physics.um>);z=(zOrigin*1.<Physics.um>)}
                                    let xVector = try (float) (r.Attribute(xn "xVector").Value)  with _ -> failwith "No vector- X"
                                    let yVector = try (float) (r.Attribute(xn "yVector").Value)  with _ -> failwith "No vector- Y"
                                    let zVector = try (float) (r.Attribute(xn "zVector").Value)  with _ -> failwith "No vector- Z"
                                    let direction = {x=xVector;y=yVector;z=zVector}
                                    match growth with
                                    | "LinearGrowDivideVectorRate" ->
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng None false  
                                    | "ConfluenceLimitedLinearGrowDivideVectorRate" ->
                                        let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (ConfluenceLimit (limit))) false   
                                    | _ as growth ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (PressureLimit (limit * 1.<zNewton/um^2>))) false                            
                                                | "AgeLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (AgeLimit (limit * 1.<second>))) false                         
                                                | "ForceLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (1.<um>) varID varState varName rng (Some (ForceLimit (limit * 1.<zNewton>))) false   
                                                | _ -> failwith "Incorrect growth type"                                    
                                //Random, varying growth rates based on vector distance from a location
                                | ( "LinearProbabilisticGrowDivideVectorRate"| "PressureLimitedLinearProbabilisticGrowDivideVectorRate" | "AgeLimitedLinearProbabilisticGrowDivideVectorRate" | "ConfluenceLimitedLinearProbabilisticGrowDivideVectorRate" | "ForceLimitedLinearProbabilisticGrowDivideVectorRate" ) as growth ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let sd = try (float) (r.Attribute(xn "SD").Value)  with _ -> failwith "Missing standard deviation of cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    let gradient = try (float) (r.Attribute(xn "Gradient").Value)  with _ -> failwith "Missing variation of growth rate"
                                                    |> (fun x -> x*1.<Physics.second^-1>)
                                    let xOrigin = try (float) (r.Attribute(xn "xOrigin").Value)  with _ -> failwith "No origin- X"
                                    let yOrigin = try (float) (r.Attribute(xn "yOrigin").Value)  with _ -> failwith "No origin- Y"
                                    let zOrigin = try (float) (r.Attribute(xn "zOrigin").Value)  with _ -> failwith "No origin- Z"
                                    let origin = {x=(xOrigin*1.<Physics.um>);y=(yOrigin*1.<Physics.um>);z=(zOrigin*1.<Physics.um>)}
                                    let xVector = try (float) (r.Attribute(xn "xVector").Value)  with _ -> failwith "No vector- X"
                                    let yVector = try (float) (r.Attribute(xn "yVector").Value)  with _ -> failwith "No vector- Y"
                                    let zVector = try (float) (r.Attribute(xn "zVector").Value)  with _ -> failwith "No vector- Z"
                                    let direction = {x=xVector;y=yVector;z=zVector}
                                    match growth with
                                    | "LinearProbabilisticGrowDivideVectorRate" ->
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng None true  
                                    | "ConfluenceLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                        let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (ConfluenceLimit (limit))) true   
                                    | _ as growth ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (PressureLimit (limit * 1.<zNewton/um^2>))) true                            
                                                | "AgeLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (AgeLimit (limit * 1.<second>))) true                         
                                                | "ForceLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState varName rng (Some (ForceLimit (limit * 1.<zNewton>))) true   
                                                | _ -> failwith "Incorrect growth type"                                    
                                
                                | "CertainDeath" ->
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    certainDeath varName
                                | "Apoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"  
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected
                                    apoptosis varID varState varName varProtection
                                | "LimitedApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"  
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing maximum number of deaths" 
                                    let apoptosis' = limitedApoptosis limit
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected
                                    apoptosis' varID varState varName varProtection
                                | "RandomApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let probModel = try (r.Attribute(xn "ProbabilityModel").Value) with _ -> failwith "Missing probability type" 
                                    let pType =  match probModel with 
                                                    | "Absolute" ->         let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            Absolute(p)
                                                    | "ModelledSingle" ->   let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            let time = try 1.<Physics.second>*(float) (r.Attribute(xn "PTime").Value) with _ -> failwith "Missing time associated with probability"
                                                                            ignore (Interface.pSlice p time (timestep*1.<Physics.second>)) //cache and check probability 
                                                                            ModelledSingle(p,time)
                                                    | _ -> failwith "Invalid probability type for this random mechanism"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected
                                    randomApoptosis varID varState varName varProtection rng pType
                                | "BiasedRandomApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let biasType = getBias (try (r.Attribute(xn "State").Value) with _ -> failwith "Missing probability type")
                                    let probModel = try (r.Attribute(xn "ProbabilityModel").Value) with _ -> failwith "Missing probability type" 
                                    let pType =  match probModel with 
                                                    | "Absolute" ->         let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            Absolute(p)
                                                    | "ModelledSingle" ->   let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            let time = try 1.<Physics.second>*(float) (r.Attribute(xn "PTime").Value) with _ -> failwith "Missing time associated with probability"
                                                                            ignore (Interface.pSlice p time (timestep*1.<Physics.second>)) //cache and check probability 
                                                                            ModelledSingle(p,time)
                                                    | _ -> failwith "Invalid probability type for this random mechanism"
                                    let power = try (float) (r.Attribute(xn "Power").Value) with _ -> failwith "Missing power of size dependence" 
                                    let refC =  try (float) (r.Attribute(xn "Constant").Value) with _ -> failwith "Missing reference constant"
                                    let refM =  try (float) (r.Attribute(xn "Gradient").Value) with _ -> failwith "Missing reference gradient"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected
                                    match biasType with
                                    | Radius ->     randomBiasApoptosis varID varState varName varProtection Radius rng power (refC*1.<um>) refM pType 
                                    | Age ->        randomBiasApoptosis varID varState varName varProtection Age rng power (refC*1.<Physics.second>) refM pType
                                    | Confluence -> randomBiasApoptosis varID varState varName varProtection Confluence rng power (refC*1.) refM pType
                                    | Force ->      randomBiasApoptosis varID varState varName varProtection Force rng power (refC*1.<zNewton>) refM pType
                                    | Pressure ->   randomBiasApoptosis varID varState varName varProtection Pressure rng power (refC*1.<zNewton um^-2>) refM pType   
                                | "RandomShrinkingApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let probModel = try (r.Attribute(xn "ProbabilityModel").Value) with _ -> failwith "Missing probability type" 
                                    let minSize = try (float) (r.Attribute(xn "DeathSize").Value) with _ -> failwith "Missing size for death"
                                    let shrinkRate = try (float) (r.Attribute(xn "ShrinkRate").Value) with _ -> failwith "Missing rate of death shrink"
                                    let pType =  match probModel with 
                                                    | "Absolute" ->         let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            Absolute(p)
                                                    | "ModelledMultiple" -> let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            let time = try 1.<Physics.second>*(float) (r.Attribute(xn "PTime").Value) with _ -> failwith "Missing time associated with probability"
                                                                            let totalShrink = try (float) (r.Attribute(xn "TotalShrink").Value) with _ -> failwith "Missing guess of average death shrink"
                                                                            ignore (Interface.multiStepProbability time (timestep*1.<Physics.second>) (shrinkRate*1.<Physics.um/Physics.second>) (totalShrink*1.<Physics.um>) p) //cache and check probability
                                                                            ModelledMultiple(p,time,(totalShrink*1.<Physics.um>))
                                                    | _ -> failwith "Invalid probability type for this random mechanism"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected  
                                    shrinkingRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) rng pType                    
                                | "BiasedRandomShrinkingApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let biasType = getBias (try (r.Attribute(xn "State").Value) with _ -> failwith "Missing probability type")
                                    let probModel = try (r.Attribute(xn "ProbabilityModel").Value) with _ -> failwith "Missing probability type" 
                                    let minSize = try (float) (r.Attribute(xn "DeathSize").Value) with _ -> failwith "Missing size for death"
                                    let shrinkRate = try (float) (r.Attribute(xn "ShrinkRate").Value) with _ -> failwith "Missing rate of death shrink"
                                    let pType =  match probModel with 
                                                    | "Absolute" ->         let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            Absolute(p)
                                                    | "ModelledMultiple" -> let p = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                                                            let time = try 1.<Physics.second>*(float) (r.Attribute(xn "PTime").Value) with _ -> failwith "Missing time associated with probability"
                                                                            let totalShrink = try (float) (r.Attribute(xn "TotalShrink").Value) with _ -> failwith "Missing guess of average death shrink"
                                                                            ignore (Interface.multiStepProbability time (timestep*1.<Physics.second>) (shrinkRate*1.<Physics.um/Physics.second>) (totalShrink*1.<Physics.um>) p) //cache and check probability
                                                                            ModelledMultiple(p,time,(totalShrink*1.<Physics.um>))
                                                    | _ -> failwith "Invalid probability type for this random mechanism"
                                    let power = try (float) (r.Attribute(xn "Power").Value) with _ -> failwith "Missing power of size dependence" 
                                    let refC =  try (float) (r.Attribute(xn "Constant").Value) with _ -> failwith "Missing reference constant"
                                    let refM =  try (float) (r.Attribute(xn "Gradient").Value) with _ -> failwith "Missing reference gradient"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name"   
                                    let protectLevel = try (r.Attribute(xn "ProtectionLevel").Value) with _ -> ""
                                    let varProtection = try (getProtection (r.Attribute(xn "ProtectionType").Value) protectLevel) with _ -> Unprotected
                                    match biasType with
                                    | Radius        -> shrinkingBiasRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) Radius rng power (refC*1.<um>) refM pType 
                                    | Age           -> shrinkingBiasRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) Age rng power (refC*1.<second>) refM pType
                                    | Pressure      -> shrinkingBiasRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) Pressure rng power (refC*1.<zNewton um^-2>) refM pType 
                                    | Force         -> shrinkingBiasRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) Force rng power (refC*1.<zNewton>) refM pType 
                                    | Confluence    -> shrinkingBiasRandomApoptosis varID varState varName varProtection (minSize*1.<Physics.um>) (shrinkRate*1.<Physics.um/Physics.second>) Confluence rng power (refC*1.) refM pType 
                                | _ -> failwith "Unknown function"

                        yield f
                        ]

    let motors = [ for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Motors") do
                        let f = match r.Attribute(xn "Function").Value with
                                | "BinaryMotor" ->
                                    let maxQN = try (int) (r.Attribute(xn "Max").Value) with _ -> failwith "Missing QN upper bound"
                                    let minProbOn  = try (float) (r.Attribute(xn "MinimumProbOn").Value)  with _ -> failwith "Missing min prob"
                                    let maxProbOn  = try (float) (r.Attribute(xn "MaximumProbOn").Value)  with _ -> failwith "Missing max prob"
                                    let time = try 1.<Physics.second>*(float) (r.Attribute(xn "PTime").Value) with _ -> failwith "Missing time associated with probability"
                                    let varIDProbability = try (int) (r.Attribute(xn "ProbId").Value) with _ -> failwith "Missing ID for variable which alters probability"
                                    let varIDMotor = try (int) (r.Attribute(xn "MotorId").Value) with _ -> failwith "Missing ID for motor variable"
                                    let forcemag  = try (float) (r.Attribute(xn "Force").Value)  with _ -> failwith "Force generated by motor"
                                    Interface.probabilisticBinaryMotor maxQN (minProbOn,maxProbOn) varIDProbability varIDMotor rng (forcemag*1.<Physics.zNewton>) time
                                | _ -> failwith "Unknown function"

                        yield f
                        ]

    let machName = try (xd.Element(xn "Topology").Element(xn "MachineCell").Attribute(xn "Name")).Value with _ -> failwith "Missing a machine cell"
    let machI0 = try (xd.Element(xn "Topology").Element(xn "MachineInit").Attribute(xn "State")).Value with _ -> failwith "Missing a machine cell"
    
    //let interfaceTopology = (machName,regions,responses)
    let intTop = {name=machName;regions=regions;clocks=clocks;responses=responses;randomMotors=motors}
    (pTypes,nbTypes,(machName,machI0),intTop,(sOrigin,maxMove),rp,rng)
    
let bmaRead (filename:string) = 
    Marshal.model_of_xml (XDocument.Load filename)