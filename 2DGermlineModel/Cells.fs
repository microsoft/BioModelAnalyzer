module Cells

// Pt coordinates are an int grid (an approximation)
//open Locations
open Gaussian


let extentX,extentY = 200.0,100.0

let pow2 x = x*x : float
let pow3 x = x*x*x: float
let pi = System.Math.PI
let ran = System.Random()

[<Struct>]
type Pt(xx:float,yy:float) =
    member this.x = xx
    member this.y = yy
    override this.ToString() = "Pt(" + string xx + "," + string yy + ")"

// Switches
let enableParallelLoops      = true //true
//let enableLocationTable      = false
//let enableLocationTableCheck = false // sanity check, but slow. it fails currently. are dead cells being included in the check?
//let enableAdaptiveTimeStep   = false // need to tune the acceptable changes for V and F (experimental!)

module Parallel =
    module Array =
        let iter f (arr: 'T array) =
            if enableParallelLoops then
                let action = new System.Action<int>(fun i -> f arr.[i])     // one time allocation per loop
                System.Threading.Tasks.Parallel.For(0,arr.Length-1,action) |> ignore        // leave division and allocation to Parallel.For
            else
                Array.iter f arr
        let inline iterN f n =
            if enableParallelLoops then
                let action = new System.Action<int>(fun i -> f i)       // one time allocation per loop
                System.Threading.Tasks.Parallel.For(0,n-1,action) |> ignore             // leave division and allocation to Parallel.For
            else
                for i = 0 to n-1 do f i done



type Grads = Delta | GLD1 | Ras | No

type TransportMarker = Mitosis | Meiosis | PreMeiosis | Stopped | DTC | Dead | Fertilised

type MolecularMarker = On | Off

let deathMech = "ZoneTime_greater16000-noEggs"
let mutable run = 1
let mutable runName = sprintf "Run%A_%s" run deathMech  
let setRunName w = sprintf "Run%A_%s" w deathMech

let mutable startTime = let now = System.DateTime.Now in sprintf "%s-%04d.%02d.%02d-%02d.%02d.%02d" runName now.Year now.Month now.Day now.Hour now.Minute now.Second


                                

let hour = 1000.
let divHour = 1000.


type Cell(x,y) =

    let mutable location   = Pt(x,y)
    let mutable markers   = Mitosis
    let mutable generation = 0
    //trying to include size
    let mutable size = 0.7
    //asynchronous growing...
    let randDivArr =  Seq.toArray (Seq.truncate 1 (normalRand (20.*divHour) (4.*divHour/3.)))//ran.NextDouble()//ran.Next(7,13)takes time steps between 7 and 13
    let mutable divide = 0
    //for MDmove
    let mutable velocity = [|0.; 0.|]
    let mutable force = [|0.; 0.|]
    //density
    let mutable density = 0.
    //for gradients
    let mutable ras = 0.
    let mutable fertilised = 0.
    let mutable notch = 0.
    let mutable gld = 0.
    //for movement rate
    let mutable goal = x
    let mutable movementrate = [|0|]
    //to mark the cells
    let mutable gfp = Off
    //to count divisions
    let mutable division = [|0|]
    //for death theories with cell cycle time and time in deathzone
    let mutable ccTime = 0
    let mutable dZoneTime = 0

          
    // expose state
    member this.Location   with get() = location and set(x) = location <- x
    member this.Markers    with get() = markers  and set(x) = markers <- x    
    member this.Generation with get() = generation and set(x) = generation <- x
    //size again...
    member this.Size       with get() = size and set(x) = size <- x
    member this.Divide     with get() = divide and set(x) = divide <- x
    // for MDmove
    member this.Velocity   with get() = velocity and set(x) = velocity <- x
    member this.Force      with get() = force and set(x) = force <- x
    // density
    member this.Densityfactor    with get() = density and set(x) = density <- x
    // for movement rate
    member this.Goal        with get() = goal and set(x) = goal <- x
    member this.MovementRate with get() = movementrate and set(x) = movementrate <- x
    // gradients
    member this.RasLevel   with get() = ras and set(x) = ras <- x
    member this.NotchLevel with get() = notch and set(x) = notch <- x 
    member this.GldLevel   with get() = gld and set(x) = gld <- x   
    //marked cells
    member this.GFP        with get() = gfp and set(x) = gfp <- x 
    //division count
    member this.Division   with get() = division and set(x) = division <- x
    //for death theories
    member this.CcTime    with get() = ccTime and set(x) = ccTime <- x
    member this.DZoneTime  with get() = dZoneTime and set(x) = dZoneTime <- x

    override this.ToString() = sprintf "C(%f,%f)" location.x location.y
    // modify state
    member this.NextGeneration() = generation <- generation + 1
    member this.Grow()          = size <- size + (sqrt(2.)*0.7-0.7)/randDivArr.[0]//(0.5+rand)*x/5000.//DIVIDE FERT AS WELLL!!! (1./float rand)
    //CHANGE here for coupling of meiotic growth with /43000. without /30000.
    member this.Grow2(x)         = size <- size + x*(6.2-0.7)/30000.
    member this.Egg(x)           = size <- size + x*(6.2-0.7)/2700.
    member this.Egg2(x)          = size <- size + x*(6.2-0.7)/400.
    //density method7
    member this.Density(x,y)       = density <- density + x/(6.*y)
    //calculating the movementrate
    member this.ReadMoveRate(x)     = movementrate <- let diff = x - movementrate.[Array.length movementrate-1]
                                                      movementrate.[Array.length movementrate-1] <- diff
                                                      Array.append movementrate [|x|]
    //notch
    member this.Notch(x)         = notch <- max (notch - x) 0.
    //gld
    member this.Gld(x)           = gld <- gld + (x)
    //rasgradient
    member this.Ras(x)           = ras <- ras + (x*5.)
    //cell death
    member this.Die()            = markers <- Dead ; 
                                   velocity <- [|0.;0.|]; force <- [|0.;0.|]
    //fertilisation
    member this.Fertilise(x)     = fertilised <- 1. (*fertilised + x/100.*) ; if fertilised >= 1. then 
                                                                                 markers <- Fertilised
let mutable cellCounter = 0
let mutable deathtime = 500.
let mutable deadCounter = 0
let mutable deadTimes : int[] = [||]
let mutable deathCells : Cell[] = [||]
let mutable deadCells : Cell[] = [||]
let mutable fertCounter = 0
let mutable fertTimes : int[] = [||]


let runChange i = runName <- setRunName i
                  startTime <- let now = System.DateTime.Now in sprintf "%s-%04d.%02d.%02d-%02d.%02d.%02d" runName now.Year now.Month now.Day now.Hour now.Minute now.Second
                  deadCounter <- 0
                  fertCounter <- 0
                  cellCounter <- 0 
                  deadTimes <- [||]
                  deathCells <- [||]
                  deadCells <- [||]
                  fertTimes <- [||]  

                  
                 
let average (n:int[]) = let mutable av = 0.
                        for i=0 to (n.Length - 1) do
                            av <- av + float n.[i]
                        av <- av/float n.Length
                        av

let floatAverage (n:float[]) =  let mutable av = 0.
                                for i=0 to (n.Length - 1) do
                                    av <- av + float n.[i]
                                av <- av/float n.Length
                                av
    
let effDivRate (divisions:int[]) = (average divisions)/20.

let effMoveRate (div:float) (movement:int[]) = ((average movement)/div)*60.

let histInt (steps:int) (divAv:float) (runNo:int) = if divAv > 0. then
                                                        let length = int (float steps/divAv) //1000000.
                                                        let intervals = Array.init length (fun x -> float x*divAv)//((float runNo-1.)*1000000.) + 
                                                        intervals
                                                    else
                                                        let intervals = [||]
                                                        intervals
                                        

let makeHist (intervals:float[]) (data:int[]) = 
    let mutable hist:int[] = [||]
    let mutable j = 0
    if Array.length data > 0 then
        for i=0 to Array.length data-1 do
            while j+1 < Array.length intervals && data.[i] > int intervals.[j+1]  do
                if Array.length hist > j && hist.[j] > 0 then
                    j <- j+1
                else
                    hist <- Array.append hist [|0|] //.[j] <- 0
                    j <- j+1
            if j+1 < Array.length intervals && int intervals.[j] < data.[i] && int intervals.[j+1] >= data.[i] then  
                if Array.length hist > j then
                   hist.[j] <- hist.[j] + 1 
                else
                    hist <- Array.append hist [|1|]                       
                
    hist

let histAv (steps:int) (intervals:float[]) (histData:int[]) = 
     let mutable av = 0.
     let mutable j = 0.
     for i=0 to (intervals.Length - 1) do
        if i < Array.length histData && intervals.[i] >= float steps/10. && intervals.[i] < float steps then //100000.1000000.
            av <- av + float histData.[i]
            j <- j+1.
     av <- av/j
     av

let border = 117.
let gldB = 31.
let rasB = 75.
let rasEnd = border + 10.
let mutable deathborder = border
let mutable deathEnd = border

let baseCell = 0.7
let straightTube = 9.9
let topTubeT = 7.7
let topTubeB = topTubeT+14.6//=22.3
let tubeDist = topTubeB-topTubeT
let bottomTubeT = topTubeB+3.//=25.3=46.1-20.8
let bottomTubeB = bottomTubeT+14.//=29.3
let tubeEnd = 69.

let RasLimit1 = 7500.//9250.//7500.   
let RasLimit2 = 7500.//9250.
let NotchMax = 1000. 




/////definition of gradients and setting the markers accordingly\\\\\
let gradient (p:Pt) =
    let x,y = p.x,p.y 
    if x < gldB && y < 25. then
        Delta
    else if (gldB <= x && x < rasB && y < 25.) then
        GLD1
    else if (rasB <= x && x < rasEnd && y < 25.) then
        Ras
    else
        No
   
let setMarkers (dt:float) (cell:Cell) =
    let grad = gradient cell.Location
    if cell.Markers <> Stopped && cell.Markers <> DTC && cell.Markers <> Dead && cell.Markers <> Fertilised then
        if grad = Delta then
            cell.NotchLevel <- NotchMax
            cell.Markers <- Mitosis
        else if grad = GLD1  then//|| grad = Ras
            cell.Notch(dt*7.)//decrease of Notch 
            cell.Gld(dt/10.)
            if cell.NotchLevel < cell.GldLevel then// = 0.  && cell.Size = 0.7
                if cell.Size < sqrt(2.)*0.7 then
                    cell.Markers <- PreMeiosis
                else
                    cell.Markers <- Meiosis
        else if grad = Ras then
            cell.Ras(dt)
            



 /////function for growing of cells\\\\\
let cellCycle (step: int) (dt:float) (cell:Cell) =
    if cell.Markers = Mitosis then
        if cell.Size < sqrt(2.)*0.7 then //is sqrt2*r
            cell.Grow()
        else
            cell.Divide <- 1
     else if cell.Markers = PreMeiosis then
        if cell.CcTime > 0 then (cell.CcTime <- cell.CcTime+1) else (cell.CcTime <- step - cell.Division.[cell.Division.Length - 1])
        cell.Grow()
    //CHANGE here for coupling of meiotic growth cell.CcTime > 19500 cell.Location.x >75.
     else if cell.Markers = Meiosis && cell.Location.x >75. && cell.RasLevel < RasLimit2 && (*cell.Location.x <= border &&*) cell.Location.y < bottomTubeT && cell.Size < 6.2 then
        if cell.CcTime > 0 then (cell.CcTime <- cell.CcTime+1) else (cell.CcTime <- step - cell.Division.[cell.Division.Length - 1])
        cell.Grow2(dt)
     else if cell.Markers = Meiosis (*&& cell.Location.x > border*) && cell.RasLevel >= RasLimit2 && cell.Location.y < bottomTubeT && cell.Size < 3.1 then
        if cell.CcTime > 0 then (cell.CcTime <- cell.CcTime+1) else (cell.CcTime <- step - cell.Division.[cell.Division.Length - 1])
        cell.Egg(dt)
     else if cell.Markers = Meiosis && cell.RasLevel >= RasLimit2 && cell.Location.y >= bottomTubeT && cell.Location.x <= 120. && cell.Size < 6.2 then
        if cell.CcTime > 0 then (cell.CcTime <- cell.CcTime+1) else (cell.CcTime <- step - cell.Division.[cell.Division.Length - 1])
        cell.Egg2(dt)
    
    
            
/////function for cell division with random axis, also writing timestep at division to cells\\\\\         
let cellDivision (step:int) (cell: Cell)  =
    let factor = ran.NextDouble()
    let sinus = 0.7* sin (factor*pi*0.7)//new cells don't overlap... they would with 0.35
    let cosinus = 0.7*cos (factor*pi*0.7)
    let timeBtwDiv = step - cell.Division.[Array.length cell.Division - 1]
    let newCell1 = Cell(cell.Location.x+cosinus, cell.Location.y+sinus, Generation = cell.Generation+1, NotchLevel = cell.NotchLevel, GFP = cell.GFP, Division = cell.Division)
    newCell1.Division.[Array.length cell.Division - 1] <- timeBtwDiv
    newCell1.Division <- Array.append newCell1.Division [|step|]
    let newCell2 = Cell(cell.Location.x-0.7*cosinus, cell.Location.y-sinus, Generation = cell.Generation+1, NotchLevel = cell.NotchLevel, GFP = cell.GFP, Division = [|step|])
    //newCell2.Division.[Array.length cell.Division - 1] <- timeBtwDiv
    //newCell2.Division <- Array.append newCell2.Division [|step|]
    [|newCell1;newCell2|]   
    


/////function for cell death, also saving time step of death and number of dead cells\\\\\
let cellDeath (step:int) (cells:Cell[]) =
    let mutable hiRas = Array.filter (fun (c:Cell) -> c.RasLevel >= RasLimit2 && c.Location.x <= rasEnd && c.Location.y < topTubeB) cells
    let mutable bigger = [||]
    let mutable k = 0
    let mutable biggerSize = [||]
    let mutable deathCandidate = Cell(0.,0.)
    let mutable l = 0
    let mutable x = border+10.
    
    if Array.length hiRas > 0 then
        if Array.length hiRas >= 5 then
            while Array.length bigger <> (Array.length hiRas - 4) do
                x <- hiRas.[k].Location.x
                let x2 = x
                bigger <- Array.filter (fun (c:Cell) -> c.Location.x >= x2) hiRas
                k <- k+1
        let deathzoneE = x
        let deathzoneS = x-27.9
        deathborder <- deathzoneS
        deathEnd <- deathzoneE
        let mutable borderCells = Array.filter (fun (c:Cell) -> c.Location.x <= deathzoneE && c.Location.x > deathzoneS && c.Location.y < topTubeB) cells 
        deathCells <- borderCells
        Array.iter (fun (c:Cell) -> (c.DZoneTime <- (c.DZoneTime + 1))) deathCells
        let mutable nonEggs = Array.filter (fun (c:Cell) -> c.RasLevel < RasLimit2) deathCells

       //death for thresholds
        for i = 0 to Array.length nonEggs - 1 do
            if nonEggs.[i].DZoneTime > 16000 then //nonEggs.[i].RasLevel > 9249.499999999999099  nonEggs.[i].CcTime > 64500deathCells.[i].RasLevel = 0. deathCells.[i].DZoneTime > 18000 deathCells.[i].CcTime > 70000 nonEggs.[i].Size > 1.329123
                nonEggs.[i].Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|nonEggs.[i]|] 
     
               
       (*   
        //death for thresholds
        for i = 0 to Array.length nonEggs - 1 do
            if nonEggs.[i].Size > 1.329123 then //nonEggs.[i].RasLevel > 9249.499999999999099  nonEggs.[i].CcTime > 64500deathCells.[i].RasLevel = 0. deathCells.[i].DZoneTime > 18000 deathCells.[i].CcTime > 70000 nonEggs.[i].Size > 1.329123
                nonEggs.[i].Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|nonEggs.[i]|]
        
     
        //timed death -> smallest/biggest
        if step >= int deathtime then
            //let mutable nonEggs = Array.filter (fun (c:Cell) -> c.RasLevel < RasLimit2) deathCells
            if Array.length nonEggs >= 1 then
                while Array.length lowerRas <> (Array.length nonEggs) do
                    deathCandidate <- nonEggs.[l]
                    let ras = nonEggs.[l].RasLevel
                    lowerRas <- Array.filter (fun (c:Cell) -> c.RasLevel <= ras) nonEggs
                    l <- l+1
                deathCandidate.Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|deathCandidate|]      
            deathtime <- float step + (ran.NextDouble() * hour)   

        //death for smallest/biggest combined with threshold
        while Array.length smallerSize <> (Array.length nonEggs) do
                    deathCandidate <- nonEggs.[l]
                    let size = nonEggs.[l].Size
                    smallerSize <- Array.filter (fun (c:Cell) -> c.Size <= size) nonEggs
                    l <- l+1
        if deathCandidate.Densityfactor > 0.76 then // &&deathCells.[i].Size > 1.2 nonEggs.[i].CcTime > 64500deathCells.[i].RasLevel = 0. deathCells.[i].DZoneTime > 18000 deathCells.[i].CcTime > 70000 nonEggs.[i].Size > 1.329123
                deathCandidate.Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|deathCandidate|] 

        //random death
        let mutable loRas = Array.filter (fun (c:Cell) -> c.RasLevel < RasLimit2) borderCells
        let mutable hiRasD = Array.filter (fun (c:Cell) -> c.RasLevel >= RasLimit2) borderCells
        if step >= int deathtime then
        //while Array.length borderCells > 75 do
            if Array.length loRas > 0 then
                let i = ran.Next(0,(Array.length loRas)-1)
                loRas.[i].Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|loRas.[i]|]
            else if Array.length hiRasD > 0 then
                let i = ran.Next(0,(Array.length hiRasD)-1)
                hiRasD.[i].Die()
                deadCounter <- deadCounter+1
                deadTimes <- Array.append deadTimes [|step|]
                deadCells <- Array.append deadCells [|hiRasD.[i]|]
            loRas <- (Array.filter (fun (c:Cell) -> c.Markers <> Dead) loRas)   
            hiRasD <- (Array.filter (fun (c:Cell) -> c.Markers <> Dead) hiRasD)
            borderCells <- Array.append loRas hiRasD
        deathtime <- float step + (ran.NextDouble() * hour)*)
    else
        ()

/////function for fertilisation, also saving time step of fertilisation and number of fertilised cells\\\\\        
let fertiliseCells (dt:float) (step:int) (cells:Cell[]) =
    let fertcells = Array.filter (fun (c:Cell) -> c.Location.x <= tubeEnd && c.Location.y > bottomTubeT && c.Markers <> Stopped) cells
    Array.iter (fun (c:Cell) -> c.Fertilise(dt)) fertcells
    let fertilised = Array.filter (fun (c:Cell) -> c.Markers = Fertilised && c.Location.x <= tubeEnd) fertcells
    if Array.length fertilised <> 0 then
        fertTimes <- Array.append fertTimes [|step|]
    fertCounter <- fertCounter + Array.length fertilised  
    Array.iter (fun (c:Cell) -> c.Die()) fertilised



/////function for Velocity Verlet algorithm, also writing movement rate to cells\\\\\
let MDmove (dt:float) (steps:int) (neighbours :Cell[]) (cells:Cell[]) = // (locationTable:LocationTable<Cell>)with mass = r/0.4  
    //function that only lets feritlised cells exit
    let limitPt (c:Cell) (pt:Pt) =  // NOTE: Iterating over cells in here will give O(n^2) in the number of cells.
                                         let x,y = pt.x,pt.y
                                         //location change for periodic border conditions
                                         if x > straightTube && x < border - 10.7 && y <= topTubeT then
                                            Pt(x,y+tubeDist)
                                         else if x > straightTube && x < border - 10.7 && y >= topTubeB && y < bottomTubeT then
                                            Pt(x,y-tubeDist)
                                         else if c.Markers <> Stopped && c.Markers <> Fertilised && x < tubeEnd && y > bottomTubeT  then
                                            Pt(tubeEnd, y)
                                         else pt     
                                             
    // first step of verlet =r(t+dt) with limitPt to get ras border
    let moveCell (c:Cell) =
        let oldPt = c.Location
        let newPt = limitPt c (Pt(c.Location.x + c.Velocity.[0]*dt + 0.5*c.Force.[0]/(pi*pow2(c.Size))*(pow2 dt), 
                                  c.Location.y + c.Velocity.[1]*dt + 0.5*c.Force.[1]/(pi*pow2(c.Size))*(pow2 dt)))
        //if enableLocationTable then locationTable.Move(c,oldPt,newPt)
        c.Location <- newPt
        //for calculating the movement rate
        if c.Location.x < (border - 10.7) && c.Location.x >= c.Goal then
            c.ReadMoveRate(steps)
            c.Goal <- c.Location.x + 2.*c.Size//adds diameter of cell to current location-> movement of one row  
    Array.iter moveCell cells (* O(n) is OK? Move not made thread safe.*)

    // second step =v(t+.5dt)(*Parallel.*)
    Parallel.Array.iter (fun (c:Cell) -> c.Velocity <- [| c.Velocity.[0] + 0.5*c.Force.[0]/(pi*pow2(c.Size))*dt; 
                                                          c.Velocity.[1] + 0.5*c.Force.[1]/(pi*pow2(c.Size))*dt |]) cells
    //function for new forces
    (cells.Length-1) |> Parallel.Array.iterN (fun k ->    
        let mutable Fpartx = 0.//x-component of force
        let mutable Fparty = 0. 
        let cells_k = cells.[k]
        if cells_k.Markers = Mitosis || cells_k.Markers = Meiosis || cells_k.Markers = PreMeiosis then
        //making the discrimination between straight tube and rest to include boundary conditions
            if cells_k.Location.y < bottomTubeT && cells_k.Location.x > straightTube && cells_k.Location.x < border-10.7 then
                cells_k.Densityfactor <- 0.
                //let cells_ls = neighbours cells_k//all cells if location table isn't enabled
                for l = 0 to neighbours.Length-1 do 
                    let cell_l = neighbours.[l]                
                    if cells_k<>cell_l then //exclude self-interaction
                        let dx = cells_k.Location.x - cell_l.Location.x //x-distance of k and l
                        let dy = cells_k.Location.y - cell_l.Location.y //y-distance of k and l
                        let Rr = sqrt(pow2 dx+pow2 dy) // distance of k and l 
                    //this will exclude all cells with distance > r1+r2 from potential 
                        if Rr <= (cells_k.Size+cell_l.Size)   then 
                            let r = cell_l.Size
                            cells_k.Density(r,Rr-r)
                            let force = (8.*pow2 (cells_k.Size+cell_l.Size)/pow3 Rr)/Rr 
                            Fpartx <- Fpartx + force*dx//x-portion of resulting force
                            Fparty <- Fparty + force*dy              
            //info: F=grad V, aber die ableitung nach dem ort wird hier gemacht, indem durch Rr geteilt wird. das "geht", weil wir mit den kleinen verrückungen jeder iteration ja quasi infinitesimal arbeiten... ;)                  
                        else if abs dx <= (cells_k.Size+cell_l.Size) && (abs dy+cells_k.Size+cell_l.Size) >= tubeDist && cell_l.Location.y < bottomTubeT then
                            let effdy = if (abs (dy+tubeDist)) >= (abs (dy-tubeDist)) then dy-tubeDist else dy+tubeDist
                            let effRr = sqrt(pow2 dx + pow2 effdy)
                            let r = cell_l.Size
                            cells_k.Density(r,effRr-r)
                            let force = (8.*pow2 (cells_k.Size+cell_l.Size)/pow3 effRr)/effRr 
                            Fpartx <- Fpartx + force*dx//x-portion of resulting force
                            Fparty <- Fparty + force*effdy   
                        else 
                            ()    
                cells.[k].Force <- [| Fpartx; Fparty |]// new forces for particle k, F=a(t+dt) 
            else //if not in the straight part, ie the old function
                cells_k.Densityfactor <- 0.
                //let cells_ls = neighbours cells_k//all cells if location table isn't enabled
                for l = 0 to neighbours.Length-1 do 
                    let cell_l = neighbours.[l]                
                    if cells_k<>cell_l then //exclude self-interaction
                        let dx = cells_k.Location.x - cell_l.Location.x //x-distance of k and l
                        let dy = cells_k.Location.y - cell_l.Location.y //y-distance of k and l
                        let Rr = sqrt(pow2 dx+pow2 dy) // distance of k and l 
                    //this will exclude all cells with distance > r1+r2 from potential 
                        if Rr > (cells_k.Size+cell_l.Size)  then 
                            ()                  
                        else 
                            let r = cell_l.Size
                            cells_k.Density(r,Rr-r)
                            if cell_l.Markers <> Stopped then
                                let force = (8.*pow2 (cells_k.Size+cell_l.Size)/pow3 Rr)/Rr 
                                Fpartx <- Fpartx + force*dx//x-portion of resulting force
                                Fparty <- Fparty + force*dy 
                            else //stopped cells should push stronger and definition not exclude 0
                                let forceborder = (5.*(pow2 Rr-pow2 (cells_k.Size+cell_l.Size)))/Rr
                                Fpartx <- Fpartx - forceborder*dx//x-portion of resulting force
                                Fparty <- Fparty - forceborder*dy                
            //info: F=grad V, aber die ableitung nach dem ort wird hier gemacht, indem durch Rr geteilt wird. das "geht", weil wir mit den kleinen verrückungen jeder iteration ja quasi infinitesimal arbeiten... ;)     
                cells.[k].Force <- [| Fpartx; Fparty |]// new forces for particle k, F=a(t+dt) 
        else//Stopped cells and DTC shouldn't move and dead cells as well
            cells.[k].Force <- [| Fpartx; Fparty |]
    )
    
    // third step =v(t+dt)
    Parallel.Array.iter (fun (c:Cell) ->let mutable r = 0.
                                        if c.Size < 1.15  then
                                            r <- 0.02//this is the thermostat that accounts for the friction
                                        else if c.Size < 2. then
                                            r <- 0.005
                                        else
                                            r <- 0.0009
                                        c.Velocity <- [| (1.-r)*c.Velocity.[0] + 0.5*c.Force.[0]/(pi*pow2(c.Size))*dt; 
                                                         (1.-r)*c.Velocity.[1] + 0.5*c.Force.[1]/(pi*pow2(c.Size))*dt |]) cells

    //adaptive timestep... not used so far!
   (* if enableAdaptiveTimeStep then
        // O(n) slow way to compute max velocity and acceleration
        let maxV2 = cells |> Seq.map (fun (c:Cell) -> pow2 c.Velocity.[0] + pow2 c.Velocity.[1]) |> Seq.max
        let maxF2 = cells |> Seq.map (fun (c:Cell) -> pow2 c.Force.[0]    + pow2 c.Force.[1])    |> Seq.max
        let changeAreaF = 0.3 // restrict impact of force on velocity... magic numbers for now by manual tuning
        let changeAreaV = 0.3 // theory to determine them later...
        let suggestedDT = min (changeAreaF / 2.0 (* triangle*) / sqrt maxF2)
                              (changeAreaV / 2.0 (* triangle*) / sqrt maxV2)
        suggestedDT
    else
        0.0 // suggest dt=0.01 (will round up to minumumDT)*)
