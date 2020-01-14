using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class BannerController : Controller
    {
        #region Banner图片列表
        public ActionResult List()
        {
            try
            {
                var db = new DefaultConnection();
                var results = new List<object>();
                var banners = db.Banner.OrderBy(n => n.order).ToList();
                foreach (var banner in banners)
                {
                    var att = db.Attachment.Where(n => n.ID == banner.AttID).FirstOrDefault();
                    results.Add(new {
                        id = banner.ID,
                        attId = att.ID,
                        name = att.Name,
                        path = Url.Action("Download", "Common", new { id = att.ID })
                    });
                }
                return ResponseUtil.OK(results);
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion

        #region 上传Banner
        [HttpPost]
        public ActionResult Add()
        {
            try
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

                // 原文件名 Test.txt
                var fileName = Path.GetFileName(file.FileName);
                // 最终文件名 yyyyMMddHHmmss+4random.txt
                var finalFileName = DiskUtil.GetFinalFileName(fileName);
                // 最终存储路径
                var path = Path.Combine(folderPath, finalFileName);

                // 保存文件
                file.SaveAs(path);

                // 判断上传的是反馈文件还是客户签字单
                var att = new XYD_Att();
                att.Name = fileName;
                att.Path = Path.Combine(finalFileName).Replace("\\", "/");

                using (var db = new DefaultConnection())
                {
                    db.Attachment.Add(att);
                    db.SaveChanges();
                    var banner = new XYD_Banner();
                    banner.AttID = att.ID;
                    banner.CreateTime = DateTime.Now;
                    banner.UpdateTime = DateTime.Now;
                    db.Banner.Add(banner);
                    db.SaveChanges();
                }

                return new JsonNetResult(new
                {
                    status = 200,
                    data = new
                    {
                        id = att.ID,
                        name = att.Name,
                        path = Url.Action("Download", "Common", new { id = att.ID })
                    }
                });
            }
            catch(Exception e)
            {
                return ResponseUtil.Error(e.Message);
            }
        }
        #endregion
    }
}