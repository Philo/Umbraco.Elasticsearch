angular.module("umbraco.resources")
    .factory("searchResource", function($http) {
        return {
            getStats: function() {
                return $http.get("backoffice/searchSection/searchApi/stats");
            },
            rebuildContentIndex: function () {
                return $http.post("backoffice/searchSection/searchApi/rebuildcontentindex");
            },
            rebuildMediaIndex: function() {
                return $http.post("backoffice/searchSection/searchApi/rebuildmediaindex");
            },
            deleteIndex: function () {
                return $http.post("backoffice/searchSection/searchApi/deleteindex");
            },
            createIndex: function () {
                return $http.post("backoffice/searchSection/searchApi/createindex");
            }
        };
    });