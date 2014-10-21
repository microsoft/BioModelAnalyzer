﻿namespace FSharpWeb2A.Controllers
open System
open System.Collections.Generic
open System.Linq
open System.Net.Http
open System.Web.Http
open FSharpWeb2A.Models

/// Retrieves values.
type FooController() =
    inherit ApiController()

    // api/foo
    member x.Get() = 42

    // api/foo/y
    member x.Get(y:int) = y*2