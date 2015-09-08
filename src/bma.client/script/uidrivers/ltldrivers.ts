/// <reference path="..\..\Scripts\typings\jquery\jquery.d.ts"/>
/// <reference path="..\..\Scripts\typings\jqueryui\jqueryui.d.ts"/>

module BMA {
    export module UIDrivers {

        export class KeyframesExpandedViewer implements IKeyframesFull {
            private keyframe: JQuery;

            constructor(keyframe: JQuery) {
                this.keyframe = keyframe;
            }

            public AddState(items) {
                this.keyframe.ltlstatesviewer('addState', items);
            }

            public GetContent() {
                return this.keyframe;
            }

            public RemovePart(p1, p2) {
                //this.keyframe.ltlstatesviewer('removePart', items);
            }
        }


        export class LTLViewer implements ILTLViewer, IKeyframesList {

            private ltlviewer: JQuery;

            constructor(ltlviewer: JQuery) {
                this.ltlviewer = ltlviewer;
            }

            public AddState(items) {
                var resdiv = this.ltlviewer.ltlviewer('Get', 'LTLStates');
                var content = resdiv.resultswindowviewer('option', 'content');
                content.keyframecompact('add', items);
            }

            public Show(tab) {
                if (tab !== undefined) {
                    var content: JQuery = this.ltlviewer.ltlviewer('Get', tab);
                    content.show();
                }
                else {
                    this.ltlviewer.ltlviewer('Show', undefined);
                }
            }

            public Hide(tab) {
                if (tab !== undefined) {
                    var content: JQuery = this.ltlviewer.ltlviewer('Get', tab);
                    content.hide();
                }
            }

            public SetResult(res) {
                var resdiv = this.ltlviewer.ltlviewer('Get', 'LTLResults');
                var content: JQuery = resdiv.resultswindowviewer('option', 'content');
                content.coloredtableviewer({ "colorData": res, type: "color" });
                content.find(".proof-propagation-overview").addClass("ltl-result-table");
                content.find('td.propagation-cell-green').removeClass("propagation-cell-green");
                content.find('td.propagation-cell-red').removeClass("propagation-cell-red").addClass("change");
            }

            GetContent() {
                return this.ltlviewer;
            }
        }

        export class TemporalPropertiesEditorDriver implements ITemporalPropertiesEditor {

            private tpeditor: JQuery;

            constructor(tpeditor: JQuery) {
                this.tpeditor = tpeditor;
            }

            public GetContent() {
                return this.tpeditor;
            }
        }
    }
}