$(document).ready(function () {
    $('.redactor').redactor({
        buttons: ['bold', 'italic', 'unorderedlist', 'orderedlist', 'alignleft', 'aligncenter', 'alignright', 'video'],
        linebreaks: true,
        minHeight: 350,
        fixed: true,
        toolbarFixedBox: true
    });

    $('.revealUpload').click(function() {
        var curWrap = $(this).closest('p');
        curWrap.fadeOut(function() {
            curWrap.siblings('input').fadeIn();
        });
    });

    //Chosen
    $(".chzn-select").chosen();
    $(".chzn-select-removable").chosen({
        allow_single_deselect: true
    });
    //



    //MediaPicker
    $('.mediaPicker').click(function () {
        var curMediaPicker = $(this);
        var targetElement = curMediaPicker.data('targetelement');
        $.colorbox({
            iframe: true,
            width: 800,
            height: 600,
            href: curMediaPicker.attr('rel') + '?' + $.param({
                TargetElement: targetElement,
                IsMultiple: curMediaPicker.data('ismultiple'),
                IsRemovable: curMediaPicker.data('isremovable'),
                fileTypesString: curMediaPicker.data('filetypes')
            })
        });

        return false;
    });
    $('.mediaPickerClear').click(function () {
        var curItem = $(this);
        curItem.siblings('input').val('');
        curItem.siblings('.niceDisplayWrapper').html('');

        return false;
    });
    //--
});

audiojs.events.ready(function () {
    var as = audiojs.createAll();
});