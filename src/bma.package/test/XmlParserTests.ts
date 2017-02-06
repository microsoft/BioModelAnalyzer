// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe("XML parser", () => {

    var grid = {
        xOrigin: 100,
        yOrigin: 200,
        xStep: 200,
        yStep: 300
    };

    it("reads variables", (done) => {            
        $.get("data/2var_unstable.xml").then((data : string) => {            
            var r = BMA.ParseXmlModel($.parseXML(data), grid);
            expect(r.Model.Name).toBe("2var_unstable");
            expect(r.Model.Variables.length).toBe(2);
            var v1 = r.Model.Variables[1];
            expect(v1.Name).toBe("Y");
            expect(v1.Type).toBe("Default");
            expect(v1.RangeTo).toBe(1);
            expect(v1.RangeFrom).toBe(0);
            expect(v1.Formula).toBe("var(X)");
            done();
        });
    });

    it("reads relationships", (done) => {
        $.get("data/2var_unstable.xml").then((data: string) => {
            var r = BMA.ParseXmlModel($.parseXML(data), grid);
            expect(r.Model.Relationships.length).toBe(2);
            var rel = r.Model.Relationships[1];
            expect(rel.Id).toBe(0);
            expect(rel.FromVariableId).toBe(2);
            expect(rel.ToVariableId).toBe(1);
            expect(rel.Type).toBe("Activator");
            done();
        });
    });

    it("reads containers", (done) => {
        $.get("data/2var_unstable.xml").then((data: string) => {
            var r = BMA.ParseXmlModel($.parseXML(data), grid);
            expect(r.Layout.Containers.length).toBe(1);
            var c = r.Layout.Containers[0];
            expect(c.Id).toBe(1);
            expect(c.PositionX).toBe(2);
            expect(c.PositionY).toBe(1);
            expect(c.Size).toBe(1);
            done();
        });
    });

    it("reads variables layout", (done) => {
        $.get("data/2var_unstable.xml").then((data: string) => {
            var r = BMA.ParseXmlModel($.parseXML(data), grid);
            expect(r.Layout.Variables.length).toBe(2);
            var vl = r.Layout.Variables[1];
            expect(vl.Id).toBe(2);
            expect(vl.PositionX).toBe(625.2);
            expect(vl.PositionY).toBe(655);
            expect(vl.Angle).toBe(0);
            done();
        });
    });

})   


/*
 * <Container Id="1" Name="">
      <PositionX>2</PositionX>
      <PositionY>1</PositionY>
      <Size>1</Size>
    </Container>
 */

/*
      <PositionX>204</PositionX>
      <PositionY>182</PositionY>
      <CellX>2</CellX>
      <CellY>1</CellY>
      <Angle>0</Angle>
 */
