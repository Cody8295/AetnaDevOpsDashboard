"use strict";

(function () {

    var restService = function ($http, $rootScope) {
        /// <summary>AngularJS service to offer create, read, update, delete functionality over Web API.</summary>

        var getData = function (url) {
            /// <summary>Gets items via the Web API.</summary>

            console.log('GET from: ' + url);

            return $http.get(url).then(function (response) {
                return response.data;
            });
        };
        var postData = function (url, body) {
            /// <summary>Posts items via the Web API.</summary>

            console.log('POST to: ' + url + ' : ' + body);

            return $http.post(url, body).then(function (response) {
                return response.data;
            });
        };
        var putData = function (url, body) {
            /// <summary>Puts items via the Web API.</summary>

            console.log('PUT to: ' + url + ' : ' + body);

            return $http.put(url, body).then(function (response) {
                return response.data;
            });
        };
        var deleteData = function (url) {
            /// <summary>Deletes items via the Web API.</summary>

            console.log('DELETE to: ' + url);

            return $http.delete(url).then(function (response) {
                return response.data;
            });
        };

        return {
            getData: getData,
            postData: postData,
            putData: putData,
            deleteData: deleteData
        };
    };

    var module = angular.module("app");
    module.factory("restService", restService);

}());