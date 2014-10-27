module BMA
{
    export function ParseXmlModel(xml: XMLDocument, grid: { xOrigin: number; yOrigin: number; xStep: number; yStep: number }): { Model: Model.BioModel; Layout: Model.Layout }
    {
        var $xml = $(xml);

        var $variables = $xml.children("Model").children("Variables").children("Variable");
        var modelVars = <Model.Variable[]>($variables.map((idx, elt) => {
            var $elt = $(elt);
            return new Model.Variable(
                parseInt($elt.attr("Id")),
                parseInt($elt.children("ContainerId").text()),
                $elt.children("Type").text(),
                $elt.attr("Name"),
                parseInt($elt.children("RangeFrom").text()),
                parseInt($elt.children("RangeTo").text()),
                $elt.children("Formula").text());
        }).get());

        var $relations = $xml.children("Model").children("Relationships").children("Relationship");
        var modelRels = <Model.Relationship[]>($relations.map((idx, elt) => {
            var $elt = $(elt);
            return new Model.Relationship(
                parseInt($elt.attr("Id")),
                parseInt($elt.children("FromVariableId").text()),
                parseInt($elt.children("ToVariableId").text()),
                $elt.children("Type").text());
        }).get());

        var $containers = $xml.children("Model").children("Containers").children("Container");
        var containers = <Model.ContainerLayout[]>($containers.map((idx, elt) => {
            var $elt = $(elt);
            return new Model.ContainerLayout(
                parseInt($elt.attr("Id")),
                parseInt($elt.children("Size").text()),
                parseInt($elt.children("PositionX").text()),
                parseInt($elt.children("PositionY").text()));
        }).get());

        var varLayouts = $variables.map((idx, elt) => {
            var $elt = $(elt);
            return new Model.VarialbeLayout(
                parseInt($elt.attr("Id")),
                parseInt($elt.children("CellX").text()) * grid.xStep + grid.xOrigin + parseFloat($elt.children("PositionX").text()) * grid.xStep / 300,
                parseInt($elt.children("CellY").text()) * grid.yStep + grid.yOrigin + parseFloat($elt.children("PositionY").text()) * grid.yStep / 350,
                Number.NaN,
                Number.NaN,
                parseFloat($elt.children("Angle").text()));
        }).get();

        return {
            Model: new Model.BioModel(
                $xml.children("Model").attr("Name"),
                modelVars,
                modelRels),
            Layout: new Model.Layout(
                containers,
                varLayouts)
        }
    }
} 