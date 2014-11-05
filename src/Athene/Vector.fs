module Vector

type Vector3D< [<Measure>] 'u> = 
    struct 
        val x: float<'u>
        val y: float<'u>
        val z: float<'u>
        new (X:float<'u>,Y:float<'u>,Z:float<'u>) = {x=X;y=Y;z=Z}
        static member (-) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = new Vector3D<'u>(v1.x-v2.x, v1.y-v2.y, v1.z-v2.z)//{x = v1.x-v2.x; y = v1.y-v2.y; z = v1.z-v2.z}
        static member (+) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = new Vector3D<'u>(v1.x+v2.x, v1.y+v2.y, v1.z+v2.z)//{x = v1.x+v2.x; y = v1.y+v2.y; z = v1.z+v2.z}
        //scalar product
        static member (*) (v1 : Vector3D<'a>, v2 : Vector3D<'b>) = v1.x*v2.x + v1.y*v2.y + v1.z*v2.z 
        static member (*) (v1 : Vector3D<'u>, s : float<'a>) = new Vector3D<'u 'a>(v1.x*s,v1.y*s,v1.z*s) //{x = v1.x*s; y = v1.y*s; z = v1.z*s}
        static member (*) (s : float<'a>, v1 : Vector3D<'u>) = new Vector3D<'u 'a>(v1.x*s,v1.y*s,v1.z*s) //{x = v1.x*s; y = v1.y*s; z = v1.z*s}
        static member (.^) (v1: Vector3D<'a>, v2: Vector3D<'b>) = new Vector3D<'a 'b>((v1.y*v2.z-v1.z*v2.y),(v1.z*v2.x-v1.x*v2.z),(v1.x*v2.y-v1.y*v2.x))////{x = v1.y*v2.z-v1.z*v2.y; y = v1.z*v2.x-v1.x*v2.z; z = v1.x*v2.y-v1.y*v2.x }
        member this.len = sqrt(this.x*this.x + this.y*this.y + this.z*this.z)
        member this.norm = 1./this.len * this
        static member (%) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = acos (v1.norm * v2.norm)
    end

type Cuboid< [<Measure>] 'u> = { origin: Vector3D<'u>; dimensions: Vector3D<'u> } with
    member this.volume = this.dimensions.x*this.dimensions.y*this.dimensions.z

let randomDirectionUnitVector (rng: System.Random) =
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    ( new Vector3D<1>(List.nth rNum 0,List.nth rNum 1,List.nth rNum 2) ).norm
