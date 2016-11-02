(function () {
    'use strict';

    config.$inject = ['$routeProvider', '$locationProvider'];

    angular.module('requirementsScheduler', [
        'ngRoute','requirementsServices'
    ]).config(config);

    function config($routeProvider, $locationProvider) {
        $routeProvider
            .when('/', {
                templateUrl: '/Views/list.html',
                controller: 'ReportsListController'
            })
            .when('/reports/add', {
                templateUrl: '/Views/add.html',
                controller: 'ReportsAddController'
            })
            .when('/reports/:id', {
                templateUrl: '/Views/report.html',
                controller: 'ReportsViewController'
            });

        $locationProvider.html5Mode(true);
    }
})();