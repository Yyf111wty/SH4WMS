var flag = 0;
var Id = 0;
$(function () {
    //动态加载按钮
    //var jsonData = JSON.parse(window.name); 
    var pageId = GetQueryString('pageId');
    loadGroupFormButton(pageId);
    //加载tree跟datagrid
    loadTreeData();
    loadDatagrid(0);  
    //OpenForm();
    //$('#btnCancel').click(function () { CloseForm(); });
    //$('#GroupForm').window({
    //    onBeforeClose: function () {
    //        window.location.reload();
    //    }
    //});
    //$('#btnConfirm').click(function () {
    //    if (flag == 1) {
    //        saveGroupAction(0);
    //    }
    //    else {
    //        saveGroupAction(Id);
    //    }
    //})

});
function GetQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return (r[2]);
    return null;
}
//新增
function addClick() {

    hg.open('新增', '/Groups/AddOrEdit?type=0', 600, 400);
}
//修改
function editClick() {
   
    //obj通过行tool修改-lay-event
    var row = hg.table.datatable('test', 'getSelected');   
    if (row) {
        var datas = "&Id=" + row.data.Id + "&GroupName=" + (row.data.GroupName) + "&parentGroupId=" + row.data.ParentGroupId + "&Describes=" + (row.data.Describes) + "&IsEnabled=" + row.data.IsEnabled;
        hg.open('编辑', '/Groups/AddOrEdit?type=1' + datas, 600, 400);
    } else {
        hg.msg('请选中一行数据，再操作！');
    }
}
//删除
function deleteClick(){
    //obj通过行tool删除-lay-event
    var row = hg.table.datatable('test', 'getSelected');  
    if (row) {
        hg.confirm("删除当前选中的数据，确定要删除吗？", function () {
            //row.del(); //只删本地数据
            $.ajax({
                async: false,
                type: "POST",
                url: '/Groups/DeleteGroups',
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


//监听switch操作事件
//hg.form.onswitch('IsEnabled',function(data){
//    hg.msg(data.elem.name+'开关checked：'+(data.elem.checked===true?'开启':'禁用'));

//});
//监听表单提交事件
hg.form.onsubmit('sreach', function (data) {
    var start = data.start;
    var end = data.end;
    if (start && end) {
        if (start > end) {
            hg.msg("时间范围错误，请重新选择!");
            return false;
        }
    } else if (start || end) {
        hg.msg("时间范围错误，请重新选择!");
        return false;
    }
    hg.msg(JSON.stringify(data));
    table && table.reload(data);
});


//加载tree
function loadTreeData() {

    //var data = [{ "id": "1", "title": "父节点同日工业自动化有限公司" }];
    // var data = JSON.stringify(getTreeJson(null)).replace('[', '').replace(']', '');//返回新字符串
    var data = eval(getTreeJson());
    //var data = JSON.stringify(getTreeJson());
    //treeid
    var tree = hg.tree.datatree('treeid', data, false);
    //tree.onspread = function () {
    //    var obj = this;        
    //    var data = eval(getTreeJson()) ; //异步获取数据
    //        obj.lazytree(data);      
    //}
    tree.onclick = function () {
        var obj = this;
        //layer.alert(obj.data);

        var id = obj.data.id;
        //var d = obj.data.children;
        loadDatagrid(id);
        //console.log(obj.data); //得到当前点击的节点数据
        //console.log(obj.state); //得到当前节点的展开状态：open、close、normal
        //console.log(obj.elem); //得到当前节点元素
        //console.log(obj.data.children); //当前节点下是否有子节点
    }
}
   

    // 后台返回数据
    function getTreeJson() {
      
        var treedata;
        $.ajax({
            type: "get",
            async: false,//同步执行
            url: "/Groups/GetDatatreeData",
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
function loadDatagrid(selectid) {

    var tableCols = [[

        { field: 'Id', title: 'Id', align: 'center', sort: true, hide: true },
        { field: 'GroupName', title: '组架名称', align: 'center',  sort: true },
        { field: 'ParentGroupName', title: '所属组架', align: 'center',  sort: true },
        {
            field: 'IsEnabled', title: '是否启用', templet: function (d) {
                var checked = d.IsEnabled ==1? 'checked' : '';
                return '<input type="checkbox" name ="' + d.id + '" value= ' + d.IsEnabled +
                    ' lay-skin="switch" lay-text="开启|禁用"  disabled="disabled" lay-filter="IsEnabled" ' + checked + '>'
            }
        },
        { field: 'Describes', title: '描述', align: 'center',  sort: true },
        { field: 'CreateOperator', title: '创建人', align: 'center',  sort: true },
        {
            field: 'CreateTime', title: '创建时间', align: 'center', sort: true, templet:
                function (d) {
                    return FormatToDate(d.CreateTime);
                }}

    ]
    ];
   //基本数据表格
    var table = hg.table.datatable('test', '用户管理', '/Groups/GetGroupsList?selectid=' + selectid, {}, tableCols, '', true, 'full-255');
    //table.onrowclick(function (obj) {

    //});
    //setfilter();
}

