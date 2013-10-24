module IO

open System.IO

let dropFrame (system: Physics.Particle list) =
    ()

let xyzWriteFrame (filename: string) (system: Physics.Particle list) =
        use file = new StreamWriter(filename, true)
        file.WriteLine(sprintf "%A" system.Length)
        file.WriteLine("Athene")
        //[for p in system -> printfn "%A %A %A %A" 1 p.location.x p.location.y p.location.z]
        ignore [for p in system -> file.WriteLine(sprintf "%A %A %A %A" 1 p.location.x p.location.y p.location.z)]
        file.Close()

let pdbRead (filename: string) =
        use file = new StreamReader(filename)
        let system = [for line in file.ReadLine() -> line]
        file.Close()
        system