﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Entity
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


}