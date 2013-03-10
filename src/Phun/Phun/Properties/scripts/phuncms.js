/*!
 * Simple Cms object.
 * Requires: jquery, backbone, and createjs
 */

// apply backbone convention for sync
(function($, Backbone, undefined) {
    Backbone.sync = function (method, model, options) {
        var ajaxType = 'GET';

        // assuming window.PhunCms has been initialized above
        // remove invalid characters and concat with action method, use window here to setup value changes
        var ajaxUrl = '/' + window.PhunCms.contentRoute + '/' + method;

        // strip lt and gt sign from subject
        var path = model["@subject"].replace(/[\<\>]/gi, "");

        // if path is a file, then append path to the current location path
        // otherwise, it is a full path so just use the full path
        if (path.indexOf('/') === -1) {
            path = window.location.pathname + "/" + path;
        }

        // set the path and content
        var myModel = {
            Data: model.get("content"),
            Path: path
        };

        // serialize data to send
        var ajaxData = JSON.stringify(myModel);

        // map appropriate ajax type with method
        if (method == 'create') {
            ajaxType = 'POST';
        } else if (method == 'update') {
            ajaxType = 'PUT';
        } else if (method == 'delete') {
            ajaxType = 'DELETE';
        } else {
            // retrieve don't need ajaxData, also our controller action is viewcontent and not retrieve
            ajaxData = null;
            ajaxUrl = ajaxUrl + "/?path=" + path;
        }

        // make ajax call
        $.ajax({
            url: ajaxUrl,
            type: ajaxType,
            data: ajaxData,
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                options.success(model);
            },
            error: function (data) {
                options.error(data);
            }
        });

    };

    if (typeof (PhunCms) != "undefined") {
        
        // method for auto loading cms content
        $(document).ready(function () {
            var nodes = $('[data-cmscontent]');
            PhunCms.contentsToLoad = nodes.length;
            
            $('[data-cmscontent]').each(function () {
                
                var $this = $(this);
                var path = $this.data("cmscontent");
                $this.attr("about", path);
                
                // if path is a file, then append path to the current location path
                // otherwise, it is a full path so just use the full path
                if (path.indexOf('/') === -1) {
                    path = window.location.pathname + "/" + path;
                }
                
                var url = "/" + PhunCms.contentRoute + "/Retrieve/?path=" + path;
                
                $.ajax({
                    url: url.replace(/\/\//gi, '/'),
                    success: function (html) {
                        PhunCms.contentsToLoad--;
                        if (html == "") html = "&nbsp;";
                        $this.html('<div property="content">' + html + '</div>');
                        if (PhunCms.contentsToLoad <= 0) {
                            PhunCms.initEditor();
                        }
                    },
                    error: function(xhr, errDesc, ex) {
                        PhunCms.contentsToLoad--;
                        $this.html('<div property="content">&nbsp;</div>');
                        if (PhunCms.contentsToLoad <= 0) {
                            PhunCms.initEditor();
                        }
                    }
                });
            });
        });
    }
})(jQuery, Backbone);