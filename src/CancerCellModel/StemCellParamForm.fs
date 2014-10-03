module StemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open ModelParameters
open Geometry

type StemCellParamForm() as this =
    inherit ParamFormBase (Width=1400, Height = 1100)
    
    let division_prob_o2_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_o2_chart_area = new ChartArea()
    let division_prob_o2_series = new Series(ChartType = SeriesChartType.Line)

    let division_prob_glucose_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_glucose_chart_area = new ChartArea()
    let division_prob_glucose_series = new Series(ChartType = SeriesChartType.Line)

    let division_prob_density_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_density_chart_area = new ChartArea()
    let division_prob_density_series = new Series(ChartType = SeriesChartType.Line)

    let sym_div_textbox = new TextBox()
    let min_division_interval_textbox = new TextBox()
    let max_division_interval_textbox = new TextBox()

    let nonstem_tostem_prob_chart = new Chart(Dock=DockStyle.Fill)
    let nonstem_tostem_chart_area = new ChartArea()
    let nonstem_tostem_prob_series = new Series(ChartType = SeriesChartType.Line)

    let min_nonstem_tostem_textbox = new TextBox()
    let max_nonstem_tostem_textbox = new TextBox()
    let tononstem_withmem_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.StemSymmetricDivisionProbability <- FormDesigner.retrieve_float(sym_div_textbox) / 100.

        ModelParameters.StemCellCycleInterval <- FormDesigner.retrieve_int_interval(min_division_interval_textbox,
                                                                                        max_division_interval_textbox)

        ModelParameters.StemToNonStemProbability <- FormDesigner.retrieve_float(tononstem_withmem_textbox)

    // initialise the window
    do
        (*-------------- STEM CELLS--------------------------*)
        base.Text <- "Stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width, base.Size.Height)

        division_prob_o2_chart.Titles.Add("Probability of stem cell division") |> ignore
        division_prob_o2_chart.ChartAreas.Add(division_prob_o2_chart_area)
        division_prob_o2_chart.Series.Add(division_prob_o2_series)
        division_prob_o2_chart_area.AxisX.Title <- "Concentration of oxygen (%)"
        division_prob_o2_chart_area.AxisY.Title <- "Probability"

        division_prob_glucose_chart.Titles.Add("Probability of stem cell division") |> ignore
        division_prob_glucose_chart.ChartAreas.Add(division_prob_glucose_chart_area)
        division_prob_glucose_chart.Series.Add(division_prob_glucose_series)
        division_prob_glucose_chart_area.AxisX.Title <- "Concentration of glucose (%)"
        division_prob_glucose_chart_area.AxisY.Title <- "Probability"

        division_prob_density_chart.Titles.Add("Probability of stem cell division") |> ignore
        division_prob_density_chart.ChartAreas.Add(division_prob_density_chart_area)
        division_prob_density_chart.Series.Add(division_prob_density_series)
        division_prob_density_chart_area.AxisX.Title <- "Cell density"
        division_prob_density_chart_area.AxisY.Title <- "Probability"

        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(int(float(base.ClientSize.Width)*0.65 - float(2*FormDesigner.x_interval)), base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        let control1 = ParamFormBase.create_logistic_func_controls(
                                 division_groupbox, null, null, division_prob_o2_chart,
                                 ModelParameters.StemDivisionProbabilityO2,
                                 ModelParameters.O2Limits, FloatInterval(0., 1.), 100.)   
                                 
        let control2 = ParamFormBase.create_logistic_func_controls(
                                 division_groupbox, control1, "totheright", division_prob_glucose_chart,
                                 ModelParameters.StemDivisionProbabilityGlucose,
                                 ModelParameters.GlucoseLimits, FloatInterval(0., 1.), 100.)     

        let control3 = ParamFormBase.create_logistic_func_controls(
                                 division_groupbox, control1, "below", division_prob_density_chart,
                                 ModelParameters.StemDivisionProbabilityDensity,
                                 ModelParameters.CellPackingDensityLimits, FloatInterval(0., 1.), 100.)

        let sym_div_label  = new Label()
        sym_div_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2,3))
        sym_div_label.AutoSize <- true
        sym_div_label.Text <- "Probability of symmetric division (%)"
        FormDesigner.place_control_below(sym_div_label, control3, 3*FormDesigner.y_interval)

        sym_div_textbox.Text <- (sprintf "%.1f" (ModelParameters.StemSymmetricDivisionProbability*float 100))
        FormDesigner.add_textbox_float_validation(sym_div_textbox, sym_div_label.Text, FloatInterval(0., 100.))
        FormDesigner.place_control_totheright(sym_div_textbox, sym_div_label)

        ParamFormBase.create_int_interval_controls(
                    division_groupbox, sym_div_label,
                    "Time (in steps) before two consecutive divisions",
                    min_division_interval_textbox, max_division_interval_textbox,
                    ModelParameters.StemCellCycleInterval, IntInterval(0, Int32.MaxValue)) |> ignore

        division_groupbox.Controls.AddRange([| sym_div_label; sym_div_textbox |])


        (*-------------- NON-STEM WITH MEMORY CELLS--------------------------*)
        let nonstem_withmem_groupbox = new GroupBox()
        nonstem_withmem_groupbox.Text <- "Transitions to the \"non-stem with memory\" state"
        nonstem_withmem_groupbox.Size <- Drawing.Size(int(float(base.ClientSize.Width)*0.35 - float(2*FormDesigner.x_interval)), base.ClientSize.Height)
        nonstem_withmem_groupbox.Location <- Drawing.Point(division_groupbox.Location.X + division_groupbox.Size.Width + FormDesigner.x_interval,
                                                            division_groupbox.Location.Y)
        nonstem_withmem_groupbox.ClientSize <- Drawing.Size(int (float nonstem_withmem_groupbox.Size.Width * 0.9),
                                                            int (float nonstem_withmem_groupbox.Size.Height * 0.9) )

        let tononstem_withmem_label = new Label()
        tononstem_withmem_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        tononstem_withmem_label.AutoSize <- true
        tononstem_withmem_label.Text <- "Probability of transition to the \"non-stem with memory\" state (%)"
        tononstem_withmem_label.Location <- FormDesigner.initial_location

        tononstem_withmem_textbox.Text <- (sprintf "%.1f" (ModelParameters.StemToNonStemProbability * (float 100)))
        FormDesigner.add_textbox_float_validation(tononstem_withmem_textbox, tononstem_withmem_label.Text, FloatInterval(0., 100.))
        FormDesigner.place_control_totheright(tononstem_withmem_textbox, tononstem_withmem_label)

        nonstem_tostem_prob_chart.Titles.Add("Probability of returning from the \"non-stem with memory\" state to the stem state") |> ignore
        nonstem_tostem_prob_chart.ChartAreas.Add(nonstem_tostem_chart_area)
        nonstem_tostem_prob_chart.Series.Add(nonstem_tostem_prob_series)
        nonstem_tostem_chart_area.AxisX.Title <- "Number of stem cells"
        nonstem_tostem_chart_area.AxisY.Title <- "Probability"

        let x3 = (!ModelParameters.NonStemToStemProbability).P2.x
        let control = ParamFormBase.create_shiftexp_func_controls(nonstem_withmem_groupbox,
                                                tononstem_withmem_label, nonstem_tostem_prob_chart,
                                                ModelParameters.NonStemToStemProbability,
                                                FloatInterval(0., x3), FloatInterval(0., 1.), 100., true)
        
        ParamFormBase.create_ok_cancel_buttons(this, nonstem_withmem_groupbox, apply_changes) |> ignore
        nonstem_withmem_groupbox.Controls.AddRange([|tononstem_withmem_label; tononstem_withmem_textbox|])

        base.Controls.AddRange([| division_groupbox; nonstem_withmem_groupbox |])
