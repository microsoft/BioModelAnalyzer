module internal bma.Cloud.Trace

open System.Diagnostics

let logInfo s =
    Trace.TraceInformation(s)



