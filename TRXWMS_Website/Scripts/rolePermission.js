
var roleList;
var treeTable;
var re;
$(function () {
    var pageId = GetQueryString('pageId');
    loadGroupFormButton(pageId);
    //加载所有角色信息
    loadDatagrid();  
    loadRolePermissionDatagrid();

});

//保存
function saveClick() {

    var row = hg.table.datatable('test', 'getSelected');
    var selectRoleid;
    if (row) {
        selectRoleid = row.data.RoleId;   //获取选中行的角色id
    } else {
        hg.msg('请选择角色并勾选对应权限模块！'); return;
    }
    var idList = "";
    idList = treeTable.checked(re).join(',')
    if (idList.length == 0) {
        //  $.messageBox("系统提示", "请帮角色勾选对应权限模块！"); return;
        hg.msg('请选择角色并勾选对应权限模块！'); return;
    }

    $.ajax({
        type: "POST",
        url: '/RolePermission/SetRolePermissionFor',
        data: { roleId: selectRoleid, permissionList: idList },
        dataType: "json",
        success: function (data) {
            if (data.result == true) {
                //$.messageBox('系统提示', data.message);
                hg.msg(data.message);
                setTimeout(function () { hg.table.datatable('userrolemodule', 'reload'); }, 500);
            }
            else {
                hg.msg(data.message);
                setTimeout(function () { hg.table.datatable('userrolemodule', 'reload'); }, 500);

                // $.messageBox('系统提示', data.message);
            }
        },
        error: function (data) {
            //$.messageBox('系统提示', data.message);
            hg.msg(data.message);
            setTimeout(function () { hg.table.datatable('userrolemodule', 'reload'); }, 500);

        }
    })
}


//传值方法
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

        { field: 'Id', title: 'Id', align: 'center',  sort: true, hide: true },
        { field: 'RoleName', title: '角色名称', align: 'center',  sort: true },
        { field: 'RoleLevel', title: '角色等级', align: 'center',  sort: true, hide: true },
        { field: 'RoleId', title: '角色id', align: 'center',  sort: true, hide: true },
        { field: 'Describes', title: '角色描述', align: 'center',  sort: true }

    ]
    ];
    //基本数据表格
    var table = hg.table.datatable('test', '角色信息', '/RolePermission/GetRoles', {}, tableCols, '', true, 'full-255');
    table.onrowclick(function (obj) {
        var RoleId = obj.data.RoleId;
        getRolePermissions(RoleId);
        loadRolePermissionDatagrid();
    });
}
//获取列表
function getRolePermissions(roleId) {
    $.ajax({
        type: "post",
        url: '/RolePermission/GetRolePermission',
        data: { roleId: roleId },
        dataType: "json",
        async: false,
        success: function (data) {
            if (data.length > 0) {
                roleList = "";
                roleList = data;
            }
            else {
                roleList = "";
                roleList = data;
               // hg.msg('该角色还未设置权限！');
                // $.messageBox('系统提示', "该角色还未设置权限！");
            }
        }
    });
}

//权限
function loadRolePermissionDatagrid() {
    var data;
    var checkdata;
    $.ajax({
        type: "post",
        url: '/RolePermission/GetRolesTreeTableData',
        dataType: "json",
        async: false,
        success: function (d) {
            data = eval(d.data);
            checkdata = eval(roleList);
        }
    });


    layui.config({
        base: '../module/treeTable/',
    })
    layui.use(['treeTable'], function () {

        var form = layui.form,
            layer = layui.layer;
        treeTable = layui.treeTable;
        // 直接下载后url: './data/table-tree.json',这个配置可能看不到数据，改为data:[],获取自己的实际链接返回json数组
        re = treeTable.render({
            elem: '#userrolemodule',
            data: data,
            icon_key: 'NodeName',
            top_value: 0,
            is_checkbox: true,
            primary_key: 'id',
            parent_key: 'pid', 
            hide_class: 'layui-hide',
            icon: {
                open: 'layui-icon layui-icon-triangle-d',
                close: 'layui-icon layui-icon-triangle-r',
                left: 16,
            },
          
            cols: [
                {
                    key: 'NodeName',
                    title: '名称',
                    width: '100px',
                    template: function (item) {
                        if (item.level == 0) {
                            return '<span style="color:Navy;">' + item.NodeName + '</span>';
                        } else if (item.level == 1) {
                            return '<span style="color:#20B2AA;">' + item.NodeName + '</span>';
                        } else if (item.level == 2) {
                            return '<span style="color:#8B658B;">' + item.NodeName + '</span>';
                        }
                    }
                },
                {
                    key: 'IsEnabled',
                    title: '是否启用',
                    width: '100px',
                    align: 'center',
                    template: function (item) {
                        if (item.IsEnabled == 1) {
                            return "<span style='color:green;'>启用</span>";
                        } else {
                            return "<span style='color:red;'>禁用</span>";
                        }
                    }
                },
                {
                    key: 'Type',
                    title: '类型',
                    width: '100px',
                    align: 'center',
                    template: function (item) {
                        if (item.Type == "模块") {
                            return "<span  style='color:Navy;'>模块</span>";
                        } else if (item.Type == "页面") {
                            return "<span style='color:#20B2AA;'>页面</span>";
                        } else {
                            return "<span style='color:#8B658B;'>按钮</span>";
                        }
                    }
                }
                ,
                {
                    key: 'NodeType',
                    title: '平台',
                    width: '100px',
                    align: 'center'

                }
                //{
                //	key: 'id',
                //	title: 'ID',
                //	width: '100px',
                //	align: 'center',
                //},
                //{
                //	key: 'pid',
                //	title: '父ID',
                //	width: '100px',
                //	align: 'center',
                //},

            ],
            checked: {
                key: 'id',
                data: checkdata,
            },
            is_click_icon: false,            
            is_cache: true,
            end: function (e) {
               form.render();
            },
        });




        // //获取选中值，返回值是一个数组（定义的primary_key参数集合）
        //o('.get-checked').click(function () {
        //    layer.msg('选中参数' + treeTable.checked(re).join(','))
        //})
        // //刷新重载树表（一般在异步处理数据后刷新显示）
        //o('.refresh').click(function () {
        //    re.data.push({ "id": 50, "pid": 0, "title": "1-4" }, { "id": 51, "pid": 50, "title": "1-4-1" });
        //    treeTable.render(re);
        //})
    })
}




