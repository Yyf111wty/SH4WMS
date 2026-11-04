
var treeTabledata;
$(function () {
    var pageId = GetQueryString('pageId');
    loadGroupFormButton(pageId);
    //初始化数据
    loadDatagrid("");
    loadRolePermissionDatagrid();
    $("#selectrow").val("");

});


layui.config({
    base: '../module/treeTable/',
})
layui.use(['treeTable', 'layer', 'code', 'form'], function () {

    var form = layui.form,
        layer = layui.layer,
        treeTable = layui.treeTable;
    // 直接下载后url: './data/table-tree.json',这个配置可能看不到数据，改为data:[],获取自己的实际链接返回json数组
    var re = treeTable.render({
        elem: '#userrolemodule',
        data: treeTabledata,
        icon_key: 'NodeName',
        is_checkbox: false,
        end: function (e) {
            form.render();
        },
        cols: [
            {
                key: 'NodeName',
                title: '名称',
                width: '120px',
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
                width: '90px',
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
                width: '80px',
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
            ,
            {
                key: 'Linkurl',
                title: '链接地址',
                width: '130px',
                align: 'center'
            }
            ,
            {
                key: 'ControllerName',
                title: '控制器',
                width: '130px',
                align: 'center'
            }
            ,
            {
                key: 'ActionName',
                title: '动作',
                width: '90px',
                align: 'center'
            }
            ,
            {
                key: 'SortNo',
                title: '排序',
                width: '90px',
                align: 'center'
               
            } , 
              {
                  key: 'rowid',
                  title: 'ID',
                width: '90px',
                align: 'center'
               
            }             
            //,
            //{
            //    key: 'ParentName',
            //    title: '所属父级名称',
            //    width: '90px',
            //    align: 'center',
            //    hidden: "hidden"
            //}              
        ]
});
//绑定行点击事件
var mydd = $('tbody  tr');
mydd.each(function (i) {
    $(this).click(function () {
        var id = $(this).attr("data-id");
        var pid = $(this).attr("data-pid");         
        //移除所有的背景色  
        mydd.css("background-color",''); 
        $(this).css('background-color', 'lightyellow');
        //隐藏域存取值
        $("#selectrow").val("");
        var selectrowdata = id+",";
        $(this).children('td').each(function (j) {  // 遍历 tr 的各个 td          
           
            if (j == 0) {
               // selectrowdata += "&NodeName='" + $(this).children('span:last').text() + "'";
                selectrowdata += "" + $(this).children('span:last').text() + ",";
            }
            if (j == 1) {
                if ($(this).children('span:last').text() == "启用") {
                    selectrowdata += "1,";
                } else {
                    selectrowdata += "0,";
                }
              
            }
            if (j == 2) {
               // selectrowdata += "&Type='" + $(this).children('span:last').text()+"'";
                selectrowdata += "" + $(this).children('span:last').text() + ",";
            }
            if (j == 3) {
               // selectrowdata += "&NodeType='" + $(this).text()+"'";
                selectrowdata += "" + $(this).text() + ",";
            }
            if (j == 4) {
                //selectrowdata += "&Linkurl='" + $(this).text() + "'";
                selectrowdata += "" + $(this).text() + ",";
            }
            if (j == 5) {
                //selectrowdata += "&ControllerName='" + $(this).text() + "'";
                selectrowdata += "" + $(this).text() + ",";
            }
            if (j == 6) {
               // selectrowdata += "&ActionName='" + $(this).text() + "'";
                selectrowdata += "" + $(this).text() + ",";
            } 
            if (j == 7) {
               // selectrowdata += "&SortNo=" + $(this).text();
                selectrowdata +=  $(this).text()+",";
            }  
            if (j == 8) {
                // selectrowdata += "&SortNo=" + $(this).text();
                selectrowdata += $(this).text()+",";
            }  
        });
        selectrowdata += pid;
        $("#selectrow").val(selectrowdata);
        loadDatagrid(id);
     
    })

});  
})

    // 上移
function positionupClick() {
  
    if ($("#selectrow").val() == "") { hg.msg('请选择权限模块！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];
    $.ajax({
        type: "POST",
        url: '/Permission/PositionUp',
        data: { Id: rowid, Type: Type, SortNo: SortNo, _parentId: pid },
        dataType: "json",
        success: function (data) {
            if (data.result == true) {
               // $.messageBox('系统提示', data.message);
                hg.msg(data.message);
                setTimeout(function () { window.location.reload(); }, 1000);
            }
            else {
               // $.messageBox('系统提示', data.message);
                hg.msg(data.message);
            }
        },
        error: function (data) {
            //$.messageBox('系统提示', data.message);
            hg.msg(data.message);
        }
    })
}

//下移
function positiondownClick() {
    if ($("#selectrow").val() == "") { hg.msg('请选择权限模块！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];
    $.ajax({
        type: "POST",
        url: '/Permission/PositionDown',
        data: { Id: rowid, Type: Type, SortNo: SortNo, _parentId: pid },
        dataType: "json",
        success: function (data) {
            if (data.result == true) {
                hg.msg(data.message);
                setTimeout(function () { window.location.reload(); }, 1000);
            }
            else {
                hg.msg(data.message);
            }
        },
        error: function (data) {
            hg.msg(data.message);
        }
    })
}


//打开新增模块窗口
function addmodularClick() {
    hg.open('新增模块', '/Permission/ModularAddOrEdit?typeflag=0', 600, 400);   

}
//打开新增页面窗口
function addpageClick() {
    //新增页面
    var selectdata = $("#selectrow").val();
    if (selectdata== "") { hg.msg('请选择模块！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];
    if (Type != "模块") {
        hg.msg('请选择模块！'); return;
    }   
    var datas = "&ParentId=" + id + "&ParentName=" + NodeName + "&NodeType=" + NodeType ;
    hg.open('新增页面', '/Permission/PageAddOrEdit?typeflag=0' + datas, 600, 500);
    

}

//打开新增按钮窗口
function addbtnClick() {
    //新增按钮
    var selectdata = $("#selectrow").val();
    if (selectdata == "") { hg.msg('请选择页面！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];
    if (Type != "页面") {
        hg.msg('请选择页面！'); return;
    }
    var datas = "&parentId=" + id + "&ParentName=" + NodeName + "&NodeType=" + NodeType;
    hg.open('新增按钮', '/Permission/ButtonAddOrEdit?typeflag=0' + datas, 600, 400);
}

//打开编辑窗口
function editClick() {
    //编辑模块、页面
    if ($("#selectrow").val() == "") { hg.msg('请选择权限模块！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];   
    if (Type== "模块") {      
        var datas = "&Id=" + id + "&NodeType=" + NodeType + "&IsEnabled=" + IsEnabled + "&NodeName=" + NodeName;
        hg.open('编辑模块', '/Permission/ModularAddOrEdit?typeflag=1' + datas, 600, 400);
       
    }
    else {
        var datas = "&id=" + id + "&NodeType=" + NodeType+ "&pageName=" + NodeName + "&linkurl=" + Linkurl + "&controller=" + ControllerName + "&action=" + ActionName + "&IsEnabled=" + IsEnabled ;       
        hg.open('编辑页面', '/Permission/PageAddOrEdit?typeflag=1' + datas, 600, 500);
    }
}

//打开编辑按钮窗口
function editbtnClick() {
    //编辑按钮
    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        var str = $("#selectrow").val().split(',');
        var id = str[0];
        var NodeName = str[1];
        var IsEnabled = str[2];
        var Type = str[3];
        var NodeType = str[4];
        var Linkurl = str[5];
        var ControllerName = str[6];
        var ActionName = str[7];
        var SortNo = str[8];
        var rowid = str[9];
        var pid = str[10];

        var datas = "&id=" + row.data.NodeId + "&NodeType=" + NodeType+ "&ParentId=" + id + "&NodeName=" + row.data.NodeName + "&KeyName=" + row.data.KeyName   + "&IsEnabled=" + IsEnabled;
        hg.open('编辑按钮', '/Permission/ButtonAddOrEdit?typeflag=1' + datas, 600, 400);
    } else {
        hg.msg('请选中一行数据，再操作！');
    }
}

//删除模块或页面
function deleteClick() {
    if ($("#selectrow").val() == "") { hg.msg('请选择权限模块！'); return; }
    var str = $("#selectrow").val().split(',');
    var id = str[0];
    var NodeName = str[1];
    var IsEnabled = str[2];
    var Type = str[3];
    var NodeType = str[4];
    var Linkurl = str[5];
    var ControllerName = str[6];
    var ActionName = str[7];
    var SortNo = str[8];
    var rowid = str[9];
    var pid = str[10];
    hg.confirm("当前选中的数据，确定要删除吗？", function () {
        $.ajax({
            async: false,
            type: "POST",
            url: '/Permission/Delete',
            data: { id: rowid },
            dataType: "json",
            success: function (data) {
                if (data.result == true) {
                    hg.msg(data.message);
                    setTimeout(function () { window.location.reload(); }, 500);

                }
                else {
                    hg.msg(data.message);
                }
            },
            error: function (data) {
                hg.msg(data.message);
            }
        })

    })
}
//删除按钮
function deletebtnClick() {
    var row = hg.table.datatable('test', 'getSelected');
    if (row) {
        hg.confirm("删除当前选中的数据，确定要删除吗？", function () {
            //row.del(); //只删本地数据
            $.ajax({
                async: false,
                type: "POST",
                url: '/Permission/Delete',
                data: { id: row.data.Id },
                dataType: "json",
                success: function (data) {
                    if (data.result == true) {
                        hg.msg(data.message);
                        //table && table.reload(); 
                        setTimeout(function () { window.location.reload(); }, 500);

                    }
                    else {
                        hg.msg(data.message);
                       // setTimeout(function () { window.location.reload(); }, 500);

                        // $.messageBox('系统提示', );
                    }

                },
                error: function (data) {
                    hg.msg(data.message);
                    //setTimeout(function () { window.location.reload(); }, 500);

                    // $.messageBox('系统提示', data.message);
                }
            })


        });
    } else {
        hg.msg('请选中一行数据，再操作！');
    }
   
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
function loadDatagrid(id) {

    var tableCols = [[

        { field: 'Id', title: 'Id', align: 'center',  sort: true, hide: true },
        { field: 'NodeId', title: 'NodeId', align: 'center',  sort: true, hide: true },
        { field: 'NodeName', title: '名称', align: 'center',  sort: true },
        {
            field: 'IsEnabled', title: '是否启用', align: 'center',  sort: true, templet: function (d) {
                if (d.IsEnabled == 1) {
                    return "<span style='color:green;'>启用</span>";
                } else {
                    return "<span style='color:red;'>禁用</span>";
                }
            }
        },
        {
            field: 'Type', title: '类型', align: 'center',sort: true, templet: function (d) {
                return "<span style='color:green;'>按钮</span>";
            }
        },
        { field: 'NodeType', title: '平台', align: 'center',  sort: true }
        ,
        { field: 'KeyName', title: '关键字', align: 'center',  sort: true }
        ,
        { field: 'SortNo', title: '排序', align: 'center',  sort: true }

    ]
    ];
    if (id == "") {
        //基本数据表格
        hg.table.datatable('test', '包含按钮', '', {}, tableCols, '', true, 'full-255');

    }
    else {
        //基本数据表格
        hg.table.datatable('test', '包含按钮', '/Permission/GetModularButtonList?typeFlag=2&&parentid=' + id, {}, tableCols, '', true, 'full-255');

    }

}

//权限
function loadRolePermissionDatagrid() {
    $.ajax({
        type: "post",
        url: "/Permission/GetModularList?typeFlag=0",
        dataType: "json",
        async: false,
        success: function (d) {
            treeTabledata = eval(d.data);

        }
    });



}


//加载按钮
function loadGroupFormButton(pageId) {

    var MyDiv = document.getElementById("group_div");
    var MyDiv1 = document.getElementById("group_div1");
    $.ajax({
        type: "get",
        url: "/Home/GetPermissionBtn",
        dataType: "json",
        data: { pageId: pageId },
        success: function (result) {
            if (result.length == 0) { return; }
            for (var i = 0; i < result.length; i++) {
                if (result[i].NodeName == "新增按钮" || result[i].NodeName == "编辑按钮" || result[i].NodeName == "删除按钮") {
                    var str = result[i].NodeName;
                    var button = "";
                    if (str.search("新增") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe654;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("编辑") != -1 || str.search("修改") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe642;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("删除") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe640;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("查询") != -1 || str.search("查看") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe615;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("设置角色") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe613;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("重置密码") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe9aa;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("上移") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe619;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("下移") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe61a;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("保存") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe653;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("设置生效") != -1 || str.search("设置失效") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe716;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("复制版本") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe655;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("推行使用") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe614;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("设置连续工艺") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe614;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("下发") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe625;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("派工") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe68d;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("指派设备和人员") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe613;</i>' + result[i].NodeName + '</button> ';
                    }
                    else {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#x1005;</i>' + result[i].NodeName + '</button> ';
                    }
                    MyDiv1.innerHTML += button;
                }
                else {
                    var str = result[i].NodeName;
                    var button = "";
                    if (str.search("新增") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe654;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("编辑") != -1 || str.search("修改") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe642;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("删除") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe640;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("查询") != -1 || str.search("查看") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe615;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("设置角色") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe613;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("重置密码") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe9aa;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("上移") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe619;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("下移") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe61a;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("保存") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe653;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("设置生效") != -1 || str.search("设置失效") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe716;</i>' + result[i].NodeName + '</button> ';
                    } else if (str.search("复制版本") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe655;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("推行使用") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe614;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("设置连续工艺") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe614;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("下发") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe625;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("派工") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe68d;</i>' + result[i].NodeName + '</button> ';
                    }
                    else if (str.search("指派设备和人员") != -1) {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe613;</i>' + result[i].NodeName + '</button> ';
                    }
                    else {
                        button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#x1005;</i>' + result[i].NodeName + '</button> ';
                    }
                    //var button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">' + result[i].NodeName + '</i></button> ';
                    MyDiv.innerHTML += button;
                }
            }
        },
        error: function (message) {
            layer.alert(message, { icon: 6 }); return;
        }
    })
}



