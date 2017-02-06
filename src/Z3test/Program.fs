// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
open Microsoft.Z3


[<EntryPoint>]
let main argv = 
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    use z = new Context(cfg)
    
    let v1 = z.MkIntConst (z.MkSymbol("v1^0"))
    let v2 = z.MkIntConst (z.MkSymbol("v2^0"))
  
  
    use s = z.MkSolver()

    s.Assert((z.MkGe(v1, z.MkInt(0))).Simplify() :?> BoolExpr)
    s.Assert((z.MkGe(v2, z.MkInt(0))).Simplify() :?> BoolExpr)
                                                             
    s.Assert((z.MkLe(v1, z.MkInt(4))).Simplify() :?> BoolExpr)
    s.Assert((z.MkLe(v2, z.MkInt(4))).Simplify() :?> BoolExpr)

    let v1_r = z.MkInt2Real(v1)
    let four = z.MkReal(4)
    let ite = z.MkITE(z.MkLt(four, v1_r), four,  v1_r) 
    let sum = z.MkAdd (z.MkReal(1,2), ite :?> RealExpr)

    s.Assert(z.MkEq(v2, z.MkReal2Int(sum :?> RealExpr)))

    let sat = s.Check()
    let model = s.Model

    let va = model.ConstInterp(model.ConstDecls.[0])
    let vb = model.ConstInterp(model.ConstDecls.[1])


        
    0 // return an integer exit code

