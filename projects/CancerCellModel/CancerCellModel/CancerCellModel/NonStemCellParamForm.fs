module NonStemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open ModelParameters
open Geometry

type NonStemCellParamForm() as this =
    inherit ParamFormBase(Width=1750, Height = 1000)
    
    let division_prob_o2_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_o2_chart_area = new ChartArea()
    let division_prob_o2_series = new Series(ChartType = SeriesChartType.Line)

    let division_prob_glucose_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_glucose_chart_area = new ChartArea()
    let division_prob_glucose_series = new Series(ChartType = SeriesChartType.Line)

    let division_prob_density_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_density_chart_area = new ChartArea()
    let division_prob_density_series = new Series(ChartType = SeriesChartType.Line)

    let min_division_interval_textbox = new TextBox()
    let max_division_interval_textbox = new TextBox()

    let necrosis_prob_o2_chart = new Chart(Dock=DockStyle.Fill)
    let necrosis_prob_o2_chart_area = new ChartArea()
    let necrosis_prob_o2_series = new Series(ChartType = SeriesChartType.Line)

    let necrosis_prob_glucose_chart = new Chart(Dock=DockStyle.Fill)
    let necrosis_prob_glucose_chart_area = new ChartArea()
    let necrosis_prob_glucose_series = new Series(ChartType = SeriesChartType.Line)

    let min_necrosis_wait_textbox = new TextBox()
    let max_necrosis_wait_textbox = new TextBox()

    let min_necrosis_desintegrate_textbox = new TextBox()
    let max_necrosis_desintegrate_textbox = new TextBox()

    let apoptosis_prob_age_chart = new Chart(Dock=DockStyle.Fill)
    let apoptosis_prob_age_chart_area = new ChartArea()
    let apoptosis_prob_age_series = new Series(ChartType = SeriesChartType.Line)

    let apply_changes(args: EventArgs) =
        ModelParameters.NonStemCellCycleInterval <-
            FormDesigner.retrieve_int_interval(min_division_interval_textbox, max_division_interval_textbox)
        
        ModelParameters.NecrosisWaitInterval <-
            FormDesigner.retrieve_int_interval(min_necrosis_wait_textbox, max_necrosis_wait_textbox)

        ModelParameters.NecrosisDesintegrationInterval <-
            FormDesigner.retrieve_int_interval(min_necrosis_desintegrate_textbox, max_necrosis_desintegrate_textbox)
    
    // initialise the window
    do
        (*-------------- DIVISION --------------------------*)
        base.Text <- "Non-stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width - FormDesigner.x_interval, base.Size.Height - FormDesigner.y_interval )

        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(int(float(base.ClientSize.Width)*0.5 - float(2*FormDesigner.x_interval)), base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        division_prob_o2_chart.Titles.Add("Probability of non-stem cell division") |> ignore
        division_prob_o2_chart.ChartAreas.Add(division_prob_o2_chart_area)
        division_prob_o2_chart.Series.Add(division_prob_o2_series)
        division_prob_o2_chart_area.AxisX.Title <- "Concentration of oxygen (%)"
        division_prob_o2_chart_area.AxisY.Title <- "Probability"

        division_prob_glucose_chart.Titles.Add("Probability of non-stem cell division") |> ignore
        division_prob_glucose_chart.ChartAreas.Add(division_prob_glucose_chart_area)
        division_prob_glucose_chart.Series.Add(division_prob_glucose_series)
        division_prob_glucose_chart_area.AxisX.Title <- "Concentration of glucose (%)"
        division_prob_glucose_chart_area.AxisY.Title <- "Probability"

        division_prob_density_chart.Titles.Add("The probability of non-stem cell division") |> ignore
        division_prob_density_chart.ChartAreas.Add(division_prob_density_chart_area)
        division_prob_density_chart.Series.Add(division_prob_density_series)
        division_prob_density_chart_area.AxisX.Title <- "Cell density"
        division_prob_density_chart_area.AxisY.Title <- "Probability"

        let control1 = ParamFormBase.create_logistic_func_controls(
                        division_groupbox, null, null, division_prob_o2_chart,
                        ModelParameters.NonStemDivisionProbabilityO2, ModelParameters.O2Limits,
                        FloatInterval(0., 1.), 100.)

        let control2 = ParamFormBase.create_logistic_func_controls(
                        division_groupbox, control1, "totheright", division_prob_glucose_chart,
                        ModelParameters.NonStemDivisionProbabilityGlucose, ModelParameters.GlucoseLimits,
                        FloatInterval(0., 1.), 100.)

        let control3 = ParamFormBase.create_logistic_func_controls(
                        division_groupbox, control1, "below", division_prob_density_chart,
                        ModelParameters.NonStemDivisionProbabilityDensity,
                        ModelParameters.CellPackingDensityLimits, FloatInterval(0., 1.), 100.)

        let control4 = ParamFormBase.create_int_interval_controls(
                        division_groupbox, control2, "Time (in steps) before two consecutive divisions",
                        min_division_interval_textbox, max_division_interval_textbox,
                        ModelParameters.NonStemCellCycleInterval, IntInterval(0, Int32.MaxValue))
        

        (*-------------- DEATH --------------------------*)

        let death_groupbox = new GroupBox()
        death_groupbox.Text <- "Cell Death"
        death_groupbox.Size <- Drawing.Size(int(float(base.ClientSize.Width)*0.5 - float(2*FormDesigner.x_interval)), base.ClientSize.Height)
        FormDesigner.place_control_totheright(death_groupbox, division_groupbox)
        death_groupbox.ClientSize <- Drawing.Size(int (float death_groupbox.Size.Width * 0.9),
                                                        int (float death_groupbox.Size.Height*0.9))

        necrosis_prob_o2_chart.Titles.Add("Probability of necrosis for a non-stem cell") |> ignore
        necrosis_prob_o2_chart.ChartAreas.Add(necrosis_prob_o2_chart_area)
        necrosis_prob_o2_chart.Series.Add(necrosis_prob_o2_series)
        necrosis_prob_o2_chart_area.AxisX.Title <- "Concentration of oxygen (%)"
        necrosis_prob_o2_chart_area.AxisY.Title <- "Probability"

        necrosis_prob_glucose_chart.Titles.Add("Probability of necrosis for a non-stem cell") |> ignore
        necrosis_prob_glucose_chart.ChartAreas.Add(necrosis_prob_glucose_chart_area)
        necrosis_prob_glucose_chart.Series.Add(necrosis_prob_glucose_series)
        necrosis_prob_glucose_chart_area.AxisX.Title <- "Concentration of glucose (%)"
        necrosis_prob_glucose_chart_area.AxisY.Title <- "Probability"

        apoptosis_prob_age_chart.Titles.Add("Probability of apoptosis for a non-stem cell") |> ignore
        apoptosis_prob_age_chart.ChartAreas.Add(apoptosis_prob_age_chart_area)
        apoptosis_prob_age_chart.Series.Add(apoptosis_prob_age_series)
        apoptosis_prob_age_chart_area.AxisX.Title <- "Cell age (time steps)"
        apoptosis_prob_age_chart_area.AxisY.Title <- "Probability"

        let control1 = ParamFormBase.create_logistic_func_controls(
                        death_groupbox, null, null, necrosis_prob_o2_chart,
                        ModelParameters.NonStemNecrosisProbabilityO2,
                        ModelParameters.O2Limits, FloatInterval(0., 1.), 100.)

        let control2 = ParamFormBase.create_logistic_func_controls(
                        death_groupbox, control1, "totheright", necrosis_prob_glucose_chart,
                        ModelParameters.NonStemNecrosisProbabilityGlucose,
                        ModelParameters.GlucoseLimits, FloatInterval(0., 1.), 100.)

        let control3 = ParamFormBase.create_logistic_func_controls(
                        death_groupbox, control1, "below", apoptosis_prob_age_chart,
                        ModelParameters.NonStemApoptosisProbabilityAge,
                        FloatInterval(0., (!ModelParameters.NonStemApoptosisProbabilityAge).Max.x*1.1),
                        FloatInterval(0., 1.), 100.)

        let control4 = ParamFormBase.create_int_interval_controls(
                        death_groupbox, control2,
                        "Waiting time (in steps) before a cell can undergo necrosis",
                        min_necrosis_wait_textbox, max_necrosis_wait_textbox,
                        ModelParameters.NecrosisWaitInterval, IntInterval(0, Int32.MaxValue))

        let control5 = ParamFormBase.create_int_interval_controls(
                        death_groupbox, control4,
                        "Time (in steps) before a necrotic cell desintegrates",
                        min_necrosis_desintegrate_textbox, max_necrosis_desintegrate_textbox,
                        ModelParameters.NecrosisDesintegrationInterval, IntInterval(0, Int32.MaxValue))

        ParamFormBase.create_ok_cancel_buttons(this, death_groupbox, apply_changes) |> ignore
        base.Controls.AddRange([|division_groupbox; death_groupbox|])