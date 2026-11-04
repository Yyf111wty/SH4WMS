$(function () {
    var loading;
    $('input[name="username"]').focus();
    $(document).keypress(function (e) {
        if (e.which == 13) {
            $('input[type="button"]').click();
        }
    });
    $('input[name="pwd"]').focus(function () {
        $(this).attr('type', 'password');
    });
    $('input[type="text"]').focus(function () {
        $(this).prev().animate({ 'opacity': '1' }, 200);
    });
    $('input[type="text"],input[type="password"]').blur(function () {
        $(this).prev().animate({ 'opacity': '.5' }, 200);
    });
    //检测账户密码规格
    $('input[name="username"],input[name="pwd"]').keyup(function () {
        var Len = $(this).val().length;
        if (!$(this).val() == '' && Len >= 3) {
            $(this).next().animate({
                'opacity': '1',
                'right': '30'
            }, 200);
        } else {
            $(this).next().animate({
                'opacity': '0',
                'right': '20'
            }, 200);
        }
    });
    layui.use('layer', function () {
        var layer = layui.layer;
        //非空验证
        $('#loginbtn').click(function () {
            var username = $('input[name="username"]').val();
            var pwd = $('input[name="pwd"]').val();
            if (username == '') {
                ErroAlert('请输入您的用户名');
            } else if (pwd == '') {
                ErroAlert('请输入密码');
            }
            else {
                var JsonData = { username: username, pwd: pwd };
                $.ajax({
                    type: "Get",
                    url: '/Login/GetLogin',
                    data: { userName: JsonData.username, password: JsonData.pwd },
                    dataType: "json",
                    beforeSend: function () {
                            loading = layer.load(0, {
                            shadeClose: false,
                            title: '加载中..',
                            shade: [0.3, '#fff'] //0.1透明度的白色背景
                        });
                    },
                    success: function (data) {
                        layer.close(loading);
                        if (data.result == true) {  

                            // ====================== 【关键修改：保存Token】 ======================
                            // ✅ 必须保存
                            //localStorage.setItem('token', data.token);
                            //localStorage.setItem('truename', data.trueName);
                          
                            var trueName = data.trueName;
                            var token = data.token;
                            // ==============================================
                            // 🔥 唯一修改：生成一个当前标签页唯一ID
                            // ==============================================
                            var tabId = Math.random().toString(36).substr(2);
                            sessionStorage.tabId = tabId; // 只存在当前标签

                            // 按 tab 隔离存储
                            localStorage.setItem("TOKEN_" + tabId, token);
                            localStorage.setItem("TRUENAME_" + tabId, trueName);

                            // 必须存一份原始 token（保证所有原有功能正常）
                            //localStorage.setItem("token", token);
                            //localStorage.setItem("truename", trueName);

                            window.location.href = "/Home/Index";
                            
                            //window.event.returnValue = false;
                        }
                        else {  
                            ErroAlert(data.message);
                        }
                    },
                    error: function () {
                        layer.close(loading);
                        ErroAlert('登录异常，未连接到服务器');
                    }
                })
            }
        })
        //$('#registerbtn').click(function () {
        //    {
        //        window.location.href = "/Login/Register";
        //        //layer.open({
        //        //    type: 1, // type: 1 表示 iframe 层类型
        //        //    title: '注册', // 标题
        //        //    content: '<iframe src="/Login/Register" frameborder="0" style="width:400px;height:400px;"></iframe>', // iframe 页面地址
        //        //    area: ['400px', '400px'] // 宽高
        //        //});
             
        //       // window.location.href = "/Login/Register";
                
               
        //    }
        //})
    })
})

//弹出
function ErroAlert(e) {
    var index = layer.alert(e, { icon: 5, time: 2000, offset: 't', closeBtn: 0, title: '错误信息', btn: [], anim: 2, shade: 0 });
    layer.style(index, {
        color: '#777'
    });
}