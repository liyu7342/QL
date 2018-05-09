using MySql.Data.MySqlClient;
namespace QL.Database.MySql
{
    
    using QL.Database;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Text;

    public class MySqlDbCommandExecutor : DbCommandExecutor
    {
        public MySqlDbCommandExecutor(MySqlConnection connection)
            : base(new MySqlDbHelper(connection))
        {
        }

        public MySqlDbCommandExecutor(MySqlDbHelper dbHelper)
            : base(dbHelper)
        {
        }

        public override T GetBy<T>(string tableName, string condition, string orderBy, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT * FROM {0}", base.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.Append(" WHERE ");
                builder.Append(condition);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                builder.Append(" ORDER BY ");
                builder.Append(orderBy);
            }
            builder.Append(" LIMIT 1");
            return base.DbHelper.ExecuteDbObject<T>(builder.ToString(), pars);
        }

        protected override DbCommand GetIdentityCommand(DbCommand command)
        {
            if (command.CommandType == CommandType.Text)
            {
                command.CommandText = command.CommandText + ";SELECT LAST_INSERT_ID();";
                return command;
            }
            DbCommand command2 = command.Connection.CreateCommand();
            if (command.Transaction != null)
            {
                command2.Transaction = command.Transaction;
            }
            command2.CommandText = "SELECT LAST_INSERT_ID()";
            command2.CommandType = CommandType.Text;
            command2.CommandTimeout = command.CommandTimeout;
            return command2;
        }

        public override List<T> Select<T>(int quantity, string tableName, string condition, string orderBy, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT * FROM {0}", base.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.AppendFormat(" WHERE {0}", condition);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                builder.AppendFormat(" ORDER BY {0}", orderBy);
            }
            if (quantity > 0)
            {
                builder.AppendFormat(" LIMIT {0}", quantity);
            }
            return base.DbHelper.ExecuteDbObjectList<T>(builder.ToString(), pars);
        }
    }
}