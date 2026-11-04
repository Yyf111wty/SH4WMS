var flag = 0;
var Id = 0;
$(function () {
    var pageId = GetQueryString('pageId');
    loadGroupFormButton(pageId);
    loadUserDatagrid("");
    //加载所有用户信息
    loadDatagrid();


});
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return (r[2]);
    return null;
}


//加载datagrid
function loadDatagrid() {

    var tableCols = [[

        { field: 'Id', title: 'Id', align: 'center', sort: true, hide: true },
        { field: 'RoleId', title: 'RoleId', align: 'center', sort: true, hide: true },
        { field: 'RoleName', title: '角色名称', align: 'center', sort: true },
        { field: 'RoleLevel', title: '角色等级', align: 'center', sort: true },
        {
            field: 'IsEnabled', title: '是否启用', templet: function (d) {
                var checked = d.IsEnabled == 1 ? 'checked' : '';
                return '<input type="checkbox" name ="' + d.id + '" value= ' + d.IsEnabled +
                    ' lay-skin="switch" lay-text="开启|禁用"  disabled="disabled" lay-filter="IsEnabled" ' + checked + '>'
            }
        },
        { field: 'Describes', title: '描述', align: 'center', sort: true },
        {
            field: 'CreateTime', title: '创建时间', align: 'center', width: 180, sort: true, templet:
                function (d) {
                    return FormatToDate(d.CreateTime);
                }
        },
        { field: 'CreateOperator', title: '创建人', align: 'center', sort: true }
        //{ field: 'UpdateTime', title: '更新时间', align: 'center', width: 100, sort: true },       
        //{ field: 'UpdateOperator', title: '更新人', align: 'center', width: 100, sort: true }


    ]
    ];
    //基本数据表格
    var table = hg.table.datatable('test', '角色信息', '/Roles/GetRoles', {}, tableCols, '', true, 'full-255');
    table.onrowclick(function (obj) {
        var Id = obj.data.RoleId;
        loadUserDatagrid(Id);
    });
}
//用户
function loadUserDatagrid(Id) {
    var tableCols = [[

        { field: 'UserName', title: '用户名', align: 'center', sort: true },
        { field: 'TrueName', title: '真实姓名', align: 'center', sort: true },
        { field: 'UserGroupName', title: '所属组别', align: 'center', sort: true },
        { field: 'Mobile', title: '联系方式', align: 'center', sort: true },
        { field: 'Email', title: '电子邮箱', align: 'center', sort: true }
    ]
    ];
    if (Id == "") {
        hg.table.datatable('userrole', '用户', '', {}, tableCols, '', true, 'full-255');
    } else {
        hg.table.datatable('userrole', '用户', '/Roles/GetUsers?roleid=' + Id, {}, tableCols, '', true, 'full-255');
    }
}

//新增
function addClick() {
    hg.open('新增', '/Roles/AddOrEdit?type=0', 600, 450);
}
//修改
function editClick() {
    //obj通过行tool修改-lay-event
    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        var datas = "&roleId=" + row.data.Id + "&roleName=" + row.data.RoleName + "&roleLevel=" + row.data.RoleLevel + "&Describes=" + (row.data.Describes) + "&isEnabled=" + row.data.IsEnabled;
        hg.open('编辑', '/Roles/AddOrEdit?type=1' + datas, 600, 450);
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
                url: '/Roles/DeleteRole',
                data: { roleId: row.data.Id },
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

