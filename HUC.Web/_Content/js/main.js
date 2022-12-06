$(document).ready(function () {

    $('input, textarea').placeholder();

    $('.login-button').colorbox({
        href: "/Auth/LoginPod",
        maxWidth: '90%',
        iframe: true,
        innerWidth: 680,
        innerHeight: 335
    });

    $('.mobile-nav').click(function () {
        $('#nav-bar').stop().slideToggle();
    });

    $('.notification-close').click(function() {
        $('.notification-bar').slideUp();
        return false;
    });

    $('.servicebanner-prev').click(function() {
        var curVisibleItem = $('.servicebanner .open');
        var prevItem = curVisibleItem.prev();

        $('.servicebanner li').removeClass('open');
        if (prevItem.length > 0) {
            prevItem.addClass('open');
        } else {
            $('.servicebanner li:last').addClass('open');
        }
    });

    $('.servicebanner-next').click(function () {
        var curVisibleItem = $('.servicebanner .open');
        var nextItem = curVisibleItem.next();

        $('.servicebanner li').removeClass('open');
        if (nextItem.length > 0) {
            nextItem.addClass('open');
        } else {
            $('.servicebanner li:first').addClass('open');
        }
    });
});

window.closeRefreshColorbox = function () {
    window.location.reload();
}

function addError(message, title) {
    title = (typeof title === "undefined") || (title == '') ? 'Error' : title;
    $('.notification-bar').hide();
    $('.notification-bar').attr('class', 'notification-bar').addClass('error');
    $('.notification-title').html(title);
    $('.notification-message').html(message);
    $('.notification-bar').slideDown();
}
function addInfo(message, title) {
    title = (typeof title === "undefined") || (title == '') ? 'Info' : title;
    $('.notification-bar').hide();
    $('.notification-bar').attr('class', 'notification-bar').addClass('warning');
    $('.notification-title').html(title);
    $('.notification-message').html(message);
    $('.notification-bar').slideDown();
}
function addSuccess(message, title) {
    title = (typeof title === "undefined") || (title == '') ? 'Success!' : title;
    $('.notification-bar').hide();
    $('.notification-bar').attr('class', 'notification-bar').addClass('success');
    $('.notification-title').html(title);
    $('.notification-message').html(message);
    $('.notification-bar').slideDown();
}
function addNotice(message, title) {
    title = (typeof title === "undefined") || (title == '') ? 'Please Note' : title;
    $('.notification-bar').hide();
    $('.notification-bar').attr('class', 'notification-bar').addClass('note');
    $('.notification-title').html(title);
    $('.notification-message').html(message);
    $('.notification-bar').slideDown();
}

//addError("Test Error!", 'Custom Error Title');
//addInfo("Information Overload", '');
//addSuccess("You WIN!!!!");
//addNotice("An important message");