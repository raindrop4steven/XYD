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

// 设置只读Cell样式
function SetReadonlyCells(cells) {
    cells.forEach(function (item) {
        $(item).css("background", "#fbfddf");
    });
}

// 设置只读Cell可写入
function SetWriteCells(cells) {
    cells.forEach(function (item) {
        $(item).data("control", 1);
    })
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
        url: '/Apps/XYD/Workflow/MappingData?mid=' + mid + "&node=" + node,
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
        url: '/Apps/XYD/Workflow/StartSubflow',
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
        url: '/Apps/XYD/Workflow/CheckSubflowPerm',
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

// 显示原流程
function ShowOriginWorkflow(mid) {
    // 判断当前用户是否具有发起子流程权限
    $.ajax({
        url: '/Apps/XYD/Workflow/GetOriginWorkflow',
        type: 'POST',
        data: {
            'mid': mid
        },
        success: function (data) {
            if (data.Data.workflowId !== null) {
                var title_bar = $('#titlebar > ul', top.document);
                // 创建显示原流程按钮
                var originWorkflow_li = document.createElement('LI');
                var originWorkflowButton = document.createElement('BUTTON');
                originWorkflowButton.setAttribute("id", "btn-custom-origin-workflow");
                originWorkflowButton.onclick = function () {
                    window.open(data.Data.workflowId, '_blank');
                };
                var i = document.createElement('I');
                i.className = 'fa fa-eye';
                i.textContent = '  查看区级流程';
                originWorkflowButton.appendChild(i);
                originWorkflow_li.appendChild(originWorkflowButton);

                title_bar.prepend(originWorkflow_li);
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
 *  FUNC 领导用，非选人式【已阅】/【提交】按钮
 ******************************************************************************************/
function RenderCustomButton(mid) {
    $.ajax({
        url: '/Apps/XYD/Workflow/GetDetailInfo',
        type: 'POST',
        data: {
            mid: mid
        },
        success: function (data) {
            if (data.Succeed) { // 如果返回正常
                var control = data.Data.control.showReadedButton;
                if (control.show) { // 如果需要显示已阅按钮
                    var targetNodeKey = control.targetNodeKey;
                    var targetCellId = '#C-' + control.row + '-' + control.col;
                    addCustomButtons(targetCellId, targetNodeKey);
                }
            } else {
                console.log(data.Message);
            }
        },
        error: function (error) {
            console.log(error);
        }
    });
}
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
// 下拉/输入列表
function ShowUnitList(divId, choiceLeft, choiceTop, choiceWidth, buttonHeight, buttonRight, options, posCellId, targetValueId, callback) {
    $("#tbSheet").css('position', 'relative');
    $(targetValueId).css("background", "#fbfddf");
    var div = document.createElement('DIV');
    div.id = divId;
    div.style = 'cursor: pointer;overflow: visible;position: absolute;left: ' + choiceLeft + ';top: ' + choiceTop + ';display: none;z-index:999;';
    var ul = document.createElement('UL');
    ul.style = ' list-style: none; margin: 0px; padding: 0px; border: 1px solid #000; background-color: #fff; position: absolute; max-height: 160px; overflow: auto; text-align: left; width:' + choiceWidth + ';';

    options.forEach(function (item) {
        // item
        var li = document.createElement('LI');
        li.style = 'padding: 4px 16px 4px 4px; white-space: nowrap; border-bottom: 1px solid #888;hover:red;';
        li.textContent = item;
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
    showUnitButton.style = 'height:' + buttonHeight + ';position:absolute;right:' + buttonRight + ';border:none;outline:none;padding:3px 12px;font-size:14px; background: url(/Apps/Workflow/images/drop.png) no-repeat right center;';

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
            // 执行回调方法
            callback && callback();
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
 * FUNC 自定义部门选择弹出
 ************************************************************************/
function ShowDeptList(divId, buttonHeight, buttonRight, posCellId, valueCellId) {
    $("#tbSheet").css('position', 'relative');
    // 添加按钮
    var showUnitButton = document.createElement('BUTTON');
    showUnitButton.id = "showUnitButton";
    showUnitButton.style = 'height:' + buttonHeight + ';position:absolute;right:' + buttonRight + ';border:none;outline:none;padding:3px 12px;font-size:14px; background: url(/Apps/Workflow/images/drop.png) no-repeat right center;';
    showUnitButton.onclick = function () {
        $('#' + divId).empty().load('/Apps/People/Shared/GetDepartment.aspx?cr=0&rp=&cbx=true').dialog('open');
        return;
    };
    $(posCellId).after(showUnitButton);

    //添加选择部门的页面
    $(document.body).append('<div id=' + divId + '></div>')
    $('#' + divId).dialog({
        title: '选择部门', autoOpen: false, modal: true, width: 750, height: 600, resizable: false,
        beforeClose: function () { $('#divDept').empty(); },
        buttons: [{
            text: '确定', click: function () {
                try {
                    var depts = getSelectedDept();
                    var resultDepts = [];
                    for (i = 0 ; i < depts.length ; i++) {
                        resultDepts.push(depts[i].name);
                    }
                    var deptstr = resultDepts.join(',');
                    $(valueCellId).text(deptstr);
                    SaveCellData(worksheet_id, getQueryString('nid'), valueCellId.split('-')[1], valueCellId.split('-')[2], deptstr, '');
                    $('#' + divId).dialog('close');
                } catch (e) {
                    alert('出错，请刷新页面。' + '\r\n' + e.message);
                    location.reload();
                }
            }
        },
			{
			    text: '取消', click: function () {
			        $('#' + divId).dialog('close');
			    }
			}]
    });
}
/*************************************************************************
 *  FUNC 审批意见修改
 **************************************************************************/
// 通用修改Cell
function initChangeOpinionCell(MessageID, callback) {
    $.ajax({
        url: "/Apps/XYD/Test/GeneralOpinion?mid=" + MessageID,
        type: 'GET',
        async: false,
        success: function (r) {
            if (r.Succeed == true) {
                // 全局修改状态
                editFlagDict = {};
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
                            addModifyTextArea(this, node, type, cellID, callback);
                        });
                    } else { // 多人审批意见
                        var historyCellID = cellID + ' > div.cell-history';
                        if ($(historyCellID).text() == "") { // 当前用户未正在处理
                            $(cellID).html("");
                            $(cellID).html(history);
                            // 初始化按钮动作
                            $(cellID + "> .my-opinion").on('click', function () {
                                addModifyTextArea(this, node, type, cellID, callback);
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
function addModifyTextArea(opinionObj, node, type, cellId, callback) {
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
        $(opinionObj).append(addModifyButton(node, type, cellId, callback))
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
function addModifyButton(node, type, cellId, callback) {
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
            url: '/Apps/XYD/Test/AddNewOpinion',
            type: 'POST',
            data: { mid: MessageID, nid: node, opinion: opinion, type: type, cellId: cellId, order: order },
            async: false,
            success: function (r) {
                if (r.Succeed) {
                    top.location.href = '/Apps/Workflow/Running/Open?mid={0}'.format(MessageID);
                    callback && callback(MessageID, node, type);
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

/******************************************************************************************
 *  FUNC 个人意见部分
 ******************************************************************************************/

function GetOpinionConfig(mid, nid) {
    $.ajax({
        url: "/Apps/XYD/Workflow/GetPrivateOpinionConfig",
        type: "POST",
        data: {
            mid: mid
        },
        success: function (data) {
            var opinionConfig = null;
            var i = 0;
            for (i = 0; i < data.Data.opinions.length; i++) {
                var item = data.Data.opinions[i];
                if (nid == item.key) {
                    opinionConfig = item.value;
                    break;
                }
                else {
                    continue;
                }
            }
            if (opinionConfig == null) {
                console.log("目前节点无个人意见配置");
            } else {
                RenderPrivateOpinion(mid, nid, opinionConfig);
            }
        },
        error: function (error) {
            console.log(error);
        }
    })
}
// 渲染个人意见
function RenderPrivateOpinion(mid, nid, config) {
    // 意见表格ID
    opinion_cell_id = config.valueCellId;
    // 意见历史表格ID
    opinion_cell_history_id = opinion_cell_id + ' > div.cell-history';;
    // 当前意见表格ID
    opinion_cell_current_id = opinion_cell_id + ' > div.cell-current';;
    // 个人意见表格ID
    opinion_list_button_id = config.optionCellId;

    // 添加个人意见列表
    AddPrivateOpinion(opinion_list_button_id);
    ShowPrivateOpinion(opinion_list_button_id);
}

// 插入【添加】按钮
function addInsertSelfButton() {
    // 获得表单
    $("#tbSheet").css('position', 'relative');
    $(opinion_cell_id).after('<button onClick="AddPrivateOpinionToDB()" style="position:absolute;right:55px;border-radius:4px;border:1px solid #28a4c9;padding:3px 12px;font-size:14px;color:#fff;background-color:#5bc0de;">添加个人意见</button>');
}
// 意见插入
function opinionChanged(id) {
    var opinion = $(id).text();
    InsertMultiOpinion(opinion);
    ResetPublicOpinion(id);
}

// 插入多人意见方法
function InsertMultiOpinion(opinion) {
    var selector = opinion_cell_history_id;
    if ($(selector).text() == "") {
        selector = opinion_cell_id;
    } else {
        selector = opinion_cell_current_id;
    }
    $(selector).html(opinion);
    nid = getQueryString("nid");
    SaveCellData(worksheet_id, getQueryString('nid'), opinion_cell_id.split('-')[1], opinion_cell_id.split('-')[2], opinion, '');
}
// 插入个人意见列表
function ShowPrivateOpinion(opinion_button_id) {
    $.get('/Apps/Tiger/Opinion/GetPrivateOpinion', function (result) {
        // 首先移除旧的意见列表
        if ($("#privateOpinion").length > 0) {
            $("#privateOpinion").remove();
        }
        var div = document.createElement('DIV');
        div.id = 'privateOpinion';
        div.style = 'cursor: pointer;overflow: visible;position: relative;left: -4px;top: 15px; display: none;z-index:999;';
        var ul = document.createElement('UL');
        ul.style = ' list-style: none; margin: 0px; padding: 0px; border: 1px solid #000; background-color: #fff; position: absolute; max-height: 160px; overflow: auto; text-align: left;';
        result.Data.opinions.forEach(function (item) {
            // item
            var li = document.createElement('LI');
            li.style = 'padding: 4px 16px 4px 4px; white-space: nowrap; border-bottom: 1px solid #888;';
            li.textContent = item.content;
            // 删除按钮
            var deleteButton = document.createElement('BUTTON');
            deleteButton.id = "deleteMe";
            deleteButton.textContent = "删除";
            deleteButton.setAttribute("opinionId", item.id);
            deleteButton.setAttribute('style', 'margin-right:5px;border-radius:4px;border: 1px solid #28a4c9;padding:5px 12px;font-size:14px;color:#fff;background-color:#5bc0de;');
            li.prepend(deleteButton);
            ul.append(li);
        });
        div.append(ul);
        $(opinion_button_id).append(div);
        // Span元素，显示选中的值
        var span = document.createElement('SPAN');
        span.id = 'selectedPrivateOpinion';
        $(opinion_button_id).append(span);
    });
}
// 插入个人意见列表
function AddPrivateOpinion(opinion_button_id) {
    $(opinion_button_id).attr('class', 'a-c b-l b-r b-t b-b drop control1');
    // 设置点击事件
    $(opinion_button_id).click(function (event) {
        console.log(event.target);
        var privateOpinion = $("#privateOpinion");
        privateOpinion.toggle();
    });
    // 设置li点击事件
    $(document).on('click', "#privateOpinion > ul > li", function (event) {
        console.log(event.target);
        console.log($(this).text());
        if (event.target.id !== 'deleteMe') {
            var opinion = $(this).contents().filter(function () { return this.nodeType == 3; })[0].nodeValue;
            $("#selectedPrivateOpinion").html(opinion);
            InsertMultiOpinion(opinion);
        }
    });
    // 添加【添加个人意见按钮]
    addInsertSelfButton();
    // 个人意见删除按钮
    $(document).on('click', '#deleteMe', function (event) {
        event.stopPropagation();
        event.preventDefault();
        console.log($(this).attr('opinionid'));
        // 判断是否需要删除
        var selectedOpinionText = $('{0} > span'.format(opinion_button_id)).text();
        var parentClone = $(this).parent('li').clone();
        parentClone.find('button').remove();
        var deleteOpinionText = parentClone.text();
        if (deleteOpinionText === selectedOpinionText) {
            $(opinion_button_id).text("");
        }
        DeletePrivateOpinionFromDB($(this).attr('opinionid'));
    });
}

// 添加内容到个人意见
function AddPrivateOpinionToDB() {
    var selector = opinion_cell_history_id;
    if ($(selector).text() == "") {
        selector = opinion_cell_id;
    } else {
        selector = opinion_cell_current_id;
    }
    var privateOpinion = $(selector).html();
    // 调用接口
    $.ajax({
        type: 'POST',
        url: '/Apps/Tiger/Opinion/AddPrivateOpinion',
        data: { 'content': privateOpinion },
        success: function (result) {
            ShowPrivateOpinion(opinion_list_button_id);
            alert("个人意见添加成功");
        },
        error: function (error) {
            console.log(error);
        }
    });

}
// 从DB中删除个人意见
function DeletePrivateOpinionFromDB(id) {
    $.ajax({
        type: 'POST',
        url: '/Apps/Tiger/Opinion/RemovePrivateOpinion',
        data: { 'id': id },
        success: function (result) {
            ShowPrivateOpinion(opinion_list_button_id);
            alert("删除成功");
        },
        error: function (error) {
            console.log("删除失败");
        }
    });
}
// 重置选择后的表格
function ResetPublicOpinion(id) {
    splitArray = id.split('-');
    SaveCellData(worksheet_id, getQueryString('nid'), splitArray[1], splitArray[2], '', '');
}

function CaculateDays(beingID, endID, destID) {
    var beginDateStr = $(beingID).text();
    var endDateStr = $(endID).text();
    if (beginDateStr.length > 0 && endDateStr.length > 0) {
        var time1 = Date.parse(beginDateStr);
        var time2 = Date.parse(endDateStr);
        if (time1 > time2) {
            alert("结束日期不能小于开始日期");
        }
        //讲两个时间相减，求出相隔的天数
        var dayCount = (Math.abs(time2 - time1)) / 1000 / 60 / 60 / 24;
        SaveCellValue($(destID), dayCount + 1);
    }
}

//数字转大写
function MoneyToCapital(n) {
    if (n == 0) {
        return "";
    }
    if (!/^(0|[1-9]\d*)(\.\d+)?$/.test(n))
        return "";
    var unit = "仟佰拾亿仟佰拾万仟佰拾元角分", str = "";
    n += "00";
    var p = n.indexOf('.');
    if (p >= 0)
        n = n.substring(0, p) + n.substr(p + 1, 2);
    unit = unit.substr(unit.length - n.length);
    for (var i = 0; i < n.length; i++)
        str += '零壹贰叁肆伍陆柒捌玖'.charAt(n.charAt(i)) + unit.charAt(i);
    return str.replace(/零(仟|佰|拾|角)/g, "零").replace(/(零)+/g, "零").replace(/零(万|亿|元)/g, "$1").replace(/(亿)万|壹(拾)/g, "$1$2").replace(/^元零?|零分/g, "").replace(/元$/g, "元整");
}

function ConvertToCapitalCell(cell1, cell2) {
    var money = $(cell1).text();
    SaveCellValue($(cell2), MoneyToCapital(money));
}

// 检查单元格是否为空
function CheckRequiredCells(cells) {
    var result = undefined;
    cells.forEach(function (item) {
        var value = $(item).text();
        if (!value) {
            result = '您尚未填写全部内容';
        }
    })
    return result;
}

// 检查住宿费用是否超过标准
function CheckHotelLimit(cityCellId, dayCellId, feeCellId) {
    var city = $(cityCellId).text();
    var day = $(dayCellId).text();
    var realHotel = $(feeCellId).text();
    if (realHotel && (!city || !day)) {
        SaveCellValue($(feeCellId), '');
        alert("请先选择申请编号");
    }
    if (city && day && realHotel) {
        $.ajax({
            'url': '/Apps/XYD/Workflow/CheckHotelLimit',
            'type': 'POST',
            'data': {
                'city': city,
                'day': day,
                'realHotel': realHotel
            },
            'success': function (data) {
                if (data.Succeed == true) {
                    console.log("住宿费用检测通过");
                } else {
                    // SaveCellValue($(feeCellId), '');
                    alert(data.Message);
                }
            },
            'error': function (error) {
                alert(error);
            }
        });
    }
}