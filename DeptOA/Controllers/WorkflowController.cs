﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using DeptOA.Entity;
using DeptOA.Common;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using System.Text;

namespace DeptOA.Controllers
{
    public class WorkflowController : Controller
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 展示部门指派页面
        public ActionResult ShowDepts()
        {
            return PartialView("GetDeptPeople");
        }
        #endregion

        #region 映射数据
        [HttpPost]
        public ActionResult MappingData(FormCollection collection)
        {
            /*
             * 参数获取
             */
            // 消息ID
            var MessageID = collection["mid"];
            // 节点ID
            var NodeID = collection["nid"];


            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageID);

            /*
             * 配置读取
             */
            using (StreamReader sr = new StreamReader(Server.MapPath("~/mapping.json")))
            {
                var config = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());
                // 根据nid获取对应配置
                var table = config.table;
                DEP_NodeValue nodeConfig = null;
                foreach(var nodeAction in config.nodes)
                {
                    if(nodeAction.key == NodeID)
                    {
                        nodeConfig = nodeAction.value;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                // 判断是否存在对应配置
                if (nodeConfig == null)
                {
                    return ResponseUtil.Error(string.Format("流程{0}节点{1}没有对应配置", MessageID, NodeID));
                }
                else
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    foreach (var mapping in nodeConfig.mappings)
                    {
                        //公文标题
                        var FieldValue = worksheet.GetWorkcell(mapping.value.row, mapping.value.col);
                        dict.Add(mapping.key, FieldValue == null ? "" : FieldValue.WorkcellValue);
                    }

                    // 检查是否有对应的记录存在
                    var checkSql = string.Format(@"SELECT Id FROM {0} WHERE MessageId = '{1}'", table, MessageID);
                    var checkResultList = DbUtil.ExecuteSqlCommand(checkSql, DbUtil.SearchInformation).ToList();
                    StringBuilder sql = new StringBuilder();
                    if (checkResultList.Count == 0) // 没有数据，新增
                    {
                        // 添加更新者、更新时间、创建者、创建时间
                        dict.Add("MessageId", MessageID);
                        dict.Add("WorkFlowId", message.FromTemplate);
                        // 主键
                        var guid = Guid.NewGuid().ToString();
                        var tableFields = string.Format("Id, {0}", string.Join(",", dict.Keys));
                        var tableValues = string.Format("'{0}', {1}", guid, string.Join(",", dict.Values.Select(i => string.Format("'{0}'", i))));

                        sql.Append(string.Format(@"INSERT INTO {0}({1}) VALUES ({2})", table, tableFields, tableValues));
                    }
                    else
                    {
                        // 已有数据，直接更新
                        // 遍历查询结果，构造SQL语句

                        sql.Append(string.Format("UPDATE {0} SET ", table));

                        foreach (var item in dict.Select((Entry, Index) => new { Entry, Index }))
                        {
                            var delemiter = "";
                            if (item.Index > 0)
                            {
                                delemiter = ",";
                            }
                            sql.Append(string.Format(@"{0} {1} = '{2}'", delemiter, item.Entry.Key, item.Entry.Value));
                        }
                        sql.Append(string.Format(@" WHERE MessageId = '{0}'", MessageID));
                    }

                    bool runResult = DbUtil.ExecuteSqlCommand(sql.ToString());

                    return ResponseUtil.OK(runResult);
                }
            }
        }
        #endregion

        public ActionResult Config()
        {
            using (StreamReader sr = new StreamReader(Server.MapPath("~/mapping.json")))
            {
                var mappingModel = JsonConvert.DeserializeObject<DEP_Node>(sr.ReadToEnd());

                return ResponseUtil.OK(mappingModel);
            }
        }
    }
}