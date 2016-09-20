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
                    { src: 'js/jquery.ui-contextmenu.min.js', dest: '../bma.client/js/jquery.ui-contextmenu.min.js' },
                    { src: 'js/jquery.ui-contextmenu.js', dest: '../bma.client/js/jquery.ui-contextmenu.js' }
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
