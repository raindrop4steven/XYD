using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Entity
{
    #region 工资
    public class XYD_Salary
    {
        public decimal Salary { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
    #endregion

    #region 流程数量
    public class XYD_DB_Message_Count
    {
        public string WorkflowId { get; set; }
        public int MessageCount { get; set; }
        public string MessageTitle { get; set; }
        public string FolderName { get; set; }
    }
    #endregion
}