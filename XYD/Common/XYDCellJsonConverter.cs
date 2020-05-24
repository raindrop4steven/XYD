using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using XYD.Entity;

namespace XYD.Common
{
    public class XYDCellJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(XYD_Base_Cell).IsAssignableFrom(objectType);
        }

        /*
         * 类型判定：
         * Type: 0-输入框；1-下拉框；2-日期选择；3-列表类型
         */
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            XYD_Base_Cell cell;
            JObject jo = JObject.Load(reader);
            int? type = (int?)jo["Type"];
            if (type == 0)
            {
                cell = new XYD_Single_Cell();
            }
            else if (type == 3)
            {
                cell = new XYD_Array_Cell();
            }
            else
            {
                throw new Exception("无效的Type类型");
            }
            serializer.Populate(jo.CreateReader(), cell);
            return cell;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}