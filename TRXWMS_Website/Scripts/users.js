var flag = 0;
var Id = 0;
$(function () {
    var pageId = GetQueryString('pageId');
    loadGroupFormButton(pageId);
    //加载tree跟datagrid
    loadTreeData();
    searchClick
    loadUserDatagrid("");
    //加载所有用户信息
    searchClick();
    
});
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return (r[2]);
    return null;
}
searchClick();
//加载tree
function loadTreeData() {
    var data = eval(getTreeJson());
    var tree = hg.tree.datatree('treeid', data, false);
    tree.onclick = function () {
        var obj = this;
        var id = obj.data.id;
        loadDatagrid(id);
        loadUserDatagrid("");
    }
}

// 后台返回数据
function getTreeJson() {

    var treedata;
    $.ajax({
        type: "get",
        async: false,//同步执行
        url: "/Users/GetDatatreeData",
        dataType: "json",
        success: function (result) {
            treedata = result;
        },
        error: function (message) {
            //$.messageBox('系统提示', message); return;
            layer.alert(message, { icon: 6 }); return;

        }
    })
    return treedata;
}
//加载datagrid
function loadDatagrid(selectGroupid) {

    var tableCols = [[

        { field: 'Id', title: 'Id', align: 'center', sort: true, hide: true },
        { field: 'UserId', title: 'UserId', align: 'center', sort: true, hide: true },
        { field: 'UserName', title: '用户名', width: 150, align: 'center', sort: true },
        { field: 'Password', title: '密码', align: 'center', sort: true, hide: true },
        { field: 'TrueName', title: '代表公司', width: 200, align: 'center', sort: true },
        { field: 'SecretKeyType', title: '秘钥类型', width: 150, align: 'center', sort: true, hide: true },
        { field: 'SecretKeyValue', title: '秘钥值', width: 150, align: 'center', sort: true, hide: true},
        { field: 'UserGroupName', title: '所属组别', width: 200, align: 'center', sort: true, hide: true},
        { field: 'UserGroupId', title: '组别id', align: 'center', sort: true, hide: true },
        { field: 'Mobile', title: '手机号', width: 200, align: 'center', sort: true },
        { field: 'Email', title: '电子邮箱', width: 200, align: 'center', sort: true, hide: true },
        {
            field: 'CreateTime', width: 200, hide: true, title: '创建时间', align: 'center', sort: true, templet:
                function (d) {
                    return FormatToDate(d.CreateTime);
                }
        },
        { field: 'CreateOperator', width: 150, title: '创建人', align: 'center', sort: true },
        //{ field: 'UpdateTime', title: '更新时间', align: 'center',  sort: true },       
        //{ field: 'UpdateOperator', title: '更新人', align: 'center',  sort: true }


    ]
    ];
    //基本数据表格
    var table = hg.table.datatable('test', '用户管理', '/Users/GetUsers?selectGroupid=' + selectGroupid, {}, tableCols, '', true, 'full-255');
    table.onrowclick(function (obj) {
        var data = obj.data.UserName;
        loadUserDatagrid(data);
    });
}
//角色
function loadUserDatagrid(name) {
    var tableCols = [[
        { field: 'Id', title: 'Id', align: 'center', sort: true, hide: true },
        { field: 'RoleName', title: '角色名称', align: 'center', sort: true },
        { field: 'Describes', title: '角色描述', align: 'center', sort: true }
    ]
    ];
    //if (name == "") {
    //    hg.table.datatable('userrole', '用户管理', '' , {}, tableCols, '', true, 'full-255');
    //} else {
    hg.table.datatable('userrole', '用户管理', '/Users/GetUserRoles?userName=' + name, {}, tableCols, '', true, 'full-255');
    //}
}


//查询
function searchClick() {
    layui.use(['form', 'layer', 'jquery'], function () {
        $ = layui.jquery;
        var form = layui.form;
        var data = form.val('example');
      
        loadDatagrid(0);
    })
}
//新增
function addClick() {
    var token = getCurrentToken();
    var url = '/Users/AddOrEdit?type=0';
    if (token) url += '&token=' + token;
    hg.open('发布项目', url, 600, 600);
    // 打开弹窗
   // hg.open('新增', '/Users/AddOrEdit?type=0', 600, 600);
}
//修改
function editClick() {
    //obj通过行tool修改-lay-event
    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        var token = getCurrentToken();
        var datas = "&Id=" + row.data.Id + "&UserName=" + row.data.UserName + "&Password=" + row.data.Password + "&TrueName=" + (row.data.TrueName) + "&UserGroupName=" + row.data.UserGroupName +
            "&UserGroupid=" + row.data.UserGroupId + "&Mobile=" + row.data.Mobile + "&Email=" + row.data.Email + "&SecretKeyType=" + row.data.SecretKeyType + "&SecretKeyValue=" + row.data.SecretKeyValue;

        var url = '/Users/AddOrEdit?type=1' + datas;
        if (token) url += '&token=' + token;
        hg.open('编辑', url, 600, 600);
    } else {
        hg.msg('请选中一行数据，再操作！');
    }
}

//删除用户
function deleteClick() {
    //obj通过行tool删除-lay-event
    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        hg.confirm("删除当前选中的数据，确定要删除吗？", function () {
            //row.del(); //只删本地数据
            $.ajax({
                async: false,
                type: "POST",
                url: '/Users/DeleteUsers',
                data: { id: row.data.Id },
                dataType: "json",
                success: function (data) {
                    if (data.result == true) {
                        hg.msg('删除成功！');
                        setTimeout(function () { window.location.reload(); }, 500);
                    }
                    else {
                        hg.msg('删除失败！' + data.message);

                    }

                },
                error: function (data) {
                    hg.msg('删除失败！' + data.message);

                }
            })


        });
    } else {
        hg.msg('请选中一行数据，再操作！');
    }
}
//设置角色
function setroleClick() {

    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        var token = getCurrentToken();
        var datas = "UserId=" + row.data.UserId + "&UserName=" + row.data.UserName + "&TrueName=" + row.data.TrueName;

        var url = '/Users/RoleSet?' + datas;
        if (token) url += '&token=' + token;
        hg.open('设置角色', url, 600, 400);
    } else {
        hg.msg('请选中一行数据，再操作！');
    }



}
//重置密码
function setpasswordClick() {

    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        var datas = "Id=" + row.data.Id + "&UserName=" + row.data.UserName + "&TrueName=" + row.data.TrueName;
        var token = getCurrentToken();
        var url = '/Users/PwdSet?' + datas;
        if (token) url += '&token=' + token;
        hg.open('重置密码', url, 600, 400);
    } else {
        hg.msg('请选中一行数据，再操作！');
    }

}



