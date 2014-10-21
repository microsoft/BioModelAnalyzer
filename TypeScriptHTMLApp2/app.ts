/// <reference path="Scripts/typings/angularjs/angular.d.ts" />

var app = angular.module('myApp', ['ngRoute'])
    .config(['$routeProvider', function ($routeProvider) {
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
    }])
    .controller('mainController', function ($scope) {
        $scope.message = "Main Content";
    });;

