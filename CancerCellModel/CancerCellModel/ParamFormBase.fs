module ParamFormBase

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open System.ComponentModel
open Cell
open ModelParameters

type ParamFormBase(?Width, ?Height) =
    inherit Form (Visible = false, Width = defaultArg Width 1200, Height = defaultArg Height 850)

    static member x_interval with get() = 10
    static member y_interval with get() = 15
    static member initial_location with get() = Drawing.Point(30, 30)
    static member label_size with get() = Drawing.Size(50, 15)
    static member button_size with get() = Drawing.Size(60, 20)
    static member textbox_size with get() = Drawing.Size(50, 10)
    //static member max_label_size with get() = Drawing.Size(100, 40)
    //static member double_label_size with get() = Drawing.Size(2*ParamFormBase.max_label_size.Width, ParamFormBase.max_label_size.Height)
    //static member double_button_size with get() = Drawing.Size(120,50)
    //static member textbox_width with get() = 50
    static member plot_size with get() = Drawing.Size(450, 480)

    static member Scale(size: Drawing.Size, (sx: float, sy: float)) =
        let width = size.Width
        let height = size.Height
        Drawing.Size(int(float width * sx), int(float height * sy))

    static member Scale(size: Drawing.Size, (sx: int, sy: int)) =
        ParamFormBase.Scale(size, (float sx, float sy))

    static member place_control_totheright(c1: Control, c2: Control) =
        c1.Location <- Drawing.Point(c2.Location.X + (if c2.AutoSize && c2.MaximumSize.Width>0 then c2.MaximumSize.Width else c2.Width) +
                                    ParamFormBase.x_interval, c2.Location.Y)

    static member place_control_below(c1: Control, c2: Control, ?extra_space: int) =
        let dy = defaultArg extra_space 0
        c1.Location <- Drawing.Point(c2.Location.X, c2.Location.Y +
                                     (if c2.AutoSize && c2.MaximumSize.Height > 0 then c2.MaximumSize.Height else c2.Height) +
                                     ParamFormBase.y_interval + dy)

    static member func_plot(series: Series, func: float -> float, x_limits: float*float) =
        let (xmin, xmax) = x_limits
        let x = ref xmin
        while !x <= xmax do
            series.Points.AddXY(!x, func(!x)) |> ignore
            x := !x + float 1

    static member get_chart_yvalue(series: Series, x: float) =
        let point = series.Points.FindByValue(float (round(x)), "X")
        point.YValues.[0]

    static member textbox_float_interval_check(textbox: TextBox, name: string, range: float*float)
                                           (args: CancelEventArgs) =
        
        let x = ref (float 0)
        let (xmin, xmax) = range
        let err = ref false
        let msg = ref ""

        if not (Double.TryParse(textbox.Text, x)) then
            err := true
            msg := (sprintf "%s must be a real number" name)
        else if !x < xmin || !x > xmax then
            err := true
            if xmax = Double.MaxValue then
                msg := (sprintf "%s can take on a real value greater or equal to >= %.1f" name xmin)
            else
                msg := (sprintf "%s can take on a real value from %.1f to %.1f" name xmin xmax)
        
        if !err then
            args.Cancel <- true
            MessageBox.Show(!msg, "Data validation error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            textbox.Select(0, textbox.Text.Length)

    static member textbox_int_interval_check(textbox: TextBox, name: string, range: int*int)
                                           (args: CancelEventArgs) =
        
        let x = ref (int 0)
        let (xmin, xmax) = range
        let err = ref false
        let msg = ref ""

        if not (Int32.TryParse(textbox.Text, x)) then
            err := true
            msg := (sprintf "%s must be an integer number" name)
        else if !x < xmin || !x > xmax then
            err := true
            if xmax = Int32.MaxValue then
                msg := (sprintf "%s can take on an integer value greater or equal to >= %d" name xmin)
            else
                msg := (sprintf "%s can take on an integer value from %d to %d" name xmin xmax)
        
        if !err then
            args.Cancel <- true
            MessageBox.Show(!msg, "Data validation error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            textbox.Select(0, textbox.Text.Length)

    static member textbox_float_func_check(textbox1: TextBox, textbox2: TextBox, name1: string, name2: string,
                                            func: float*float->bool, err_msg: string)(args: CancelEventArgs) =
        let (x1, x2) = (ref (float 0), ref (float 0))
        let err = ref false
        let msg = ref ""

        if not (Double.TryParse(textbox1.Text, x1)) then
            err := true
            msg := (sprintf "%s must be a real number" name1)
        else if not (Double.TryParse(textbox2.Text, x2)) then
            err := true
            msg := (sprintf "%s must be a real number" name2)
        else if not (func(!x1, !x2)) then
            err := true
            msg := err_msg
        
        if !err then
            args.Cancel <- true
            MessageBox.Show(!msg, "Data validation error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            textbox1.Select(0, textbox1.Text.Length)

    static member add_textbox_float_validation(textbox: TextBox, name: string, range: float*float) =
        textbox.Validating.Add(fun args -> ParamFormBase.textbox_float_interval_check(textbox, name, range)(args))

    static member add_textbox_int_validation(textbox: TextBox, name: string, range: int*int) =
        textbox.Validating.Add(fun args -> ParamFormBase.textbox_int_interval_check(textbox, name, range)(args))

    static member add_textbox_float_less_check(textbox1: TextBox, textbox2: TextBox, name1: string, name2: string) =
        textbox1.Validating.Add(fun args -> ParamFormBase.textbox_float_func_check(textbox1, textbox2, name1, name2,
                                                                                    (fun (x1: float, x2: float) -> x1 < x2),
                                                                                    (sprintf "%s should be less than %s" name1 name2))(args))

    static member retrieve_logistic_func_param(x1_textbox: TextBox, x2_textbox: TextBox,
                                                min_textbox: TextBox, max_textbox: TextBox) =

        let (x1, x2, min, max) = (ref (float 0), ref (float 0), ref (float 0), ref (float 0))

        if not (Double.TryParse(x1_textbox.Text, x1)) ||
           not (Double.TryParse(x2_textbox.Text, x2)) ||
           not (Double.TryParse(min_textbox.Text, min)) ||
           not (Double.TryParse(max_textbox.Text, max))
        then
           raise (InnerError("Unable to parse logistic function parameters"))
        
        // convert percents to a fraction
        min := !min / (float 100)
        max := !max / (float 100)
        (!x1, !x2, !min, !max)

    static member retrieve_exp_func_param(x3_textbox: TextBox,
                                           min_textbox: TextBox, max_textbox: TextBox) =

        let (x3, min, max) = (ref (int 0), ref (float 0), ref (float 0))

        if not (Int32.TryParse(x3_textbox.Text, x3)) ||
           not (Double.TryParse(min_textbox.Text, min)) ||
           not (Double.TryParse(max_textbox.Text, max))
        then
            raise (InnerError("Unable to parse exponent function parameters"))

        (!x3, !min, !max)

    static member retrieve_float(textbox: TextBox) =
        let x = ref (float 0)
        if not (Double.TryParse(textbox.Text, x))
        then raise (InnerError("Unable to parse float"))
        !x

    static member retrieve_int(textbox: TextBox) =
        let x = ref 0
        if not (Int32.TryParse(textbox.Text, x))
        then raise (InnerError("Unable to parse float"))
        !x

    static member refresh_logistic_func_chart(chart: Chart, x1_textbox: TextBox, x2_textbox: TextBox,
                                                min_textbox: TextBox, max_textbox: TextBox,
                                                mu_textbox: TextBox, s_textbox: TextBox,
                                                x_limits: float*float(*, model_params: ModelParameters*)) =
        
        let (x1, x2, min, max) = ParamFormBase.retrieve_logistic_func_param(x1_textbox, x2_textbox, min_textbox, max_textbox)        
        let (mu, s, _, _) = ModelParameters.logistic_func_param(x1, x2, min, max)
        
        let series: Series = chart.Series.Item(0)
        series.Points.Clear()
        let func =  ModelParameters.logistic_func(mu, s, min, max)
        ParamFormBase.func_plot(series, (fun (x: float) -> func(x)), x_limits)
        
        let chart_area = chart.ChartAreas.Item(0)
        chart_area.AxisY.Maximum <- 1.1 * max
        chart_area.AxisY.Minimum <- float 0
        chart_area.AxisY.Interval <- 0.1 * max
        chart.Refresh()

        mu_textbox.Text <- (sprintf "%.1f" mu)
        s_textbox.Text <- (sprintf "%.1f" s)

    static member refresh_exp_func_chart(chart: Chart, x3_textbox: TextBox,
                                            min_textbox: TextBox, max_textbox: TextBox,
                                            n_textbox: TextBox) =

        let (x3, min, max) = ParamFormBase.retrieve_exp_func_param(x3_textbox, min_textbox, max_textbox)
        let (n, _, _) = ModelParameters.exp_func_param(x3, min, max)
        
        let series = chart.Series.Item(0)
        let chart_area = chart.ChartAreas.Item(0)

        series.Points.Clear()
        ParamFormBase.func_plot(series, (ModelParameters.exp_func(n, min, max)),
            (float 0, 1.5 * (float x3)))
        
        chart_area.AxisY.Maximum <- 1.1 * max
        chart_area.AxisY.Minimum <- float 0
        chart_area.AxisY.Interval <- 0.1 * max
        chart.Refresh()

        n_textbox.Text <- (sprintf "%.1f" n)

    static member show_summary(chart: Chart, tooltip: ToolTip, get_summary: float->string)(args: MouseEventArgs) =
        if args.Button = MouseButtons.Right then
            let x = (chart.ChartAreas.Item(0).AxisX.PixelPositionToValue(float args.X))
            tooltip.Show(get_summary(x), chart, Drawing.Point(args.X + 15, args.Y - 15), 20000)
        else if args.Button =  MouseButtons.Left then
            tooltip.Hide(chart)

    static member create_logistic_func_controls(parent: Control, prev_control: Control, chart: Chart,
                                                x1_textbox: TextBox, x2_textbox: TextBox,
                                                mu_textbox: TextBox, s_textbox: TextBox,
                                                min_textbox: TextBox, max_textbox: TextBox,
                                                param: float*float*float*float, x_limits: float*float) =

        //chart.Titles.Add("1 / (1/max + exp((mu - x)/s))") |> ignore
        
        let panel = new Panel()
        panel.ClientSize <- ParamFormBase.plot_size
        
        if prev_control <> null
            then ParamFormBase.place_control_below(panel, prev_control)
            else panel.Location <- ParamFormBase.initial_location

        panel.Controls.Add(chart)

        let chart_area = chart.ChartAreas.Item(0)
        let series = chart.Series.Item(0)

        //let division_prob_chart = new Chart(Dock=DockStyle.Fill)
        //let chart_area = new ChartArea()
        //chart_area.AxisX.Interval <- float 10
        let (xmin, xmax) = x_limits
        chart_area.AxisX.Maximum <- xmax
        chart_area.AxisX.Minimum <- xmin
        chart_area.AxisX.IsLabelAutoFit <- true

        let (x1, x2, min, max) = param
        let (mu, s, _,  _) = ModelParameters.logistic_func_param(param)
        let x1_label = new Label()
        x1_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (2, 2))
        x1_label.AutoSize <- true
        x1_label.Text <- "X coordinate of saturation point 1"
        ParamFormBase.place_control_below(x1_label, panel)

        //let x1_textbox = new TextBox()
        x1_textbox.Size <- ParamFormBase.textbox_size
        x1_textbox.Text <- (sprintf "%.0f" x1)
        ParamFormBase.add_textbox_float_validation(x1_textbox, x1_label.Text, ExternalState.O2Limits)
        ParamFormBase.place_control_totheright(x1_textbox, x1_label)

        let x2_label = new Label()
        x2_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (2, 2))
        x2_label.AutoSize <- true
        x2_label.Text <- "X coordinate of saturation point 2"
        ParamFormBase.place_control_totheright(x2_label, x1_textbox)

        //let x2_textbox = new TextBox()
        x2_textbox.Size <- ParamFormBase.textbox_size
        x2_textbox.Text <- (sprintf "%.0f" x2)
        ParamFormBase.add_textbox_float_validation(x2_textbox, x2_label.Text, ExternalState.O2Limits)
        ParamFormBase.place_control_totheright(x2_textbox, x2_label)

        let min_label = new Label()
        //min_label.Width <- 30
        min_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (1.7, float 2))
        min_label.AutoSize <- true
        min_label.Text <- "The minimum probability (%)"
        ParamFormBase.place_control_below(min_label, x1_label)

        //let max_textbox = new TextBox()
        min_textbox.Size <- ParamFormBase.textbox_size
        min_textbox.Text <- (sprintf "%.1f" (min* float 100))
        ParamFormBase.add_textbox_float_validation(min_textbox, min_label.Text, (float 0, float 100))
        ParamFormBase.place_control_totheright(min_textbox, min_label)

        let max_label = new Label()
        //max_label.Width <- 30
        max_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (1.7, float 2))
        max_label.AutoSize <- true
        max_label.Text <- "The maximum probability (%)"
        ParamFormBase.place_control_totheright(max_label, min_textbox)

        //let max_textbox = new TextBox()
        max_textbox.Size <- ParamFormBase.textbox_size
        max_textbox.Text <- (sprintf "%.1f" (max* float 100))
        ParamFormBase.add_textbox_float_validation(max_textbox, max_label.Text, (float 0, float 100))
        ParamFormBase.add_textbox_float_less_check(min_textbox, max_textbox, min_label.Text, max_label.Text)
        ParamFormBase.place_control_totheright(max_textbox, max_label)

        let refresh_button = new Button()
        refresh_button.Text <- "Refresh the plot"
        refresh_button.MaximumSize <- ParamFormBase.Scale(ParamFormBase.button_size, (2, 1))
        refresh_button.AutoSize <- true
        ParamFormBase.place_control_totheright(refresh_button, max_textbox)
        refresh_button.Click.Add(fun args -> ParamFormBase.refresh_logistic_func_chart(
                                                chart, x1_textbox, x2_textbox,
                                                min_textbox, max_textbox, mu_textbox, s_textbox, x_limits))

        let mu_label = new Label()
        mu_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 2))
        mu_label.AutoSize <- true
        mu_label.Text <- "mu"
        ParamFormBase.place_control_below(mu_label, min_label)

        //let mu_textbox = new TextBox()
        mu_textbox.Size <- ParamFormBase.textbox_size
        mu_textbox.Text <- (sprintf "%.1f" mu)
        ParamFormBase.place_control_totheright(mu_textbox, mu_label)
        mu_textbox.Enabled <- false

        let s_label = new Label()
        s_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 2))
        s_label.AutoSize <- true
        s_label.Text <- "s"
        ParamFormBase.place_control_totheright(s_label, mu_textbox)

        //let s_textbox = new TextBox()
        s_textbox.Size <- ParamFormBase.textbox_size
        s_textbox.Text <- (sprintf "%.1f" s)
        ParamFormBase.place_control_totheright(s_textbox, s_label)
        s_textbox.Enabled <- false

        ParamFormBase.refresh_logistic_func_chart(chart, x1_textbox, x2_textbox,
            min_textbox, max_textbox, mu_textbox, s_textbox, x_limits)

        parent.Controls.AddRange([| panel; x1_label; x1_textbox; x2_label; x2_textbox;
            (*mu_label; mu_textbox; s_label; s_textbox;*) min_label; min_textbox; max_label; max_textbox; refresh_button |])

        let tooltip = new ToolTip()
        chart.MouseClick.Add(ParamFormBase.show_summary(chart, tooltip,
                                                        (fun (x:float) -> sprintf "X=%.1f Y=%.1f" x (ModelParameters.logistic_func(mu, s, min, max)(x)))))

        min_label

    static member create_exp_func_controls(parent: Control, prev_control: Control, chart: Chart, 
                                            x3_textbox: TextBox, n_textbox: TextBox,
                                            min_textbox: TextBox, max_textbox: TextBox,
                                            param: int*float*float, x_limits: float*float) =


        let panel = new Panel()
        panel.ClientSize <- ParamFormBase.plot_size
        
        if prev_control <> null
            then ParamFormBase.place_control_below(panel, prev_control)
            else panel.Location <- ParamFormBase.initial_location
        
        panel.Controls.Add(chart)

        let tooltip = new ToolTip()
        //chart.Titles.Add("max * n^(-x)") |> ignore

        let (x3, min, max) = param
        
        let chart_area = chart.ChartAreas.Item(0)
        let series = chart.Series.Item(0)

        chart_area.AxisX.Maximum <- 1.5*(float x3)
        chart_area.AxisX.Minimum <- float 0
        chart_area.AxisX.Interval <- 0.1*(float x3)
        chart_area.AxisX.IsLabelAutoFit <- true
        
        let x3_label = new Label()
        //x3_label.Width <- 400
        x3_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (2, 2))
        x3_label.AutoSize <- true
        x3_label.Text <- "X coordinate of the saturation point"
        ParamFormBase.place_control_below(x3_label, panel)

        //let x1_textbox = new TextBox()
        x3_textbox.Size <- ParamFormBase.textbox_size
        x3_textbox.Text <- (sprintf "%d" (int x3))
        ParamFormBase.add_textbox_int_validation(x3_textbox, x3_label.Text, (0, Int32.MaxValue))
        ParamFormBase.place_control_totheright(x3_textbox, x3_label)

        let min_label = new Label()
        //max_label.Width <- 30
        min_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (1.7, float 2))
        min_label.AutoSize <- true
        min_label.Text <- "The minimum probability (%)"
        ParamFormBase.place_control_below(min_label, x3_label)

        //let max_textbox = new TextBox()
        min_textbox.Size <- ParamFormBase.textbox_size
        min_textbox.Text <- (sprintf "%.1f" (min* float 100))
        ParamFormBase.add_textbox_float_validation(min_textbox, min_label.Text, (float 0, float 100))
        ParamFormBase.place_control_totheright(min_textbox, min_label)

        let max_label = new Label()
        //max_label.Width <- 30
        max_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (1.7, float 2))
        max_label.AutoSize <- true
        max_label.Text <- "The maximum probability (%)"
        ParamFormBase.place_control_totheright(max_label, min_textbox)

        //let max_textbox = new TextBox()
        max_textbox.Size <- ParamFormBase.textbox_size
        max_textbox.Text <- (sprintf "%.1f" (max* float 100))
        ParamFormBase.add_textbox_float_validation(max_textbox, max_label.Text, (float 0, float 100))
        ParamFormBase.add_textbox_float_less_check(min_textbox, max_textbox, min_label.Text, max_label.Text)
        ParamFormBase.place_control_totheright(max_textbox, max_label)

        let refresh_button = new Button()
        refresh_button.Text <- "Refresh the plot"
        refresh_button.MaximumSize <- ParamFormBase.Scale(ParamFormBase.button_size, (2, 1))
        refresh_button.AutoSize <- true
        ParamFormBase.place_control_totheright(refresh_button, max_textbox)
        refresh_button.Click.Add(fun args -> ParamFormBase.refresh_exp_func_chart(
                                                chart, x3_textbox,
                                                min_textbox, max_textbox, n_textbox))

        let (n, _, _) = ModelParameters.exp_func_param(ModelParameters.NonStemToStemProbParam)
        let n_label = new Label()
        n_label.MaximumSize <- ParamFormBase.Scale(ParamFormBase.label_size, (0.4, float 2))
        n_label.Text <- "n"
        ParamFormBase.place_control_below(n_label, x3_label)

        //let mu_textbox = new TextBox()
        n_textbox.Size <- ParamFormBase.textbox_size
        n_textbox.Text <- (sprintf "%.1f" n)
        ParamFormBase.place_control_totheright(n_textbox, n_label)
        n_textbox.Enabled <- false

        ParamFormBase.refresh_exp_func_chart(chart, x3_textbox, min_textbox, max_textbox, n_textbox) 

        parent.Controls.AddRange([| panel; x3_label; x3_textbox;
            (*n_label; n_textbox;*) min_label; min_textbox; max_label; max_textbox; refresh_button|])

        chart.MouseClick.Add(ParamFormBase.show_summary(chart, tooltip,
                                                        (fun (x:float) -> sprintf "X=%.1f Y=%.1f" x (ModelParameters.exp_func(n, min, max)(x)))))

        min_label

    static member create_ok_cancel_buttons(parent: Control, prev_control: Control, ok_func: EventArgs->unit) =
        let ok_button = new Button()
        ok_button.MaximumSize <- ParamFormBase.Scale(ParamFormBase.button_size, (1, 1))
        ok_button.AutoSize <- true
        ok_button.Text <- "Ok"
        ParamFormBase.place_control_totheright(ok_button, prev_control)
        ok_button.Click.Add(fun args -> ok_func(args); parent.Visible <- false)

        let cancel_button = new Button()
        cancel_button.MaximumSize <- ParamFormBase.Scale(ParamFormBase.button_size, (1, 1))
        cancel_button.AutoSize <- true
        cancel_button.Text <- "Cancel"
        ParamFormBase.place_control_below(cancel_button, ok_button)
        cancel_button.Click.Add(fun args -> parent.Visible <- false)

        parent.Controls.AddRange([| ok_button; cancel_button |])
        ok_button

    static member hide_form(form: Form)(args: FormClosingEventArgs) =
        args.Cancel <- true
        form.Visible <- false