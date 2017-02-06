// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
it("creates BMA.Model.ProofResult", () => {
    var isStable = true;
    var time = 15;
    var ticks = ["one", 4, { ten: 10 }];
    var proof = new BMA.Model.ProofResult(isStable, time, ticks);

    expect(proof.IsStable).toEqual(isStable);
    expect(proof.Time).toEqual(time);
    expect(proof.Ticks).toEqual(ticks);
}) 
