$(function () {
    openPwd();
    $('#editpass').click(function () {
        $('#editPw').window('open');
    });
    $('#btnEp').click(function () {
        serverLogin();
    })
    $('#btnCancel').click(function () { closePwd(); })
    $('#loginOut').click(function () {
        $.messager.confirm('系统提示', '您确定要退出本次登录吗?', function (r) {
            if (r) {
                location.href = '/Login/Index';
                sessionStorage.removeItem("userName"); 
                sessionStorage.removeItem("password"); 
            }
        });
    });
});


//设置登录窗口
function openPwd() {
    $('#editPw').window({
        title: '修改密码',
        width: 300,
        modal: true,
        shadow: true,
        closed: true,
        height: 160,
        resizable: false
    });
}
//关闭登录窗口
function closePwd() {
    $('#editPw').window('close');
}

//修改密码
function serverLogin() {
    var $newpass = $('#txtNewPass');
    var $rePass = $('#txtRePass');
    if ($newpass.val() == '') {
        $.messageBox('系统提示', "请输入密码"); return;
    }
    if ($rePass.val() == '') {
        $.messageBox('系统提示', "请再一次输入密码"); return;
    }
    if ($newpass.val() != $rePass.val()) {
        $.messageBox('系统提示', "两次密码不一致！请重新输入"); return;
    }
    $.ajax({
        type: "POST",
        url: '/Home/ResetPassword',
        data: { password: $newpass.val() },
        dataType: "json",
        success: function (data) {
            if (data.result == true) {
                $.messageBox('系统提示', "恭喜，密码修改成功！");
                $newpass.val('');
                $rePass.val('');
                closePwd();
            }
            else {
                $.messageBox('系统提示', data.message);
            }
        },
        error: function (data) {
            $.messageBox('系统提示', data.message);
        }
    })

}



