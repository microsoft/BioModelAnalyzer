module BMA {
    export module Model {

        export function MapVariableNames(f: string, mapper: (string) => string[]) {
            var namestory = {};
            if (f !== undefined && f != null) {
                f = f.trim();
                // Convert default function to null
                if (f.toLowerCase() == "avg(pos)-avg(neg)")
                    return null;
                // Replace variable names with IDs
                var varPrefix = "var(";
                var startPos = 0;
                var index: number;
                while ((index = f.indexOf(varPrefix, startPos)) >= 0) {
                    var endIndex = f.indexOf(")", index);
                    if (endIndex < 0)
                        break;
                    var varName = f.substring(index + varPrefix.length, endIndex);
                    namestory[varName] = (namestory[varName] === undefined) ? 0 : namestory[varName] + 1;
                    var map = mapper(varName);
                    var m = (map instanceof Array) ? map[namestory[varName]] : map;
                    f = f.substring(0, index + varPrefix.length) + m + f.substr(endIndex);
                    startPos = index + 1;
                }
            }
            return f;
        }

        // Returns object whose JSON representation matches external format:
        // 1) Variables in formulas are identified by IDs
        // 2) Default function avg(pos)-avg(neg) is replaced with null formula
        export function ExportBioModel(model: BioModel) {

            function GetIdByName(id: number, name: string): string[] {
                var results = model.Variables.filter(function (v2: Variable) {
                    return v2.Name == name &&
                        model.Relationships.some(function (r: Relationship) {
                            return r.ToVariableId == id && r.FromVariableId == v2.Id;
                            // || r.FromVariableId == id && r.ToVariableId == v2.Id
                        });
                });
                //if (results.length > 1)
                //    throw new Error("Ambiguous variable name " + name + " in formula for variable id = " + id);
                //else if (results.length == 0)
                if (results.length == 0)
                    throw new Error("Unknown variable " + name + " in formula for variable id = " + id);
                var res = [];
                res = res.concat(results.map(x => x.Id.toString()));
                return res;
            }

            return {
                Name: model.Name,
                Variables: model.Variables.map(v => {
                    return {
                        Id: v.Id,
                        RangeFrom: v.RangeFrom,
                        RangeTo: v.RangeTo,
                        Formula: MapVariableNames(v.Formula, name => GetIdByName(v.Id, name))
                    }
                }),
                Relationships: model.Relationships.map(r => {
                    return {
                        Id: r.Id,
                        FromVariable: r.FromVariableId,
                        ToVariable: r.ToVariableId,
                        Type: r.Type
                    }
                })
            }
        }

        export function ExportModelAndLayout(model: BioModel, layout: Layout) {
            return {
                Model: ExportBioModel(model),
                Layout: {
                    Variables: layout.Variables.map(v => {
                        var mv = model.GetVariableById(v.Id);
                        return {
                            Id: v.Id,
                            Name: mv.Name,
                            Type: mv.Type,
                            ContainerId: mv.ContainerId,
                            PositionX: v.PositionX,
                            PositionY: v.PositionY,
                            CellX: v.CellX,
                            CellY: v.CellY,
                            Angle: v.Angle,
                        }
                    }),
                    Containers: layout.Containers.map(c => {
                        return {
                            Id: c.Id,
                            Name: c.Name,
                            Size: c.Size,
                            PositionX: c.PositionX,
                            PositionY: c.PositionY
                        }
                    })
                }
            }
        }

        export function ImportModelAndLayout(json: any) {
            var id = {};
            json.Layout.Variables.forEach(v => {
                id[v.Id] = v;
            });
            return {
                Model: new BioModel(json.Model.Name,
                    json.Model.Variables.map(v => new Variable(v.Id, id[v.Id].ContainerId, id[v.Id].Type, id[v.Id].Name, v.RangeFrom, v.RangeTo,
                        MapVariableNames(v.Formula, s => id[parseInt(s)].Name))),
                    json.Model.Relationships.map(r => new Relationship(r.Id, r.FromVariable, r.ToVariable, r.Type))),
                Layout: new Layout(json.Layout.Containers.map(c => new ContainerLayout(c.Id, c.Name, c.Size, c.PositionX, c.PositionY)),
                    json.Layout.Variables.map(v => new VariableLayout(v.Id, v.PositionX, v.PositionY, v.CellX, v.CellY, v.Angle)))
            }
        }
    }
} 