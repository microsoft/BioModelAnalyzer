// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module PRNG

//collection of random number generators based on System.Random
//Each should be passed the seeded random number generator from main, RNG, so that it can be reproduced

//Uses 2 random numbers and returns a pair of redistributed random numbers in a list
let gaussianMargalisPolar rng mean sd =
    let rec gMP (rng: System.Random) =
        let a = rng.NextDouble() * 2.0 - 1.0
        let b = rng.NextDouble() * 2.0 - 1.0
        let c = a ** 2. + b ** 2.
        match c with
        | test when test < 1. -> (a , b , c )
        | _ -> gMP rng 
    let (a , b, c ) = gMP rng
    let modifier = (-2.0 * log c / c)**0.5
    (a * sd * modifier + mean, b * sd * modifier + mean)

let gaussianMargalisPolar' : System.Random -> float = 
    let next_one = ref None 
    (fun rng -> 
    match !next_one with 
    | None -> 
        let (a,b) = gaussianMargalisPolar rng 0. 1.
        next_one := Some b
        a
    | Some b ->
        next_one := None
        b 
        )
 
//let gaussianMargalisPolar' =
//    //let next_one = ref None
//    let mutable next_one = None
//    fun rng mean sd ->
//    match next_one with
//    | None -> 
//        let (a,b) = gaussianMargalisPolar rng mean sd
//        next_one <- Some b
//        a
//    | Some b ->
//        next_one <- None
//        b
//
//    (fun rng mean sd ->
//    let rec gMP (rng: System.Random) =
//        let a = rng.NextDouble() * 2.0 - 1.0
//        let b = rng.NextDouble() * 2.0 - 1.0
//        let c = a ** 2. + b ** 2.
//        match c with
//        | test when test < 1. -> [a ; b ; c ]
//        | _ -> gMP rng 
//    let [a ; b; c ] = gMP rng
//    let modifier = (-2.0 * log c / c)**0.5
//    [a * sd * modifier + mean; b * sd * modifier + mean]

let rec nGaussianRandomMP rng mean sd (number:int) = 
    [for i in [0..(number-1)] -> mean + sd * (gaussianMargalisPolar' rng)]
