$(function () {
    var loading;
   
    layui.use('layer', function () {
        var layer = layui.layer;
        //非空验证
        $('#addbtn').click(function () {
            var username = $('input[name="UserName"]').val();
            var pwd = $('input[name="Password"]').val();
            var mobile = $('input[name="Mobile"]').val();
            if (username == '') {
                layer.alert('请输入您的公司名称');
            } else if (pwd == '') {
                layer.alert('请输入密码');
            }
            else if (mobile == '') {
                layer.alert('请输入电话号码');
            }
            else {
                var JsonData = { UserName: username, Password: pwd, Mobile: mobile };
                $.ajax({
                    async: false,
                    type: "POST",
                    url: '/Users/Register',
                    data: JsonData,
                    dataType: "json",
                    //beforeSend: function () {
                    //    loading = layer.load(0, {
                    //        shadeClose: false,
                    //        title: '加载中..',
                    //        shade: [0.3, '#fff'] //0.1透明度的白色背景
                    //    });
                    //},
                    success: function (data) {
                        window.location.href = "/Login/Index";
                       
                        //if (data.result == true) {
                           
                        //    window.location.href = "/Login/Index";

                        //    //window.event.returnValue = false;
                        //}
                        //else {
                        //    ErroAlert(data.message);
                        //}
                    },
                    error: function () {
                       
                        layer.alert('登录异常，未连接到服务器');
                    }
                });
            }
        })
     
    })
})

////弹出
//function ErroAlert(e) {
//    var index = layer.alert(e, { icon: 5, time: 2000, offset: 't', closeBtn: 0, title: '错误信息', btn: [], anim: 2, shade: 0 });
//    layer.style(index, {
//        color: '#777'
//    });
//}