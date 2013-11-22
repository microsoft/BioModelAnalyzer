module GlobalStateParamForm

open System
open System.Windows.Forms
open ParamFormBase
open ModelParameters
open Geometry

type GlobalStateParamForm() as this =
    inherit ParamFormBase(Width = 900, Height = 550)

    let c1_textbox = new TextBox()
    let c2_textbox = new TextBox()
    let c3_textbox = new TextBox()
    let o2_diffusion_coeff_textbox = new TextBox()
    let egf_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.O2Param <- (FormDesigner.retrieve_float(c1_textbox),
            FormDesigner.retrieve_float(c2_textbox), FormDesigner.retrieve_float(c3_textbox))

        ModelParameters.EGFProb <- FormDesigner.retrieve_float(egf_textbox)

    do
        /////////////////////// NUTRIENTS ///////////////////////////////

        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(int (float base.Size.Width*0.55) - 2* FormDesigner.x_interval, base.Size.Height - 4* FormDesigner.y_interval)
        nutrient_groupbox.Location <- FormDesigner.initial_location
        nutrient_groupbox.ClientSize <- Drawing.Size(int (float nutrient_groupbox.Size.Width * 0.9),
                                                        int (float nutrient_groupbox.Size.Height*0.9))

        let o2_func_label = new Label()
        o2_func_label.Text <- "The concentration of oxygen at point (x,y) is calculated as follows:\n\n\
                O2(x, y, t+dt) = O2(x, y, t) + dt * (D* nabla_squared(O2(x, y, t)) + \n    \
                    supply_rate - consumption_rate)\n\n\
                where nabla_squared(O2) (or Laplace operator) is the sum of second \n    \
                derivates at point(x, y),\n\n\
                supply_rate = c1 if the point (x, y) is outside the tumour mass\n    \
                    and 0 otherwise\n\n\
                and consumption_rate = (c2*dividing_cells + c3*non_dividing_cells)\n    \
                    where dividing_cells and non_dividing_cells are the numbers of\n    \
                    dividing and non-dividing resp. live cells in the grid mesh\n    \
                    embracing the point (x,y)\n"

        o2_func_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10, 13))
        o2_func_label.AutoSize <- true
        o2_func_label.Location <- FormDesigner.initial_location

        let (c1, c2, c3) = ModelParameters.O2Param
        let c1_label = new Label()
        c1_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c1_label.AutoSize <- true
        c1_label.Text <- "c1 (Supply of oxygen per time step)"
        FormDesigner.place_control_below(c1_label, o2_func_label)

        c1_textbox.Text <- (sprintf "%.3f" c1)
        FormDesigner.add_textbox_float_validation(c1_textbox, c1_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c1_textbox, c1_label)

        let c2_label = new Label()
        c2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c2_label.AutoSize <- true
        c2_label.Text <- "c2 (Consumption of oxygen by one dividing cell per time step)"
        FormDesigner.place_control_below(c2_label, c1_label)

        c2_textbox.Text <- (sprintf "%.3f" c2)
        FormDesigner.add_textbox_float_validation(c2_textbox, c2_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c2_textbox, c2_label)

        let c3_label = new Label()
        c3_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c3_label.AutoSize <- true
        c3_label.Text <- "c3 (Consumption of oxygen by one non-dividing live cell per time step)"
        FormDesigner.place_control_below(c3_label, c2_label)

        c3_textbox.Text <- (sprintf "%.3f" c3)
        FormDesigner.add_textbox_float_validation(c3_textbox, c3_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c3_textbox, c3_label)

        let o2_diffusion_coeff_label = new Label()
        o2_diffusion_coeff_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        o2_diffusion_coeff_label.AutoSize <- true
        o2_diffusion_coeff_label.Text <- "D (oxygen diffusion coefficient)"
        FormDesigner.place_control_below(o2_diffusion_coeff_label, c3_label)

        o2_diffusion_coeff_textbox.Text <- (sprintf "%.1f" ModelParameters.OxygenDiffusionCoeff)
        FormDesigner.add_textbox_float_validation(o2_diffusion_coeff_textbox, o2_diffusion_coeff_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(o2_diffusion_coeff_textbox, o2_diffusion_coeff_label)

        nutrient_groupbox.Controls.AddRange([| o2_func_label; c1_label; c1_textbox;
                                                c2_label; c2_textbox; c3_label; c3_textbox;
                                                o2_diffusion_coeff_label; o2_diffusion_coeff_textbox |])

        ///////////////////// PATHWAYS ///////////////////////////////////////
        
        let pathway_groupbox = new GroupBox()
        pathway_groupbox.Text <- "Pathways"
        pathway_groupbox.Size <- Drawing.Size(int(float base.Size.Width*0.4) - 2* FormDesigner.x_interval, base.Size.Height - 4* FormDesigner.y_interval)
        FormDesigner.place_control_totheright(pathway_groupbox, nutrient_groupbox)
        pathway_groupbox.ClientSize <- Drawing.Size(int (float pathway_groupbox.Size.Width * 0.9),
                                                        int (float pathway_groupbox.Size.Height*0.9))

        let egf_label = new Label()
        egf_label.Text <- "The probability that EGF is Up"
        egf_label.Location <- FormDesigner.initial_location

        egf_textbox.Text <- (sprintf "%.1f" ModelParameters.EGFProb)
        FormDesigner.place_control_totheright(egf_textbox, egf_label)

        pathway_groupbox.Controls.AddRange([| egf_label; egf_textbox |])
        ParamFormBase.create_ok_cancel_buttons(this, pathway_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])