module CellCycleParamForm

open System
open System.Windows.Forms
open ParamFormBase
open ModelParameters
open Geometry

type CellCycleParamForm() as this =
    inherit ParamFormBase(Width=800, Height=500)

    let stem_G1_min_textbox = new TextBox()
    let stem_G1_max_textbox = new TextBox()
    let stem_S_min_textbox = new TextBox()
    let stem_S_max_textbox = new TextBox()
    let stem_G2M_min_textbox = new TextBox()
    let stem_G2M_max_textbox = new TextBox()
    let nonstem_G1_min_textbox = new TextBox()
    let nonstem_G1_max_textbox = new TextBox()
    let nonstem_S_min_textbox = new TextBox()
    let nonstem_S_max_textbox = new TextBox()
    let nonstem_G2M_min_textbox = new TextBox()
    let nonstem_G2M_max_textbox = new TextBox()

    let apply_changes(args: EventArgs) = 
        ModelParameters.StemWaitBeforeCommitToDivideInterval <- FormDesigner.retrieve_int_interval(stem_G1_min_textbox, stem_G1_max_textbox)
        ModelParameters.StemWaitBeforeG2MInterval <- FormDesigner.retrieve_int_interval(stem_S_min_textbox, stem_S_max_textbox)
        ModelParameters.StemWaitBeforeDivisionInterval <- FormDesigner.retrieve_int_interval(stem_G2M_min_textbox, stem_G2M_max_textbox)
        ModelParameters.NonStemWaitBeforeCommitToDivideInterval <- FormDesigner.retrieve_int_interval(nonstem_G1_min_textbox, nonstem_G1_max_textbox)
        ModelParameters.NonStemWaitBeforeG2MInterval <- FormDesigner.retrieve_int_interval(nonstem_S_min_textbox, nonstem_S_max_textbox)
        ModelParameters.NonStemWaitBeforeDivisionInterval <- FormDesigner.retrieve_int_interval(nonstem_G2M_min_textbox, nonstem_G2M_max_textbox)

    let create_cell_cycle_param_controls(g1_min_textbox: TextBox, g1_max_textbox: TextBox, s_min_textbox: TextBox, s_max_textbox: TextBox, g2m_min_textbox: TextBox, g2m_max_textbox: TextBox, parent: Control) =
        
        let cell_cycle_param_label = new Label()
        cell_cycle_param_label.Text <- "Cell cycle parameters:"
        cell_cycle_param_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (6, 1))
        cell_cycle_param_label.AutoSize <- true
        cell_cycle_param_label.Location <- FormDesigner.initial_location

        let g1_label = new Label()
        g1_label.Text <- "Time in G1 (hours):"
        g1_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g1_label.AutoSize <- true
        
        let g1_min_label = new Label()
        g1_min_label.Text <- "Minimum"
        g1_min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        g1_min_label.AutoSize <- true

        let g1_max_label = new Label()
        g1_max_label.Text <- "Maximum"
        g1_max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        g1_max_label.AutoSize <- true
        
        FormDesigner.place_control_below(g1_label, cell_cycle_param_label, 15)
        FormDesigner.place_control_totheright(g1_min_label, g1_label)
        FormDesigner.place_control_totheright(g1_min_textbox, g1_min_label)
        FormDesigner.place_control_totheright(g1_max_label, g1_min_textbox)
        FormDesigner.place_control_totheright(g1_max_textbox, g1_max_label)

        let s_label = new Label()
        s_label.Text <- "Time in S (hours):"
        s_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_label.AutoSize <- true

        let s_min_label = new Label()
        s_min_label.Text <- "Minimum"
        s_min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        s_min_label.AutoSize <- true

        let s_max_label = new Label()
        s_max_label.Text <- "Maximum"
        s_max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        s_max_label.AutoSize <- true

        FormDesigner.place_control_below(s_label, g1_label)
        FormDesigner.place_control_totheright(s_min_label, s_label)
        FormDesigner.place_control_totheright(s_min_textbox, s_min_label)
        FormDesigner.place_control_totheright(s_max_label, s_min_textbox)
        FormDesigner.place_control_totheright(s_max_textbox, s_max_label)

        let g2m_label = new Label()
        g2m_label.Text <- "Time in G2/M (hours):"
        g2m_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_label.AutoSize <- true

        let g2m_min_label = new Label()
        g2m_min_label.Text <- "Minimum"
        g2m_min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        g2m_min_label.AutoSize <- true

        let g2m_max_label = new Label()
        g2m_max_label.Text <- "Maximum"
        g2m_max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 1))
        g2m_max_label.AutoSize <- true

        FormDesigner.place_control_below(g2m_label, s_label)
        FormDesigner.place_control_totheright(g2m_min_label, g2m_label)
        FormDesigner.place_control_totheright(g2m_min_textbox, g2m_min_label)
        FormDesigner.place_control_totheright(g2m_max_label, g2m_min_textbox)
        FormDesigner.place_control_totheright(g2m_max_textbox, g2m_max_label)

        parent.Controls.AddRange([|cell_cycle_param_label; g1_label; g1_min_label; g1_min_textbox; g1_max_label; g1_max_textbox; 
                                    s_label; s_min_label; s_min_textbox; s_max_label; s_max_textbox; 
                                    g2m_label; g2m_min_label; g2m_min_textbox; g2m_max_label; g2m_max_textbox|])


    do 
        base.Text <- "Cell cycle parameters"

        // cell cycle parameters for stem cells 
        let stem_cell_cycle_groupbox = new GroupBox() 
        stem_cell_cycle_groupbox.Text <- "Stem cells"
        stem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        stem_cell_cycle_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        stem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float stem_cell_cycle_groupbox.Size.Width * 0.9),
                                                            int (float stem_cell_cycle_groupbox.Size.Height * 0.9))

        stem_G1_min_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeCommitToDivideInterval.Min/2))
        stem_G1_max_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeCommitToDivideInterval.Max/2))
        stem_S_min_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeG2MInterval.Min/2))
        stem_S_max_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeG2MInterval.Max/2))
        stem_G2M_min_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeDivisionInterval.Min/2))
        stem_G2M_max_textbox.Text <- (sprintf "%d" (ModelParameters.StemWaitBeforeDivisionInterval.Max/2)) 

        create_cell_cycle_param_controls(stem_G1_min_textbox, stem_G1_max_textbox, stem_S_min_textbox, stem_S_max_textbox, stem_G2M_min_textbox, stem_G2M_max_textbox, stem_cell_cycle_groupbox)

        // cell cycle parameters for non-stem cells
        let nonstem_cell_cycle_groupbox = new GroupBox()
        nonstem_cell_cycle_groupbox.Text <- "Non-stem cells"
        nonstem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        FormDesigner.place_control_below(nonstem_cell_cycle_groupbox, stem_cell_cycle_groupbox)
        nonstem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float nonstem_cell_cycle_groupbox.Size.Width * 0.9),
                                                                int (float nonstem_cell_cycle_groupbox.Size.Height * 0.9))

        nonstem_G1_min_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeCommitToDivideInterval.Min/2))
        nonstem_G1_max_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeCommitToDivideInterval.Max/2))
        nonstem_S_min_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeG2MInterval.Min/2))
        nonstem_S_max_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeG2MInterval.Max/2))
        nonstem_G2M_min_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeDivisionInterval.Min/2))
        nonstem_G2M_max_textbox.Text <- (sprintf "%d" (ModelParameters.NonStemWaitBeforeDivisionInterval.Max/2))

        create_cell_cycle_param_controls(nonstem_G1_min_textbox, nonstem_G1_max_textbox, nonstem_S_min_textbox, nonstem_S_max_textbox, nonstem_G2M_min_textbox, nonstem_G2M_max_textbox, nonstem_cell_cycle_groupbox)

        ParamFormBase.create_ok_cancel_buttons(this, stem_cell_cycle_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([|stem_cell_cycle_groupbox; nonstem_cell_cycle_groupbox|])