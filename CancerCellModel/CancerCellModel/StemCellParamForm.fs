module StemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters

type StemCellParamForm() =
    inherit ParamFormBase ()
    
    let division_prob_chart = new Chart(Dock=DockStyle.Fill)
    let division_chart_area = new ChartArea()
    let division_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_textbox = new TextBox()
    let x2_textbox = new TextBox()
    let mu_textbox = new TextBox()
    let s_textbox = new TextBox()
    let max_division_textbox = new TextBox()

    let nonstem_tostem_prob_chart = new Chart(Dock=DockStyle.Fill)
    let nonstem_tostem_chart_area = new ChartArea()
    let nonstem_tostem_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x3_textbox = new TextBox()
    let n_textbox = new TextBox()
    let max_nonstem_tostem_textbox = new TextBox()

    // plot the probability function of cell division
    
    // initialise the window
    do
        (*-------------- STEM CELLS--------------------------*)
        base.Text <- "Stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width - ParamFormBase.x_interval, base.Size.Height - ParamFormBase.x_interval )

        division_prob_chart.Titles.Add("The probability of cell division") |> ignore
        division_prob_chart.ChartAreas.Add(division_chart_area)
        division_prob_chart.Series.Add(division_prob_series)

        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* ParamFormBase.x_interval, base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(ParamFormBase.x_interval, ParamFormBase.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        let control = ParamFormBase.create_logistic_func_controls(
                                 division_groupbox, null, division_prob_chart,
                                 x1_textbox, x2_textbox, mu_textbox, s_textbox,
                                 max_division_textbox, ModelParameters.StemDivisionProbParam,
                                 (ExternalState.O2Limits))

        let sym_div_label  = new Label()
        sym_div_label.MaximumSize <- Drawing.Size(ParamFormBase.max_label_width, ParamFormBase.max_label_height)
        sym_div_label.AutoSize <- true
        sym_div_label.Text <- "The probability of symmetric division (%)"
        ParamFormBase.place_control_below(sym_div_label, control)

        let sym_div_textbox = new TextBox()
        sym_div_textbox.Text <- (sprintf "%.1f" (ModelParameters.SymRenewProb* float 100) )
        ParamFormBase.place_control_totheright(sym_div_textbox, sym_div_label)

        division_groupbox.Controls.AddRange([|sym_div_label; sym_div_textbox|])


        (*-------------- NON-STEM CELLS--------------------------*)
        let nonstem_withmem_groupbox = new GroupBox()
        nonstem_withmem_groupbox.Text <- "Transitions to non-stem \"with memory\""
        nonstem_withmem_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2*ParamFormBase.x_interval, base.ClientSize.Height)
        nonstem_withmem_groupbox.Location <- Drawing.Point(division_groupbox.Location.X + division_groupbox.Size.Width + ParamFormBase.x_interval,
                                                            division_groupbox.Location.Y)
        nonstem_withmem_groupbox.ClientSize <- Drawing.Size(int (float nonstem_withmem_groupbox.Size.Width * 0.9),
                                                            int (float nonstem_withmem_groupbox.Size.Height * 0.9) )

        let tononstem_withmem_label = new Label()
        tononstem_withmem_label.MaximumSize <- Drawing.Size(2*ParamFormBase.max_label_width, ParamFormBase.max_label_height)
        tononstem_withmem_label.AutoSize <- true
        tononstem_withmem_label.Text <- "The probability of the transition of a stem cell into the \"non-stem with memory\" state (%)"
        tononstem_withmem_label.Location <- ParamFormBase.initial_location

        let tononstem_withmem_textbox = new TextBox()
        tononstem_withmem_textbox.Text <- (sprintf "%.1f" (ModelParameters.StemToNonStemProbParam * (float 100)))
        ParamFormBase.place_control_totheright(tononstem_withmem_textbox, tononstem_withmem_label)

        nonstem_tostem_prob_chart.Titles.Add("The probability of transition from non-stem (with memory) to stem state") |> ignore
        nonstem_tostem_prob_chart.ChartAreas.Add(nonstem_tostem_chart_area)
        nonstem_tostem_prob_chart.Series.Add(nonstem_tostem_prob_series)

        let (x3, _) = ModelParameters.NonStemToStemProbParam
        ParamFormBase.create_exp_func_controls(nonstem_withmem_groupbox, tononstem_withmem_label, nonstem_tostem_prob_chart,
                                                x3_textbox, n_textbox, max_nonstem_tostem_textbox,
                                                ModelParameters.NonStemToStemProbParam, (float 0, float x3))

        nonstem_withmem_groupbox.Controls.AddRange([|tononstem_withmem_label; tononstem_withmem_textbox |])
        base.Controls.AddRange([|division_groupbox; nonstem_withmem_groupbox|])