namespace XYD.Entity
{

    #region XYD_流程数量
    public class XYD_DB_Message_Count
    {
        public string WorkflowId { get; set; }
        public int MessageCount { get; set; }
        public string MessageTitle { get; set; }
        public string FolderName { get; set; }
    }
    #endregion
    
    #region XYD_已审批记录
    public class XYD_DealResult
    {
        public string DocumentTitle { get; set; }
        public string ClosedOrHairTime { get; set; }
        public string MessageId { get; set; }
        public string WorkflowId { get; set; }
        public string MessageTitle { get; set; }
        public string CreateTime { get; set; }
        public string ReceiveTime { get; set; }
        public string Operation { get; set; }
        public string MessageIssuedBy { get; set; }
        public string EmplName { get; set; }
        public string MessageStatusName { get; set; }
    }
    #endregion

    #region XYD_待处理记录
    public class XYD_PendingResult
    {
        public string DocumentTitle { get; set; }
        public string ClosedOrHairTime { get; set; }
        public string MessageId { get; set; }
        public string WorkflowId { get; set; }
        public string InitiateEmplId { get; set; }
        public string InitiateEmplName { get; set; }
        public string MessageTitle { get; set; }
        public string MyTask { get; set; }
        public string ReceiveTime { get; set; }
        public string MessageIssuedBy { get; set; }
    }
    #endregion
}