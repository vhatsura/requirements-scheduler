(function () {
    'use strict';

    angular
        .module('requirementsScheduler')
        .controller('reportsController', reportsController);

    reportsController.$inject = ['$scope', 'Reports'];

    function reportsController($scope, Reports) {
        $scope.reports = Reports.query();
    }
})();
