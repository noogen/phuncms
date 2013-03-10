// RequireJS configuration stuff
var myShimConfig = {
    'jquery': {
        exports: 'jQuery'
    },
    'jqueryui': {
        deps: ['jquery'],
        exports: 'jQuery.ui'
    },
    'underscore': {
        exports: '_'
    },
    'backbone': {
        deps: ['underscore', 'jquery'],
        exports: 'Backbone'
    },
    'vie': {
        deps: ['backbone'],
        exports: 'VIE'
    },
    'jquery.rdfquery': {
        deps: ['jquery']
    },
    'annotate': {
        deps: ['jquery.rdfquery']
    },
    'hallo': {
        deps: ['rangy-core', 'jqueryui', 'annotate'],
        exports: 'jQuery.fn.hallo'
    },
    'ckeditor/ckeditor': {
        deps: ['jquery']
    },
    'create': {
        deps: ['backbone', 'vie', 'jqueryui'],
        exports: 'jQuery.fn.midgardCreate'
    },
    'phuncms': {
        deps: ['create', 'ckeditor/ckeditor', 'phuncms.config']
        // deps: ['create', 'hallo', 'phuncms.config']
    },
    'jquery.cookie': {
        deps: ['jquery'],
        exports: 'jQuery.cookie'
    },
    'jquery.dynatree': {
        deps: ['jquery', 'jqueryui', 'jquery.cookie'],
        exports: 'jQuery.fn.dynatree'
    }
};

function doesLibExists(key) {
    if (key == "jquery") {
        return (typeof (jQuery) !== "undefined");
    }
    if (key == "backbone") {
        // vie needs 0.9.2 or higher
        return (typeof (Backbone) !== "undefined") && (parseFloat(Backbone.VERSION.substr(2)) >= 9.2);
    }
    if (key == "undercore") {
        return (typeof (_) !== "undefined");
    }
    if (key == "vie") {
        return (typeof (VIE) !== "undefined");
    }
    if (key == "hallo") {
        return (typeof (jQuery) !== "undefined") && (typeof (jQuery.fn.hallo) !== "undefined");
    }
    if (key == "jqueryui") {
        // hallo needs 1.9.2 or higher
        return (typeof (jQuery) !== "undefined") && (typeof (jQuery.ui) !== "undefined") && (parseFloat(jQuery.ui.version.substr(2)) >= 9.2);
    }
    if (key == "jquery.cookie") {
        return (typeof (jQuery) !== "undefined") && (typeof (jQuery.cookie) !== "undefined");
    }
    if (key == "jquery.dynatree") {
        return (typeof (jQuery) !== "undefined") && (typeof (jQuery.ui) !== "undefined") && (typeof (jQuery.ui.dynatree) !== "undefined");
    }
};

for (var k in myShimConfig) {
    if (doesLibExists(k)) {
        delete myShimConfig[k];
    } else if (typeof (myShimConfig[k].deps) != "undefined") {
        var newDeps = [];
        for (var i = myShimConfig[k].deps.length - 1; i >= 0; i--) {
            if (!doesLibExists(myShimConfig[k].deps[i])) {
                newDeps.push(myShimConfig[k].deps[i]);
            }
        }
        if (newDeps.length > 0) {
            myShimConfig[k].deps = newDeps;
        } else {
            delete myShimConfig[k]["deps"];
        }
    }
}

requirejs.config({
    shim: myShimConfig
});

define(["phuncms"], function () {
    $(document).ready(function () {
        $('body').midgardCreate({
            url: function () {
                return 'javascript:false;';
            }
        });

    }); // end document ready
});