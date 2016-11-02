(function () {
    'use strict';

    angular
        .module('requirementsScheduler')
        .controller('ReportsListController', ReportsListController)
        .controller('ReportsAddController', ReportsAddController)
        .controller('ReportsViewController', ReportsViewController);

    ReportsListController.$inject = ['$scope', 'Report'];

    function ReportsListController($scope, Report) {
        $scope.reports = Report.query();
    }

    ReportsAddController.$inject = ['$scope', '$location', 'Report'];

    function ReportsAddController($scope, $location, Report) {
        $scope.report = new Report();
        $scope.add = function() {
            $scope.report.$save(function() {
                $location.path('/');
            });
        };
    }

    ReportsViewController.$inject = ['$scope', '$routeParams', 'Report'];

    function ReportsViewController($scope, $routeParams, Report) {
        $scope.report = Report.get({ id: $routeParams.id });
    }
})();
