using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace DeptOA.Common
{
    public class DiskUtil
    {
        /// <summary>
        /// 获得文件最终存储名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFinalFileName(string fileName)
        {
            // 拓展名 txt
            var extension = Path.GetExtension(fileName);

            // 文件最终存储路径，日期+随机数
            var finalFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + DiskUtil.RandomString(4);
            if (!string.IsNullOrEmpty(extension))
            {
                finalFileName += extension;
            }

            return finalFileName;
        }

        /// <summary>
        /// 使用Linq获得随机数
        /// </summary>
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 使用Linq获得随机验证码
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <returns></returns>
        public static string RandomCode(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 预约单号生成规则:yyyyMMddHHmmss1234
        /// </summary>
        /// <returns></returns>
        public static string GetFormNumber()
        {
            var getFormNumber = DateTime.Now.ToString("yyyyMMddHHmmss") + DiskUtil.RandomCode(4);

            return getFormNumber;
        }
    }
}