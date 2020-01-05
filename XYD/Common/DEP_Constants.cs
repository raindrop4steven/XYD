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
    }
}