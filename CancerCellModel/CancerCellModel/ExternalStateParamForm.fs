module ExtStateParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters


type ExtStateParamForm() =
    inherit ParamFormBase()

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let egf_textbox = new TextBox()

    do
        // OXYGEN
        let oxygen_groupbox = new GroupBox()
        oxygen_groupbox.Text <- "Oxygen"
        oxygen_groupbox.Size <- Drawing.Size(base.ClientSize.Width, base.ClientSize.Height)
        oxygen_groupbox.Location <- ParamFormBase.initial_location
        oxygen_groupbox.ClientSize <- Drawing.Size(int (float oxygen_groupbox.Size.Width * 0.9),
                                                        int (float oxygen_groupbox.Size.Height*0.9))

        let func_label = new Label()
        func_label.Text <- "O2(t + dt) = O2(t) + c1*dt - c2 *dt*LiveCells(t)"
        func_label.Location <- ParamFormBase.initial_location

        let c1_label = new Label()
        c1_label.MaximumSize <- ParamFormBase.max_label_size
        sym_div_label.AutoSize <- true
        sym_div_label.Text <- "c1"
        ParamFormBase.place_control_below(sym_div_label, control)