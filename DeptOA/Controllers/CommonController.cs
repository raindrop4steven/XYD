using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DeptOA.Common;
using DeptOA.Models;
using Appkiz.Apps.Workflow;
using Appkiz.Apps.Workflow.Library;
using Appkiz.Library.Security.Authentication;
using Appkiz.Library.Common;
using System.Xml;

namespace DeptOA.Controllers
{
    public class CommonController : Controller
    {
        private WorkflowMgr wmgr = new WorkflowMgr();
        private SheetMgr smgr = new SheetMgr();

        #region 上传文件
        [HttpPost]
        public ActionResult Upload(FormCollection collection)
        {
            try
            {
                /*
                 * 参数获取
                 */
                // 原附件ID
                var AttID = collection["AttID"];
                // 行,列
                var row = int.Parse(collection["row"]);
                var col = int.Parse(collection["col"]);
                // 当前用户
                var employee = (User.Identity as AppkizIdentity).Employee;

                /*
                 * 参数校验
                 */
                // 原附件ID
                if (string.IsNullOrEmpty(AttID))
                {
                    return ResponseUtil.Error("附件ID不存在");
                }

                /*
                 * 获取原附件信息
                 */
                var OldAttachment = this.wmgr.GetAttachment(AttID);
                if (OldAttachment == null)
                {
                    return ResponseUtil.Error("附件不存在");
                }
                else
                {
                    // 个人上传文件夹检查
                    var folder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"];
                    var folderPath = folder;
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // 提取上传的文件
                    HttpPostedFileBase file = Request.Files[0];
                    var FileName = Guid.NewGuid().ToString();
                    // 最终存储路径
                    var path = Path.Combine(folderPath, FileName);
                    // 保存文件
                    file.SaveAs(path);

                    Attachment NewAttachment = new Attachment();
                    NewAttachment.AttID = FileName;
                    NewAttachment.AttFileName = OldAttachment.AttFileName;
                    NewAttachment.AttExtension = OldAttachment.AttExtension;
                    NewAttachment.AttFileSize = (int)new FileInfo(path).Length;
                    NewAttachment.UploadTime = DateTime.Now;
                    NewAttachment.WkfObjectType = "worksheet";
                    NewAttachment.WkfObjectID = OldAttachment.WkfObjectID;
                    this.wmgr.AddAttachment(NewAttachment);

                    lock (Worksheet.SaveLocker)
                    {
                        Worksheet worksheet = this.smgr.GetWorksheet(NewAttachment.WkfObjectID);
                        Workcell workcell1 = worksheet.GetWorkcell(row, col);
                        if (workcell1.WorkcellInternalValue.IndexOf(FileName) < 0)
                        {
                            if (workcell1.WorkcellInternalValue == "")
                            {
                                workcell1.WorkcellInternalValue = FileName;
                            }
                            else
                            {
                                Workcell workcell2 = workcell1;
                                workcell2.WorkcellInternalValue = workcell2.WorkcellInternalValue + ";" + FileName;
                            }
                            workcell1.WorkcellValue = "<ul>";
                            string workcellInternalValue = workcell1.WorkcellInternalValue;
                            char[] chArray = new char[1] { ';' };
                            var filteredCellInternalValueList = new List<string>();
                            foreach (string att_id in workcellInternalValue.Split(chArray))
                            {
                                // 如果是以前的附件，直接过滤掉，达到删除的效果
                                if (att_id == OldAttachment.AttID)
                                {
                                    continue;
                                }
                                else
                                {
                                    filteredCellInternalValueList.Add(att_id);
                                    Attachment attachment = this.wmgr.GetAttachment(AttID);
                                    if (attachment != null)
                                    {
                                        string fileIcon = attachment.AttFileName.ToFileIcon(System.Web.HttpContext.Current);
                                        Workcell workcell2 = workcell1;
                                        workcell2.WorkcellValue = workcell2.WorkcellValue + "<li>" + fileIcon + attachment.AttFileName + "</li> ";
                                    }
                                }
                            }
                            workcell1.WorkcellValue += "</ul>";
                            workcell1.WorkcellInternalValue = string.Join(";", filteredCellInternalValueList);
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
                        this.smgr.UpdateWorksheet(worksheet);

                        // 删除旧的附件
                        this.wmgr.DelAttachment(OldAttachment.AttID);
                    }
                    return this.Json((object)new
                    {
                        Succeed = true,
                        Data = NewAttachment
                    });
                }
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 下载文件
        public ActionResult Download(int id)
        {
            using (var db = new DefaultConnection())
            {
                var attach = db.Att.Where(a => a.ID == id).FirstOrDefault();
                if (attach == null)
                {
                    return ResponseUtil.Error("附件不存在");
                }
                else
                {
                    var filename = attach.Path;
                    var fullUploadPath = System.Configuration.ConfigurationManager.AppSettings["UploadPath"];
                    string filepath = Path.Combine(fullUploadPath, filename);
                    byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                    string contentType = MimeMapping.GetMimeMapping(filepath);

                    return File(filedata, contentType, attach.Name);
                }
            }
        }
        #endregion

        public ActionResult CellAttachment(string sid, int row, int col)
        {
            List<Attachment> attachmentList = new List<Attachment>();
            Worksheet worksheet = this.smgr.GetWorksheet(sid);
            if (worksheet != null)
            {
                Workcell workcell = worksheet.GetWorkcell(row, col);
                if (workcell.WorkcellDataSource == Enum_WorkcellDataSource.Attachment)
                {
                    string workcellInternalValue = workcell.WorkcellInternalValue;
                    char[] chArray = new char[1] { ';' };
                    foreach (string AttID in workcellInternalValue.Split(chArray))
                    {
                        if (!string.IsNullOrWhiteSpace(AttID))
                        {
                            Attachment attachment = this.wmgr.GetAttachment(AttID);
                            if (attachment != null)
                                attachmentList.Add(attachment);
                        }
                    }
                }
            }

            return ResponseUtil.OK(new
            {
                attachments = attachmentList
            });
        }
    }
}