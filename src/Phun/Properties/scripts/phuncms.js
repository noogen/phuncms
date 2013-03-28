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
            Path: "/page/" + path.replace(/^(\/)+/, '')
        };

        // serialize data to send
        var ajaxData = JSON.stringify(myModel);

        // map appropriate ajax type with method
        if (method == 'create') {
            ajaxType = 'POST';
        } else if (method == 'update') {
            ajaxType = 'POST';
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
            
            function afterAllLoaded() {
                if (PhunCms.contentsToLoad <= 0) {
                    PhunCms.initEditor();
                }
            }
            
            $('[data-cmscontent]').each(function () {
                
                var $this = $(this);
                var path = $this.data("cmscontent");
                $this.attr("about", path);  
				var text = $this.text().replace(/\s/gi, '');
				
                if (text != "") {
                    PhunCms.contentsToLoad--;
                    var html = $this.html();

                    if (html.toLowerCase().indexOf("<body>") > 0) {
                        html = html.split(/(<body>|<\/body>)/ig)[2];
                    }
                    
                    $this.html('<div property="content">' + html + '</div>');
                    afterAllLoaded();
                    return;
                }                              

                // if path is a file, then append path to the current location path
                // otherwise, it is a full path so just use the full path
                if (path.indexOf('/') === -1) {
                    path = window.location.pathname + "/" + path;
                }
                
                path = "/page/" + path.replace(/^(\/)+/, '');
                
                var url = "/" + PhunCms.contentRoute + "/Retrieve/?path=" + path;
                
                $.ajax({
                    url: url.replace(/\/\//gi, '/'),
                    success: function (html) {
                        PhunCms.contentsToLoad--;
                        if (html == "") html = "";

                        if (html.toLowerCase().indexOf("<body>") > 0) {
                            html = html.split(/(<body>|<\/body>)/ig)[2];
                        }
                       
                        $this.html('<div property="content">' + html + '</div>');
                        afterAllLoaded();
                    },
                    error: function(xhr, errDesc, ex) {
                        PhunCms.contentsToLoad--;
                        $this.html('<div property="content">&nbsp;</div>');
                        afterAllLoaded();
                    }
                });
            });
        });
    }
})(jQuery, Backbone);