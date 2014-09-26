module CellDivisionStatisticsForm

open System
open System.Windows.Forms
open Model
open ParamFormBase

type CellDivisionStatisticsForm (stemcell_division_statistics: CellDivisionStatistics,
                                 nonstemcell_division_statistics: CellDivisionStatistics) =

    inherit ParamFormBase (Visible = false, Width = 700, Height = 300)
    
    let stem_div_average_interval_textbox = new TextBox()
    let stem_div_min_interval_textbox = new TextBox()
    let stem_div_max_interval_textbox = new TextBox()

    let nonstem_div_average_interval_textbox = new TextBox()
    let nonstem_div_min_interval_textbox = new TextBox()
    let nonstem_div_max_interval_textbox = new TextBox()

    let update_data(average_textbox: TextBox, min_textbox: TextBox, max_textbox: TextBox, stat: CellDivisionStatistics) =
        average_textbox.Text <- (sprintf "%.1f" stat.AverageTimeBetweenDivisions)
        min_textbox.Text <- (sprintf "%d" stat.MinTimeBetweenDivisions)    
        max_textbox.Text <- (sprintf "%d" stat.MaxTimeBetweenDivisions)

    let create_division_stat_controls(average_textbox: TextBox, min_textbox: TextBox, max_textbox: TextBox,
                                        parent: Control, stat: CellDivisionStatistics) =
        let div_interval_label = new Label()
        div_interval_label.Text <- "Time between two consecutive divisions (in steps):"
        div_interval_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (6, 1))
        div_interval_label.AutoSize <- true
        div_interval_label.Location <- FormDesigner.initial_location

        let average_label = new Label()
        average_label.Text <- "Average"
        average_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        average_label.AutoSize <- true
        FormDesigner.place_control_below(average_label, div_interval_label)

        average_textbox.Enabled <- false
        FormDesigner.place_control_totheright(average_textbox, average_label)
        
        let min_label = new Label()
        min_label.Text <- "Minimum"
        min_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        min_label.AutoSize <- true
        FormDesigner.place_control_totheright(min_label, average_textbox)

        min_textbox.Enabled <- false
        FormDesigner.place_control_totheright(min_textbox, min_label)

        let max_label = new Label()
        max_label.Text <- "Maximum"
        max_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1.2, float 1))
        max_label.AutoSize <- true
        FormDesigner.place_control_totheright(max_label, min_textbox)
        
        max_textbox.Enabled <- false
        FormDesigner.place_control_totheright(max_textbox, max_label)

        update_data(average_textbox, min_textbox, max_textbox, stat)
        parent.Controls.AddRange([|div_interval_label;
                                            average_label; average_textbox;
                                            min_label; min_textbox;
                                            max_label; max_textbox|])

    do
        base.Text <- "Cell division statistics"

        let stem_groupbox = new GroupBox()
        stem_groupbox.Text <- "Stem cells"
        stem_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        stem_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        stem_groupbox.ClientSize <- Drawing.Size(int (float stem_groupbox.Size.Width * 0.9),
                                                        int (float stem_groupbox.Size.Height * 0.9))

        create_division_stat_controls(stem_div_average_interval_textbox,
                                        stem_div_min_interval_textbox, stem_div_max_interval_textbox,
                                        stem_groupbox, stemcell_division_statistics)

        let nonstem_groupbox = new GroupBox()
        nonstem_groupbox.Text <- "Non-stem cells"
        nonstem_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        FormDesigner.place_control_below(nonstem_groupbox, stem_groupbox)
        nonstem_groupbox.ClientSize <- Drawing.Size(int (float nonstem_groupbox.Size.Width * 0.9),
                                                        int (float nonstem_groupbox.Size.Height * 0.9))

        create_division_stat_controls(nonstem_div_average_interval_textbox,
                                        nonstem_div_min_interval_textbox, nonstem_div_max_interval_textbox,
                                        nonstem_groupbox, nonstemcell_division_statistics)

        base.Controls.AddRange([|stem_groupbox; nonstem_groupbox|])

    override this.Refresh() =
        update_data(stem_div_average_interval_textbox,
                stem_div_min_interval_textbox, stem_div_max_interval_textbox,
                stemcell_division_statistics)

        update_data(nonstem_div_average_interval_textbox,
                nonstem_div_min_interval_textbox, nonstem_div_max_interval_textbox,
                nonstemcell_division_statistics)
