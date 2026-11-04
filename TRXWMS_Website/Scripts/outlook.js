var _menus;
$(function () {
    InitLeftMenu();
  
})

//初始化左侧
function InitLeftMenu() {   
    $.ajax({
        url: "/Home/GetPermissionData",
        type: "get",
        dataType: "json",
        success: function (result) {
            _menus = result;
            $.each(_menus.menus, function (i, n) {
                var menulist = '';
                menulist += "<li class='open'>";
                $.each(n.menus, function (j, o) {
                    menulist += '<a class="border active"> <i class="iconfont">&#xe623;</i><cite>' + o.menuname + '</cite><i class="iconfont nav_right">&#xe609;</i></a> ';
                })
                menulist += '<ul class="sub-menu" style="display: block;">';
                $('#nav').accordion('add', {
                    title: n.menuname,
                    content: menulist,
                    iconCls: 'icon ' + n.icon
                });
            });
            $('.easyui-accordion li a').click(function () {
                var tabTitle = $(this).children('.nav').text();
                var menuid = $(this).attr("ref");
                var url = $(this).attr("rel") +"?pageId="+menuid+"";
                var icon = getIcon(menuid, icon);

                addTab(tabTitle, url, icon);
                $('.easyui-accordion li div').removeClass("selected");
                $(this).parent().addClass("selected");
            }).hover(function () {
                $(this).parent().addClass("hover");
            }, function () {
                $(this).parent().removeClass("hover");
            });
            //选中第一个
            var panels = $('#nav').accordion('panels');
            var t = panels[0].panel('options').title;
            $('#nav').accordion('select', t);
            
        },
        error: function (message) {
            $.messageBox5s('系统提示', message); return;
        }
    })
}
