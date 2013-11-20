module ExternalStateParamForm

open System
open System.Windows.Forms
open ParamFormBase
open ModelParameters
open Geometry

type ExternalStateParamForm() as this =
    inherit ParamFormBase(Width = 1000, Height = 700)

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let c3_textbox = new TextBox()
    let egf_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.O2Param <- (FormDesigner.retrieve_float(c1_textbox),
            FormDesigner.retrieve_float(c2_textbox), FormDesigner.retrieve_float(c3_textbox))

        ModelParameters.EGFProb <- FormDesigner.retrieve_float(egf_textbox)

    do
        /////////////////////// NUTRIENTS ///////////////////////////////

        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* FormDesigner.x_interval, int ( float base.Size.Height *0.7))
        nutrient_groupbox.Location <- FormDesigner.initial_location
        nutrient_groupbox.ClientSize <- Drawing.Size(int (float nutrient_groupbox.Size.Width * 0.9),
                                                        int (float nutrient_groupbox.Size.Height*0.9))

        let o2_func_label = new Label()
        o2_func_label.Text <- "The concentration of oxygen:\n\n\
                            O2(t + dt) = O2(t) + dt*(c1 - c2*NumOfDividingCells(t) -\n            \
                            c3*NumOfLiveNonDividingCells(t))"

        o2_func_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10, 4))
        o2_func_label.AutoSize <- true
        o2_func_label.Location <- FormDesigner.initial_location

        let (c1, c2, c3) = ModelParameters.O2Param
        let c1_label = new Label()
        c1_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c1_label.AutoSize <- true
        c1_label.Text <- "c1 (Supply of oxygen per time step)"
        FormDesigner.place_control_below(c1_label, o2_func_label)

        c1_textbox.Text <- (sprintf "%.3f" c1)
        FormDesigner.add_textbox_float_validation(c1_textbox, c1_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c1_textbox, c1_label)

        let c2_label = new Label()
        c2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c2_label.AutoSize <- true
        c2_label.Text <- "c2 (Consumption of oxygen by dividing cells per time step)"
        FormDesigner.place_control_below(c2_label, c1_label)

        c2_textbox.Text <- (sprintf "%.3f" c2)
        FormDesigner.add_textbox_float_validation(c2_textbox, c2_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c2_textbox, c2_label)

        let c3_label = new Label()
        c3_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c3_label.AutoSize <- true
        c3_label.Text <- "c3 (Consumption of oxygen by non-dividing live cells per time step)"
        FormDesigner.place_control_below(c3_label, c2_label)

        c3_textbox.Text <- (sprintf "%.3f" c3)
        FormDesigner.add_textbox_float_validation(c3_textbox, c3_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c3_textbox, c3_label)

        let o2_percell_func_label = new Label()
        o2_percell_func_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10, 4))
        o2_percell_func_label.AutoSize <- true
        o2_percell_func_label.Text <- "The concentration of oxygen per one cell:\n\n\
                O2_per_cell(t) = O2(t)/(k*(NumOfNonDividingLiveCells +\n             \
                (c2/c3) * NumOfDividingCells))"
        FormDesigner.place_control_below(o2_percell_func_label, c3_label)

        let Nmax_label = new Label()
        Nmax_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        Nmax_label.AutoSize <- true
        Nmax_label.Text <- "The maximum number of cells in the model"
        FormDesigner.place_control_below(Nmax_label, o2_percell_func_label)

        nutrient_groupbox.Controls.AddRange([| o2_func_label; c1_label; c1_textbox; c2_label; c2_textbox; c3_label; c3_textbox;
            o2_percell_func_label |])

        ///////////////////// PATHWAYS ///////////////////////////////////////
        
        let pathway_groupbox = new GroupBox()
        pathway_groupbox.Text <- "Pathways"
        pathway_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* FormDesigner.x_interval, int ( float base.Size.Height *0.7))
        FormDesigner.place_control_totheright(pathway_groupbox, nutrient_groupbox)
        pathway_groupbox.ClientSize <- Drawing.Size(int (float pathway_groupbox.Size.Width * 0.9),
                                                        int (float pathway_groupbox.Size.Height*0.9))

        let egf_label = new Label()
        egf_label.Text <- "The probability that EGF is Up"
        egf_label.Location <- FormDesigner.initial_location

        egf_textbox.Text <- (sprintf "%.1f" ModelParameters.EGFProb)
        FormDesigner.place_control_totheright(egf_textbox, egf_label)

        pathway_groupbox.Controls.AddRange([| egf_label; egf_textbox |])
        ParamFormBase.create_ok_cancel_buttons(this, pathway_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])