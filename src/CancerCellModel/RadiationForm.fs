module RadiationForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open ModelParameters
open Geometry
open RadiationEvent

type RadiationForm() as this =
    inherit ParamFormBase(Width=900, Height = 500)

    let g0g1_alpha_textbox = new TextBox()
    let g0g1_beta_textbox = new TextBox()
    let s_alpha_textbox = new TextBox()
    let s_beta_textbox = new TextBox()
    let g2m_alpha_textbox = new TextBox()
    let g2m_beta_textbox = new TextBox()
    let g0g1_sf_textbox = new TextBox()
    let s_sf_textbox = new TextBox()
    let g2m_sf_textbox = new TextBox()
    let radiation_dose_textbox = new TextBox()
    let radiation_time_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.G0G1RadiationParam <- (FormDesigner.retrieve_float(g0g1_alpha_textbox), FormDesigner.retrieve_float(g0g1_beta_textbox))
        ModelParameters.SRadiationParam <- (FormDesigner.retrieve_float(s_alpha_textbox), FormDesigner.retrieve_float(s_beta_textbox))
        ModelParameters.G2MRadiationParam <- (FormDesigner.retrieve_float(g2m_alpha_textbox), FormDesigner.retrieve_float(g2m_beta_textbox))
        ModelParameters.RadiationDose <- FormDesigner.retrieve_float(radiation_dose_textbox)
        ModelParameters.RadiationEventTimeStep <- FormDesigner.retrieve_int(radiation_time_textbox)

    let update_data(g0g1_sf_textbox: TextBox, s_sf_textbox: TextBox, g2m_sf_textbox: TextBox) =
        g0g1_sf_textbox.Text <- (sprintf "%.4f" RadiationEvent.G0G1PhaseSF)
        s_sf_textbox.Text <- (sprintf "%.4f" RadiationEvent.SPhaseSF)
        g2m_sf_textbox.Text <- (sprintf "%.4f" RadiationEvent.G2MPhaseSF)

    do 
        base.Text <- "Radiation event"

        let radiation_groupbox = new GroupBox()
        radiation_groupbox.Text <- "Radiation"
        radiation_groupbox.Size <- Drawing.Size(base.Size.Width - FormDesigner.x_interval, base.Size.Height - FormDesigner.y_interval)
        radiation_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        radiation_groupbox.ClientSize <- Drawing.Size(int (float radiation_groupbox.Size.Width * 0.9), int (float radiation_groupbox.Size.Height * 0.9))

        let (g0g1_alpha, g0g1_beta) = ModelParameters.G0G1RadiationParam
        let (s_alpha, s_beta) = ModelParameters.SRadiationParam
        let (g2m_alpha, g2m_beta) = ModelParameters.G2MRadiationParam
        g0g1_alpha_textbox.Text <- (sprintf "%.4f" g0g1_alpha)
        g0g1_beta_textbox.Text <- (sprintf "%.4f" g0g1_beta)
        s_alpha_textbox.Text <- (sprintf "%.4f" s_alpha)
        s_beta_textbox.Text <- (sprintf "%.4f" s_beta)
        g2m_alpha_textbox.Text <- (sprintf "%.4f" g2m_alpha)
        g2m_beta_textbox.Text <- (sprintf "%.4f" g2m_beta)
        radiation_dose_textbox.Text <- (sprintf "%.2f" ModelParameters.RadiationDose)
        radiation_time_textbox.Text <- (sprintf "%d" ModelParameters.RadiationEventTimeStep)

        let radiation_param_label = new Label()
        radiation_param_label.Text <- "Radiation parameters:\n\n\
                                        The surviving fraction (SF) of cells following a radiation event is calculated as follows for each cell cycle phase,\n\n                \
                                        SF = exp( (-alpha*radiation_dose) + (-beta*radiation_dose^2) )\n"
        radiation_param_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (20, 5))
        radiation_param_label.AutoSize <- true
        radiation_param_label.Location <- FormDesigner.initial_location

        let g0g1_label = new Label()
        g0g1_label.Text <- "G0, G1 phases:"
        g0g1_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0g1_label.AutoSize <- true

        let g0g1_alpha_label = new Label()
        g0g1_alpha_label.Text <- "G0, G1 alpha:"
        g0g1_alpha_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0g1_alpha_label.AutoSize <- true

        let g0g1_beta_label = new Label()
        g0g1_beta_label.Text <- "G0, G1 beta:"
        g0g1_beta_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0g1_beta_label.AutoSize <- true

        let g0g1_sf_label = new Label()
        g0g1_sf_label.Text <- "G0, G1 surviving fraction:"
        g0g1_sf_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0g1_sf_label.AutoSize <- true

        FormDesigner.place_control_below(g0g1_label, radiation_param_label, 20)
        FormDesigner.place_control_totheright(g0g1_alpha_label, g0g1_label)
        FormDesigner.place_control_totheright(g0g1_alpha_textbox, g0g1_alpha_label)
        FormDesigner.place_control_totheright(g0g1_beta_label, g0g1_alpha_textbox)
        FormDesigner.place_control_totheright(g0g1_beta_textbox, g0g1_beta_label)
        FormDesigner.place_control_below(g0g1_sf_label, g0g1_alpha_label)
        g0g1_sf_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g0g1_sf_textbox, g0g1_sf_label)

        let s_label = new Label()
        s_label.Text <- "S phase:"
        s_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_label.AutoSize <- true

        let s_alpha_label = new Label()
        s_alpha_label.Text <- "S alpha:"
        s_alpha_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_alpha_label.AutoSize <- true

        let s_beta_label = new Label()
        s_beta_label.Text <- "S beta:"
        s_beta_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_beta_label.AutoSize <- true

        let s_sf_label = new Label()
        s_sf_label.Text <- "S surviving fraction:"
        s_sf_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_sf_label.AutoSize <- true

        FormDesigner.place_control_below(s_label, g0g1_label, 40)
        FormDesigner.place_control_totheright(s_alpha_label, s_label)
        FormDesigner.place_control_totheright(s_alpha_textbox, s_alpha_label)
        FormDesigner.place_control_totheright(s_beta_label, s_alpha_textbox)
        FormDesigner.place_control_totheright(s_beta_textbox, s_beta_label)
        FormDesigner.place_control_below(s_sf_label, s_alpha_label)
        s_sf_textbox.Enabled <- false
        FormDesigner.place_control_totheright(s_sf_textbox, s_sf_label)

        let g2m_label = new Label()
        g2m_label.Text <- "G2/M phase:"
        g2m_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_label.AutoSize <- true

        let g2m_alpha_label = new Label()
        g2m_alpha_label.Text <- "G2/M alpha:"
        g2m_alpha_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_alpha_label.AutoSize <- true

        let g2m_beta_label = new Label()
        g2m_beta_label.Text <- "G2/M beta:"
        g2m_beta_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_beta_label.AutoSize <- true

        let g2m_sf_label = new Label()
        g2m_sf_label.Text <- "G2/M surviving fraction:"
        g2m_sf_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_sf_label.AutoSize <- true

        FormDesigner.place_control_below(g2m_label, s_label, 40)
        FormDesigner.place_control_totheright(g2m_alpha_label, g2m_label)
        FormDesigner.place_control_totheright(g2m_alpha_textbox, g2m_alpha_label)
        FormDesigner.place_control_totheright(g2m_beta_label, g2m_alpha_textbox)
        FormDesigner.place_control_totheright(g2m_beta_textbox, g2m_beta_label)
        FormDesigner.place_control_below(g2m_sf_label, g2m_alpha_label)
        g2m_sf_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g2m_sf_textbox, g2m_sf_label)

        let radiation_dose_label = new Label()
        radiation_dose_label.Text <- "Radiation dose:"
        radiation_dose_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 1))
        radiation_dose_label.AutoSize <- true
        FormDesigner.place_control_below(radiation_dose_label, g2m_label, 50)
        FormDesigner.place_control_totheright(radiation_dose_textbox, radiation_dose_label)

        let radiation_time_label = new Label()
        radiation_time_label.Text <- "Radiation event time (simulation step):"
        radiation_time_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 1))
        radiation_time_label.AutoSize <- true
        FormDesigner.place_control_below(radiation_time_label, radiation_dose_label)
        FormDesigner.place_control_totheright(radiation_time_textbox, radiation_time_label)

        radiation_groupbox.Controls.AddRange([| radiation_param_label;
                                                g0g1_label; g0g1_alpha_label; g0g1_alpha_textbox; g0g1_beta_label; g0g1_beta_textbox; g0g1_sf_label; g0g1_sf_textbox;
                                                s_label; s_alpha_label; s_alpha_textbox; s_beta_label; s_beta_textbox; s_sf_label; s_sf_textbox;
                                                g2m_label; g2m_alpha_label; g2m_alpha_textbox; g2m_beta_label; g2m_beta_textbox; g2m_sf_label; g2m_sf_textbox 
                                                radiation_dose_label; radiation_dose_textbox; radiation_time_label; radiation_time_textbox |])


        ParamFormBase.create_ok_cancel_buttons(this, radiation_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([| radiation_groupbox |])

    override this.Refresh() =
            update_data(g0g1_sf_textbox, s_sf_textbox, g2m_sf_textbox)