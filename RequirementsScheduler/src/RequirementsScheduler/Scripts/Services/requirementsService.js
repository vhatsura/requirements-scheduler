(function () {
    'use strict';

    angular
        .module('requirementsServices', ['ngResource'])
        .factory('Report', Report);

    Report.$inject = ['$resource'];

    function Report($resource) {
        return $resource('/api/movies/:id');
    }
})();