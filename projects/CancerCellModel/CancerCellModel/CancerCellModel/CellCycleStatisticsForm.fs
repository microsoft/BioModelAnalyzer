module CellCycleStatisticsForm

open System
open System.Windows.Forms
open Model
open ParamFormBase

type CellCycleStatisticsForm (stem_cell_cycle_statistics: CellCycleStatistics, nonstem_cell_cycle_statistics: CellCycleStatistics) = 
    inherit ParamFormBase(Visible = false, Width = 700, Height = 300)

    let stem_G0_numofcells_textbox = new TextBox()
    let stem_G0_percentofcells_textbox = new TextBox()
    let stem_G1_numofcells_textbox = new TextBox()
    let stem_G1_percentofcells_textbox = new TextBox()
    let stem_S_numofcells_textbox = new TextBox()
    let stem_S_percentofcells_textbox = new TextBox()
    let stem_G2M_numofcells_textbox = new TextBox()
    let stem_G2M_percentofcells_textbox = new TextBox()

    let nonstem_G0_numofcells_textbox = new TextBox()
    let nonstem_G0_percentofcells_textbox = new TextBox()
    let nonstem_G1_numofcells_textbox = new TextBox()
    let nonstem_G1_percentofcells_textbox = new TextBox()
    let nonstem_S_numofcells_textbox = new TextBox()
    let nonstem_S_percentofcells_textbox = new TextBox()
    let nonstem_G2M_numofcells_textbox = new TextBox()
    let nonstem_G2M_percentofcells_textbox = new TextBox()
    
    let update_data(G0_numofcells_textbox: TextBox, G0_percentofcells_textbox: TextBox, 
                        G1_numofcells_textbox: TextBox, G1_percentofcells_textbox: TextBox,
                        S_numofcells_textbox: TextBox, S_percentofcells_textbox: TextBox,
                        G2M_numofcells_texbox: TextBox, G2M_percentofcells_textbox: TextBox,
                        stat: CellDivisionStatistics) =
        G0_numofcells_textbox.Text <- (sprintf "%.1f" stat.NumOfCellsG0)
        G0_percentofcells_textbox.Text <- (sprintf "%d" stat.PercentOfCellsG0)    
        G1_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsG1)
        G1_percentofcells_textbox.Text <- (sprintf "%d" stat.PercentOfCellsG1)
        S_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsS)
        S_percentofcells_textbox.Text <- (sprintf "%d" stat.PercentOfCellsS)
        G2M_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsG2M)
        G2M_percentofcells_textbox.Text <- (sprintf "%d" stat.PercentOfCellsG2M)

    let create_cell_cycle_stat_controls(G0_numofcells_textbox: TextBox, G0_percentofcells_textbox: TextBox, 
                                            G1_numofcells_textbox: TextBox, G1_percentofcells_textbox: TextBox,
                                            S_numofcells_textbox: TextBox, S_percentofcells_textbox: TextBox,
                                            G2M_numofcells_texbox: TextBox, G2M_percentofcells_textbox: TextBox,
                                            parent: Control, stat: CellCycleStatistics) =
        
        let cell_cycle_stat_label = new Label()
        cell_cycle_stat_label.Text <- "Cell cycle statistics:"
        cell_cycle_stat_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (6, 1))
        cell_cycle_stat_label.AutoSize <- true
        cell_cycle_stat_label.Location <- FormDesigner.initial_location

        let G0_numofcells_label = new Label()
        G0_numofcells_label.Text <- "Number of cells in G0"
        G0_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G0_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(G0_numofcells_label, cell_cycle_stat_label)
        G0_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G0_numofcells_textbox, G0_numofcells_label)

        let G0_percentofcells_label = new Label()
        G0_percentofcells_label.Text <- "Percent of cells in G0"
        G0_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G0_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(G0_percentofcells_label, G0_numofcells_textbox)
        G0_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G0_percentofcells_textbox, G0_percentofcells_label)

        let G1_numofcells_label = new Label()
        G1_numofcells_label.Text <- "Number of cells in G1"
        G1_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G1_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(G1_numofcells_label, G0_numofcells_label)
        G1_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G1_numofcells_textbox, G1_numofcells_label)

        let G1_percentofcells_label = new Label()
        G1_percentofcells_label.Text <- "Percent of cells in G1"
        G1_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G1_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(G1_percentofcells_label, G1_numofcells_textbox)
        G1_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G1_percentofcells_textbox, G1_percentofcells_label)

        let S_numofcells_label = new Label()
        S_numofcells_label.Text <- "Number of cells in S"
        S_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        S_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(S_numofcells_label, G1_numofcells_label)
        S_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(S_numofcells_textbox, S_numofcells_label)

        let S_percentofcells_label = new Label()
        S_percentofcells_label.Text <- "Percent of cells in S"
        S_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        S_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(S_percentofcells_label, S_numofcells_textbox)
        S_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(S_percentofcells_textbox, S_percentofcells_label)

        let G2M_numofcells_label = new Label()
        G2M_numofcells_label.Text <- "Number of cells in G2M"
        G2M_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G2M_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(G2M_numofcells_label, S_numofcells_label)
        G2M_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G2M_numofcells_textbox, G2M_numofcells_label)

        let G2M_percentofcells_label = new Label()
        G2M_percentofcells_label.Text <- "Percent of cells in G2M"
        G2M_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (1, 1))
        G2M_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(G2M_percentofcells_label, G2M_numofcells_textbox)
        G2M_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(G2M_percentofcells_textbox, G2M_percentofcells_label)

        update_data(G0_numofcells_textbox, G0_percentofcells_textbox, G1_numofcells_textbox, G1_percentofcells_textbox, 
                        S_numofcells_textbox, S_percentofcells_textbox, G2M_numofcells_textbox, G2M_percentofcells_textbox, stat)
        parent.Controls.AddRange([|cell_cycle_stat_label;
                                    G0_numofcells_label; G0_numofcells_textbox; G0_percentofcells_label; G0_percentofcells_textbox;
                                    G1_numofcells_label; G1_numofcells_textbox; G1_percentofcells_label; G1_percentofcells_textbox;
                                    S_numofcells_label; S_numofcells_textbox; S_percentofcells_label; S_percentofcells_textbox;
                                    G2M_numofcells_label; G2M_numofcells_textboxl; G2M_percentofcells_label; G2M_percentofcells_textbox|])

    do 
        let stem_cell_cycle_groupbox = new GroupBox()
        stem_cell_cycle_groupbox.Text <- "Stem cells"
        stem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        stem_cell_cycle_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        stem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float stem_groupbox.Size.Width * 0.9),
                                                            int (float stem_groupbox.Size.Height * 0.9))

        create_cell_cycle_stat_controls(stem_G0_numofcells_textbox, stem_G0_percentofcells_textbox, stem_G1_numofcells_textbox, stem_G1_percentofcells_textbox,
                                        stem_S_numofcells_textbox, stem_S_percentofcells_textbox, stem_G2M_numofcells_textbox, stem_G2M_percentofcells_textbox, 
                                        stem_cell_cycle_groupbox, stem_cell_cycle_statistics)

        let nonstem_cell_cycle_groupbox = new GroupBox()
        nonstem_cell_cycle_groupbox.Text <- "Non-stem cells"
        nonstem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        FormDesigner.place_control_below(nonstem_cell_cycle_groupbox, stem_cell_cycle_groupbox)
        nonstem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float nonstem_groupbox.Size.Width * 0.9),
                                                                int (float nonstem_groupbox.Size.Height * 0.9))

        create_cell_cycle_stat_controls(nonstem_G0_numofcells_textbox, nonstem_G0_percentofcells_textbox, nonstem_G1_numofcells_textbox, nonstem_G1_percentofcells_textbox,
                                        nonstem_S_numofcells_textbox, nonstem_S_percentofcells_textbox, nonstem_G2M_numofcells_textbox, nonstem_G2M_percentofcells_textbox, 
                                        nonstem_cell_cycle_groupbox, nonstem_cell_cycle_statistics)

        base.Controls.AddRange([|stem__cell_cycle_groupbox; nonstem_cell_cycle_groupbox|])

    override this.Refresh() =
        update_data(stem_G0_numofcells_textbox, stem_G0_percentofcells_textbox,
                    stem_G1_numofcells_textbox, stem_G1_percentofcells_textbox,
                    stem_S_numofcells_textbox, stem_S_percentofcells_textbox,
                    stem_G2M_numofcells_textbox, stem_G2M_percentofcells_textbox,
                    stem_cell_cycle_statistics)

        update_data(nonstem_G0_numofcells_textbox, nonstem_G0_percentofcells_textbox,
                    nonstem_G1_numofcells_textbox, nonstem_G1_percentofcells_textbox,
                    nonstem_S_numofcells_textbox, nonstem_S_percentofcells_textbox,
                    nonstem_G2M_numofcells_textbox, nonstem_G2M_percentofcells_textbox,
                    nonstem_cell_cycle_statistics)