using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security;
using XYD.Common;
using XYD.Entity;
using XYD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XYD.Services
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
            var templateId = message.FromTemplate;
            var version = WorkflowUtil.GetDefaultConfigVersion(templateId);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var mapping in mappings.value)
            {
                //公文标题
                var FieldValue = worksheet.GetWorkcell(mapping.value.row, mapping.value.col);
                var Value = FieldValue == null ? "" : FieldValue.WorkcellValue;
                if (dict.ContainsKey(mapping.key))
                {
                    dict[mapping.key] = string.Format("{0} {1}", dict[mapping.key], Value);
                }
                else
                {
                    dict.Add(mapping.key, Value);
                }
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
                dict.Add("Version", version);
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

        #region 获得公文页面详情数据
        public object GetDetailInfo(string MessageID, string nodeKey, List<DEP_Detail> details)
        {
            /*
             * 变量定义
             */
            // 字典存储
            List<object> attachments = new List<object>();
            List<object> resultAttachments = new List<object>();
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
                dict.Add(detail.key, value);
            }

            dict.Add("mid", MessageID);
            dict.Add("nid", nodeKey);
            dict.Add("sid", worksheet.WorksheetID);

            return dict;
        }
        #endregion

        #region 获取流程处理记录
        public List<object> GetWorkflowHistory(string mid)
        {
            var results = new List<object>();

            var message = mgr.GetMessage(mid);
            Doc doc = mgr.GetDocByWorksheetID(mgr.GetDocHelperIdByMessageId(mid));
            Worksheet worksheet = doc.Worksheet;

            using (var db = new DefaultConnection())
            {
                var auditRecords = db.Audit_Record.Where(n => n.MessageID == mid).OrderByDescending(n => n.CreateTime).ToList();
                foreach (var record in auditRecords)
                {
                    var auditHistory = new
                    {
                        HandledBy = record.EmplID,
                        EmplName = orgMgr.GetEmployee(record.EmplID).EmplName,
                        Avatar = string.Format("/Apps/People/Shared/do_ShowPhoto.aspx?tag=logo&emplid={0}", record.EmplID),
                        HandleTime = record.CreateTime,
                        Operation = record.Operation,
                        Opinion = record.Opinion
                    };
                    results.Add(auditHistory);
                }
                return results;
            }
        }
        #endregion

        #region 获取待处理人信息
        public List<object> GetMessageHandle(string mid)
        {
            var results = new List<object>();
            List<MessageHandle> messageHandles = mgr.FindMessageHandle("MessageID=@MessageID", new Dictionary<string, object>()
              {
                {
                  "@MessageID",
                  (object) mid
                }
              }, "CreateTime asc, HandleStatus desc");
            foreach(var handle in messageHandles)
            {
                var handleInfo = new
                {
                    UserID = handle.UserID,
                    EmplName = orgMgr.GetEmployee(handle.UserID).EmplName,
                    Avatar = string.Format("/Apps/People/Shared/do_ShowPhoto.aspx?tag=logo&emplid={0}", handle.UserID),
                    CreateTime = handle.CreateTime
                };
                results.Add(handleInfo);
            }
            return results;
        }
        #endregion
    }
}