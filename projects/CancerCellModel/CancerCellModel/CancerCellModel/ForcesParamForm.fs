module ForcesParamForm

open ParamFormBase
open ModelParameters
open Geometry
open System
open System.Windows.Forms
open System.Windows.Forms.DataVisualization.Charting

type ForcesParamForm() as this =
    inherit ParamFormBase (Width=600, Height = 500)

    let repulsive_force_chart = new Chart(Dock=DockStyle.Fill)
    let repulsive_force_chart_area = new ChartArea()
    let repulsive_force_series = new Series(ChartType = SeriesChartType.Line)

    let friction_force_textbox = new TextBox()

    let apply_changes(args: EventArgs) = ()

    // initialise the window
    do
        base.Text <- "Forces"
        base.ClientSize <- Drawing.Size(base.Size.Width, base.Size.Height)

        repulsive_force_chart.Titles.Add("Repulsive force") |> ignore
        repulsive_force_chart.ChartAreas.Add(repulsive_force_chart_area)
        repulsive_force_chart.Series.Add(repulsive_force_series)
        repulsive_force_chart_area.AxisX.Title <- "Cell displacement"
        repulsive_force_chart_area.AxisY.Title <- "Repulsive force"

        let control = ParamFormBase.create_shiftexp_func_controls(
                                 this, null, repulsive_force_chart, ModelParameters.RepulsiveForce,
                                 FloatInterval(0., 1.),  ModelParameters.DisplacementInterval, 1., false)

        ParamFormBase.refresh_shiftexp_func_chart(repulsive_force_chart,
            !ModelParameters.RepulsiveForce, ModelParameters.DisplacementInterval)

        let friction_force_label  = new Label()
        friction_force_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (2,3))
        friction_force_label.AutoSize <- true
        friction_force_label.Text <- "Viscosity coefficient"
        FormDesigner.place_control_below(friction_force_label, control, 2*FormDesigner.y_interval)

        friction_force_textbox.Text <- (sprintf "%.1f" ModelParameters.ViscosityCoeff)
        FormDesigner.add_textbox_float_validation(friction_force_textbox, friction_force_label.Text,
            FloatInterval(0., Double.MaxValue))

        FormDesigner.place_control_totheright(friction_force_textbox, friction_force_label)

        base.Controls.AddRange([| friction_force_label; friction_force_textbox |])