function umbElasticsearchController($log, $scope, $timeout, notificationsService, umbElasticsearchResource, $q) {
    var $poller;

    function pollBusyState(interval) {
        $timeout.cancel($poller);
        $poller = $timeout(function poll() {
            $scope.getIndicesInfo();
            if ($scope.busyState.Busy) {
                $poller = $timeout(poll, interval);
            }
        }, interval);
    }

    function withBusyCheck(pollingInterval) {
        var deferred = $q.defer();
        umbElasticsearchResource.isBusy().then(function (busyState) {
            $scope.busyState = busyState;
            if ($scope.busyState.Busy) {
                notificationsService.warning("Busy Operation Notification", "Another index operation is currently underway, you can not perform another operation until this is complete");
                pollBusyState(pollingInterval || 30000);
                deferred.reject(busyState);
            } else {
                deferred.resolve(busyState);
                $timeout(function() {
                    $scope.getIndicesInfo();
                }, 1000);
            }
        });
        return deferred.promise;
    }

    $scope.isBusy = function() {
        return $scope.busyState.Busy;
    }

    $scope.getContentServicesList = function () {
        umbElasticsearchResource.getContentIndexServices().then(function (data) {
            $scope.contentServices = data;
        });
    };

    $scope.getMediaServicesList = function () {
        umbElasticsearchResource.getMediaIndexServices().then(function (data) {
            $scope.mediaServices = data;
        });
    };

    $scope.deleteIndex = function(indexName) {
        withBusyCheck(2000).then(function() {
            return umbElasticsearchResource.deleteIndexByName(indexName);
        }).always(function() {
            $scope.getIndicesInfo();
        });
    }

    $scope.activateIndex = function (indexName) {
        withBusyCheck(2000).then(function () {
            return umbElasticsearchResource.activateIndexByName(indexName);
        }).always(function() {
            $scope.getIndicesInfo();
            $scope.getContentServicesList();
            $scope.getMediaServicesList();
        });
    };

    $scope.getIndicesInfo = function () {
        $scope.indexInfo = null;
        $scope.indexName = null;
        return umbElasticsearchResource.getIndicesInfo().then(function (data) {
            $scope.info = data;
            return umbElasticsearchResource.isBusy().then(function (busyState) {
                $scope.busyState = busyState;
                if ($scope.busyState.Busy) {
                    pollBusyState(10000);
                }
            });
        });
    };

    $scope.viewIndexInfo = function (indexName) {
        return umbElasticsearchResource.getIndexInfo(indexName).then(function (data) {
            $scope.indexName = indexName;
            $scope.indexInfo = data;
        });
    };

    $scope.getVersionNumber = function () {
        return umbElasticsearchResource.getVersionNumber().then(function (version) {
            $scope.versionNumber = version;
        });
    };

    $scope.rebuildContentIndex = function (indexName) {
        withBusyCheck(5000).then(function () {
            return umbElasticsearchResource.rebuildContentIndex(indexName).then(function () {
                notificationsService.success("Content Index Rebuild", "Content Index rebuild completed");
            }, function () {
                notificationsService.error("Content Index Rebuild", "Content Index Rebuild Failed");
            });
        }).always(function() {
            $scope.getIndicesInfo();
        });
    };

    $scope.rebuildMediaIndex = function (indexName) {
        withBusyCheck(5000).then(function () {
            return umbElasticsearchResource.rebuildMediaIndex(indexName).then(function () {
                notificationsService.success("Media Index Rebuild", "Media Index rebuild completed");
            }, function () {
                notificationsService.error("Media Index Rebuild", "Media Index Rebuild Failed");
            });
        }).always(function () {
            $scope.getIndicesInfo();
        });
    };

    $scope.refreshIndexList = function () {
        $scope.getIndicesInfo();
    };

    $scope.addIndex = function addIndex() {
        notificationsService.success('Creating Index', 'Index addition has started');
        umbElasticsearchResource.createIndex().then(function () {
            notificationsService.success("Index Create", "Index was added");
        }, function () {
            notificationsService.error("Index Create", "Index create Failed");
        }).always(function() {
            $scope.getIndicesInfo();
        });
    };

    $scope.setIndexStatusRowStyle = function (item) {
        switch(item.Status) {
            case "Active":
                return { "font-weight": "bold" };
            case "Busy":
                return { "font-style": "italic" };
            default:
                return { };
        }
    }

    $scope.setIndexStatusStyle = function (item) {
        switch (item.Status) {
            case "Active":
                return "icon-checkbox color-green";
            case "Busy":
                return "icon-hourglass color-orange";
            default:
                return "icon-checkbox-empty color-black";
        }
    }

    function init() {
        $scope.busyState = { Busy: false, Message: "", Elapsed: "" };
        $scope.available = false;

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
            return JSON ? JSON.stringify(json, null, "  ") : json;
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
    .controller("umbElasticsearchController", ["$log", "$scope", "$timeout", "notificationsService", "umbElasticsearchResource", "$q", umbElasticsearchController]);