module Visualisation

open System
//open System.Windows.Controls
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open Cell

type StatType = Live | Dead | Stem | NonStem

type LineChartForm(title) =
    inherit Form (Visible = false, Width = 1400, Height = 1000)

    let chart = new Chart(Dock=DockStyle.Fill)

    do
        chart.ChartAreas.Add (new ChartArea())
        base.Controls.Add (chart)
        chart.Legends.Add(new Legend())

    member this.add_points(xs:seq<float*float>, series: Series) =
        for t, n in xs do
            series.Points.AddXY(t, n) |> ignore

    member this.Chart with get() = chart

type StemCellParamForm() =
    inherit Form (Visible = false, Width = 1200, Height = 800)
    
    let division_prob_series = new Series(ChartType = SeriesChartType.Line,
                                          name ="Probability of division of stem cells")

    let division_prob_chart = new Chart(Dock=DockStyle.Fill)
    let chart_area = new ChartArea()
    let x1_textbox = new TextBox()
    let x2_textbox = new TextBox()
    let mu_textbox = new TextBox()
    let s_textbox = new TextBox()
    let max_textbox = new TextBox()

    let func_plot(series: Series, func: float -> float, xmin: float, xmax: float) =
        let x = ref xmin
        while !x <= xmax do
            series.Points.AddXY(!x, func(!x)) |> ignore
            x := !x + float 1
    
    let refresh() =
        let (x1_default, x2_default, max_default) = ModelParameters.StemDivisionProbParam
        let (x1, x2, max) = (ref x1_default, ref x2_default, ref max_default)

        if not (Double.TryParse(x1_textbox.Text, x1)) ||
           not (Double.TryParse(x2_textbox.Text, x2)) ||
           not (Double.TryParse(max_textbox.Text, max))
        then
            x1 := x1_default
            x2 := x2_default
            max := max_default

        let (mu, s, _) = ModelParameters.logistic_func_param(!x1, !x2, !max)
        
        division_prob_series.Points.Clear()
        func_plot(division_prob_series, (ModelParameters.logistic_func(mu, s, !max)),
            ExternalState.MinO2, ExternalState.MaxO2)
        
        chart_area.AxisY.Maximum <- 1.1 * !max
        chart_area.AxisY.Minimum <- float 0
        chart_area.AxisY.Interval <- 0.1 * !max
        division_prob_chart.Refresh()

        mu_textbox.Text <- (sprintf "%.3f" mu)
        s_textbox.Text <- (sprintf "%.3f" s)
                
    do
        base.Text <- "Stem cells"

        let (x1, x2, max) = ModelParameters.StemDivisionProbParam
        let (mu, s, max) = ModelParameters.logistic_func_param(ModelParameters.StemDivisionProbParam)
        let x_interval = 10
        let y_interval = 20
        let label_width = 20
        let textbox_width = 50

        //let division_prob_chart = new Chart(Dock=DockStyle.Fill)
        //let chart_area = new ChartArea()
        chart_area.AxisX.Maximum <- ExternalState.MaxO2 + float 1
        chart_area.AxisX.Minimum <- ExternalState.MinO2
        chart_area.AxisX.Interval <- float 10
        chart_area.AxisX.IsLabelAutoFit <- true
        refresh()        

        division_prob_chart.Titles.Add("Probability of cell division") |> ignore
        division_prob_chart.Titles.Add("1 / (1/m + exp((mu - x)/s))") |> ignore
        division_prob_chart.ChartAreas.Add(chart_area)
        division_prob_chart.Series.Add(division_prob_series)
        //division_prob_chart.Location <- Drawing.Point(10, 200)
        division_prob_chart.Size <- Drawing.Size(100, 100)
        
        let division_groupbox = new GroupBox()
        division_groupbox.Text <- "Division"
        division_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 3* x_interval, base.ClientSize.Height - 2*y_interval)
        division_groupbox.Location <- Drawing.Point(x_interval, y_interval)
        division_groupbox.ClientSize <- Drawing.Size(int (float division_groupbox.Size.Width * 0.9),
                                                        int (float division_groupbox.Size.Height*0.9))
        base.Controls.Add(division_groupbox)

        let panel = new Panel()
        panel.ClientSize <- Drawing.Size(int (float division_groupbox.ClientSize.Width * 0.8), (division_groupbox.ClientSize.Height-150))
        panel.Location <- Drawing.Point(30, 30)
        panel.Controls.Add(division_prob_chart)

        let x1_label = new Label()
        //x1_label.Width <- 400
        x1_label.Text <- "X coordinate of inflection point 1"
        x1_label.Location <- Drawing.Point(panel.Location.X, panel.Location.Y + panel.Size.Height + y_interval)

        //let x1_textbox = new TextBox()
        x1_textbox.Width <- textbox_width
        x1_textbox.Text <- (sprintf "%.3f" x1)
        x1_textbox.Location <- Drawing.Point(x1_label.Location.X + x1_label.Size.Width + x_interval, x1_label.Location.Y)

        let x2_label = new Label()
        //x2_label.Width <- 400
        x2_label.Text <- "X coordinate of inflection point 2"
        x2_label.Location <- Drawing.Point(x1_textbox.Location.X + x1_textbox.Size.Width + x_interval, x1_label.Location.Y)

        //let x2_textbox = new TextBox()
        x2_textbox.Width <- textbox_width
        x2_textbox.Text <- (sprintf "%.3f" x2)
        x2_textbox.Location <- Drawing.Point(x2_label.Location.X + x2_label.Size.Width + x_interval, x1_label.Location.Y)

        let (mu, s, x) = ModelParameters.logistic_func_param(ModelParameters.StemDivisionProbParam)
        let mu_label = new Label()
        mu_label.Width <- label_width
        mu_label.Text <- "mu"
        mu_label.Location <- Drawing.Point(panel.Location.X, x2_label.Location.Y + x2_label.Size.Height + y_interval)

        //let mu_textbox = new TextBox()
        mu_textbox.Width <- textbox_width
        mu_textbox.Text <- (sprintf "%.3f" mu)
        mu_textbox.Location <- Drawing.Point(mu_label.Location.X + mu_label.Size.Width + x_interval, mu_label.Location.Y)
        mu_textbox.Enabled <- false

        let s_label = new Label()
        s_label.Width <- label_width
        s_label.Text <- "s"
        s_label.Location <- Drawing.Point(mu_textbox.Location.X + mu_textbox.Size.Width + x_interval, mu_label.Location.Y)

        //let s_textbox = new TextBox()
        s_textbox.Width <- textbox_width
        s_textbox.Text <- (sprintf "%.3f" s)
        s_textbox.Location <- Drawing.Point(s_label.Location.X + s_label.Size.Width + x_interval, mu_label.Location.Y)
        s_textbox.Enabled <- false

        let max_label = new Label()
        //max_label.Width <- 30
        max_label.Text <- "m (the maximum)"
        max_label.Location <- Drawing.Point(s_textbox.Location.X + s_textbox.Size.Width + x_interval, mu_label.Location.Y)

        //let max_textbox = new TextBox()
        max_textbox.Width <- textbox_width
        max_textbox.Text <- (sprintf "%.3f" max)
        max_textbox.Location <- Drawing.Point(max_label.Location.X + max_label.Size.Width + x_interval, mu_label.Location.Y)

        let refresh_button = new Button()
        refresh_button.Text <- "Refresh"
        refresh_button.Location <- Drawing.Point(max_textbox.Location.X + max_textbox.Size.Width + x_interval, mu_label.Location.Y)
        refresh_button.Click.Add(fun args -> refresh(); )

        division_groupbox.Controls.AddRange([|panel; x1_label; x1_textbox; x2_label; x2_textbox;
            mu_label; mu_textbox; s_label; s_textbox; max_label; max_textbox; refresh_button|])

        let nonstem_withmem_groupbox = new GroupBox()
        nonstem_withmem_groupbox.Text <- "Transitions to non-stem \"with memory\""
        nonstem_withmem_groupbox.Size <- Drawing.Size(base.ClientSize.Width/2 - 3*x_interval, int (float base.ClientSize.Height*0.9))
        nonstem_withmem_groupbox.Location <- Drawing.Point(division_groupbox.Location.X + division_groupbox.Size.Width + x_interval,
                                                            division_groupbox.Location.Y)
        nonstem_withmem_groupbox.ClientSize <- Drawing.Size(int (float nonstem_withmem_groupbox.Size.Width * 0.9),
                                                            int (float nonstem_withmem_groupbox.Size.Height*0.9))
        base.Controls.AddRange([|division_groupbox; nonstem_withmem_groupbox|])

         
type ExtStatisticsForm (title) =
    inherit LineChartForm (title)
    
    let series_o2 = new Series(ChartType = SeriesChartType.Line, name ="O2")

    do
        base.Chart.Series.Add (series_o2)
        series_o2.Color <- Drawing.Color.Red

    member this.AddPoints(xs: seq<float*float>(*, stat_type: StatType*)) =
    // Add data to the series in a loop
          base.add_points(xs, 
          (* (match stat_type with
            | _ -> *)series_o2)
            //)

    member this.Clear() =
        series_o2.Points.Clear()

type CellStatisticsForm (title) =
    inherit LineChartForm(title)

    let series_live = new Series(ChartType = SeriesChartType.Line, name="Live cells")
    let series_dead = new Series(ChartType = SeriesChartType.Line, name="Dead cells")
    let series_stem = new Series(ChartType = SeriesChartType.Line, name="Stem cells")
    let series_non_stem = new Series(ChartType = SeriesChartType.Line, name="Non-stem cells")

    let stem_cells_dialog = new StemCellParamForm()

    do
        // create the main chart (with statistics for different kinds of cells)
        base.Chart.Series.Add (series_live)
        base.Chart.Series.Add (series_dead)
        base.Chart.Series.Add (series_stem)
        base.Chart.Series.Add (series_non_stem)
        series_live.Color <- Drawing.Color.Red
        series_dead.Color <- Drawing.Color.Orange
        series_stem.Color <- Drawing.Color.DarkBlue
        series_non_stem.Color <- Drawing.Color.LightGreen

        // create the main menu
        base.Menu <- new MainMenu()
        let prob_func = new MenuItem("Probability functions")
        base.Menu.MenuItems.Add(prob_func) |> ignore
        let stem_cells = new MenuItem("Stem cells")
        prob_func.MenuItems.Add(stem_cells) |> ignore
        let non_stem_cells = new MenuItem("Non-stem cells")
        prob_func.MenuItems.Add(non_stem_cells) |> ignore

        stem_cells.Click.Add(fun args -> stem_cells_dialog.Visible <- true)

    member this.AddPoints(xs: seq<float*float>, stat_type: StatType) =
    // Add data to the series in a loop
          base.add_points(xs, 
           (match stat_type with
            | Live -> series_live
            | Dead -> series_dead
            | Stem -> series_stem
            | NonStem -> series_non_stem))

    member this.Clear() =
        series_live.Points.Clear()
        series_dead.Points.Clear()
        series_stem.Points.Clear()
        series_non_stem.Points.Clear()