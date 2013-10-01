module ExternalStateParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters


type ExternalStateParamForm() =
    inherit ParamFormBase(Width = 1200, Height = 350)

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let c3_textbox = new TextBox()
    let egf_textbox = new TextBox()

    let apply_nutrient_changes(args: EventArgs) =
        ModelParameters.O2Param <- (ParamFormBase.retrieve_float(c1_textbox),
            ParamFormBase.retrieve_float(c2_textbox), ParamFormBase.retrieve_float(c3_textbox))

    let apply_pathway_changes(args: EventArgs) = 
        ModelParameters.EGFProb <- ParamFormBase.retrieve_float(egf_textbox)

    do
        /////////////////////// NUTRIENTS ///////////////////////////////

        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* ParamFormBase.x_interval, int ( float base.Size.Height *0.7))
        nutrient_groupbox.Location <- ParamFormBase.initial_location
        nutrient_groupbox.ClientSize <- Drawing.Size(int (float nutrient_groupbox.Size.Width * 0.9),
                                                        int (float nutrient_groupbox.Size.Height*0.9))

        let func_label = new Label()
        func_label.Text <- "The concentration of oxygen:\n\n\
                            O2(t + dt) = O2(t) + dt*(c1 - c2*NumOfDividingCells(t) - c3*NumOfLiveNonDividingCells(t))\n\n\
                            where c1 is supply (per time step),\n\
                            c2 is consumption of dividing cells (per time step),\n\
                            c3 is consumption of non-dividing live cells (per time step)"

        func_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (10, 7))
        func_label.AutoSize <- true
        func_label.Location <- ParamFormBase.initial_location

        let (c1, c2, c3) = ModelParameters.O2Param
        let c1_label = new Label()
        c1_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 1))
        c1_label.AutoSize <- true
        c1_label.Text <- "c1"
        ParamFormBase.place_control_below(c1_label, func_label)

        c1_textbox.Text <- (sprintf "%.1f" c1)
        ParamFormBase.add_textbox_float_validation(c1_textbox, c1_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c1_textbox, c1_label)

        let c2_label = new Label()
        c2_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 1))
        c2_label.AutoSize <- true
        c2_label.Text <- "c2"
        ParamFormBase.place_control_totheright(c2_label, c1_textbox)

        c2_textbox.Text <- (sprintf "%.1f" c2)
        ParamFormBase.add_textbox_float_validation(c2_textbox, c2_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c2_textbox, c2_label)

        let c3_label = new Label()
        c3_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 1))
        c3_label.AutoSize <- true
        c3_label.Text <- "c3"
        ParamFormBase.place_control_totheright(c3_label, c2_textbox)

        c3_textbox.Text <- (sprintf "%.1f" c3)
        ParamFormBase.add_textbox_float_validation(c3_textbox, c3_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c3_textbox, c3_label)

        nutrient_groupbox.Controls.AddRange([|func_label; c1_label; c1_textbox; c2_label; c2_textbox; c3_label; c3_textbox|])
        ParamFormBase.create_apply_button(nutrient_groupbox, c1_label, apply_nutrient_changes) |> ignore

        ///////////////////// PATHWAYS ///////////////////////////////////////
        
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
        ParamFormBase.create_apply_button(pathway_groupbox, egf_label, apply_pathway_changes) |> ignore

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])