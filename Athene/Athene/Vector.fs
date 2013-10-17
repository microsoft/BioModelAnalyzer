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
    static member (-) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = {x = v1.x-v2.x; y = v1.y-v2.y; z = v1.z-v2.z}
    static member (+) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = {x = v1.x+v2.x; y = v1.y+v2.y; z = v1.z+v2.z}
    static member (*) (v1 : Vector3D<'u>, v2 : Vector3D<'u>) = v1.x*v2.x + v1.y*v2.y + v1.z*v2.z
    static member (*) (v1 : Vector3D<'u>, s : float<_>) = {x = v1.x*s; y = v1.y*s; z = v1.z*s}
    static member (*) (s : float<_>, v1 : Vector3D<'u>) = {x = v1.x*s; y = v1.y*s; z = v1.z*s}
    //static member (~) (v1 : Vector3D<'u>) = (v1.x**2. + v1.y**2. + v1.z**2.)**0.5
    static member (.^) (v1: Vector3D<'u>, v2: Vector3D<'u>) = {x = v1.y*v2.z-v1.z*v2.y; y = v1.z*v2.x-v1.x*v2.z; z = v1.x*v2.y-v1.y*v2.x }
