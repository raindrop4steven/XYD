using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XYD.Common;
using XYD.Models;

namespace XYD.Controllers
{
    public class SerialNoController : Controller
    {
        /// <summary>
        /// 获取阅件编号信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetSerialNumber(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            var year = Convert.ToString(DateTime.Now.Year);
            using (var db = new DefaultConnection())
            {
                var entity = db.SerialNo.Where(n => n.Name == name).FirstOrDefault();
                if (entity != null)
                {
                    if (!year.Contains(Convert.ToString(entity.Year)))
                    {
                        entity.Year += 1;
                        entity.Number = 1;
                        db.SaveChanges();
                    }
                    var num = string.Empty;
                    if (entity.Number < 10)
                    {
                        num = string.Format("000{0}", entity.Number);
                    }
                    else if (entity.Number < 100)
                    {
                        num = string.Format("00{0}", entity.Number);
                    }
                    else if (entity.Number < 1000)
                    {
                        num = string.Format("0{0}", entity.Number);
                    }
                    var serialNo = string.Format("{0}-{1}", DateTime.Now.ToString("yyyyMMdd"), num);
                    return ResponseUtil.OK(serialNo);
                }
                else
                {
                    return ResponseUtil.Error("没有找到对应的编号配置");
                }
            }
        }
    }
}