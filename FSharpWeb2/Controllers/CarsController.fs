namespace FSharpWeb2.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open FSharpWeb2.Models

/// Retrieves values.
type CarsController() =
    inherit ApiController()

    let values = [| { Make = "Ford"; Model = "Mustang" }; { Make = "Nissan"; Model = "Titan" } |]

    /// Gets all values.
    member x.Get()  = values

    member x.Get(id:int) = 
        Array.get values id |> Seq.singleton


    /// Post request x -> 2x
    member x.Post(z:int) = z * 2 
