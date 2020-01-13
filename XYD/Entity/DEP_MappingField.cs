using Appkiz.Apps.Workflow.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Entity
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

    #region XYD工作流模版
    public class XYD_Templates
    {
        public List<XYD_Template_Entity> workflows { get; set; }
    }

    public class XYD_Template_Entity
    {
        // Id
        public string Id { get; set; }
        // 名称
        public string Name { get; set; }
        // 图片
        public string Image { get; set; }
        // 排序
        public int Order { get; set; }
    }

    public class XYD_Operation
    {
        // 计算来源
        public List<XYD_CellPos> Origin { get; set; }
        // 计算目的
        public XYD_CellPos Dest { get; set; }
        // 计算方式
        public string Operation { get; set; }
        // 计算类型
        public string Type { get; set; }
    }
    #endregion

    #region XYD流程发起
    // 意见列表
    public class XYD_Cell_Options
    {
        // 值
        public string Value { get; set; }
        // 内部值
        public string InterValue { get; set; }
    }
    // 单元格值
    public class XYD_Cell_Value
    {
        // 标题
        public string Title { get; set; }
        // 显示的值
        public string Value { get; set; }
        // 内部值
        public string InterValue { get; set; }
        // 类型
        public int Type;
        // 是否可以编辑
        public bool CanEdit { get; set; }
        // 行
        public int Row { get; set; }
        // 列
        public int Col { get; set; }
        // 意见列表
        public List<XYD_Cell_Options> Options { get; set; }
        // 是否需要刷新
        public bool NeedRefresh { get; set; }
        // 是否必填
        public bool Required { get; set; }
    }
    // 基础单元格类型
    public class XYD_Base_Cell
    {
        // 类型
        public int Type { get; set; }
        // 标题
        public List<string> Header { get; set; }
    }
    // 单一单元格类型
    public class XYD_Single_Cell : XYD_Base_Cell
    {
        // 值
        public XYD_Cell_Value Value { get; set; }
    }
    // 数组单元格类型
    public class XYD_Array_Cell : XYD_Base_Cell
    {
        // 值
        public List<List<XYD_Cell_Value>> Array { get; set; }
    }
    public class XYD_Fields
    {
        public List<XYD_Base_Cell> Fields { get; set; }
        public List<XYD_Operation> Operations { get; set; }
    }
    #endregion

    #region XYD流程审批
    public class XYD_Audit_Cell
    {
        public int Row { get; set; }
        public int Col { get; set; } 
    }

    public class XYD_Audit_Node
    {
        public string NodeID { get; set; }
        public XYD_Audit_Cell Operate { get; set; }
        public XYD_Audit_Cell Opinion { get; set; }
    }

    public class XYD_Audit
    {
        public List<XYD_Audit_Node> Nodes { get; set; }
    }
    #endregion

    #region XYD事务编号
    public class XYD_CellPos
    {
        // 行
        public int Row { get; set; }
        // 列
        public int Col { get; set; }
    }

    public class XYD_Serial
    {
        // 映射来源：申请表单
        public string FromId { get; set; }
        // 映射目标：报销表单
        public string ToId { get; set; }
        // 节点ID
        public string NodeId { get; set; }
        // 映射
        public List<SubflowMapping> MappingOut { get; set; }
        // 编号位置
        public XYD_CellPos SnPos { get; set; }
    }

    public class XYD_Serials
    {
        public List<XYD_Serial> Serials { get; set; }
    }
    #endregion

    #region XYD_已审批记录
    public class XYD_DealResult
    {
        public string DocumentTilte { get; set; }
        public string ClosedOrHairTime { get; set; }
        public string MessageId { get; set; }
        public string WorkflowId { get; set; }
        public string MessageTitle { get; set; }
        public string CreateTime { get; set; }
        public string ReceiveTime { get; set; }
        public string Operation { get; set; }
        public string MessageIssuedBy { get; set; }
        public string EmplName { get; set; }
    }
    #endregion

    #region XYD_待处理记录
    public class XYD_PendingResult
    {
        public string DocumentTilte { get; set; }
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