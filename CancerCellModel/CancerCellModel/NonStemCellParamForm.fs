module NonStemCellParamForm

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open ParamFormBase
open Cell
open ModelParameters

type NonStemCellParamForm() =
    inherit ParamFormBase ()
    
    let division_prob_chart = new Chart(Dock=DockStyle.Fill)
    let division_chart_area = new ChartArea()
    let division_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_division_textbox = new TextBox()
    let x2_division_textbox = new TextBox()
    let mu_division_textbox = new TextBox()
    let s_division_textbox = new TextBox()
    let max_division_textbox = new TextBox()

    let death_prob_chart = new Chart(Dock=DockStyle.Fill)
    let death_chart_area = new ChartArea()
    let death_prob_series = new Series(ChartType = SeriesChartType.Line)
    let x1_death_textbox = new TextBox()
    let x2_death_textbox = new TextBox()
    let mu_death_textbox = new TextBox()
    let s_death_textbox = new TextBox()
    let max_death_textbox = new TextBox()
    
    // initialise the window
    do
        (*-------------- DIVISION --------------------------*)
        base.Text <- "Non-stem cells"
        base.ClientSize <- Drawing.Size(base.Size.Width - ParamFormBase.x_interval, base.Size.Height - ParamFormBase.x_interval )

        let (x1, x2, max) = ModelParameters.StemDivisionProbParam
        let (mu, s, _) = ModelParameters.logistic_func_param(ModelParameters.StemDivisionProbParam)
        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* ParamFormBase.x_interval, base.ClientSize.Height)
        division_groupbox.Location <- Drawing.Point(ParamFormBase.x_interval, ParamFormBase.y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))

        division_prob_chart.Titles.Add("The probability of cell division") |> ignore
        division_prob_chart.ChartAreas.Add(division_chart_area)
        division_prob_chart.Series.Add(division_prob_series)

        ParamFormBase.create_logistic_func_controls(
                       division_groupbox, null, division_prob_chart,
                       x1_division_textbox, x2_division_textbox, mu_division_textbox, s_division_textbox, max_division_textbox,
                       ModelParameters.StemDivisionProbParam, (ExternalState.O2Limits)) |> ignore

  
        (*-------------- DEATH --------------------------*)

        let death_groupbox = new GroupBox()
        death_groupbox.Text <- "Division"
        death_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 2* ParamFormBase.x_interval, base.ClientSize.Height)
        ParamFormBase.place_control_totheright(death_groupbox, division_groupbox)
        death_groupbox.ClientSize <- Drawing.Size(int (float death_groupbox.Size.Width * 0.9),
                                                        int (float death_groupbox.Size.Height*0.9))

        death_prob_chart.Titles.Add("The probability of cell death") |> ignore
        death_prob_chart.ChartAreas.Add(death_chart_area)
        death_prob_chart.Series.Add(death_prob_series)

        ParamFormBase.create_logistic_func_controls(
                        death_groupbox, null, death_prob_chart,
                        x1_death_textbox, x2_death_textbox, mu_death_textbox, s_death_textbox,
                        max_death_textbox, ModelParameters.DeathProbParam, (ExternalState.O2Limits)) |> ignore