#light
namespace Functional

open System
open System.Threading.Tasks

module PSeq = 
  open System.Linq

  let inline ofSeq l = 
      ParallelEnumerable.AsParallel<_>(l:seq<_>)
  
  let inline map f pe =
      ParallelEnumerable.Select(pe, Func<_,_>(f))

  let inline map_concat f pe =
      ParallelEnumerable.SelectMany(pe, Func<_,_>(f))

  let inline filter f pe =
      ParallelEnumerable.Where(pe, Func<_,_>(f))

  let inline length l = 
      ParallelEnumerable.Count(l)
      
  let inline sum (l:ParallelQuery<(^a)>) = 
      let zero = LanguagePrimitives.GenericZero<(^a)>
      ParallelEnumerable.Aggregate(l, zero, fun a b -> a + b)

  let inline concat s1 s2 = 
      ParallelEnumerable.Concat(s1, s2)



[<AutoOpen>]
module ParallelAuto = 
  open PSeq

  let pfor nfrom nto f = 
      Parallel.For(nfrom, nto + 1, Action<_>(f)) |> ignore
  
  type ParallelSeqBuilder() =
    member x.For(p, f) = p |> ofSeq |> map_concat (fun n -> (f n) :> seq<_>)
    member x.Bind(p, f) = x.For(p, f)
    member x.Combine(s1, s2) = concat s1 s2
    member x.Delay(f) = f()
    member x.Zero() = Seq.empty |> ofSeq
    member x.Yield(n) = n |> Seq.singleton |> ofSeq
    
  let pseq = ParallelSeqBuilder()