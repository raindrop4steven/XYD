using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    public class DEP_Constants
    {
        /// <summary>
        /// 工作流ID
        /// </summary>
        // 传阅节点
        public static string Transfer_Node_Key_Header = "NODE_C_";
        // 发起节点
        public static string Start_Node_Key = "NODE0001";
        /*
         * 极光推送类别
         */
        // 工作流
        public static string JPush_Workflow_Type = "Workflow";

        /*
         * 自定义方法头
         */
        public static string Custom_Func_Header = "#custom";

        /*
         * 工作流操作
         */
        public static string Audit_Operate_Type_Agree = "同意";
        public static string Audit_Operate_Type_Disagree = "驳回";
        public static string Audit_Operate_Type_Start = "发起申请";
        public static string Audit_Operate_Type_End = "结束";

        /*
         * 资产状态:Available-可申领；Used-已申领；Scraped-已报废；
         */
        public static string Asset_Status_Available = "Available";
        public static string Asset_Status_Used = "Used";
        public static string Asset_Status_Scraped = "Scraped";
        /*
         * 资产操作类型: 添加、申请、归还、报废
         */
        public static string Asset_Operation_Add = "Add";
        public static string Asset_Operation_Apply = "Apply";
        public static string Asset_Operation_Return = "Return";
        public static string Asset_Operation_Scrap = "Scrap";

        /*
         * 地区
         */
        public static string System_Config_Area_All = "-1";
        public static string System_Config_Area_SH = "SH";
        public static string System_Config_Name_SH = "上海";
        public static string System_Config_Area_WX = "WX";
        public static string System_Config_Name_WX = "无锡";

        /*
         * 学历列表
         */
        public static string Education_Level_High = "高中";
        public static string Education_Level_Zhuan = "专科";
        public static string Education_Level_Ben = "本科";
        public static string Education_Level_Shuo = "硕士";
        public static string Education_Level_Bo = "博士";

        /*
         * 默认分页
         */
        public static int Page = 0;
        public static int Size = 10;

        /*
         * 默认地区
         */
        public static string Default_Area_Key = "WX";
        public static string Role_Name_WuXi = "无锡";
        public static string Role_Name_ShangHai = "上海";

        /**
         * 请假审批状态
         */
        public static string Leave_Status_YES = "YES";
        public static string Leave_Status_NO = "NO";
        public static string Leave_Status_Auditing = "Auditing";
        public static string Leave_Year_Type = "年假";
        /**
         * 模块代码
         */
        public static string Module_Asset_Code = "Asset";
        public static string Module_Information_Code = "Information";

        /**
         * 模块权限代码
         */
        // 固定资产模块领导权限
        public static string Perm_Asset_Leader = "asset_leader";
        // 个人信息模块领导权限
        public static string Perm_Info_Leader = "information_leader";

        /**
         * 城市级别
         */
        // 1:一线城市（含直辖市）
        // 2：省会城市
        // 3：省辖市、县级市及以下
        // 4：国外
        public static int First_Tier_City = 1;
        public static int Second_Tier_City = 2;
        public static int Third_Tier_City = 3;
        public static int Aboard_City = 4;

        /**
         * 职位
         */
        // 总经理
        public static int CEO = 0;
        // 副总经理
        public static int ViceCEO = 1;
        // 部门经历
        public static int DeptManager = 2;
        // 普通员工
        public static int Staff = 3;

        /**
         * 配置类型
         */
        public static string Config_Type_Main = "main";
        public static string Config_Type_App = "app";
        public static string Config_Type_Audit = "audit";
        public static string Config_Type_Start = "start";
        public static string Config_Type_Event = "event";

        /**
         * 节点类型
         */
        // 发起节点
        public static int NODE_TYPE_START = 1;
        public static int NODE_TYPE_NORMAL = 2;
        public static int NODE_TYPE_AUTO = 3;
        // 结束节点
        public static int NODE_TYPE_END = 4;
        public static int NODE_TYPE_READ = 5;
        public static int NODE_TYPE_MAIN_NORMAL = 6;
        public static int NODE_TYPE_SUB = 9;

        /**
         * 备用金状态
         */
        // 未还款
        public static string MONEY_WAIT_PAY = "0";
        // 已还款
        public static string MONEY_REPAY = "1";
        // 已逾期
        public static string MONEY_DELAYED = "2";

        /**
         * 资产类别
         */
        // 固定资产
        public static string ASSET_CATEGORY_ASSET = "Asset";
        // 日常用品
        public static string ASSET_CATEGORY_CONSUME = "Consume";

        /**
         * 车辆公里填写状态
         */
        // 未填写
        public static string CAR_MILES_UNFINISH = "UNFINISH";
        // 已填写
        public static string CAR_MILES_FINISH = "FINISH";
        // 已取消
        public static string CAR_MILES_CANCEL = "CANCEL";

        /**
         * 发票对应workflowID
         */
        public static string INVOICE_WORKFLOW_ID = "Invoice";
        public static string INVOICE_DEPT_SH_CODE = "20";
        public static string INVOICE_DEPT_SH_NAME = "上海综合管理部";
        public static string INVOICE_DEPT_WX_CODE = "03";
        public static string INVOICE_DEPT_WX_NAME = "无锡综合管理部";
        public static string INVOICE_VOUCHER_TYPE_EXPRESS = "660101";
        /**
         * 凭证类型
         */
        // 普通凭证
        public static int VOUCHER_TYPE_NORMAL = 1;
        // 发票凭证
        public static int VOUCHER_TYPE_INVOICE = 2;

        /**
         * 出勤申请时间选择类型
         */
        public static string DATE_SELECT_TYPE_HOUR = "小时";

        /**
         * 无锡行政管理人员
         */
        public static string WuXi_XingZheng_User = "100009";

        public enum CALENDAR_TYPE
        {
            Holiday = 0, // 节日
            Adjust = 1, // 调休
            Rest = 2, // 休息
            Work = 3, // 上班
            Late = 4, // 迟到
            LeaveEarly = 5, // 早退
            Absent = 6, // 旷工
            Leave = 7, // 请假
            BizTrp = 8 // 出差
        }

        /*
         * 每日正常工作小时数
         */
        public static double Normal_Work_Hours = 8;
    }
}