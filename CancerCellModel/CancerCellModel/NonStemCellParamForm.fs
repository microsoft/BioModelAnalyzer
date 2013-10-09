﻿module NonStemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters

type NonStemCellParamForm() as this =
    inherit ParamFormBase ()
    
    let division_prob_chart = new Chart(Dock=DockStyle.Fill)
    let division_chart_area = new ChartArea()
    let division_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_division_textbox = new TextBox()
    let x2_division_textbox = new TextBox()
    let mu_division_textbox = new TextBox()
    let s_division_textbox = new TextBox()
    let min_division_prob_textbox = new TextBox()
    let max_division_prob_textbox = new TextBox()
    let min_division_interval_textbox = new TextBox()
    let max_division_interval_textbox = new TextBox()


    let death_prob_chart = new Chart(Dock=DockStyle.Fill)
    let death_chart_area = new ChartArea()
    let death_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_death_textbox = new TextBox()
    let x2_death_textbox = new TextBox()
    let mu_death_textbox = new TextBox()
    let s_death_textbox = new TextBox()
    let min_death_prob_textbox = new TextBox()
    let max_death_prob_textbox = new TextBox()
    let min_death_wait_textbox = new TextBox()
    let max_death_wait_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.NonStemDivisionProbParam <- ParamFormBase.retrieve_logistic_func_param(x1_division_textbox,
                        x2_division_textbox, min_division_prob_textbox, max_division_prob_textbox)

        ModelParameters.DeathProbParam <- ParamFormBase.retrieve_logistic_func_param(
                         x1_death_textbox, x2_death_textbox,
                         min_death_prob_textbox, max_death_prob_textbox)
    
    // initialise the window
    do
        (*-------------- DIVISION --------------------------*)
        base.Text <- "Non-stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width - ParamFormBase.x_interval, base.Size.Height - ParamFormBase.x_interval )

        let (x1, x2, min, max) = ModelParameters.StemDivisionProbParam
        let (mu, s, _, _) = ModelParameters.logistic_func_param(ModelParameters.StemDivisionProbParam)
        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* ParamFormBase.x_interval, base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(ParamFormBase.x_interval, ParamFormBase.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        division_prob_chart.Titles.Add("The probability of non-stem cell division") |> ignore
        division_prob_chart.ChartAreas.Add(division_chart_area)
        division_prob_chart.Series.Add(division_prob_series)
        division_chart_area.AxisX.Title <- "The concentration of oxygen (%)"
        division_chart_area.AxisY.Title <- "The probability"

        let control = ParamFormBase.create_logistic_func_controls(
                       division_groupbox, null, division_prob_chart,
                       x1_division_textbox, x2_division_textbox, mu_division_textbox, s_division_textbox,
                       min_division_prob_textbox, max_division_prob_textbox,
                       ModelParameters.NonStemDivisionProbParam, (ExternalState.O2Limits))

        let (div_interval_min, div_interval_max) = ModelParameters.NonStemIntervalBetweenDivisions
        let min_division_interval_label = new Label()
        min_division_interval_label.Text <- "The minimum time (in steps) before two consecutive divisions"
        min_division_interval_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (float 2, 3.5))
        min_division_interval_label.AutoSize <- true
        ParamFormBase.add_textbox_int_validation(min_division_interval_textbox, min_division_interval_label.Text, (0, Int32.MaxValue))
        ParamFormBase.place_control_below(min_division_interval_label, control)

        min_division_interval_textbox.Text <- (sprintf "%d" div_interval_min)
        ParamFormBase.place_control_totheright(min_division_interval_textbox, min_division_interval_label)

        let max_division_interval_label = new Label()
        max_division_interval_label.Text <- "The maximum time (in steps) before two consecutive divisions"
        max_division_interval_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (float 2, 3.5))
        max_division_interval_label.AutoSize <- true
        ParamFormBase.place_control_totheright(max_division_interval_label, min_division_interval_textbox)

        max_division_interval_textbox.Text <- (sprintf "%d" div_interval_max)
        ParamFormBase.add_textbox_int_validation(max_division_interval_textbox, max_division_interval_label.Text, (0, Int32.MaxValue))
        ParamFormBase.place_control_totheright(max_division_interval_textbox, max_division_interval_label)

        division_groupbox.Controls.AddRange([|min_division_interval_label; min_division_interval_textbox;
                                                max_division_interval_label; max_division_interval_textbox|])

        (*-------------- DEATH --------------------------*)

        let death_groupbox = new GroupBox()
        death_groupbox.Text <- "Division"
        death_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* ParamFormBase.x_interval, base.ClientSize.Height)
        ParamFormBase.place_control_totheright(death_groupbox, division_groupbox)
        death_groupbox.ClientSize <- Drawing.Size(int (float death_groupbox.Size.Width * 0.9),
                                                        int (float death_groupbox.Size.Height*0.9))

        death_prob_chart.Titles.Add("The probability of non-stem cell death") |> ignore
        death_prob_chart.ChartAreas.Add(death_chart_area)
        death_prob_chart.Series.Add(death_prob_series)
        death_chart_area.AxisX.Title <- "The concentration of oxygen (%)"
        death_chart_area.AxisY.Title <- "The probability"

        let control = ParamFormBase.create_logistic_func_controls(
                        death_groupbox, null, death_prob_chart,
                        x1_death_textbox, x2_death_textbox, mu_death_textbox, s_death_textbox,
                        min_death_prob_textbox, max_death_prob_textbox,
                        ModelParameters.DeathProbParam, (ExternalState.O2Limits))

        let (death_interval_min, death_interval_max) = ModelParameters.DeathWaitInterval
        let min_death_wait_label = new Label()
        min_death_wait_label.Text <- "The minimum time (in steps) before two death program can be activated"
        min_death_wait_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (float 2, 3.5))
        min_death_wait_label.AutoSize <- true
        ParamFormBase.place_control_below(min_death_wait_label, control)

        min_death_wait_textbox.Text <- (sprintf "%d" death_interval_min)
        ParamFormBase.place_control_totheright(min_death_wait_textbox, min_death_wait_label)

        let max_death_wait_label = new Label()
        max_death_wait_label.Text <- "The maximum time (in steps) before two death program can be activated"
        max_death_wait_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (float 2, 3.5))
        max_death_wait_label.AutoSize <- true
        ParamFormBase.place_control_totheright(max_death_wait_label, min_death_wait_textbox)

        max_death_wait_textbox.Text <- (sprintf "%d" death_interval_max)
        ParamFormBase.place_control_totheright(max_death_wait_textbox, max_death_wait_label)

        death_groupbox.Controls.AddRange([| min_death_wait_label; min_death_wait_textbox;
            max_death_wait_label; max_death_wait_textbox |])

        ParamFormBase.create_ok_cancel_buttons(this, death_groupbox, apply_changes) |> ignore
        base.Controls.AddRange([|division_groupbox; death_groupbox|])