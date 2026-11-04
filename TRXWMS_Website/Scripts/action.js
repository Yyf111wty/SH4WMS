$(function () {
    var Timing;   //扫描物料计时
    var flag = false;    //是否已启动计时标识
    var isLock = false; //是否已经锁定工序
    var StepList = [];    //当前产品工序列表，包含物料、工艺参数列表
    var current = 0; //当前工序计数
    var scanArray = [];//记录已扫过的条码，做重复过滤
    var selectedStepData = [];//当前锁定的工序
    var lock = document.getElementById("lock");
    var unlock = document.getElementById("unlock");
    var ProductCode = document.getElementById("ProductCode");
    var orderID = document.getElementById("OrderID");
    var planStartDate = document.getElementById("PlanStartDate");
    var planTotalQty = document.getElementById("PlanTotalQty");
    var orderStatus = document.getElementById("OrderStatus");
    var okQty = document.getElementById("OkQty");
    var ngQty = document.getElementById("NgQty");
    var planEndDate = document.getElementById("PlanEndDate");
    var productType = document.getElementById("ProductType");
    var surplusQty = document.getElementById("SurplusQty");
    var repairQty = document.getElementById("RepairQty");
    var sopurl = document.getElementById("sopurl");
    var parent = document.getElementById("parent");
    var parameterView = document.getElementById("parameterView");
    var MaterialData = [];  //采集的物料条码集
    var PassStepData = [];//工序过账基础数据集
    var Ispostcount = 0;  //该产品下工序过账个数
    var stepId = [];//上线打码工序
    var user = $("#username").val();   //登录账户
    var lineid = $("#lineid").val();   //线别id
    var machineid = $("#machineid").val();   //工位id
    $("#ProductCode").focus();
    if (machineid == undefined || machineid =="") {
        layer.alert('登录用户未绑定工位或不是该工位绑定的账号，无法作业', {
            skin: 'layui-layer-molv'
            , anim: 1
            , btn: ['确定']
            , icon: 2
            , btn1: function (index) {
                layer.close(index);
                document.getElementById("ProductCode").style.display = "none";
                document.getElementById("Release").style.display = "none";
                document.getElementById("lock").style.display = "none";
                document.getElementById("unlock").style.display = "none";
                document.getElementById("pass").style.display = "none";
                document.getElementById("Partcode").style.display = "none";
                document.getElementById("devicework").style.display = "none";
                document.getElementById("ngcheck").style.display = "none";
                document.getElementById("QC").style.display = "none";
                document.getElementById("Repair").style.display = "none";
            }
        });
        return;
    }
    //页面初始化
    initform(machineid);
    $('#fullScreen').bind('click', function () {
        var fullscreenElement = document.fullscreenElement || document.mozFullScreenElement || document.webkitFullscreenElement;
        if (fullscreenElement == null) {
            entryFullScreen();
            $(this).html('<i class="layui-icon layui-icon-screen-restore" style="color: #eeeeee;">全屏切换</i>');
            $("#ProductCode").focus();
            //document.getElementById("title").style.display = "";
        } else {
            exitFullScreen();
            $(this).html('<i class="layui-icon layui-icon-screen-full" style=" color: #eeeeee;">全屏切换</i>');
            //document.getElementById("title").style.display = "none";
        }
    })
    //轮询读取消息提示
    getMessage(machineid);
    //如果工位是上线打码，就获取钢印号
    if (machineid == "1001" || machineid == "1000" || machineid=="1100") {
        getSNPrint(ProductCode, machineid, lineid, orderID, productType, planStartDate, planEndDate, orderStatus, parent,
            planTotalQty, okQty, ngQty, surplusQty, repairQty, sopurl, stepId, parameterView, StepList);
        isLock = true;
        $("#Partcode").focus();
    }
    //放行
    $("#Release").on('click', function () {
        if (ProductCode.value == "" || orderID.innerHTML == "") {
            layer.alert('未获取到钢印号，无法放行', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (orderStatus.innerHTML == "不合格") {
            layer.alert('该产品是不良品，不可放行', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (productType.innerHTML == "返修") {
            layer.alert('此产品是返修品，是否需要重新打印标签？', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 3
                , btn1: function (index) {
                    layer.close(index);
                    $.ajax({
                        type: "get",
                        url: "/Action/RePrint",
                        data: { Sn: ProductCode.value, MachineId: machineid },
                        dataType: "json",
                        success: function (result) {
                            var jsonresult = JSON.parse(result);
                            var status = jsonresult.success;
                            if (status == false) {
                                layer.msg("打印失败", { icon: 2 });
                                return;
                            }
                            window.location.reload();
                        }
                    })
                }
            });
        }
        else {
            $.ajax({
                type: "get",
                url: "/Action/SNPass",
                data: { Sn: ProductCode.value, WorkId: orderID.innerHTML, LineId: lineid, MachineId: machineid, StepId: stepId[0], Emp: user },
                dataType: "json",
                beforeSend: function () {
                    loading = layer.load(0, {
                        shadeClose: false,
                        title: '加载中..',
                        shade: [0.3, '#fff'] //0.1透明度的白色背景
                    });
                },
                success: function (result) {
                    layer.close(loading);
                    var jsonresult = JSON.parse(result);
                    var status = jsonresult.success;
                    if (status == false) {
                        layer.alert(jsonresult.msg, {
                            skin: 'layui-layer-molv'
                            , anim: 1
                            , btn: ['确定']
                            , icon: 2
                            , btn1: function (index) {
                                layer.close(index);
                                window.location.reload();
                               
                            }
                        });
                        return;
                    }
                    window.location.reload();
                },
                error: function (message) {
                    layer.msg(message.statusText, { icon: 2 });
                }
            })
        }
    })
    //扫描产品码
    $("#ProductCode").keyup(function () {
        if (event.keyCode == 13) {
            if (ProductCode.value == "") {
                return;
            }
            else {
                isLock = false;
                StepList = [];
                selectedStepData = [];
                Ispostcount = 0;
                current = 0;
                scanArray = [];
                clearInterval(Timing);
                flag = false;
                orderID.innerHTML = "";
                planStartDate.innerHTML = "";
                planTotalQty.innerHTML = "";
                okQty.innerHTML = "";
                ngQty.innerHTML = "";
                planEndDate.innerHTML = "";
                productType.innerHTML = "";
                surplusQty.innerHTML = "";
                repairQty.innerHTML = "";
                orderStatus.innerHTML = "";
                sopurl.src = "";
                parent.innerHTML = "";
                parameterView.innerHTML = "";
                $("#Partcode").removeAttr("disabled");
                $.ajax({
                    type: "get",
                    url: "/Action/GetOrderList",
                    dataType: "json",
                    data: { Sn: ProductCode.value, MachineId: machineid, LineId: lineid },
                    beforeSend: function () {
                        loading = layer.load(0, {
                            shadeClose: false,
                            title: '加载中..',
                            shade: [0.3, '#fff'] //0.1透明度的白色背景
                        });
                    },
                    success: function (result) {
                        layer.close(loading);
                        var jsonresult = JSON.parse(result);
                        var status = jsonresult.success;
                        if (status == false) {
                            layer.alert(jsonresult.msg, {
                                skin: 'layui-layer-molv'
                                , anim: 1
                                , btn: ['确定']
                                , icon: 2
                                , btn1: function (index) {
                                    layer.close(index);
                                    orderID.innerHTML = "";
                                    planStartDate.innerHTML = "";
                                    planTotalQty.innerHTML = "";
                                    okQty.innerHTML = "";
                                    ngQty.innerHTML = "";
                                    orderStatus.innerHTML = "";
                                    planEndDate.innerHTML = "";
                                    productType.innerHTML = "";
                                    surplusQty.innerHTML = "";
                                    repairQty.innerHTML = "";
                                    sopurl.src = "";
                                    $("#ProductCode").val("");
                                    $("#Partcode").attr("disabled", true);
                                    $("#ProductCode").focus();
                                }
                            });
                            return;
                        }
                        var results = jsonresult.data[0];
                        if (results.OrderStatus == "不合格") {
                            layer.msg('该产品标记为不良，无法作业', { icon: 2 });
                            $("#Partcode").attr("disabled", true);
                        }
                        orderID.innerHTML = "" + results.OrderID + "";
                        planStartDate.innerHTML = "" + results.PlanStartDate + "";
                        planTotalQty.innerHTML = "";
                        okQty.innerHTML = "";
                        ngQty.innerHTML = "";
                        planEndDate.innerHTML = "" + results.PlanEndDate + "";
                        productType.innerHTML = "" + results.ProductType + "";
                        orderStatus.innerHTML = "" + results.OrderStatus + "";
                        surplusQty.innerHTML = "";
                        repairQty.innerHTML = "";
                        for (var i = 0; i < results.StepList.length; i++) {
                            StepList.push(results.StepList[i]);
                            //动态创建layui table
                            var timediv = document.createElement("div");
                            timediv.style = "height:15px;width:100%";
                            var label = document.createElement("lable");
                            label.setAttribute("id", "time" + results.StepList[i].StepID);
                            label.innerHTML = results.StepList[i].PassTime;
                            label.className = "time-lable";
                            timediv.appendChild(label);
                            var label = document.createElement("lable");
                            label.className = "time";
                            label.innerHTML = "已用时间：";
                            timediv.appendChild(label);
                            parent.appendChild(timediv);
                            var card = document.createElement("div");
                            card.className = "process-card";
                            card.setAttribute("id", results.StepList[i].StepID);
                            var title = document.createElement("div");
                            title.style = "height:20px;width:100%";
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                            title.appendChild(span);
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                            title.appendChild(span);
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.setAttribute("id", "Status" + results.StepList[i].StepID);
                            span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                            title.appendChild(span);
                            if (results.StepList[i].StepStatus == "不合格") {
                                var a = document.createElement("a");
                                a.className = "tabel-title";
                                a.setAttribute("id", "Check" + results.StepList[i].StepID);
                                a.innerHTML = "质检结果:" + results.StepList[i].StepStatus;
                                a.href = "javascript:void(0)";
                                a.style = "text-decoration:underline;color:#ff6a00";
                                a.addEventListener('dblclick', function (element) {
                                    layer.open({
                                        type: 2,
                                        title: ['质检结果', 'color:#fff;background-color:#01AAED;'],
                                        shadeClose: true,
                                        shade: 0.4,
                                        area: ['40%', '60%'],
                                        content: "/MES/Action/QCReview?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML + "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + results.StepList[i].StepID + "",
                                        cancel: function () {
                                            layer.close(layer.index);
                                        }
                                    })
                                });
                                title.appendChild(a);
                            }
                            else {
                                var span = document.createElement("span");
                                span.className = "tabel-title";
                                span.setAttribute("id", "Check" + results.StepList[i].StepID);
                                span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                                title.appendChild(span);
                            }
                            card.appendChild(title);
                            var table = document.createElement("table");
                            table.className = "layui-table";
                            table.setAttribute("id", "table" + results.StepList[i].StepID);
                            table.style = "width:98%; margin-left:7px;margin-right:9px;";
                            var thead = document.createElement("thead");
                            table.appendChild(thead);
                            var tr = document.createElement("tr");
                            thead.appendChild(tr);
                            var th = document.createElement("th");
                            th.innerHTML = "序号";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "材料编号";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "材料名称及规格";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "需求数量";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "完成数量";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var tbody = document.createElement("tbody");
                            table.appendChild(tbody);
                            var partData = results.StepList[i].PartList;
                            for (var j = 0; j < partData.length; j++) {
                                var tr = document.createElement("tr");
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].No;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                var a = document.createElement("a");
                                a.innerHTML = partData[j].Material_No;
                                a.style.color = "#ff6a00";
                                a.href = "javascript:void(0);";
                                a.setAttribute("id", partData[j].Material_ImgUrl);
                                a.addEventListener('dblclick', function (element) {
                                    var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                    //弹出层
                                    layer.open({
                                        type: 1,
                                        shade: 0.8,
                                        offset: 'auto',
                                        area: [1000 + 'px', 850 + 'px'],
                                        shadeClose: true,
                                        scrollbar: false,
                                        title: "物料图片预览",
                                        content: imgHtml,
                                        cancel: function () {
                                        }
                                    });
                                });
                                td.appendChild(a);
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Material_Name;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Need_Qty;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Finish_Qty;
                                tr.appendChild(td);
                                tbody.appendChild(tr);
                            }
                            card.appendChild(table);
                            parent.appendChild(card);
                            var hr = document.createElement("hr");
                            hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                            parent.appendChild(hr);
                            if (results.StepList[i].StepIsPost == "已过账") {
                                document.getElementById("table" + results.StepList[i].StepID).style.backgroundColor = "#7bfa67";
                            }
                        }
                        if (isRepeat(StepList)) {
                            lock.style.display = "";
                            unlock.style.display = "";
                        } else {
                            lock.style.display = "none";
                            unlock.style.display = "none";
                        }
                        for (var i = 0; i < StepList.length; i++) {
                            if (StepList[i].StepIsPost == "已过账") {
                                Ispostcount = Ispostcount + 1;
                            }
                        }
                        if (Ispostcount == StepList.length) {
                            $("#Partcode").attr("disabled", true);
                            isLock = true;
                            return;
                        }
                        //判断是否有同等级别的工序
                        if (isRepeat(StepList)) {
                            for (var i = 0; i < StepList.length; i++) {
                                if (StepList[i].isLock == "Y") {
                                    document.getElementById("" + StepList[i].StepID).style.backgroundColor = "#52bbf7";
                                    selectedStepData = StepList[i];
                                    planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                    okQty.innerHTML = selectedStepData.OkQty;
                                    ngQty.innerHTML = selectedStepData.NgQty
                                    surplusQty.innerHTML = selectedStepData.SurplusQty;
                                    repairQty.innerHTML = selectedStepData.RepairQty;
                                    sopurl.src = selectedStepData.SopUrl;
                                    isLock = true;
                                    loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                    break;
                                }
                            }
                        }
                        else {
                            for (var i = 0; i < StepList.length; i++) {
                                if (StepList[i].StepIsPost == "未过账") {
                                    selectedStepData = StepList[i];
                                    current = i;
                                    break;
                                }
                            }
                            document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                            planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                            okQty.innerHTML = selectedStepData.OkQty;
                            ngQty.innerHTML = selectedStepData.NgQty
                            surplusQty.innerHTML = selectedStepData.SurplusQty;
                            repairQty.innerHTML = selectedStepData.RepairQty;
                            sopurl.src =selectedStepData.SopUrl;
                            isLock = true;
                            loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                        }
                    },
                    error: function (message) {
                        layer.close(loading);
                        layer.msg('请求服务器出现异常，检查登录是否超时' + message, { icon: 2 });
                    }
                })
                $("#Partcode").focus();
            }
        }
    })
    $("#parent").delegate("div", "click", function () {
        if (isLock) { return; }
        var id = $(this).attr("id");
        for (var i = 0; i < StepList.length; i++) {
            var item = StepList[i].StepID;
            if (item == id) {
                document.getElementById("" + id).style.backgroundColor = "#808080";
                selectedStepData = StepList[i];
            }
            else {
                document.getElementById("" + item).style.backgroundColor = "#DCDCDC";

            }
        }
    })
    //锁定工序
    $("#lock").on('click', function () {
        if (selectedStepData.length == 0) {
            layer.alert('请选定工序，进行锁定', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (document.getElementById("Status" + selectedStepData.StepID).innerHTML == "工序状态:已过账") {
            layer.alert('该工序已过账，无法锁定', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        $.ajax({
            type: "get",
            url: "/Action/StepLock",
            data: { Sn: ProductCode.value, WorkId: orderID.innerHTML, LineNo: lineid, MachineId: machineid, StepID: selectedStepData.StepID, Emp: user },
            dataType: "json",
            success: function (result) {
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == false) {
                    layer.alert(jsonresult.msg, {
                        skin: 'layui-layer-molv'
                        , anim: 1
                        , btn: ['确定']
                        , icon: 2
                        , btn1: function (index) {
                            layer.close(index);
                        }
                    });
                    return;
                }
                else {
                    document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                    $("#Partcode").val("");
                    $("#Partcode").focus();
                    isLock = true;
                    planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                    okQty.innerHTML = selectedStepData.OkQty;
                    ngQty.innerHTML = selectedStepData.NgQty
                    surplusQty.innerHTML = selectedStepData.SurplusQty;
                    repairQty.innerHTML = selectedStepData.RepairQty;
                    sopurl.src = selectedStepData.SopUrl;
                    loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                }
            },
            error: function (message) {
                layer.msg(message.statusText, { icon: 2 });
            }
        })
    })
    //解锁工序
    $("#unlock").on('click', function () {
        layer.open({
            title: '请输入组长密钥',
            //area: ['300px', '240px'],
            closeBtn: '1',//右上角的关闭
            content: '<div>  <input type="password" id="zzpwd" name="zzpwd" lay-verify="required" class="layui-input" autocomplete="off" style="width:250px;"></div>',
            btn: ['确认', '取消'],
            yes: function (index, layero) {
                var pwd = $('#zzpwd').val();
                $.ajax({
                    type: "get",
                    async: false,
                    url: "/Action/CheckSecretKey",
                    data: { secretKeyValue: pwd, secretKeyType: "组长" },
                    dataType: "json",
                    success: function (result) {
                        var jsonresult = JSON.parse(result);
                        var status = jsonresult.success;
                        if (status == false) {
                            layer.msg(jsonresult.msg, { icon: 2 });
                            return;
                        }
                        $.ajax({
                            type: "get",
                            url: "/Action/StepUnLock",
                            data: { Sn: ProductCode.value, WorkId: orderID.innerHTML, LineNo: lineid, MachineId: machineid, StepID: selectedStepData.StepID, Emp: pwd },
                            dataType: "json",
                            success: function (result) {
                                var jsonresult = JSON.parse(result);
                                var status = jsonresult.success;
                                if (status == false) {
                                    layer.alert(jsonresult.msg, {
                                        skin: 'layui-layer-molv'
                                        , anim: 1
                                        , btn: ['确定']
                                        , icon: 2
                                        , btn1: function (index) {
                                            layer.close(index);
                                        }
                                    });
                                    return;
                                }
                                else {
                                    scanArray = [];
                                    clearInterval(Timing);
                                    flag = false;
                                    document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#DCDCDC";
                                    var time = document.getElementById("time" + selectedStepData.StepID);
                                    time.innerHTML = "";
                                    for (var i = 1; i < selectedStepData.PartList.length + 1; i++) {
                                        document.getElementById("table" + selectedStepData.StepID).rows[i].style.backgroundColor = "";
                                        document.getElementById("table" + selectedStepData.StepID).rows[i].cells[4].innerHTML = "0";
                                    }
                                    isLock = false;
                                    planTotalQty.innerHTML = "";
                                    okQty.innerHTML = "";
                                    ngQty.innerHTML = "";
                                    surplusQty.innerHTML = "";
                                    repairQty.innerHTML = "";
                                    sopurl.src = "";
                                    selectedStepData = [];
                                    parameterView.innerHTML = "";
                                    $("#Partcode").val("");
                                    layer.close(index);
                                }
                            },
                            error: function (message) {
                                layer.msg(message.statusText, { icon: 2 });
                            }
                        })
                    },
                    error: function (message) {
                        layer.msg(message.statusText, { icon: 2 });
                    }
                })
            },
            no: function (index) {
                layer.close(index);
                return false;
            },
            close: function (index) {
                return false;
            }
        });
    })
    //采集物料条码
    $("#Partcode").keyup(function () {
        if (event.keyCode == 13) {   
            var code = document.getElementById("Partcode");
            if (StepList.length == 0) {
                layer.alert('请先扫描有效产品码', {
                    time: 3000
                    , skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        $("#Partcode").val("");
                        $("#ProductCode").focus();
                        layer.close(index);
                    }
                });
                return;
            }         
            if (machineid == "1001") {
                selectedStepData = StepList[0];
            }
            //判断是否有平级工序未锁定的
            if (isLock == false) {
                layer.alert('有平级工序，请选中工序后锁定', {
                    time: 3000
                    , skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        $("#Partcode").val("");
                        layer.close(index);
                    }
                });
                return;
            }
            //计时
            if (flag == false) {
                flag = true;
                var s = 0;
                Timing = setInterval(function () {
                    s++;
                    var time = document.getElementById("time" + selectedStepData.StepID);
                    time.innerHTML = s + "s";
                }, 1000);
            }
            //判断是否扫描重复条码
            var index = scanArray.indexOf(code.value);
            if (index != -1) {
                layer.alert('该条码已经采集过，请确认', {
                    time: 3000
                    , skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        $("#Partcode").val("");
                        layer.close(index);
                    }
                });
                return;
            }
            //判断读取扫描的物料条码
            for (var i = 0; i < selectedStepData.PartList.length; i++) {
                var value = code.value.split("-")[0];  //取物料条码前固定格式
                if (selectedStepData.PartList[i].Material_No == value) {   //如果存在物料条码
                    let row = selectedStepData.PartList[i].No;
                    var needQty = document.getElementById("table" + selectedStepData.StepID).rows[row].cells[3];
                    var finishQty = document.getElementById("table" + selectedStepData.StepID).rows[row].cells[4];
                    if (needQty.innerHTML == finishQty.innerHTML) {
                        layer.alert('该物料采集数量已满', {
                            time: 3000
                            , skin: 'layui-layer-molv'
                            , anim: 1
                            , btn: ['确定']
                            , icon: 2
                            , btn1: function (index) {
                                $("#Partcode").val("");
                                $("#Partcode").focus();
                                layer.close(index);
                            }
                        });
                        return;
                    }
                    if (selectedStepData.PartList[i].Material_Type == "Y") {  //如果是关键性物料
                        finishQty = document.getElementById("table" + selectedStepData.StepID).rows[row].cells[4];
                        finishQty.innerHTML = Number(finishQty.innerHTML) + 1;
                        document.getElementById("table" + selectedStepData.StepID).rows[row].style.backgroundColor = "#f1fb96";
                    } else {
                        finishQty.innerHTML = needQty.innerHTML;
                    }
                    if (needQty.innerHTML == finishQty.innerHTML) {
                        document.getElementById("table" + selectedStepData.StepID).rows[row].style.backgroundColor = "#7bfa67";
                    }
                    //material.title = material.title + "," + code.value;   //记录采集的条码
                    scanArray.push(code.value);
                    MaterialData.push({ "PartID": selectedStepData.PartList[i].Material_No, "PartSN": code.value/*document.getElementById("table" + selectedStepData.StepID).rows[i].cells[1].title*/ });
                    $("#Partcode").val("");
                    break;
                }
            }
        }
    })
    //工序过账
    $("#pass").on('click', function () {
        var requestUrl = [];
        var checkisallOK = false;
        PassStepData = [];
        GetParamsNG = false;
        isGetvalue = false;
        if (machineid == "1001") {
            selectedStepData = StepList[0];
        }
        if (selectedStepData.length == 0) {
            layer.alert('当前无锁定工序，无法过账', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (selectedStepData.StepIsPost == "已过账" || document.getElementById("Status" + selectedStepData.StepID).innerHTML == "工序状态:已过账") {
            layer.alert('该工序已过账，无法重复过账', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        for (var i = 1; i < document.getElementById("table" + selectedStepData.StepID).rows.length; i++) {
            if (document.getElementById("table" + selectedStepData.StepID).rows[i].style.backgroundColor != "rgb(123, 250, 103)") {
                layer.alert('有物料未采集完成，无法过账', {
                    time: 3000
                    , skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        $("#Partcode").val("");
                        layer.close(index);
                    }
                });
                return;
            }
        }
        if (selectedStepData.ParameterList != null) {
            $("#Parameter > tbody tr").each(function () {
                var status = $(this).find('td').eq(10).text();
                if (status != "OK") {
                    checkisallOK = true;
                }
            })
            if (checkisallOK) {
                layer.alert('设备始能未通过，无法过账', {
                    skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        layer.close(index);
                    }
                });
                return;
            }
        }
        PassStepData.push({ "SN": ProductCode.value, "OrderID": orderID.innerHTML, "Line_No": lineid, "Machine_id": machineid, "Step_id": selectedStepData.StepID, "EMP": user, "WorkTime": document.getElementById("time" + selectedStepData.StepID).innerHTML });
        if (machineid == "1001")   //特殊工位
        {
            requestUrl.push("/Action/SpecialStepGoPass");
        }
        else {
            requestUrl.push("/Action/PassStep");
        }
        $.ajax({
            type: "post",
            url: requestUrl,
            data: { StepData: JSON.stringify(PassStepData), MaterialData: JSON.stringify(MaterialData) },
            dataType: "json",
            beforeSend: function () {
                loading = layer.load(0, {
                    shadeClose: false,
                    title: '加载中..',
                    shade: [0.3, '#fff'] //0.1透明度的白色背景
                });
            },
            success: function (result) {
                layer.close(loading);
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == false) {
                    layer.alert(jsonresult.msg, {
                        skin: 'layui-layer-molv'
                        , anim: 1
                        , btn: ['确定']
                        , icon: 2
                        , btn1: function (index) {
                            layer.close(index);
                        }
                    });
                    return;
                }

                flag = false;
                scanArray = [];
                clearInterval(Timing);
                MaterialData = [];
                PassStepData = [];
                document.getElementById("Status" + selectedStepData.StepID).innerHTML = "工序状态:已过账";
                document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#DCDCDC";
                if (machineid != "1001") {
                    if (checkisallPass(ProductCode.value, machineid, lineid) == true) {   //判断是否工序都已过账
                        $("#Partcode").attr("disabled", true);
                        $("#ProductCode").select();
                    }
                    else {
                        $("#Partcode").val("");
                        $("#Partcode").focus();
                    }
                    if (isRepeat(StepList) == true) {
                        isLock = false;
                        selectedStepData = [];
                        $("#ProductCode").select();
                        isLock = false;
                        StepList = [];
                        selectedStepData = [];
                        Ispostcount = 0;
                        current = 0;
                        scanArray = [];
                        clearInterval(Timing);
                        flag = false;
                        orderID.innerHTML = "";
                        planStartDate.innerHTML = "";
                        planTotalQty.innerHTML = "";
                        okQty.innerHTML = "";
                        ngQty.innerHTML = "";
                        planEndDate.innerHTML = "";
                        productType.innerHTML = "";
                        surplusQty.innerHTML = "";
                        repairQty.innerHTML = "";
                        orderStatus.innerHTML = "";
                        sopurl.src = "";
                        parent.innerHTML = "";
                        parameterView.innerHTML = "";
                        $("#Partcode").removeAttr("disabled");
                        $.ajax({
                            type: "get",
                            url: "/Action/GetOrderList",
                            dataType: "json",
                            data: { Sn: ProductCode.value, MachineId: machineid, LineId: lineid },
                            beforeSend: function () {
                                loading = layer.load(0, {
                                    shadeClose: false,
                                    title: '加载中..',
                                    shade: [0.3, '#fff'] //0.1透明度的白色背景
                                });
                            },
                            success: function (result) {
                                layer.close(loading);
                                var jsonresult = JSON.parse(result);
                                var status = jsonresult.success;
                                if (status == false) {
                                    layer.msg(jsonresult.msg, { icon: 2 });
                                    layer.alert(jsonresult.msg, {
                                        skin: 'layui-layer-molv'
                                        , anim: 1
                                        , btn: ['确定']
                                        , icon: 2
                                        , btn1: function (index) {
                                            layer.close(index);
                                            orderID.innerHTML = "";
                                            planStartDate.innerHTML = "";
                                            planTotalQty.innerHTML = "";
                                            okQty.innerHTML = "";
                                            ngQty.innerHTML = "";
                                            orderStatus.innerHTML = "";
                                            planEndDate.innerHTML = "";
                                            productType.innerHTML = "";
                                            surplusQty.innerHTML = "";
                                            repairQty.innerHTML = "";
                                            sopurl.src = "";
                                            $("#ProductCode").val("");
                                            $("#Partcode").attr("disabled", true);
                                            $("#ProductCode").focus();
                                        }
                                    });
                                    return;
                                }
                                var results = jsonresult.data[0];
                                if (results.OrderStatus == "不合格") {
                                    layer.msg('该产品标记为不良，无法作业', { icon: 2 });
                                    $("#Partcode").attr("disabled", true);
                                }
                                //snCode.push(ProductCode.value);  //记录已扫描钢印号
                                orderID.innerHTML = "" + results.OrderID + "";
                                planStartDate.innerHTML = "" + results.PlanStartDate + "";
                                planTotalQty.innerHTML = "";
                                okQty.innerHTML = "";
                                ngQty.innerHTML = "";
                                planEndDate.innerHTML = "" + results.PlanEndDate + "";
                                productType.innerHTML = "" + results.ProductType + "";
                                orderStatus.innerHTML = "" + results.OrderStatus + "";
                                surplusQty.innerHTML = "";
                                repairQty.innerHTML = "";
                                for (let i = 0; i < results.StepList.length; i++) {
                                    StepList.push(results.StepList[i]);
                                    //动态创建layui table
                                    var timediv = document.createElement("div");
                                    timediv.style = "height:15px;width:100%";
                                    var label = document.createElement("lable");
                                    label.setAttribute("id", "time" + results.StepList[i].StepID);
                                    label.className = "time-lable";
                                    timediv.appendChild(label);
                                    var label = document.createElement("lable");
                                    label.className = "time";
                                    label.innerHTML = "已用时间：";
                                    timediv.appendChild(label);
                                    parent.appendChild(timediv);
                                    var card = document.createElement("div");
                                    card.className = "process-card";
                                    card.setAttribute("id", results.StepList[i].StepID);
                                    var title = document.createElement("div");
                                    title.style = "height:20px;width:100%";
                                    var span = document.createElement("span");
                                    span.className = "tabel-title";
                                    span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                                    title.appendChild(span);
                                    var span = document.createElement("span");
                                    span.className = "tabel-title";
                                    span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                                    title.appendChild(span);
                                    var span = document.createElement("span");
                                    span.className = "tabel-title";
                                    span.setAttribute("id", "Status" + results.StepList[i].StepID);
                                    span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                                    title.appendChild(span);
                                    if (results.StepList[i].StepStatus == "不合格") {
                                        var a = document.createElement("a");
                                        a.className = "tabel-title";
                                        a.setAttribute("id", "Check" + results.StepList[i].StepID);
                                        a.innerHTML = "质检结果:" + results.StepList[i].StepStatus;
                                        a.href = "javascript:void(0)";
                                        a.style = "text-decoration:underline;color:#ff6a00";
                                        a.addEventListener('dblclick', function (element) {
                                            layer.open({
                                                type: 2,
                                                title: ['质检结果', 'color:#fff;background-color:#01AAED;'],
                                                shadeClose: true,
                                                shade: 0.4,
                                                area: ['40%', '60%'],
                                                content: "/MES/Action/QCReview?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML + "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + results.StepList[i].StepID + "",
                                                cancel: function () {
                                                    layer.close(layer.index);
                                                }
                                            })
                                        });
                                        title.appendChild(a);
                                    }
                                    else {
                                        var span = document.createElement("span");
                                        span.className = "tabel-title";
                                        span.setAttribute("id", "Check" + results.StepList[i].StepID);
                                        span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                                        title.appendChild(span);
                                    }
                                    card.appendChild(title);
                                    var table = document.createElement("table");
                                    table.className = "layui-table";
                                    table.setAttribute("id", "table" + results.StepList[i].StepID);
                                    table.style = "width:98%; margin-left:7px;margin-right:9px;";
                                    var thead = document.createElement("thead");
                                    table.appendChild(thead);
                                    var tr = document.createElement("tr");
                                    thead.appendChild(tr);
                                    var th = document.createElement("th");
                                    th.innerHTML = "序号";
                                    th.style.backgroundColor = "#ffffff";
                                    th.style.fontWeight = "bolder";
                                    tr.appendChild(th);
                                    var th = document.createElement("th");
                                    th.innerHTML = "材料编号";
                                    th.style.backgroundColor = "#ffffff";
                                    th.style.fontWeight = "bolder";
                                    tr.appendChild(th);
                                    var th = document.createElement("th");
                                    th.innerHTML = "材料名称及规格";
                                    th.style.backgroundColor = "#ffffff";
                                    th.style.fontWeight = "bolder";
                                    tr.appendChild(th);
                                    var th = document.createElement("th");
                                    th.innerHTML = "需求数量";
                                    th.style.backgroundColor = "#ffffff";
                                    th.style.fontWeight = "bolder";
                                    tr.appendChild(th);
                                    var th = document.createElement("th");
                                    th.innerHTML = "完成数量";
                                    th.style.backgroundColor = "#ffffff";
                                    th.style.fontWeight = "bolder";
                                    tr.appendChild(th);
                                    var tbody = document.createElement("tbody");
                                    table.appendChild(tbody);
                                    var partData = results.StepList[i].PartList;
                                    for (var j = 0; j < partData.length; j++) {
                                        var tr = document.createElement("tr");
                                        var td = document.createElement("td");
                                        td.innerHTML = partData[j].No;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        var a = document.createElement("a");
                                        a.innerHTML = partData[j].Material_No;
                                        a.style.color = "#ff6a00";
                                        a.href = "javascript:void(0);";
                                        a.setAttribute("id", partData[j].Material_ImgUrl);
                                        a.addEventListener('dblclick', function (element) {
                                            var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                            //弹出层
                                            layer.open({
                                                type: 1,
                                                shade: 0.8,
                                                offset: 'auto',
                                                area: [1000 + 'px', 850 + 'px'],
                                                shadeClose: true,
                                                scrollbar: false,
                                                title: "图片预览",
                                                content: imgHtml,
                                                cancel: function () {
                                                }
                                            });
                                        });
                                        td.appendChild(a);
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = partData[j].Material_Name;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = partData[j].Need_Qty;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = partData[j].Finish_Qty;
                                        tr.appendChild(td);
                                        tbody.appendChild(tr);
                                    }
                                    card.appendChild(table);
                                    parent.appendChild(card);
                                    var hr = document.createElement("hr");
                                    hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                                    parent.appendChild(hr);
                                    if (results.StepList[i].StepIsPost == "已过账") {
                                        document.getElementById("table" + results.StepList[i].StepID).style.backgroundColor = "#7bfa67";
                                    }
                                }
                                if (isRepeat(StepList)) {
                                    lock.style.display = "";
                                    unlock.style.display = "";
                                } else {
                                    lock.style.display = "none";
                                    unlock.style.display = "none";
                                }
                                for (var i = 0; i < StepList.length; i++) {
                                    if (StepList[i].StepIsPost == "已过账") {
                                        Ispostcount = Ispostcount + 1;
                                    }
                                }
                                if (Ispostcount == StepList.length) {
                                    $("#Partcode").attr("disabled", true);
                                    isLock = true;
                                    return;
                                }
                                //判断是否有同等级别的工序
                                if (isRepeat(StepList)) {
                                    for (var i = 0; i < StepList.length; i++) {
                                        if (StepList[i].isLock == "Y") {
                                            document.getElementById("" + StepList[i].StepID).style.backgroundColor = "#52bbf7";
                                            selectedStepData = StepList[i];
                                            planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                            okQty.innerHTML = selectedStepData.OkQty;
                                            ngQty.innerHTML = selectedStepData.NgQty
                                            surplusQty.innerHTML = selectedStepData.SurplusQty;
                                            repairQty.innerHTML = selectedStepData.RepairQty;
                                            sopurl.src = selectedStepData.SopUrl;
                                            isLock = true;
                                            loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                            break;
                                        }
                                    }
                                }
                                else {
                                    for (var i = 0; i < StepList.length; i++) {
                                        if (StepList[i].StepIsPost == "未过账") {
                                            selectedStepData = StepList[i];
                                            current = i;
                                            break;
                                        }
                                    }
                                    document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                                    planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                    okQty.innerHTML = selectedStepData.OkQty;
                                    ngQty.innerHTML = selectedStepData.NgQty
                                    surplusQty.innerHTML = selectedStepData.SurplusQty;
                                    repairQty.innerHTML = selectedStepData.RepairQty;
                                    sopurl.src = selectedStepData.SopUrl;
                                    isLock = true;
                                    loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                }
                            },
                            error: function (message) {
                                layer.close(loading);
                                layer.msg(message.statusText, { icon: 2 });
                            }
                        })
                    } else {
                        if (current == StepList.length - 1) {
                            isLock = true;
                            selectedStepData = [];
                            return;
                        } else {
                            current = current + 1;
                            parameterView.innerHTML = "";
                            selectedStepData = StepList[current];    //取当前第一个工序
                            if (selectedStepData.StepIsPost != "已过账") {
                                document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                                planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                okQty.innerHTML = selectedStepData.OkQty;
                                ngQty.innerHTML = selectedStepData.NgQty
                                surplusQty.innerHTML = selectedStepData.SurplusQty;
                                repairQty.innerHTML = selectedStepData.RepairQty;
                                sopurl.src = selectedStepData.SopUrl;
                                loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                            }
                        }
                    }
                }       
                layer.msg('工序过账成功', {
                    icon: 1,
                    time: 3000
                });
                if (machineid == "1001") {
                    window.location.reload();
                }              
            },
            error: function (message) {
                layer.msg(message.statusText, { icon: 2 });
            }
        })
    })
    //质检
    $("#QC").on('click', function () {
        if (ProductCode.value == "" || StepList.length == 0) {
            layer.alert('请先扫描有效产品码', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        var childform = "";
        var parentform = "";
        var sn = ProductCode.value;
        parentform = layer.open({
            title: '请输入质检密钥',
            closeBtn: '1',
            content: '<div>  <input type="password" id="zjpwd" name="zjpwd" lay-verify="required" class="layui-input" autocomplete="off"  style="width:250px;"></div>',
            btn: ['确认', '取消'],
            yes: function (index) {
                var pwd = $('#zjpwd').val();
                $.ajax({
                    type: "get",
                    async: false,
                    url: "/Action/CheckSecretKey",
                    data: { secretKeyValue: pwd, secretKeyType: "质检" },
                    dataType: "json",
                    success: function (result) {
                        var jsonresult = JSON.parse(result);
                        var status = jsonresult.success;
                        if (status == false) {
                            layer.msg(jsonresult.msg, { icon: 2 });
                            return;
                        }
                        childform = layer.open({
                            type: 2,
                            title: ['质检', 'color:#fff;background-color:#01AAED;'],
                            shadeClose: true,
                            shade: 0.4,
                            area: ['40%', '70%'],
                            content: "/MES/Action/QC?SN=" + sn + "&PWD=" + pwd + "&WorkId=" + orderID.innerHTML + "&LineId=" + lineid + "",
                            cancel: function () {
                                layer.close(childform);
                                layer.close(parentform);
                            },
                            end: function () {
                                if (ProductCode.value == "") { return; }
                                else {
                                    isLock = false;
                                    StepList = [];
                                    selectedStepData = [];
                                    Ispostcount = 0;
                                    current = 0;
                                    scanArray = [];
                                    clearInterval(Timing);
                                    flag = false;
                                    orderID.innerHTML = "";
                                    planStartDate.innerHTML = "";
                                    planTotalQty.innerHTML = "";
                                    okQty.innerHTML = "";
                                    ngQty.innerHTML = "";
                                    planEndDate.innerHTML = "";
                                    productType.innerHTML = "";
                                    surplusQty.innerHTML = "";
                                    repairQty.innerHTML = "";
                                    orderStatus.innerHTML = "";
                                    sopurl.src = "";
                                    parent.innerHTML = "";
                                    parameterView.innerHTML = "";
                                    $("#Partcode").removeAttr("disabled");
                                    $.ajax({
                                        type: "get",
                                        url: "/Action/GetOrderList",
                                        dataType: "json",
                                        data: { Sn: ProductCode.value, MachineId: machineid, LineId: lineid },
                                        beforeSend: function () {
                                            loading = layer.load(0, {
                                                shadeClose: false,
                                                title: '加载中..',
                                                shade: [0.3, '#fff'] //0.1透明度的白色背景
                                            });
                                        },
                                        success: function (result) {
                                            layer.close(loading);
                                            var jsonresult = JSON.parse(result);
                                            var status = jsonresult.success;
                                            if (status == false) {
                                                layer.msg(jsonresult.msg, { icon: 2 });
                                                layer.alert(jsonresult.msg, {
                                                    skin: 'layui-layer-molv'
                                                    , anim: 1
                                                    , btn: ['确定']
                                                    , icon: 2
                                                    , btn1: function (index) {
                                                        layer.close(index);
                                                        orderID.innerHTML = "";
                                                        planStartDate.innerHTML = "";
                                                        planTotalQty.innerHTML = "";
                                                        okQty.innerHTML = "";
                                                        ngQty.innerHTML = "";
                                                        orderStatus.innerHTML = "";
                                                        planEndDate.innerHTML = "";
                                                        productType.innerHTML = "";
                                                        surplusQty.innerHTML = "";
                                                        repairQty.innerHTML = "";
                                                        sopurl.src = "";
                                                        $("#ProductCode").val("");
                                                        $("#Partcode").attr("disabled", true);
                                                        $("#ProductCode").focus();
                                                    }
                                                });
                                                return;
                                            }
                                            var results = jsonresult.data[0];
                                            if (results.OrderStatus == "不合格") {
                                                layer.msg('该产品标记为不良，无法作业', { icon: 2 });
                                                $("#Partcode").attr("disabled", true);
                                            }
                                            //snCode.push(ProductCode.value);  //记录已扫描钢印号
                                            orderID.innerHTML = "" + results.OrderID + "";
                                            planStartDate.innerHTML = "" + results.PlanStartDate + "";
                                            planTotalQty.innerHTML = "";
                                            okQty.innerHTML = "";
                                            ngQty.innerHTML = "";
                                            planEndDate.innerHTML = "" + results.PlanEndDate + "";
                                            productType.innerHTML = "" + results.ProductType + "";
                                            orderStatus.innerHTML = "" + results.OrderStatus + "";
                                            surplusQty.innerHTML = "";
                                            repairQty.innerHTML = "";
                                            for (let i = 0; i < results.StepList.length; i++) {
                                                StepList.push(results.StepList[i]);
                                                //动态创建layui table
                                                var timediv = document.createElement("div");
                                                timediv.style = "height:15px;width:100%";
                                                var label = document.createElement("lable");
                                                label.setAttribute("id", "time" + results.StepList[i].StepID);
                                                label.className = "time-lable";
                                                timediv.appendChild(label);
                                                var label = document.createElement("lable");
                                                label.className = "time";
                                                label.innerHTML = "已用时间：";
                                                timediv.appendChild(label);
                                                parent.appendChild(timediv);
                                                var card = document.createElement("div");
                                                card.className = "process-card";
                                                card.setAttribute("id", results.StepList[i].StepID);
                                                var title = document.createElement("div");
                                                title.style = "height:20px;width:100%";
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                                                title.appendChild(span);
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                                                title.appendChild(span);
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.setAttribute("id", "Status" + results.StepList[i].StepID);
                                                span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                                                title.appendChild(span);
                                                if (results.StepList[i].StepStatus == "不合格") {
                                                    var a = document.createElement("a");
                                                    a.className = "tabel-title";
                                                    a.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                    a.innerHTML = "质检结果:" + results.StepList[i].StepStatus;
                                                    a.href = "javascript:void(0)";
                                                    a.style = "text-decoration:underline;color:#ff6a00";
                                                    a.addEventListener('dblclick', function (element) {
                                                        layer.open({
                                                            type: 2,
                                                            title: ['质检结果', 'color:#fff;background-color:#01AAED;'],
                                                            shadeClose: true,
                                                            shade: 0.4,
                                                            area: ['40%', '60%'],
                                                            content: "/MES/Action/QCReview?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML + "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + results.StepList[i].StepID + "",
                                                            cancel: function () {
                                                                layer.close(layer.index);
                                                            }
                                                        })
                                                    });
                                                    title.appendChild(a);
                                                }
                                                else {
                                                    var span = document.createElement("span");
                                                    span.className = "tabel-title";
                                                    span.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                    span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                                                    title.appendChild(span);
                                                }
                                                card.appendChild(title);
                                                var table = document.createElement("table");
                                                table.className = "layui-table";
                                                table.setAttribute("id", "table" + results.StepList[i].StepID);
                                                table.style = "width:98%; margin-left:7px;margin-right:9px;";
                                                var thead = document.createElement("thead");
                                                table.appendChild(thead);
                                                var tr = document.createElement("tr");
                                                thead.appendChild(tr);
                                                var th = document.createElement("th");
                                                th.innerHTML = "序号";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "材料编号";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "材料名称及规格";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "需求数量";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "完成数量";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var tbody = document.createElement("tbody");
                                                table.appendChild(tbody);
                                                var partData = results.StepList[i].PartList;
                                                for (var j = 0; j < partData.length; j++) {
                                                    var tr = document.createElement("tr");
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].No;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    var a = document.createElement("a");
                                                    a.innerHTML = partData[j].Material_No;
                                                    a.style.color = "#ff6a00";
                                                    a.href = "javascript:void(0);";
                                                    a.setAttribute("id", partData[j].Material_ImgUrl);
                                                    a.addEventListener('dblclick', function (element) {
                                                        var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                                        //弹出层
                                                        layer.open({
                                                            type: 1,
                                                            shade: 0.8,
                                                            offset: 'auto',
                                                            area: [1000 + 'px', 850 + 'px'],
                                                            shadeClose: true,
                                                            scrollbar: false,
                                                            title: "图片预览",
                                                            content: imgHtml,
                                                            cancel: function () {
                                                            }
                                                        });
                                                    });
                                                    td.appendChild(a);
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Material_Name;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Need_Qty;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Finish_Qty;
                                                    tr.appendChild(td);
                                                    tbody.appendChild(tr);
                                                }
                                                card.appendChild(table);
                                                parent.appendChild(card);
                                                var hr = document.createElement("hr");
                                                hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                                                parent.appendChild(hr);
                                                if (results.StepList[i].StepIsPost == "已过账") {
                                                    document.getElementById("table" + results.StepList[i].StepID).style.backgroundColor = "#7bfa67";
                                                }
                                            }
                                            if (isRepeat(StepList)) {
                                                lock.style.display = "";
                                                unlock.style.display = "";
                                            } else {
                                                lock.style.display = "none";
                                                unlock.style.display = "none";
                                            }
                                            for (var i = 0; i < StepList.length; i++) {
                                                if (StepList[i].StepIsPost == "已过账") {
                                                    Ispostcount = Ispostcount + 1;
                                                }
                                            }
                                            if (Ispostcount == StepList.length) {
                                                $("#Partcode").attr("disabled", true);
                                                isLock = true;
                                                return;
                                            }
                                            //判断是否有同等级别的工序
                                            if (isRepeat(StepList)) {
                                                for (var i = 0; i < StepList.length; i++) {
                                                    if (StepList[i].isLock == "Y") {
                                                        document.getElementById("" + StepList[i].StepID).style.backgroundColor = "#52bbf7";
                                                        selectedStepData = StepList[i];
                                                        planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                        okQty.innerHTML = selectedStepData.OkQty;
                                                        ngQty.innerHTML = selectedStepData.NgQty
                                                        surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                        repairQty.innerHTML = selectedStepData.RepairQty;
                                                        sopurl.src = selectedStepData.SopUrl;
                                                        isLock = true;
                                                        loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                                        break;
                                                    }
                                                }
                                            }
                                            else {
                                                for (var i = 0; i < StepList.length; i++) {
                                                    if (StepList[i].StepIsPost == "未过账") {
                                                        selectedStepData = StepList[i];
                                                        current = i;
                                                        break;
                                                    }
                                                }
                                                document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                                                planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                okQty.innerHTML = selectedStepData.OkQty;
                                                ngQty.innerHTML = selectedStepData.NgQty
                                                surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                repairQty.innerHTML = selectedStepData.RepairQty;
                                                sopurl.src = selectedStepData.SopUrl;
                                                isLock = true;
                                                loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                            }
                                        },
                                        error: function (message) {
                                            layer.close(loading);
                                            layer.msg(message.statusText, { icon: 2 });
                                        }
                                    })
                                }
                            }
                        });
                    },
                    error: function (message) {
                        layer.msg(message.statusText, { icon: 2 });
                    }
                })

            },
            no: function (index) {
                layer.close(index);
            },
            close: function (index) {
            }
        })
    })
    //返修
    $("#Repair").on('click', function () {
        var childform = "";
        var parentform = "";
        parentform = layer.open({
            title: '请输入质检密钥',
            closeBtn: '1',//右上角的关闭
            content: '<div>  <input type="password" id="zjpwd" name="zjpwd" lay-verify="required" class="layui-input" autocomplete="off" style="width:250px;"></div>',
            btn: ['确认', '取消'],
            yes: function (index) {
                var pwd = $('#zjpwd').val();
                $.ajax({
                    type: "get",
                    async: false,
                    url: "/Action/CheckSecretKey",
                    data: { secretKeyValue: pwd, secretKeyType: "质检" },
                    dataType: "json",
                    success: function (result) {
                        var jsonresult = JSON.parse(result);
                        var status = jsonresult.success;
                        if (status == false) {
                            layer.msg(jsonresult.msg, { icon: 2 });
                            return;
                        }
                        childform = layer.open({
                            type: 2,
                            title: ['返修', 'color:#fff;background-color:#01AAED;'],
                            shadeClose: true,
                            shade: 0.4,
                            area: ['80%', '85%'],
                            content: "/MES/Action/Repair?PWD=" + pwd + "",
                            cancel: function () {
                                layer.close(childform);
                                layer.close(parentform);
                            },
                            end: function () {
                                if (ProductCode.value == "") { return; }
                                else {
                                    isLock = false;
                                    StepList = [];
                                    selectedStepData = [];
                                    Ispostcount = 0;
                                    current = 0;
                                    scanArray = [];
                                    clearInterval(Timing);
                                    flag = false;
                                    orderID.innerHTML = "";
                                    planStartDate.innerHTML = "";
                                    planTotalQty.innerHTML = "";
                                    okQty.innerHTML = "";
                                    ngQty.innerHTML = "";
                                    planEndDate.innerHTML = "";
                                    productType.innerHTML = "";
                                    surplusQty.innerHTML = "";
                                    repairQty.innerHTML = "";
                                    orderStatus.innerHTML = "";
                                    sopurl.src = "";
                                    parent.innerHTML = "";
                                    parameterView.innerHTML = "";
                                    $("#Partcode").removeAttr("disabled");
                                    $.ajax({
                                        type: "get",
                                        url: "/Action/GetOrderList",
                                        dataType: "json",
                                        data: { Sn: ProductCode.value, MachineId: machineid, LineId: lineid },
                                        beforeSend: function () {
                                            loading = layer.load(0, {
                                                shadeClose: false,
                                                title: '加载中..',
                                                shade: [0.3, '#fff'] //0.1透明度的白色背景
                                            });
                                        },
                                        success: function (result) {
                                            layer.close(loading);
                                            var jsonresult = JSON.parse(result);
                                            var status = jsonresult.success;
                                            if (status == false) {
                                                layer.msg(jsonresult.msg, { icon: 2 });
                                                layer.alert(jsonresult.msg, {
                                                    skin: 'layui-layer-molv'
                                                    , anim: 1
                                                    , btn: ['确定']
                                                    , icon: 2
                                                    , btn1: function (index) {
                                                        layer.close(index);
                                                        orderID.innerHTML = "";
                                                        planStartDate.innerHTML = "";
                                                        planTotalQty.innerHTML = "";
                                                        okQty.innerHTML = "";
                                                        ngQty.innerHTML = "";
                                                        orderStatus.innerHTML = "";
                                                        planEndDate.innerHTML = "";
                                                        productType.innerHTML = "";
                                                        surplusQty.innerHTML = "";
                                                        repairQty.innerHTML = "";
                                                        sopurl.src = "";
                                                        $("#ProductCode").val("");
                                                        $("#Partcode").attr("disabled", true);
                                                        $("#ProductCode").focus();
                                                    }
                                                });
                                                return;
                                            }
                                            var results = jsonresult.data[0];
                                            if (results.OrderStatus == "不合格") {
                                                layer.msg('该产品标记为不良，无法作业', { icon: 2 });
                                                $("#Partcode").attr("disabled", true);
                                            }
                                            //snCode.push(ProductCode.value);  //记录已扫描钢印号
                                            orderID.innerHTML = "" + results.OrderID + "";
                                            planStartDate.innerHTML = "" + results.PlanStartDate + "";
                                            planTotalQty.innerHTML = "";
                                            okQty.innerHTML = "";
                                            ngQty.innerHTML = "";
                                            planEndDate.innerHTML = "" + results.PlanEndDate + "";
                                            productType.innerHTML = "" + results.ProductType + "";
                                            orderStatus.innerHTML = "" + results.OrderStatus + "";
                                            surplusQty.innerHTML = "";
                                            repairQty.innerHTML = "";
                                            for (let i = 0; i < results.StepList.length; i++) {
                                                StepList.push(results.StepList[i]);
                                                //动态创建layui table
                                                var timediv = document.createElement("div");
                                                timediv.style = "height:15px;width:100%";
                                                var label = document.createElement("lable");
                                                label.setAttribute("id", "time" + results.StepList[i].StepID);
                                                label.className = "time-lable";
                                                timediv.appendChild(label);
                                                var label = document.createElement("lable");
                                                label.className = "time";
                                                label.innerHTML = "已用时间：";
                                                timediv.appendChild(label);
                                                parent.appendChild(timediv);
                                                var card = document.createElement("div");
                                                card.className = "process-card";
                                                card.setAttribute("id", results.StepList[i].StepID);
                                                var title = document.createElement("div");
                                                title.style = "height:20px;width:100%";
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                                                title.appendChild(span);
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                                                title.appendChild(span);
                                                var span = document.createElement("span");
                                                span.className = "tabel-title";
                                                span.setAttribute("id", "Status" + results.StepList[i].StepID);
                                                span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                                                title.appendChild(span);
                                                if (results.StepList[i].StepStatus == "不合格") {
                                                    var a = document.createElement("a");
                                                    a.className = "tabel-title";
                                                    a.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                    a.innerHTML = "质检结果:" + results.StepList[i].StepStatus;
                                                    a.href = "javascript:void(0)";
                                                    a.style = "text-decoration:underline;color:#ff6a00";
                                                    a.addEventListener('dblclick', function (element) {
                                                        layer.open({
                                                            type: 2,
                                                            title: ['质检结果', 'color:#fff;background-color:#01AAED;'],
                                                            shadeClose: true,
                                                            shade: 0.4,
                                                            area: ['40%', '60%'],
                                                            content: "/MES/Action/QCReview?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML + "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + results.StepList[i].StepID + "",
                                                            cancel: function () {
                                                                layer.close(layer.index);
                                                            }
                                                        })
                                                    });
                                                    title.appendChild(a);
                                                }
                                                else {
                                                    var span = document.createElement("span");
                                                    span.className = "tabel-title";
                                                    span.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                    span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                                                    title.appendChild(span);
                                                }
                                                card.appendChild(title);
                                                var table = document.createElement("table");
                                                table.className = "layui-table";
                                                table.setAttribute("id", "table" + results.StepList[i].StepID);
                                                table.style = "width:98%; margin-left:7px;margin-right:9px;";
                                                var thead = document.createElement("thead");
                                                table.appendChild(thead);
                                                var tr = document.createElement("tr");
                                                thead.appendChild(tr);
                                                var th = document.createElement("th");
                                                th.innerHTML = "序号";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "材料编号";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "材料名称及规格";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "需求数量";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var th = document.createElement("th");
                                                th.innerHTML = "完成数量";
                                                th.style.backgroundColor = "#ffffff";
                                                th.style.fontWeight = "bolder";
                                                tr.appendChild(th);
                                                var tbody = document.createElement("tbody");
                                                table.appendChild(tbody);
                                                var partData = results.StepList[i].PartList;
                                                for (var j = 0; j < partData.length; j++) {
                                                    var tr = document.createElement("tr");
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].No;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    var a = document.createElement("a");
                                                    a.innerHTML = partData[j].Material_No;
                                                    a.style.color = "#ff6a00";
                                                    a.href = "javascript:void(0);";
                                                    a.setAttribute("id", partData[j].Material_ImgUrl);
                                                    a.addEventListener('dblclick', function (element) {
                                                        var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                                        //弹出层
                                                        layer.open({
                                                            type: 1,
                                                            shade: 0.8,
                                                            offset: 'auto',
                                                            area: [1000 + 'px', 850 + 'px'],
                                                            shadeClose: true,
                                                            scrollbar: false,
                                                            title: "图片预览",
                                                            content: imgHtml,
                                                            cancel: function () {
                                                            }
                                                        });
                                                    });
                                                    td.appendChild(a);
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Material_Name;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Need_Qty;
                                                    tr.appendChild(td);
                                                    var td = document.createElement("td");
                                                    td.innerHTML = partData[j].Finish_Qty;
                                                    tr.appendChild(td);
                                                    tbody.appendChild(tr);
                                                }
                                                card.appendChild(table);
                                                parent.appendChild(card);
                                                var hr = document.createElement("hr");
                                                hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                                                parent.appendChild(hr);
                                                if (results.StepList[i].StepIsPost == "已过账") {
                                                    document.getElementById("table" + results.StepList[i].StepID).style.backgroundColor = "#7bfa67";
                                                }
                                            }
                                            if (isRepeat(StepList)) {
                                                lock.style.display = "";
                                                unlock.style.display = "";
                                            } else {
                                                lock.style.display = "none";
                                                unlock.style.display = "none";
                                            }
                                            for (var i = 0; i < StepList.length; i++) {
                                                if (StepList[i].StepIsPost == "已过账") {
                                                    Ispostcount = Ispostcount + 1;
                                                }
                                            }
                                            if (Ispostcount == StepList.length) {
                                                $("#Partcode").attr("disabled", true);
                                                isLock = true;
                                                return;
                                            }
                                            //判断是否有同等级别的工序
                                            if (isRepeat(StepList)) {
                                                for (var i = 0; i < StepList.length; i++) {
                                                    if (StepList[i].isLock == "Y") {
                                                        document.getElementById("" + StepList[i].StepID).style.backgroundColor = "#52bbf7";
                                                        selectedStepData = StepList[i];
                                                        planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                        okQty.innerHTML = selectedStepData.OkQty;
                                                        ngQty.innerHTML = selectedStepData.NgQty
                                                        surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                        repairQty.innerHTML = selectedStepData.RepairQty;
                                                        sopurl.src =selectedStepData.SopUrl;
                                                        isLock = true;
                                                        loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                                        break;
                                                    }
                                                }
                                            }
                                            else {
                                                for (var i = 0; i < StepList.length; i++) {
                                                    if (StepList[i].StepIsPost == "未过账") {
                                                        selectedStepData = StepList[i];
                                                        current = i;
                                                        break;
                                                    }
                                                }
                                                document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                                                planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                okQty.innerHTML = selectedStepData.OkQty;
                                                ngQty.innerHTML = selectedStepData.NgQty
                                                surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                repairQty.innerHTML = selectedStepData.RepairQty;
                                                sopurl.src =  selectedStepData.SopUrl;
                                                isLock = true;
                                                loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                            }
                                        },
                                        error: function (message) {
                                            layer.close(loading);
                                            layer.msg(message.statusText, { icon: 2 });
                                        }
                                    })
                                }
                            }
                        });
                    },
                    error: function (message) {
                        layer.msg(message.statusText, { icon: 2 });
                    }
                })


            },
            no: function (index) {
                layer.close(index);
            },
            close: function (index) {
            },
        });

    })
    //不良确认
    $("#ngcheck").on('click', function () {
        if (selectedStepData.length == 0) {
            layer.alert('未获取到锁定工序，无法确认不良', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (selectedStepData.ParameterList == null) {
            layer.alert('该工序无需采集加工参数，无法确认不良', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        var checkisGetValue = false;
        $("#Parameter > tbody tr").each(function () {
            if ($(this).find('td').eq(10).text() != "") {
                checkisGetValue = true;
                return false;
            }
        })
        if (checkisGetValue == false) {
            layer.alert('未采集过加工参数，无法确认不良', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        var childform = "";
        var parentform = "";
        parentform = layer.open({
            title: '请输入组长密钥',
            closeBtn: '1',//右上角的关闭
            content: '<div>  <input type="password" id="zzpwd" name="zzpwd" lay-verify="required" class="layui-input" autocomplete="off" style="width:250px;"></div>',
            btn: ['确认', '取消'],
            yes: function (index) {
                var pwd = $('#zzpwd').val();
                $.ajax({
                    type: "get",
                    async: false,
                    url: "/Action/CheckSecretKey",
                    data: { secretKeyValue: pwd, secretKeyType: "组长" },
                    dataType: "json",
                    success: function (result) {
                        var jsonresult = JSON.parse(result);
                        var status = jsonresult.success;
                        if (status == false) {
                            layer.msg(jsonresult.msg, { icon: 2 });
                            return;
                        }
                        childform = layer.open({
                            title: ['不良确认', 'color:#fff;background-color:#ff0000;'],
                            area: ['500px', '240px'],
                            closeBtn: '1',//右上角的关闭
                            content: '<div> <textarea id="Reason" name="Reason" placeholder="请输入不良原因" class="layui-textarea"></textarea></div>',
                            btn: ['确认', '取消'],
                            yes: function (index, layero) {
                                var reason = $("#Reason").val();
                                if (reason == "") {
                                    layer.msg('请输入不良原因', { icon: 2 });
                                    return;
                                }
                                $.ajax({
                                    type: "get",
                                    url: "/Action/NGConfirm",
                                    dataType: "json",
                                    data: { Sn: ProductCode.value, Workid: orderID.innerHTML, Lineid: lineid, emp: pwd, Machineid: machineid, Stepid: selectedStepData.StepID, reason: reason },
                                    success: function (result) {
                                        var jsonresult = JSON.parse(result);
                                        var status = jsonresult.success;
                                        if (status == false) {
                                            layer.alert(jsonresult.msg, {
                                                skin: 'layui-layer-molv'
                                                , anim: 1
                                                , btn: ['确定']
                                                , icon: 2
                                                , btn1: function (index) {
                                                    layer.close(index);
                                                }
                                            });
                                            return;
                                        }
                                        layer.alert('该工序已确认为不良', {
                                            skin: 'layui-layer-molv'
                                            , anim: 1
                                            , btn: ['确定']
                                            , icon: 1
                                            , btn1: function (index) {
                                                layer.close(index);
                                                layer.close(childform);
                                                layer.close(parentform);
                                                if (ProductCode.value == "") {
                                                    return;
                                                }
                                                else {
                                                    isLock = false;
                                                    StepList = [];
                                                    selectedStepData = [];
                                                    Ispostcount = 0;
                                                    current = 0;
                                                    scanArray = [];
                                                    clearInterval(Timing);
                                                    flag = false;
                                                    orderID.innerHTML = "";
                                                    planStartDate.innerHTML = "";
                                                    planTotalQty.innerHTML = "";
                                                    okQty.innerHTML = "";
                                                    ngQty.innerHTML = "";
                                                    planEndDate.innerHTML = "";
                                                    productType.innerHTML = "";
                                                    surplusQty.innerHTML = "";
                                                    repairQty.innerHTML = "";
                                                    orderStatus.innerHTML = "";
                                                    sopurl.src = "";
                                                    parent.innerHTML = "";
                                                    parameterView.innerHTML = "";
                                                    $("#Partcode").removeAttr("disabled");
                                                    $.ajax({
                                                        type: "get",
                                                        url: "/Action/GetOrderList",
                                                        dataType: "json",
                                                        data: { Sn: ProductCode.value, MachineId: machineid, LineId: lineid },
                                                        beforeSend: function () {
                                                            loading = layer.load(0, {
                                                                shadeClose: false,
                                                                title: '加载中..',
                                                                shade: [0.3, '#fff'] //0.1透明度的白色背景
                                                            });
                                                        },
                                                        success: function (result) {
                                                            layer.close(loading);
                                                            var jsonresult = JSON.parse(result);
                                                            var status = jsonresult.success;
                                                            if (status == false) {
                                                                layer.alert(jsonresult.msg, {
                                                                    skin: 'layui-layer-molv'
                                                                    , anim: 1
                                                                    , btn: ['确定']
                                                                    , icon: 2
                                                                    , btn1: function (index) {
                                                                        layer.close(index);
                                                                        orderID.innerHTML = "";
                                                                        planStartDate.innerHTML = "";
                                                                        planTotalQty.innerHTML = "";
                                                                        okQty.innerHTML = "";
                                                                        ngQty.innerHTML = "";
                                                                        orderStatus.innerHTML = "";
                                                                        planEndDate.innerHTML = "";
                                                                        productType.innerHTML = "";
                                                                        surplusQty.innerHTML = "";
                                                                        repairQty.innerHTML = "";
                                                                        sopurl.src = "";
                                                                        $("#ProductCode").val("");
                                                                        $("#Partcode").attr("disabled", true);
                                                                        $("#ProductCode").focus();
                                                                    }
                                                                });
                                                                return;
                                                            }
                                                            var results = jsonresult.data[0];
                                                            if (results.OrderStatus == "不合格") {
                                                                layer.msg('该产品标记为不良，无法作业', { icon: 2 });
                                                                $("#Partcode").attr("disabled", true);
                                                            }
                                                            orderID.innerHTML = "" + results.OrderID + "";
                                                            planStartDate.innerHTML = "" + results.PlanStartDate + "";
                                                            planTotalQty.innerHTML = "";
                                                            okQty.innerHTML = "";
                                                            ngQty.innerHTML = "";
                                                            planEndDate.innerHTML = "" + results.PlanEndDate + "";
                                                            productType.innerHTML = "" + results.ProductType + "";
                                                            orderStatus.innerHTML = "" + results.OrderStatus + "";
                                                            surplusQty.innerHTML = "";
                                                            repairQty.innerHTML = "";
                                                            for (let i = 0; i < results.StepList.length; i++) {
                                                                StepList.push(results.StepList[i]);
                                                                //动态创建layui table
                                                                var timediv = document.createElement("div");
                                                                timediv.style = "height:15px;width:100%";
                                                                var label = document.createElement("lable");
                                                                label.setAttribute("id", "time" + results.StepList[i].StepID);
                                                                label.innerHTML = results.StepList[i].PassTime;
                                                                label.className = "time-lable";
                                                                timediv.appendChild(label);
                                                                var label = document.createElement("lable");
                                                                label.className = "time";
                                                                label.innerHTML = "已用时间：";
                                                                timediv.appendChild(label);
                                                                parent.appendChild(timediv);
                                                                var card = document.createElement("div");
                                                                card.className = "process-card";
                                                                card.setAttribute("id", results.StepList[i].StepID);
                                                                var title = document.createElement("div");
                                                                title.style = "height:20px;width:100%";
                                                                var span = document.createElement("span");
                                                                span.className = "tabel-title";
                                                                span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                                                                title.appendChild(span);
                                                                var span = document.createElement("span");
                                                                span.className = "tabel-title";
                                                                span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                                                                title.appendChild(span);
                                                                var span = document.createElement("span");
                                                                span.className = "tabel-title";
                                                                span.setAttribute("id", "Status" + results.StepList[i].StepID);
                                                                span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                                                                title.appendChild(span);
                                                                if (results.StepList[i].StepStatus == "不合格") {
                                                                    var a = document.createElement("a");
                                                                    a.className = "tabel-title";
                                                                    a.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                                    a.innerHTML = "质检结果:" + results.StepList[i].StepStatus;
                                                                    a.href = "javascript:void(0)";
                                                                    a.style = "text-decoration:underline;color:#ff6a00";
                                                                    a.addEventListener('dblclick', function (element) {
                                                                        layer.open({
                                                                            type: 2,
                                                                            title: ['质检结果', 'color:#fff;background-color:#01AAED;'],
                                                                            shadeClose: true,
                                                                            shade: 0.4,
                                                                            area: ['40%', '60%'],
                                                                            content: "/MES/Action/QCReview?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML + "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + results.StepList[i].StepID + "",
                                                                            cancel: function () {
                                                                                layer.close(layer.index);
                                                                            }
                                                                        })
                                                                    });
                                                                    title.appendChild(a);
                                                                }
                                                                else {
                                                                    var span = document.createElement("span");
                                                                    span.className = "tabel-title";
                                                                    span.setAttribute("id", "Check" + results.StepList[i].StepID);
                                                                    span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                                                                    title.appendChild(span);
                                                                }
                                                                card.appendChild(title);
                                                                var table = document.createElement("table");
                                                                table.className = "layui-table";
                                                                table.setAttribute("id", "table" + results.StepList[i].StepID);
                                                                table.style = "width:98%; margin-left:7px;margin-right:9px;";
                                                                var thead = document.createElement("thead");
                                                                table.appendChild(thead);
                                                                var tr = document.createElement("tr");
                                                                thead.appendChild(tr);
                                                                var th = document.createElement("th");
                                                                th.innerHTML = "序号";
                                                                th.style.backgroundColor = "#ffffff";
                                                                th.style.fontWeight = "bolder";
                                                                tr.appendChild(th);
                                                                var th = document.createElement("th");
                                                                th.innerHTML = "材料编号";
                                                                th.style.backgroundColor = "#ffffff";
                                                                th.style.fontWeight = "bolder";
                                                                tr.appendChild(th);
                                                                var th = document.createElement("th");
                                                                th.innerHTML = "材料名称及规格";
                                                                th.style.backgroundColor = "#ffffff";
                                                                th.style.fontWeight = "bolder";
                                                                tr.appendChild(th);
                                                                var th = document.createElement("th");
                                                                th.innerHTML = "需求数量";
                                                                th.style.backgroundColor = "#ffffff";
                                                                th.style.fontWeight = "bolder";
                                                                tr.appendChild(th);
                                                                var th = document.createElement("th");
                                                                th.innerHTML = "完成数量";
                                                                th.style.backgroundColor = "#ffffff";
                                                                th.style.fontWeight = "bolder";
                                                                tr.appendChild(th);
                                                                var tbody = document.createElement("tbody");
                                                                table.appendChild(tbody);
                                                                var partData = results.StepList[i].PartList;
                                                                for (var j = 0; j < partData.length; j++) {
                                                                    var tr = document.createElement("tr");
                                                                    var td = document.createElement("td");
                                                                    td.innerHTML = partData[j].No;
                                                                    tr.appendChild(td);
                                                                    var td = document.createElement("td");
                                                                    var a = document.createElement("a");
                                                                    a.innerHTML = partData[j].Material_No;
                                                                    a.style.color = "#ff6a00";
                                                                    a.href = "javascript:void(0);";
                                                                    a.setAttribute("id", partData[j].Material_ImgUrl);
                                                                    a.addEventListener('dblclick', function (element) {
                                                                        var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                                                        //弹出层
                                                                        layer.open({
                                                                            type: 1,
                                                                            shade: 0.8,
                                                                            offset: 'auto',
                                                                            area: [1000 + 'px', 850 + 'px'],
                                                                            shadeClose: true,
                                                                            scrollbar: false,
                                                                            title: "物料图片预览",
                                                                            content: imgHtml,
                                                                            cancel: function () {
                                                                            }
                                                                        });
                                                                    });
                                                                    td.appendChild(a);
                                                                    tr.appendChild(td);
                                                                    var td = document.createElement("td");
                                                                    td.innerHTML = partData[j].Material_Name;
                                                                    tr.appendChild(td);
                                                                    var td = document.createElement("td");
                                                                    td.innerHTML = partData[j].Need_Qty;
                                                                    tr.appendChild(td);
                                                                    var td = document.createElement("td");
                                                                    td.innerHTML = partData[j].Finish_Qty;
                                                                    tr.appendChild(td);
                                                                    tbody.appendChild(tr);
                                                                }
                                                                card.appendChild(table);
                                                                parent.appendChild(card);
                                                                var hr = document.createElement("hr");
                                                                hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                                                                parent.appendChild(hr);
                                                                if (results.StepList[i].StepIsPost == "已过账") {
                                                                    document.getElementById("table" + results.StepList[i].StepID).style.backgroundColor = "#7bfa67";
                                                                }
                                                            }
                                                            if (isRepeat(StepList)) {
                                                                lock.style.display = "";
                                                                unlock.style.display = "";
                                                            } else {
                                                                lock.style.display = "none";
                                                                unlock.style.display = "none";
                                                            }
                                                            for (var i = 0; i < StepList.length; i++) {
                                                                if (StepList[i].StepIsPost == "已过账") {
                                                                    Ispostcount = Ispostcount + 1;
                                                                }
                                                            }
                                                            if (Ispostcount == StepList.length) {
                                                                $("#Partcode").attr("disabled", true);
                                                                isLock = true;
                                                                return;
                                                            }
                                                            //判断是否有同等级别的工序
                                                            if (isRepeat(StepList)) {
                                                                for (var i = 0; i < StepList.length; i++) {
                                                                    if (StepList[i].isLock == "Y") {
                                                                        document.getElementById("" + StepList[i].StepID).style.backgroundColor = "#52bbf7";
                                                                        selectedStepData = StepList[i];
                                                                        planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                                        okQty.innerHTML = selectedStepData.OkQty;
                                                                        ngQty.innerHTML = selectedStepData.NgQty
                                                                        surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                                        repairQty.innerHTML = selectedStepData.RepairQty;
                                                                        sopurl.src =  selectedStepData.SopUrl;
                                                                        isLock = true;
                                                                        loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                            else {
                                                                for (var i = 0; i < StepList.length; i++) {
                                                                    if (StepList[i].StepIsPost == "未过账") {
                                                                        selectedStepData = StepList[i];
                                                                        current = i;
                                                                        break;
                                                                    }
                                                                }
                                                                document.getElementById("" + selectedStepData.StepID).style.backgroundColor = "#52bbf7";
                                                                planTotalQty.innerHTML = selectedStepData.PlanTotalQty;
                                                                okQty.innerHTML = selectedStepData.OkQty;
                                                                ngQty.innerHTML = selectedStepData.NgQty
                                                                surplusQty.innerHTML = selectedStepData.SurplusQty;
                                                                repairQty.innerHTML = selectedStepData.RepairQty;
                                                                sopurl.src = selectedStepData.SopUrl;
                                                                isLock = true;
                                                                loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid);
                                                            }
                                                        },
                                                        error: function (message) {
                                                            layer.close(loading);
                                                            layer.msg('请求服务器出现异常' + message, { icon: 2 });
                                                        }
                                                    })
                                                    $("#Partcode").focus();
                                                }
                                            }
                                        });
                                    },
                                    error: function (message) {
                                        layer.msg(message.statusText, { icon: 2 });
                                    }
                                })
                            },
                            cancel: function () {
                                layer.close(childform);
                                layer.close(parentform);
                            },
                        });
                    },
                    error: function (message) {
                        layer.msg(message.statusText, { icon: 2 });
                    }
                })
            },
            no: function (index) {
                layer.close(index);
            },
        });
    })
    //设备始能
    $("#devicework").on('click', function () {
        var checkisGetValue = false;
        $("#Parameter > tbody tr").each(function () {
            if ($(this).find('td').eq(10).text() != "") {
                checkisGetValue = true;
                return false;
            }
        })
        if (checkisGetValue) {
            layer.alert('已经采集过加工参数，无法再次采集', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (machineid == "1001") {
            selectedStepData = StepList[0];
        }
        if (selectedStepData.length == 0) {
            layer.alert('未获取到锁定工序，无法采集设备参数', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (selectedStepData.ParameterList == null) {
            layer.alert('该工序无需采集加工参数', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        if (selectedStepData.StepIsPost == "已过账") {
            layer.alert('该工序已过账，无法再次采集加工参数', {
                skin: 'layui-layer-molv'
                , anim: 1
                , btn: ['确定']
                , icon: 2
                , btn1: function (index) {
                    layer.close(index);
                }
            });
            return;
        }
        for (var i = 1; i < document.getElementById("table" + selectedStepData.StepID).rows.length; i++) {
            if (document.getElementById("table" + selectedStepData.StepID).rows[i].style.backgroundColor != "rgb(123, 250, 103)") {
                layer.alert('请先扫描完物料，再设备始能', {
                    skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        $("#Partcode").val("");
                        layer.close(index);
                    }
                });
                return;
            }
        }
        layer.msg('正在下发设备参数', {
            shade: 0.4,
            icon: 6,
        });
        //layer.msg('正在下发设备参数', { icon: 6 });
        $.ajax({
            type: "get",
            url: "/Action/DeviceWork",
            data: { Sn: ProductCode.value, workid: orderID.innerHTML, lineid: lineid, machineid: machineid, stepid: selectedStepData.StepID },
            dataType: "json",
            success: function (result) {
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == false) {
                    layer.alert(jsonresult.msg, {
                        skin: 'layui-layer-molv'
                        , anim: 1
                        , btn: ['确定']
                        , icon: 2
                        , btn1: function (index) {
                            layer.close(index);
                        }
                    });
                    return;
                }

                //轮询采集加工参数
                getParameters(ProductCode.value, orderID.innerHTML, lineid, machineid, selectedStepData.StepID);
            },
            error: function (message) {
                layer.msg(message.statusText, { icon: 2 });
            }
        })
    })
})

//加载工艺参数列表
function loadParameterData(selectedStepData, parameterView, ProductCode, orderID, lineid, machineid) {
    parameterView.innerHTML = "";
    if (selectedStepData.ParameterList == null) { return; }
    if (selectedStepData.ParameterList.length > 0) {
        var tbody = "";
        var table = document.createElement("table");
        table.className = "layui-table";
        table.style = "margin-top:-5px;";
        table.setAttribute("id", "Parameter");
        var thead = document.createElement("thead");
        thead.style = "color:#ffffff";
        table.appendChild(thead);
        var tr = document.createElement("tr");
        thead.appendChild(tr);
        var th = document.createElement("th");
        th.innerHTML = "父级ID";
        th.style.display = "none";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "类型";
        th.style.display = "none";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "序号";
        th.style.display = "none";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "工具ID";
        th.style.display = "none";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "参数位置";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "参数名称";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "数量";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "设计条件";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "公差";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "单位";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "状态";
        tr.appendChild(th);
        var th = document.createElement("th");
        th.innerHTML = "操作";
        tr.appendChild(th);
        tbody = document.createElement("tbody");
        table.appendChild(tbody);
        parameterView.appendChild(table);

        for (var i = 0; i < selectedStepData.ParameterList.length; i++) {
            var tr = document.createElement("tr");
            tbody.appendChild(tr);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Parent_id;
            td.style.display = "none";
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Type;
            td.style.display = "none";
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].No;
            td.style.display = "none";
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Tool_No;
            td.style.display = "none";
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Parameter_Location;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Parameter_Name;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Parameter_Qty;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Conditions;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Tolerance;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Unit;
            tr.appendChild(td);
            var td = document.createElement("td");
            td.innerHTML = selectedStepData.ParameterList[i].Status;
            tr.appendChild(td);
            var td = document.createElement("td");
            var a = document.createElement("a");
            a.href = "javascript:void(0)";
            a.style = "color:#3609f2;text-decoration:underline;";
            a.innerHTML = selectedStepData.ParameterList[i].Action;
            a.addEventListener('dblclick', function (element) {
                var status = $(this).closest('tr').find('td').eq(10).text();
                if (status == "") {
                    layer.msg('先点击设备始能，获取加工数据', { icon: 2 });
                    return;
                }
                var parentid = $(this).closest('tr').find('td').eq(0).text();
                var qty = $(this).closest('tr').find('td').eq(6).text();   //总数量
                var type = $(this).closest('tr').find('td').eq(1).text();   //类型
                layer.open({
                    type: 2,
                    title: ['参数详情', 'color:#fff;background-color:#01AAED;'],
                    shadeClose: true,
                    shade: 0.4,
                    area: ['40%', '90%'],
                    content: "/MES/Action/Details?SN=" + ProductCode.value + "&Work_id=" + orderID.innerHTML +
                        "&Line_id=" + lineid + "&Machine_id=" + machineid + "&Step_id=" + selectedStepData.StepID + "&Parent_id=" + parentid + "&Qty=" + qty + "&Type=" + type + "",
                    cancel: function () {
                        layer.close(layer.index);
                    },
                    end: function () {
                        var isrework = $("#isRework").val();   //明细中是否点击了重做
                        var parentid = $("#parentid").val();   //明细中点击重做的parentid
                        if (isrework == "YES") {
                            $("#Parameter > tbody tr").each(function () {
                                if ($(this).find('td').eq(0).text() == parentid) {
                                    $(this).find('td').eq(10).text("");
                                    $(this).find('td').eq(10).css('backgroundColor', '#ffffff');
                                }
                            })
                            getParameters(ProductCode.value, orderID.innerHTML, lineid, machineid, selectedStepData.StepID);    //开始轮询
                        }
                    }
                })
            });
            td.appendChild(a);
            tr.appendChild(td);
        }
    }

}
//验证是否有同级工序，有重复返回true；否则返回false
function isRepeat(ary) {
    var hash = {};
    for (var i = 0; i < ary.length; i++) {
        if (hash[ary[i].StepLevel]) {
            return true;
        }
        hash[ary[i].StepLevel] = true;
    }
    return false;
}
//全屏
function entryFullScreen() {
    var docE = document.documentElement;
    if (docE.requestFullscreen) {
        docE.requestFullscreen();
    } else if (docE.mozRequestFullScreen) {
        docE.mozRequestFullScreen();
    } else if (docE.webkitRequestFullScreen) {
        docE.webkitRequestFullscreen();
    }
}
//退出全屏
function exitFullScreen() {
    var docE = document;
    if (docE.exitFullscreen) {
        docE.exitFullscreen();
    } else if (docE.mozCancelFullScreen) {
        docE.mozCancelFullScreen();
    } else if (docE.webkitCancelFullScreen) {
        docE.webkitCancelFullScreen();
    }
}
//轮询获取钢印号
function getSNPrint(productCode, machineid, lineid, orderID, productType, planStartDate, planEndDate, orderStatus, parent,
    planTotalQty, okQty, ngQty, surplusQty, repairQty, sopurl, stepId, parameterView, StepList) {
    var snTime;// 轮询读取钢印
    var selectedStepData = [];
    snTime = setInterval(function () {
        $.ajax({
            type: "get",
            url: "/Action/GetSNPrint",
            data: { MachineId: machineid, Lineid: lineid },
            dataType: "json",
            success: function (result) {
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == true) {
                    if (jsonresult.data.length > 0) {
                        parent.innerHTML = "";
                        clearInterval(snTime);
                        var results = jsonresult.data[0];
                        productCode.value = jsonresult.code;
                        orderID.innerHTML = "" + results.OrderID + "";
                        planStartDate.innerHTML = "" + results.PlanStartDate + "";
                        planEndDate.innerHTML = "" + results.PlanEndDate + "";
                        productType.innerHTML = "" + results.ProductType + "";
                        orderStatus.innerHTML = "" + results.OrderStatus + "";
                        for (var i = 0; i < results.StepList.length; i++) {
                            StepList.push(results.StepList[i]);
                            planTotalQty.innerHTML = results.StepList[0].PlanTotalQty;
                            okQty.innerHTML = results.StepList[0].OkQty;
                            ngQty.innerHTML = results.StepList[0].NgQty
                            surplusQty.innerHTML = results.StepList[0].SurplusQty;
                            repairQty.innerHTML = results.StepList[0].RepairQty;
                            stepId.push(results.StepList[0].StepID);
                            sopurl.src =  results.StepList[0].SopUrl;
                            var timediv = document.createElement("div");
                            timediv.style = "height:15px;width:100%";
                            var label = document.createElement("lable");
                            label.setAttribute("id", "time" + results.StepList[i].StepID);
                            label.className = "time-lable";
                            timediv.appendChild(label);
                            if (machineid == "1001") {
                                var label = document.createElement("lable");
                                label.className = "time";
                                label.innerHTML = "已用时间：";
                                timediv.appendChild(label);
                            }
                            parent.appendChild(timediv);
                            var card = document.createElement("div");
                            card.className = "process-card";
                            card.setAttribute("id", results.StepList[i].StepID);
                            if (machineid == "1001") {
                                card.style.backgroundColor = "#52bbf7";
                            }
                            var title = document.createElement("div");
                            title.style = "height:20px;width:100%";
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.innerHTML = "工序号:" + results.StepList[i].StepID + "";
                            title.appendChild(span);
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.innerHTML = "工序名称:" + results.StepList[i].StepName + "";
                            title.appendChild(span);
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.setAttribute("id", "Status" + results.StepList[i].StepID);
                            span.innerHTML = "工序状态:" + results.StepList[i].StepIsPost + "";
                            title.appendChild(span);
                            var span = document.createElement("span");
                            span.className = "tabel-title";
                            span.setAttribute("id", "Check" + results.StepList[i].StepID);
                            span.innerHTML = "质检结果:" + results.StepList[i].StepStatus + "";
                            title.appendChild(span);
                            card.appendChild(title);
                            var table = document.createElement("table");
                            table.className = "layui-table";
                            table.setAttribute("id", "table" + results.StepList[i].StepID);
                            table.style = "width:98%; margin-left:7px;margin-right:9px;";
                            var thead = document.createElement("thead");
                            table.appendChild(thead);
                            var tr = document.createElement("tr");
                            thead.appendChild(tr);
                            var th = document.createElement("th");
                            th.innerHTML = "序号";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "材料编号";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "材料名称及规格";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "需求数量";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var th = document.createElement("th");
                            th.innerHTML = "完成数量";
                            th.style.backgroundColor = "#ffffff";
                            th.style.fontWeight = "bolder";
                            tr.appendChild(th);
                            var tbody = document.createElement("tbody");
                            table.appendChild(tbody);
                            var partData = results.StepList[i].PartList;
                            for (var j = 0; j < partData.length; j++) {
                                var tr = document.createElement("tr");
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].No;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                var a = document.createElement("a");
                                a.innerHTML = partData[j].Material_No;
                                a.style.color = "#ff6a00";
                                a.href = "javascript:void(0);";
                                a.setAttribute("id", partData[j].Material_ImgUrl);
                                a.addEventListener('dblclick', function (element) {
                                    var imgHtml = "<img src='" + element.currentTarget.id + "' width='1000px' height=800px'/>";
                                    //弹出层
                                    layer.open({
                                        type: 1,
                                        shade: 0.8,
                                        offset: 'auto',
                                        area: [1000 + 'px', 850 + 'px'],
                                        shadeClose: true,
                                        scrollbar: false,
                                        title: "物料图片预览",
                                        content: imgHtml,
                                        cancel: function () {
                                        }
                                    });
                                });
                                td.appendChild(a);
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Material_Name;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Need_Qty;
                                tr.appendChild(td);
                                var td = document.createElement("td");
                                td.innerHTML = partData[j].Finish_Qty;
                                tr.appendChild(td);
                                tbody.appendChild(tr);
                            }
                            card.appendChild(table);
                            parent.appendChild(card);
                            var hr = document.createElement("hr");
                            hr.style = "height:1px;border:none;border-top:0px solid #555555;";
                            parent.appendChild(hr);
                        }
                        selectedStepData = StepList[0];
                        loadParameterData(selectedStepData, parameterView, productCode, orderID, lineid, machineid);
                    }
                }
            }
        })
    }, 3000);
}
//轮询message
function getMessage(machineid) {
    var messageTime;// 轮询读取消息
    var IsShow = false;//是否已经弹出  
    messageTime = setInterval(function () {
        var IDlist = [];
        $.ajax({
            type: "get",
            url: "/Action/GetMessage",
            data: { MachineId: machineid },
            dataType: "json",
            success: function (result) {
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == true) {
                    var results = jsonresult.data[0];
                    $("#message").val(JSON.stringify(results));
                    if (results.length > 0) {
                        for (var i = 0; i < results.length; i++) {
                            IDlist.push(results[i].ID);
                        }
                    }
                    if (IsShow == false) {
                        layer.open({
                            type: 2,
                            title: ['错误提示', 'color:#fff;background-color:#f62222;font-size:20px'],
                            shadeClose: false,
                            closeBtn: 0,
                            shade: 0.4,
                            area: ['60%', '60%'],
                            content: "/MES/Action/Message",
                            btn: ['已阅读'],
                            btn1: function (index) {
                                IsShow == false;
                                $.ajax({
                                    type: "get",
                                    url: "/Action/ChangeMessageStatus",
                                    data: { info: JSON.stringify(IDlist)},
                                    dataType: "json",
                                    success: function (result) {

                                    },

                                });
                                layer.close(index);
                                getMessage(machineid);
                            },
                        })
                        IsShow = true;
                        clearInterval(messageTime);
                    }
                }
            }
        })
    }, 3000);
}
//初始化页面
function initform(machineid) {
    if (machineid == "1000" || machineid=="1100") {
        document.getElementById("ProductCode").setAttribute("placeholder", "正在读取钢印号");
        document.getElementById("Release").style.display = "";
        document.getElementById("lock").style.display = "none";
        document.getElementById("unlock").style.display = "none";
        document.getElementById("pass").style.display = "none";
        document.getElementById("Partcode").style.display = "none";
        document.getElementById("devicework").style.display = "none";
        document.getElementById("ngcheck").style.display = "none";
        document.getElementById("QC").style.display = "none";
        document.getElementById("Repair").style.display = "none";
        $("#ProductCode").attr("disabled", true);

    }
    else if (machineid == "1001") {
        document.getElementById("ProductCode").setAttribute("placeholder", "正在读取钢印号");
        document.getElementById("Release").style.display = "none";
        document.getElementById("lock").style.display = "none";
        document.getElementById("unlock").style.display = "none";
        $("#ProductCode").attr("disabled", true);
    }
    else {
        document.getElementById("Release").style.display = "none";
        document.getElementById("lock").style.display = "";
        document.getElementById("unlock").style.display = "";
        document.getElementById("pass").style.display = "";
        document.getElementById("Partcode").style.display = "";
        document.getElementById("devicework").style.display = "";
        document.getElementById("ngcheck").style.display = "";
        document.getElementById("QC").style.display = "";
        document.getElementById("Repair").style.display = "";
        $("#ProductCode").removeAttr("disabled");

        //layer.open({
        //    type: 2,
        //    title: ['打印页面', 'color:#fff;background-color:#01AAED;'],
        //    shadeClose: false,
        //    shade: 0.4,
        //    area: ['40%', '50%'],
        //    content: "/MES/Action/PrintSN",
        //    cancel: function (index) {
        //        layer.close(index);
        //    },
        //})


    }
}
//判断工序是否都已过账
function checkisallPass(value, machineid, lineid) {
    var isContinus;
    var count = 0;
    $.ajax({
        type: "get",
        url: "/Action/GetOrderList",
        async: false,
        dataType: "json",
        data: { Sn: value, MachineId: machineid, LineId: lineid },
        success: function (result) {
            var jsonresult = JSON.parse(result);
            var results = jsonresult.data[0];
            for (let i = 0; i < results.StepList.length; i++) {
                if (results.StepList[i].StepIsPost == "已过账") {
                    count = count + 1;
                }
            }
            if (count == results.StepList.length) {
                isContinus = true;
            } else {
                isContinus = false;
            }
        },
        error: function (message) {
            isContinus = false;
        }
    })
    return isContinus;
}
//轮询加工参数
function getParameters(SN, Workid, Lineid, Machineid, Stepid) {
    var parameterTime;// 轮询采集加工参数  
    //var rows = [];  //取唯一parentid
    parameterTime = setInterval(function () {
        $.ajax({
            type: "get",
            url: "/Action/GetParameters",
            data: { Sn: SN, workid: Workid, lineid: Lineid, machineid: Machineid, stepid: Stepid },
            dataType: "json",
            success: function (result) {
                var myMsg = layer.msg('正在读取加工参数', {
                    shade: 0.4,
                    icon: 16,
                    time: false //取消自动关闭
                });
                var jsonresult = JSON.parse(result);
                var status = jsonresult.success;
                if (status == true) {
                    var results = jsonresult.data[0];
                    //$("#Parameter > tbody tr").each(function () {
                    //    if (has(rows, $(this).find('td').eq(0).text())==false) {
                    //        rows.push($(this).find('td').eq(0).text());
                    //    }
                    //})
                    if (results.length > 0) {
                        layer.close(myMsg);//手动关闭
                        clearInterval(parameterTime);
                        $("#Parameter > tbody tr").each(function () {
                            var parentid = $(this).find('td').eq(0).text();
                            for (var i = 0; i < results.length; i++) {
                                if (parentid == results[i].id) {
                                    $(this).find('td').eq(10).text(results[i].status);
                                    if (results[i].status == "OK") {
                                        $(this).find('td').eq(10).css('backgroundColor', '#0eb626');
                                    }
                                    else {
                                        $(this).find('td').eq(10).css('backgroundColor', '#e10a0a');
                                    }
                                    $(this).find('td').eq(10).css('color', '#ffffff');
                                    break;
                                }
                            }
                        })
                    }
                }
            }
        })
    }, 2000);
}