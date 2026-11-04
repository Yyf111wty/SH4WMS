$(function () {
    var sopserver = $("#sopserver").val();   //sop图片服务器地址
    var ProductCode = document.getElementById("ProductCode");
    var OrderID = document.getElementById("OrderID");
    var Sopurl = document.getElementById("sopurl"); 
    var Stepid = document.getElementById("stepid");
    var Stepname = document.getElementById("stepname");
    var Result = document.getElementById("result");
    var Content = document.getElementById("work_content");  //已加工工序显示区域
    var PartInfo = document.getElementById("PartInfo");    //物料明细显示区域
    var ParentData = [];   //已加工工序数据源
    var PartData = [];   //工序下物料数据源
    var MaterialData = []; //最终有效的物料条码
    var scanArray = [];//已扫描的物料条码
    $('#fullScreen').bind('click', function () {
        var fullscreenElement = document.fullscreenElement || document.mozFullScreenElement || document.webkitFullscreenElement;
        if (fullscreenElement == null) {
            entryFullScreen();
            $(this).html('<i class="layui-icon layui-icon-screen-restore" style="color: #eeeeee;">退出全屏</i>');
            $("#ProductCode").focus();
            document.getElementById("title").style.display = "";
        } else {
            exitFullScreen();
            $(this).html('<i class="layui-icon layui-icon-screen-full" style=" color: #eeeeee;">全屏</i>');
            document.getElementById("title").style.display = "none";
        }
    })

    $("#ProductCode").keyup(function () {
        if (event.keyCode == 13) {
            if (ProductCode.value == "") {
                return;
            }
            else {
                ParentData = [];
                $.ajax({
                    type: "get",
                    url: "/OfflineRepair/GetWorkedMachines",
                    dataType: "json",
                    data: { Sn: ProductCode.value },
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
                                    OrderID.innerHTML = "";
                                    Sopurl.src = "";
                                    Content.innerHTML = "";
                                    PartInfo.innerHTML = "";
                                    $("#Result").val("");
                                    $("#Reason").val("");
                                    $("#Remark").val(""); 
                                    $("#ProductCode").val("");
                                    $("#ProductCode").focus();
                                }
                            });
                            return;
                        }
                        var results = jsonresult.data[0];
                        OrderID.innerHTML = results.Work_id;
                        for (var i = 0; i < results.LOffLineRepairBaseStepInfo.length; i++) {
                            ParentData.push(results.LOffLineRepairBaseStepInfo[i]);
                            var tr = document.createElement("tr");
                            tr.setAttribute("id", i);
                            tr.addEventListener('click', function (elem) {
                                var rowindex = elem.currentTarget.id;
                                $("#Result").val(results.LOffLineRepairBaseStepInfo[rowindex].Result);
                                $("#Reason").val(results.LOffLineRepairBaseStepInfo[rowindex].Reason);
                                $("#Remark").val(results.LOffLineRepairBaseStepInfo[rowindex].Remark);
                            });
                            var td = document.createElement("td");
                            var checkbox = document.createElement("input");
                            checkbox.type = "checkbox";
                            checkbox.setAttribute("id", i);
                            checkbox.setAttribute("name", "ckb");
                            if (results.LOffLineRepairBaseStepInfo[i].Flag == false) {
                                checkbox.disabled = "disabled";
                            }
                            checkbox.onclick = function (elem) {
                                if (this.checked) {
                                    PartData = [];
                                    var rowindex = elem.currentTarget.id;
                                    Sopurl.src = sopserver+ results.LOffLineRepairBaseStepInfo[rowindex].SopUrl;
                                    Stepid.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].Step_id;
                                    Stepname.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].Step_Name;
                                    Result.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].Result;
                                    for (var j = 0; j < results.LOffLineRepairBaseStepInfo[rowindex].LPartData.length; j++) {
                                        PartData.push(results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j]);
                                        var tr = document.createElement("tr");
                                        var td = document.createElement("td");
                                        td.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j].No;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j].Material_No;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j].Material_Name;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j].Need_Qty;
                                        tr.appendChild(td);
                                        var td = document.createElement("td");
                                        td.innerHTML = results.LOffLineRepairBaseStepInfo[rowindex].LPartData[j].Finish_Qty;
                                        tr.appendChild(td);
                                        PartInfo.appendChild(tr);
                                    }
                                }
                                else {
                                    PartData = [];
                                    PartInfo.innerHTML = "";
                                    Sopurl.src = "";
                                }
                            };
                            td.appendChild(checkbox);
                            tr.appendChild(td);
                            var td = document.createElement("td");
                            td.innerHTML = results.LOffLineRepairBaseStepInfo[i].Id;
                            tr.appendChild(td);
                            var td = document.createElement("td");
                            td.innerHTML = results.LOffLineRepairBaseStepInfo[i].Machine_Name;
                            tr.appendChild(td);
                            var td = document.createElement("td");
                            td.innerHTML = results.LOffLineRepairBaseStepInfo[i].Step_Name;
                            tr.appendChild(td);
                            var td = document.createElement("td");
                            td.innerHTML = results.LOffLineRepairBaseStepInfo[i].Repair_Status;
                            tr.appendChild(td);
                            Content.appendChild(tr);
                        }      
                    },
                    error: function (message) {
                        layer.close(loading);
                        layer.msg('请求服务器出现异常' + message, { icon: 2 });
                    }
                })
            }
        }
    })
    $("#PartCode").keyup(function () {
        if (event.keyCode == 13) {
            var code = document.getElementById("Partcode");
            var checkLength = $("input:checkbox[name='ckb']:checked").length;
            if (checkLength == 0) {
                layer.alert('请先选中需要返修的工序', {
                    skin: 'layui-layer-molv'
                    , anim: 1
                    , btn: ['确定']
                    , icon: 2
                    , btn1: function (index) {
                        layer.close(index);
                        $("#PartCode").val("");
                    }
                });
                return;
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
            for (var i = 0; i < PartData.length; i++) {
                var value = code.value.split("-")[0];  //取物料条码前固定格式
                if (PartData[i].Material_No == value) {   //如果存在物料条码
                    let row = PartData.No;
                    var needQty = document.getElementById("Part_tb").rows[row].cells[3];
                    var finishQty = document.getElementById("Part_tb").rows[row].cells[4];
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
                    if (PartData[i].Material_Type == "Y") {  //如果是关键性物料
                        finishQty = document.getElementById("Part_tb").rows[row].cells[4];
                        finishQty.innerHTML = Number(finishQty.innerHTML) + 1;
                        document.getElementById("Part_tb").rows[row].style.backgroundColor = "#f1fb96";
                    } else {
                        finishQty.innerHTML = needQty.innerHTML;
                    }
                    if (needQty.innerHTML == finishQty.innerHTML) {
                        document.getElementById("Part_tb").rows[row].style.backgroundColor = "#7bfa67";
                    }
                    scanArray.push(code.value);
                    MaterialData.push({ "PartID": PartData[i].Material_No, "PartSN": code.value});
                    $("#Partcode").val("");
                    break;
                }
            }
        }
    })
})

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