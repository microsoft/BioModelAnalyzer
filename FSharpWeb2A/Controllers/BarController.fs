namespace FSharpWeb2A.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open FSharpWeb2A.Models

type BarController() =
    inherit ApiController()

    // GET api/bar
    member x.Get() = { Status=true; Log="Bar"}

