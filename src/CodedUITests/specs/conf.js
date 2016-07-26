exports.config = {
  // seleniumAddress: 'http://localhost:4444/wd/hub',
  specs: ['basic-spec.js'],
  multiCapabilities: [{
     'browserName': 'chrome'
   }, {
     'browserName': 'firefox'
   }],
  jasmineNodeOpts: {
      showColors: true,
      defaultTimeoutInterval: 30000,
      isVerbose:false,
      includeStackTrace:false
  }
};