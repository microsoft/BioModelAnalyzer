module Gaussian

//source:http://fssnip.net/3g

open System

// Config for normalRand
// Number of samples to average [4 to 10?] (tails stretch/flatten out as this gets larger)
let nSamples = 10    

// Creating Random() w/o a seed uses the time of day so if many are created in
// rapid succession they will all use the same seed resulting in identical
// sequences.  seed() utilizes an MT safe single instance of Random() to seed the
// generators below to to avoid this problem.
let seed = 
    let seedGen = new Random()
    (fun () -> lock seedGen (fun () -> seedGen.Next()))

/// Creates an infinite sequence of gaussian distributed random #'s with mean and std 
/// MT safe, but returned sequence is not
/// possible range of (0,sigma) is +-sigma*sqrt(3*nSamples)
let normalRand mean sigma =
    // calc normalization factors up front & alloc a random()   
    let norm = sigma * sqrt (3*nSamples|>float)
    let shift = norm - mean
    let scale = 2.0 * norm / (float nSamples)
    let randGen = new Random(seed())
    
    // return a gaussian # by averaging another random seq (central limit theory)
    let rec gaussAvg n acc =
        if n > 0 then gaussAvg (n-1) (acc+randGen.NextDouble())
        else (acc * scale) - shift
        
    let rec gaussSeq() = seq { yield gaussAvg nSamples 0.0; yield! gaussSeq()}

    gaussSeq()


