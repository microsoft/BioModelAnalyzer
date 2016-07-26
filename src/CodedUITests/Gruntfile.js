module.exports = function (grunt) {
    grunt.initConfig({
        protractor: {
            options: {
                // Location of your protractor config file
                configFile: "specs/conf.js",
            
                // Do you want the output to use fun colors?
                noColor: false,

                // Set to true if you would like to use the Protractor command line debugging tool
                // debug: true,
            
                // Additional arguments that are passed to the webdriver command
                args: { }
            },
            e2e: {
                options: {
                // Stops Grunt process if a test fails
                keepAlive: false
                }
            },
            continuous: {
                options: {
                keepAlive: true
                }
            }
        }});

    grunt.loadNpmTasks('grunt-protractor-runner');

    grunt.registerTask('default', ['protractor:e2e']);;
};