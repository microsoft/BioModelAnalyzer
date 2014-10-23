/// <reference path="Scripts/typings/angularjs/angular.d.ts" />
var app = angular.module('myApp', ['ngRoute']);


// We don't really need routing...
app.config(function ($routeProvider) {
    $routeProvider.when('/', {
        templateUrl: 'views/home.html',
        controller: 'homeController'
    }).when('/about', {
        templateUrl: 'views/about.html',
        controller: 'aboutController'
    }).otherwise({
        redirectTo: '/'
    });
});

app.controller("mainController", [
    '$scope', '$http', function ($scope, $http) {
        $scope.pgm = "test";
        $scope.debug_console = "dbg started";
        $scope.run_foo = function () {
            $scope.debug_console = "run_foo";
            $http.get("api/api/foo").error(function (x) {
                console.log('error');
                console.log("<=:" + x);
                $scope.debug_console = "error";
            }).success(function (r) {
                console.log('success');
                console.log("<=:" + r);
                $scope.debug_console = r.Status + ' ' + r.Log;
            });
        };

        $scope.run_bar = function () {
            $scope.debug_console = "run_bar";
            $http.get("api/api/bar").error(function (x) {
                $scope.debug_console = "error";
            }).success(function (r) {
                console.log('success');
                console.log("<=:" + r);
                $scope.debug_console = r.Status + ' ' + r.Log;
            });
        };
    }]);
//# sourceMappingURL=app.js.map
