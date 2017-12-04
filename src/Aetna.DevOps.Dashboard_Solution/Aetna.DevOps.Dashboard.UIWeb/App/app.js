(function () {

    var app = angular.module("app", ["ngRoute","ngCookies","angularMoment","chart.js"]);

    app.config(function ($routeProvider,$locationProvider) {
        $locationProvider.hashPrefix('');
        $routeProvider
            .when("/", {
                templateUrl: "App/Views/Home/index.html",
                controller: "homeIndexController",
                controller: "octopusController"
            })
            .when("/AboutUs/", {
                templateUrl: "App/Views/Home/aboutUs.html",
                controller: "homeAboutUsController"
            })
            .when("/ContactUs/", {
                templateUrl: "App/Views/Home/contactUs.html",
                controller: "homeContactUsController"
            })
            .otherwise({ redirectTo: "/" });
    });

    app.config(['$httpProvider', function ($httpProvider,$routeProvider) {
        //initialize get if not there
        if (!$httpProvider.defaults.headers.get) {
            $httpProvider.defaults.headers.get = {};
        }

        //disable IE ajax request caching
        $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
        // extra
        $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    }]);

    app.run(function ($rootScope, $location, restService) {
        $rootScope.copyrightDate = new Date();

        $rootScope.currentEnvironment = {};

        restService.getData('api/Metadata/Environment')
            .then(function (data) {
                if (data) {
                    $rootScope.currentEnvironment.name = data.environmentName;
                    $rootScope.currentEnvironment.showEnvironment = data.showEnvironment;
                    $rootScope.currentEnvironment.cssClass = data.cssClass;
                    $rootScope.currentEnvironment.octoHost = data.octoHost;
                    $rootScope.currentEnvironment.buildDate = data.buildDate;
                    $rootScope.currentEnvironment.version = data.version;
                } else {
                    $rootScope.currentEnvironment.name = "-?-";
                    $rootScope.currentEnvironment.showEnvironment = true;
                    $rootScope.currentEnvironment.cssClass = "label-danger";
                    $rootScope.currentEnvironment.octoHost = "N/A";
                    $rootScope.currentEnvironment.buildDate = null;
                    $rootScope.currentEnvironment.version = null;
                }
            }, function () {
                // Ignore
                $rootScope.currentEnvironment.name = "-?-";
                $rootScope.currentEnvironment.showEnvironment = false;
                $rootScope.currentEnvironment.cssClass = "label-danger";
                $rootScope.currentEnvironment.octoHost = "N/A";
                $rootScope.currentEnvironment.buildDate = null;
                $rootScope.currentEnvironment.version = null;
            });

        $rootScope.userLoading = true;
        restService.getData('api/Metadata/UserDetail')
            .then(function (data) {
                if (data) {
                    $rootScope.currentUserDetail = data;

                    $rootScope.userLoggedIn = true;
                    $rootScope.userName = data.firstName + " " + data.lastName + " (" + data.aetnaUserId + ")";
                    $rootScope.userId = data.aetnaUserId;
                    $rootScope.isAdmin = data.isAdmin;
                    $rootScope.domainGroups = data.domainGroups;
                } else {
                    $rootScope.currentUserDetail = null;

                    $rootScope.userLoggedIn = false;
                    $rootScope.userName = "Not Logged In";
                    $rootScope.userid = null;
                    $rootScope.isAdmin = false;
                    $rootScope.domainGroups = [];
                }
                $rootScope.userLoading = false;
            }, function () {
                $rootScope.userLoading = false;
            });
    });

}());