// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
describe('proof analysis', function() {
  
  var startAnalysis = function(){
    // Start proof analysis
    browser.driver.findElement(by.id('icon1')).click();
  };
  var waitAnalysis = function (timeout) {
    browser.driver.wait(function() {
        return browser.driver.findElement(by.id('tabs-1')).isDisplayed();
    }, timeout);
  };
  var exists = function (locator) {
    browser.driver.findElement(locator)
      .then(function(element) {
        // ok, found        
      }),
      function(err) {
        browser.driver.promise.rejected(err);
      }    
  }

  it('stabilizes toymodel-stable', function() {
    browser.driver.get('http://bmanew.cloudapp.net/tool.html?Model=preloaded/ToyModelStable.json');
    
    startAnalysis();
    waitAnalysis(5000);  
    exists(by.css('.stabilize-prooved'));
  });

  it("doesn't stabilize toymodel-unstable", function() {
    browser.driver.get('http://bmanew.cloudapp.net/tool.html?Model=preloaded/ToyModelUnstable.json');
    
    startAnalysis();
    waitAnalysis(5000);  
    exists(by.css('.stabilize-failed'));
  });
});
