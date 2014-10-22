/// <reference path="Scripts/typings/angularjs/angular.d.ts" />
var app = angular.module('myApp', ['ngRoute']);


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
        var counter = [0, 1, 2];
        $scope.pgm = "test";
        $scope.debug_console = "dbg started";
        $scope.run_cars = function () {
            $scope.debug_console = "run_cars";
            $http.get("api/api/cars").error(function (x) {
                console.log('error');
                console.log("<=:" + x);
                $scope.debug_console = "error";
            }).success(function (c) {
                console.log('success');
                console.log("<=:" + c);
                $scope.debug_console = c.Make + c.Model;
            });
        };

        $scope.run_foo = function () {
            $scope.debug_console = "run_foo";
            $http.get("api/api/foo").error(function (x) {
                $scope.debug_console = "error";
            }).success(function (c) {
                $scope.debug_console = c.Make + c.Model;
            });
        };
    }]);
//# sourceMappingURL=app.js.map
