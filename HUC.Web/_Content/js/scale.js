$(document).ready(function () {

});


//Notification Functions
// Type of the notice. "notice", "info", "success", or "error".
var notifyOptions = {
    addclass: "stack-bar-top",
    styling: 'bootstrap3',
    sticker: false,
    hide: false,
    width: '70%'
};

function addError(message, title) {
    var currentOptions = notifyOptions;
    currentOptions.title = (typeof title === "undefined") || (title == '') ? 'Error' : title;
    currentOptions.type = "error";

    currentOptions.text = message;
    var n = $.pnotify(currentOptions).click(function () { n.pnotify_remove(); });
}
function addInfo(message, title) {
    var currentOptions = notifyOptions;
    currentOptions.title = (typeof title === "undefined") || (title == '') ? 'Info' : title;
    currentOptions.type = "info";

    currentOptions.text = message;
    var n = $.pnotify(currentOptions).click(function () { n.pnotify_remove(); });
}
function addSuccess(message, title) {
    var currentOptions = notifyOptions;
    currentOptions.title = (typeof title === "undefined") || (title == '') ? 'Success!' : title;
    currentOptions.type = "success";

    currentOptions.text = message;
    var n = $.pnotify(currentOptions).click(function () { n.pnotify_remove(); });
}
function addNotice(message, title) {
    var currentOptions = notifyOptions;
    currentOptions.title = (typeof title === "undefined") || (title == '') ? 'Please Note' : title;
    currentOptions.type = "notice";

    currentOptions.text = message;
    var n = $.pnotify(currentOptions).click(function () { n.pnotify_remove(); });
}

//addError("Test Error!", 'Custom Error Title');
//addInfo("Information Overload", '');
//addSuccess("You WIN!!!!");
//addNotice("An important message");