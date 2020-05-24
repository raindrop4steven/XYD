using System.Collections.Generic;

namespace XYD.Entity
{
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
        // 类型
        public string Type { get; set; }
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
        // 附件
        public List<object> Atts { get; set; }
        // 类型
        public int Type;
        // 是否可以编辑
        public dynamic CanEdit { get; set; }
        // 行
        public int Row { get; set; }
        // 列
        public int Col { get; set; }
        // 意见列表
        public dynamic Options { get; set; }
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
    #endregion

    #region XYD_事件
    public class XYD_Event_Argument
    {
        public string MessageId { get; set; }
        public string NodeId { get; set; }
        public XYD_Cell_Value CurrentCellValue { get; set; }
        public List<XYD_Base_Cell> Fields { get; set; }
    }

    public class XYD_Event
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public string Event { get; set; }
    }

    public class XYD_EventCells
    {
        public List<XYD_Event> cells;
    }
    #endregion

    #region XYD_自定义方法结构
    public class XYD_Custom_Func
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public List<string> ArgumentsArray { get; set; }
    }
    #endregion

    #region XYD_配置版本
    public class XYD_Version
    {
        public string WorkflowId { get; set; }
        public string Version { get; set; }
    }
    public class XYD_Config_Version
    {
        public List<XYD_Version> versions { get; set; }
    }
    #endregion
}