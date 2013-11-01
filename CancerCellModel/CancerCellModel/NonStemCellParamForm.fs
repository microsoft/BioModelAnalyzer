module NonStemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters
open MyMath
open Geometry

type NonStemCellParamForm() as this =
    inherit ParamFormBase (Width=1200, Height = 1000)
    
    let division_prob_o2_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_o2_chart_area = new ChartArea()
    let division_prob_o2_series = new Series(ChartType = SeriesChartType.Line)

    let division_prob_density_chart = new Chart(Dock=DockStyle.Fill)
    let division_prob_density_chart_area = new ChartArea()
    let division_prob_density_series = new Series(ChartType = SeriesChartType.Line)

    let x1_division_textbox = new TextBox()
    let x2_division_textbox = new TextBox()
    let mu_division_textbox = new TextBox()
    let s_division_textbox = new TextBox()
    let min_division_prob_textbox = new TextBox()
    let max_division_prob_textbox = new TextBox()
    let min_division_interval_textbox = new TextBox()
    let max_division_interval_textbox = new TextBox()


    let death_prob_o2_chart = new Chart(Dock=DockStyle.Fill)
    let death_prob_o2_chart_area = new ChartArea()
    let death_prob_o2_series = new Series(ChartType = SeriesChartType.Line)

    let death_prob_density_chart = new Chart(Dock=DockStyle.Fill)
    let death_prob_density_chart_area = new ChartArea()
    let death_prob_density_series = new Series(ChartType = SeriesChartType.Line)

    let x1_death_textbox = new TextBox()
    let x2_death_textbox = new TextBox()
    let mu_death_textbox = new TextBox()
    let s_death_textbox = new TextBox()
    let min_death_prob_textbox = new TextBox()
    let max_death_prob_textbox = new TextBox()
    let min_death_wait_textbox = new TextBox()
    let max_death_wait_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.NonStemDivisionProbParam := ParamFormBase.retrieve_logistic_func_param(x1_division_textbox,
                        x2_division_textbox, min_division_prob_textbox, max_division_prob_textbox)

        ModelParameters.DeathProbOnO2Param := ParamFormBase.retrieve_logistic_func_param(
                         x1_death_textbox, x2_death_textbox,
                         min_death_prob_textbox, max_death_prob_textbox)
    
    // initialise the window
    do
        (*-------------- DIVISION --------------------------*)
        base.Text <- "Non-stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width - FormDesigner.x_interval, base.Size.Height - FormDesigner.x_interval )

        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* FormDesigner.x_interval, base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        division_prob_o2_chart.Titles.Add("The probability of non-stem cell division") |> ignore
        division_prob_o2_chart.ChartAreas.Add(division_prob_o2_chart_area)
        division_prob_o2_chart.Series.Add(division_prob_o2_series)
        division_prob_o2_chart_area.AxisX.Title <- "The concentration of oxygen (%)"
        division_prob_o2_chart_area.AxisY.Title <- "Probability"

        let control1 = ParamFormBase.create_logistic_func_controls(
                        division_groupbox, null, division_prob_o2_chart,
                        ModelParameters.NonStemDivisionProbParam, (ExternalState.O2Limits))

        let control2 = ParamFormBase.create_int_interval_controls(
                        division_groupbox, control1, "Time (in steps) before two consecutive divisions",
                        min_division_interval_textbox, max_division_interval_textbox,
                        ModelParameters.NonStemIntervalBetweenDivisions)

        division_prob_density_chart.Titles.Add("The probability of non-stem cell division") |> ignore
        division_prob_density_chart.ChartAreas.Add(division_prob_density_chart_area)
        division_prob_density_chart.Series.Add(division_prob_density_series)
        division_prob_density_chart_area.AxisX.Title <- "Cell density"
        division_prob_density_chart_area.AxisY.Title <- "Probability"

        ParamFormBase.create_logistic_func_controls(
                        division_groupbox, control2, division_prob_density_chart,
                        ModelParameters.DivisionProbOnCellDensity,
                        ExternalState.CellPackDensityLimits) |> ignore

        (*-------------- DEATH --------------------------*)

        let death_groupbox = new GroupBox()
        death_groupbox.Text <- "Division"
        death_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* FormDesigner.x_interval, base.ClientSize.Height)
        FormDesigner.place_control_totheright(death_groupbox, division_groupbox)
        death_groupbox.ClientSize <- Drawing.Size(int (float death_groupbox.Size.Width * 0.9),
                                                        int (float death_groupbox.Size.Height*0.9))

        death_prob_o2_chart.Titles.Add("The probability of non-stem cell death") |> ignore
        death_prob_o2_chart.ChartAreas.Add(death_prob_o2_chart_area)
        death_prob_o2_chart.Series.Add(death_prob_o2_series)
        death_prob_o2_chart_area.AxisX.Title <- "The concentration of oxygen (%)"
        death_prob_o2_chart_area.AxisY.Title <- "Probability"

        let control1 = ParamFormBase.create_logistic_func_controls(
                        death_groupbox, null, death_prob_o2_chart,
                        ModelParameters.DeathProbOnO2Param, (ExternalState.O2Limits))

        let control2 = ParamFormBase.create_int_interval_controls(
                        death_groupbox, control1,
                        "Waiting time (in steps) before the death program can be activated",
                        min_death_wait_textbox, max_death_wait_textbox,
                        ModelParameters.DeathWaitInterval)

        death_prob_density_chart.Titles.Add("The probability of non-stem cell death") |> ignore
        death_prob_density_chart.ChartAreas.Add(death_prob_density_chart_area)
        death_prob_density_chart.Series.Add(death_prob_density_series)
        death_prob_density_chart_area.AxisX.Title <- "Cell density"
        death_prob_density_chart_area.AxisY.Title <- "Probability"

        ParamFormBase.create_logistic_func_controls(
              death_groupbox, control2, death_prob_density_chart,
              ModelParameters.DeathProbDependOnCellDensity,
              (ExternalState.CellPackDensityLimits)) |> ignore

        ParamFormBase.create_ok_cancel_buttons(this, death_groupbox, apply_changes) |> ignore
        base.Controls.AddRange([|division_groupbox; death_groupbox|])