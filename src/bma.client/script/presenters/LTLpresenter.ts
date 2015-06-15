module BMA {
    export module Presenters {
        export class LTLPresenter {

            keyframescompact: BMA.UIDrivers.IKeyframesList;

            constructor(keyframescompact: BMA.UIDrivers.IKeyframesList) {

                var that = this;
                window.Commands.On("AddKeyframe", function () {
                    keyframescompact.Add("New");
                });

                window.Commands.On("ChangedKeyframeName", function (item: { ind; name} ) {
                    alert('ind=' + item.ind + ' name=' + item.name);
                });

                window.Commands.On("KeyframeSelected", function (item: { ind }) {
                    alert('selected ind=' + item.ind);
                });
                
            }
        }
    }
} 