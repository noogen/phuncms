 function getParameterByName(name) {
	name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
	var regexS = "[\\?&]" + name + "=([^&#]*)";
	var regex = new RegExp(regexS);
	var results = regex.exec(window.location.search);
	return (results == null) ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function makeSlug(urlString, filter) {
     // changes, e.g., "Petty theft" to "petty_theft"
     // remove all these words from the string before urlifying

     var s = urlString;

     if (filter) {
         var removelist = ["a", "an", "as", "at", "before", "but", "by", "for", "from",
             "is", "in", "into", "like", "of", "off", "on", "onto", "per",
             "since", "than", "the", "this", "that", "to", "up", "via", "het", "de", "een", "en",
             "with"];

         var r = new RegExp('\\b(' + removelist.join('|') + ')\\b', 'gi');
         s = s.replace(r, '');
     }

     s = s.replace(/[^\/\-\w\s]/g, ''); // remove unneeded chars
     s = s.replace(/^\s+|\s+$/g, ''); // trim leading/trailing spaces
     s = s.replace(/[\-\s]+/g, '\-'); // convert spaces to hyphens
     s = s.toLowerCase(); // convert to lowercase

     return s; // trim to first num_chars chars
}

var contentPath = '/' + getParameterByName("contentPath").replace(/(\/)+$/gi, '').replace(/^(\/)+/gi, '');

$(document).ready(function() {
    $("form[data-action]").each(function() {
        var formAction = $(this).data("action");
        $(this).attr("action", contentPath + '/' + formAction.replace(/(\/)+$/gi, '').replace(/^(\/)+/gi, ''));
    });
});