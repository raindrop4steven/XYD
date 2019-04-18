using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeptOA.Entity
{
    /// <summary>
    /// 映射的字段结构
    /// </summary>
    public class DEP_MappingField
    {
        // 键值
        public string key { get; set; }
        // 行
        public int row { get; set; }
        // 列
        public int col { get; set; }
    }

    public class DEP_MappingFields
    {
        // 数据库表
        public string table { get; set; }
        // 对应的字段
        public List<DEP_MappingField> fields { get; set; }
    }

    public class DEP_MappingModel
    {
        public DEP_MappingFields mapping { get; set; }
    }
}