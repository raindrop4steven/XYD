using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DeptOA.Common
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

        public static List<object> GetPendingResult(SqlDataReader reader)
        {
            var ResultList = new List<object>();
            while (reader.Read())
            {
                int number = reader.GetInt32(0);
                string DocumentNumber = reader.GetString(1);
                string DocumentTilte = reader.GetString(2);
                string ClosedOrHairTime = reader.GetString(3);
                string MessageId = reader.GetString(4);
                string WorkflowId = reader.GetString(5);
                string SequenceName = reader.GetString(6);
                string SequenceNumber = reader.GetString(7);
                string InitiateEmplId = reader.GetString(8);
                string InitiateEmplName = reader.GetString(9);
                string MessageTitle = reader.GetString(10);
                string MyTask = reader.GetString(11);
                string ReceiveTime = reader.GetString(12);

                ResultList.Add(new
                {
                    number = number,
                    DocumentNumber = DocumentNumber,
                    DocumentTitle = DocumentTilte,
                    ClosedOrHairTime = ClosedOrHairTime,
                    MessageId = MessageId,
                    WorkflowId = WorkflowId,
                    SequenceName = SequenceName,
                    SequenceNumber = SequenceNumber,
                    InitiateEmplId = InitiateEmplId,
                    InitiateEmplName = InitiateEmplName,
                    MessageTitle = MessageTitle,
                    MyTask = MyTask,
                    ReceiveTime = ReceiveTime
                });
            }

            return ResultList;
        }
        #endregion

    }
}