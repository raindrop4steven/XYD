function onSheetLoad() {
    // 样式先载入
    AddCustomCss();

    // 功能定制跟进
    loadScripts(["/Apps/XYD/Content/sdk/sdk.js"], function () {
        main();
    });
};
function onSheetCheck() {
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
}

//function showGoods(mid) {
//    $.ajax({
//        url: "/Apps/XYD/Asset/AvailableAssets",
//        type: "GET",
//        success: function (data) {
//            var goodsHtml = '<div id="dialog" title="物品列表"><ul>';
//            data.Data.forEach(function (item) {
//                goodsHtml += '<li><input  class="goodItem" type="checkbox" text="' + item.Name + '">' + item.Name + '库存' + item.Count + '件</li>';
//            });
//            goodsHtml += '</ul><button id="confirmGoods">确定</button></div>';
//            $("#tbSheet").append(goodsHtml);
//            $("#dialog").dialog({
//                title: "请选择物品",
//                modal: true,
//                width: '240px',
//                height: 'auto',
//                resizable: true,
//                open: function () {
//                    $("#dialog").val(goodsHtml);
//                }
//            });
//            $("#confirmGoods").click(function () {
//                var checkedItems = []
//                $(".goodItem").each(function () {
//                    if ($(this).is(':checked')) {
//                        checkedItems.push($(this).attr("text"));
//                    }
//                });
//                console.log(checkedItems.join(","));
//                $.ajax({
//                    url: "/Apps/XYD/Workflow/MappingGoods?mid=" + getQueryString("mid") + "&goods=" + checkedItems.join(","),
//                    type: "GET",
//                    success: function () {
//                        alreadyChoose = true;
//                        location.reload();
//                    },
//                    error: function (error) {
//                        alert(error);
//                    }
//                });
//            });
//        },
//        error: function (error) {
//            console.log(error);
//        }
//    });
//}

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