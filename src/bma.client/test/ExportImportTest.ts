describe("model", () => {

    describe("export and import", () => {

        it("maps variable names correctly", () => {
            expect(BMA.Model.MapVariableNames("var(a)-var(b)", s => s.toUpper())).toBe("var(A)-var(B)");
        });

    })
});