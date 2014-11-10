(*  Module IO: Read and Write files
    Does some checking of file format and tests input values
 *)

module IO

open System
open System.Collections.Generic
open System.IO
open Physics
//open Vector
open Interface
open System.Xml.Linq
open System.Runtime.Serialization.Formatters.Binary

(*
'Spherical E. coli' particle
Particle('E',{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 1.,  0.7<um>, 1.3<pg um^-3>, false)
Particle("E",{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 0.7<um>, 1.3<pg um^-3>, 1.<second>, 1., false)
*)
[<Serializable>]
type systemState = {Physical: Physics.Particle array; Formal: Map<QN.var,int> array}
[<Serializable>]
type systemStorage = {Physical: Physics.Particle array; LFormal: (QN.var*int) array array}

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
    let mapped = state'.LFormal |> Array.map (fun m -> Map.ofArray m) 
    let p = state'.Physical
    //Read the maximum gensym and sync the current gensym to that
    let maxG = p |> Array.map (fun i -> i.id) |> Array.max
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
    new Particle(gensym(),name,new Vector.Vector3D<um>((xr*1.<um>),(yr*1.<um>),(zr*1.<um>)),
                        {Physics.defaultParticleInfo with 
                            //id=gensym();
                            //name=name;
                            //location= new Vector.Vector3D<um>((xr*1.<um>),(yr*1.<um>),(zr*1.<um>));
                            velocity= new Vector.Vector3D<um/second>()//{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>};
                            orientation= new Vector.Vector3D<1>(1.,0.,0.);//{x=1.;y=0.;z=0.};
                            Friction= 1.<second>;
                            radius=1.<um>;
                            density=1.<pg um^-3>;
                            age=0.<second>;
                            gRand=(PRNG.gaussianMargalisPolar' rng);
                            freeze=true
                            } )

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

    
    
let xyzWriteFrame (file:StreamWriter) (machName: string) (system: Physics.Particle array)  =
        let mSystem = Array.filter (fun (p:Physics.Particle) -> p.name=machName) system
        file.WriteLine(sprintf "%A" mSystem.Length)
        file.WriteLine("Athene")
        ignore [for p in mSystem -> file.WriteLine(sprintf "%s %A %A %A %A %A %A %A %A" p.name p.location.x p.location.y p.location.z p.details.radius p.details.age (p.details.pressure/1000000000.) p.details.confluence (p.details.forceMag/1000000000.))] 


let csvWriteStates (file:StreamWriter) (machines: Map<QN.var,int> array) = 
        file.WriteLine(String.concat "," (Array.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better

let dumpSystem (filename: string) (state: systemState) = 
        use file = new FileStream(filename, FileMode.Create)//new StreamWriter(filename, false)
        //file.WriteLine(machines.Length)
        //file.WriteLine(String.concat "," (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better
        //file.WriteLine(String.concat "," (seq { for p in particles -> p.ToString }) )
        //Create a mapless system state
        //This is because BinaryFormatter cannot serialize maps http://fpish.net/topic/None/59723
        let mapless = state.Formal |> Array.map (fun m -> Map.toArray m)
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
                        searchType:searchType
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
    let searchType =    match (try (xd.Element(xn "Topology").Element(xn "System").Element(xn "SearchType").Value) with _ -> failwith "No defined searchtype") with
                        | "Simple" -> Simple
                        | "Grid" -> Grid 
                        | _ -> failwith "No defined searchtype"
    
    let rng = System.Random(seed)

    let rp = {steps=steps;temperature=(temperature*1.<Physics.Kelvin>);timestep=(timestep*1.<Physics.second>);pg=pg;mg=mg;ig=ig;vdt=vdt;report=report;nonBond=(nonBond*1.<Physics.um>);checkpointReport=checkpoint;searchType=searchType}

    let sOrigin = new Vector.Vector3D<um>(  (try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "X").Value) with _ -> failwith "Set an x origin")*1.<um>,
                                            (try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Y").Value) with _ -> failwith "Set a y origin")*1.<um>,
                                            (try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Z").Value) with _ -> failwith "Set a z origin")*1.<um>)
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
                    yield (tName,(tFric*1.<second>, tRadius*1.<um>, tDensity*1.<pg um^-3>, tFreeze)) ]
                    |> Map.ofList
    let nbTypes = [ for bi in xd.Element(xn "Topology").Element(xn "NonBonded").Elements(xn "Interaction") do
                    let biName = try bi.Attribute(xn "Name").Value with _ -> failwith "Cannot read type"
                    let biMap =  [ for bj in bi.Elements(xn "jInteraction") do 
                                    let bjName = try bj.Attribute(xn "Name").Value with _ -> failwith "Cannot read type"
                                    let bond = match (try (int) (bj.Element(xn "Type").Value) with _ -> failwith "Missing bond type") with
                                        // SI: consider storing descriptive text here, rather than 0-4.     
                                                |0 -> noForce
                                                |1 -> 
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let powerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let repelPower = match powerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a
                                                    
                                                    hardSphereForce repelPower (rC*1.<zNewton>) (Physics.IntPower(1)) 0.<zNewton> 0.<um>
                                                |2 ->
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let aC = try (float) (bj.Element(xn "AttractCoeff").Value) with _ -> failwith "Missing attract constant"
                                                    let aP = try (float) (bj.Element(xn "AttractPower").Value) with _ -> failwith "Missing attract power"
                                                    let aCO = try (float) (bj.Element(xn "AttractCutOff").Value) with _ -> failwith "Missing attract cutoff"
                                                    let repelPowerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let repelPower = match repelPowerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a
                                                    let attractPowerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let attractPower = match attractPowerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a
                                                    hardSphereForce repelPower (rC*1.<zNewton>) attractPower (aC*1.<zNewton>) (aCO*1.<um>)
                                                |3 -> 
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let repelPowerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let repelPower = match repelPowerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a
                                                    softSphereForce repelPower (rC*1.<zNewton>) (Physics.IntPower(1)) 0.<zNewton> 0.<um>
                                                |4 ->
                                                    let rC = try (float) (bj.Element(xn "RepelCoeff").Value) with _ -> failwith "Missing repel constant"
                                                    let rP = try (float) (bj.Element(xn "RepelPower").Value) with _ -> failwith "Missing repel power"
                                                    let aC = try (float) (bj.Element(xn "AttractCoeff").Value) with _ -> failwith "Missing attract constant"
                                                    let aP = try (float) (bj.Element(xn "AttractPower").Value) with _ -> failwith "Missing attract power"
                                                    let aCO = try (float) (bj.Element(xn "AttractCutOff").Value) with _ -> failwith "Missing attract cutoff"
                                                    let repelPowerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let repelPower = match repelPowerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a
                                                    let attractPowerType = try (bj.Element(xn "RepelPower").Attribute(xn "Type").Value) with _ -> failwith "Missing power type"
                                                    let attractPower = match attractPowerType with
                                                                        | "Float" -> Physics.FloatPower(rP)
                                                                        | "Int"   -> Physics.IntPower(int rP)
                                                                        | a -> failwith "Unrecognised power type- %a" a                                                    
                                                    softSphereForce repelPower (rC*1.<zNewton>) attractPower (aC*1.<zNewton>) (aCO*1.<um>)
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
                         yield ({Vector.Cuboid.origin= new Vector.Vector3D<um>(oX*1.<um>,oY*1.<um>,oZ*1.<um>);Vector.Cuboid.dimensions=new Vector.Vector3D<um>(dX*1.<um>,dY*1.<um>,dZ*1.<um>)},varID,varState)
                         ]
    let clocks = [for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Clock") do
                        let inputId = try (int) (r.Element(xn "Input").Attribute(xn "ID").Value) with _ -> failwith "Missing clock input ID"
                        let inputThreshold = try (int) (r.Element(xn "Input").Attribute(xn "Threshold").Value) with _ -> failwith "Missing clock threshold"
                        let inputTime = try (float) (r.Element(xn "Input").Attribute(xn "Time").Value) with _ -> failwith "Missing clock time limit"
                        
                        let OutputId = try (int) (r.Element(xn "Output").Attribute(xn "ID").Value) with _ -> failwith "Missing clock output ID"
                        let OutputState = try (int) (r.Element(xn "Output").Attribute(xn "State").Value) with _ -> failwith "Missing clock output state"
                        
                        yield ({Input=inputId;InputThreshold=inputThreshold;OutputID=OutputId;OutputState=OutputState;TimeLimit=inputTime*1.<Physics.second>})
                        
                         ]
 
    let getGrowthBasics (r:XElement) =
                                    let growthStyle =   try (r.Attribute(xn "GrowthStyle").Value) with _ -> "Radial" 
                                    let rate =  match growthStyle with 
                                                | "Radial" -> Interface.RadialGrowthType((try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate")*1.<Physics.um/Physics.second>)
                                                | "Volume" -> Interface.VolumeGrowthType((try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate")*1.<Physics.um^3/Physics.second>)
                                                | a -> failwith ("Unrecognised growth type- " + a )
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"
                                    let varName = try r.Attribute(xn "Name").Value with _ -> failwith "Missing variable name" 
                                    {rate=rate;varID=varID;varState=varState;varName=varName;limit=NoLimit}

    let responses = [ for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Response") do
                        let f = match r.Attribute(xn "Function").Value with
                                | ("LinearGrow" | "PressureLinearGrow" | "AgeLinearGrow" | "ForceLinearGrow" | "ConfluenceLinearGrow" ) as growth ->
                                    let growthInfo = getGrowthBasics r 
                                    match growth with
                                        | "ConfluenceLinearGrow" -> 
                                            let max  = try (int) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing maximum"
                                            limitedLinearGrow {growthInfo with limit=ConfluenceLimit(max)}
                                        | _ ->
                                            let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing maximum"
                                            match growth with
                                                | "LinearGrow" ->
                                                    limitedLinearGrow {growthInfo with limit=RadiusLimit(max*1.<um>)}
                                                | "PressureLinearGrow" ->
                                                    limitedLinearGrow {growthInfo with limit=PressureLimit (max*1.<zNewton/um^2>)}
                                                | "AgeLinearGrow" ->
                                                    limitedLinearGrow {growthInfo with limit=AgeLimit (max*1.<second>)}
                                                | "ForceLinearGrow" ->
                                                    limitedLinearGrow {growthInfo with limit=ForceLimit (max*1.<zNewton>)}
                                                | _ -> failwith "Incorrect growth type"
                                //Linear, synchronous division process
                                | ( "LinearGrowDivide"| "PressureLimitedLinearGrowDivide" | "AgeLimitedLinearGrowDivide" | "ConfluenceLimitedLinearGrowDivide" | "ForceLimitedLinearGrowDivide" ) as growth ->
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let growthInfo = getGrowthBasics r 
                                    match growth with
                                        | "LinearGrowDivide" ->
                                            linearGrowDivide growthInfo (max*1.<um>) (1.<um>) rng false                              
                                        | "ConfluenceLimitedLinearGrowDivide" ->
                                                    let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                                    linearGrowDivide {growthInfo with limit=ConfluenceLimit (limit)} (max*1.<um>) (1.<um>) rng false   
                                        | _ ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=PressureLimit (limit * 1.<zNewton/um^2>)} (max*1.<um>) (1.<um>) rng false                            
                                                | "AgeLimitedLinearGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=AgeLimit (limit * 1.<second>)} (max*1.<um>) (1.<um>) rng false                         
                                                | "ForceLimitedLinearGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=ForceLimit (limit * 1.<zNewton>)} (max*1.<um>) (1.<um>) rng false   
                                                | _ -> failwith "Incorrect growth type"
                                 //Unbiased, random division process
                                 | ("ProbabilisticGrowDivide" | "PressureLimitedProbabilisticGrowDivide" | "AgeLimitedProbabilisticGrowDivide" | "ConfluenceLimitedProbabilisticGrowDivide" | "ForceLimitedProbabilisticGrowDivide") as growth ->
                                    let growthInfo = getGrowthBasics r 
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let sd = try (float) (r.Attribute(xn "SD").Value)  with _ -> failwith "Missing standard deviation of cell size"
                                    match growth with
                                        | "ProbabilisticGrowDivide" ->
                                            linearGrowDivide growthInfo (max*1.<um>) (sd*1.<um>) rng true                            
                                        | "ConfluenceLimitedProbabilisticGrowDivide" ->
                                            let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing confluence limit"
                                            linearGrowDivide {growthInfo with limit=ConfluenceLimit (limit)} (max*1.<um>) (sd*1.<um>) rng true
                                        | _ as growth -> 
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=PressureLimit (limit * 1.<zNewton/um^2>)} (max*1.<um>) (sd*1.<um>) rng true                            
                                                | "AgeLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=AgeLimit (limit * 1.<second>)} (max*1.<um>) (sd*1.<um>) rng true                                                  
                                                | "ForceLimitedProbabilisticGrowDivide" ->
                                                    linearGrowDivide {growthInfo with limit=ForceLimit (limit * 1.<zNewton>)} (max*1.<um>) (sd*1.<um>) rng true    
                                                | _ -> failwith "Incorrect growth type"                   
                                //Linear, varying growth rates based on vector distance from a location
                                | ( "LinearGrowDivideVectorRate"| "PressureLimitedLinearGrowDivideVectorRate" | "AgeLimitedLinearGrowDivideVectorRate" | "ConfluenceLimitedLinearGrowDivideVectorRate" | "ForceLimitedLinearGrowDivideVectorRate" ) as growth ->
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let growthInfo = getGrowthBasics r
                                    let gradient = try (float) (r.Attribute(xn "Gradient").Value)  with _ -> failwith "Missing variation of growth rate"
                                                    |> (fun x -> x*1.<Physics.second^-1>)
                                    let xOrigin = try (float) (r.Attribute(xn "xOrigin").Value)  with _ -> failwith "No origin- X"
                                    let yOrigin = try (float) (r.Attribute(xn "yOrigin").Value)  with _ -> failwith "No origin- Y"
                                    let zOrigin = try (float) (r.Attribute(xn "zOrigin").Value)  with _ -> failwith "No origin- Z"
                                    let origin = new Vector.Vector3D<um>(xOrigin*1.<um>,yOrigin*1.<um>,zOrigin*1.<um>)//{Vector.Vector3D.x=(xOrigin*1.<Physics.um>);Vector.Vector3D.y=(yOrigin*1.<Physics.um>);Vector.Vector3D.z=(zOrigin*1.<Physics.um>)}
                                    let xVector = try (float) (r.Attribute(xn "xVector").Value)  with _ -> failwith "No vector- X"
                                    let yVector = try (float) (r.Attribute(xn "yVector").Value)  with _ -> failwith "No vector- Y"
                                    let zVector = try (float) (r.Attribute(xn "zVector").Value)  with _ -> failwith "No vector- Z"
                                    let direction = new Vector.Vector3D<1>(xVector,yVector,zVector)//{Vector.Vector3D.x=xVector;Vector.Vector3D.y=yVector;Vector.Vector3D.z=zVector}
                                    match growth with
                                    | "LinearGrowDivideVectorRate" ->
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient growthInfo (max*1.<um>) (1.<um>) rng false  
                                    | "ConfluenceLimitedLinearGrowDivideVectorRate" ->
                                        let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=ConfluenceLimit (limit)} (max*1.<um>) (1.<um>) rng false   
                                    | _ as growth ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=PressureLimit (limit * 1.<zNewton/um^2>)} (max*1.<um>) (1.<um>) rng false                            
                                                | "AgeLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=AgeLimit (limit * 1.<second>)} (max*1.<um>) (1.<um>) rng false                         
                                                | "ForceLimitedLinearGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=ForceLimit (limit * 1.<zNewton>)} (max*1.<um>) (1.<um>) rng false   
                                                | _ -> failwith "Incorrect growth type"                                    
                                //Random, varying growth rates based on vector distance from a location
                                | ( "LinearProbabilisticGrowDivideVectorRate"| "PressureLimitedLinearProbabilisticGrowDivideVectorRate" | "AgeLimitedLinearProbabilisticGrowDivideVectorRate" | "ConfluenceLimitedLinearProbabilisticGrowDivideVectorRate" | "ForceLimitedLinearProbabilisticGrowDivideVectorRate" ) as growth ->
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let sd = try (float) (r.Attribute(xn "SD").Value)  with _ -> failwith "Missing standard deviation of cell size"
                                    let growthInfo = getGrowthBasics r
                                    let gradient = try (float) (r.Attribute(xn "Gradient").Value)  with _ -> failwith "Missing variation of growth rate"
                                                    |> (fun x -> x*1.<Physics.second^-1>)
                                    let xOrigin = try (float) (r.Attribute(xn "xOrigin").Value)  with _ -> failwith "No origin- X"
                                    let yOrigin = try (float) (r.Attribute(xn "yOrigin").Value)  with _ -> failwith "No origin- Y"
                                    let zOrigin = try (float) (r.Attribute(xn "zOrigin").Value)  with _ -> failwith "No origin- Z"
                                    let origin = new Vector.Vector3D<um>(xOrigin*1.<um>,yOrigin*1.<um>,zOrigin*1.<um>)//{Vector.Vector3D.x=(xOrigin*1.<Physics.um>);Vector.Vector3D.y=(yOrigin*1.<Physics.um>);Vector.Vector3D.z=(zOrigin*1.<Physics.um>)}
                                    let xVector = try (float) (r.Attribute(xn "xVector").Value)  with _ -> failwith "No vector- X"
                                    let yVector = try (float) (r.Attribute(xn "yVector").Value)  with _ -> failwith "No vector- Y"
                                    let zVector = try (float) (r.Attribute(xn "zVector").Value)  with _ -> failwith "No vector- Z"
                                    let direction = new Vector.Vector3D<1>(xVector,yVector,zVector)//{Vector.Vector3D.x=xVector;Vector.Vector3D.y=yVector;Vector.Vector3D.z=zVector}
                                    match growth with
                                    | "LinearProbabilisticGrowDivideVectorRate" ->
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient growthInfo (max*1.<um>) (sd*1.<um>) rng true  
                                    | "ConfluenceLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                        let limit = try (int) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                        Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=ConfluenceLimit (limit)} (max*1.<um>) (sd*1.<um>) rng true   
                                    | _ as growth ->
                                            let limit = try (float) (r.Attribute(xn "Limit").Value) with _ -> failwith "Missing limit"
                                            match growth with
                                                | "PressureLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=PressureLimit (limit * 1.<zNewton/um^2>)} (max*1.<um>) (sd*1.<um>) rng true                            
                                                | "AgeLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=AgeLimit (limit * 1.<second>)} (max*1.<um>) (sd*1.<um>) rng true                         
                                                | "ForceLimitedLinearProbabilisticGrowDivideVectorRate" ->
                                                    Interface.linearGrowDivideWithVectorDistanceDependence origin direction gradient {growthInfo with limit=ForceLimit (limit * 1.<zNewton>)} (max*1.<um>) (sd*1.<um>) rng true   
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