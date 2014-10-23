/// <reference path="Scripts/typings/angularjs/angular.d.ts" />

var app = angular.module('myApp', ['ngRoute']);

interface Result {
    Status: boolean
    Log: string
}

// The Model part of MVC?
interface myAppScope extends ng.IScope {
    pgm : string
    debug_console : string
    run_foo(): void
    run_bar(): void
}

// We don't really need routing...
app.config(function($routeProvider) {
    $routeProvider
        .when('/', {
            templateUrl: 'views/home.html',
            controller: 'homeController'
        })
        .when('/about', {
            templateUrl: 'views/about.html',
            controller: 'aboutController'
        })
        .otherwise({
            redirectTo: '/'
        });
});

app.controller("mainController", ['$scope', '$http', ($scope: myAppScope, $http: ng.IHttpService) => {
    $scope.pgm = "test";
    $scope.debug_console = "dbg started";
    $scope.run_foo = () => {
        $scope.debug_console = "run_foo";
        $http.get("api/api/foo") // Don't know why this is api/api. 
            .error(x => {
                console.log('error');
                console.log("<=:" + x);
                $scope.debug_console = "error";
            })
            .success((r: Result) => {
                console.log('success');
                console.log("<=:" + r)
                $scope.debug_console = r.Status + ' ' + r.Log;
            })
    }

    $scope.run_bar = () => {
        $scope.debug_console = "run_bar";
        $http.get("api/api/bar")
            .error(x => {
                $scope.debug_console = "error";
            })
            .success((r: Result) => {
                console.log('success');
                console.log("<=:" + r)
                $scope.debug_console = r.Status + ' ' + r.Log;
            })
    };

}]);
