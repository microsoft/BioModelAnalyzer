module StemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters
open MyMath
open Geometry

type StemCellParamForm() as this =
    inherit ParamFormBase (Width=1200, Height = 900)
    
    let division_prob_chart = new Chart(Dock=DockStyle.Fill)
    let division_chart_area = new ChartArea()
    let division_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_textbox = new TextBox()
    let x2_textbox = new TextBox()
    let mu_textbox = new TextBox()
    let s_textbox = new TextBox()
    let min_prob_division_textbox = new TextBox()
    let max_prob_division_textbox = new TextBox()
    let sym_div_textbox = new TextBox()
    let min_division_interval_textbox = new TextBox()
    let max_division_interval_textbox = new TextBox()

    let nonstem_tostem_prob_chart = new Chart(Dock=DockStyle.Fill)
    let nonstem_tostem_chart_area = new ChartArea()
    let nonstem_tostem_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x3_textbox = new TextBox()
    let n_textbox = new TextBox()
    let min_nonstem_tostem_textbox = new TextBox()
    let max_nonstem_tostem_textbox = new TextBox()
    let tononstem_withmem_textbox = new TextBox()

    let model_params = new ModelParameters()

    let apply_changes(args: EventArgs) =
        ModelParameters.StemDivisionProbParam := ParamFormBase.retrieve_logistic_func_param(
                                                    x1_textbox, x2_textbox,
                                                    min_prob_division_textbox, max_prob_division_textbox)

        ModelParameters.SymRenewProb <- FormDesigner.retrieve_float(sym_div_textbox) / float 100
        ModelParameters.StemIntervalBetweenDivisions <- IntInterval(FormDesigner.retrieve_int(min_division_interval_textbox),
                                                            FormDesigner.retrieve_int(max_division_interval_textbox))
        ModelParameters.NonStemToStemProbParam := ParamFormBase.retrieve_shiftexp_func_param(
                                                    x3_textbox, min_nonstem_tostem_textbox, max_nonstem_tostem_textbox)

        ModelParameters.StemToNonStemProbParam <- FormDesigner.retrieve_float(tononstem_withmem_textbox)

    // initialise the window
    do
        (*-------------- STEM CELLS--------------------------*)
        base.Text <- "Stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width, base.Size.Height)

        division_prob_chart.Titles.Add("The probability of stem cell division") |> ignore
        division_prob_chart.ChartAreas.Add(division_chart_area)
        division_prob_chart.Series.Add(division_prob_series)
        division_chart_area.AxisX.Title <- "The concentration of oxygen (%)"
        division_chart_area.AxisY.Title <- "The probability"

        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* FormDesigner.x_interval, base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        let control = ParamFormBase.create_logistic_func_controls(
                                 division_groupbox, null, division_prob_chart,
                                 (*x1_textbox, x2_textbox, mu_textbox, s_textbox,
                                 min_prob_division_textbox, max_prob_division_textbox,*)
                                 ModelParameters.StemDivisionProbParam,
                                 (ExternalState.O2Limits))

        let sym_div_label  = new Label()
        sym_div_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2,3))
        sym_div_label.AutoSize <- true
        sym_div_label.Text <- "The probability of symmetric division (%)"
        FormDesigner.place_control_below(sym_div_label, control, 2*FormDesigner.y_interval)

        sym_div_textbox.Text <- (sprintf "%.1f" (ModelParameters.SymRenewProb* float 100) )
        FormDesigner.add_textbox_float_validation(sym_div_textbox, sym_div_label.Text, FloatInterval(0., 100.))
        FormDesigner.place_control_totheright(sym_div_textbox, sym_div_label)

        ParamFormBase.create_int_interval_controls(
                    division_groupbox, sym_div_label,
                    "Time (in steps) before two consecutive divisions",
                    min_division_interval_textbox, max_division_interval_textbox,
                    ModelParameters.StemIntervalBetweenDivisions) |> ignore

        division_groupbox.Controls.AddRange([| sym_div_label; sym_div_textbox |])


        (*-------------- NON-STEM WITH MEMORY CELLS--------------------------*)
        let nonstem_withmem_groupbox = new GroupBox()
        nonstem_withmem_groupbox.Text <- "Transitions to the \"non-stem with memory\" state"
        nonstem_withmem_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2*FormDesigner.x_interval, base.ClientSize.Height)
        nonstem_withmem_groupbox.Location <- Drawing.Point(division_groupbox.Location.X + division_groupbox.Size.Width + FormDesigner.x_interval,
                                                            division_groupbox.Location.Y)
        nonstem_withmem_groupbox.ClientSize <- Drawing.Size(int (float nonstem_withmem_groupbox.Size.Width * 0.9),
                                                            int (float nonstem_withmem_groupbox.Size.Height * 0.9) )

        let tononstem_withmem_label = new Label()
        tononstem_withmem_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        tononstem_withmem_label.AutoSize <- true
        tononstem_withmem_label.Text <- "The probability of transition to the \"non-stem with memory\" state (%)"
        tononstem_withmem_label.Location <- FormDesigner.initial_location

        tononstem_withmem_textbox.Text <- (sprintf "%.1f" (ModelParameters.StemToNonStemProbParam * (float 100)))
        FormDesigner.add_textbox_float_validation(tononstem_withmem_textbox, tononstem_withmem_label.Text, FloatInterval(0., 100.))
        FormDesigner.place_control_totheright(tononstem_withmem_textbox, tononstem_withmem_label)

        nonstem_tostem_prob_chart.Titles.Add("The probability of returning from the \"non-stem with memory\" state to the stem state") |> ignore
        nonstem_tostem_prob_chart.ChartAreas.Add(nonstem_tostem_chart_area)
        nonstem_tostem_prob_chart.Series.Add(nonstem_tostem_prob_series)
        nonstem_tostem_chart_area.AxisX.Title <- "The number of stem cells"
        nonstem_tostem_chart_area.AxisY.Title <- "The probability"

        let x3 = (!ModelParameters.NonStemToStemProbParam).P2.x
        let control = ParamFormBase.create_shiftexp_func_controls(nonstem_withmem_groupbox, tononstem_withmem_label, nonstem_tostem_prob_chart,
                                                (*x3_textbox, n_textbox, min_nonstem_tostem_textbox, max_nonstem_tostem_textbox,*)
                                                ModelParameters.NonStemToStemProbParam, FloatInterval(0., x3))
        
        ParamFormBase.create_ok_cancel_buttons(this, nonstem_withmem_groupbox, apply_changes) |> ignore
        nonstem_withmem_groupbox.Controls.AddRange([|tononstem_withmem_label; tononstem_withmem_textbox|])

        base.Controls.AddRange([| division_groupbox; nonstem_withmem_groupbox |])

(*    member this.init() =
        ParamFormBase.refresh_logistic_func_chart(
                        division_prob_chart, x1_textbox, x2_textbox, 
                                 min_prob_division_textbox, max_prob_division_textbox,
                                 mu_textbox, s_textbox,
                                 (ExternalState.O2Limits))


        ParamFormBase.refresh_exp_func_chart(nonstem_tostem_prob_chart, x3_textbox,
                        min_nonstem_tostem_textbox, max_nonstem_tostem_textbox,
                        n_textbox)*)