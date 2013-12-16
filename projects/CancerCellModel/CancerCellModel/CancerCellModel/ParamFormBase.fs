module ParamFormBase

open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting
open System.ComponentModel
open MathFunctions
open Geometry

type FormDesigner() =
    static member x_interval with get() = 10
    static member y_interval with get() = 15
    static member initial_location with get() = Drawing.Point(30, 30)
    static member label_size with get() = Drawing.Size(50, 15)
    static member button_size with get() = Drawing.Size(60, 20)
    static member textbox_size with get() = Drawing.Size(50, 10)
    static member plot_size with get() = Drawing.Size(350, 350)

    static member Scale(size: Drawing.Size, (sx: float, sy: float)) =
        let width = size.Width
        let height = size.Height
        Drawing.Size(int(float width * sx), int(float height * sy))

    static member Scale(size: Drawing.Size, (sx: int, sy: int)) =
        FormDesigner.Scale(size, (float sx, float sy))

    static member place_control_totheright(c1: Control, c2: Control) =
        c1.Location <- Drawing.Point(c2.Location.X + (if c2.AutoSize && c2.MaximumSize.Width>0 then c2.MaximumSize.Width else c2.Width) +
                                    FormDesigner.x_interval, c2.Location.Y)

    static member place_control_below(c1: Control, c2: Control, ?extra_space: int) =
        let dy = defaultArg extra_space 0
        c1.Location <- Drawing.Point(c2.Location.X, c2.Location.Y +
                                     (if c2.AutoSize && c2.MaximumSize.Height > 0 then c2.MaximumSize.Height else c2.Height) +
                                     FormDesigner.y_interval + dy)

    static member textbox_float_interval_check(textbox: TextBox, name: string, range: FloatInterval)
                                           (args: CancelEventArgs) =
        
        let mutable x = ref (0.)
        let err = ref false
        let msg = ref ""

        if not (Double.TryParse(textbox.Text, x)) then
            err := true
            msg := (sprintf "%s must be a real number" name)
        else if !x < (range).Min || !x > (range).Max then
            err := true
            if (range).Max = Double.MaxValue then
                msg := (sprintf "%s can take on a real value greater or equal to >= %.1f" name (range).Min)
            else
                msg := (sprintf "%s can take on a real value from %.1f to %.1f" name (range).Min (range).Max)
        
        if !err then
            args.Cancel <- true
            MessageBox.Show(!msg, "Data validation error", MessageBoxButtons.OK, MessageBoxIcon.Error) |> ignore
            textbox.Select(0, textbox.Text.Length)

    static member textbox_int_interval_check(textbox: TextBox, name: string, range: IntInterval)
                                           (args: CancelEventArgs) =
        
        let x = ref (int 0)
        let err = ref false
        let msg = ref ""

        if not (Int32.TryParse(textbox.Text, x)) then
            err := true
            msg := (sprintf "%s must be an integer number" name)
        else if !x < range.Min || !x > range.Max then
            err := true
            if range.Max = Int32.MaxValue then
                msg := (sprintf "%s can take on an integer value greater or equal to >= %d" name range.Min)
            else
                msg := (sprintf "%s can take on an integer value from %d to %d" name range.Min range.Max)
        
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

    static member add_textbox_float_validation(textbox: TextBox, name: string, range: FloatInterval) =
        textbox.Validating.Add(fun args -> FormDesigner.textbox_float_interval_check(textbox, name, range)(args))

    static member add_textbox_int_validation(textbox: TextBox, name: string, range: IntInterval) =
        textbox.Validating.Add(fun args -> FormDesigner.textbox_int_interval_check(textbox, name, range)(args))

    static member add_textbox_float_less_check(textbox1: TextBox, textbox2: TextBox, name1: string, name2: string) =
        textbox1.Validating.Add(fun args -> FormDesigner.textbox_float_func_check(textbox1, textbox2, name1, name2,
                                                                                    (fun (x1: float, x2: float) -> x1 < x2),
                                                                                    (sprintf "%s should be less than %s" name1 name2))(args))

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

    static member retrieve_int_interval(min_textbox: TextBox, max_textbox: TextBox) =
        IntInterval(FormDesigner.retrieve_int(min_textbox), FormDesigner.retrieve_int(max_textbox))

type LogisticFuncParamDialog(x_limits: FloatInterval) as this =
    inherit Form (Visible = false, Width = 600, Height = 400)

    let x1_textbox = new TextBox()
    let x2_textbox = new TextBox()
    let min_label = new Label()
    let min_textbox = new TextBox()
    let max_label = new Label()
    let max_textbox = new TextBox()
    let ok_button = new Button()
    let mutable y_scale = 1.

    do
        let x1_label = new Label()
        x1_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 2))
        x1_label.AutoSize <- true
        x1_label.Text <- "X coordinate of saturation point 1"
        x1_label.Location <- FormDesigner.initial_location

        x1_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.add_textbox_float_validation(x1_textbox, x1_label.Text, x_limits)
        FormDesigner.place_control_totheright(x1_textbox, x1_label)

        let x2_label = new Label()
        x2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 2))
        x2_label.AutoSize <- true
        x2_label.Text <- "X coordinate of saturation point 2"
        FormDesigner.place_control_totheright(x2_label, x1_textbox)

        x2_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.add_textbox_float_validation(x2_textbox, x2_label.Text, x_limits)
        FormDesigner.place_control_totheright(x2_textbox, x2_label)

        min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1.7, float 2))
        min_label.AutoSize <- true
        min_label.Text <- "The minimum function value"
        FormDesigner.place_control_below(min_label, x1_label)

        min_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.place_control_totheright(min_textbox, min_label)

        max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1.7, float 2))
        max_label.AutoSize <- true
        max_label.Text <- "The maximum function value"
        FormDesigner.place_control_totheright(max_label, min_textbox)

        max_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.place_control_totheright(max_textbox, max_label)

        this.AcceptButton <- ok_button
        ok_button.Text <- "Ok"
        ok_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (2, 1))
        ok_button.AutoSize <- true
        FormDesigner.place_control_totheright(ok_button, max_textbox)
        ok_button.Click.Add(fun args -> this.DialogResult <- DialogResult.OK; this.Close())

        base.Controls.AddRange([| x1_label; x1_textbox; x2_label; x2_textbox;
            min_label; min_textbox; max_label; max_textbox; ok_button |])

    member this.Init(func: LogisticFunc, y_interval: FloatInterval, yscale: float) =
        y_scale <- yscale
        this.DialogResult <- DialogResult.Cancel
        x1_textbox.Text <- (sprintf "%.0f" func.Min.x)
        x2_textbox.Text <- (sprintf "%.0f" func.Max.x)
        min_textbox.Text <- (sprintf "%.1f" (y_scale*func.Min.y))
        max_textbox.Text <- (sprintf "%.1f" (y_scale*func.Max.y))
        FormDesigner.add_textbox_float_validation(min_textbox, min_label.Text, y_interval)
        FormDesigner.add_textbox_float_validation(max_textbox, max_label.Text, y_interval)
        FormDesigner.add_textbox_float_less_check(min_textbox, max_textbox, min_label.Text, max_label.Text)


    member this.Func with get() =
                                    let x1 = FormDesigner.retrieve_float(x1_textbox)
                                    let x2 = FormDesigner.retrieve_float(x2_textbox)
                                    let min = FormDesigner.retrieve_float(min_textbox) / y_scale
                                    let max = FormDesigner.retrieve_float(max_textbox) / y_scale
                                    LogisticFunc(min = Point(x1, min), max = Point(x2, max))

type ShiftExpFuncParamDialog(x_limits: FloatInterval, is_var_integer: bool) as this =
    inherit Form (Visible = false, Width = 600, Height = 400)

    let x3_textbox = new TextBox()
    let min_label = new Label()
    let min_textbox = new TextBox()
    let max_label = new Label()
    let max_textbox = new TextBox()
    let ok_button = new Button()
    let mutable y_scale = 1.

    do
        let x3_label = new Label()
        x3_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2, 2))
        x3_label.AutoSize <- true
        x3_label.Text <- "X coordinate of the saturation point"
        x3_label.Location <- FormDesigner.initial_location

        x3_textbox.Size <- FormDesigner.textbox_size
        if is_var_integer then
            FormDesigner.add_textbox_int_validation(x3_textbox, x3_label.Text,
                    IntInterval(int (Math.Round(x_limits.Min)), int (Math.Round(x_limits.Max))))
        else
            FormDesigner.add_textbox_float_validation(x3_textbox, x3_label.Text, x_limits)

        FormDesigner.place_control_totheright(x3_textbox, x3_label)

        min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1.7, float 2))
        min_label.AutoSize <- true
        min_label.Text <- "The minimum function value"
        FormDesigner.place_control_below(min_label, x3_label)

        min_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.place_control_totheright(min_textbox, min_label)

        max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1.7, float 2))
        max_label.AutoSize <- true
        max_label.Text <- "The maximum function value"
        FormDesigner.place_control_totheright(max_label, min_textbox)

        max_textbox.Size <- FormDesigner.textbox_size
        FormDesigner.place_control_totheright(max_textbox, max_label)

        this.AcceptButton <- ok_button
        ok_button.Text <- "Ok"
        ok_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (2, 1))
        ok_button.AutoSize <- true
        FormDesigner.place_control_totheright(ok_button, max_textbox)
        ok_button.Click.Add(fun args -> this.DialogResult <- DialogResult.OK; this.Close())

        base.Controls.AddRange([| x3_label; x3_textbox;
            min_label; min_textbox; max_label; max_textbox; ok_button |])

    member this.Init(func: ShiftExponentFunc, func_interval: FloatInterval, yscale: float) =
        y_scale <- yscale
        this.DialogResult <- DialogResult.Cancel
        x3_textbox.Text <- (sprintf "%.1f" (func.P2.x))
        min_textbox.Text <- (sprintf "%.1f" (func.YMin* y_scale))
        max_textbox.Text <- (sprintf "%.1f" (func.P1.y* y_scale))
        FormDesigner.add_textbox_float_validation(min_textbox, min_label.Text, func_interval)

    member this.Func with get() =
                                    let x3 = FormDesigner.retrieve_float(x3_textbox)
                                    let min = FormDesigner.retrieve_float(min_textbox) / y_scale
                                    let max = FormDesigner.retrieve_float(max_textbox) / y_scale
                                    ShiftExponentFunc(p1 = Point(0., max), p2 = Point(x3, min + (max - min)/y_scale), ymin = min)

type ParamFormBase(?Width, ?Height) =
    inherit Form (Visible = false, Width = defaultArg Width 1200, Height = defaultArg Height 850)

    static member func_plot(series: Series, func: float -> float, x_limits: FloatInterval) =
        let x = ref x_limits.Min
        let n = 100
        let dx = (x_limits.Max - x_limits.Min) / float n
        while !x <= x_limits.Max do
            series.Points.AddXY(!x, func(!x)) |> ignore
            x := !x + dx

    static member get_chart_yvalue(series: Series, x: float) =
        let point = series.Points.FindByValue(float (round(x)), "X")
        point.YValues.[0]

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
        LogisticFunc(min = Point(!x1, !min), max = Point(!x2, !max))

    static member retrieve_shiftexp_func_param(x3_textbox: TextBox,
                                                min_textbox: TextBox, max_textbox: TextBox) =

        let (x3, min, max) = (ref (int 0), ref (float 0), ref (float 0))

        if not (Int32.TryParse(x3_textbox.Text, x3)) ||
           not (Double.TryParse(min_textbox.Text, min)) ||
           not (Double.TryParse(max_textbox.Text, max))
        then
            raise (InnerError("Unable to parse exponent function parameters"))

        ShiftExponentFunc(p1 = Point(float 0, !max), p2 = Point(float !x3, !min + (!max - !min)/100.), ymin = !min)

    static member refresh_logistic_func_chart(chart: Chart, func: LogisticFunc,
                                                x_limits: FloatInterval) =
        
        let series: Series = chart.Series.Item(0)
        series.Points.Clear()
        ParamFormBase.func_plot(series, (fun (x: float) -> func.Y(x)), x_limits)
        
        let chart_area = chart.ChartAreas.Item(0)
        chart_area.AxisY.Maximum <- 1.1 * func.Max.y
        chart_area.AxisY.Minimum <- float 0
        chart_area.AxisY.Interval <- 0.1 * func.Max.y
        chart.Refresh()

    static member refresh_shiftexp_func_chart(chart: Chart, func: ShiftExponentFunc, x_limits: FloatInterval) =

        let series = chart.Series.Item(0)
        let chart_area = chart.ChartAreas.Item(0)

        series.Points.Clear()
        ParamFormBase.func_plot(series, func.Y, x_limits)
        
        let ymax = (max func.P1.y func.P2.y)
        chart_area.AxisY.Maximum <- 1.1 * ymax
        chart_area.AxisY.Minimum <- func.YMin
        chart_area.AxisY.Interval <- 0.1 * (ymax - func.YMin)
        chart.Refresh()

    static member show_summary(chart: Chart, tooltip: ToolTip, get_summary: float->string)(args: MouseEventArgs) =
        if args.Button = MouseButtons.Right then
            let x = (chart.ChartAreas.Item(0).AxisX.PixelPositionToValue(float args.X))
            tooltip.Show(get_summary(x), chart, Drawing.Point(args.X + 15, args.Y - 15), 20000)
        else if args.Button =  MouseButtons.Left then
            tooltip.Hide(chart)

    static member show_summary2(form: Form, tooltip: ToolTip, get_summary: Drawing.Point->string)(args: MouseEventArgs) =
        if args.Button = MouseButtons.Left then
            tooltip.Show(get_summary(args.Location), form,
                                Drawing.Point(args.X + 15, args.Y - 15), 20000)

        else if args.Button =  MouseButtons.Right then
            tooltip.Hide(form)


    static member create_logistic_func_controls(parent: Control, prev_control: Control, chart: Chart,
                                                func: ref<LogisticFunc>,
                                                x_limits: FloatInterval, y_limits: FloatInterval, y_scale: float) =

        let panel = new Panel()
        panel.ClientSize <- FormDesigner.plot_size
        
        if prev_control <> null
            then FormDesigner.place_control_below(panel, prev_control)
            else panel.Location <- FormDesigner.initial_location

        panel.Controls.Add(chart)

        let chart_area = chart.ChartAreas.Item(0)
        let series = chart.Series.Item(0)

        chart_area.AxisX.Maximum <- x_limits.Max
        chart_area.AxisX.Minimum <- x_limits.Min
        chart_area.AxisX.IsLabelAutoFit <- true

        let change_param_button = new Button()
        change_param_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (2.5, 1.))
        change_param_button.AutoSize <- true
        change_param_button.Text <- "Change parameters"
        change_param_button.Click.Add(ParamFormBase.show_logistic_func_dialog(chart, func, x_limits, y_limits, y_scale))
        FormDesigner.place_control_totheright(change_param_button, panel)

        ParamFormBase.refresh_logistic_func_chart(chart, !func, x_limits)

        parent.Controls.AddRange([| panel; change_param_button |])

        let tooltip = new ToolTip()
        chart.MouseClick.Add(ParamFormBase.show_summary(chart, tooltip,
                                                        (fun (x:float) -> sprintf "X=%.1f Y=%.1f" x ((!func).Y(x)))))

        panel

    static member create_shiftexp_func_controls(parent: Control, prev_control: Control, chart: Chart, 
                                                func: ref<ShiftExponentFunc>,
                                                x_limits: FloatInterval, y_limits: FloatInterval, y_scale: float,
                                                is_var_integer: bool) =


        let panel = new Panel()
        panel.ClientSize <- FormDesigner.plot_size
        
        if prev_control <> null
            then FormDesigner.place_control_below(panel, prev_control)
            else panel.Location <- FormDesigner.initial_location
        
        panel.Controls.Add(chart)
        let tooltip = new ToolTip()
        
        let chart_area = chart.ChartAreas.Item(0)
        let series = chart.Series.Item(0)

        chart_area.AxisX.Maximum <- 1.5* (!func).P2.x
        chart_area.AxisX.Minimum <- float 0
        chart_area.AxisX.Interval <- 0.1* (!func).P2.x
        chart_area.AxisX.IsLabelAutoFit <- true

        let change_param_button = new Button()
        change_param_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (2.5, 1.))
        change_param_button.AutoSize <- true
        change_param_button.Text <- "Change parameters"
        change_param_button.Click.Add(ParamFormBase.show_shiftexp_func_dialog(chart, func, x_limits, y_limits, y_scale, is_var_integer))
        FormDesigner.place_control_totheright(change_param_button, panel)

        ParamFormBase.refresh_shiftexp_func_chart(chart, !func, x_limits) 
        parent.Controls.AddRange([| panel; change_param_button |])
        chart.MouseClick.Add(ParamFormBase.show_summary(chart, tooltip,
                                                        (fun (x:float) -> sprintf "X=%.1f Y=%.1f" x ((!func).Y(x)))))

        panel

    static member create_ok_cancel_buttons(parent: Control, prev_control: Control, ok_func: EventArgs->unit) =
        let ok_button = new Button()
        ok_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (1, 1))
        ok_button.AutoSize <- true
        ok_button.Text <- "Ok"
        FormDesigner.place_control_totheright(ok_button, prev_control)
        ok_button.Click.Add(fun args -> ok_func(args); parent.Visible <- false)

        let cancel_button = new Button()
        cancel_button.MaximumSize <- FormDesigner.Scale(FormDesigner.button_size, (1, 1))
        cancel_button.AutoSize <- true
        cancel_button.Text <- "Cancel"
        FormDesigner.place_control_below(cancel_button, ok_button)
        cancel_button.Click.Add(fun args -> parent.Visible <- false)

        parent.Controls.AddRange([| ok_button; cancel_button |])
        ok_button

    static member hide_form(form: Form)(args: FormClosingEventArgs) =
        args.Cancel <- true
        form.Visible <- false

    static member show_logistic_func_dialog(chart: Chart, func: ref<LogisticFunc>,
                                            x_limits: FloatInterval, y_limits: FloatInterval, y_scale: float)
                                            (args: EventArgs) =

        let logistic_func_dialog = new LogisticFuncParamDialog(x_limits)
        logistic_func_dialog.Init(!func, y_limits, y_scale)
        if logistic_func_dialog.ShowDialog() = DialogResult.OK then
            func := logistic_func_dialog.Func
            ParamFormBase.refresh_logistic_func_chart(chart, !func, x_limits)
        logistic_func_dialog.Dispose()

    static member show_shiftexp_func_dialog(chart: Chart, func: ref<ShiftExponentFunc>,
                                            x_limits: FloatInterval, y_limits: FloatInterval, y_scale: float,
                                            is_var_integer: bool)(args: EventArgs) =

        let shift_exp_func_dialog = new ShiftExpFuncParamDialog(x_limits, is_var_integer)
        shift_exp_func_dialog.Init(!func, y_limits, y_scale)
        if shift_exp_func_dialog.ShowDialog() = DialogResult.OK then
            func := shift_exp_func_dialog.Func
            ParamFormBase.refresh_shiftexp_func_chart(chart, !func, x_limits)
        shift_exp_func_dialog.Dispose()

    static member create_int_interval_controls(parent: Control, prev_control: Control, name: string,
                                                min_textbox: TextBox, max_textbox: TextBox,
                                                interval: IntInterval, limits: IntInterval) =
        let interval_label = new Label()
        interval_label.Text <- name
        interval_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10., 1.))
        interval_label.AutoSize <- true
        FormDesigner.place_control_below(interval_label, prev_control)

        let min_label = new Label()
        min_label.Text <- "Min"
        min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (0.6, 1.))
        min_label.AutoSize <- true
        FormDesigner.place_control_below(min_label, interval_label)

        min_textbox.Text <- (sprintf "%d" interval.Min)
        FormDesigner.add_textbox_int_validation(min_textbox, min_label.Text, limits)
        FormDesigner.place_control_totheright(min_textbox, min_label)

        let max_label = new Label()
        max_label.Text <- "Max"
        max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (0.6, 1.))
        max_label.AutoSize <- true
        FormDesigner.place_control_totheright(max_label, min_textbox)

        max_textbox.Text <- (sprintf "%d" interval.Max)
        FormDesigner.add_textbox_int_validation(max_textbox, max_label.Text, limits)
        FormDesigner.place_control_totheright(max_textbox, max_label)

        parent.Controls.AddRange([| interval_label; min_label; min_textbox; max_label; max_textbox |])
        min_label

    static member create_float_interval_controls(parent: Control, prev_control: Control, name: string,
                                                    min_textbox: TextBox, max_textbox: TextBox,
                                                    interval: FloatInterval, limits: FloatInterval) =
        let interval_label = new Label()
        interval_label.Text <- name
        interval_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10., 1.))
        interval_label.AutoSize <- true
        FormDesigner.place_control_below(interval_label, prev_control)

        let min_label = new Label()
        min_label.Text <- "Min"
        min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (0.6, 1.))
        min_label.AutoSize <- true
        FormDesigner.place_control_below(min_label, interval_label)

        min_textbox.Text <- (sprintf "%.1f" interval.Min)
        FormDesigner.add_textbox_float_validation(min_textbox, min_label.Text, limits)
        FormDesigner.place_control_totheright(min_textbox, min_label)

        let max_label = new Label()
        max_label.Text <- "Max"
        max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (0.6, 1.))
        max_label.AutoSize <- true
        FormDesigner.place_control_totheright(max_label, min_textbox)

        max_textbox.Text <- (sprintf "%.1f" interval.Max)
        FormDesigner.add_textbox_float_validation(max_textbox, max_label.Text, limits)
        FormDesigner.place_control_totheright(max_textbox, max_label)

        parent.Controls.AddRange([| interval_label; min_label; min_textbox; max_label; max_textbox |])
        min_label

[<AbstractClass>]
type DrawingForm(Visible: bool, Width: int, Height: int) as this =
    inherit ParamFormBase(Visible = Visible, Width = Width, Height = Height)

    let width, height = this.ClientSize.Width, this.ClientSize.Height
    let bitmap = new System.Drawing.Bitmap(width, height)
    let graphics = System.Drawing.Graphics.FromImage(bitmap)

    abstract member get_summary: Geometry.Point -> String
    member this.Graphics with get() = graphics
    member this.Bitmap with get() = bitmap
    member this.Width with get() = width
    member this.Height with get() = height