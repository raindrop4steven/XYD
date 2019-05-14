/*
 * SDK.js 前端表单通用方法和组建抽出
 */

/**************************************************************************
 * STYLE 添加全局样式，修复弹出框被文件选择框挡住的问题
 *************************************************************************/
function AddCustomCss() {
    // 修复弹出框被文件选择框挡住的问题
    var styleTag = $('<style>.DropDown { z-index: 999; } .my-niban-opinion a, .my-opinion a {display:inline-block; margin-right: 10px;border: 1px solid #337ab7;font-size: 13px;padding: 3px 5px;margin-bottom: 10px;margin-left:15px;} .my-niban-opinion textarea, .my-opinion textarea { margin-top: 3px;margin-left: 15px;} #receiveNo > ul > li:hover,div#unit > ul > li:hover {background: rgb(207,229,239);}</style>')

    $('html > head').append(styleTag);
    // 表单居中
    $("#tbSheet").css('margin-left', '-71px');
    $("#page").css('display', 'flex');
    $("#page").css('width', '100%');
    $("#page").css('height', '100%');
    $("#page").css('justify-content', 'center');
}

/*********************************************************************
 * FUNC 草稿类
 *********************************************************************/
// 覆盖默认的【保存草稿】操作
function onSaveDraft() {
    $('#btn-savedraft', top.document).click(function (e) {
        e.preventDefault();
        // 保存映射表数据
        updateTable();
        // 执行默认保存草稿操作
        $(this).unbind('click').click();
    });
}

// 更新映射表
function updateTable() {
    var mid = getQueryString("mid");
    var node = getQueryString("nid");

    $.ajax({
        type: 'GET',
        url: '/Apps/DEP/Workflow/MappingData?mid=' + mid + "&node=" + node,
        success: function (data) {
            console.log("数据映射成功");
        },
        error: function (error) {
            console.log(error);
        }
    });
}

/*************************************************************************
 * FUNC 子流程处理
 *************************************************************************/
// 启动子流程
function startSubflow() {
    var mid = getQueryString("mid");
    var node = getQueryString("nid");

    $.ajax({
        url: '/Apps/DEP/Workflow/StartSubflow',
        type: 'POST',
        data: {
            'mid': mid,
            'nid': node
        },
        success: function (data) {
            if (data.Succeed) {
                // 在新页面打开流程
                window.open(data.Data.Url, '_blank');
            }
        },
        error: function (error) {
            console.log(error);
        }
    })
}

// 添加子流程启动按钮
function addSubWorkflowButton() {
    // 判断当前用户是否具有发起子流程权限
    $.ajax({
        url: '/Apps/DEP/Workflow/CheckSubflowPerm',
        type: 'GET',
        success: function (data) {
            if (data.Data.havePermission) {
                var title_bar = $('#titlebar > ul', top.document);
                // 创建子流程按钮
                var subWorkflow_li = document.createElement('LI');
                var subWorkflowButton = document.createElement('BUTTON');
                subWorkflowButton.setAttribute("id", "btn-custom-sub-workflow");
                subWorkflowButton.onclick = function () {
                    startSubflow();
                };
                var i = document.createElement('I');
                i.className = 'fa fa-upload';
                i.textContent = '  启动部门流程';
                subWorkflowButton.appendChild(i);
                subWorkflow_li.appendChild(subWorkflowButton);

                title_bar.prepend(subWorkflow_li);
            }
        },
        error: function (error) {
            console.log(error);
        }
    })
}

/*********************************************************************
 * FUNC 工具类
 *********************************************************************/
// 获取url中的参数
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i"); // 匹配目标参数
    var result = window.location.search.substr(1).match(reg); // 对querystring匹配目标参数
    if (result != null) {
        return decodeURIComponent(result[2]);
    } else {
        return null;
    }
};

function IsWkfRunning() {
    var wkfStatus = $("#left-panel > p:nth-child(3) > span", top.document).text();
    if (wkfStatus === '已完成') {
        return false;
    } else {
        return true;
    }
}

/******************************************************************************************
 *  FUNC 自动选人提交
 ******************************************************************************************/
/**************************************************************************
 * STYLE 为领导添加【已阅】和【提交】按钮
 *************************************************************************/
// 领导按钮
function addCustomButtons(cell_id, nodekey) {
    // 自定义顶部按钮
    var btn_submit = $('#btn-submit', top.document);
    var title_bar = $('#titlebar > ul', top.document);

    var row = cell_id.split('-')[1]
    var col = cell_id.split('-')[2]
    // 隐藏原先的提交按钮
    btn_submit.hide();
    // 创建已阅按钮
    var read_li = document.createElement('LI');
    var read_button = document.createElement('BUTTON');
    read_button.setAttribute("id", "btn-custom-read");
    read_button.onclick = function () {
        SaveCellData(worksheet_id, getQueryString('nid'), row, col, '已阅', '');
        OverrideSubmit(true, cell_id, nodekey);
    };
    var i = document.createElement('I');
    i.className = 'fa fa-send';
    i.textContent = ' 已阅';
    read_button.appendChild(i);
    read_li.appendChild(read_button);
    // 创建提交按钮
    var submit_li = document.createElement('LI');
    var submit_button = document.createElement('BUTTON');
    submit_button.setAttribute("id", "btn-custom-send");
    submit_button.onclick = function () { OverrideSubmit(false, cell_id, nodekey) };
    var i = document.createElement('I');
    i.className = 'fa fa-send';
    i.textContent = ' 提交';
    submit_button.appendChild(i);
    submit_li.appendChild(submit_button);
    title_bar.prepend(submit_li);
    title_bar.prepend(read_li);
}

function OverrideSubmit(IsRead, CellID, NodeKey) {
    // Fix: 领导意见为空，不能提交
    // 领导意见第一个人提交和后续人提交对应的位置不同
    var currentOpinion = "";
    if ($(".cell-current").length > 0) { // 后续领导提交
        currentOpinion = $(".cell-current").text();
    } else {
        // 首位领导提交
        currentOpinion = $(CellID).text();
    }
    if (IsRead || currentOpinion != undefined && currentOpinion.length > 0) {
        // TODO： 判断是否需要覆盖默认的提交操作
        // 流程ID
        var MessageID = getQueryString("mid");
        // 节点ID
        var FromNodeKey = getQueryString("nid");
        // 前一个节点
        var NodeKey = NodeKey;

        // 获取下一个提交人
        $.ajax({
            type: 'GET',
            url: '/Apps/Tiger/Workflow/MessageHandle?MessageID=' + MessageID + '&NodeKey=' + NodeKey + '&FromNodeKey=' + FromNodeKey,
            success: function (result) {
                console.log(result);
                Send(MessageID, NodeKey, FromNodeKey, result.Data.empl);
            }
        });
    } else {
        alert("请输入审批意见");
    }
}

function Send(MessageID, NodeKey, FromNodeKey, empl) {
    $.ajax({
        type: 'POST',
        url: '/Apps/Workflow/Running/Send',
        data: {
            'mid': MessageID,
            'nid': FromNodeKey,
            'targets': '[{"node":"{0}","empl":"{1}","mode":"","memo":""}]'.format(NodeKey, empl),
            'memo': '',
            'memoAtts': [],
            'digitCertData': '',
            'digitSourceData': '',
            'digitSignedData': ''
        },
        success: function (result) {
            // 发送通知，使用无效nodekey，略过设置提醒节点
            var pdata = {}
            pdata.nlist = $.json2str('[]')
            pdata.nodekey = FromNodeKey;
            pdata.mid = MessageID

            $.ajax({
                url: '/Apps/Tiger/Workflow/AddNotifyRecord',
                type: 'POST',
                data: pdata,
                async: false,
                dataType: 'json',
                success: function (r) {
                    console.log(r)
                }
            });
            console.log(result);
            // 刷新网页
            top.location.href = '/Apps/Workflow/Running/Open?mid={0}&sendsucc=1'.format(MessageID);
        },
        error: function (error) {
            console.log(error);
        }
    })
}

// 设置Cell值
function SaveCellData(sid, nid, row, col, val, ival) {
    $.ajax({
        type: 'POST',
        url: '/Apps/Workflow/Worksheet/SaveCell',
        data: {
            'sid': sid,
            'nid': nid,
            'row': row,
            'col': col,
            'val': val,
            'ival': ival
        },
        success: function (result) {
            console.log(result);
        },
        error: function (error) {
            console.log(error);
        }
    });
}

/*************************************************************************
 * FUNC 自定义输入/选择控件
 ************************************************************************/
// 来文单位列表
function ShowUnitList(divId, left, width, options, posCellId, targetValueId) {
    $("#tbSheet").css('position', 'relative');
    var div = document.createElement('DIV');
    div.id = divId;
    div.style = 'cursor: pointer;overflow: visible;position: absolute;left: ' + left + ';top: 200px;display: none;z-index:999;';
    var ul = document.createElement('UL');
    ul.style = ' list-style: none; margin: 0px; padding: 0px; border: 1px solid #000; background-color: #fff; position: absolute; max-height: 160px; overflow: auto; text-align: left; width:' + width + ';';

    options.forEach(function (item) {
        // item
        var li = document.createElement('LI');
        li.style = 'padding: 4px 16px 4px 4px; white-space: nowrap; border-bottom: 1px solid #888;hover:red;';
        li.textContent = item.content;
        ul.append(li);
    });
    div.append(ul);
    $(posCellId).after(div);

    var showUnitButton = document.createElement('BUTTON');
    showUnitButton.id = "showUnitButton";
    showUnitButton.onclick = function () {
        // $('#unit').toggle();
        var display = $('#' + divId).css('display');
        if (display === 'none') {
            $('#' + divId).css('display', 'block');
        } else {
            $('#' + divId).css('display', 'none');
        }
    };
    showUnitButton.style = 'height:72px;position:absolute;right:483px;border:none;outline:none;padding:3px 12px;font-size:14px; background: url(/Apps/Workflow/images/drop.png) no-repeat right center;';

    $(posCellId).after(showUnitButton);

    // 设置li点击事件
    $(document).on('click', "#" + divId + "> ul > li", function (event) {
        console.log(event.target);
        console.log($(this).text());
        if (event.target.id !== 'deleteMe') {
            var opinion = $(this).contents().filter(function () { return this.nodeType == 3; })[0].nodeValue;
            $(targetValueId).text(opinion);
            SaveCellData(worksheet_id, getQueryString('nid'), targetValueId.split('-')[1], targetValueId.split('-')[2], opinion, '');
            $("#" + divId).css('display', 'none');
        }
    });
    $("#tbSheet td").delegate('.EditBox', 'focusout', function () {
        $("#" + divId).css('display', 'none');
    });
    $("#tbSheet td").click(function () {
        $("#" + divId).css('display', 'none');
    });
}

/*************************************************************************
 *  FUNC 审批意见修改
 **************************************************************************/
// 通用修改Cell
function initChangeOpinionCell(MessageID) {
    $.ajax({
        url: "/Apps/DEP/Test/GeneralOpinion?mid=" + MessageID,
        type: 'GET',
        async: false,
        success: function (r) {
            if (r.Succeed == true) {
                r.Data.forEach(function (item) {
                    var cellID = item.cellId;
                    var type = item.type;
                    var history = item.history;
                    var node = item.node;
                    // 使用字典存储每个节点的编辑状态
                    editFlagDict[node] = true;

                    if (type == 0) { // 单人审批意见
                        $(cellID).html("");
                        $(cellID).html(history);
                        // 初始化按钮动作
                        $(cellID + " > .my-opinion").on('click', function () {
                            addModifyTextArea(this, node, type, cellID);
                        });
                    } else { // 多人审批意见
                        var historyCellID = cellID + ' > div.cell-history';
                        if ($(historyCellID).text() == "") { // 当前用户未正在处理
                            $(cellID).html("");
                            $(cellID).html(history);
                            // 初始化按钮动作
                            $(cellID + "> .my-opinion").on('click', function () {
                                addModifyTextArea(this, node, type, cellID);
                            });
                        } else { // 当前用户正在处理，则不显示编辑按钮
                            $(historyCellID).html(history);
                            $(cellID).find(".fa.fa-edit").first().hide();
                        }
                    }
                });
            }
            else {
                console.log(r.Message);
            }
        }
    });
}

//转换编辑区域
function addModifyTextArea(opinionObj, node, type, cellId) {
    if (editFlagDict[node]) {
        //首先清空该td的点击事件
        var opinionVal = $(opinionObj).find('span').text();
        $(opinionObj).find('span').hide();

        //插入textarea 编辑框
        var tarea = $("<textarea />").width('95%').addClass("modify-leader");
        tarea.val(opinionVal)
        tarea.css({
            'resize': 'none',
            'outline': 'none',
            'height': '100%'
        })
        $(opinionObj).append(tarea)
        //添加提交按钮
        $(opinionObj).append(addModifyButton(node, type, cellId))
        $(opinionObj).append(addCancelButton(node, type, cellId))
        // 隐藏编辑图标
        $(cellId).find(".fa.fa-edit").first().hide();
        $(cellId + '> .my-opinion').unbind('click');
    }
    else {
        editFlagDict[node] = true;
        // 显示编辑图标
        $(cellId).find(".fa.fa-edit").first().show();
    }
}

//添加修改的保存按钮
function addModifyButton(node, type, cellId) {
    //创建保存按钮
    var modify_button = document.createElement('A');
    modify_button.textContent = "保存修改"
    modify_button.setAttribute("id", "btn-read-only-modify");
    modify_button.onclick = function () {
        // 流程ID
        var MessageID = getQueryString("mid");
        // 排序
        var order = $(cellId).find('.my-opinion').first().attr("order");
        if (order === undefined) {
            order = "";
        }
        // 获取
        var opinion = $(".modify-leader").val();
        $.ajax({
            url: '/Apps/DEP/Test/AddNewOpinion',
            type: 'POST',
            data: { mid: MessageID, nid: node, opinion: opinion, type: type, cellId: cellId, order: order },
            async: false,
            success: function (r) {
                if (r.Succeed) {
                    top.location.href = '/Apps/Workflow/Running/Open?mid={0}'.format(MessageID);
                }
            }
        });
    }
    return modify_button
}

//添加修改的取消按钮
function addCancelButton(node, type, cellId) {

    var c_button = document.createElement('A');
    c_button.textContent = "取消"
    c_button.setAttribute("id", "btn-modify-cancel");
    c_button.onclick = function () {
        editFlagDict[node] = false;
        $(cellId + '> .my-opinion').find('textarea').remove();
        $(cellId + '> .my-opinion').find('a').remove();
        $(cellId + '> .my-opinion').find('span').show();
        $(cellId + '> .my-opinion').on('click', function () {
            addModifyTextArea(this, node, type, cellId)
        });
    }
    return c_button
}