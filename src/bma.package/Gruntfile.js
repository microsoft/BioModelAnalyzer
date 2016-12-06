/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {

    grunt.initConfig({
        concat: {
            tool: {
                options: {
                    separator: '\n'
                },
                src: [
                    "js/svgplot.js",
                    "js/scalablegridlinesplot.js",
                    "Scripts/formulaParser.js",
                    "Scripts/targetFuncParser.js",
                    "script/XmlModelParser.js" ,
                    "script/SVGHelper.js",
                    "script/LTLHelper.js",
                    "script/ModelHelper.js",
                    "script/commands.js",
                    "script/elementsregistry.js",
                    "script/functionsregistry.js",
                    "script/keyframesregistry.js",
                    "script/localRepository.js",
                    "script/changeschecker.js",
                    "script/onedrive/OneDriveRepository.js",
                    "script/onedrive/OneDrive.js",
                    "script/model/biomodel.js",
                    "script/model/model.js",
                    "script/model/analytics.js",  
                    "script/model/visualsettings.js",
                    "script/model/exportimport.js",
                    "script/model/operation.js",
                    "script/model/operationlayout.js",
                    "script/uidrivers/commondrivers.js",
                    "script/uidrivers/ltldrivers.js",
                    "script/presenters/undoredopresenter.js",
                    "script/presenters/presenters.js",
                    "script/presenters/proofpresenter.js",
                    "script/presenters/simulationpresenter.js",
                    "script/presenters/modelstoragepresenter.js",
                    "script/presenters/formulavalidationpresenter.js",
                    "script/presenters/furthertestingpresenter.js",
                    "script/presenters/localstoragepresenter.js",
                    "script/presenters/storagepresenter.js",
                    "script/presenters/onedrivestoragepresenter.js",
                    "script/UserLog.js",
                    "script/widgets/accordeon.js",
                    "script/widgets/bmaslider.js",
                    "script/widgets/coloredtableviewer.js",
                    "script/widgets/containernameeditor.js",
                    "script/widgets/drawingsurface.js",
                    "script/widgets/progressiontable.js",
                    "script/widgets/proofresultviewer.js",
                    "script/widgets/furthertestingviewer.js",
                    "script/widgets/localstoragewidget.js",
                    "script/widgets/modelstoragewidget.js",
                    "script/widgets/onedrivestoragewidget.js",
                    "script/widgets/resultswindowviewer.js",
                    "script/widgets/simulationplot.js",
                    "script/widgets/simulationexpanded.js",
                    "script/widgets/simulationviewer.js",
                    "script/widgets/userdialog.js",
                    "script/widgets/variablesOptionsEditor.js",
                    "script/widgets/visibilitysettings.js",
                    "script/widgets/formulaeditor.js",
                    "script/widgets/tftexteditor.js",
                    "script/widgets/ltl/keyframetable.js",
                    "script/widgets/ltl/keyframecompact.js",
                    "script/widgets/ltl/ltlstatesviewer.js",
                    "script/widgets/ltl/ltlviewer.js",
                    "script/widgets/ltl/ltlresultsviewer.js",
                    "script/widgets/ltl/stateseditor.js",
                    "script/widgets/ltl/statescompact.js",
                    "script/widgets/ltl/statetooltip.js",
                    "script/widgets/ltl/compactltlresult.js",
                    "script/widgets/ltl/tpeditor.js",
                    "script/widgets/ltl/tpviewer.js",
                    "script/presenters/ltl/LTLpresenter.js",
                    "script/presenters/ltl/states.js",
                    "script/presenters/ltl/temporalproperties.js",
                    "script/operatorsregistry.js",
                ],
                dest: 'tool.js',
                nonull: true
            },

        },
        uglify: {
            options: {
                sourceMap: true
            },
            dist: {
                files: {
                    'tool.min.js': ['tool.js']
                }
            }
        },
        copy: {
            main: {
                files: [
                    { src: 'tool.js', dest: '../bma.client/tool.js' },
                    { src: 'tool.min.js', dest: '../bma.client/tool.min.js' },
                    { src: 'app.js', dest: '../bma.client/app.js' },
                    { src: 'css/bma.css', dest: '../bma.client/css/bma.css' },
                    { src: 'script/widgets/codeeditor.js', dest: '../bma.client/codeeditor.js' },
                    { src: 'js/idd.js', dest: '../bma.client/js/idd.js' },
                    { src: 'js/idd.heatmapworker.js', dest: '../bma.client/js/idd.heatmapworker.js' },
                    { src: 'js/idd.heatmapworker.js', dest: '../bma.client/js/script/idd.heatmapworker.js' },
                    { src: '../../paket-files/carhartl/jquery-cookie/jquery.cookie.js', dest: 'js/jquery.cookie.js' },
                    { src: '../../paket-files/carhartl/jquery-cookie/jquery.cookie.js', dest: '../bma.client/js/jquery.cookie.js' },
                    { src: '../../paket-files/ChenWenBrian/FileSaver.js/FileSaver.js', dest: 'js/FileSaver.js' },
                    { src: '../../paket-files/ChenWenBrian/FileSaver.js/FileSaver.js', dest: '../bma.client/js/FileSaver.js' },
                    { src: '../../paket-files/furf/jquery-ui-touch-punch/jquery.ui.touch-punch.min.js', dest: 'js/jquery.ui.touch-punch.min.js' },
                    { src: '../../paket-files/furf/jquery-ui-touch-punch/jquery.ui.touch-punch.min.js', dest: '../bma.client/js/jquery.ui.touch-punch.min.js' },
                    { src: '../../paket-files/gabceb/jquery-browser-plugin/dist/jquery.browser.min.js', dest: 'js/jquery.browser.min.js' },
                    { src: '../../paket-files/gabceb/jquery-browser-plugin/dist/jquery.browser.min.js', dest: '../bma.client/js/jquery.browser.min.js' },
                    { src: '../../paket-files/jquery/jquery-mousewheel/jquery.mousewheel.js', dest: 'js/jquery.mousewheel.js' },
                    { src: '../../paket-files/jquery/jquery-mousewheel/jquery.mousewheel.js', dest: '../bma.client/js/jquery.mousewheel.js' },
                    { src: '../../paket-files/jquery/jquery-mousewheel/jquery.mousewheel.min.js', dest: 'js/jquery.mousewheel.min.js' },
                    { src: '../../paket-files/jquery/jquery-mousewheel/jquery.mousewheel.min.js', dest: '../bma.client/js/jquery.mousewheel.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svg.js', dest: 'js/jquery.svg.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svg.js', dest: '../bma.client/js/jquery.svg.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svg.min.js', dest: 'js/jquery.svg.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svg.min.js', dest: '../bma.client/js/jquery.svg.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svganim.js', dest: 'js/jquery.svganim.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svganim.js', dest: '../bma.client/js/jquery.svganim.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svganim.min.js', dest: 'js/jquery.svganim.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svganim.min.js', dest: '../bma.client/js/jquery.svganim.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svgdom.min.js', dest: 'js/jquery.svgdom.min.js' },
                    { src: '../../paket-files/kbwood/svg/jquery.svgdom.min.js', dest: '../bma.client/js/jquery.svgdom.min.js' },
                    { src: '../../paket-files/mar10/jquery-ui-contextmenu/jquery.ui-contextmenu.js', dest: 'js/jquery.ui-contextmenu.js' },
                    { src: '../../paket-files/mar10/jquery-ui-contextmenu/jquery.ui-contextmenu.js', dest: '../bma.client/js/jquery.ui-contextmenu.js' },
                    { src: '../../paket-files/mar10/jquery-ui-contextmenu/jquery.ui-contextmenu.min.js', dest: 'js/jquery.ui-contextmenu.min.js' },
                    { src: '../../paket-files/mar10/jquery-ui-contextmenu/jquery.ui-contextmenu.min.js', dest: '../bma.client/js/jquery.ui-contextmenu.min.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.js', dest: '../bma.client/js/monaco/base/common/worker/simpleWorker.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.nls.js', dest: '../bma.client/js/monaco/base/common/worker/simpleWorker.nls.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/worker/workerMain.js', dest: '../bma.client/js/monaco/base/worker/workerMain.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.js', dest: '../bma.client/js/monaco/editor/base/common/worker/simpleWorker.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.nls.js', dest: '../bma.client/js/monaco/editor/base/common/worker/simpleWorker.nls.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/worker/workerMain.js', dest: '../bma.client/js/monaco/editor/base/worker/workerMain.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.js', dest: '../bma.client/js/vs/base/common/worker/simpleWorker.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/common/worker/simpleWorker.nls.js', dest: '../bma.client/js/vs/base/common/worker/simpleWorker.nls.js' },
                    { src: 'node_modules/monaco-editor/min/vs/base/worker/workerMain.js', dest: '../bma.client/js/vs/base/worker/workerMain.js' },
                    { src: 'node_modules/monaco-editor/min/vs/editor/editor.main.js', dest: '../bma.client/js/monaco/editor/editor.main.js' },
                    { src: 'node_modules/monaco-editor/min/vs/editor/editor.main.nls.js', dest: '../bma.client/js/monaco/editor/editor.main.nls.js' },
                    { src: 'node_modules/monaco-editor/min/vs/editor/editor.main.css', dest: '../bma.client/js/monaco/editor/editor.main.css' },
                    { src: 'node_modules/monaco-editor/min/vs/loader.js', dest: '../bma.client/js/monaco/loader.js' },
                    { src: 'node_modules/monaco-editor/min/vs/loader.js', dest: '../bma.client/js/vs/loader.js' }
                ]
            },
        },
        less: {
            development: {
                files: {
                    "css/bma.css": "css/bma.less"
                }
            }
        }
        
        //jasmine: {
        //    options: {
        //        keepRunner: true,
        //        vendor: [
        //            "ext/jquery/dist/jquery.js",
        //            "ext/rxjs/dist/rx.lite.js",
        //            "<%= concat.dist.dest %>"
        //        ]
        //    },

        //    src: ['test/**/*.js']
        //},
        

    });

    grunt.loadNpmTasks('grunt-contrib-uglify');
    grunt.loadNpmTasks('grunt-contrib-concat');
    grunt.loadNpmTasks('grunt-contrib-jasmine');
    //grunt.loadNpmTasks('grunt-base64');
    grunt.loadNpmTasks('grunt-contrib-copy');
    //grunt.loadNpmTasks('grunt-bower-task');
    grunt.loadNpmTasks("grunt-contrib-less");

    grunt.registerTask('default', ['concat:tool', 'less:development', 'uglify:dist', 'copy:main']);
};
