using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XYD.Common
{
    /// <summary>
    /// 响应帮助工具类
    /// </summary>
    public class ResponseUtil
    {
        /// <summary>
        /// 20x 无数据的响应
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JsonNetResult OK(string message)
        {
            return new JsonNetResult(new
            {
                Succeed = true,
                Message = message
            });
        }

        /// <summary>
        /// 40x,50x无数据错误响应
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JsonNetResult Error(string message)
        {
            return new JsonNetResult(new
            {
                Succeed = false,
                Message = message
            });
        }

        /// <summary>
        /// 20x 无数据的响应
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static JsonNetResult OK(object data)
        {
            return new JsonNetResult(new
            {
                Succeed = true,
                Data = data
            });
        }
    }
}