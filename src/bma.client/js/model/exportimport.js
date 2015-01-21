var BMA;
(function (BMA) {
    (function (Model) {
        function MapVariableNames(f, mapper) {
            if (f != null) {
                f = f.trim();

                // Convert default function to null
                if (f.toLowerCase() == "avg(pos)-avg(neg)")
                    return null;

                // Replace variable names with IDs
                var varPrefix = "var(";
                var startPos = 0;
                var index;
                while ((index = f.indexOf(varPrefix, startPos)) >= 0) {
                    var endIndex = f.indexOf(")", index);
                    if (endIndex < 0)
                        break;
                    var varName = f.substring(index + varPrefix.length, endIndex);
                    f = f.substring(0, index + varPrefix.length) + mapper(varName) + f.substr(endIndex);
                    startPos = index + 1;
                }
            }
            return f;
        }
        Model.MapVariableNames = MapVariableNames;

        // Returns object whose JSON representation matches external format:
        // 1) Variables in formulas are identified by IDs
        // 2) Default function avg(pos)-avg(neg) is replaced with null formula
        function ExportBioModel(model) {
            function GetIdByName(id, name) {
                var results = model.Variables.filter(function (v2) {
                    return v2.Name == name && model.Relationships.some(function (r) {
                        return r.FromVariableId == id && r.ToVariableId == v2.Id || r.ToVariableId == id && r.FromVariableId == v2.Id;
                    });
                });
                if (results.length > 1)
                    throw new Error("Ambiguous variable name " + name + " in formula for variable id = " + id);
                else if (results.length == 0)
                    throw new Error("Unknown variable " + name + " in formula for variable id = " + id);
                return results[0].Id;
            }

            return {
                ModelName: model.Name,
                Variables: model.Variables.map(function (v) {
                    return {
                        Id: v.Id,
                        RangeFrom: v.RangeFrom,
                        RangeTo: v.RangeTo,
                        Formula: MapVariableNames(v.Formula, function (name) {
                            return GetIdByName(v.Id, name).toString();
                        })
                    };
                }),
                Relationships: model.Relationships.map(function (r) {
                    return {
                        Id: r.Id,
                        FromVariable: r.FromVariableId,
                        ToVariable: r.ToVariableId,
                        Type: r.Type
                    };
                })
            };
        }
        Model.ExportBioModel = ExportBioModel;

        function ExportModelAndLayout(model, layout) {
            return {
                Model: ExportBioModel(model),
                Layout: {
                    Variables: layout.Variables.map(function (v) {
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
                            Angle: v.Angle
                        };
                    }),
                    Containers: layout.Containers.map(function (c) {
                        return {
                            Id: c.Id,
                            Name: c.Name,
                            Size: c.Size,
                            PositionX: c.PositionX,
                            PositionY: c.PositionY
                        };
                    })
                }
            };
        }
        Model.ExportModelAndLayout = ExportModelAndLayout;

        function ImportModelAndLayout(json) {
            var id = {};
            json.Layout.Variables.forEach(function (v) {
                id[v.Id] = v;
            });
            return {
                Model: new BioModel(json.Model.Name, json.Model.Variables.map(function (v) {
                    return new Variable(v.Id, id[v.Id].ContainerId, id[v.Id].Name, id[v.Id].Type, v.RangeFrom, v.RangeTo, MapVariableNames(v.Formula, function (s) {
                        return id[parseInt(s)].Name;
                    }));
                }), json.Model.Relationships.map(function (r) {
                    return new Relationship(r.Id, r.FromVariable, r.ToVariable, r.Type);
                })),
                Layout: new Layout(json.Layout.Containers.map(function (c) {
                    return new ContainerLayout(c.Id, c.Name, c.Size, c.PositionX, c.PositionY);
                }), json.Layout.Variables.map(function (v) {
                    return new VariableLayout(v.Id, v.PositionX, v.PositionY, v.CellX, v.CellY, v.Angle);
                }))
            };
        }
        Model.ImportModelAndLayout = ImportModelAndLayout;
    })(BMA.Model || (BMA.Model = {}));
    var Model = BMA.Model;
})(BMA || (BMA = {}));
