'use strict';
(function() {
    // Create controller
    function contentController($scope, $http, notificationsService) {
        var updateModelValue = function (response) {
            var xml = JSON.parse(response.data); 
            $scope.model.value = xml;
        }

        var currentNode = $scope.currentNode;
        var currentNodeId = currentNode.id;
        $scope.model = {};
        $scope.currentNodeName = currentNode.name;

        $http({
            method: 'GET',
            url: 'backoffice/XmlEdit/content/getxml',
            params: { nodeId: currentNodeId }
        }).then(function(response) {
            updateModelValue(response);
        }, function errorCallback(response) {
            notificationsService.error("Couldn't get XML");
        });

        $scope.hideDialog = function() {
            $scope.nav.hideDialog(true);
        }

        $scope.saveXml = function() {
            var data = {
                nodeId: currentNodeId,
                xml: $scope.model.value
            };

            $http({
                method: 'POST',
                url: 'backoffice/XmlEdit/content/savexml',
                data: data
            }).then(function(response) {
                notificationsService.success('Successfully saved XML');
                updateModelValue(response);
                $scope.hideDialog();
                // TODO: Refresh property values
            }, function errorCallback(response) {
                //JSON.stringify(response)
                notificationsService.error("Couldn't save XML");
            });
        }
    }

    // Register controller
    angular.module("umbraco").controller("XmlEdit.ContentController", contentController);
})();