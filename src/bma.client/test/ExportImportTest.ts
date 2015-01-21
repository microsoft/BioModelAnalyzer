describe("model transformation", () => {

    it("maps variable names correctly", () => {
        expect(BMA.Model.MapVariableNames("var(a)-var(b)", s => s.toUpperCase())).toBe("var(A)-var(B)");
    });
});