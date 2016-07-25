open Microsoft.Z3


[<EntryPoint>]
let main argv = 
    let cfg = System.Collections.Generic.Dictionary()
    cfg.Add("MODEL", "true")
    use z = new Context(cfg)
    let a = z.MkRealConst "a"
    let b = z.MkRealConst "b"
    let one = z.MkReal(1)
    let two = z.MkReal(2)
    let p1 = z.MkGe(z.MkAdd(a, b), one).Simplify() :?> BoolExpr
    let p2 = z.MkLe(z.MkSub(a, b), two)

    

    use s = z.MkSolver()
    s.Assert(p1, p2)
    let sat = s.Check()
    let model = s.Model

    let va = model.ConstInterp(model.ConstDecls.[0])
    let vb = model.ConstInterp(model.ConstDecls.[1])


        
    0 // return an integer exit code

