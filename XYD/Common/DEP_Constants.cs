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
         * 默认分页
         */
        public static int Page = 0;
        public static int Size = 10;
        
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
        public static int NODE_TYPE_END = 4;
        public static int NODE_TYPE_READ = 5;
        public static int NODE_TYPE_MAIN_NORMAL = 6;
        public static int NODE_TYPE_SUB = 9;
    }
}