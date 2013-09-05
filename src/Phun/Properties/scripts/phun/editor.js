$(document).ready(function () {
    angular.bootstrap(document, ['editorApp']);
});

/*global angular:true */
// it has to be defined like this for minification to work
var NavCtrl = ['$scope', '$window', '$location',  '$timeout', '$rootScope', function($scope, $window, $location,$timeout, $rootScope) {
    'use strict';

    $scope.filePath = $rootScope.filePath;
    $scope.returnUrl = $rootScope.returnUrl;
    $scope.pagePath = $scope.filePath.replace('_default.vash', '').replace('_default.htm', '');
    $scope.isPage = $scope.pagePath.length < $scope.filePath.length;
    $scope.canViewPage =  $scope.isPage || $scope.filePath.indexOf('.htm') > 0;
    $scope.isNotInsideIFrame = (window.self === window.top);
    
    $scope.doClose = function() {
        window.close();
    };
    
    $scope.doSave = function () {
        $("form").submit();
    };

    $scope.doViewPage = function () {
        var truePath = ($scope.isPage) ? $scope.pagePath : $scope.filePath;
        window.open(truePath.replace('/page', ''), '');
    };
}];

/*global angular:true */
// it has to be defined like this for minification to work
var MainCtrl = ['$scope', '$window', '$location', '$timeout', '$rootScope', function ($scope, $window, $location, $timeout, $rootScope) {
    'use strict';
    $scope.filePath = $rootScope.filePath;
    $scope.returnUrl = $rootScope.returnUrl;
}];

var editorApp = angular.module('editorApp', [])
    .config(['$routeProvider', function ($routeProvider) {
        // can map $routeProvider.when here
    }]).run(['$rootScope', '$window', '$timeout', function ($rootScope, $window, $timeout) {
        // bootstrap here
        var editorHolder = $("#code");
        var filePath = getParameterByName("path");
        CodeMirror.commands.autocomplete = function (cm) {
            CodeMirror.showHint(cm, CodeMirror.htmlHint);
        };
        
        $rootScope.editorType = getParameterByName("editor");
        if (typeof ($rootScope.editorType) == "undefined") {
            $rootScope.editorType = "simple";
        }
        
        $rootScope.filePath = filePath;
        $rootScope.returnUrl = window.location.pathname + window.location.search;

        // ajax load the data
        $.ajax({
            url: contentPath + "/RetrieveSecure?forEdit=true&path=" + filePath,
            cache: false,
            data: null,
            success: function (data) {
                data += '';
                $(".bar").hide();
                $(".content").show();
                if (data.indexOf('\n') < 0 && data.indexOf('\r') < 0) {
                    data = html_beautify(data, {
                        'indent_inner_html': true,
                        'indent_size': 4,
                        'indent_char': ' ',
                        'wrap_line_length': 0,
                        'brace_style': 'expand',
                        'unformatted': ['a', 'sub', 'sup', 'b', 'i', 'u'],
                        'preserve_newlines': true,
                        'max_preserve_newlines': 5,
                        'indent_handlebars': false
                    });
                }
                
                if (filePath.indexOf(".vash") > 0 || filePath.indexOf(".js") > 0 || filePath.indexOf(".css") > 0 || filePath.indexOf("_default.htm") > 0 || $rootScope.editorType == "advance") {
                    $rootScope.editorType = "advance";
                    $rootScope.editor = CodeMirror.fromTextArea(editorHolder[0], {
                        lineNumbers: true,
                        mode: 'application/x-razor',
                        indentUnit: 4,
                        indentWithTabs: false,
                        autoCloseTags: true,
                        enterMode: "keep",
                        tabMode: "shift",
                        extraKeys: { "Ctrl-Space": "autocomplete" }
                    });

                    $rootScope.editor.setValue(data);
                }
                else {
                    $rootScope.editorType = "simple";
                    $rootScope.editor = CKEDITOR.replace(editorHolder[0], {
                        fullPage: data.toLowerCase().indexOf('<html>') > -1,
                        extraPlugins: 'wysiwygarea,codemirror',
                        height: '100%',
                        enterMode: CKEDITOR.ENTER_BR
                    });

                    $rootScope.editor.setData(data);
                }
            }
        });
    }]);