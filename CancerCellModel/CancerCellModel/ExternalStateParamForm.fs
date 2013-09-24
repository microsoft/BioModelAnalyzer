module ExternalStateParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters


type ExternalStateParamForm() =
    inherit ParamFormBase(Width = 1000, Height = 250)

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let egf_textbox = new TextBox()

    do
        // OXYGEN
        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* ParamFormBase.x_interval, int ( float base.Size.Height *0.7))
        nutrient_groupbox.Location <- ParamFormBase.initial_location
        nutrient_groupbox.ClientSize <- Drawing.Size(int (float nutrient_groupbox.Size.Width * 0.9),
                                                        int (float nutrient_groupbox.Size.Height*0.9))

        let func_label = new Label()
        func_label.Text <- "Oxygen\n O2(t + dt) = O2(t) + c1*dt - c2*dt*NumOfLiveCells(t)"
        let width = ParamFormBase.max_label_size.Width
        let height = ParamFormBase.max_label_size.Height 
        func_label.MaximumSize <- Drawing.Size(4*width, height)
        func_label.AutoSize <- true
        func_label.Location <- ParamFormBase.initial_location

        let (c1, c2) = ModelParameters.O2Param
        let c1_label = new Label()
        c1_label.Width <- ParamFormBase.label_width
        c1_label.Text <- "c1"
        ParamFormBase.place_control_below(c1_label, func_label)

        c1_textbox.Text <- (sprintf "%.1f" c1)
        ParamFormBase.add_textbox_float_validation(c1_textbox, c1_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c1_textbox, c1_label)

        let c2_label = new Label()
        c2_label.Width <- ParamFormBase.label_width
        c2_label.Text <- "c2"
        ParamFormBase.place_control_totheright(c2_label, c1_textbox)

        c2_textbox.Text <- (sprintf "%.1f" c2)
        ParamFormBase.add_textbox_float_validation(c2_textbox, c2_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c2_textbox, c2_label)

        nutrient_groupbox.Controls.AddRange([|func_label; c1_label; c1_textbox; c2_label; c2_textbox|])

        let pathway_groupbox = new GroupBox()
        pathway_groupbox.Text <- "Pathways"
        pathway_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* ParamFormBase.x_interval, int ( float base.Size.Height *0.7))
        ParamFormBase.place_control_totheright(pathway_groupbox, nutrient_groupbox)
        pathway_groupbox.ClientSize <- Drawing.Size(int (float pathway_groupbox.Size.Width * 0.9),
                                                        int (float pathway_groupbox.Size.Height*0.9))

        let egf_label = new Label()
        egf_label.Text <- "The probability that EGF is Up"
        egf_label.Location <- ParamFormBase.initial_location

        egf_textbox.Text <- (sprintf "%.1f" ModelParameters.EGFProb)
        ParamFormBase.place_control_totheright(egf_textbox, egf_label)

        pathway_groupbox.Controls.AddRange([| egf_label; egf_textbox |])

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])