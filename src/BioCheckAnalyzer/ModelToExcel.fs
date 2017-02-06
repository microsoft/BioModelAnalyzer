// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module ModelToExcel

//
// Convert QN to Excel. 
// Originally coded by Andrew Fernandes (Saffron Walden School). 
//
(*
This is the layout of the spreadsheet. The static data (var id, range) 
of the model is in the range (id,0):(Function,9). The initial values 
are down the t0 column, (t0,0) : (t0,9). 

id,Name,RangeFr,RangeTo,Function, t,    t',   t'', ..., t''''
1  x1   n1      m1      f1        0     U1'() 
..
9  x9   n9      m9      f9        0     U9'()

T1                                T1()  T1'()
...
T9                                T9()  T9'() 

The dynamic part is the Tx() (target) and Ux() (update) functions. The Tx 
ones read current state from the data column above them, to calculate the 
next target value. The Ux' ones read the previous state (the previous 
column in fact), and the target Tx values to work out the next state value. 

Bugs:
1. Conditional Formatting is slow (turned off for now).
2. Calling RANDBETWEEN to get t0 data is slow. (Causes recalculation of 
entire model?)
3. saveSpreadsheet writes the ss out to C:\Documents. Why?!
*)
open System
open System.IO
open System.Reflection
open Microsoft.Office.Interop.Excel
open Expr


/// meta information about field names 
// Using these let's us avoid magic numbers (2,6,...) in the code. 
let fld_ID   = "ID"
let fld_Name = "Name"
let fld_RangeFrom    = "RangeFrom"
let fld_RangeTo      = "RangeTo"
let fld_Function     = "Function"
let fld_InitialValue = "InitialValue"

/// Convert 1 to 'A', 2 to 'B', etc. 
// http://stackoverflow.com/questions/181596/how-to-convert-a-column-number-eg-127-into-an-excel-column-eg-aa
let col_name (c:int) = 
    assert (c >= 1)
    let dividend = ref c
    let columnName = ref ""
    while (!dividend > 0) do
        let modulo = (!dividend - 1) % 26
        columnName := (string(char (65 + modulo))) + !columnName
        dividend := (!dividend - modulo) / 26
    done    
    !columnName

/// Translate Expr.expr to Excel formula. Current var value at state_col. 
let excel_of_expr var_to_row (state_col : int) e  =
    let rec loop e = 
        match e with
        | Var(v) -> (col_name state_col) + (string)(Map.find v var_to_row)
        | Const(i) -> (string)i
        | Plus(e,f) -> "(" + loop e  + "+" + loop f  + ")"
        | Minus(e,f) -> "(" + loop e  + "-" + loop f  + ")"
        | Times(e,f) -> "(" + loop  e  +  "*" + loop  f  +   ")"
        | Div(e,f) -> "ceiling(" + loop  e  + "/" + loop  f  + ",1)"
        | Max(e,f) -> "max(" + loop  e  + "," + loop  f  + ")"
        | Min(e,f) -> "min(" + loop  e  +  "," + loop  f  + ")"
        | Ceil(e) -> "ceiling(" + loop  e  + ",1)"
        | Abs(e) -> "abs(" + loop e + ")"
        | Floor(e) -> "floor(" + loop  e  + ",1)"
        | Ave(ee) when List.length(ee) = 0 -> "0"
        | Ave(ee) ->  "ceiling(average(" + (String.concat "," (List.map (fun e -> loop  e ) ee)) + "),1)"
        | Sum(ee) ->  "ceiling(sum(" + (String.concat "," (List.map (fun e -> loop  e ) ee)) + "),1)"

    loop e 

/// [setCellText sheet x y txt] sets sheet's col x and row y cell to txt
// col and row are 1-based, like normal spreadsheets. 
let setCellText (sheet:_Worksheet) (x : int) (y : int) (text) =
    assert (x >= 1 && y >= 1)
    let range = sprintf "%s%d" (col_name x) y
    sheet.Range(range).Value(Missing.Value) <- text

/// Write all the data, functions for v. 
let printVariableToExcel sheet max_time headings_to_col var_to_row num_of_vars (v:QN.node)  = 
    let (var, name, rangeFrom, rangeTo, f) = (v.var, v.name, (fst v.range), (snd v.range), v.f)
    let rowIdx = Map.find var var_to_row 
    
    setCellText sheet (Map.find fld_ID headings_to_col) rowIdx var
    setCellText sheet (Map.find fld_Name headings_to_col) rowIdx name
    setCellText sheet (Map.find fld_RangeFrom headings_to_col) rowIdx rangeFrom
    setCellText sheet (Map.find fld_RangeTo headings_to_col) rowIdx rangeTo
    setCellText sheet (Map.find fld_Function headings_to_col) rowIdx (Expr.str_of_expr f) // for documentation only 
    setCellText sheet (Map.find fld_InitialValue headings_to_col) rowIdx rangeFrom // Over-written by randomize later.
    
    // Set up target functions
    setCellText sheet 2 (rowIdx + num_of_vars) (sprintf "target%d" var)
    
    // the Function column (we'll start populating the data after this). 
    let base_col = Map.find fld_Function headings_to_col 

    for i = 1 to max_time do
        // T_v^i, value of target_v at time i 
        let t_v_i = "=" + excel_of_expr var_to_row (base_col+i) f 
        setCellText sheet (base_col+i) (rowIdx + num_of_vars) t_v_i
        // Write update functions
        if i > 1 then  // Ignore t0, the initial values.
            let prev_col = base_col + i - 1 // previous column
            let current_state = sprintf "%s%d" (col_name prev_col) rowIdx                   // previous column, same row 
            let t_current_state = sprintf "%s%d" (col_name prev_col) (rowIdx + num_of_vars) // previous column, target row           
            let range_from = // same row, RangeFrom column            
                let range_from_col = col_name (Map.find fld_RangeFrom headings_to_col)
                sprintf "%s%d" ("$"+range_from_col) rowIdx 
            let range_to =   // same row, RangeTo column
                let range_to_col = col_name (Map.find fld_RangeTo headings_to_col)
                sprintf "%s%d" ("$"+range_to_col) rowIdx     
            // IF statement copied from example spreadsheet for structure: 
            //"=IF(AND(F37>F2,F2<Table1[@RangeTo]),F2+1,IF(AND(F37<F2,F2>Table1[@RangeFrom]),F2-1,F2))" 
            let update_fun = 
                sprintf "=IF(AND(%s>%s,%s<%s),%s+1,IF(AND(%s<%s,%s>%s),%s-1,%s))" 
                    t_current_state current_state   current_state range_to    current_state 
                    t_current_state current_state   current_state range_from  current_state 
                    current_state
            setCellText sheet (base_col + i) rowIdx update_fun

let randomize_t0_value sheet headings_to_col var_to_row (v:QN.node)  = 
    let (var, rangeFrom, rangeTo) = (v.var, (fst v.range), (snd v.range))
    let row = Map.find var var_to_row 
    let col = Map.find "InitialValue" headings_to_col
    let ran = sprintf "=RANDBETWEEN(%d,%d)" rangeFrom rangeTo
    setCellText sheet col row ran

/// Set conditional formatting for cells that contain the actual data (SLOW=?)
//let colour_the_changes (sheet:_Worksheet) top_left bottom_right = 
//        let (x0,y0), (xn,yn) = top_left, bottom_right
//        let range = sprintf "%s%d:%s%d" (col_name x0) y0 (col_name xn) yn
//        let format = sheet.Range(range)
//                          .FormatConditions
//                          .Add(XlFormatConditionType.xlCellValue, XlFormatConditionOperator.xlNotEqual, (sprintf "=%s" cellBefore)) :?> FormatCondition
//        format.Interior.Color <- XlRgbColor.rgbYellow 
            
/// Convert qn to excel
// The spreadsheet starts from row 1, col 1. (Not 0-based.)
let excel_of_qn sheet qn max_time =    
    // Place the headings in row 1    
    let headings = [fld_ID; fld_Name; fld_RangeFrom; fld_RangeTo; fld_Function; fld_InitialValue]
    let headings_to_col = Map.ofList (List.zip headings [1 .. (List.length headings)])
    Map.iter (fun fld col -> setCellText sheet col 1 fld) headings_to_col
    let headings_range = "A1:" + (col_name (List.length headings)) + "1"
    sheet.Range(headings_range).Font.Bold <- true
    sheet.Range(headings_range).HorizontalAlignment <- XlHAlign.xlHAlignCenter

    let num_of_vars = List.length qn 
    let var_to_row = Map.ofList (List.zip (List.map (fun (v:QN.node) -> v.var) qn) [2 .. (num_of_vars+1)])
    List.iter (fun var -> printVariableToExcel sheet max_time headings_to_col var_to_row num_of_vars var) qn
    
    List.iter (fun var -> randomize_t0_value sheet headings_to_col var_to_row var) qn 
    sheet 

let saveSpreadsheet (app:ApplicationClass) (sheet : _Worksheet) filename =
    sheet.SaveAs filename // SI: look in Documents!
    app.Quit()
 
// SI: we don't used init_values yet
let model_to_excel qn max_time init_values = 
    let app = ApplicationClass(Visible = false)
    let sheet = app.Workbooks.Add().Worksheets.[1] :?> _Worksheet
    let sheet = excel_of_qn sheet qn max_time 
    (app, sheet)
