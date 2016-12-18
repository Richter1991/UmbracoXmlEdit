'use strict';
(function () {
    // Register controller
    angular.module("umbraco").controller("XmlEdit.ContentController", ['$scope', '$http', '$timeout', 'notificationsService', function ($scope, $http, $timeout, notificationsService) {
        function initCodemirror() {
            // Set timeout to make sure value is bound before we initialize codemirror
            $timeout(function () {
                // Initialize codemirror
                if (typeof (CodeMirror) != 'undefined') {
                    var textArea = document.getElementById('xmledit-content');

                    // Only init if value is set when XML is loaded, otherwise it'll won't be shown at all
                    if (textArea.value) { 
                        var codeEditor = CodeMirror.fromTextArea(textArea, {
                            tabMode: "shift",
                            matchBrackets: true,
                            indentUnit: 4,
                            indentWithTabs: true,
                            enterMode: "keep",
                            lineWrapping: false,
                            lineNumbers: true
                        });

                        codeEditor.on('change', function (obj) {
                            $scope.model.value = obj.getValue();
                        });
                    }
                }
            }, 10);
        }

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
        }).then(function (response) {
            updateModelValue(response);
            initCodemirror();
        }, function errorCallback(response) {
            notificationsService.error("Couldn't get XML");
        });

        $scope.hideDialog = function () {
            $scope.nav.hideDialog(true);
        }

        $scope.saveXml = function (publish) {
            var data = {
                nodeId: currentNodeId,
                xml: $scope.model.value,
                publish: publish
            };

            $http({
                method: 'POST',
                url: 'backoffice/XmlEdit/content/savexml',
                data: data
            }).then(function (response) {
                notificationsService.success(getSuccessMessage(publish));
                updateModelValue(response);
                $scope.hideDialog();
                // TODO: Refresh property values
            }, function errorCallback(response) {
                //JSON.stringify(response)
                notificationsService.error("Couldn't save XML");
            });
        }

        function getSuccessMessage(publish) {
            var message = 'Successfully saved';
            if (publish) {
                message += ' and published';
            }
            message += ' XML';

            return message;
        }
    }]);
})();