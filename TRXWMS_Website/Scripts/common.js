function msgShow(title, msgString, msgType) {
    $.messager.alert(title, msgString, msgType);
}
$.extend({
    messageBox: function (title, msg) {
        $.messager.show({
            title: '<span class="fa fa-info">&nbsp;&nbsp;' + title + '</span>', msg: msg, timeout: 5000, showType: 'slide', style: {
                left: '',
                right: 10,
                top: '',
                bottom:10,
                width: 250,
            }
        });
    }
});
