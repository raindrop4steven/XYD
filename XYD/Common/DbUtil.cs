using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XYD.Entity;

namespace XYD.Common
{
    public class DbUtil
    {

        #region 基本委托定义
        /// <summary>
        /// 表数据委托方法定义
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public delegate List<object> DataHandler(SqlDataReader reader);
        public delegate string SingleDataHandler(SqlDataReader reader);
        public delegate int IntSingleDataHandler(SqlDataReader reader);

        /// 查询第三方表数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static IEnumerable<object> ExecuteSqlCommand(string sqlText, DataHandler handler)
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var sqlConnection = new SqlConnection(connectionString);
            var resultList = new List<object>();

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            // 基本的查询
            cmd.CommandText = sqlText;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = sqlConnection;

            sqlConnection.Open();

            reader = cmd.ExecuteReader();
            // 委托读取数据
            resultList = handler(reader);

            sqlConnection.Close();
            return resultList;
        }

        /// <summary>
        /// 执行SQL语句，没有代理
        /// </summary>
        /// <param name="sqlText"></param>
        public static bool ExecuteSqlCommand(string sqlText)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var sqlConnection = new SqlConnection(connectionString);

                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                // 基本的查询
                cmd.CommandText = sqlText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection;

                sqlConnection.Open();

                reader = cmd.ExecuteReader();


                sqlConnection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public static void ExecuteProcedure(string procName, Dictionary<string, object> paramDict)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var sqlConnection = new SqlConnection(connectionString);

                var command = new SqlCommand();
                command.CommandText = procName;
                command.CommandType = CommandType.StoredProcedure;
                command.Connection = sqlConnection;

                // 参数赋值
                foreach(KeyValuePair<string, object> item in paramDict)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }

                sqlConnection.Open();

                command.ExecuteNonQuery();

                sqlConnection.Close();
            }
            catch(Exception e)
            {
                throw new Exception(string.Format("存储过程出错：{0}, 详细信息:{1}", procName, e.Message));
            }
        }

        public static int ExecuteScalar(string sqlText)
        {
            try
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                var sqlConnection = new SqlConnection(connectionString);

                SqlCommand cmd = new SqlCommand();

                // 基本的查询
                cmd.CommandText = sqlText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = sqlConnection;

                sqlConnection.Open();

                int count = (int)(cmd.ExecuteScalar());


                sqlConnection.Close();
                return count;
            }
            catch(Exception exception)
            {
                throw exception;
            }
        }
        #endregion

        #region 实例委托方法
        /// <summary>
        /// 查找映射数据是否存在对应记录
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object>SearchInformation(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string Id = reader.GetString(0);
                ResultList.Add(Id);
            }

            return ResultList;
        }

        /// <summary>
        /// 待办
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetPendingResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var result = new XYD_PendingResult();
                long number = reader.GetInt64(0);
                result.DocumentTilte = reader.GetString(1);
                result.ClosedOrHairTime = reader.GetString(2);
                result.MessageId = reader.GetString(3);
                result.WorkflowId = reader.GetString(4);
                result.InitiateEmplId = reader.GetString(5);
                result.InitiateEmplName = reader.GetString(6);
                result.MessageTitle = reader.GetString(7);
                result.MyTask = reader.GetString(8);
                result.ReceiveTime = reader.GetString(9);
                result.MessageIssuedBy = reader.GetString(10);

                ResultList.Add(result);
            }

            return ResultList;
        }

        /// <summary>
        /// 待办
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetAlarmPendingResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string DocumentTilte = reader.GetString(1);
                string ClosedOrHairTime = reader.GetString(2);
                string MessageId = reader.GetString(3);
                string WorkflowId = reader.GetString(4);
                string InitiateEmplId = reader.GetString(5);
                string InitiateEmplName = reader.GetString(6);
                string MessageTitle = reader.GetString(7);
                string MyTask = reader.GetString(8);
                string ReceiveTime = reader.GetString(9);
                int days = reader.GetInt32(10);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    InitiateEmplId = InitiateEmplId,
                    InitiateEmplName = InitiateEmplName,
                    MessageTitle = MessageTitle,
                    MyTask = MyTask,
                    ReceiveTime = ReceiveTime,
                    days = days
                });
            }

            return ResultList;
        }

        /// <summary>
        /// 已处理
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetDealResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var result = new XYD_DealResult();
                long number = reader.GetInt64(0);
                result.DocumentTilte = reader.GetString(1);
                result.ClosedOrHairTime = reader.GetString(2);
                result.MessageId = reader.GetString(3);
                result.WorkflowId = reader.GetString(4);
                result.MessageTitle = reader.GetString(5);
                result.CreateTime = reader.GetString(6);
                result.ReceiveTime = reader.GetString(7);
                result.MessageIssuedBy = reader.GetString(8);
                ResultList.Add(result);
            }

            return ResultList;
        }

        /// <summary>
        /// 发出未完成
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetNoCompleteResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string DocumentTilte = reader.GetString(1);
                string ClosedOrHairTime = reader.GetString(2);
                string MessageId = reader.GetString(3);
                string WorkflowId = reader.GetString(4);
                string MessageTitle = reader.GetString(5);
                string MyTask = reader.GetString(6);
                string ReceiveTime = reader.GetString(7);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    MyTask = MyTask,
                    ReceiveTime = ReceiveTime
                });
            }

            return ResultList;
        }

        /// <summary>
        /// 已完成
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetCompleteResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string DocumentTilte = reader.GetString(1);
                string ClosedOrHairTime = reader.GetString(2);
                string MessageId = reader.GetString(3);
                string WorkflowId = reader.GetString(4);
                string MessageTitle = reader.GetString(5);
                string CreateTime = reader.GetString(6);
                string ReceiveTime = reader.GetString(7);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    CreateTime = CreateTime,
                    ReceiveTime = ReceiveTime,
                });
            }

            return ResultList;
        }

        /// <summary>
        /// 我的申请
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetMyApplyResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string DocumentTilte = reader.GetString(1);
                string ClosedOrHairTime = reader.GetString(2);
                string MessageId = reader.GetString(3);
                string WorkflowId = reader.GetString(4);
                string MessageTitle = reader.GetString(5);
                string CreateTime = reader.GetString(6);
                string ReceiveTime = reader.GetString(7);
                int MessageStatus = reader.GetInt32(8);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    CreateTime = CreateTime,
                    ReceiveTime = ReceiveTime,
                    MessageStatus = MessageStatus
                });
            }

            return ResultList;
        }

        /// <summary>
        /// 获得草稿信息
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetDraftResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string DocumentTilte = reader.GetString(1);
                string ClosedOrHairTime = reader.GetString(2);
                string MessageId = reader.GetString(3);
                string WorkflowId = reader.GetString(4);
                string MessageTitle = reader.GetString(5);
                string CreateTime = reader.GetString(6);
                string ReceiveTime = reader.GetString(7);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    CreateTime = CreateTime,
                    ReceiveTime = ReceiveTime
                });
            }

            return ResultList;
        }

        /// <summary>
        /// 搜索公文
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetSearchResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                long number = reader.GetInt64(0);
                string MessageId = reader.GetString(1);
                string WorkFlowId = reader.GetString(2);
                string ClosedOrHairTime = reader.GetString(3);
                string DocumentTitle = reader.GetString(4);
                string MessageIssuedBy = reader.GetString(5);
                string EmplName = reader.GetString(6);
                string MessageIssuedDept = reader.GetString(7);
                string MessageIssuedDeptName = reader.GetString(8);
                string DeptName = reader.GetString(9);
                string MessageTitle = reader.GetString(10);
                int MessageStatus = reader.GetInt32(11);
                string MessageCreateTime = reader.GetString(12);
                string MessageStatusName = reader.GetString(13);

                ResultList.Add(new
                {
                    MessageId = MessageId,
                    WorkFlowId = WorkFlowId,
                    ClosedOrHairTime = ClosedOrHairTime,
                    DocumentTitle = DocumentTitle,
                    MessageIssuedBy = MessageIssuedBy,
                    MessageIssuedName = EmplName,
                    MessageIssuedDept = MessageIssuedDept,
                    MessageIssuedDeptName = MessageIssuedDeptName,
                    DeptName = DeptName,
                    MessageTitle = MessageTitle,
                    MessageStatus = MessageStatus,
                    MessageCreateTime = MessageCreateTime,
                    MessageStatusName = MessageStatusName
                });
            }

            return ResultList;
        }
        /// <summary>
        /// 根据用户获得顶级部门
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> GetWorkflowByUser(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string deptId = reader.GetString(0);

                ResultList.Add(deptId);
            }

            return ResultList;
        }
        #endregion

        #region 已收藏列表
        public static List<object> WKF_FavoriteList(SqlDataReader reader)
        {
            var ResultList = new List<object>();

            while (reader.Read())
            {
                var MessageId = reader.GetString(1);
                var ClosedOrHairTime = reader.GetDateTime(2);
                var DocumentTitle = reader.GetString(3);
                var InitiateEmplId = reader.GetString(4);
                var InitiateEmplName = reader.GetString(5);
                var MessageTitle = reader.GetString(6);
                var MyTask = reader.GetString(7);
                var ReceiveTime = reader.GetString(8);
                var WorkFlowId = reader.GetString(9);
                ResultList.Add(new
                {
                    MessageId = MessageId,
                    ClosedOrHairTime = ClosedOrHairTime,
                    DocumentTitle = DocumentTitle,
                    InitiateEmplId = InitiateEmplId,
                    InitiateEmplName = InitiateEmplName,
                    MessageTitle = MessageTitle,
                    MyTask = MyTask,
                    ReceiveTime = ReceiveTime,
                    WorkFlowId = WorkFlowId
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取通知人员列表
        public static List<object> GetNotifyReceivers(SqlDataReader reader)
        {
            var ResultList = new List<object>();

            while (reader.Read())
            {
                var HandledBy = reader.GetString(0);
                
                ResultList.Add(HandledBy);
            }
            return ResultList;
        }
        #endregion

        #region 根据用户排序获得用户列表
        public static List<object> WKF_GlobalSortNo(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            var dict = new Dictionary<string, string>();

            while (reader.Read())
            {
                string EmplID = reader.GetString(0);
                string EmplName = reader.GetString(1);
                dict.Add(EmplID, EmplName);
            }

            ResultList.Add(dict);

            return ResultList;
        }
        #endregion
    }
}