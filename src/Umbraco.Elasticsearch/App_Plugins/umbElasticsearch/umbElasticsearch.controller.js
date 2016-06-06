﻿function umbElasticsearchController($log, $scope, $timeout, notificationsService, searchResource) {
    $scope.deleteIndex = function (indexName) {
        $scope.busy = true;
        return searchResource.deleteIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.busy = false;
        });
    }

    $scope.buildIndex = function (indexName) {
        $scope.busy = true;
        return searchResource.buildIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.busy = false;
        });
    }

    $scope.activateIndex = function (indexName) {
        $scope.busy = true;
        return searchResource.activateIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.busy = false;
        });
    }

    $scope.getIndicesInfo = function () {
        $scope.busy = true;
        return searchResource.getIndicesInfo().then(function (data) {
            $scope.busy = false;
            $scope.info = data.data;
        });
    };

    $scope.rebuildContentIndex = function (indexName) {
        $scope.busy = true;
        notificationsService.success('Rebuilding Content Index', 'Content Index rebuild has started');
        searchResource.rebuildContentIndex(indexName).then(function () {
            $timeout(function () {
                $scope.busy = false;
                notificationsService.success("Content Index Rebuild", "Content Index rebuild completed");
                $scope.getIndicesInfo();
            }, 5000);
        }, function () {
            $scope.busy = false;
            notificationsService.error("Content Index Rebuild", "Content Index Rebuild Failed");
            $scope.getIndicesInfo();
        });
    }

    $scope.rebuildMediaIndex = function (indexName) {
        $scope.busy = true;
        notificationsService.success('Rebuilding Media Index', 'Media Index rebuild has started');
        searchResource.rebuildMediaIndex(indexName).then(function () {
            $timeout(function () {
                $scope.busy = false;
                notificationsService.success("Media Index Rebuild", "Index rebuild completed");
                $scope.getIndicesInfo();
            }, 5000);
        }, function () {
            $scope.busy = false;
            notificationsService.error("Media Index Rebuild", "Media Index Rebuild Failed");
            $scope.getIndicesInfo();
        });
    }

    $scope.refreshIndexLIst = function () {
        $scope.getIndicesInfo();
    }

    function addIndex() {
        $scope.busy = true;
        notificationsService.success('Creating Index', 'Index addition has started');
        searchResource.createIndex().then(function () {
            $scope.busy = false;
            notificationsService.success("Index Create", "Index was added");
            $scope.getIndicesInfo();
        }, function () {
            $scope.busy = false;
            notificationsService.error("Index Create", "Index create Failed");
            $scope.getIndicesInfo();
        });
    }

    $scope.getContentServicesList = function() {
        searchResource.getContentIndexServices().then(function(data) {
            $scope.contentServices = data.data;
        });
    }

    $scope.getMediaServicesList = function () {
        searchResource.getMediaIndexServices().then(function (data) {
            $scope.mediaServices = data.data;
        });
    }

    $scope.busy = false;
    $scope.addIndex = addIndex;
 
    function init() {
        $log.info('initialise');
        $scope.settings = searchResource.getSettings();
        $log.info('settings', $scope.settings);
        if ($scope.settings) {
            $scope.getContentServicesList();
            $scope.getMediaServicesList();
        }
    }

    init();
}

angular
    .module("umbraco")
    .controller("umbElasticsearchController", ["$log", "$scope", "$timeout", "notificationsService", "searchResource", umbElasticsearchController]);