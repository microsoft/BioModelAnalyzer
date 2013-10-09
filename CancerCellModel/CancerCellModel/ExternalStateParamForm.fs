module ExternalStateParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters


type ExternalStateParamForm() as this =
    inherit ParamFormBase(Width = 1000, Height = 700)

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let c3_textbox = new TextBox()
    let k_textbox = new TextBox()
    let Nmax_textbox = new TextBox()
    let egf_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.O2Param <- (ParamFormBase.retrieve_float(c1_textbox),
            ParamFormBase.retrieve_float(c2_textbox), ParamFormBase.retrieve_float(c3_textbox),
            ParamFormBase.retrieve_float(k_textbox))

        ModelParameters.MaxNumOfCells <- ParamFormBase.retrieve_int(Nmax_textbox)

        ModelParameters.EGFProb <- ParamFormBase.retrieve_float(egf_textbox)

    do
        /////////////////////// NUTRIENTS ///////////////////////////////

        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(base.Size.Width/2 - 2* ParamFormBase.x_interval, int ( float base.Size.Height *0.7))
        nutrient_groupbox.Location <- ParamFormBase.initial_location
        nutrient_groupbox.ClientSize <- Drawing.Size(int (float nutrient_groupbox.Size.Width * 0.9),
                                                        int (float nutrient_groupbox.Size.Height*0.9))

        let o2_func_label = new Label()
        o2_func_label.Text <- "The concentration of oxygen:\n\n\
                            O2(t + dt) = O2(t) + dt*(c1 - c2*NumOfDividingCells(t) -\n            \
                            c3*NumOfLiveNonDividingCells(t))"

        o2_func_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (10, 4))
        o2_func_label.AutoSize <- true
        o2_func_label.Location <- ParamFormBase.initial_location

        let (c1, c2, c3, k) = ModelParameters.O2Param
        let c1_label = new Label()
        c1_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (5, 2))
        c1_label.AutoSize <- true
        c1_label.Text <- "c1 (Supply of oxygen per time step)"
        ParamFormBase.place_control_below(c1_label, o2_func_label)

        c1_textbox.Text <- (sprintf "%.3f" c1)
        ParamFormBase.add_textbox_float_validation(c1_textbox, c1_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c1_textbox, c1_label)

        let c2_label = new Label()
        c2_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (5, 2))
        c2_label.AutoSize <- true
        c2_label.Text <- "c2 (Consumption of oxygen by dividing cells per time step)"
        ParamFormBase.place_control_below(c2_label, c1_label)

        c2_textbox.Text <- (sprintf "%.3f" c2)
        ParamFormBase.add_textbox_float_validation(c2_textbox, c2_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c2_textbox, c2_label)

        let c3_label = new Label()
        c3_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (5, 2))
        c3_label.AutoSize <- true
        c3_label.Text <- "c3 (Consumption of oxygen by non-dividing live cells per time step)"
        ParamFormBase.place_control_below(c3_label, c2_label)

        c3_textbox.Text <- (sprintf "%.3f" c3)
        ParamFormBase.add_textbox_float_validation(c3_textbox, c3_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(c3_textbox, c3_label)

        let o2_percell_func_label = new Label()
        o2_percell_func_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (10, 4))
        o2_percell_func_label.AutoSize <- true
        o2_percell_func_label.Text <- "The concentration of oxygen per one cell:\n\n\
                O2_per_cell(t) = O2(t)/(k*(NumOfNonDividingLiveCells +\n             \
                (c2/c3) * NumOfDividingCells))"
        ParamFormBase.place_control_below(o2_percell_func_label, c3_label)

        let k_label = new Label()
        k_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (5, 2))
        k_label.AutoSize <- true
        k_label.Text <- "k (Oxygen interaction constant - \
                reflects how much effect a cell can have on O2)"
        ParamFormBase.place_control_below(k_label, o2_percell_func_label)

        k_textbox.Text <- (sprintf "%.3f" k)
        ParamFormBase.add_textbox_float_validation(k_textbox, k_label.Text, (float 0, Double.MaxValue))
        ParamFormBase.place_control_totheright(k_textbox, k_label)

        let Nmax_label = new Label()
        Nmax_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (5, 2))
        Nmax_label.AutoSize <- true
        Nmax_label.Text <- "The maximum number of cells in the model"
        ParamFormBase.place_control_below(Nmax_label, k_label)

        Nmax_textbox.Text <- (sprintf "%d" ModelParameters.MaxNumOfCells)
        ParamFormBase.add_textbox_int_validation(Nmax_textbox, k_label.Text, (0, Int32.MaxValue))
        ParamFormBase.place_control_totheright(Nmax_textbox, Nmax_label)        

        nutrient_groupbox.Controls.AddRange([| o2_func_label; c1_label; c1_textbox; c2_label; c2_textbox; c3_label; c3_textbox;
            o2_percell_func_label; k_label; k_textbox |])

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
        ParamFormBase.create_ok_cancel_buttons(this, pathway_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])