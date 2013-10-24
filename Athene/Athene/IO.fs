module IO

open System.IO
open Physics
open Vector

(*
'Spherical E. coli' particle
Particle({x=0.<um>;y=0.<um>;z=0.<um>},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 0.00002<second>, 0.7<um>, 1.3<pg um^-3>, false)
*)

let dropFrame (system: Physics.Particle list) =
    ()

let cart2Particle ((name:string), (xr:float), (yr:float), (zr:float)) = 
    Particle(name,{x=(xr*1.<um>);y=(yr*1.<um>);z=(zr*1.<um>)},{x=0.<um/second>;y=0.<um/second>;z=0.<um/second>}, 1.<second>, 1.<um>, 1.<pg um^-3>, false)

let xyzWriteFrame (filename: string) (system: Physics.Particle list) =
        use file = new StreamWriter(filename, true)
        file.WriteLine(sprintf "%A" system.Length)
        file.WriteLine("Athene")
        //[for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        ignore [for p in system -> file.WriteLine(sprintf "%A %A %A %A" 1 p.location.x p.location.y p.location.z)]
        file.Close()

let pdbRead (filename: string) =
    let atomParse (line: string) = 
        let name =    (line.Substring (11,5)).Trim()
        let x = float (line.Substring (30,8))
        let y = float (line.Substring (38,8))
        let z = float (line.Substring (46,8))
        cart2Particle (name, x,y,z)
    [for line in File.ReadLines(filename) do match line with
                                                            | atom when atom.StartsWith("ATOM") -> yield atomParse line
                                                            | _ -> () ]
    
    