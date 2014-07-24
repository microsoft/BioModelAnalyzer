module GlobalStateParamForm

open System
open System.Windows.Forms
open ParamFormBase
open ModelParameters
open Geometry

type GlobalStateParamForm() as this =
    inherit ParamFormBase(Width = 950, Height = 1000)

    let c1_o2_textbox = new TextBox()
    let c2_o2_textbox = new TextBox()
    let c3_o2_textbox = new TextBox()
    let o2_diffusion_coeff_textbox = new TextBox()
    let c1_glucose_textbox = new TextBox()
    let c2_glucose_textbox = new TextBox()
    let c3_glucose_textbox = new TextBox()
    let glucose_diffusion_coeff_textbox = new TextBox()
    let egf_textbox = new TextBox()

    let apply_changes(args: EventArgs) =
        ModelParameters.O2Param <- (FormDesigner.retrieve_float(c1_o2_textbox),
            FormDesigner.retrieve_float(c2_o2_textbox), FormDesigner.retrieve_float(c3_o2_textbox))

        ModelParameters.GlucoseParam <- (FormDesigner.retrieve_float(c1_glucose_textbox),
            FormDesigner.retrieve_float(c2_glucose_textbox), FormDesigner.retrieve_float(c3_glucose_textbox))

        ModelParameters.EGFProb <- FormDesigner.retrieve_float(egf_textbox)

    do
        base.Text <- "Global state parameters"

        /////////////////////// NUTRIENTS ///////////////////////////////
        let nutrient_groupbox = new GroupBox()
        nutrient_groupbox.Text <- "Nutrients"
        nutrient_groupbox.Size <- Drawing.Size(int (float base.Size.Width*0.62) - 2*FormDesigner.x_interval, base.Size.Height - 4*FormDesigner.y_interval)
        (*nutrient_groupbox.Location <- FormDesigner.initial_location*)
        nutrient_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        nutrient_groupbox.ClientSize <- Drawing.Size(int(float nutrient_groupbox.Size.Width * 0.9), int(float nutrient_groupbox.Size.Height * 0.9))
        
        // build o2_func_label
        let o2_func_label = new Label()
        o2_func_label.Text <- "The concentration of oxygen at point (x,y) is calculated as follows:\n\n\
                O2(x, y, t+dt) = O2(x, y, t) + dt * (D_O2 * nabla_squared(O2(x, y, t)) + \n    \
                    supply_rate - consumption_rate)\n\n\
                where nabla_squared(O2) (or Laplace operator) is the sum of second \n    \
                derivates at point(x,y)\n\n\
                supply_rate = c1_O2 if the point (x,y) is outside the tumour mass\n    \
                    and 0 otherwise\n\n\
                and consumption_rate = (c2_O2*dividing_cells + c3_O2*non_dividing_cells)\n    \
                    where dividing_cells and non_dividing_cells are the numbers of\n    \
                    dividing and non-dividing resp. live cells in the grid mesh\n    \
                    embracing the point (x,y)\n"

        o2_func_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10, 13))
        o2_func_label.AutoSize <- true
        o2_func_label.Location <- FormDesigner.initial_location

        let (c1_o2, c2_o2, c3_o2) = ModelParameters.O2Param
        let c1_o2_label = new Label()
        c1_o2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c1_o2_label.AutoSize <- true
        c1_o2_label.Text <- "c1_O2 (Supply of oxygen per time step)"
        FormDesigner.place_control_below(c1_o2_label, o2_func_label)

        c1_o2_textbox.Text <- (sprintf "%.3f" c1_o2)
        FormDesigner.add_textbox_float_validation(c1_o2_textbox, c1_o2_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c1_o2_textbox, c1_o2_label)

        let c2_o2_label = new Label()
        c2_o2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c2_o2_label.AutoSize <- true
        c2_o2_label.Text <- "c2_O2 (Consumption of oxygen by one dividing cell per time step)"
        FormDesigner.place_control_below(c2_o2_label, c1_o2_label)

        c2_o2_textbox.Text <- (sprintf "%.3f" c2_o2)
        FormDesigner.add_textbox_float_validation(c2_o2_textbox, c2_o2_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c2_o2_textbox, c2_o2_label)

        let c3_o2_label = new Label()
        c3_o2_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c3_o2_label.AutoSize <- true
        c3_o2_label.Text <- "c3_O2 (Consumption of oxygen by one non-dividing live cell per time step)"
        FormDesigner.place_control_below(c3_o2_label, c2_o2_label)

        c3_o2_textbox.Text <- (sprintf "%.3f" c3_o2)
        FormDesigner.add_textbox_float_validation(c3_o2_textbox, c3_o2_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c3_o2_textbox, c3_o2_label)

        let o2_diffusion_coeff_label = new Label()
        o2_diffusion_coeff_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        o2_diffusion_coeff_label.AutoSize <- true
        o2_diffusion_coeff_label.Text <- "D_O2 (oxygen diffusion coefficient)"
        FormDesigner.place_control_below(o2_diffusion_coeff_label, c3_o2_label)

        o2_diffusion_coeff_textbox.Text <- (sprintf "%.1f" ModelParameters.OxygenDiffusionCoeff)
        FormDesigner.add_textbox_float_validation(o2_diffusion_coeff_textbox, o2_diffusion_coeff_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(o2_diffusion_coeff_textbox, o2_diffusion_coeff_label)


        // build glucose_func_label
        let glucose_func_label = new Label()
        glucose_func_label.Text <- "The concentration of glucose at point (x,y) is calculated as follows:\n\n\
                Glucose(x, y, t+dt) = Glucose(x, y, t) + dt * (D_Gl * nabla_squared(Glucose(x, y, t)) + \n     \
                    supply_rate - consumption_rate)\n\n\
                where nabla_squared(Glucose) (or Laplace operator) is the sum of second \n      \
                derivatives at point(x,y),\n\n\
                supply_rate = c1_Glucose if the point (x,y) is outside the tumour mass\n    \
                    and 0 otherwise\n\n\
                and consumption_rate = (c2_Glucose*dividing_cells + c3_Glucose*non-dividing_cells)\n    \
                    where dividing_cells and non_dividing_cells are the numbers of\n    \
                    dividing and non_dividing live cells in the grid mesh\n     \
                    embracing the point (x,y)\n"

        glucose_func_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (10, 13))
        glucose_func_label.AutoSize <- true
        (*glucose_func_label.Location <- FormDesigner.initial_location*)
        FormDesigner.place_control_below(glucose_func_label, o2_diffusion_coeff_label, 25)

        let (c1_glucose, c2_glucose, c3_glucose) = ModelParameters.GlucoseParam
        let c1_glucose_label = new Label()
        c1_glucose_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c1_glucose_label.AutoSize <- true
        c1_glucose_label.Text <- "c1_Glucose (Supply of glucose per time step)"
        FormDesigner.place_control_below(c1_glucose_label, glucose_func_label)

        c1_glucose_textbox.Text <- (sprintf "%.3f" c1_glucose)
        FormDesigner.add_textbox_float_validation(c1_glucose_textbox, c1_glucose_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c1_glucose_textbox, c1_glucose_label)

        let c2_glucose_label = new Label()
        c2_glucose_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5,2))
        c2_glucose_label.AutoSize <- true
        c2_glucose_label.Text <- "c2_Glucose (Consumption of glucose by one dividing cell per time step)"
        FormDesigner.place_control_below(c2_glucose_label, c1_glucose_label)

        c2_glucose_textbox.Text <- (sprintf "%.3f" c2_glucose)
        FormDesigner.add_textbox_float_validation(c2_glucose_textbox, c2_glucose_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c2_glucose_textbox, c2_glucose_label)

        let c3_glucose_label = new Label()
        c3_glucose_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        c3_glucose_label.AutoSize <- true
        c3_glucose_label.Text <- "c3_Glucose (Consumption of glucose by one non-dividing live cell per time step)"
        FormDesigner.place_control_below(c3_glucose_label, c2_glucose_label)

        c3_glucose_textbox.Text <- (sprintf "%.3f" c3_glucose)
        FormDesigner.add_textbox_float_validation(c3_glucose_textbox, c3_glucose_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(c3_glucose_textbox, c3_glucose_label)

        let glucose_diffusion_coeff_label = new Label()
        glucose_diffusion_coeff_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (5, 2))
        glucose_diffusion_coeff_label.AutoSize <- true
        glucose_diffusion_coeff_label.Text <- "D_Gl (glucose diffusion coefficient)"
        FormDesigner.place_control_below(glucose_diffusion_coeff_label, c3_glucose_label)

        glucose_diffusion_coeff_textbox.Text <- (sprintf "%.1f" ModelParameters.GlucoseDiffusionCoeff)
        FormDesigner.add_textbox_float_validation(glucose_diffusion_coeff_textbox, glucose_diffusion_coeff_label.Text, FloatInterval(0., Double.MaxValue))
        FormDesigner.place_control_totheright(glucose_diffusion_coeff_textbox, glucose_diffusion_coeff_label)


        // build nutrient_groupbox
        nutrient_groupbox.Controls.AddRange([| o2_func_label; c1_o2_label; c1_o2_textbox;
                                                c2_o2_label; c2_o2_textbox; c3_o2_label; c3_o2_textbox;
                                                o2_diffusion_coeff_label; o2_diffusion_coeff_textbox;
                                                glucose_func_label; c1_glucose_label; c1_glucose_textbox;
                                                c2_glucose_label; c2_glucose_textbox; c3_glucose_label; c3_glucose_textbox;
                                                glucose_diffusion_coeff_label; glucose_diffusion_coeff_textbox |])


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

        egf_textbox.Text <- (sprintf "%.2f" ModelParameters.EGFProb)
        FormDesigner.place_control_totheright(egf_textbox, egf_label)

        pathway_groupbox.Controls.AddRange([| egf_label; egf_textbox |])
        
        ParamFormBase.create_ok_cancel_buttons(this, pathway_groupbox, apply_changes) |> ignore

        base.Controls.AddRange([|nutrient_groupbox; pathway_groupbox|])