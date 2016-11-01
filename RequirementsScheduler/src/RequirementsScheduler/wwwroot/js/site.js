// Write your Javascript code.
var requirementsScheduler = angular.module('requirementsScheduler', ["ui.router"]);

//delete in production
requirementsScheduler.run(["$rootScope"], function($rootScope) {
    $rootScope.$on('$stateChangeError',
        function(event, toState, toParams, fromState, fromParams, error) {
            console.log(event);
            console.log(toState);
            console.log(toParams);
            console.log(fromState);
            console.log(fromParams);
            console.log(error);
        });

    $rootScope.$on('$stateNotFound',
        function(event, unfoundState, fromState, fromParams) {
            console.log(event);
            console.log(unfoundState);
            console.log(fromState);
            console.log(fromParams);
        });
});
//end delete

requirementsScheduler.config(["$stateProvider", "$urlRouteProvider",
function($stateProvider, $urlRouteProvider) {
    $urlRouteProvider.otherwise("/home/overview");

    $stateProvider
    .state("home", { abstract: true, url: "/home", templateUrl: "/templates/home.html" })
    .state("overview",
        {
            parent: "home", url: "/overview", templateUrl: "/templates/overview.html"
        })
}])