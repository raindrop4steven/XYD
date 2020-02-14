function onSheetLoad() {
    // 样式先载入
    AddCustomCss();

    // 功能定制跟进
    loadScripts(["/Apps/XYD/Content/sdk/sdk.js"], function () {
        main();
    });
};
function onSheetCheck() {
    return CheckRequiredCells(['#C-5-3']);
};
function onAnyCellUpdate() {
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
        GetSerialSn(MessageID);
        SetReadonlyCells(['#C-7-13', '#C-10-12', '#C-11-12', '#C-12-12', '#C-13-12', '#C-14-12', '#C-15-12', '#C-16-12', '#C-17-3', '#C-18-3',
        '#C-10-13', '#C-11-13', '#C-12-13', '#C-13-13', '#C-14-13', '#C-15-13', '#C-16-13']);
    }
}

function GetSerialSn(mid) {
    $.ajax({
        type: "GET",
        url: "/Apps/XYD/Workflow/GetSourceSerial?mid=" + mid,
        success: function (data) {
            serials = [];
            data.Data.records.forEach(function (item) {
                serials.push(item.Sn);
            });
            // shanghai
            ShowUnitList("unit", "133px", "184px", "254px", "46px", "784px", serials, '#C-5-13', '#C-5-3', MappingSourceData);
            // wuxi
            // ShowUnitList("unit", "133px", "184px", "248px", "46px", "712px", serials, '#C-5-13', '#C-5-3', MappingSourceData);
        }
    })
}

function MappingSourceData() {
    var sn = $("#C-5-3").text();
    var mid = getQueryString("mid");
    $.ajax({
        type: 'GET',
        url: '/Apps/XYD/Workflow/MappingSourceToDest?mid=' + mid + '&sn=' + sn,
        success: function (data) {
            location.reload();
        }
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

// 通用样式修改
function AddCustomCss() {
    // 表单居中
    $("#tbSheet").css('margin-left', '-25px');
    $("#page").css('display', 'flex');
    $("#page").css('width', '100%');
    $("#page").css('height', '100%');
    $("#page").css('justify-content', 'center');
}