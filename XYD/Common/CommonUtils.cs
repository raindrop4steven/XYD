using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace XYD.Common
{
    public class CommonUtils
    {
        // 变量定义
        private static Random random = new Random();
        
        #region 复制对象属性
        public static void CopyProperties<T>(object src, object dest)
        {
            Type t = src.GetType();
            PropertyInfo[] properties = t.GetProperties();

            foreach (PropertyInfo pi in properties)
            {
                if (pi.CanWrite)
                {
                    pi.SetValue(dest, pi.GetValue(src, null), null);
                }
            }
        }
        #endregion
        
        #region 获得文件最终存储名
        public static string GetFinalFileName(string fileName)
        {
            // 拓展名 txt
            var extension = Path.GetExtension(fileName);

            // 文件最终存储路径，日期+随机数
            var finalFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + RandomString(4);
            if (!string.IsNullOrEmpty(extension))
            {
                finalFileName += extension;
            }

            return finalFileName;
        }
        #endregion

        #region 使用Linq获得随机数
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion
        
        #region 使用Linq获得随机验证码
        public static string RandomCode(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion
    }
}