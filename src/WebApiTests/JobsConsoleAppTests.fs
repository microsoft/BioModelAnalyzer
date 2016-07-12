module JobsConsoleAppTests

open NUnit.Framework
open FsCheck
open System.IO
open FSharp.Collections.ParallelSeq
open System.Diagnostics
open Newtonsoft.Json.Linq
open System
open System.Text
open LTLTests

let perform job = 
    let outputFile = Path.GetTempFileName()
    let path = Environment.CurrentDirectory
    use p = new Process()
    try
        p.StartInfo.UseShellExecute <- false
        p.StartInfo.FileName <- Path.Combine(path, "AnalyzeLTL.exe")
        p.StartInfo.Arguments <- sprintf @"""%s"" ""%s""" job outputFile
        p.StartInfo.CreateNoWindow <- true
        p.StartInfo.RedirectStandardOutput <- true
        p.StartInfo.RedirectStandardError <- true

        Trace.WriteLine(sprintf "Starting process '%s %s'..." p.StartInfo.FileName p.StartInfo.Arguments)
        p.Start() |> ignore       
        
        let errors = StringBuilder()
        p.ErrorDataReceived.Add(fun e -> errors.AppendLine e.Data |> ignore)
        p.BeginErrorReadLine()
        let output = p.StandardOutput.ReadToEnd()
        let errors = errors.ToString()

        p.WaitForExit() 

        Trace.WriteLine("Output: " + output)
        Trace.WriteLine("Errors: " + errors)

        match p.ExitCode with
        | 0 -> 
            let res = File.ReadAllText(outputFile)
            res
        | code -> failwithf "Process has exited with code %d; output: %s, errors: %s" code output errors
    finally
        File.Delete(outputFile)

[<Test; Timeout(600000)>]
let ``Console app checks LTL Polarity``() =
    checkJob perform