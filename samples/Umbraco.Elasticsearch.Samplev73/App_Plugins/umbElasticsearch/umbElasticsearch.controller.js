function umbElasticsearchController($log, $scope, $timeout, notificationsService, umbElasticsearchResource) {

    $scope.getContentServicesList = function () {
        umbElasticsearchResource.getContentIndexServices().then(function (data) {
            $scope.contentServices = data.data;
        });
    };

    $scope.getMediaServicesList = function () {
        umbElasticsearchResource.getMediaIndexServices().then(function (data) {
            $scope.mediaServices = data.data;
        });
    };

    $scope.deleteIndex = function (indexName) {
        $scope.busy = true;
        return umbElasticsearchResource.deleteIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.busy = false;
        });
    };

    $scope.buildIndex = function (indexName) {
        $scope.busy = true;
        return umbElasticsearchResource.buildIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.busy = false;
        });
    };

    $scope.activateIndex = function (indexName) {
        $scope.busy = true;
        return umbElasticsearchResource.activateIndexByName(indexName).then(function () {
            $scope.getIndicesInfo();
            $scope.getContentServicesList();
            $scope.getMediaServicesList();
            $scope.busy = false;
        });
    };

    $scope.getIndicesInfo = function () {
        return umbElasticsearchResource.getIndicesInfo().then(function (data) {
            $scope.info = data.data;
        });
    };

    $scope.getVersionNumber = function () {
        return umbElasticsearchResource.getVersionNumber().then(function (version) {
            $scope.versionNumber = version;
        });
    };

    $scope.rebuildContentIndex = function (indexName) {
        $scope.busy = true;
        notificationsService.success('Rebuilding Content Index', 'Content Index rebuild has started');
        umbElasticsearchResource.rebuildContentIndex(indexName).then(function () {
            $scope.busy = false;
            notificationsService.success("Content Index Rebuild", "Content Index rebuild completed");
            $scope.getIndicesInfo();
        }, function () {
            $scope.busy = false;
            notificationsService.error("Content Index Rebuild", "Content Index Rebuild Failed");
            $scope.getIndicesInfo();
        });
    };

    $scope.rebuildMediaIndex = function (indexName) {
        $scope.busy = true;
        notificationsService.success('Rebuilding Media Index', 'Media Index rebuild has started');
        umbElasticsearchResource.rebuildMediaIndex(indexName).then(function () {
            $scope.busy = false;
            notificationsService.success("Media Index Rebuild", "Index rebuild completed");
            $scope.getIndicesInfo();
        }, function () {
            $scope.busy = false;
            notificationsService.error("Media Index Rebuild", "Media Index Rebuild Failed");
            $scope.getIndicesInfo();
        });
    };

    $scope.refreshIndexList = function () {
        $scope.getIndicesInfo();
    };

    $scope.addIndex = function addIndex() {
        $scope.busy = true;
        notificationsService.success('Creating Index', 'Index addition has started');
        umbElasticsearchResource.createIndex().then(function () {
            $scope.busy = false;
            notificationsService.success("Index Create", "Index was added");
            $scope.getIndicesInfo();
        }, function () {
            $scope.busy = false;
            notificationsService.error("Index Create", "Index create Failed");
            $scope.getIndicesInfo();
        });
    };

    $scope.busy = false;
    $scope.available = false;
    function init() {
        umbElasticsearchResource.ping().then(function (available) {
            $scope.available = available;
            if (available) {
                $scope.settings = umbElasticsearchResource.getSettings();
                if ($scope.settings) {
                    $scope.getVersionNumber();
                    $scope.getIndicesInfo();
                    $scope.getContentServicesList();
                    $scope.getMediaServicesList();
                }
            }
        });
    }

    init();
}

angular
    .module("umbraco")
    .controller("umbElasticsearchController", ["$log", "$scope", "$timeout", "notificationsService", "umbElasticsearchResource", umbElasticsearchController]);