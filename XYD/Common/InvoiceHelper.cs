using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace XYD.Common
{
    public static class InvoiceHelper
    {
        static readonly string openPlatformUrl = "https://open.leshui365.com";
        static readonly string myAppKey = "4603a223bda7472dbc82631f00d8dbe8";
        static readonly string myAppSecret = "abc7b003-016c-47b8-860a-28e4f6915e53";
        static readonly string sacnMethod = "invoiceInfoByQRCode";
        static readonly string normalMethod = "invoiceInfoForCom";

        #region 获取发票信息
        /// <summary>
        /// 获取发票信息
        /// </summary>
        /// <param name="invoiceCode"></param>
        /// <param name="invoiceNumber"></param>
        /// <param name="billTime"></param>
        /// <param name="invoiceAmount"></param>
        /// <param name="checkCode"></param>
        /// <param name="msg"></param>
        /// <param name="invoiceResult"></param>
        /// <param name="free"></param>
        /// <returns></returns>
        public static bool GetInvoiceInfo(string invoiceCode, string invoiceNumber, string billTime, 
            string invoiceAmount, string checkCode, ref string msg, ref string invoiceResult, ref bool free)
        {
            msg = string.Empty;
            invoiceResult = string.Empty;
            free = true;

            //invoiceResult = "{'invoiceDataCode':'3200184160','invoiceNumber':'08063572','invoiceTypeName':'江苏增值税（专用发票）','invoiceTypeCode':'01','checkDate':'2019-08-15 13:47:01','checkNum':'1','billingTime':'2019-07-10','purchaserName':'无锡瑞德纺织服装设计有限公司','taxpayerNumber':'913202116744373058','taxpayerAddressOrId':'无锡市滨湖区震泽路27号 0510-82763430','taxpayerBankAccount':'中国农业银行无锡锡山支行 10650101040222878','salesName':'黑牡丹纺织有限公司','salesTaxpayerNum':'91320400323906739N','salesTaxpayerAddress':'常州市天宁区青洋北路47号 0519-68866900','salesTaxpayerBankAccount':'工行新区支行 1105021609001387840','totalAmount':'3575.20','totalTaxNum':'464.80','totalTaxSum':'4040.00','invoiceRemarks':'松','taxDiskCode':'661401656771','checkCode':'85188703713281059871','voidMark':'0','tollSign':'99','tollSignName':'','isBillMark':'N','invoiceDetailData':[{'lineNum':'1','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S329J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'2','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'15105-QHS','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'3','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S343J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'4','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S354J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'5','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'18087-16Q','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'6','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'11166S','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'7','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'22','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'389.38','model':'15802-B2Q','taxRate':'13%','tax':'50.62','isBillLine':'N'}]}";
            //return true;

            var rtv = GetToken(myAppKey, myAppSecret);
            if (rtv.Success)
            {
                var invoiceArgs = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(checkCode))
                {
                    invoiceArgs = new Dictionary<string, string>
                    {
                        {"invoiceCode", invoiceCode},
                        {"invoiceNumber", invoiceNumber},
                        {"billTime", billTime},
                        {"invoiceAmount", invoiceAmount},
                        {"token", rtv.Data}
                    };
                }
                else
                {
                    invoiceArgs = new Dictionary<string, string>
                    {
                        {"invoiceCode", invoiceCode},
                        {"invoiceNumber", invoiceNumber},
                        {"billTime", billTime},
                        {"checkCode", checkCode},
                        {"invoiceAmount", invoiceAmount},
                        {"token", rtv.Data}
                    };
                }

                rtv = GetInvoiceInfo(JsonConvert.SerializeObject(invoiceArgs), normalMethod);
                if (rtv.Success)
                {
                    var rtvJson = (JObject)JsonConvert.DeserializeObject(rtv.Data);
                    if (rtvJson["RtnCode"].ToString() == "00" && rtvJson["resultCode"].ToString() == "1000")
                    {
                        free = false;
                        invoiceResult = rtvJson["invoiceResult"].ToString();
                        return true;
                    }
                    else
                    {
                        if (rtvJson["invoicefalseCode"].ToString() == "201" || rtvJson["invoicefalseCode"].ToString() == "220")
                        {
                            free = false;
                        }
                        msg = rtvJson["resultMsg"].ToString();
                        return false;
                    }
                }
                else
                {
                    msg = rtv.Message;
                    return false;
                }
            }
            else
            {
                msg = rtv.Message;
                return false;
            }
        }
        #endregion

        #region 通过发票二维码获取发票信息
        /// <summary>
        /// 通过发票二维码获取发票信息
        /// </summary>
        /// <param name="scanStr"></param>
        /// <returns></returns>
        public static bool GetInvoiceInfoByQRCode(string scanStr, ref string msg, ref string invoiceResult, ref bool free)
        {
            msg = string.Empty;
            invoiceResult = string.Empty;
            free = true;

            //invoiceResult = "{'invoiceDataCode':'3200184160','invoiceNumber':'08063572','invoiceTypeName':'江苏增值税（专用发票）','invoiceTypeCode':'01','checkDate':'2019-08-15 13:47:01','checkNum':'1','billingTime':'2019-07-10','purchaserName':'无锡瑞德纺织服装设计有限公司','taxpayerNumber':'913202116744373058','taxpayerAddressOrId':'无锡市滨湖区震泽路27号 0510-82763430','taxpayerBankAccount':'中国农业银行无锡锡山支行 10650101040222878','salesName':'黑牡丹纺织有限公司','salesTaxpayerNum':'91320400323906739N','salesTaxpayerAddress':'常州市天宁区青洋北路47号 0519-68866900','salesTaxpayerBankAccount':'工行新区支行 1105021609001387840','totalAmount':'3575.20','totalTaxNum':'464.80','totalTaxSum':'4040.00','invoiceRemarks':'松','taxDiskCode':'661401656771','checkCode':'85188703713281059871','voidMark':'0','tollSign':'99','tollSignName':'','isBillMark':'N','invoiceDetailData':[{'lineNum':'1','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S329J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'2','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'15105-QHS','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'3','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S343J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'4','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'19-S354J','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'5','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'18087-16Q','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'6','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'30','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'530.97','model':'11166S','taxRate':'13%','tax':'69.03','isBillLine':'N'},{'lineNum':'7','goodserviceName':'*纺织产品*牛仔布','unit':'米','number':'22','price':'17.699115044247787','zeroTaxRateSign':'','zeroTaxRateSignName':'非零税率','sum':'389.38','model':'15802-B2Q','taxRate':'13%','tax':'50.62','isBillLine':'N'}]}";
            //return true;

            var rtv = GetToken(myAppKey, myAppSecret);
            if (rtv.Success)
            {
                var invoiceArgs = new Dictionary<string, string>
                {
                    {"scanStr", scanStr},
                    {"token", rtv.Data}
                };

                rtv = GetInvoiceInfo(JsonConvert.SerializeObject(invoiceArgs), sacnMethod);
                if (rtv.Success)
                {
                    var rtvJson = (JObject)JsonConvert.DeserializeObject(rtv.Data);
                    if (rtvJson["RtnCode"].ToString() == "00" && rtvJson["resultCode"].ToString() == "1000")
                    {
                        free = false;
                        invoiceResult = rtvJson["invoiceResult"].ToString();
                        return true;
                    }
                    else
                    {
                        if (rtvJson["invoicefalseCode"].ToString() == "201" || rtvJson["invoicefalseCode"].ToString() == "220")
                        {
                            free = false;
                        }
                        msg = rtvJson["resultMsg"].ToString();
                        return false;
                    }
                }
                else
                {
                    msg = rtv.Message;
                    return false;
                }
            }
            else
            {
                msg = rtv.Message;
                return false;
            }
        }
        #endregion

        #region 获取授权码token
        /// <summary>
        /// 获取授权码。
        /// </summary>
        /// <param name="appKey">客户应用key：需向开放平台申请</param>
        /// <param name="appSecret">客户应用密钥：需向开放平台申请</param>
        /// <returns></returns>
        private static ReturnValue<string> GetToken(string appKey, string appSecret)
        {
            var rtv = new ReturnValue<string> { Success = false };

            try
            {

                var url = string.Format("{0}/getToken?appKey={1}&appSecret={2}", openPlatformUrl, appKey, appSecret);

                var client = new HttpClient();
                var task = client.GetStringAsync(url);
                task.Wait();

                rtv.Data = ((JObject)JsonConvert.DeserializeObject(task.Result))["token"].ToString();
                rtv.Success = true;
            }
            catch (AggregateException ex)
            {
                rtv.Message = ex.InnerException.Message;
            }
            catch (Exception ex)
            {
                rtv.Message = ex.Message;
            }

            return rtv;
        }
        #endregion

        #region 获取发票信息
        /// <summary>
        /// 获取发票信息
        /// </summary>
        /// <param name="invoiceParams"></param>
        /// <returns></returns>
        static ReturnValue<string> GetInvoiceInfo(string invoiceParams, string method)
        {
            var rtv = new ReturnValue<string> { Success = false };

            try
            {
                var url = openPlatformUrl + "/api/" + method;
                var httpContent = new StringContent(invoiceParams, Encoding.UTF8, "application/json");

                var client = new HttpClient();
                var postTask = client.PostAsync(url, httpContent);
                postTask.Wait();

                var readTask = postTask.Result.Content.ReadAsStringAsync();
                readTask.Wait();

                rtv.Data = readTask.Result;
                rtv.Success = true;
            }
            catch (AggregateException ex)
            {
                rtv.Message = ex.InnerException.Message;
            }
            catch (Exception ex)
            {
                rtv.Message = ex.Message;
            }

            return rtv;
        }
        #endregion

    }

    #region 自定义类
    /// <summary>
    /// 用来表示方法或属性的返回值，返回的数据类型有<b>T</b>决定
    /// </summary>
    /// <typeparam name="T">数据的类型</typeparam>
    public class ReturnValue<T>
    {
        /// <summary>
        /// true表示成功，否则失败
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息代码
        /// </summary>
        public string MessageCode { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public T Data { get; set; }
    }

    /// <summary>
    /// 用来表示方法或属性的返回值
    /// </summary>
    public class ReturnValue : ReturnValue<object>
    {
    }
    #endregion
}
