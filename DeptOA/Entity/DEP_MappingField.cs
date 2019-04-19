using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Entity
{
    #region 节点
    public class DEP_Node
    {
        // 表明
        public string table { get; set; }
        // 节点列表
        public List<DEP_NodeAction> nodes { get; set; }
    }

    public class DEP_NodeAction
    {
        public string key { get; set; }
        public DEP_NodeValue value { get; set; }
    }

    public class DEP_NodeValue
    {
        public List<DEP_MappingField> mappings { get; set; }
        public List<DEP_Action> actions { get; set; }
    }
    #endregion

    #region 映射
    public class DEP_MappingField
    {
        // 键
        public string key { get; set; }
        // 值
        public DEP_CellField value { get; set; }
    }

    public class DEP_CellField
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
        public DEP_DetailAction value { get; set; }
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



}