namespace QL.Database
{
    using QL.Core.Extensions;
    using QL.Core.Log;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class DbCommandExecutor
    {
        public DbCommandExecutor(QL.Database.DbHelper dbHelper)
        {
            this.DbHelper = dbHelper;
        }

        public DbCommandExecutor(DbConnection connection)
            : this(new QL.Database.DbHelper(connection))
        {
        }

        public virtual int Delete(string tableName, params long[] ids)
        {
            return this.Delete(tableName, string.Format("{0} IN ({1})", this.DbHelper.QuoteIdentifier("Id"), string.Join<long>(",", ids)));
        }

        public int Delete(string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.Delete(tableName, null, null);
            }
            return this.Delete(tableName, condition.Condition, condition.Parameters);
        }

        public int Delete(string tableName, string condition)
        {
            return this.Delete(tableName, condition, null);
        }

        public virtual int Delete(string tableName, string condition, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("DELETE FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.Append(" WHERE ");
                builder.Append(condition);
            }
            return this.DbHelper.ExecuteNonQuery(builder.ToString(), pars);
        }

        protected long ExecuteIdentity(DbCommandWrapped command)
        {
            using (command)
            {
                return this.ExecuteIdentity(command.Command, command.KeepConnection);
            }
        }

        public long ExecuteIdentity(DbCommand command)
        {
            return this.ExecuteIdentity(command, false);
        }

        public long ExecuteIdentity(string commandText)
        {
            return this.ExecuteIdentity(this.DbHelper.CreateDbCommandWrapped(commandText));
        }

        public long ExecuteIdentity(CommandType commandType, string commandText)
        {
            return this.ExecuteIdentity(this.DbHelper.CreateDbCommandWrapped(commandType, commandText));
        }

        protected virtual long ExecuteIdentity(DbCommand command, bool keepConnection)
        {
            long num = 0L;
            bool flag = command.Connection.State == ConnectionState.Closed;
            if (flag)
            {
                this.DbHelper.OpenConnection(command.Connection);
            }
            try
            {
                DbCommand identityCommand = this.GetIdentityCommand(command);
                if (identityCommand == command)
                {
                    num = identityCommand.ExecuteScalar().As<long>();
                }
                else if ((command.ExecuteNonQuery() > 0) && (identityCommand != null))
                {
                    num = identityCommand.ExecuteScalar().As<long>();
                }
                if (flag && !keepConnection)
                {
                    command.Connection.Close();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "Execute query error: {0}", new object[] { command.CommandText });
                throw exception;
            }
            return num;
        }

        public long ExecuteIdentity(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteIdentity(this.DbHelper.CreateDbCommandWrapped(commandText, parameters));
        }

        public long ExecuteIdentity(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteIdentity(this.DbHelper.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        public bool Exists(string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.Exists(tableName, null, null);
            }
            return this.Exists(tableName, condition.Condition, condition.Parameters);
        }

        public bool Exists(string tableName, string condition)
        {
            return this.Exists(tableName, condition, null);
        }

        public virtual bool Exists(string tableName, string condition, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT 1 FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.AppendFormat(" WHERE {0}", condition);
            }
            using (IDataReader reader = this.DbHelper.ExecuteReader(builder.ToString(), pars))
            {
                return reader.Read();
            }
        }

        public T GetBy<T>(string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.GetBy<T>(tableName, null, string.Empty, new DbParameter[0]);
            }
            return this.GetBy<T>(tableName, condition.Condition, condition.OrderBy, condition.Parameters);
        }

        public virtual T GetBy<T>(string tableName, long id)
        {
            return this.GetBy<T>(tableName, string.Format("{0}={1}", this.DbHelper.QuoteIdentifier("Id"), id));
        }

        public T GetBy<T>(string tableName, string condition)
        {
            return this.GetBy<T>(tableName, condition, string.Empty, new DbParameter[0]);
        }

        public virtual T GetBy<T>(string tableName, string condition, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT * FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.Append(" WHERE ");
                builder.Append(condition);
            }
            return this.DbHelper.ExecuteDbObject<T>(builder.ToString(), pars);
        }

        public virtual T GetBy<T>(string tableName, string condition, string orderBy, params DbParameter[] pars)
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                return this.GetBy<T>(tableName, condition, pars);
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT TOP 1 * FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.Append(" WHERE ");
                builder.Append(condition);
            }
            builder.Append(" ORDER BY ");
            builder.Append(orderBy);
            return this.DbHelper.ExecuteDbObject<T>(builder.ToString(), pars);
        }

        protected virtual DbCommand GetIdentityCommand(DbCommand command)
        {
            string str = "SELECT @@IDENTITY";
            if (typeof(SqlCommand).IsInstanceOfType(command))
            {
                str = "SELECT SCOPE_IDENTITY()";
                if (command.CommandType == CommandType.Text)
                {
                    command.CommandText = command.CommandText + ";" + str;
                    return command;
                }
            }
            DbCommand command2 = command.Connection.CreateCommand();
            if (command.Transaction != null)
            {
                command2.Transaction = command.Transaction;
            }
            command2.CommandText = str;
            command2.CommandType = CommandType.Text;
            command2.CommandTimeout = command.CommandTimeout;
            return command2;
        }

        public bool Insert(string tableName, object obj)
        {
            return this.Insert(tableName, obj, 0L);
        }

        public virtual bool Insert(string tableName, object obj, long groups)
        {
            QL.Database.DbCommandBuilder builder = this.DbHelper.CreateDbCommandBuilder(tableName);
            PropertyDescriptor descriptor = null;
            Type type = typeof(DbFieldAttribute);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            if (properties.Count == 0)
            {
                return false;
            }
            foreach (PropertyDescriptor descriptor2 in properties)
            {
                DbFieldAttribute attribute = (DbFieldAttribute)descriptor2.Attributes[type];
                if (attribute != null)
                {
                    if (attribute.Identity)
                    {
                        descriptor = descriptor2;
                    }
                    else if (!attribute.ReadOnly && attribute.AllowOperation(groups, true))
                    {
                        string name = string.IsNullOrEmpty(attribute.Name) ? descriptor2.Name : attribute.Name;
                        builder.AddField(name, this.DbHelper.CreateDbParameter(name, attribute.GetDbType(descriptor2.PropertyType), attribute.Size, descriptor2.GetValue(obj)));
                    }
                }
            }
            long num = 0L;
            if (builder.HasFields)
            {
                if (descriptor != null)
                {
                    num = this.ExecuteIdentity(builder.InsertCommandText, builder.Parameters);
                    if (num > 0L)
                    {
                        object obj2 = num;
                        if (descriptor.PropertyType != typeof(long))
                        {
                            obj2 = num.As(descriptor.PropertyType, num);
                        }
                        descriptor.SetValue(obj, obj2);
                    }
                }
                else
                {
                    num = builder.Insert();
                }
            }
            return (num > 0L);
        }

        public virtual List<T> Remove<T>(string tableName, params long[] Ids)
        {
            string str = string.Join<long>(",", Ids);
            return this.Remove<T>(tableName, string.Format("{0} IN ({1})", this.DbHelper.QuoteIdentifier("Id"), str));
        }

        public List<T> Remove<T>(string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.Remove<T>(tableName, null, null);
            }
            return this.Remove<T>(tableName, condition.Condition, condition.Parameters);
        }

        public List<T> Remove<T>(string tableName, string condition)
        {
            return this.Remove<T>(tableName, condition, null);
        }

        public virtual List<T> Remove<T>(string tableName, string condition, params DbParameter[] pars)
        {
            List<T> list = this.Select<T>(tableName, condition, null, pars);
            this.Delete(tableName, condition, pars);
            return list;
        }

        public List<T> Select<T>(string tableName)
        {
            return this.Select<T>(tableName, null, null);
        }

        public List<T> Select<T>(string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.Select<T>(tableName, null, null);
            }
            return this.Select<T>(tableName, condition.Condition, condition.OrderBy, condition.Parameters);
        }

        public List<T> Select<T>(string tableName, string condition)
        {
            return this.Select<T>(tableName, condition, null);
        }

        public List<T> Select<T>(int quantity, string tableName, DbConditionBuilder condition)
        {
            if (condition == null)
            {
                return this.Select<T>(quantity, tableName, null, null);
            }
            return this.Select<T>(quantity, tableName, condition.Condition, condition.OrderBy, condition.Parameters);
        }

        public List<T> Select<T>(int quantity, string tableName, string condition)
        {
            return this.Select<T>(quantity, tableName, condition, null);
        }

        public List<T> Select<T>(string tableName, string condition, string orderBy)
        {
            return this.Select<T>(tableName, condition, orderBy, null);
        }

        public List<T> Select<T>(int quantity, string tableName, string condition, string orderBy)
        {
            return this.Select<T>(quantity, tableName, condition, orderBy, null);
        }

        public virtual List<T> Select<T>(string tableName, string condition, string orderBy, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("SELECT * FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            if (!string.IsNullOrEmpty(condition))
            {
                builder.AppendFormat(" WHERE {0}", condition);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                builder.AppendFormat(" ORDER BY {0}", orderBy);
            }
            return this.DbHelper.ExecuteDbObjectList<T>(builder.ToString(), pars);
        }

        public virtual List<T> Select<T>(int quantity, string tableName, string condition, string orderBy, params DbParameter[] pars)
        {
            StringBuilder builder = new StringBuilder();
            if (quantity > 0)
            {
                builder.AppendFormat("SELECT TOP {0} * FROM {1}", quantity, this.DbHelper.QuoteIdentifier(tableName));
            }
            else
            {
                builder.AppendFormat("SELECT * FROM {0}", this.DbHelper.QuoteIdentifier(tableName));
            }
            if (!string.IsNullOrEmpty(condition))
            {
                builder.AppendFormat(" WHERE {0}", condition);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                builder.AppendFormat(" ORDER BY {0}", orderBy);
            }
            return this.DbHelper.ExecuteDbObjectList<T>(builder.ToString(), pars);
        }

        public bool Update(string tableName, object obj)
        {
            return this.Update(tableName, obj, 0L);
        }

        public virtual bool Update(string tableName, object obj, long groups)
        {
            QL.Database.DbCommandBuilder builder = this.DbHelper.CreateDbCommandBuilder(tableName);
            Type type = typeof(DbFieldAttribute);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            if (properties.Count == 0)
            {
                return false;
            }
            foreach (PropertyDescriptor descriptor in properties)
            {
                DbFieldAttribute attribute = (DbFieldAttribute)descriptor.Attributes[type];
                if (attribute != null)
                {
                    string name = string.IsNullOrEmpty(attribute.Name) ? descriptor.Name : attribute.Name;
                    if ((!attribute.Identity && !attribute.ReadOnly) && attribute.AllowOperation(groups, false))
                    {
                        builder.AddField(name, this.DbHelper.CreateDbParameter(name, attribute.GetDbType(descriptor.PropertyType), attribute.Size, descriptor.GetValue(obj)));
                    }
                    if (attribute.PrimaryKey)
                    {
                        builder.AddCondition(name, this.DbHelper.CreateDbParameter(name, attribute.GetDbType(descriptor.PropertyType), attribute.Size, descriptor.GetValue(obj)));
                    }
                }
            }
            return (builder.Update() > 0);
        }

        public QL.Database.DbHelper DbHelper { get; private set; }
    }
}
