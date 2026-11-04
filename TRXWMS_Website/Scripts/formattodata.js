//日期转换
function FormatToDate(val) {
    if (val != null) {
        var date = new Date(parseInt(val.replace("/Date(", "").replace(")/", ""), 10));
        //月份为0-11，所以+1，月份小于10时补个0
        var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
        var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
        var h = date.getHours();
        h = h < 10 ? '0' + h : h;
        var min = date.getMinutes();
        min = min < 10 ? '0' + min : min;
        var s = date.getSeconds();
        s = s < 10 ? '0' + s : s;
        return date.getFullYear() + "-" + month + "-" + currentDate + "  " + h + ":" + min + ":" + s;
    }
    return "";
}

//$.ajaxSetup({
//    contentType: "application/x-www-form-urlencoded;charset=utf-8",
//    complete: function (XMLHttpRequest, textStatus) {
//        //通过XMLHttpRequest取得响应头，sessionstatus，
//        var sessionstatus = XMLHttpRequest.getResponseHeader("sessionstatus");
//        if (sessionstatus == "timeout") {
//            //如果超时就处理 ，指定要跳转的页面
//            window.top.location = "/Login/Index";
//        }
//    }
//});