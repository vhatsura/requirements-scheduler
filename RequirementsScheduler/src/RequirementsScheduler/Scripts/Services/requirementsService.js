(function () {
    'use strict';

    var requirementsServices = angular.module('requirementsServices', ['ngResource']);

    requirementsServices.factory('Reports', ['$resource',
    function($resource) {
        return $resource('/api/reports/',
            {},
            {
                query: { method: 'GET', params: {}, isArray: true }
            });
    }]);
})();