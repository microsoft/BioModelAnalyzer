module SimUI
open Cells
open Write

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Media.Imaging
open System.IO

let enableBeepOnDropFile = true
let windowTitle          = "Refine Simulator"



/////definition of bitmap dump\\\\\
module VisualGrab =
    open System.Windows.Media.Animation
    open System.Windows.Media.Imaging
    let saveVisualToPNG (vs:Visual) (w:int,h:int) (path:string) =        
        let bmp = new RenderTargetBitmap(w,h,96.0,96.0,PixelFormats.Pbgra32)
        bmp.Render(vs)
        let png = new PngBitmapEncoder()
        png.Frames.Add(BitmapFrame.Create(bmp))
        use stm = IO.File.Create(path)
        png.Save(stm)
        if enableBeepOnDropFile then System.Console.Beep(100,10)

type Simulation =
    abstract Reset           : unit -> unit
    abstract Iterate         : int  -> unit
    abstract Render          : unit -> Drawing    
    abstract Summary         : unit -> string
    abstract Cells           : unit -> Cell[]
    abstract DropImagePrefix : unit -> (int * int * string) option

type Mode = Paused | Running of bool(*pending reset*)


/////definition of simulation environment\\\\\
let run (s:Simulation) =
    let drawingImage = DrawingImage()
    let image = Image(Source = drawingImage)
    
    let text  = TextBox()
    let bar   = ToolBar()

    //buttons in tool bar
    let resetB    = Button(Content="Reset")
    let step1     = Button(Content="Step 1")
    let step10    = Button(Content="Step 10")
    let step100   = Button(Content="Step 100")
    let step500   = Button(Content="Step 500")    
    let step1000  = Button(Content="Step 1000")
    let run10     = Button(Content="Run 10")
    let run100    = Button(Content="Run 100")
    let run500    = Button(Content="Run 500")    
    let run1000   = Button(Content="Run 1000")
    let run100000   = Button(Content="Run 100000")
    let run100000000   = Button(Content="Run 100*1000000")
    let stop      = Button(Content="Stop")
    let write     = Button(Content="Write Cells to File")  
    let moverate  = Button(Content="Check Movement Rate")
    let marked    = Button(Content="Write Marked Cell Locations")
    let divided   = Button(Content="Check Division Rate")
    let fert   = Button(Content="Check Fertilisation Rate")
    let dead   = Button(Content="Check Death Rate")
    List.map bar.Items.Add [resetB;step1;step10;step100;step500;step1000;run10;run100;run500;run1000;run100000;run100000000;stop;write;moverate;marked;divided;fert;dead] |> ignore
    
    let panel = StackPanel()
    panel.Background  <- Brushes.Gray
    panel.Orientation <- Orientation.Vertical
    
    //status bar showing current status of simulation    
    let status     = Primitives.StatusBar()
    let statusItem = Primitives.StatusBarItem(Content="")
    status.Items.Add statusItem
    
    panel.Children.Add bar
    panel.Children.Add status
    panel.Children.Add image
    
    //window
    let win = Window(Visibility = Visibility.Visible, Content = panel,Title = windowTitle,Height=600.0)
    image.SizeChanged.Add(fun _ -> win.Title <- sprintf "%s [%d,%d]" windowTitle (int image.ActualWidth) (int image.ActualHeight))

    let execUI f = win.Dispatcher.Invoke(Threading.DispatcherPriority.Background,new Action(fun () -> f()))  |> ignore<obj>
    let execPool x f c = 
        let exec () = 
            let res = f x
            execUI (fun () -> c res)
        System.Threading.ThreadPool.QueueUserWorkItem(new Threading.WaitCallback(fun _ -> exec()))
        ()

    // execution state (single thread!)
    let stepDone = ref 0
    let stepTodo = ref 0
    let stepSize = ref 1
    let active   = ref Paused    
    let runtime  = ref 0L 
    let speedAve = ref 0.0
    let speedNow = ref 0.0
    let addSteps n = stepTodo := !stepTodo + n
    
    //updating the status
    let reportStatus() =
        statusItem.Content <- sprintf "Done %d of %d (stepping %d). Current speed %.2f steps/sec. Ave speed %.2f steps/sec. [%s]"
                                      !stepDone (!stepDone + !stepTodo) !stepSize !speedNow !speedAve (s.Summary())
    
    let sw = System.Diagnostics.Stopwatch()

    let reset() = 
        match !active with
        | Paused    -> s.Reset(); drawingImage.Drawing <- s.Render()
        | Running _ -> active := Running true(*<-reset pending*)
    
    //image drop    
    let dropImage n (w,h) prefix =
        VisualGrab.saveVisualToPNG image (w,h) (sprintf "%s.%08d.png" prefix n)

    //execution
    let rec launch() =
        match !active with
        | Running _ -> ()           
        | Paused when !stepTodo>0 ->
            active   := Running false
            let context = System.Threading.SynchronizationContext.Current            
            Async.Start(
                async {
                    let nSteps = min !stepTodo !stepSize
                    do  stepTodo := !stepTodo - nSteps
                    do  stepDone := !stepDone + nSteps            
                    do! Async.SwitchToThreadPool()
                    do  sw.Reset(); sw.Start(); s.Iterate nSteps; sw.Stop()
                    do  execUI(fun () ->
                            runtime  := !runtime + sw.ElapsedMilliseconds
                            speedAve := (float !stepDone / (float !runtime / 1000.0))
                            speedNow := (float nSteps    / (float  sw.ElapsedMilliseconds / 1000.0))
                            drawingImage.Drawing <- s.Render();
                            reportStatus();
                            if !stepDone % 1000 = 0 then
                                match s.DropImagePrefix() with
                                | None -> ()
                                | Some (w,h,prefix) -> dropImage !stepDone (w,h) prefix
                            match !active with
                            | Paused -> failwith "unexpected Paused when finished a running step"
                            | Running pendingReset -> 
                                active := Paused                                
                                if pendingReset then                                
                                    reset()
                                else
                                    launch())
                    })
        | Paused -> ()
    
    //definition of how simu steps
    let step n k = stepTodo := !stepTodo + (n*k)
                   stepSize := k
                   launch()

    //functions writing data to files
    let writeCells() = let cells = s.Cells()
                       writeCells !stepDone cells
                       launch()

    //let writeMarkedCells() = let cells = s.Cells()
                             //writeMarkedCellLocations !stepDone cells
                             //launch()

    let writeMovementRate() = let cells = s.Cells()
                              writeMovementRate !stepDone cells
                              launch()
 
    let writeDivisionRate() = let cells = s.Cells()
                              writeDivisionRate !stepDone cells
                              launch()

    let writeDeadCellData() = writeDeadCellData !stepDone deadCells
                              launch()

    let writeFertRate() = let cells = s.Cells()
                          writeFertRate !stepDone
                          launch()
    
    (*let running max s worms = let mutable i = 0
                              while i < Array.length worms-1 do
                                    worm <- worms.[i]
                                    step max s
                                    i <- i+1*)

    //defining what happens when buttons are clicked
    resetB.Click.Add(fun _    -> reset())
    step1.Click.Add(fun _     -> step 1 1)
    step10.Click.Add(fun _    -> step 1 10)
    step100.Click.Add(fun _   -> step 1 100)
    step500.Click.Add(fun _   -> step 1 500)
    step1000.Click.Add(fun _  -> step 1 1000)
    run10.Click.Add(fun _     -> step 10 1)
    run100.Click.Add(fun _    -> step 100 1)
    run500.Click.Add(fun _    -> step 500 1)
    run1000.Click.Add(fun _   -> step 100 10)
    run100000.Click.Add(fun _ -> step 10000 10)
    run100000000.Click.Add(fun _ -> step 10000000 10)
    stop.Click.Add(fun _      -> stepTodo := 0)
    write.Click.Add(fun _     -> writeCells())
    moverate.Click.Add(fun _     -> writeMovementRate())
    //marked.Click.Add(fun _     -> writeMarkedCells())
    divided.Click.Add(fun _     -> writeDivisionRate())
    fert.Click.Add(fun _     -> writeFertRate())
    dead.Click.Add(fun _     -> writeDeadCellData())
    drawingImage.Drawing <- s.Render()
    win