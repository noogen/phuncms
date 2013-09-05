var basePath = '/page';

/*global angular:true */
// it has to be defined like this for minification to work
var NavCtrl = ['$scope', '$window', '$location', '$timeout', '$rootScope', '$http', function ($scope, $window, $location, $timeout, $rootScope, $http) {
    'use strict';

    $scope.elements = [];
    $scope.paths = [];
    $scope.savedPath = '';
    $scope.lastPath = '';
    $scope.brand = 'Page Manager';
    $scope.newPageName = '';
    
    var checkPath = $location.url();
    if (checkPath.indexOf('&') > 0) {
        $location.url('/');
    }
    
    if ($rootScope.mode == 'all') {
        $scope.brand = 'File Manager';
    } else if ($rootScope.mode == 'browser') {
        $scope.brand = 'File Browser';
    }

    var addAPage = $("#addAPage").dialog({
        modal: true,
        autoOpen: false,
        width: 500,
        buttons: {
            "Add": function () {

                var fileName = $("#pageName").val();
                var fileExt = fileName.substr(fileName.indexOf('.')).replace('.', '');
                var theName = fileName.replace(fileExt, '');
                theName = makeSlug(theName.replace(/\//gi, "").replace(/\\/gi, ""));

                if (theName != "") {
                    $.ajax({
                        type: "POST",
                        url: $rootScope.contentPath + "/Create",
                        data: { path: $location.path + theName + "/_default.htm", data: '<!DOCTYPE html><html><head><title>' + $("#pageName").val() + '</title></head><body></body></html>' },
                        cache: false,
                        success: function (data) {
                            $location.path($location.path() + ' ');
                        }
                    });
                }

                $(this).dialog("close");
            },

            // creates a button to cancel the dialog.
            "Cancel": function () {

                // Close the dialog
                $(this).dialog("close");

                // Clear any values that may of been entered.
                $("#pageName").val("");
            }
        }
    });
    
    $scope.resultPageName = function() {
        var theName = $scope.newPageName;
        if (theName.indexOf('.') != -1) {
            var fileExt = theName.substr(theName.indexOf('.')).replace('.', '');
            theName = theName.replace(fileExt, '');
        }
        theName = makeSlug(theName.replace(/\//gi, "").replace(/\\/gi, ""));
        return theName;
    };
    
    $scope.doCreatePartial = function () {
        var fileExt = '';
        var fileName = prompt("Enter a partial/file name:");
        if (fileName && fileName != '') {

            var theName = fileName;
            if (fileName.indexOf('.') > 0) {
                fileExt = fileName.substr(fileName.indexOf('.')).replace('.', '');
                theName = theName.replace(fileExt, '');
            }

            theName = makeSlug(theName.replace(/\//gi, '').replace(/\\/gi, ''));
            theName += makeSlug(fileExt);
            if (theName != "") {
                var fileExists = false;
                for (var i = 0; i < $scope.elements.length; i++) {
                    if (!$scope.elements[i].IsFolder && $scope.elements[i].FileName == theName) {
                        fileExists = true;
                        break;
                    }
                }
                
                if (fileExists) {
                    if (!confirm('Existing file "' + theName + '" found.  Overwrite?')) {
                        return;
                    }
                }
                
                $.ajax({
                    type: "POST",
                    url: $rootScope.contentPath + "/Create",
                    data: { path: $location.path() + theName, data: 'TODO: Please replace me with content.' },
                    cache: false,
                    success: function (data) {
                        $location.path($location.path() + ' ');
                    }
                });
            }
        }
    };
    
    $scope.doClose = function () {
        window.close();
    };

    $scope.doCreatePage = function () {
        $scope.addPageOpen = true;
    };

    $scope.doCreateFolder = function () {
        var theName = window.prompt('Enter name of new folder under: ' + $location.path(), '');
        theName = makeSlug(theName.replace(/\//gi, '').replace(/\\/gi, ''));

        if (theName != null && theName != '') {
            var fileExists = false;
            for (var i = 0; i < $scope.elements.length; i++) {
                if ($scope.elements[i].IsFolder && $scope.elements[i].FileName == theName) {
                    fileExists = true;
                    break;
                }
            }

            if (fileExists) {
                if (alert('Unable to continue, existing page/folder "' + theName + '" found.')) {
                    return;
                }
            }
            
            $scope.elements.push({
                ModifyDate: new Date(),
                CreateDate: new Date(),
                IsFolder: true,
                Path: $location.path() + theName + '/',
                FileName: theName
            });
        }
    };
    
    $('#fileupload').fileupload({
        url: $rootScope.contentPath + '/FileManagerUpload',
        singleFileUploads: true,
        formData: [],
        dataType: 'json',
        add: function (e, data) {
            $scope.savedPath = basePath + $location.path();
            data.formData = [{
                name: 'path',
                value: $scope.savedPath
            }];
            data.submit();
        },
        done: function (e, data) {
            // refresh path
            $location.path($scope.savedPath);
        }
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');;

    $scope.doViewPage = function (element) {
        window.open(element.Path.substr(5), '', '');
    };

    $scope.doDownload = function (element) {
        window.open($rootScope.contentPath + "/RetrieveSecure?path=" + encodeURI(element.Path), '', '');
    };

    $scope.doEdit = function (element) {
        if (element.IsFolder) return;

        window.open($rootScope.contentPath + "/Edit?path=" + encodeURI(element.Path), '', '');
    };

    $scope.doDelete = function (element) {
        var message = "Delete " + (element.IsFolder ? "folder: " : "file: ");
        message += element.FileName + "?";
        if (confirm(message)) {
            $.ajax({
                type: "DELETE",
                url: $rootScope.contentPath + "/Delete",
                data: { path: element.Path },
                cache: false,
                success: function (data) {
                    $timeout($scope.reload, 5);
                }
            });
        }
    };

    $scope.goElement = function (element) {
        if (element.IsFolder) {
            $location.path(($rootScope.mode != 'all') ? element.Path.substr(5) : element.Path);
            return;
        }

        if ($rootScope.mode == 'browser') {
            $rootScope.editorCallback(element.Path);
            return;
        }
        
        $scope.doDownload(element);
    };

    $scope.reload = function () {
        var queryPath = $rootScope.contentPath + '/List?path=' + basePath + $location.path();
        $http.get(queryPath).success(function (response) {
            $scope.elements.length = 0;
            for (var i = 0; i < response.length; i++) {
                response[i].ModifyDate = new Date(parseInt(response[i].ModifyDate.substr(6)));
                response[i].CreateDate = new Date(parseInt(response[i].CreateDate.substr(6)));
                response[i].ClassName = "ext" + response[i].FileExtension.toLowerCase().replace('.', '_');
                $scope.elements.push(response[i]);
            }
        });
    };
    
    $rootScope.$on('PageChanged', function () {
        $scope.paths.length = 0;
        $scope.lastPath = '';

        var updatedPath = $location.path().replace(/^\/|\/$/g, '');
        if (updatedPath.length > 0) {
            var paths = updatedPath.split('/');
            var currentPath = '/';

            for (var i = 0; i < (paths.length - 1) ; i++) {
                var path = paths[i];
                currentPath += path + '/';

                $scope.paths.push({ name: path, value: currentPath });
            }

            $scope.lastPath = paths[paths.length - 1];
        }
        
        $scope.reload();
    });
}];

var MainCtrl = ['$scope', '$rootScope', '$location', function ($scope, $rootScope, $location) {
    'use strict';

    var checkPath = $location.url();
    if (checkPath.indexOf('&') > 0) {
        $location.url('/');
    }
    
    $rootScope.$broadcast('PageChanged');
}];

var pageManagerApp = angular.module('pageManagerApp', [])
    .config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
        // can map $routeProvider.when here
        $routeProvider.when('/*path', { controller: MainCtrl, templateUrl: 'partials/folder-list.htm' })
            .otherwise({ redirectTo: '/' });
    }]).run(['$rootScope', '$window', '$timeout', function ($rootScope, $window, $timeout) {
        $rootScope.mode = getParameterByName('mode');
        if ($rootScope.mode == 'all') {
            basePath = '/';
        }
        if (typeof ($rootScope.mode) == 'undefined' || ($rootScope.mode != 'all' && $rootScope.mode != 'browser')) {
            $rootScope.mode = 'page';
        }
        $rootScope.contentPath = contentPath;
        $rootScope.basePath = basePath;
        $rootScope.editorCallback = ckEditorCallback;
    }]);

// ckeditor callback model
function ckEditorCallback(fileId) {
    var funcNum = getParameterByName('CKEditorFuncNum');
    window.opener.CKEDITOR.tools.callFunction(funcNum, contentPath + '/download' + fileId);
    window.close();
}

$(document).ready(function () {
    new FastClick(document.body);
    angular.bootstrap(document, ['pageManagerApp']);
});
