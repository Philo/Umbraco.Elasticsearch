function umbElasticsearchController($log, $scope, $timeout, notificationsService, umbElasticsearchResource, assetsService) {

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
        $scope.indexInfo = null;
        $scope.indexName = null;
        return umbElasticsearchResource.getIndicesInfo().then(function (data) {
            $scope.info = data.data;
        });
    };

    $scope.viewIndexInfo = function (indexName) {
        return umbElasticsearchResource.getIndexInfo(indexName).then(function (data) {
            $scope.indexName = indexName;
            $scope.indexInfo = data.data;
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
        var refresher = $timeout(function () {
            $scope.getIndicesInfo();
        }, 5000);

        umbElasticsearchResource.rebuildContentIndex(indexName).then(function () {
            $timeout.cancel(refresher);
            $scope.busy = false;
            notificationsService.success("Content Index Rebuild", "Content Index rebuild completed");
            $scope.getIndicesInfo();
        }, function () {
            $timeout.cancel(refresher);
            $scope.busy = false;
            notificationsService.error("Content Index Rebuild", "Content Index Rebuild Failed");
            $scope.getIndicesInfo();
        });
    };

    $scope.rebuildMediaIndex = function (indexName) {
        $scope.busy = true;
        notificationsService.success('Rebuilding Media Index', 'Media Index rebuild has started');

        var refresher = $timeout(function () {
            $scope.getIndicesInfo();
        }, 5000);

        umbElasticsearchResource.rebuildMediaIndex(indexName).then(function () {
            $timeout.cancel(refresher);
            $scope.busy = false;
            notificationsService.success("Media Index Rebuild", "Index rebuild completed");
            $scope.getIndicesInfo();
        }, function () {
            $timeout.cancel(refresher);
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
        umbElasticsearchResource.getPluginVersionInfo().then(function (version) {
            $scope.pluginVersionInfo = version;
        });

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
    .filter('prettyJSON', function () {
        function prettyPrintJson(json) {
            return JSON ? JSON.stringify(json, null, '  ') : 'your browser doesnt support JSON so cant pretty print';
        }
        return prettyPrintJson;
    })
    .directive('confirmClick', function ($window) {
        var i = 0;
        return {
            restrict: 'A',
            priority: 1,
            compile: function (tElem, tAttrs) {
                var fn = '$$confirmClick' + i++,
                    _ngClick = tAttrs.ngClick;
                tAttrs.ngClick = fn + '($event)';

                return function (scope, elem, attrs) {
                    var confirmMsg = attrs.confirmClick || 'Are you sure?';

                    scope[fn] = function (event) {
                        if ($window.confirm(confirmMsg)) {
                            scope.$eval(_ngClick, { $event: event });
                        }
                    };
                };
            }
        };
    })
    .controller("umbElasticsearchController", ["$log", "$scope", "$timeout", "notificationsService", "umbElasticsearchResource", "assetsService", umbElasticsearchController]);