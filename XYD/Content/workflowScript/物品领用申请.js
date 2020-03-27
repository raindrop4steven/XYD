function onSheetLoad() {
    // 样式先载入
    AddCustomCss();

    // 功能定制跟进
    loadCss("/Apps/XYD/Content/static/layui/css/layui.css");
    loadScripts(["/Apps/XYD/Content/sdk/sdk.js", "/Apps/XYD/Content/static/layui/layui.js"], function () {
        main();
    });
};
function onSheetCheck() {
};
function onAnyCellUpdate(row, col) {
    OpinionChanged(row, col);
};

// 每个表单的定制入口
function main() {
    /*
     * 参数获取
     */
    // 获取节点ID
    var nid = getQueryString("nid");
    //当没有节点Id 所以处于只读状态 初始化按钮
    var MessageID = getQueryString("mid");
    // 保存草稿
    onSaveDraft();
    if (nid === 'NODE0001') {
        AddClearButtons(6, 10, 11);
        
        AddSelectGoodEvent();
    }
}

// 给商品选择添加事件
function AddSelectGoodEvent() {
    $("#C-6-3").click(function () { ShowGoods("C-6-3", "C-6-5", "C-6-9"); })
    $("#C-7-3").click(function () { ShowGoods("C-7-3", "C-7-5", "C-7-9"); })
    $("#C-8-3").click(function () { ShowGoods("C-8-3", "C-8-5", "C-8-9"); })
    $("#C-9-3").click(function () { ShowGoods("C-9-3", "C-9-5", "C-9-9"); })
    $("#C-10-3").click(function () { ShowGoods("C-10-3", "C-10-5", "C-10-9"); })
    $("#C-11-3").click(function () { ShowGoods("C-11-3", "C-11-5", "C-11-9"); })
}
// 展示商品页面
function ShowGoods(nameId, modelId, unitId) {
    var MessageID = getQueryString("mid");
    layui.use(['layer'], function () {
        var layer = layui.layer;
        layer.ready(function () {
            layer.open({
                type: 2,
                title: '库存物品清单',
                content: [window.location.origin + "/Apps/XYD/Home/ShowGoods?nameId=" + nameId + "&modelId=" + modelId + "&unitId=" + unitId + "&mid=" + MessageID, 'no'],
                area: ['820px', '424px']
            });
        });
    });
}

/******************************************************************
 * 工具方法 【必须】
 * ****************************************************************/
function loadScripts(array, callback) {
    var loader = function (src, handler) {
        var script = document.createElement("script");
        script.src = src;
        script.onload = script.onreadystatechange = function () {
            script.onreadystatechange = script.onload = null;
            handler();
        }
        var head = document.getElementsByTagName("head")[0];
        (head || document.body).appendChild(script);
    };
    (function run() {
        if (array.length != 0) {
            loader(array.shift(), run);
        } else {
            callback && callback();
        }
    })();
}

function loadCss(url) {
    var link = document.createElement("link");
    link.href = url;
    link.type = "text/css";
    link.rel = "stylesheet";
    link.media = "screen,print";
    document.getElementsByTagName("head")[0].appendChild(link);
}

// 通用样式修改
function AddCustomCss() {
    // 表单居中
    $("#tbSheet").css('margin-left', '-25px');
    $("#page").css('display', 'flex');
    $("#page").css('width', '100%');
    $("#page").css('height', '100%');
    $("#page").css('justify-content', 'center');
}