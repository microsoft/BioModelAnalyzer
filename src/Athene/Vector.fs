module Vector

(*
   static member (~-) (v : Vector) =
     Vector(-1.0 * v.x, -1.0 * v.y)
   static member (*) (v : Vector, a) =
     Vector(a * v.x, a * v.y)
   static member (*) (a, v: Vector) =
     Vector(a * v.x, a * v.y)
   override this.ToString() =
     this.x.ToString() + " " + this.y.ToString()
     *)

type Vector3D< [<Measure>] 'u> = { x: float<'u>; y: float<'u>; z: float<'u> } with 
    //static member (%-) (v1 : Vector3D<'u>) = sqrt(v1.x*v1.x + v1.y*v1.y + v1.z*v1.z)
    static member (-) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = {x = v1.x-v2.x; y = v1.y-v2.y; z = v1.z-v2.z}
    //static member (~-) (v1 : Vector3D<'u>) = {x = -v1.x; y = -v1.y; z = -v1.z}
    static member (+) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = {x = v1.x+v2.x; y = v1.y+v2.y; z = v1.z+v2.z}
    static member (*) (v1 : Vector3D<_>, v2 : Vector3D<_>) = v1.x*v2.x + v1.y*v2.y + v1.z*v2.z
    static member (*) (v1 : Vector3D<'u>, s : float<_>) = {x = v1.x*s; y = v1.y*s; z = v1.z*s}
    static member (*) (s : float<_>, v1 : Vector3D<'u>) = {x = v1.x*s; y = v1.y*s; z = v1.z*s}
    static member (.^) (v1: Vector3D<'u>, v2: Vector3D<'u>) = {x = v1.y*v2.z-v1.z*v2.y; y = v1.z*v2.x-v1.x*v2.z; z = v1.x*v2.y-v1.y*v2.x }
    //static member (~&) (v1 : Vector3D<'u>) = v1 * {x=1.;y=1.;z=1.}
    member this.len = sqrt(this.x*this.x + this.y*this.y + this.z*this.z)
    member this.norm = 1./this.len * this
    static member (%) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = acos (v1.norm * v2.norm)

type Cuboid< [<Measure>] 'u> = { origin: Vector3D<'u>; dimensions: Vector3D<'u> } with
    member this.volume = this.dimensions.x*this.dimensions.y*this.dimensions.z

let randomDirectionUnitVector (rng: System.Random) =
    let rNum = PRNG.nGaussianRandomMP rng 0. 1. 3
    { x= (List.nth rNum 0) ; y= (List.nth rNum 1); z= (List.nth rNum 2)}.norm

let smallestElements (v1: Vector3D<_>) (v2: Vector3D<_>) =
    match v1 with
    | v when v.x <= v2.x && v.y <= v2.y && v.z <= v2.z -> v                        //<<<
    | v when v.x <= v2.x && v.y <= v2.y && v.z >  v2.z -> {x=v.x;y=v.y;z=v2.z}     //<<>
    | v when v.x <= v2.x && v.y >  v2.y && v.z <= v2.z -> {x=v.x;y=v2.y;z=v.z}     //<><
    | v when v.x >  v2.x && v.y <= v2.y && v.z <= v2.z -> {x=v2.x;y=v.y;z=v.z}     //><<
    | v when v.x <= v2.x && v.y >  v2.y && v.z >  v2.z -> {x=v.x;y=v2.y;z=v2.z}    //<>>
    | v when v.x >  v2.x && v.y <= v2.y && v.z >  v2.z -> {x=v2.x;y=v.y;z=v2.z}    //><>
    | v when v.x >  v2.x && v.y >  v2.y && v.z <= v2.z -> {x=v2.x;y=v2.y;z=v.z}    //>><
    | v when v.x >  v2.x && v.y >  v2.y && v.z >  v2.z -> v2                       //>>>
    | _ -> failwith "Unmatched condition"

let largestElements (v1: Vector3D<_>) (v2: Vector3D<_>) =
    match v1 with
    | v when v.x <= v2.x && v.y <= v2.y && v.z <= v2.z -> v2                       //<<<
    | v when v.x <= v2.x && v.y <= v2.y && v.z >  v2.z -> {x=v2.x;y=v2.y;z=v.z}    //<<>
    | v when v.x <= v2.x && v.y >  v2.y && v.z <= v2.z -> {x=v2.x;y=v.y;z=v2.z}    //<><
    | v when v.x >  v2.x && v.y <= v2.y && v.z <= v2.z -> {x=v.x;y=v2.y;z=v2.z}    //><<
    | v when v.x <= v2.x && v.y >  v2.y && v.z >  v2.z -> {x=v2.x;y=v.y;z=v.z}     //<>>
    | v when v.x >  v2.x && v.y <= v2.y && v.z >  v2.z -> {x=v.x;y=v2.y;z=v.z}     //><>
    | v when v.x >  v2.x && v.y >  v2.y && v.z <= v2.z -> {x=v.x;y=v.y;z=v2.z}     //>><
    | v when v.x >  v2.x && v.y >  v2.y && v.z >  v2.z -> v                        //>>>
    | _ -> failwith "Unmatched condition"

let rec vecMinMax (v: Vector3D<_> list) (acc: Vector3D<_>*Vector3D<_>) =
    match v with
    | head:: tail -> 
                    let (minVec,maxVec) = acc
                    vecMinMax tail ( (smallestElements minVec head), (largestElements maxVec head) )
    | [] -> acc