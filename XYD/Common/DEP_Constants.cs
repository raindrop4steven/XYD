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

        /*
         * 极光推送类别
         */
        // 工作流
        public static string JPush_Workflow_Type = "Workflow";

        public static string Audit_Operate_Type_Agree = "同意";
        public static string Audit_Operate_Type_Disagree = "驳回";

        /*
         * 资产状态:Available-可申领；Used-已申领；Scraped-已报废；
         */
        public static string Asset_Status_Available = "Available";
        public static string Asset_Status_Used = "Used";
        public static string Asset_Status_Scraped = "Scraped";
        public static string Asset_Status_All = "-1";
        /*
         * 资产操作类型: 添加、申请、归还、报废
         */
        public static string Asset_Operation_Add = "Add";
        public static string Asset_Operation_Apply = "Apply";
        public static string Asset_Operation_Return = "Return";
        public static string Asset_Operation_Scrap = "Scrap";

        /*
         * 选项所有值
         */
        public static int Asset_Category_All = -1; 
    }
}