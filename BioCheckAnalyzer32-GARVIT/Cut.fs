module Cut

let ExploreCuts (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) =
    let mutable cutNode = 0
    let mutable cutAt = 0

    for node in qn do
        if (fst bounds.[node.var]) < (snd bounds.[node.var]) then 
            cutNode <- node.var
            cutAt <- ((fst bounds.[cutNode]) + (snd bounds.[cutNode]) - 1) / 2

    for node in qn do
        for cutPoint in [(fst bounds.[node.var]) .. (snd bounds.[node.var])-1] do
            if Oracle.CanStrictlyIncrease node cutPoint ranges bounds then 
                if Oracle.CanStrictlyDecrease node (cutPoint+1) ranges bounds then
                    if Log.level(1) then Log.log_debug(sprintf "Node: %d, cutPoint: %d, TwoWay" node.var cutPoint)
                else
                    if Log.level(1) then Log.log_debug(sprintf "Node: %d, cutPoint: %d, OneWay" node.var cutPoint)
                    cutNode <- node.var
                    cutAt <-cutPoint
            elif Oracle.CanStrictlyDecrease node (cutPoint+1) ranges bounds then
                if Log.level(1) then Log.log_debug(sprintf "Node: %d, cutPoint: %d, OneWay" node.var cutPoint)
                cutNode <- node.var
                cutAt <-cutPoint
            else
                if Log.level(1) then Log.log_debug(sprintf "Node: %d, cutPoint: %d, ZeroWay" node.var cutPoint)
                cutNode <- node.var
                cutAt <-cutPoint

    (cutNode, cutAt)