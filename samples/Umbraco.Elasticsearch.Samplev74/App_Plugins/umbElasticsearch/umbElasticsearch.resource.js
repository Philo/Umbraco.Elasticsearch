angular.module("umbraco.resources")
    .factory("umbElasticsearchResource", function ($http, umbRequestHelper) {
        var apiUrl = function (method) {
            return umbRequestHelper.getApiUrl("umbElasticsearchApiUrl", method);
        }

        return {
            getVersionNumber: function () {
                return $http.get(apiUrl("SearchVersionInfo")).then(function (data) {
                    console.log(data.data.version);
                    return data.data.version;
                });
            },
            getIndicesInfo: function () {
                return $http.get(apiUrl("IndicesInfo"));
            },
            rebuildContentIndex: function (indexName) {
                return $http.post(apiUrl("RebuildContentIndex"), '"' + indexName + '"');
            },
            rebuildMediaIndex: function (indexName) {
                return $http.post(apiUrl("RebuildMediaIndex"), '"' + indexName + '"');
            },
            createIndex: function () {
                return $http.post(apiUrl("CreateIndex"));
            },
            deleteIndexByName: function (indexName) {
                return $http.post(apiUrl("DeleteIndexByName"), '"' + indexName + '"');
            },
            activateIndexByName: function (indexName) {
                return $http.post(apiUrl("ActivateIndexByName"), '"' + indexName + '"');
            },
            getContentIndexServices: function () {
                return $http.get(apiUrl("ContentIndexServicesList"));
            },
            getMediaIndexServices: function () {
                return $http.get(apiUrl("MediaIndexServicesList"));
            },
            getSettings: function () {
                return Umbraco.Sys.ServerVariables.umbracoPlugins.umbElasticsearch;
            },
            ping: function () {
                return $http.get(apiUrl("Ping")).then(function (response) {
                    console.log(response);
                    return response.data.active !== null && response.data.active === true;
                });
            }
        };
    });