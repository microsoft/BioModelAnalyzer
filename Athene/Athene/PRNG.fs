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
        | test when test < 1. -> [a ; b ; c ]
        | _ -> gMP rng 
    let [a ; b; c ] = gMP rng
    let modifier = -2.0 * log c / c
    [a * sd * modifier + mean; b * sd * modifier + mean]

let rec nGaussianRandomMP rng mean sd (number:int) = 
    let results = List.reduce (fun acc item -> acc @ item) [ for i in [0..2..number] -> gaussianMargalisPolar rng mean sd ]
    match (number%2) with
    | 0 -> results
    | 1 -> results.Tail
    | _ -> failwith "Bad quantity of random numbers"