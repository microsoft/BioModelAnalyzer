describe('proof analysis', function() {
  var driver = browser.driver;  
  var startAnalysis = function(){
    // Start proof analysis
    driver.findElement(by.id('icon1')).click();
  };
  var waitAnalysis = function (timeout) {
    driver.wait(function() {
        return driver.findElement(by.id('tabs-1')).isDisplayed();
    }, timeout);
  };
  var exists = function (locator) {
    driver.findElement(locator)
      .then(function(element) {
        // ok, found        
      }),
      function(err) {
        webdriver.promise.rejected(err);
      }    
  }

  it('stabilizes toymodel-stable', function() {
    driver.get('http://bmanew.cloudapp.net/tool.html?Model=preloaded/ToyModelStable.json');
    
    startAnalysis();
    waitAnalysis(5000);  
    exists(by.css('.stabilize-prooved'));
  });

    it("doesn't stabilize toymodel-unstable", function() {
    driver.get('http://bmanew.cloudapp.net/tool.html?Model=preloaded/ToyModelUnstable.json');
    
    startAnalysis();
    waitAnalysis(5000);  
    exists(by.css('.stabilize-failed'));
  });
});