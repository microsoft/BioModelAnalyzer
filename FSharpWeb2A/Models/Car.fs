namespace FSharpWeb2A.Models

open Newtonsoft.Json

//[<CLIMutable>]
type Car = {
    Make : string
    Model : string
}

type Results = {
    Status : bool;
    Log: string; 
    }