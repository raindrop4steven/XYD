using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using DeptOA.Common;
using DeptOA.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DeptOA.Services
{
    public class WorkflowService
    {
        WorkflowMgr mgr = new WorkflowMgr();
        OrgMgr orgMgr = new OrgMgr();

        #region 新增或更新映射记录
        public bool AddOrUpdateRecord(string MessageID, string tableName, DEP_Mapping mappings)
        {
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageID);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var mapping in mappings.value)
            {
                //公文标题
                var FieldValue = worksheet.GetWorkcell(mapping.value.row, mapping.value.col);
                dict.Add(mapping.key, FieldValue == null ? "" : FieldValue.WorkcellValue);
            }

            // 检查是否有对应的记录存在
            var checkSql = string.Format(@"SELECT Id FROM {0} WHERE MessageId = '{1}'", tableName, MessageID);
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

                sql.Append(string.Format(@"INSERT INTO {0}({1}) VALUES ({2})", tableName, tableFields, tableValues));
            }
            else
            {
                sql.Append(string.Format("UPDATE {0} SET ", tableName));

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

            return runResult;
        }
        #endregion

        #region 获得详情
        public object GetDetailInfo(string MessageID, string nodeKey, List<DEP_Detail> details)
        {
            /*
             * 变量定义
             */
            // 字典存储
            List<object> attachments = new List<object>();
            List<object> resultAttachments = new List<object>();
            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            /*
             * 获取表单详情
             */
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageID);

            foreach(var detail in details)
            {
                var value = WorkflowUtil.GetCellValue(worksheet, detail.value.row, detail.value.col, detail.type);
                // 判断是否是附件
                if(detail.type == 10 && ((List<object>)value).Count > 0)
                {
                    attachments.Add(value);
                }
                else
                {
                    dict.Add(detail.key, value);
                }
            }

            foreach(var attach in attachments)
            {
                resultAttachments.Add(new
                {
                    documentTitle = dict["DocumentTitle"],
                    data = attach
                });
            }
            //resultDict.Add("data", dict);
            resultDict.Add("mid", MessageID);
            resultDict.Add("nid", nodeKey);
            resultDict.Add("sid", worksheet.WorksheetID);
            resultDict.Add("attachments", resultAttachments);

            return resultDict;
        }
        #endregion

        #region 获得公文页面详情数据
        public object GetPageInfo(string MessageID, string nodeKey, List<DEP_Detail> details)
        {
            /*
                 * 变量定义
                 */
            // 字典存储
            List<object> attachments = new List<object>();
            List<object> resultAttachments = new List<object>();
            Dictionary<string, object> resultDict = new Dictionary<string, object>();
            Dictionary<string, object> dict = new Dictionary<string, object>();

            /*
             * 获取表单详情
             */
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(MessageID));
            Worksheet worksheet = doc.Worksheet;
            Message message = mgr.GetMessage(MessageID);

            foreach (var detail in details)
            {
                var value = WorkflowUtil.GetCellValue(worksheet, detail.value.row, detail.value.col, detail.type);
                // 判断是否是附件
                if (detail.type == 10 && ((List<object>)value).Count > 0)
                {
                    attachments.Add(value);
                }
                else
                {
                    dict.Add(detail.key, value);
                }
            }

            foreach (var attach in attachments)
            {
                resultAttachments.Add(new
                {
                    documentTitle = dict["DocumentTitle"],
                    data = attach
                });
            }
            //resultDict.Add("data", dict);
            resultDict.Add("mid", MessageID);
            resultDict.Add("nid", nodeKey);
            resultDict.Add("sid", worksheet.WorksheetID);
            resultDict.Add("attachments", resultAttachments);

            return resultDict;
        }
        #endregion
    }
}