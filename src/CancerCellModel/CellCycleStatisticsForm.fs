module CellCycleStatisticsForm

open System
open System.Windows.Forms
open Model
open ParamFormBase

type CellCycleStatisticsForm (stem_cell_cycle_statistics: CellCycleStatistics, nonstem_cell_cycle_statistics: CellCycleStatistics) = 
    
    inherit ParamFormBase (Visible = false, Width = 700, Height = 850)

    let stem_G0_numofcells_textbox = new TextBox()
    let stem_G0_percentofcells_textbox = new TextBox()
    let stem_G1_numofcells_textbox = new TextBox()
    let stem_G1_percentofcells_textbox = new TextBox()
    let stem_S_numofcells_textbox = new TextBox()
    let stem_S_percentofcells_textbox = new TextBox()
    let stem_G2M_numofcells_textbox = new TextBox()
    let stem_G2M_percentofcells_textbox = new TextBox()
    let stem_total_functioning_textbox = new TextBox()
    let stem_total_prenecrotic_textbox = new TextBox()
    let stem_total_apoptotic_textbox = new TextBox()
    let stem_total_necrotic_textbox = new TextBox()

    let nonstem_G0_numofcells_textbox = new TextBox()
    let nonstem_G0_percentofcells_textbox = new TextBox()
    let nonstem_G1_numofcells_textbox = new TextBox()
    let nonstem_G1_percentofcells_textbox = new TextBox()
    let nonstem_S_numofcells_textbox = new TextBox()
    let nonstem_S_percentofcells_textbox = new TextBox()
    let nonstem_G2M_numofcells_textbox = new TextBox()
    let nonstem_G2M_percentofcells_textbox = new TextBox()
    let nonstem_total_functioning_textbox = new TextBox()
    let nonstem_total_prenecrotic_textbox = new TextBox()
    let nonstem_total_apoptotic_textbox = new TextBox()
    let nonstem_total_necrotic_textbox = new TextBox()
    
    let update_data(g0_numofcells_textbox: TextBox, g0_percentofcells_textbox: TextBox, 
                        g1_numofcells_textbox: TextBox, g1_percentofcells_textbox: TextBox,
                        s_numofcells_textbox: TextBox, s_percentofcells_textbox: TextBox,
                        g2m_numofcells_textbox: TextBox, g2m_percentofcells_textbox: TextBox,
                        total_functioning_textbox: TextBox, total_prenecrotic_textbox: TextBox, total_apoptotic_textbox: TextBox, total_necrotic_textbox: TextBox,
                        stat: CellCycleStatistics) =
        g0_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsG0)
        g0_percentofcells_textbox.Text <- (sprintf "%.1f" stat.PercentOfCellsG0)    
        g1_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsG1)
        g1_percentofcells_textbox.Text <- (sprintf "%.1f" stat.PercentOfCellsG1)
        s_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsS)
        s_percentofcells_textbox.Text <- (sprintf "%.1f" stat.PercentOfCellsS)
        g2m_numofcells_textbox.Text <- (sprintf "%d" stat.NumOfCellsG2M)
        g2m_percentofcells_textbox.Text <- (sprintf "%.1f" stat.PercentOfCellsG2M)
        total_functioning_textbox.Text <- (sprintf "%d" stat.TotalFunctioningCells)
        total_prenecrotic_textbox.Text <- (sprintf "%d" stat.TotalPreNecroticCells)
        total_apoptotic_textbox.Text <- (sprintf "%d" stat.TotalApoptoticCells)
        total_necrotic_textbox.Text <- (sprintf "%d" stat.TotalNecroticCells)

    let create_cell_cycle_stat_controls(g0_numofcells_textbox: TextBox, g0_percentofcells_textbox: TextBox, 
                                            g1_numofcells_textbox: TextBox, g1_percentofcells_textbox: TextBox,
                                            s_numofcells_textbox: TextBox, s_percentofcells_textbox: TextBox,
                                            g2m_numofcells_textbox: TextBox, g2m_percentofcells_textbox: TextBox,
                                            total_functioning_textbox: TextBox, total_prenecrotic_textbox: TextBox, 
                                            total_apoptotic_textbox: TextBox, total_necrotic_textbox: TextBox, 
                                            parent: Control, stat: CellCycleStatistics) =
        
        let cell_cycle_stat_label = new Label()
        cell_cycle_stat_label.Text <- "Cell cycle statistics:"
        cell_cycle_stat_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (6, 1))
        cell_cycle_stat_label.AutoSize <- true
        cell_cycle_stat_label.Location <- FormDesigner.initial_location

        let g0_numofcells_label = new Label()
        g0_numofcells_label.Text <- "Number of cells in G0"
        g0_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(g0_numofcells_label, cell_cycle_stat_label, 15)
        g0_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g0_numofcells_textbox, g0_numofcells_label)

        let g0_percentofcells_label = new Label()
        g0_percentofcells_label.Text <- "Percent of cells in G0"
        g0_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g0_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(g0_percentofcells_label, g0_numofcells_textbox)
        g0_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g0_percentofcells_textbox, g0_percentofcells_label)

        let g1_numofcells_label = new Label()
        g1_numofcells_label.Text <- "Number of cells in G1"
        g1_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g1_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(g1_numofcells_label, g0_numofcells_label)
        g1_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g1_numofcells_textbox, g1_numofcells_label)

        let g1_percentofcells_label = new Label()
        g1_percentofcells_label.Text <- "Percent of cells in G1"
        g1_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g1_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(g1_percentofcells_label, g1_numofcells_textbox)
        g1_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g1_percentofcells_textbox, g1_percentofcells_label)

        let s_numofcells_label = new Label()
        s_numofcells_label.Text <- "Number of cells in S"
        s_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(s_numofcells_label, g1_numofcells_label)
        s_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(s_numofcells_textbox, s_numofcells_label)

        let s_percentofcells_label = new Label()
        s_percentofcells_label.Text <- "Percent of cells in S"
        s_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        s_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(s_percentofcells_label, s_numofcells_textbox)
        s_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(s_percentofcells_textbox, s_percentofcells_label)

        let g2m_numofcells_label = new Label()
        g2m_numofcells_label.Text <- "Number of cells in G2/M"
        g2m_numofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_numofcells_label.AutoSize <- true
        FormDesigner.place_control_below(g2m_numofcells_label, s_numofcells_label)
        g2m_numofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g2m_numofcells_textbox, g2m_numofcells_label)

        let g2m_percentofcells_label = new Label()
        g2m_percentofcells_label.Text <- "Percent of cells in G2/M"
        g2m_percentofcells_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        g2m_percentofcells_label.AutoSize <- true
        FormDesigner.place_control_totheright(g2m_percentofcells_label, g2m_numofcells_textbox)
        g2m_percentofcells_textbox.Enabled <- false
        FormDesigner.place_control_totheright(g2m_percentofcells_textbox, g2m_percentofcells_label)

        let total_functioning_label = new Label()
        total_functioning_label.Text <- "Total functioning cells"
        total_functioning_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        total_functioning_label.AutoSize <- true
        FormDesigner.place_control_below(total_functioning_label, g2m_numofcells_label, 30)
        total_functioning_textbox.Enabled <- false
        FormDesigner.place_control_totheright(total_functioning_textbox, total_functioning_label)

        let total_prenecrotic_label = new Label()
        total_prenecrotic_label.Text <- "Total prenecrotic cells"
        total_prenecrotic_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3,1))
        total_prenecrotic_label.AutoSize <- true
        FormDesigner.place_control_below(total_prenecrotic_label, total_functioning_label)
        total_prenecrotic_textbox.Enabled <- false
        FormDesigner.place_control_totheright(total_prenecrotic_textbox, total_prenecrotic_label)

        let total_necrotic_label = new Label()
        total_necrotic_label.Text <- "Total necrotic cells"
        total_necrotic_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        total_necrotic_label.AutoSize <- true
        FormDesigner.place_control_below(total_necrotic_label, total_prenecrotic_label)
        total_necrotic_textbox.Enabled <- false
        FormDesigner.place_control_totheright(total_necrotic_textbox, total_necrotic_label)

        let total_apoptotic_label = new Label()
        total_apoptotic_label.Text <- "Total apoptotic cells"
        total_apoptotic_label.MaximumSize <- FormDesigner.Scale(FormDesigner.label_size, (3, 1))
        total_apoptotic_label.AutoSize <- true
        FormDesigner.place_control_below(total_apoptotic_label, total_necrotic_label)
        total_apoptotic_textbox.Enabled <- false
        FormDesigner.place_control_totheright(total_apoptotic_textbox, total_apoptotic_label)

        update_data(g0_numofcells_textbox, g0_percentofcells_textbox, g1_numofcells_textbox, g1_percentofcells_textbox, 
                        s_numofcells_textbox, s_percentofcells_textbox, g2m_numofcells_textbox, g2m_percentofcells_textbox, 
                        total_functioning_textbox, total_prenecrotic_textbox, total_necrotic_textbox, total_apoptotic_textbox, stat)
        
        parent.Controls.AddRange([|cell_cycle_stat_label;
                                    g0_numofcells_label; g0_numofcells_textbox; g0_percentofcells_label; g0_percentofcells_textbox;
                                    g1_numofcells_label; g1_numofcells_textbox; g1_percentofcells_label; g1_percentofcells_textbox;
                                    s_numofcells_label; s_numofcells_textbox; s_percentofcells_label; s_percentofcells_textbox;
                                    g2m_numofcells_label; g2m_numofcells_textbox; g2m_percentofcells_label; g2m_percentofcells_textbox;
                                    total_functioning_label; total_functioning_textbox; total_prenecrotic_label; total_prenecrotic_textbox;
                                    total_necrotic_label; total_necrotic_textbox; total_apoptotic_label; total_apoptotic_textbox|])

    do 
        base.Text <- "Cell cycle statistics"

        let stem_cell_cycle_groupbox = new GroupBox()
        stem_cell_cycle_groupbox.Text <- "Stem cells"
        stem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        stem_cell_cycle_groupbox.Location <- Drawing.Point(FormDesigner.x_interval, FormDesigner.y_interval)
        stem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float stem_cell_cycle_groupbox.Size.Width * 0.9),
                                                            int (float stem_cell_cycle_groupbox.Size.Height * 0.9))

        create_cell_cycle_stat_controls(stem_G0_numofcells_textbox, stem_G0_percentofcells_textbox, stem_G1_numofcells_textbox, stem_G1_percentofcells_textbox,
                                        stem_S_numofcells_textbox, stem_S_percentofcells_textbox, stem_G2M_numofcells_textbox, stem_G2M_percentofcells_textbox, 
                                        stem_total_functioning_textbox, stem_total_prenecrotic_textbox, stem_total_necrotic_textbox, stem_total_apoptotic_textbox, 
                                        stem_cell_cycle_groupbox, stem_cell_cycle_statistics)

        let nonstem_cell_cycle_groupbox = new GroupBox()
        nonstem_cell_cycle_groupbox.Text <- "Non-stem cells"
        nonstem_cell_cycle_groupbox.Size <- Drawing.Size(base.ClientSize.Width - FormDesigner.x_interval, base.ClientSize.Height/2 - FormDesigner.y_interval)
        FormDesigner.place_control_below(nonstem_cell_cycle_groupbox, stem_cell_cycle_groupbox)
        nonstem_cell_cycle_groupbox.ClientSize <- Drawing.Size(int (float nonstem_cell_cycle_groupbox.Size.Width * 0.9),
                                                                int (float nonstem_cell_cycle_groupbox.Size.Height * 0.9))

        create_cell_cycle_stat_controls(nonstem_G0_numofcells_textbox, nonstem_G0_percentofcells_textbox, nonstem_G1_numofcells_textbox, nonstem_G1_percentofcells_textbox,
                                        nonstem_S_numofcells_textbox, nonstem_S_percentofcells_textbox, nonstem_G2M_numofcells_textbox, nonstem_G2M_percentofcells_textbox, 
                                        nonstem_total_functioning_textbox, nonstem_total_prenecrotic_textbox, nonstem_total_necrotic_textbox, nonstem_total_apoptotic_textbox,
                                        nonstem_cell_cycle_groupbox, nonstem_cell_cycle_statistics)

        base.Controls.AddRange([|stem_cell_cycle_groupbox; nonstem_cell_cycle_groupbox|])

    override this.Refresh() =
        update_data(stem_G0_numofcells_textbox, stem_G0_percentofcells_textbox,
                    stem_G1_numofcells_textbox, stem_G1_percentofcells_textbox,
                    stem_S_numofcells_textbox, stem_S_percentofcells_textbox,
                    stem_G2M_numofcells_textbox, stem_G2M_percentofcells_textbox,
                    stem_total_functioning_textbox, stem_total_prenecrotic_textbox, 
                    stem_total_necrotic_textbox, stem_total_apoptotic_textbox, stem_cell_cycle_statistics)

        update_data(nonstem_G0_numofcells_textbox, nonstem_G0_percentofcells_textbox,
                    nonstem_G1_numofcells_textbox, nonstem_G1_percentofcells_textbox,
                    nonstem_S_numofcells_textbox, nonstem_S_percentofcells_textbox,
                    nonstem_G2M_numofcells_textbox, nonstem_G2M_percentofcells_textbox,
                    nonstem_total_functioning_textbox, nonstem_total_prenecrotic_textbox, 
                    nonstem_total_necrotic_textbox, nonstem_total_apoptotic_textbox, nonstem_cell_cycle_statistics)