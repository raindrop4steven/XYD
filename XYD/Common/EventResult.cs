using System.Collections.Generic;
using XYD.Entity;

namespace XYD.Common
{
    public class EventResult
    {
        /// <summary>
        /// 返回数据刷新
        /// </summary>
        /// <param name="refresh"></param>
        /// <param name="Fields"></param>
        /// <returns></returns>
        public static object OK(List<XYD_Base_Cell> Fields)
        {
            return new
            {
                refresh = true,
                fields = Fields
            };
        }

        /// <summary>
        /// 返回消息不刷新
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static object OK(string message)
        {
            return new
            {
                refresh = false,
                message = message
            };
        }
    }
}