module Write

open Cells

open System.IO

let writeAllDeathHist (t: int) (hist:int[][]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-RunAllDeathHist-run%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Death Histograms for all Runs ##\n\n" 

        //going through all runs...
        for i=0 to Array.length hist-1 do
            fprintf sw "Run %d," (i+1)
            for j=0 to Array.length hist.[i] - 2 do
                fprintf sw "%d," hist.[i].[j]
            fprintf sw "%d\n" hist.[i].[Array.length hist.[i] - 1]
                    
        sw.Close()

let writeAllFertHist (t: int) (hist:int[][]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-RunAllFertHist-run%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Fertilisation Histograms for all Runs ##\n\n" 

        //going through all runs...
        for i=0 to Array.length hist-1 do
            fprintf sw "Run %d," (i+1)
            for j=0 to Array.length hist.[i] - 2 do
                fprintf sw "%d," hist.[i].[j]
            fprintf sw "%d\n" hist.[i].[Array.length hist.[i] - 1]
                    
        sw.Close()

let writeAllSizeDist (t: int) (locs:float[][]) (areas:float[][] ) (av:float[][]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-RunAllSizeDist-run%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Size Distributions for all Runs ##\n\n" 

        //going through all runs...
        for i=0 to Array.length locs-1 do
            fprintf sw "%d, Location," (i+1)
            for j=0 to Array.length locs.[i] - 2 do
                fprintf sw "%f," locs.[i].[j]
            fprintf sw "%f\n" locs.[i].[Array.length locs.[i] - 1]
            fprintf sw "%d, Areas," (i+1)
            for j=0 to Array.length areas.[i] - 2 do
                fprintf sw "%f," areas.[i].[j]
            fprintf sw "%f\n" areas.[i].[Array.length areas.[i] - 1]
            fprintf sw "%d, average Area," (i+1)
            for j=0 to Array.length av.[i] - 2 do
                fprintf sw "%f," av.[i].[j]
            fprintf sw "%f\n\n" av.[i].[Array.length av.[i] - 1]
        
        sw.Close()

let writeAllRates (t: int) (data:float[][] ) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-RunAllRates-run%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Final Rates for Cells ##\n\n" 
        fprintf sw "Run,Division,Movement,Death,Fertilisation\n"

        for i=0 to Array.length data-1 do
            fprintf sw "%d,%f,%f,%f,%f\n" (i+1) data.[i].[0] data.[i].[1] data.[i].[2] data.[i].[3]
        
        sw.Close()

let writeRates (t: int) (div:float) (move:float) (av:float) (death:float) (fert:float) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-Rates-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Final Rates for Cells ##\n\n" 
        fprintf sw "Division,Movement,avMove,Death,Fertilisation\n"
        fprintf sw "%f,%f,%f,%f,%f\n" div move av death fert
        
        sw.Close()


let writeHistData (t: int) (div:float) (move:float) (inter:float[]) (death:int[]) (fert:int[])  =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-HistData-step%A.fs" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file //c:\Users\benhall\Desktop
        let sw = StreamWriter(path)                 
                
        fprintf sw "let divRate = %f\n" div 
        fprintf sw "let moveRate = %f\n" move 
        fprintf sw "let intervals = %A\n" inter
        fprintf sw "let deathHisto = %A\n" death
        fprintf sw "let fertHisto = %A\n" fert
        
        sw.Close()


let writeCells (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-Cells-step%A.fs" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file //c:\Users\benhall\Desktop
        let sw = StreamWriter(path)                 
                
        fprintf sw "module ArrayCells\n" 
        fprintf sw "open Cells\n" 
        fprintf sw "open Locations\n\n"
        fprintf sw "let startcells = [|"
        fprintf sw "Cell(%f, %f, Size = %f, Markers = %A, NotchLevel = %f, RasLevel = %f, GldLevel = %f, Velocity = %A, Force = %A, Divide = %d, CcTime = %d, DZoneTime = %d, GFP = %A)" cells.[0].Location.x cells.[0].Location.y cells.[0].Size cells.[0].Markers cells.[0].NotchLevel cells.[0].RasLevel cells.[0].GldLevel cells.[0].Velocity cells.[0].Force cells.[0].Divide cells.[0].CcTime cells.[0].DZoneTime cells.[0].GFP
        for i=1 to (cells.Length-1) do
            fprintf sw "; Cell(%f, %f, Size = %f, Markers = %A, NotchLevel = %f, RasLevel = %f, GldLevel = %f, Velocity = %A, Force = %A, Divide = %d, CcTime = %d, DZoneTime = %d, GFP = %A)" cells.[i].Location.x cells.[i].Location.y cells.[i].Size cells.[i].Markers cells.[i].NotchLevel cells.[i].RasLevel cells.[i].GldLevel cells.[i].Velocity cells.[i].Force cells.[i].Divide cells.[i].CcTime cells.[i].DZoneTime cells.[i].GFP
        fprintf sw "|]"

        sw.Close()

let writeCellData (sw: StreamWriter) (sw2: StreamWriter) (t: int) (data: Cell[]) =
    let cells = Array.filter (fun (c:Cell) -> c.Markers <> DTC && c.Markers <> Stopped) data 
    let mitotic = Array.length (Array.filter (fun (c:Cell) -> c.Markers = Mitosis) data)
    let meiotic = Array.length (Array.filter (fun (c:Cell) -> c.Markers = Meiosis && c.RasLevel < RasLimit2) data)
    let oocytes = Array.length (Array.filter (fun (c:Cell) -> c.Markers = Meiosis && c.RasLevel >= RasLimit2) data)
    fprintf sw2 "%d,%d,%d,%d,%d,%d\n" t mitotic meiotic oocytes deadCounter fertCounter
    for i = 0 to (cells.Length-1) do
        fprintf sw "%d,%f,%f,%f,%A,%f,%f,%f,%d\n" t cells.[i].Location.x cells.[i].Location.y cells.[i].Size cells.[i].Markers cells.[i].NotchLevel cells.[i].RasLevel cells.[i].Densityfactor cells.[i].CcTime

(*let writeMarkedCellLocations (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-GFPLoc-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)  
        
        let markedCells = Array.filter (fun (c:Cell) -> c.GFP = On) cells 
        for i=0 to (markedCells.Length-1) do
            fprintf sw "Step,Location x, Location y\n"
            fprintf sw "%A,%f,%f\n" t markedCells.[i].Location.x markedCells.[i].Location.y
            
        sw.Close()*)

let writeRasLevel (sw: StreamWriter) (t: int) (cells: Cell[]) =
    let behindDZone = Array.filter (fun (c:Cell) -> c.Location.x > deathEnd && c.Location.x < deathEnd + 10.) cells
    for i = 0 to (behindDZone.Length - 1) do
        fprintf sw "%d,%f,%f\n" t behindDZone.[i].Location.x behindDZone.[i].RasLevel

let writeGFPData (sw: StreamWriter) (t: int) (cells: Cell[]) =
    for i = 0 to (cells.Length-1) do
        fprintf sw "%d,%f,%f,%f,%A,%f,%f,%f,%d,%d\n" t cells.[i].Location.x cells.[i].Location.y cells.[i].Size cells.[i].Markers cells.[i].NotchLevel cells.[i].RasLevel cells.[i].Densityfactor cells.[i].DZoneTime cells.[i].CcTime

let writeMovementRate (t: int) (cells: Cell[])  =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-Move-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path) 
        fprintf sw "## Step Numbers for movement of at least one diameter ##\n\n"

        let mutable avMovement = [||]
        for i=0 to (cells.Length-1) do
            let movement = cells.[i].MovementRate
            avMovement <- Array.append avMovement [|float (Array.sum movement - movement.[0] - movement.[movement.Length-1])/ float (movement.Length-2)|]
            for k=1 to (movement.Length-2) do
                    fprintf sw "%d," movement.[k]
            fprintf sw "\n"
        fprintf sw "\n"
        //could do average of all here as well...
        fprintf sw "## Average movement rate per cell ##\n\n"
        for k=0 to (avMovement.Length-1) do
            fprintf sw "%f," avMovement.[k]    

        sw.Close()
 
let writeDivisionRate (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-Division-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Division times per cell ##\n\n"

        let mutable avDivTime = [||]
        for i=0 to (cells.Length-1) do
            let divide = cells.[i].Division
            avDivTime <- Array.append avDivTime [|float (Array.sum divide - divide.[0] - divide.[divide.Length-1])/ float (divide.Length-2)|]
            for k=0 to (divide.Length-2) do
                fprintf sw "%d," divide.[k]
            fprintf sw "\n"
        fprintf sw "\n"
        //could do average of all here as well...
        fprintf sw "## Average time between divisions per cell ##\n\n"
        for k=0 to (avDivTime.Length-1) do
            fprintf sw "%f," avDivTime.[k] 
              
        sw.Close()

let writeDeadCellData (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-DeadCellData-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Dead Cells: %d ##\n" deadCounter
        fprintf sw "## Total Cells: %d ##\n\n" cellCounter
        fprintf sw "## Data for Dead Cells ##\n\n"
        fprintf sw "Raslimit: %f\n\n" RasLimit2
        fprintf sw "Death Time,Position x,Position y,Size,Notch Level,Ras Level,Density Factor,Time in Zone,Dev. Time\n" 


        for i=0 to (cells.Length-1) do
            fprintf sw "%d,%f,%f,%f,%f,%f,%f,%d,%d\n" deadTimes.[i] cells.[i].Location.x cells.[i].Location.y cells.[i].Size cells.[i].NotchLevel cells.[i].RasLevel cells.[i].Densityfactor cells.[i].DZoneTime cells.[i].CcTime

        sw.Close()

let writeFertRate (t: int) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-FertRate-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path)
        fprintf sw "## Fertilised Cells: %d ##\n\n" fertCounter
        fprintf sw "## Fertilisation times ##\n\n"


        for k=0 to (fertTimes.Length-1) do
            fprintf sw "%d\n" fertTimes.[k]

        sw.Close()

let writeDeathData (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-DeathData-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path) 
        fprintf sw "## Data for Cells in the Death Zone ##\n\n"
        fprintf sw "Raslimit: %f\n\n" RasLimit2
        fprintf sw "Density,DensityAv,Ras Level,RasAv,Size,SizeAv,Time in Zone,ZoneAv,Dev. Time,DevTimeAv,x Location\n"
        let mutable densAv = 0.
        let mutable RasAv = 0.
        let mutable SizeAv = 0.
        let mutable DZoneAv = 0.
        let mutable CcAv = 0.

        for i = 0 to (cells.Length - 1) do
            densAv <- densAv + cells.[i].Densityfactor
            RasAv <- RasAv + cells.[i].RasLevel
            SizeAv <- SizeAv + cells.[i].Size
            DZoneAv <- DZoneAv + float cells.[i].DZoneTime
            CcAv <- CcAv + float cells.[i].CcTime

        densAv <- densAv/float cells.Length 
        RasAv <- RasAv/float cells.Length
        SizeAv <- SizeAv/float cells.Length
        DZoneAv <- DZoneAv/float cells.Length
        CcAv <- CcAv/float cells.Length
         
        for i = 0 to (cells.Length-1) do
            
            fprintf sw "%f,%f,%f,%f,%f,%f,%d,%f,%d,%f,%f\n" cells.[i].Densityfactor densAv cells.[i].RasLevel RasAv cells.[i].Size SizeAv cells.[i].DZoneTime DZoneAv cells.[i].CcTime CcAv cells.[i].Location.x
        
        sw.Close()

let writeSizeDistr (t: int) (cells: Cell[]) =
        let now = System.DateTime.Now 
        let file = sprintf "%s-%04d.%02d.%02d-%02d.%02d-SizeDistr-step%A.csv" runName now.Year now.Month now.Day now.Hour now.Minute t
        let path = @"c:\Users\benhall\Skydrive @ Microsoft\GermlineData\" + file
        let sw = StreamWriter(path) 
        fprintf sw "## Size and location of Cells in the Ras Zone ##\n\n"
        fprintf sw "x Location,Size,SizeAv\n"
        let mutable SizeAv = 0.
        let rasZone = Array.filter (fun (c:Cell) -> c.Location.x > rasB && c.Location.y < topTubeB) cells

        for i = 0 to (rasZone.Length - 1) do
            SizeAv <- SizeAv + rasZone.[i].Size

        SizeAv <- SizeAv/float rasZone.Length
         
        for i = 0 to (rasZone.Length-1) do
            fprintf sw "%f,%f,%f\n" rasZone.[i].Location.x rasZone.[i].Size SizeAv 
        
        sw.Close()