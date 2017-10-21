(function () {

    var app = angular.module("app");

    var homeIndexController = function ($scope, $http) {
        
        $http.get("/api/Octo/projectGroups").then(function (response) {
            $(".projectGroups").replaceWith("<span class=\"pull-right\">"+response.data+"</span>");
        });
        $http.get("/api/Octo/projects").then(function (response) {
            $(".projects").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/lifecycles").then(function (response) {
            $(".lifecycles").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        $http.get("/api/Octo/environments").then(function (response) {
            $(".environments").replaceWith("<span class=\"pull-right\">" + response.data + "</span>");
        });
        
    };

    app.controller("homeIndexController", homeIndexController);

}());