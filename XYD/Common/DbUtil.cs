using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using XYD.Entity;
using XYD.Models;

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

        /// <summary>
        /// 指定数据连接字符串
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sqlText"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IEnumerable<object> ExecuteSqlCommand(string connectionString, string sqlText, DataHandler handler)
        {
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
        /// 执行SQL语句，没有代理
        /// </summary>
        /// <param name="sqlText"></param>
        public static void ExecuteSqlCommand(string connectionString, string sqlText)
        {
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
                foreach (KeyValuePair<string, object> item in paramDict)
                {
                    command.Parameters.AddWithValue(item.Key, item.Value);
                }

                sqlConnection.Open();

                command.ExecuteNonQuery();

                sqlConnection.Close();
            }
            catch (Exception e)
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
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static int ExecuteScalar(string connectionString, string sqlText)
        {
            try
            {
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
            catch (Exception exception)
            {
                throw exception;
            }
        }

        #region 查询工资
        public static List<object> GetSalary(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                decimal shouldPay = reader.GetDecimal(0);
                decimal salary = reader.GetDecimal(1);
                int year = reader.GetInt32(2);
                int month = reader.GetByte(3);
                ResultList.Add(new XYD_Salary
                {
                    ShouldPay = shouldPay,
                    Salary = salary,
                    Year = year,
                    Month = month
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取部门列表
        public static List<object> GetDeptIds(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var deptId = reader.GetString(0);

                ResultList.Add(deptId);
            }
            return ResultList;
        }
        #endregion

        #region 查询工资明细
        public static List<object> GetDetailSalary(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                ResultList.Add(new
                {
                    cPsn_Num = reader.GetString(0),
                    cPsn_Name = reader.GetString(1),
                    cDeptName = reader.GetString(2),
                    F_9 = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                    F_10 = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    F_12 = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                    F_11 = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                    F_26 = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    F_1 = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                    F_13 = reader.IsDBNull(9) ? 0m : reader.GetDecimal(9),
                    F_14 = reader.IsDBNull(10) ? 0m : reader.GetDecimal(10),
                    F_15 = reader.IsDBNull(11) ? 0m : reader.GetDecimal(11),
                    F_16 = reader.IsDBNull(12) ? 0m : reader.GetDecimal(12),
                    F_17 = reader.IsDBNull(13) ? 0m : reader.GetDecimal(13),
                    F_18 = reader.IsDBNull(14) ? 0m : reader.GetDecimal(14),
                    F_19 = reader.IsDBNull(15) ? 0m : reader.GetDecimal(15),
                    F_20 = reader.IsDBNull(16) ? 0m : reader.GetDecimal(16),
                    F_21 = reader.IsDBNull(17) ? 0m : reader.GetDecimal(17),
                    F_22 = reader.IsDBNull(18) ? 0m : reader.GetDecimal(18),
                    F_23 = reader.IsDBNull(19) ? 0m : reader.GetDecimal(19),
                    F_1102 = reader.IsDBNull(20) ? 0m : reader.GetDecimal(20),
                    F_1103 = reader.IsDBNull(21) ? 0m : reader.GetDecimal(21),
                    F_1104 = reader.IsDBNull(22) ? 0m : reader.GetDecimal(22),
                    F_1105 = reader.IsDBNull(23) ? 0m : reader.GetDecimal(23),
                    F_1106 = reader.IsDBNull(24) ? 0m : reader.GetDecimal(24),
                    F_1108 = reader.IsDBNull(25) ? 0m : reader.GetDecimal(25),
                    F_1112 = reader.IsDBNull(26) ? 0m : reader.GetDecimal(26),
                    F_1116 = reader.IsDBNull(27) ? 0m : reader.GetDecimal(27),
                    F_1115 = reader.IsDBNull(28) ? 0m : reader.GetDecimal(28),
                    F_1114 = reader.IsDBNull(29) ? 0m : reader.GetDecimal(29),
                    F_1113 = reader.IsDBNull(30) ? 0m : reader.GetDecimal(30),
                    F_1001 = reader.IsDBNull(31) ? 0m : reader.GetDecimal(31),
                    F_1002 = reader.IsDBNull(32) ? 0m : reader.GetDecimal(32),
                    F_6 = reader.IsDBNull(33) ? 0m : reader.GetDecimal(33),
                    F_1003 = reader.IsDBNull(34) ? 0m : reader.GetDecimal(34),
                    F_25 = reader.IsDBNull(35) ? 0m : reader.GetDecimal(35),
                    F_24 = reader.IsDBNull(36) ? 0m : reader.GetDecimal(36),
                    F_1004 = reader.IsDBNull(37) ? 0m : reader.GetDecimal(37),
                    F_2 = reader.IsDBNull(38) ? 0m : reader.GetDecimal(38),
                    F_3 = reader.IsDBNull(39) ? 0m : reader.GetDecimal(39),
                    cDepCode = reader.GetString(40),
                    iYear = reader.GetInt32(41),
                    iMonth = reader.GetByte(42)
                });
            }
            return ResultList;
        }

        internal static object ExecuteSqlCommand(string sql, DateTime date, DateTime dateTime, object backupMoneyReport)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 查询工资明细
        public static List<object> GetReportSalary(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                ResultList.Add(new
                {
                    cPsn_Num = reader.GetString(0),
                    cPsn_Name = reader.GetString(1),
                    cDeptName = reader.GetString(2),
                    F_9 = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                    F_10 = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    F_12 = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                    F_11 = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                    F_26 = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    F_1 = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8),
                    F_13 = reader.IsDBNull(9) ? 0m : reader.GetDecimal(9),
                    F_14 = reader.IsDBNull(10) ? 0m : reader.GetDecimal(10),
                    F_15 = reader.IsDBNull(11) ? 0m : reader.GetDecimal(11),
                    F_16 = reader.IsDBNull(12) ? 0m : reader.GetDecimal(12),
                    F_17 = reader.IsDBNull(13) ? 0m : reader.GetDecimal(13),
                    F_18 = reader.IsDBNull(14) ? 0m : reader.GetDecimal(14),
                    F_19 = reader.IsDBNull(15) ? 0m : reader.GetDecimal(15),
                    F_20 = reader.IsDBNull(16) ? 0m : reader.GetDecimal(16),
                    F_21 = reader.IsDBNull(17) ? 0m : reader.GetDecimal(17),
                    F_22 = reader.IsDBNull(18) ? 0m : reader.GetDecimal(18),
                    F_23 = reader.IsDBNull(19) ? 0m : reader.GetDecimal(19),
                    F_1102 = reader.IsDBNull(20) ? 0m : reader.GetDecimal(20),
                    F_1103 = reader.IsDBNull(21) ? 0m : reader.GetDecimal(21),
                    F_1104 = reader.IsDBNull(22) ? 0m : reader.GetDecimal(22),
                    F_1105 = reader.IsDBNull(23) ? 0m : reader.GetDecimal(23),
                    F_1106 = reader.IsDBNull(24) ? 0m : reader.GetDecimal(24),
                    F_1108 = reader.IsDBNull(25) ? 0m : reader.GetDecimal(25),
                    F_1112 = reader.IsDBNull(26) ? 0m : reader.GetDecimal(26),
                    F_1116 = reader.IsDBNull(27) ? 0m : reader.GetDecimal(27),
                    F_1115 = reader.IsDBNull(28) ? 0m : reader.GetDecimal(28),
                    F_1114 = reader.IsDBNull(29) ? 0m : reader.GetDecimal(29),
                    F_1113 = reader.IsDBNull(30) ? 0m : reader.GetDecimal(30),
                    F_1001 = reader.IsDBNull(31) ? 0m : reader.GetDecimal(31),
                    F_1002 = reader.IsDBNull(32) ? 0m : reader.GetDecimal(32),
                    F_6 = reader.IsDBNull(33) ? 0m : reader.GetDecimal(33),
                    F_1003 = reader.IsDBNull(34) ? 0m : reader.GetDecimal(34),
                    F_25 = reader.IsDBNull(35) ? 0m : reader.GetDecimal(35),
                    F_24 = reader.IsDBNull(36) ? 0m : reader.GetDecimal(36),
                    F_1004 = reader.IsDBNull(37) ? 0m : reader.GetDecimal(37),
                    F_2 = reader.IsDBNull(38) ? 0m : reader.GetDecimal(38),
                    F_3 = reader.IsDBNull(39) ? 0m : reader.GetDecimal(39),
                    cDepCode = reader.GetString(40)
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取备用金统计
        public static List<object> BackupMoneyReport(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var EmplName = reader.GetString(0);
                var DeptName = reader.GetString(1);
                var Amount = reader.GetDecimal(2);
                ResultList.Add(new XYD_BackupMoneyReport
                {
                    EmplName = EmplName,
                    DeptName = DeptName,
                    Amount = Amount
                });
            }
            return ResultList;
        }
        #endregion

        #region 查询未同步供应商
        public static List<object> GetUnSyncVendor(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var Code = reader.GetString(0);
                var Name = reader.GetString(1);
                ResultList.Add(new XYD_Vendor
                {
                    Code = Code,
                    Name = Name
                });
            }
            return ResultList;
        }
        #endregion

        #region 实例委托方法
        /// <summary>
        /// 查找映射数据是否存在对应记录
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> SearchInformation(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string Id = reader.GetString(0);
                ResultList.Add(Id);
            }

            return ResultList;
        }
        #endregion

        #region 获取映射文件版本
        public static List<object> GetReceiveFile(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string version = reader.GetString(0);
                ResultList.Add(version);
            }
            return ResultList;
        }
        #endregion

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
                result.DocumentTitle = reader.GetString(1);
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

        #region 查询员工
        internal static List<object> searchEmployee(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                ResultList.Add(new
                {
                    EmplID = reader.GetString(0),
                    EmplNO = reader.GetString(1),
                    EmplName = reader.GetString(2),
                    DeptName = reader.GetString(3),
                    PositionName = reader.GetString(4),
                    ContractDate = reader.IsDBNull(5) ? string.Empty : reader.GetDateTime(5).ToString("yyyy-MM-dd")
                });
            }

            return ResultList;
        }
        #endregion

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
                result.DocumentTitle = reader.GetString(1);
                result.ClosedOrHairTime = reader.GetString(2);
                result.MessageId = reader.GetString(3);
                result.WorkflowId = reader.GetString(4);
                result.MessageTitle = reader.GetString(5);
                result.CreateTime = reader.GetString(6);
                result.ReceiveTime = reader.GetString(7);
                result.MessageIssuedBy = reader.GetString(8);
                result.EmplName = reader.GetString(9);
                result.MessageStatusName = reader.GetString(10);
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
                string MessageStatusName = reader.GetString(8);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    CreateTime = CreateTime,
                    ReceiveTime = ReceiveTime,
                    MessageStatusName = MessageStatusName
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
                string MessageIssuedBy = reader.GetString(9);

                ResultList.Add(new
                {
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    MessageTitle = MessageTitle,
                    CreateTime = CreateTime,
                    ReceiveTime = CreateTime,
                    MessageStatus = MessageStatus,
                    MessageIssuedBy = MessageIssuedBy
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

        public static List<object> GetHistory(SqlDataReader reader)
        {
            var ResultList = new List<object>();

            while (reader.Read())
            {
                ResultList.Add(new
                {
                    NodeName = reader.GetString(0),
                    EmplName = reader.GetString(1),
                    Operation = reader.GetString(2),
                    Opinion = reader.GetString(3),
                    CreateTime = reader.GetDateTime(4) == null ? string.Empty : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss")
                });
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

        #region 获得数据库选项
        public static List<object> GetOptions(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string Value = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                string InterValue = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                ResultList.Add(new XYD_Cell_Options
                {
                    Value = Value,
                    InterValue = InterValue
                });
            }
            return ResultList;
        }
        #endregion


        /// <summary>
        /// 查找流程数量
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<object> CountMessage(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string WorkflowId = reader.GetString(0);
                int MessageCount = reader.GetInt32(1);
                string MessageTitle = reader.GetString(2);
                string FolderName = reader.GetString(3);
                ResultList.Add(new XYD_DB_Message_Count
                {
                    WorkflowId = WorkflowId,
                    MessageCount = MessageCount,
                    MessageTitle = MessageTitle,
                    FolderName = FolderName
                });
            }
            return ResultList;
        }

        #region 获得用友数据
        public static List<object> GetU8Person(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                string cPersonCode = reader.GetString(0);
                string cPersonName = reader.GetString(1);
                string cDepCode = reader.GetString(2);
                ResultList.Add(new XYD_U8_Person
                {
                    cPersonCode = cPersonCode,
                    cPersonName = cPersonName,
                    cDepCode = cDepCode
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取考勤结果
        public static List<object> GetLeaveReport(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var ID = reader.GetInt32(0);
                var EmplID = reader.GetString(1);
                var EmplName = reader.GetString(2);
                var StartDate = reader.GetDateTime(3);
                var EndDate = reader.GetDateTime(4);
                var CreateTime = reader.GetDateTime(5);
                var Category = reader.GetString(6);
                var Status = reader.GetString(7);
                var Reason = reader.GetString(8);
                ResultList.Add(new XYD_Leave_Result
                {
                    ID = ID,
                    EmplID = EmplID,
                    EmplName = EmplName,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    CreateTime = CreateTime,
                    Category = Category,
                    Status = Status,
                    Reason = Reason
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取用户统计
        public static List<object> GetUserReport(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while(reader.Read())
            {
                ResultList.Add(new XYD_UserReport()
                {
                    EmplID = reader.GetString(0),
                    EmplName = reader.GetString(1),
                    EmplSex = reader.GetString(2),
                    EmplNO = reader.GetString(3),
                    DeptName = reader.GetString(4),
                    PositionName = reader.GetString(5),
                    EmplBirth = reader.IsDBNull(6) ? string.Empty : reader.GetDateTime(6).ToString("yyyy-MM-dd"),
                    CredNo = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                    BankNo = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    EmployeeDate = reader.IsDBNull(9) ? string.Empty : reader.GetDateTime(9).ToString("yyyy-MM-dd"),
                    FormalDate = reader.IsDBNull(10) ? string.Empty : reader.GetDateTime(10).ToString("yyyy-MM-dd"),
                    ContractDate = reader.IsDBNull(11) ? string.Empty : reader.GetDateTime(11).ToString("yyyy-MM-dd"),
                    SocialInsuranceTotalMonth = reader.IsDBNull(12) ? 0 : reader.GetInt32(12),
                    Residence = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                    CurrentAddress = reader.IsDBNull(14) ? string.Empty : reader.GetString(14)
                });
            }
            return ResultList;
        }
        #endregion

        #region 获取考勤记录
        public static List<object> GetLeaveRecord(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                var EmplID = reader.GetString(0);
                var EmplName = reader.GetString(1);
                var EmplNO = reader.GetString(2);
                var Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                var StartDate = reader.GetDateTime(4);
                var EndDate = reader.GetDateTime(5);
                var Reason = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                
                ResultList.Add(new XYD_Leave_Search
                {
                    EmplID = EmplID,
                    EmplName = EmplName,
                    EmplNO = EmplNO,
                    Category = Category,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Reason = Reason
                });
            }
            return ResultList;
        }
        #endregion

    }
}