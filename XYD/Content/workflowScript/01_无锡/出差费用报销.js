﻿/*
 * 变量定义
 */
var isCEO = false;

function onSheetLoad() {
    // 样式先载入
    AddCustomCss();

    // 功能定制跟进
    loadScripts(["/Apps/XYD/Content/sdk/sdk.js"], function () {
        main();
    });
};
function onSheetCheck() {
    var nid = getQueryString("nid");
    if (nid === 'NODE0001') {
        var validLine = CheckLineRequired(11, 17, 1, 13, [1]);
        if (validLine == null) {
            if (!isCEO) {
                return CheckRequiredCells(['#C-5-3']);
            }
        } else {
            return validLine;
        }
    }
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
        RenderPage(MessageID);
        AddClearButtons(11, 17, 13);
    }
}

function RenderPage(MessageID) {
    $.ajax({
        type: "GET",
        url: "/Apps/XYD/Workflow/CheckDirectRefund",
        success: function (data) {
            isCEO = data.Data.isCEO;
            if (isCEO) {
                SetWriteCells(['#C-5-3', '#C-7-13', '#C-8-3', '#C-9-3']);
            } else {
                GetSerialSn(MessageID);
            }
            SetReadonlyCells(['#C-5-3', '#C-7-13', '#C-8-3', '#C-9-3', '#C-18-3', '#C-18-9', '#C-18-12', '#C-19-3', '#C-18-14']);
        }
    })
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
            ShowUnitList("unit", "133px", "184px", "250px", "46px", "624px", serials, '#C-5-13', '#C-5-3', MappingSourceData);
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