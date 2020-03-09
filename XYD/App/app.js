(function () {
    'use strict';

    var app = angular.module('DEPApp', [
        'ngAnimate',
        'ngRoute'
    ]);
    app.run(['$rootScope', '$location', function ($rootScope, $location) {
        $rootScope.$on('$routeChangeStart', function () {
            var topNav = angular.element('#appkiz-frame-tabmenus').find('li');

            topNav.removeClass('selected');


            var frameMenu = angular.element('[href="/Apps/XYD/index.html#' + $location.path() + '"]');
            for (var i = 0; i < frameMenu.length; i++) {
                var menu = frameMenu[i];
                menu.parentNode.className = 'selected';
            }

        });
    }]);
    app.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {

        //这是因为Angular 1.6 版本更新后 对路由做的处理，这样才可以和以前版本一样正常使用
        //$locationProvider.hashPrefix('');

        $routeProvider.when('/', { templateUrl: './App/home.html', controller: 'HomeController', reloadOnSearch: false, styleUrls: ['./Content/css/home.css'] });
        $routeProvider.when('/index', { templateUrl: './App/home.html', controller: 'HomeController', reloadOnSearch: false, styleUrls: ['./Content/css/home.css'] });
        $routeProvider.when('/search', { templateUrl: './App/search.html', controller: 'SearchController', reloadOnSearch: false, styleUrls: ['./Content/css/search.css'] });
    }]);

    app.service('httpService', function ($http) {
        return {
            post: function (url, data, callback, error) {
                $http.post(url, data).success(function (r) {
                    if (typeof (r.Success) != 'undefined') {
                        if (r.Success) {
                            if (typeof (callback) == 'function')
                                callback(r);
                        }
                        else {
                            if (r.Message == 'notlogin') {
                                //var encodedUrl = escape(location.pathname + location.hash);
                                window.location = '/Admin/Index.html';
                            }
                            else {
                                if (typeof (error) == 'function') {
                                    error(r.Message);
                                }
                                else {
                                    alert(r.Message);
                                    callback(r);
                                }
                            }
                        }
                    }
                    else {
                        if (typeof (callback) == 'function')
                            callback(r);
                    }
                });
            },
            getPage: function (url, data, callback) {
                $http.post(url, data).success(function (r) {
                    callback(r);
                });
            },
            get: function (url, data, callback, error) {
                $http.get(url, data).success(function (r) {
                    callback(r);
                });
            },
            download: function (url, params, callback, error) {
                var load = new Loading();
                load.init();
                load.start();
                $http({
                    url:url,
                    method: "GET",//接口方法
                    params: params,
                        headers: {
                            'Content-type': 'application/zip'
                        },
                    responseType: 'arraybuffer'
                }).success(function (data, status, headers, config) {
                    load.stop();
                    var blob = new Blob([data], { type: "application/zip" });
                    var objectUrl = URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    document.body.appendChild(a);
                    a.setAttribute('style', 'display:none');
                    a.setAttribute('href', objectUrl);
                    var filename = '' + moment(new Date()).format('YYYYMMDDhhmmss') + '.zip';
                    a.setAttribute('download', filename);
                    a.click();
                    URL.revokeObjectURL(objectUrl);
                    // 删除暂存的文件
                    $http.get('/Apps/Archives/WorkflowQuery/DeleteZip', {}).success(function (r) {
                        callback(r);
                    });

                }).error(function (data, status, headers, config) {
                    alert('出现异常！无法批量导出！');
                    load.stop();
                });

            }
        };
    });

    app.controller('MainController', ['$rootScope', '$scope', '$location', 'httpService', function ($rootScope, $scope, $location, httpService) {
        //console.log('angular init success');
    }]);

    app.controller('HomeController', ['$rootScope', '$scope', '$location', 'httpService', function ($rootScope, $scope, $location, httpService) {
        // 获得待处理审批数量
        function getPendingCount() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetPendingCount',
                null,
                function (data) {
                    $scope.wkfPendingMessage = data;
                },
                function (data) {
                    
                });
        }
        // 获得已审批
        function getDealWithCount() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetDealWithCount',
                null,
                function (data) {
                    $scope.wkfDealWithMessage = data;
                },
                function (data) {

                });
        }
        // 获得我的待批申请数量
        function getNoCompleteCount() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetNoCompleteCount',
                null,
                function (data) {
                    $scope.wkfNoCompleteMessage = data;
                },
                function (data) {

                });
        }
        // 获得我的已批申请数量
        function getCompleteCount() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetCompleteCount',
                null,
                function (data) {
                    $scope.wkfCompleteMessage = data;
                },
                function (data) {

                });
        }
        // 获得我的草稿数量
        function getDraftCount() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetDraftCount',
                null,
                function (data) {
                    $scope.wkfDraftMessage = data;
                },
                function (data) {

                });
        }
        //获取登录用户的角色
        function getRoleInfo() {
            httpService.post(
                '/Apps/Archives/WorkflowQuery/GetRoleInfo',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.roleArr = data;
                    $scope.isDeptAdmin = false;
                    _.map(data, function (o) {
                        if (o.RoleID === '794b8c23-ebc8-4e05-ab28-699210daaf78') {
                            $scope.isDeptAdmin = true;
                            return;
                        }
                    });
                },
                function (data) {
                    //console.log('error', data);
                });
        }
        //获取所有工作流模板
        function GetTempList() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetTempList',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.wkfDefaultTempList = data.Data;
                },
                function (data) {
                    //console.log('error', data);
                });
        }
        //获取有权限的流程模板
        function findTemplates() {
            httpService.post(
                '/Apps/Workflow/Start/FindTemplates?folder=&name=',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.wkfTempList = _.sortBy(data, 'CategoryName');
                },
                function (data) {
                    //console.log('error', data);
                });
        }

        //获取收文序列
        function getSequenceInfo() {
            httpService.post(
                '/Apps/Archives/WorkFlowPage/GetSequenceInfo',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.swxl = data;
                },
                function (data) {
                    //console.log('error', data);
                });
        }

        $scope.sfwrq = null;
        $scope.fqsj = null;
        $scope.zzsj = null;
        $scope.fqr = null;

        //绑定日期控件
        function onClickDate() {
            $('input.select-date').click(function () {
                var obj = $(this);
                $('#dlg-date-selector').dialog({
                    autoOpen: true,
                    modal: true,
                    title: '选择日期区间',
                    buttons: [
					{
					    text: '确定', click: function () {
					        var d1 = $('#date-from').val();
					        var d2 = $('#date-to').val();
					        obj.val(d1 + '至' + d2);
					        obj.trigger('input');
					        obj.trigger('change');

					        var t1 = moment("d1", "yyyy-MM-dd hh:mm:ss");
					        obj.next().val(d1 + ' 00:00:00');
					        var t2 = moment("d2", "yyyy-MM-dd hh:mm:ss");
					        obj.next().next().val(d2 ? d2 + ' 23:59:59' : '');

					        obj.next().trigger('input');
					        obj.next().next().trigger('input');

					        obj.next().trigger('change');
					        obj.next().next().trigger('change');

					        obj.attr('data-from', d1);
					        obj.attr('data-to', d2);
					        $('#dlg-date-selector').dialog('close');
					    }
					},
					{ text: '取消', click: function () { $('#dlg-date-selector').dialog('close'); } }
                    ]
                });
            });

            var dates = $('#date-from, #date-to').datepicker({
                defaultdate: '+1w',
                changeMonth: true,
                numberOfMonths: 2,
                regional: $.datepicker.regional['zh-CN'],
                onSelect: function (selectedDate) {
                    var option = this.id == "date-from" ? "minDate" : "maxDate",
					instance = $(this).data("datepicker"),
					date = $.datepicker.parseDate(
						instance.settings.dateFormat || $.datepicker._defaults.dateFormat,
						selectedDate,
						instance.settings);
                    dates.not(this).datepicker("option", option, date);
                }
            });
            $('#date-from, #date-to').datepicker($.datepicker.regional["zh-CN"]);
        }

        $('input.select-empl').click(function () {
            var obj = $(this);
            appkiz.people.getPeopleDlg({
                multi: false,
                ok: function (r) {
                    obj.val(r[0].name);
                    obj.attr('data-emplid', r[0].id);
                    obj.trigger('input');
                    obj.trigger('change');

                    obj.next().val(r[0].id);
                    obj.next().trigger('input');
                    obj.next().trigger('change');
                }
            });
        });

        var defaultPaginationConf = {
            //当期页
            currentPage: 1,
            //数据总条数
            totalItems: 0,
            //每页展示的条数
            itemsPerPage: 10,
            //页面数量
            pagesLength: 10,
            //每页展示条数数组
            perPageOptions: [10, 20, 30, 40, 50],
            onChange: function () {
                console.log('开始分页');
                $scope.getPageData($scope.url, $scope.pageSearch);
            }
        };
        $scope.paginationConf = {};
        //初始化分页参数
        angular.copy(defaultPaginationConf, $scope.paginationConf);
        //$scope.paginationConf = defaultPaginationConf;

        var defaultPageSearch = {
            // 申请标题
            Title: null,
            // 来文文号
            DocumentNumber: null,
            // 来文单位
            DocumentUnit: null,
            // 开始收/发文日期
            StartClosedOrHairTime: null,
            // 结束收/发文日期
            EndClosedOrHairTime: null,
            // 序列编号
            SequenceNumber: null,
            // 开始发起时间
            StartCreatedTime: null,
            // 结束发起时间
            EndCreatedTime: null,
            // 开始终止时间
            StartEndTime: null,
            // 结束终止时间
            EndEndTime: null,
            // 开始接收时间
            StartReceiveTime: null,
            // 结束接收时间
            EndReceiveTime: null,
            // 收文序列名称
            SequenceName: null,
            SortColumn: null,
            SortDirection: null,
            PageSize: $scope.paginationConf.itemsPerPage,
            PageNumber: $scope.paginationConf.currentPage,
            QueryCondition: null,
            WorkFlowId: '',
            MessageIssuedBy: null,
        }

        //初始化查询参数
        $scope.pageSearch = {};
        angular.copy(defaultPageSearch, $scope.pageSearch);
        //$scope.pageSearch = defaultPageSearch;

        //当前列表数据
        $scope.activeTableData = [];

        // 隐藏和开启定义
        $scope.showConfig = {
            "needDo": true,
            "iDone": false,
            "mineDoing": false,
            "mineDone": false,
            "mineDraft": false
        }

        $scope.ResetShowConfig = function ResetShowConfig(navId) {
            if (navId == 'needDo') {
                $scope.showConfig['iDone'] = false;
                $scope.showConfig['mineDoing'] = false;
                $scope.showConfig['mineDone'] = false;
                $scope.showConfig['mineDraft'] = false;
            } else if (navId == 'iDone') {
                $scope.showConfig['needDo'] = false;
                $scope.showConfig['mineDoing'] = false;
                $scope.showConfig['mineDone'] = false;
                $scope.showConfig['mineDraft'] = false;
            } else if (navId == 'mineDoing') {
                $scope.showConfig['needDo'] = false;
                $scope.showConfig['iDone'] = false;
                $scope.showConfig['mineDone'] = false;
                $scope.showConfig['mineDraft'] = false;
            } else if (navId == 'mineDone') {
                $scope.showConfig['needDo'] = false;
                $scope.showConfig['iDone'] = false;
                $scope.showConfig['mineDoing'] = false;
                $scope.showConfig['mineDraft'] = false;
            } else {
                $scope.showConfig['needDo'] = false;
                $scope.showConfig['iDone'] = false;
                $scope.showConfig['mineDoing'] = false;
                $scope.showConfig['mineDone'] = false;
            }
            
        }

        $scope.url = 'GetPendingInfo';
        var activeTab = window.location.href.split('#/')[1] || '';

        //待处理
        $scope.getPageData = function getPageData(url, pageSearch) {

            $scope.pageSearch.PageSize = $scope.paginationConf.itemsPerPage;
            $scope.pageSearch.PageNumber = $scope.paginationConf.currentPage;

            httpService.post(
                '/Apps/XYD/WorkFlowPage/' + url,
                pageSearch,
                function (result) {
                    console.log('callback->', result);
                    $scope.activeTableData = result.Data;
                    $scope.paginationConf.totalItems = result.TotalInfo.TotalRecouds;
                },
                function (data) {
                    console.log('error->', data);
                });
        }

        //当前列表页的函数
        $scope.activePageSearchFunc = function activePageSearchFunc(navId) {
            switch (navId) {
                case 'needDo':
                    $scope.url = 'GetPendingInfo';
                    break;
                case 'mineDoing':
                    $scope.url = 'GetNoCompleteInfo';
                    break;
                case 'mineDone':
                    $scope.url = 'GetCompleteInfo';
                    break;
                case 'iDone':
                    $scope.url = 'GetDealWithInfo';
                    break;
                case 'iStop':
                    $scope.url = 'GetTerminationInfo';
                    break;
                case 'mineDraft':
                    $scope.url = 'GetDraftInfo';
                    break;
                default:
                    $scope.url = 'GetPendingInfo';
            }
            $scope.getPageData($scope.url, $scope.pageSearch);
        }
        //点击搜索
        $scope.onSearch = function onSearch(navId) {
            $scope.paginationConf.currentPage = 1;
            $scope.getPageData($scope.url, $scope.pageSearch);
        }
        //重置
        $scope.onReset = function onReset() {
            angular.copy(defaultPageSearch, $scope.pageSearch);
            $scope.sfwrq = null;
            $scope.fqsj = null;
            $scope.zzsj = null;
            $scope.getPageData($scope.url, $scope.pageSearch);
            $scope.fqr = null;
        }
        //导航条切换效果
        $scope.onChangeTemplate = function onChangeTemplate(navId) {
            $scope.ResetShowConfig(navId);
            $scope.showConfig[navId] = !$scope.showConfig[navId];
            $scope.pageConfig.activeNav = navId;
            $scope.pageConfig.activeWorkflow = '';
            $scope.fqr = null;
            //初始化分页参数
            angular.copy(defaultPaginationConf, $scope.paginationConf);
            //初始化查询参数
            angular.copy(defaultPageSearch, $scope.pageSearch);
            $scope.sfwrq = null;
            $scope.fqsj = null;
            $scope.zzsj = null;
            $scope.activePageSearchFunc(navId);
        }
        // 点击模版
        $scope.onChangeWorkflow = function onChangeWorkflow(navId, workflowId) {
            $scope.pageConfig.activeNav = navId;
            $scope.pageConfig.activeWorkflow = workflowId;
            $scope.fqr = null;
            //初始化分页参数
            angular.copy(defaultPaginationConf, $scope.paginationConf);
            //初始化查询参数
            angular.copy(defaultPageSearch, $scope.pageSearch);
            $scope.pageSearch.WorkFlowId = workflowId;
            $scope.sfwrq = null;
            $scope.fqsj = null;
            $scope.zzsj = null;
            $scope.activePageSearchFunc(navId);
        }
        // 回车事件
        $scope.enterEvent = function (e) {
            var keycode = window.event ? e.keyCode : e.which;
            if (keycode == 13) {
                $scope.onSearch();
            }
        }
        // 更新活跃工作流模版
        $scope.updateActiveWorkflow = function () {
            $scope.pageConfig.activeWorkflow = $scope.pageSearch.WorkFlowId;
        }

        function init() {
            $scope.titleObj = {
                //待处理
                needDo: ['序号', '申请类型', '申请标题',  '申请日期', '接收时间', '发起人', '发起时间', '当前环节'],
                //我发出未完成
                mineDoing: ['序号', '申请类型', '申请标题', '申请日期', '发起时间', '当前环节'],
                //我发出已完成
                mineDone: ['序号', '申请类型', '申请标题', '申请日期', '发起时间', '完成时间'],
                //我处理过的
                iDone: ['序号', '申请类型', '申请标题', '申请日期', '接收时间', '发起人', '发起时间', '状态'],
                //我终止的
                iStop: ['序号', '申请类型', '申请标题', '申请日期', '发起时间', '终止时间'],
                //我的草稿
                mineDraft: ['序号', '申请类型', '申请标题', '申请日期', '发起时间', '起草时间']
            }

            $scope.pageHeader = {
                //待处理
                needDo: '待处理审批',
                //我发出未完成
                mineDoing: '我的待批申请',
                //我发出已完成
                mineDone: '我的已批申请',
                //我处理过的
                iDone: '已处理审批',
                //我终止的
                iStop: '已终止审批',
                //我的草稿
                mineDraft: '草稿'
            }


            $scope.pageConfig = {
                activeNav: 'needDo',
                activeWorkflow: ''
            }

            getPendingCount();
            getDealWithCount();
            getNoCompleteCount();
            getCompleteCount();
            getDraftCount();
            GetTempList();
            getRoleInfo();
            findTemplates();
            getSequenceInfo();
            onClickDate();
           
            //当前浏览器标签页为显性时，刷新页面
            document.addEventListener('visibilitychange', function () {
                if (document.visibilityState == 'visible' && activeTab === 'index' && $scope.pageConfig.activeNav === 'needDo') {
                    window.location.reload();
                }
            });
            //定时刷新
            setTimeout(function () { if (activeTab === 'index' && $scope.pageConfig.activeNav === 'needDo') { window.location.reload(); } }, 180000);
        };
        init();
    }]);

    app.controller('SearchController', ['$rootScope', '$scope', '$location', 'httpService', function ($rootScope, $scope, $location, httpService) {

        //获取所有工作流模板
        function GetTempList() {
            httpService.post(
                '/Apps/XYD/WorkflowPage/GetTempList',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.wkfDefaultTempList = data.Data;
                },
                function (data) {
                    //console.log('error', data);
                });
        }
        //获取有权限的流程模板
        function findTemplates() {
            httpService.post(
                '/Apps/Workflow/Start/FindTemplates?folder=&name=',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.wkfTempList = _.sortBy(data, 'CategoryName');
                },
                function (data) {
                    //console.log('error', data);
                });
        }

        //获取收文序列
        function getSequenceInfo() {
            httpService.post(
                '/Apps/Archives/WorkFlowPage/GetSequenceInfo',
                null,
                function (data) {
                    //console.log('callback', data);
                    $scope.swxl = data;
                },
                function (data) {
                    //console.log('error', data);
                });
        }

        $scope.sfwrq = null;
        $scope.fqbm = null;
        $scope.fqr = null;
        $scope.activeTableData = [];

        $('input.select-dept').click(function () {
            var obj = $(this);
            appkiz.people.getDepartmentDlg({
                current: '0',
                ok: function (r) {
                    obj.val(r[0].name);
                    obj.attr('data-deptid', r[0].id);
                    obj.trigger('input');
                    obj.trigger('change');

                    obj.next().val(r[0].id);
                    obj.next().trigger('input');
                    obj.next().trigger('change');
                }
            });
        });
        $('input.select-empl').click(function () {
            var obj = $(this);
            appkiz.people.getPeopleDlg({
                multi: false,
                ok: function (r) {
                    obj.val(r[0].name);
                    obj.attr('data-emplid', r[0].id);
                    obj.trigger('input');
                    obj.trigger('change');

                    obj.next().val(r[0].id);
                    obj.next().trigger('input');
                    obj.next().trigger('change');
                }
            });
        });

        //绑定日期控件
        function onClickDate() {
            $('input.select-date').click(function () {
                var obj = $(this);
                $('#dlg-date-selector2').dialog({
                    autoOpen: true,
                    modal: true,
                    title: '选择日期区间',
                    buttons: [
					{
					    text: '确定', click: function () {
					        var d1 = $('#date-from2').val();
					        var d2 = $('#date-to2').val();
					        obj.val(d1 + '至' + d2);
					        obj.trigger('input');
					        obj.trigger('change');

					        var t1 = moment("d1", "yyyy-MM-dd hh:mm:ss");
					        obj.next().val(d1 + ' 00:00:00');
					        var t2 = moment("d2", "yyyy-MM-dd hh:mm:ss");
					        obj.next().next().val(d2 ? d2 + ' 23:59:59' : '');

					        obj.next().trigger('input');
					        obj.next().next().trigger('input');

					        obj.next().trigger('change');
					        obj.next().next().trigger('change');

					        obj.attr('data-from', d1);
					        obj.attr('data-to', d2);
					        $('#dlg-date-selector2').dialog('close');
					    }
					},
					{ text: '取消', click: function () { $('#dlg-date-selector2').dialog('close'); } }
                    ]
                });
            });

            var dates = $('#date-from2, #date-to2').datepicker({
                defaultdate: '+1w',
                changeMonth: true,
                numberOfMonths: 2,
                regional: $.datepicker.regional['zh-CN'],
                onSelect: function (selectedDate) {
                    var option = this.id == "date-from2" ? "minDate" : "maxDate",
					instance = $(this).data("datepicker"),
					date = $.datepicker.parseDate(
						instance.settings.dateFormat || $.datepicker._defaults.dateFormat,
						selectedDate,
						instance.settings);
                    dates.not(this).datepicker("option", option, date);
                }
            });
            $('#date-from2, #date-to2').datepicker($.datepicker.regional["zh-CN"]);
        }

        var defaultPaginationConf = {
            //当期页
            currentPage: 1,
            //数据总条数
            totalItems: 0,
            //每页展示的条数
            itemsPerPage: 10,
            //页面数量
            pagesLength: 10,
            //每页展示条数数组
            perPageOptions: [10, 20, 30, 40, 50],
            onChange: function () {
                console.log('开始分页');
                $scope.getPageData($scope.url, $scope.pageSearch);
            }
        };
        $scope.paginationConf = {};
        //初始化分页参数
        angular.copy(defaultPaginationConf, $scope.paginationConf);
        //$scope.paginationConf = defaultPaginationConf;

        var defaultPageSearch = {
            // 申请标题
            QueryCondition: null,
            // 来文文号
            DocumentNumber: null,
            // 来文单位
            DocumentUnit: null,
            // 开始收/发文日期
            StartClosedOrHairTime: null,
            // 结束收/发文日期
            EndClosedOrHairTime: null,
            // 流程状态  流程状态：0：草稿 1：运行中 2：已完成 3：终止信息
            MessageStatus: '',
            // 发起人Id
            MessageIssuedBy: null,
            // 发起人部门Id
            MessageIssuedDept: null,
            // 收文序列名称
            SequenceName: '',
            // 收文序列号
            SequenceNumber: null,
            SortColumn: null,
            SortDirection: null,
            PageSize: $scope.paginationConf.itemsPerPage,
            PageNumber: $scope.paginationConf.currentPage,
            //QueryCondition: null,
            WorkFlowId: '',
        }

        //初始化查询参数
        $scope.pageSearch = {};
        angular.copy(defaultPageSearch, $scope.pageSearch);

        //查询
        $scope.getPageData = function getPageData() {

            $scope.pageSearch.PageSize = $scope.paginationConf.itemsPerPage;
            $scope.pageSearch.PageNumber = $scope.paginationConf.currentPage;

            httpService.post(
                '/Apps/XYD/WorkflowPage/GetWorkflowQueryInfo',
                $scope.pageSearch,
                function (result) {
                    //console.log('callback->', result);
                    $scope.activeTableData = result.Data;
                    $scope.paginationConf.totalItems = result.TotalInfo.TotalRecouds;
                },
                function (data) {
                    console.log('error->', data);
                });
        }

        //点击搜索
        $scope.onSearch = function onSearch() {
            $scope.paginationConf.currentPage = 1;
            $scope.getPageData();
        }
        //重置
        $scope.onReset = function onReset() {
            $scope.sfwrq = null;
            $scope.fqbm = null;
            $scope.fqr = null;
            angular.copy(defaultPageSearch, $scope.pageSearch);
            $scope.getPageData();
        }
        //导出
        $scope.onExport = function onExport() {
            $scope.pageSearch.PageSize = $scope.paginationConf.itemsPerPage;
            $scope.pageSearch.PageNumber = $scope.paginationConf.currentPage;

            var load = new Loading();
            load.init();
            load.start();

            httpService.get(
                '/Apps/Archives/WorkflowQuery/CebGeneratePdf',
                { params: $scope.pageSearch },
                function (data) {
                    load.stop();
                    console.log(data);
                    if (data) {
                        httpService.download(
                            '/Apps/Archives/WorkflowQuery/DownLoadWorkflow',
                            $scope.pageSearch,
                            function (result) {
                                console.log('导出callback->', result);
                            },
                            function (data) {
                                console.log('导出error->', data);
                            });
                    };
                },
                function (data) {
                    alert('文件转化异常！无法批量导出！');
                    load.stop();
                });



        }
        // 回车事件
        $scope.enterEvent = function (e) {
            var keycode = window.event ? e.keyCode : e.which;
            if (keycode == 13) {
                $scope.onSearch();
            }
        }
        function init() {
            $scope.titleObj = ['序号', '申请类型',  '申请标题', '申请日期', '发起部门', '发起人', '流程状态'];
            $scope.wkfStateList = [
                { value: 0, text: '草稿' },
                { value: 1, text: '运行中' },
                { value: 2, text: '已完成' },
                //{ value: 3, text: '终止信息' },
            ];

            $scope.wkfStateStyle = [
                //'text-warning',
                //'text-info',
                //'text-success',
                //'text-danger',
                'label-default',
                'label-info',
                'label-success',
                'label-warning'
            ];

            GetTempList();
            findTemplates();
            getSequenceInfo();
            onClickDate();
            $scope.getPageData();

        };
        init();

    }]);

    //=================分页指令===================
    app.directive('tmPagination', function () {
        return {
            restrict: 'EA',
            scope: {
                conf: '='
            },
            templateUrl: 'App/content_tm_pagination.html',
            replace: true,
            link: function ($scope, $element, $attrs) {

                var conf = $scope.conf;

                // 默认分页长度
                var defaultPagesLength = 9;

                // 默认分页选项可调整每页显示的条数
                var defaultPerPageOptions = [10, 15, 20, 30, 50];

                // 默认每页的个数
                var defaultPerPage = 15;

                // 获取分页长度
                if (conf.pagesLength) {
                    // 判断一下分页长度
                    conf.pagesLength = parseInt(conf.pagesLength, 10);

                    if (!conf.pagesLength) {
                        conf.pagesLength = defaultPagesLength;
                    }

                    // 分页长度必须为奇数，如果传偶数时，自动处理
                    if (conf.pagesLength % 2 === 0) {
                        conf.pagesLength += 1;
                    }

                } else {
                    conf.pagesLength = defaultPagesLength
                }

                // 分页选项可调整每页显示的条数
                if (!conf.perPageOptions) {
                    conf.perPageOptions = defaultPagesLength;
                }

                // pageList数组
                function getPagination(newValue, oldValue) {

                    // conf.currentPage
                    if (conf.currentPage) {
                        conf.currentPage = parseInt($scope.conf.currentPage, 10);
                    }

                    if (!conf.currentPage) {
                        conf.currentPage = 1;
                    }

                    // conf.totalItems
                    if (conf.totalItems) {
                        conf.totalItems = parseInt(conf.totalItems, 10);
                    }

                    // conf.totalItems
                    if (!conf.totalItems) {
                        conf.totalItems = 0;
                        return;
                    }

                    // conf.itemsPerPage
                    if (conf.itemsPerPage) {
                        conf.itemsPerPage = parseInt(conf.itemsPerPage, 10);
                    }
                    if (!conf.itemsPerPage) {
                        conf.itemsPerPage = defaultPerPage;
                    }

                    // numberOfPages
                    conf.numberOfPages = Math.ceil(conf.totalItems / conf.itemsPerPage);

                    // 如果分页总数>0，并且当前页大于分页总数
                    if ($scope.conf.numberOfPages > 0 && $scope.conf.currentPage > $scope.conf.numberOfPages) {
                        $scope.conf.currentPage = $scope.conf.numberOfPages;
                    }

                    // 如果itemsPerPage在不在perPageOptions数组中，就把itemsPerPage加入这个数组中
                    var perPageOptionsLength = $scope.conf.perPageOptions.length;

                    // 定义状态
                    var perPageOptionsStatus;
                    for (var i = 0; i < perPageOptionsLength; i++) {
                        if (conf.perPageOptions[i] == conf.itemsPerPage) {
                            perPageOptionsStatus = true;
                        }
                    }
                    // 如果itemsPerPage在不在perPageOptions数组中，就把itemsPerPage加入这个数组中
                    if (!perPageOptionsStatus) {
                        conf.perPageOptions.push(conf.itemsPerPage);
                    }

                    // 对选项进行sort
                    conf.perPageOptions.sort(function (a, b) { return a - b });


                    // 页码相关
                    $scope.pageList = [];
                    if (conf.numberOfPages <= conf.pagesLength) {
                        // 判断总页数如果小于等于分页的长度，若小于则直接显示
                        for (i = 1; i <= conf.numberOfPages; i++) {
                            $scope.pageList.push(i);
                        }
                    } else {
                        // 总页数大于分页长度（此时分为三种情况：1.左边没有...2.右边没有...3.左右都有...）
                        // 计算中心偏移量
                        var offset = (conf.pagesLength - 1) / 2;
                        if (conf.currentPage <= offset) {
                            // 左边没有...
                            for (i = 1; i <= offset + 1; i++) {
                                $scope.pageList.push(i);
                            }
                            $scope.pageList.push('...');
                            $scope.pageList.push(conf.numberOfPages);
                        } else if (conf.currentPage > conf.numberOfPages - offset) {
                            $scope.pageList.push(1);
                            $scope.pageList.push('...');
                            for (i = offset + 1; i >= 1; i--) {
                                $scope.pageList.push(conf.numberOfPages - i);
                            }
                            $scope.pageList.push(conf.numberOfPages);
                        } else {
                            // 最后一种情况，两边都有...
                            $scope.pageList.push(1);
                            $scope.pageList.push('...');

                            for (i = Math.ceil(offset / 2) ; i >= 1; i--) {
                                $scope.pageList.push(conf.currentPage - i);
                            }
                            $scope.pageList.push(conf.currentPage);
                            for (i = 1; i <= offset / 2; i++) {
                                $scope.pageList.push(conf.currentPage + i);
                            }

                            $scope.pageList.push('...');
                            $scope.pageList.push(conf.numberOfPages);
                        }
                    }

                    $scope.$parent.conf = conf;
                }

                // prevPage
                $scope.prevPage = function () {
                    if (conf.currentPage > 1) {
                        conf.currentPage -= 1;
                    }
                    getPagination();
                    if (conf.onChange) {
                        conf.onChange();
                    }
                };

                // nextPage
                $scope.nextPage = function () {
                    if (conf.currentPage < conf.numberOfPages) {
                        conf.currentPage += 1;
                    }
                    getPagination();
                    if (conf.onChange) {
                        conf.onChange();
                    }
                };

                // 变更当前页
                $scope.changeCurrentPage = function (item) {

                    if (item == '...') {
                        return;
                    } else {
                        conf.currentPage = item;
                        getPagination();
                        // conf.onChange()函数
                        if (conf.onChange) {
                            conf.onChange();
                        }
                    }
                };

                // 修改每页展示的条数
                $scope.changeItemsPerPage = function () {

                    // 一发展示条数变更，当前页将重置为1
                    conf.currentPage = 1;

                    getPagination();
                    // conf.onChange()函数
                    if (conf.onChange) {
                        conf.onChange();
                    }
                };

                // 跳转页
                $scope.jumpToPage = function () {
                    var num = $scope.jumpPageNum;
                    if (num.match(/\d+/)) {
                        num = parseInt(num, 10);

                        if (num && num != conf.currentPage) {
                            if (num > conf.numberOfPages) {
                                num = conf.numberOfPages;
                            }

                            // 跳转
                            conf.currentPage = num;
                            getPagination();
                            // conf.onChange()函数
                            if (conf.onChange) {
                                conf.onChange();
                            }
                            $scope.jumpPageNum = '';
                        }
                    }

                };

                $scope.jumpPageKeyUp = function (e) {
                    var keycode = window.event ? e.keyCode : e.which;

                    if (keycode == 13) {
                        $scope.jumpToPage();
                    }
                }

                $scope.$watch('conf.totalItems', function (value, oldValue) {

                    // 在无值或值相等的时候，去执行onChange事件
                    if (!value || value == oldValue) {

                        if (conf.onChange) {
                            conf.onChange();
                        }
                    }
                    getPagination();
                })
            }
        }
    });
})();
