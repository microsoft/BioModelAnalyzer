(* SI: give brief description of what module does. 
    Module IO: Read and Write xml files.... *)

module IO

open System.IO
open Physics
open Vector
open Interface
open System.Xml.Linq

(*
'Spherical E. coli' particle
Particle('E',{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 1.,  0.7<um>, 1.3<pg um^-3>, false)
Particle("E",{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 0.000000005<second>, 0.7<um>, 1.3<pg um^-3>, 1.<second>, 1., false)
*)

let dropFrame (system: Physics.Particle list) =
    ()

let dropStates (machines: Map<QN.var,int> list) =
    ()

let cart2Particle ((name:string), (xr:float), (yr:float), (zr:float), (rng:System.Random)) = 
    //Particle(gensym(),name,{x=(xr*1.<um>);y=(yr*1.<um>);z=(zr*1.<um>)},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, 1.<second>, 1.<um>, 1.<pg um^-3>, 0.<second>, (PRNG.gaussianMargalisPolar' rng), true)
    {id=gensym();name=name;location={x=(xr*1.<um>);y=(yr*1.<um>);z=(zr*1.<um>)};velocity={x=0.<um/second>;y=0.<um/second>;z=0.<um/second>};orientation={x=1.;y=0.;z=0.};Friction= 1.<second>;radius=1.<um>;density=1.<pg um^-3>;age=0.<second>;gRand=(PRNG.gaussianMargalisPolar' rng);freeze=true} 
    
let xyzWriteFrame (filename: string) (machName: string) (system: Physics.Particle list) =
        use file = new StreamWriter(filename, true)
        let mSystem = [for p in system do match System.String.Equals(p.name,machName) with 
                                            | true -> yield p
                                            | false -> ()
                                            ]
        file.WriteLine(sprintf "%A" mSystem.Length)
        file.WriteLine("Athene")
        //[for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        ignore [for p in mSystem -> file.WriteLine(sprintf "%s %A %A %A %A %A" p.name p.location.x p.location.y p.location.z p.radius p.age)]
        file.Close()

let csvWriteStates (filename: string) (machines: Map<QN.var,int> list) = 
        use file = new StreamWriter(filename, true)
        //[for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        //ignore [for p in system -> file.WriteLine(sprintf "%s %A %A %A" p.name p.location.x p.location.y p.location.z)]
        file.WriteLine(String.concat "," (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better
        file.Close()

let dumpSystem (filename: string) (particles: Physics.Particle list) (machines: Map<QN.var,int> list) = 
        use file = new StreamWriter(filename, false)
        file.WriteLine(machines.Length)
        file.WriteLine(String.concat "," (List.map (fun m -> Map.fold (fun s k v -> s + ";" + (string)k + "," + (string)v) "" m) machines)) //This will be *impossible* to read. Must do better
        file.WriteLine(String.concat "," (seq { for p in particles -> p.ToString }) )
        file.Close()

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

let xmlTopRead (filename: string) (rng: System.Random) =
    let xn s = XName.Get(s)
    let xd = XDocument.Load(filename)
    let maxMove = try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "MaxMove").Value) with _ -> failwith "Set a maximum move distance"
    let sOrigin = {x=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "X").Value) with _ -> failwith "Set an x origin")*1.<um>;y=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Y").Value) with _ -> failwith "Set a y origin")*1.<um>;z=(try (float) (xd.Element(xn "Topology").Element(xn "System").Element(xn "Origin").Attribute(xn "Z").Value) with _ -> failwith "Set a z origin")*1.<um>}
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
    let responses = [ for r in xd.Element(xn "Topology").Element(xn "Interface").Elements(xn "Response") do
                        let f = match r.Attribute(xn "Function").Value with
                                | "LinearGrow" ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"
                                    linearGrow (rate*1.<um/second>) (max*1.<um>) varID varState
                                | "LinearGrowDivide" ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    linearGrowDivide (rate*1.<um/second>) (max*1.<um>) varID varState rng                                
                                 | "ProbabilisticGrowDivide" ->
                                    let rate = try (float) (r.Attribute(xn "Rate").Value) with _ -> failwith "Missing growth rate"
                                    let max  = try (float) (r.Attribute(xn "Max").Value)  with _ -> failwith "Missing max cell size"
                                    let sd = try (float) (r.Attribute(xn "SD").Value)  with _ -> failwith "Missing standard deviation of cell size"
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    probabilisticGrowDivide (rate*1.<um/second>) (max*1.<um>) (sd*1.<um>) varID varState rng                                
                                | "Apoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    apoptosis varID varState                                 
                                | "RandomApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let probability = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death" 
                                    randomApoptosis varID varState rng (probability*1.<Physics.second^-1>)
                                | "SizeRandomApoptosis" ->
                                    let varID = try (int) (r.Attribute(xn "Id").Value) with _ -> failwith "Missing variable ID"
                                    let varState = try (int) (r.Attribute(xn "State").Value) with _ -> failwith "Missing variable state"   
                                    let probability = try (float) (r.Attribute(xn "Probability").Value) with _ -> failwith "Missing probability of death"
                                    let power = try (float) (r.Attribute(xn "Power").Value) with _ -> failwith "Missing power of size dependence" 
                                    randomSizeApoptosis varID varState rng power (probability*1.<Physics.second^-1>)                    
                                | _ -> failwith "Unknown function"

                        yield f
                        ]
    let machName = try (xd.Element(xn "Topology").Element(xn "MachineCell").Attribute(xn "Name")).Value with _ -> failwith "Missing a machine cell"
    let machI0 = try (xd.Element(xn "Topology").Element(xn "MachineInit").Attribute(xn "State")).Value with _ -> failwith "Missing a machine cell"
    
    //let interfaceTopology = (machName,regions,responses)
    let intTop = {name=machName;regions=regions;responses=responses}
    (pTypes,nbTypes,(machName,machI0),intTop,(sOrigin,maxMove))
    
//let topRead (filename: string) =
//    topology files describe the basic forces in the system
//    They are csvs in sections with the following format
//    System,Nonbonded cutoff, nlupdate
//    Types,name,frictioncoeff,radius,density,freeze
//    NonBonded,name,name,type,a,b,c,d
//      ->where a,b,c and d are parameters for the energy function
//    Tissues,name,particles,bonds,bondtype
//    Contents,name,number
//    let lineParse (line:string) =
//        let elements = (line.Split[|','|])
//        match Array.get elements 0 with 
//        | "System"    -> ()
//        | "Type"      -> Particle((Array.get elements 1),{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, (float (Array.get elements 2) ) *1.<second>, (float (Array.get elements 3) ) *1.<um>, (float (Array.get elements 4) ) *1.<pg um^-3>, (bool (Array.get elements 5)))
//        | "NonBonded" -> ()
//        | "Tissues"   -> ()
//        | _ -> ()
//        ()
//    let Bdict = Map
//    let AddFuncToDict (a: string) (b: string) F (D : Map) =
//        let LocalMap = Map (b, F) 
//        D.Add (a,LocalMap)
//    let Bonds = (Physics.hardSphereForce, Physics.hardStickySphereForce)
//    let PTypes = [for item in File.ReadLines(filename) do match (item.Split[|','|]) with 
//                                                                | elements when (Array.get elements 0) = "Type" -> yield Particle((Array.get elements 1),{x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>},{x=1.;y=0.;z=0.}, (float (Array.get elements 2) ) *1.<second>, (float (Array.get elements 3) ) *1.<um>, (float (Array.get elements 4) ) *1.<pg um^-3>, (System.String.Equals((Array.get elements 5),0.<second>,"true")))
//                                                                | _ -> ()
//                                                                ]
//    let BTypes = [for item in File.ReadLines(filename) do match (item.Split[|','|]) with 
//                                                                | elements when (Array.get elements 0) = "NonBonded" -> yield match (int (Array.get elements 1)) with
//                                                                                                                                | 0 -> Physics.hardSphereForce ((float (Array.get elements 5))*1.0<aNewton>)
//                                                                                                                                | 1 -> Physics.hardStickySphereForce ((float (Array.get elements 5))*1.0<aNewton>) ((float (Array.get elements 5))*1.0<aNewton/um>) ((float (Array.get elements 5))*1.0<um>)
//                                                                                                                                | _ -> failwith "Bad NonBonded Topology"
//                                                                | _ -> () ]
//    PTypes

let bmaRead (filename:string) = 
    Marshal.model_of_xml (XDocument.Load filename)