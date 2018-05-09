/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DbCommandBuilder
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using QL.Core.Extensions;
namespace QL.Database
{
    /// <summary>
    /// 数据命令构造器，可生成INSERT或UPDATE语句
    /// </summary>
    public class DbCommandBuilder
    {
        /// <summary>
        /// 构造默认实例
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dbHelper">数据库帮助对象</param>
        public DbCommandBuilder(string tableName, DbHelper dbHelper)
        {
            this.TableName = tableName;
            this.DbHelper = dbHelper;
            Fields = new List<string>();
            Conditions = new List<string>();
            FieldsDbParameters = new Dictionary<string, DbParameter>(10, StringComparer.OrdinalIgnoreCase);
            ConditionsDbParameters = new Dictionary<string, DbParameter>(10, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 字段列表
        /// </summary>
        private List<string> Fields;
        /// <summary>
        /// 条件列表
        /// </summary>
        private List<string> Conditions;
        /// <summary>
        /// 字段列表里用的参数列表
        /// </summary>
        private Dictionary<string, DbParameter> FieldsDbParameters;
        /// <summary>
        /// 条件里用的参数列表
        /// </summary>
        private Dictionary<string, DbParameter> ConditionsDbParameters;
        /// <summary>
        /// 数据库帮助对象
        /// </summary>
        public DbHelper DbHelper { get; private set; }

        /// <summary>
        /// 表名,如User
        /// </summary>d
        public string TableName
        {
            get;
            private set;
        }

        /// <summary>
        /// 返回所有参数
        /// </summary>
        public virtual DbParameter[] Parameters
        {
            get
            {
                List<DbParameter> pars = new List<DbParameter>();
                foreach (var p in this.FieldsDbParameters)
                {
                    if (!this.ConditionsDbParameters.ContainsKey(p.Key))
                        pars.Add(p.Value);
                }
                pars.AddRange(this.ConditionsDbParameters.Values);
                return pars.ToArray();
            }
        }

        /// <summary>
        /// 获取某个名称的参数
        /// </summary>
        /// <param name="name">参数名称,不带前缀</param>
        /// <returns></returns>
        public DbParameter GetParameter(string name)
        {
            name = this.DbHelper.GetDbParameterName(name);
            if (this.FieldsDbParameters.ContainsKey(name))
            {
                return this.FieldsDbParameters[name];
            }
            else if (this.ConditionsDbParameters.ContainsKey(name))
            {
                return this.ConditionsDbParameters[name];
            }
            return null;
        }

        /// <summary>
        /// 判断是否存在某个参数
        /// </summary>
        /// <param name="name">不带前缀的参数名称</param>
        /// <returns></returns>
        public bool ContainParameter(string name)
        {
            name = this.DbHelper.GetDbParameterName(name);
            return this.FieldsDbParameters.ContainsKey(name)
                || this.ConditionsDbParameters.ContainsKey(name);
        }

        /// <summary>
        /// 返回Insert命令
        /// </summary>
        public virtual string InsertCommandText
        {
            get
            {
                if (string.IsNullOrEmpty(this.TableName) || this.Fields.Count == 0) return string.Empty;

                //字段列表
                StringBuilder fields = new StringBuilder();
                //值列表
                StringBuilder values = new StringBuilder();
                foreach (string name in this.Fields)
                {
                    if (fields.Length != 0)
                    {
                        fields.Append(",");
                        values.Append(",");
                    }
                    fields.Append(this.DbHelper.QuoteIdentifier(name));
                    values.Append(this.DbHelper.GetDbParameterName(name));
                }

                StringBuilder buffer = new StringBuilder();
                buffer.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", this.DbHelper.QuoteIdentifier(this.TableName), fields.ToString(), values.ToString());

                return buffer.ToString();
            }
        }
        /// <summary>
        /// 返回Update命令
        /// </summary>
        public virtual string UpdateCommandText
        {
            get
            {
                if (string.IsNullOrEmpty(this.TableName) || this.Fields.Count == 0) return string.Empty;

                //字段列表
                StringBuilder fields = new StringBuilder();
                foreach (string name in this.Fields)
                {
                    if (fields.Length != 0)
                    {
                        fields.Append(",");
                    }
                    fields.Append(string.Format("{0}={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(name)));
                }

                //条件列表
                StringBuilder conditions = new StringBuilder();
                foreach (string name in this.Conditions)
                {
                    if (conditions.Length != 0)
                    {
                        conditions.Append(" AND ");
                    }
                    conditions.Append(string.Format("{0}={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(name)));
                }

                StringBuilder buffer = new StringBuilder();
                buffer.AppendFormat("UPDATE {0} SET {1}", this.DbHelper.QuoteIdentifier(this.TableName), fields.ToString());
                if (conditions.Length > 0)
                {
                    buffer.Append(" WHERE ");
                    buffer.Append(conditions.ToString());
                }

                return buffer.ToString();
            }
        }
        /// <summary>
        /// 增加参数
        /// </summary>
        /// <param name="list"></param>
        /// <param name="par"></param>
        protected void AddParameter(Dictionary<string, DbParameter> list, DbParameter par)
        {
            if (par != null)
            {
                if (list.ContainsKey(par.ParameterName))
                {
                    list[par.ParameterName] = par;
                }
                else
                {
                    list.Add(par.ParameterName, par);
                }
            }
        }
        #region AddField
        /// <summary>
        /// 添加字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型</param>
        /// <param name="value">参数值</param>
        public void AddField(string name, DbType type, object value)
        {
            this.AddField(name, this.DbHelper.CreateDbParameter(name, type, value));
        }
        /// <summary>
        /// 添加字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="value">参数值</param>
        public void AddField(string name, DbType type, int size, object value)
        {
            this.AddField(name, this.DbHelper.CreateDbParameter(name, type, size, value));
        }
        /// <summary>
        /// 添加字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="par"></param>
        public void AddField(string name, DbParameter par)
        {
            if (!Fields.Exists(x => string.Equals(name, x, StringComparison.OrdinalIgnoreCase)))
            {
                //不存在则添加新的
                Fields.Add(name);
            }
            this.AddParameter(this.FieldsDbParameters, par);
        }
        #endregion

        #region AddCondition
        /// <summary>
        /// 添加条件字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型</param>
        /// <param name="value">参数值</param>
        public void AddCondition(string name, DbType type, object value)
        {
            this.AddCondition(name, this.DbHelper.CreateDbParameter(name, type, value));
        }
        /// <summary>
        /// 添加条件字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型</param>
        /// <param name="size">参数大小</param>
        /// <param name="value">参数值</param>
        public void AddCondition(string name, DbType type, int size, object value)
        {
            this.AddCondition(name, this.DbHelper.CreateDbParameter(name, type, size, value));
        }
        /// <summary>
        /// 添加条件字段参数
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="par"></param>
        public void AddCondition(string name, DbParameter par)
        {
            if (!Conditions.Exists(x => string.Equals(name, x, StringComparison.OrdinalIgnoreCase)))
            {
                //不存在则添加新的
                Conditions.Add(name); 
            }
            this.AddParameter(this.ConditionsDbParameters, par);
        }
        #endregion

        /// <summary>
        /// 是否有待插入或更新的字段
        /// </summary>
        public bool HasFields
        {
            get
            {
                return this.Fields.Count > 0;
            }
        }

        /// <summary>
        /// 执行插入操作
        /// </summary>
        /// <returns></returns>
        public int Insert()
        {
            string sql = this.InsertCommandText;
            if (string.IsNullOrEmpty(sql)) return 0;
            return this.DbHelper.ExecuteNonQuery(sql, this.Parameters);
        }
        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <returns></returns>
        public int Update()
        {
            string sql = this.UpdateCommandText;
            if (string.IsNullOrEmpty(sql)) return 0;
            return this.DbHelper.ExecuteNonQuery(sql, this.Parameters);
        }

        /// <summary>
        /// 清除所有字段参数
        /// </summary>
        public void ClearAll()
        {
            this.Fields.Clear();
            this.Conditions.Clear();
            this.FieldsDbParameters.Clear();
            this.ConditionsDbParameters.Clear();
        }
    }
}
