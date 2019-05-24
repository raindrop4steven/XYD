using Appkiz.Apps.Workflow.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Entity
{
    #region 部门-公文映射表关系
    public class RelationItem
    {
        /// <summary>
        /// 
        /// </summary>
        public string dept { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string tableName { get; set; }
    }

    public class DeptFlowRelation
    {
        /// <summary>
        /// 
        /// </summary>
        public List<RelationItem> relations { get; set; }
    }
    #endregion

    #region 部门-公文关系
    public class Workflow
    {
        public string dept { get; set; }
        public List<string> flows { get; set; }
    }

    public class Workflows
    {
        public List<Workflow> workflows { get; set; }
    }
    #endregion

    #region 部门-子流程关系
    public class Subflow
    {
        public string dept { get; set; }
        public string subflowId { get; set; }
    }

    public class DeptSubflowRelatoin
    {
        public List<Subflow> subflows { get; set; }
    }
    #endregion

    #region 党办子节点关系

    public class Subflows
    {
        public List<SubWorkflowRelation> subflows { get; set; }
    }

    public class SubWorkflowRelation
    {
        public string TemplateId { get; set; }
        public SubflowConfig Config { get; set; }
    }
    #endregion

    #region 节点与通知范围关系
    public class Receiver
    {
        public string nid { get; set; }
        public int scope { get; set; }
    }

    public class Notification
    {
        public string eventType { get; set; }
        public List<Receiver> receivers { get; set; }
    }

    public class Notify
    {
        public string originNode { get; set; }
        public List<Notification> notifications { get; set; }
    }

    public class DEP_Notifies
    {
        public List<Notify> notify { get; set; }
    }
    #endregion

    #region 公文报警配置
    public class AlarmValue
    {
        public int row { get; set; }
        public int col { get; set; }
        public int before { get; set; }
    }

    public class AlarmConfig
    {
        public string key { get; set; }
        public AlarmValue value { get; set; }
    }

    public class DEP_Alarms
    {
        public int days { get; set; }
        public List<AlarmConfig> configs { get; set; }
    }

    public class DEP_MessageAlarmConfig
    {
        public DEP_Alarms alarms { get; set; }
    }
    #endregion

    #region 公文意见修改配置
    public class OpinionValue
    {
        public int row { get; set; }
        public int col { get; set; }
    }

    public class Opinion
    {
        public string node { get; set; }
        public string key { get; set; }
        public string order { get; set; }
        public OpinionValue value { get; set; }
        public int type { get; set; }
    }

    public class DEP_Opinions
    {
        public List<Opinion> opinions { get; set; }
    }
    #endregion

    #region 个人意见插入配置
    public class PrivateOpinionValue
    {
        public string valueCellId { get; set; }
        public string optionCellId { get; set; }
    }

    public class PrivateOpinion
    {
        public string key { get; set; }
        public PrivateOpinionValue value { get; set; }
    }

    public class DEP_PrivateOpinions
    {
        public List<PrivateOpinion> opinions { get; set; }
    }
    #endregion

    #region 节点
    public class DEP_Node
    {
        // 表名
        public string table { get; set; }
        // 属性
        public DEP_NodeAction values { get; set; }
    }

    public class DEP_NodeAction
    {
        // 映射
        public List<DEP_Mapping> mappings { get; set; }
        // 动作
        public List<DEP_Action> actions { get; set; }
        // 详情
        public List<DEP_Detail> details { get; set; }
        // 属性
        public List<DEP_Control> controls { get; set; }
        // 控件类型
        public List<DEP_InputType> inputTypes { get; set; }
    }
    #endregion

    #region 映射
    public class DEP_Mapping
    {
        // 键
        public string key { get; set; }
        // 值
        public List<DEP_CellField> value { get; set; }
    }

    public class DEP_CellField
    {
        public string key { get; set; }
        public DEP_CellPos value { get; set; }
    }

    public class DEP_CellPos
    {
        // 行
        public int row { get; set; }
        // 列
        public int col { get; set; }
    }
    #endregion

    #region 动作
    public class DEP_Action
    {
        // 键
        public string key { get; set; }
        // 值
        public List<DEP_ActionField> value { get; set; }
    }

    public class DEP_ActionField
    {
        public string key { get; set; }
        public DEP_DetailAction value;
    }

    public class DEP_DetailAction
    {
        // 显示
        public bool show { get; set; }
        // 行
        public int row { get; set; }
        // 列
        public int col { get; set; }
        // 目标节点
        public string targetNodeKey { get; set; }
    }
    #endregion

    #region 详情
    public class DEP_Detail
    {
        public string key { get; set; }
        public DEP_DetailPos value { get; set; }
        public int type { get; set; }
    }

    public class DEP_DetailPos
    {
        public int row { get; set; }
        public int col { get; set; }
    }
    #endregion

    #region 控制
    public class DEP_Control
    {
        public string key { get; set; }
        public object value { get; set; }
    }
    #endregion

    #region 输入控件类型
    public class DEP_InputType
    {
        // 节点
        public string key { get; set; }
        // 控件配置
        public InputType value { get; set; }
    }

    public class InputType
    {
        // 控件类型
        public string type { get; set; }
        // 其他
        public object extra { get; set; }
    }
    #endregion

}