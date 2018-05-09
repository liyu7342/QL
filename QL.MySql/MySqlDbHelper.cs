/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  MySqlDbHelper
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using QL.Core.Extensions;
using System.Collections;
namespace QL.Database.MySql
{
    /// <summary>
    /// 基于MySql数据库的DbHelper实例
    /// </summary>
    public class MySqlDbHelper : DbHelper
    {
        #region 构造函数
        /// <summary>
        /// 根据DbConnection实例化对象
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        public MySqlDbHelper(MySqlConnection connection) : this(connection, false)
        {}
        /// <summary>
        /// 根据DbConnection及是否保持连接状态参数实例化对象
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <param name="keepConnection">是否保持数据源的连接状态。如果true则连接被打开后不会自动关闭</param>
        public MySqlDbHelper(MySqlConnection connection, bool keepConnection) : base(connection, keepConnection)
        {}
        #endregion

        /// <summary>
        /// 返回当前的数据库的连接对象实例
        /// </summary>
        public new MySqlConnection Connection
        {
            get
            {
                return base.Connection.As<MySqlConnection>();
            }
        }

        /// <summary>
        /// 返回或设置当前执行的命令所用的事务
        /// </summary>
        public new MySqlTransaction DbTransaction
        {
            get
            {
                return base.DbTransaction.As<MySqlTransaction>();
            }
            set
            {
                base.DbTransaction = value;
            }
        }

        #region 构造命令参数
        /// <summary>
        /// 获取带前缀的参数名称
        /// </summary>
        /// <param name="name">源名称,不带前缀</param>
        /// <returns></returns>
        public override string GetDbParameterName(string name)
        {
            return name.StartsWith("?") ? name : string.Concat("?", name);
        }
        #endregion

        #region 字符转义
        /// <summary>
        /// 需要转义的字符列表
        /// </summary>
        private static BitArray EscapeCharArray = CreateEscapeCharArray();
        /// <summary>
        /// 特殊需要转义的字符
        /// </summary>
        private const string EscapeChars = "\u0022\u0027\u0060\u00b4\u02b9\u02ba\u02bb\u02bc\u02c8\u02ca\u02cb\u02d9\u0300\u0301\u2018\u2019\u201a\u2032\u2035\u275b\u275c\uff07\u005c\u00a5\u0160\u20a9\u2216\ufe68\uff3c";
       
        /// <summary>
        /// 建立需要转义的字符列表表
        /// </summary>
        /// <returns></returns>
        private static BitArray CreateEscapeCharArray()
        {
            var arries = new BitArray(char.MaxValue + 1);
            foreach (char c in EscapeChars)
            {
                arries[c] = true;
            }
            return arries;
        }
        /// <summary>
        /// 对字符数据进行转义，如将单引号、双引号、斜杠字符转义
        /// </summary>
        /// <param name="value">需要转义的字符</param>
        /// <returns></returns>
        public override string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            int p = -1;
            for (var i = 0; i < value.Length; i++)
            {
                if (EscapeCharArray[value[i]])
                {
                    p = i;
                    break;
                }
            }
            if (p == -1) return value;  //不需要转义
            StringBuilder buffer = new StringBuilder(value.Substring(0, p));
            for (var i = p; i < value.Length; i++)
            {
                char c = value[i];
                if (EscapeCharArray[c])
                {
                    buffer.Append('\\');
                }
                buffer.Append(c);
            }
            return buffer.ToString();
        }
        #endregion

        #region 获取数据执行者
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DbCommandExecutor CreateDbCommandExecutor()
        {
            return new MySqlDbCommandExecutor(this);
        }
        #endregion
    }
}
