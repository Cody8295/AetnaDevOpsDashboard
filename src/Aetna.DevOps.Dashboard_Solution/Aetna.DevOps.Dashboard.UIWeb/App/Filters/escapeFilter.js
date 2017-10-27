(function () {

    var escapeFilter = function () {
        return function (input) {
            if (input) {
                return window.encodeURIComponent(input);
            }
            return "";
        }
    }

    var module = angular.module("app");
    module.filter("escape", escapeFilter);

}());