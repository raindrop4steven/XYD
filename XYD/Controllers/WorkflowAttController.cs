using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Common;
using Appkiz.Library.Security.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using XYD.Common;
using XYD.Entity;

namespace XYD.Controllers
{
    public class WorkflowAttController : Controller
    {
        private WorkflowMgr wmgr = new WorkflowMgr();
        private SheetMgr smgr = new SheetMgr();

        #region 上传附件文件
        [Authorize]
        public ActionResult Upload(FormCollection collection)
        {
            try
            {
                /*
                 * 参数获取
                 */
                // 行,列
                var mid = collection["mid"];
                var nid = collection["nid"];
                var sid = collection["sid"];
                var row = int.Parse(collection["row"]);
                var col = int.Parse(collection["col"]);
                var inputFields =collection["fields"];
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                WorkflowUtil.ConfirmStartWorkflow(mid, inputFields);
                /*
                 * 获取原附件信息
                 */
                var folderPath = System.Configuration.ConfigurationManager.AppSettings["UploadPath"];
                // 提取上传的文件
                HttpPostedFileBase file = Request.Files[0];
                var FileName = file.FileName;
                var Extension = FileName.Contains(".") ? FileName.Substring(FileName.LastIndexOf(".") + 1) : "";
                var AttID = Guid.NewGuid().ToString();
                // 最终存储路径
                var path = Path.Combine(folderPath, AttID);
                file.SaveAs(path);

                Attachment theAttachment = new Attachment();
                theAttachment.AttID = AttID;
                theAttachment.AttFileName = FileName;
                theAttachment.AttExtension = Extension;
                theAttachment.AttFileSize = (int)new FileInfo(path).Length;
                theAttachment.UploadTime = DateTime.Now;
                theAttachment.WkfObjectType = "worksheet";
                theAttachment.WkfObjectID = sid;
                wmgr.AddAttachment(theAttachment);
                lock (Worksheet.SaveLocker)
                {
                    Worksheet worksheet = this.smgr.GetWorksheet(sid);
                    Workcell workcell1 = worksheet.GetWorkcell(row, col);
                    if (workcell1.WorkcellInternalValue.IndexOf(AttID) < 0)
                    {
                        if (workcell1.WorkcellInternalValue == "")
                        {
                            workcell1.WorkcellInternalValue = AttID;
                        }
                        else
                        {
                            Workcell workcell2 = workcell1;
                            workcell2.WorkcellInternalValue = workcell2.WorkcellInternalValue + ";" + AttID;
                        }
                        workcell1.WorkcellValue = "<ul>";
                        string workcellInternalValue = workcell1.WorkcellInternalValue;
                        char[] chArray = new char[1] { ';' };
                        foreach (string FileAttID in workcellInternalValue.Split(chArray))
                        {
                            Attachment attachment = this.wmgr.GetAttachment(FileAttID);
                            if (attachment != null)
                            {
                                string fileIcon = attachment.AttFileName.ToFileIcon(this.HttpContext.ApplicationInstance.Context);
                                Workcell workcell2 = workcell1;
                                workcell2.WorkcellValue = workcell2.WorkcellValue + "<li>" + fileIcon + attachment.AttFileName + "</li> ";
                            }
                        }
                        workcell1.WorkcellValue += "</ul>";
                    }
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(worksheet.Document);
                    foreach (XmlNode node in xmlDocument.GetElementsByTagName("Cell"))
                    {
                        if (node.AttributeValue("Row") == workcell1.WorkcellRow.ToString() && node.AttributeValue("Col") == workcell1.WorkcellCol.ToString())
                        {
                            if (node.Attributes["Value"] != null)
                            {
                                node.Attributes["Value"].Value = workcell1.WorkcellValue;
                            }
                            else
                            {
                                XmlAttribute attribute = xmlDocument.CreateAttribute("Value");
                                attribute.Value = workcell1.WorkcellValue;
                                node.Attributes.SetNamedItem((XmlNode)attribute);
                            }
                            if (node.Attributes["InternalValue"] != null)
                            {
                                node.Attributes["InternalValue"].Value = workcell1.WorkcellInternalValue;
                                break;
                            }
                            XmlAttribute attribute1 = xmlDocument.CreateAttribute("InternalValue");
                            attribute1.Value = workcell1.WorkcellInternalValue;
                            node.Attributes.SetNamedItem((XmlNode)attribute1);
                            break;
                        }
                    }
                    worksheet.Document = xmlDocument.InnerXml;
                    smgr.UpdateWorksheet(worksheet);
                }
                var fields = WorkflowUtil.GetStartFields(employee.EmplID, nid, mid);
                return ResponseUtil.OK(EventResult.OK(fields.Fields));
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 删除附件
        public ActionResult DelCellAtt(FormCollection collection)
        {
            try
            {
                /*
                 * 参数获取
                 */
                // 行,列
                var mid = collection["mid"];
                var nid = collection["nid"];
                var sid = collection["sid"];
                var attId = collection["attId"];
                var row = int.Parse(collection["row"]);
                var col = int.Parse(collection["col"]);
                var inputFields = collection["fields"];
                WorkflowUtil.ConfirmStartWorkflow(mid, inputFields);
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                Worksheet worksheet = this.smgr.GetWorksheet(sid);
                Workcell workcell = worksheet.GetWorkcell(row, col);
                string str = "<ul>";
                string NewInternalValue = "";
                string workcellInternalValue = workcell.WorkcellInternalValue;
                char[] chArray = new char[1] { ';' };
                foreach (string AttID in workcellInternalValue.Split(chArray))
                {
                    if (!(AttID == attId))
                    {
                        Attachment attachment = this.wmgr.GetAttachment(AttID);
                        if (attachment != null)
                        {
                            NewInternalValue = NewInternalValue + (NewInternalValue.Length > 0 ? ";" : "") + AttID;
                            str = str + "<li>" + attachment.AttFileName.ToFileIcon(this.HttpContext.ApplicationInstance.Context) + attachment.AttFileName + "</li>";
                        }
                    }
                }
                string NewValue = str + "</ul>";
                worksheet.SetCellValue(workcell.WorkcellRow, workcell.WorkcellCol, NewValue, NewInternalValue);
                worksheet.Save();
                this.wmgr.DelAttachment(attId);
                var fields = WorkflowUtil.GetStartFields(employee.EmplID, nid, mid);
                return ResponseUtil.OK(EventResult.OK(fields.Fields));
            }
            catch (Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}