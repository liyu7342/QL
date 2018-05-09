/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  MySqlDataPagingReader
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace QL.Database.MySql
{
    /// <summary>
    /// 基于MySql数据库实现的DbDataPagingReader
    /// </summary>
    public class MySqlDataPagingReader
        : DbDataPagingReader
    {
        /// <summary>
        /// 根据MySqlConnection对象实例化
        /// </summary>
        /// <param name="sqlConnection">MySql.Data.MySqlClient.MySqlConnection数据连接对象</param>
        public MySqlDataPagingReader(MySqlConnection sqlConnection)
            : base(sqlConnection)
        { }
        /// <summary>
        /// 根据数据库帮助对象实例化
        /// </summary>
        /// <param name="dbHelper"></param>
        public MySqlDataPagingReader(MySqlDbHelper dbHelper) : base(dbHelper) { }

        /// <summary>
        /// 获取当前页的数据并返回一个数据对象列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override List<T> ReadAsDbObjectList<T>()
        {
            try
            {
                this.BeginExecute();
                this.InitPageCount();
                if (this.RecordCount > 0)
                {
                    string sql = this.GetQueryCommand();
                    using (var command = this.DbHelper.CreateDbCommandWrapped(sql.ToString(), this.Condition.Parameters))
                    {
                        return command.ExecuteDbObjectList<T>();
                    }
                }
                else
                {
                    return new List<T>();
                }
            }
            finally
            {
                this.EndExecute();
            }
        }

        /// <summary>
        /// 获取当前页的数据并返回一个数据表
        /// </summary>
        /// <returns></returns>
        public override System.Data.DataTable ReadAsDataTable()
        {
            try
            {
                this.BeginExecute();
                this.InitPageCount();
                if (this.RecordCount > 0)
                {
                    string sql = this.GetQueryCommand();
                    using (var command = this.DbHelper.CreateDbCommandWrapped(sql.ToString(), this.Condition.Parameters))
                    {
                        DataSet ds = new DataSet();
                        using (var adapter = this.DbHelper.CreateDbDataAdapter(command.Command))
                        {
                            command.FillDataSet(adapter, ds);
                            return ds.Tables.Count > 0 ? ds.Tables[0] : null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                this.EndExecute();
            }
        }
        #region 获取查询的命令语句
        /// <summary>
        /// 获取查询的命令语句
        /// </summary>
        /// <returns></returns>
        protected virtual string GetQueryCommand()
        {
            StringBuilder sql = new StringBuilder();
            int s = (this.PageNumber - 1) * this.PageSize;

            sql.Append(this.Select);
            if (this.Condition.HasCondition)
            {
                sql.Append(" ");
                sql.Append(this.Condition.ToString());
            }
            if (!string.IsNullOrEmpty(this.OrderBy))
            {
                sql.Append(" ORDER BY ");
                sql.Append(this.OrderBy);
            }
            sql.AppendFormat(" LIMIT {0},{1}", s, this.PageSize);
            return sql.ToString();
        }
        #endregion

        public string OrderBy { get; set; }
    }
}
