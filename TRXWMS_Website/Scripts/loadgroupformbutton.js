$.ajaxSetup({
    beforeSend: function (xhr) {
        var token = getCurrentToken();
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
        if (window.__wmsPageId) {
            xhr.setRequestHeader('X-Page-Id', window.__wmsPageId);
        }
        if (window.__wmsButtonKey) {
            xhr.setRequestHeader('X-Button-Key', window.__wmsButtonKey);
        }
        if (window.__wmsButtonName) {
            xhr.setRequestHeader('X-Button-Name', encodeWmsHeaderValue(window.__wmsButtonName));
        }
    }
});

function encodeWmsHeaderValue(value) {
    if (!value) {
        return value;
    }
    if (/^[\u0000-\u00FF]*$/.test(value)) {
        return value;
    }
    return encodeURIComponent(value);
}

(function initWmsPageContext() {
    if (typeof GetQueryString !== 'function') {
        return;
    }
    var currentPageId = GetQueryString('pageId');
    if (currentPageId) {
        window.__wmsPageId = currentPageId;
    }
})();

$(document).on('click', '#group_div button', function () {
    var name = $(this).text().replace(/\s+/g, '');
    if (name.indexOf('查询') >= 0 || name.indexOf('查看') >= 0) {
        window.__wmsButtonKey = '';
        window.__wmsButtonName = '';
        return;
    }
    window.__wmsButtonKey = this.id || '';
    window.__wmsButtonName = name;
});

//加载按钮
function loadGroupFormButton(pageId, onLoaded) {
    var MyDiv = document.getElementById("group_div");

    getPermissionButtons(pageId, function (result) {
            if (result.length == 0) {
                if (typeof onLoaded === 'function') {
                    onLoaded();
                }
                return;
            }
            if (typeof GetQueryString === 'function') {
                var currentPageId = GetQueryString('pageId');
                if (currentPageId) {
                    window.__wmsPageId = currentPageId;
                }
            }
            for (var i = 0; i < result.length; i++) {
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
                    button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe637;</i>' + result[i].NodeName + '</button> ';
                }
                else if (str.search("指派设备和人员") != -1) {
                    button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#xe613;</i>' + result[i].NodeName + '</button> ';
                }
                else {
                    button = ' <button class="layui-btn layui-btn-sm"    id="' + result[i].KeyName + '" onclick="' + result[i].KeyName + 'Click()"><i  class="layui-icon">&#x1005;</i>' + result[i].NodeName + '</button> ';
                }
                MyDiv.innerHTML += button;
            }
            if (typeof onLoaded === 'function') {
                onLoaded();
            }
    });
}

// 权限按钮缓存：同一标签页内在 TTL 内只请求一次，避免每次切换页面都访问服务端
var __permBtnMemory = {};
var __permBtnPending = {};
var PERM_BTN_CACHE_TTL = 5 * 60 * 1000;

function getPermissionButtonCacheKey(pageId) {
    return 'PERMBTN_' + (getCurrentToken() || '') + '_' + (pageId || '');
}

function readPermissionButtonCache(cacheKey) {
    var memory = __permBtnMemory[cacheKey];
    if (memory && memory.expireAt > new Date().getTime()) {
        return memory.values;
    }
    try {
        var raw = sessionStorage.getItem(cacheKey);
        if (!raw) {
            return null;
        }
        var parsed = JSON.parse(raw);
        if (!parsed || !parsed.expireAt || parsed.expireAt <= new Date().getTime()) {
            sessionStorage.removeItem(cacheKey);
            return null;
        }
        __permBtnMemory[cacheKey] = parsed;
        return parsed.values;
    } catch (e) {
        return null;
    }
}

function writePermissionButtonCache(cacheKey, values) {
    var entry = { values: values || [], expireAt: new Date().getTime() + PERM_BTN_CACHE_TTL };
    __permBtnMemory[cacheKey] = entry;
    try {
        sessionStorage.setItem(cacheKey, JSON.stringify(entry));
    } catch (e) {
        // 超出存储配额时忽略，仅使用内存缓存
    }
}

function clearPermissionButtonCache() {
    __permBtnMemory = {};
    __permBtnPending = {};
    try {
        var keys = [];
        for (var i = 0; i < sessionStorage.length; i++) {
            var key = sessionStorage.key(i);
            if (key && key.indexOf('PERMBTN_') === 0) {
                keys.push(key);
            }
        }
        for (var j = 0; j < keys.length; j++) {
            sessionStorage.removeItem(keys[j]);
        }
    } catch (e) {
    }
}

function getPermissionButtons(pageId, callback) {
    var cacheKey = getPermissionButtonCacheKey(pageId);
    var cached = readPermissionButtonCache(cacheKey);
    if (cached) {
        callback(cached);
        return;
    }

    if (__permBtnPending[cacheKey]) {
        __permBtnPending[cacheKey].push(callback);
        return;
    }
    __permBtnPending[cacheKey] = [callback];

    $.ajax({
        type: "get",
        url: "/Home/GetPermissionBtn",
        dataType: "json",
        data: { pageId: pageId },
        success: function (result) {
            var values = result || [];
            writePermissionButtonCache(cacheKey, values);
            var queue = __permBtnPending[cacheKey] || [];
            delete __permBtnPending[cacheKey];
            for (var i = 0; i < queue.length; i++) {
                queue[i](values);
            }
        },
        error: function (message) {
            var queue = __permBtnPending[cacheKey] || [];
            delete __permBtnPending[cacheKey];
            for (var i = 0; i < queue.length; i++) {
                queue[i]([]);
            }
            if (typeof layer !== 'undefined' && layer.alert) {
                layer.alert(message, { icon: 6 });
            }
        }
    });
}

function getCurrentToken() {
    // 🔥 取当前标签唯一ID
    var tabId = sessionStorage.tabId;
    if (!tabId) return null;

    // 优先URL，其次按标签取token
    var urlParams = new URLSearchParams(window.location.search);
    var t = urlParams.get('token');
    if (t) return t;

    return localStorage.getItem("TOKEN_" + tabId);
}
