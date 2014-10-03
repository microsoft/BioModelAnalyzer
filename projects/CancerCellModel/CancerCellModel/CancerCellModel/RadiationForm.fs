module RadiationForm

//open System
//open System.Windows.Forms
//open System.Windows.Forms.DataVisualization.Charting
//open ParamFormBase
//open ModelParameters
//open Geometry
//
//type RadiationForm() as this =
//    inherit ParamFormBase (Width=900, Height = 900)
//
//    let radiation_dose_textbox = new TextBox()
//    let radiation_time_textbox = new TextBox()
//
//    let apply_changes(args: EventArgs) =
//        ModelParameters.RadiationDose <- FormDesigner.retrieve_int(radiation_dose_textbox)
//        ModelParameters.RadiationTime <- FormDesigner.retrieve_int(radiation_time_textbox)
//
//    do 
//        base.Text <- "Radiation"
//        base.ClientSize <- Drawing.Size(base.Size.Width, base.Size.Height)
//
//        let radiation_groupbox = new GroupBox()
//        radiation_groupbox.Text <- "Radiation"
//        radiation_groupbox.Size <- DrawingSize(int (float base.Size.Width*0.9), base.Size.Height - 4*FormDesigner.y_interval)
//        radiation_groupbox.Location <- DrawingPoint(FormDesigner.x_interval, FormDesigner.y_interval)
//        radiation_groupbox.ClientSize <- DrawingSize(int (float radiation_groupbox.Size.Width * 0.9), int (float radiation_groupbox.Size.Height * 0.9))