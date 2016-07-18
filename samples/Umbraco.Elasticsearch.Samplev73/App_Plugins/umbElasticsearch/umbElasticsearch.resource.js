/*global Umbraco */
angular.module("umbraco.resources")
    .factory("umbElasticsearchResource", function ($http, umbRequestHelper) {
        var apiUrl = function (method) {
            return umbRequestHelper.getApiUrl("umbElasticsearchApiUrl", method);
        }

        return {
            getVersionNumber: function () {
                return $http.get(apiUrl("SearchVersionInfo")).then(function(data) {
                    return data.data.version;
                });
            },
            getPluginVersionInfo: function () {
                return $http.get(apiUrl("PluginVersionInfo")).then(function (data) {
                    return data.data;
                });
            },
            getIndicesInfo: function () {
                return umbRequestHelper.resourcePromise($http.get(apiUrl("IndicesInfo")));
            },
            getIndexInfo: function (indexName) {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("GetIndexInfo"), '"' + indexName + '"'));
            },
            rebuildContentIndex: function (indexName) {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("RebuildContentIndex"), '"' + indexName + '"'));
            },
            rebuildMediaIndex: function (indexName) {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("RebuildMediaIndex"), '"' + indexName + '"'));
            },
            createIndex: function () {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("CreateIndex")));
            },
            deleteIndexByName: function (indexName) {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("DeleteIndexByName"), '"' + indexName + '"'));
            },
            activateIndexByName: function (indexName) {
                return umbRequestHelper.resourcePromise($http.post(apiUrl("ActivateIndexByName"), '"' + indexName + '"'));
            },
            getContentIndexServices: function () {
                return umbRequestHelper.resourcePromise($http.get(apiUrl("ContentIndexServicesList")));
            },
            getMediaIndexServices: function () {
                return umbRequestHelper.resourcePromise($http.get(apiUrl("MediaIndexServicesList")));
            },
            getSettings: function () {
                return Umbraco.Sys.ServerVariables.umbracoPlugins.umbElasticsearch;
            },
            ping: function () {
                return $http.get(apiUrl("Ping")).then(function (response) {
                    return response.data.active !== null && response.data.active === true;
                });
            },
            isBusy: function () {
                return $http.get(apiUrl("IsBusy")).then(function (response) {
                    return response.data.busy !== null && response.data.busy === true;
                });
            }
        };
    });