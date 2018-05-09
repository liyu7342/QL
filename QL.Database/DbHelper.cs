namespace QL.Database
{
    using QL.Core.Extensions;
    using QL.Core.Log;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public class DbHelper : IDisposable
    {
        private System.Data.Common.DbTransaction _DbTransaction;
        private DbCommandExecutor _Executor;
        [ThreadStatic]
        private static bool _ignoreSlaveConnection;
        private DbProviderFactory ConnectionDbProviderFactory;
        private bool DbTransactionConnectionIsClosed;
        private System.Data.Common.DbCommandBuilder IdentifierFormatter;
        private volatile int seqParameterIndex;

        public DbHelper(DbConnection connection)
            : this(connection, false)
        {
        }

        public DbHelper(DbConnection connection, bool keepConnection)
        {
            this.ChangeDbConnection(connection, keepConnection);
            this.SlaveConnections = new List<DbConnection>();
        }

        public System.Data.Common.DbTransaction BeginTransaction()
        {
            if (this._DbTransaction == null)
            {
                this.DbTransactionConnectionIsClosed = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.DbTransactionConnectionIsClosed = true;
                    this.OpenConnection(this.Connection);
                }
                this._DbTransaction = this.Connection.BeginTransaction();
                this.DbTransactionIsCreatedByDbConnection = true;
            }
            return this._DbTransaction;
        }

        public System.Data.Common.DbTransaction BeginTransaction(IsolationLevel il)
        {
            if (this._DbTransaction == null)
            {
                this.DbTransactionConnectionIsClosed = false;
                if (this.Connection.State == ConnectionState.Closed)
                {
                    this.DbTransactionConnectionIsClosed = true;
                    this.OpenConnection(this.Connection);
                }
                this._DbTransaction = this.Connection.BeginTransaction(il);
                this.DbTransactionIsCreatedByDbConnection = true;
            }
            return this._DbTransaction;
        }

        public void ChangeDbConnection(DbConnection connection)
        {
            this.ChangeDbConnection(connection, false);
        }

        public void ChangeDbConnection(DbConnection connection, bool keepConnection)
        {
            this.Connection = connection;
            this.KeepConnection = keepConnection;
            this.DbTransaction = null;
            this.OnDbConnectionReset();
        }

        public void Close()
        {
            this.CloseConnection(this.Connection);
            foreach (DbConnection connection in this.SlaveConnections)
            {
                this.CloseConnection(connection);
            }
        }

        protected void CloseConnection(DbConnection connection)
        {
            if ((connection != null) && (connection.State != ConnectionState.Closed))
            {
                connection.Close();
            }
            this.seqParameterIndex = 0;
        }

        public void CommitTransaction()
        {
            if (this.DbTransaction != null)
            {
                this.DbTransaction.Commit();
                this.RecycleTransaction();
            }
        }

        public DbCommand CreateDbCommand()
        {
            DbCommand command = null;
            DbConnection connection = this.Connection;
            if (this.DbTransaction != null)
            {
                connection = this.DbTransaction.Connection;
                command = connection.CreateCommand();
                command.Transaction = this.DbTransaction;
            }
            else
            {
                command = connection.CreateCommand();
            }
            if (connection.ConnectionTimeout > command.CommandTimeout)
            {
                command.CommandTimeout = connection.ConnectionTimeout;
            }
            return command;
        }

        public DbCommand CreateDbCommand(string commandText)
        {
            return this.CreateDbCommand(CommandType.Text, commandText, null);
        }

        public DbCommand CreateDbCommand(CommandType commandType, string commandText)
        {
            return this.CreateDbCommand(commandType, commandText, null);
        }

        public DbCommand CreateDbCommand(string commandText, params DbParameter[] parameters)
        {
            return this.CreateDbCommand(CommandType.Text, commandText, parameters);
        }

        public DbCommand CreateDbCommand(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            DbCommand command = null;
            if (!_ignoreSlaveConnection && this.IsSelectCommandText(commandType, commandText))
            {
                command = this.CreateDbCommandFromDbConnection(this.GetSlaveConnection(commandType, commandText));
            }
            if (command == null)
            {
                command = this.CreateDbCommand();
            }
            command.CommandType = commandType;
            command.CommandText = commandText;
            StringBuilder builder = new StringBuilder(0x80);
            builder.AppendFormat("[sql-ds] {0}\r\n", command.Connection.DataSource);
            builder.AppendFormat("[sql-command] {0}\r\n", commandText);
            if (parameters != null)
            {
                foreach (DbParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        builder.AppendFormat("[sql-param] {0} = {1}\r\n", parameter.ParameterName, parameter.Value);
                        if (command.Parameters.Contains(parameter.ParameterName))
                        {
                            command.Parameters[parameter.ParameterName] = parameter;
                        }
                        else
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                }
            }
            Logger.Debug(builder.ToString());
            return command;
        }

        public virtual QL.Database.DbCommandBuilder CreateDbCommandBuilder(string tableName)
        {
            return new QL.Database.DbCommandBuilder(tableName, this);
        }

        protected virtual DbCommandExecutor CreateDbCommandExecutor()
        {
            return new DbCommandExecutor(this);
        }

        protected DbCommand CreateDbCommandFromDbConnection(DbConnection connection)
        {
            DbCommand command = null;
            if (connection != null)
            {
                command = connection.CreateCommand();
                if (connection.ConnectionTimeout > command.CommandTimeout)
                {
                    command.CommandTimeout = connection.ConnectionTimeout;
                }
            }
            return command;
        }

        private DbCommandWrapped CreateDbCommandWrapped(DbCommand command)
        {
            return DbCommandWrapped.Create(command, this.KeepConnection);
        }

        public DbCommandWrapped CreateDbCommandWrapped(string commandText)
        {
            return this.CreateDbCommandWrapped(this.CreateDbCommand(commandText));
        }

        public DbCommandWrapped CreateDbCommandWrapped(CommandType commandType, string commandText)
        {
            return this.CreateDbCommandWrapped(this.CreateDbCommand(commandType, commandText));
        }

        public DbCommandWrapped CreateDbCommandWrapped(string commandText, params DbParameter[] parameters)
        {
            return this.CreateDbCommandWrapped(this.CreateDbCommand(commandText, parameters));
        }

        public DbCommandWrapped CreateDbCommandWrapped(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.CreateDbCommandWrapped(this.CreateDbCommand(commandType, commandText, parameters));
        }

        public virtual DbConditionBuilder CreateDBConditionBuilder()
        {
            return new DbConditionBuilder(this);
        }

        public virtual DbDataAdapter CreateDbDataAdapter(DbCommand command)
        {
            DbProviderFactory dbProviderFactory = GetDbProviderFactory(command.Connection);
            if (dbProviderFactory == null)
            {
                return null;
            }
            DbDataAdapter adapter = dbProviderFactory.CreateDataAdapter();
            adapter.SelectCommand = command;
            return adapter;
        }

        public DbParameter CreateDbParameter(string name)
        {
            DbParameter parameter = this.GetDbProviderFactory().CreateParameter();
            parameter.ParameterName = this.GetDbParameterName(name);
            return parameter;
        }

        public DbParameter CreateDbParameter(string name, object value)
        {
            DbParameter parameter = this.CreateDbParameter(name);
            parameter.Value = this.GetDbParameterValue(value, DbType.Object, null);
            return parameter;
        }

        public DbParameter CreateDbParameter(string name, DbType dbType, object value)
        {
            DbParameter parameter = this.CreateDbParameter(name);
            parameter.DbType = dbType;
            parameter.Value = this.GetDbParameterValue(value, dbType, null);
            return parameter;
        }

        public DbParameter CreateDbParameter(string name, DbType dbType, ParameterDirection direction, object value)
        {
            DbParameter parameter = this.CreateDbParameter(name);
            parameter.DbType = dbType;
            parameter.Direction = direction;
            parameter.Value = this.GetDbParameterValue(value, dbType, null);
            return parameter;
        }

        public DbParameter CreateDbParameter(string name, DbType dbType, int size, object value)
        {
            DbParameter parameter = this.CreateDbParameter(name);
            parameter.DbType = dbType;
            parameter.Size = size;
            parameter.Value = this.GetDbParameterValue(value, dbType, new int?(size));
            return parameter;
        }

        public DbParameter CreateDbParameter(string name, DbType dbType, int size, ParameterDirection direction, object value)
        {
            DbParameter parameter = this.CreateDbParameter(name);
            parameter.DbType = dbType;
            parameter.Size = size;
            parameter.Direction = direction;
            parameter.Value = this.GetDbParameterValue(value, dbType, new int?(size));
            return parameter;
        }

        public virtual string CreateSeqParameterName()
        {
            int num;
            this.seqParameterIndex = (num = this.seqParameterIndex) + 1;
            return ("p" + num);
        }

        public void Dispose()
        {
            if (this.Connection != null)
            {
                this.Close();
                this.Connection.Dispose();
                this.Connection = null;
            }
        }

        public virtual string EscapeString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            int length = -1;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\'')
                {
                    length = i;
                    break;
                }
            }
            if (length == -1)
            {
                return value;
            }
            StringBuilder builder = new StringBuilder(value.Substring(0, length));
            builder.Append("''");
            for (int j = length + 1; j < value.Length; j++)
            {
                char ch = value[j];
                if (ch == '\'')
                {
                    builder.Append('\'');
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        protected DataSet ExecuteDataSet(DbCommandWrapped command)
        {
            DataSet dataset = new DataSet();
            this.FillDataSet(dataset, command);
            return dataset;
        }

        public DataSet ExecuteDataSet(DbCommand command)
        {
            DataSet dataset = new DataSet();
            this.FillDataSet(dataset, command);
            return dataset;
        }

        public DataSet ExecuteDataSet(string commandText)
        {
            return this.ExecuteDataSet(this.CreateDbCommandWrapped(commandText));
        }

        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            return this.ExecuteDataSet(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public DataSet ExecuteDataSet(string commandText, DbParameter[] parameters)
        {
            return this.ExecuteDataSet(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public DataSet ExecuteDataSet(CommandType commandType, string commandText, DbParameter[] parameters)
        {
            return this.ExecuteDataSet(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected T ExecuteDbObject<T>(DbCommandWrapped command)
        {
            using (DbDataReader reader = this.ExecuteReader(command))
            {
                if (reader.Read())
                {
                    return reader.ToObject<T>();
                }
                return default(T);
            }
        }

        public T ExecuteDbObject<T>(DbCommand command)
        {
            using (DbDataReader reader = this.ExecuteReader(command))
            {
                if (reader.Read())
                {
                    return reader.ToObject<T>();
                }
                return default(T);
            }
        }

        public T ExecuteDbObject<T>(string commandText)
        {
            return this.ExecuteDbObject<T>(this.CreateDbCommandWrapped(commandText));
        }

        public T ExecuteDbObject<T>(CommandType commandType, string commandText)
        {
            return this.ExecuteDbObject<T>(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public T ExecuteDbObject<T>(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteDbObject<T>(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public T ExecuteDbObject<T>(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteDbObject<T>(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected List<T> ExecuteDbObjectList<T>(DbCommandWrapped command)
        {
            using (command)
            {
                return command.ExecuteDbObjectList<T>();
            }
        }

        public List<T> ExecuteDbObjectList<T>(DbCommand command)
        {
            using (DbDataReader reader = this.ExecuteReader(command))
            {
                return reader.ToObjectList<T>();
            }
        }

        public List<T> ExecuteDbObjectList<T>(string commandText)
        {
            return this.ExecuteDbObjectList<T>(this.CreateDbCommandWrapped(commandText));
        }

        public List<T> ExecuteDbObjectList<T>(CommandType commandType, string commandText)
        {
            return this.ExecuteDbObjectList<T>(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public List<T> ExecuteDbObjectList<T>(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteDbObjectList<T>(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public List<T> ExecuteDbObjectList<T>(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteDbObjectList<T>(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected int ExecuteNonQuery(DbCommandWrapped command)
        {
            using (command)
            {
                DateTime now = DateTime.Now;
                int num = command.ExecuteNonQuery();
                this.ProcessSlowCommandLog(command.Command, now, DateTime.Now);
                return num;
            }
        }

        public int ExecuteNonQuery(DbCommand command)
        {
            int num2;
            bool flag = command.Connection.State == ConnectionState.Closed;
            if (flag)
            {
                this.OpenConnection(command.Connection);
            }
            bool flag2 = false;
            try
            {
                DateTime now = DateTime.Now;
                int num = command.ExecuteNonQuery();
                this.ProcessSlowCommandLog(command, now, DateTime.Now);
                num2 = num;
            }
            catch (Exception exception)
            {
                flag2 = true;
                this.LogCommandError(exception, command);
                throw exception;
            }
            finally
            {
                if (flag || flag2)
                {
                    command.Connection.Close();
                }
            }
            return num2;
        }

        public int ExecuteNonQuery(string commandText)
        {
            return this.ExecuteNonQuery(this.CreateDbCommandWrapped(commandText));
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public int ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteNonQuery(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteNonQuery(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected virtual DbDataReader ExecuteReader(DbCommandWrapped command)
        {
            using (command)
            {
                DateTime now = DateTime.Now;
                DbDataReader reader = command.ExecuteReader();
                this.ProcessSlowCommandLog(command.Command, now, DateTime.Now);
                return reader;
            }
        }

        public DbDataReader ExecuteReader(DbCommand command)
        {
            DbDataReader reader2;
            bool flag = command.Connection.State == ConnectionState.Closed;
            if (flag)
            {
                this.OpenConnection(command.Connection);
            }
            try
            {
                DateTime now = DateTime.Now;
                DbDataReader reader = command.ExecuteReader(flag ? CommandBehavior.CloseConnection : CommandBehavior.Default);
                this.ProcessSlowCommandLog(command, now, DateTime.Now);
                reader2 = reader;
            }
            catch (Exception exception)
            {
                this.LogCommandError(exception, command);
                if (flag)
                {
                    command.Connection.Close();
                }
                throw exception;
            }
            return reader2;
        }

        public DbDataReader ExecuteReader(string commandText)
        {
            return this.ExecuteReader(this.CreateDbCommandWrapped(commandText));
        }

        public DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return this.ExecuteReader(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public DbDataReader ExecuteReader(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteReader(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteReader(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected object ExecuteScalar(DbCommandWrapped command)
        {
            using (command)
            {
                DateTime now = DateTime.Now;
                object obj2 = command.ExecuteScalar();
                this.ProcessSlowCommandLog(command.Command, now, DateTime.Now);
                return obj2;
            }
        }

        public object ExecuteScalar(DbCommand command)
        {
            object obj3;
            bool flag = command.Connection.State == ConnectionState.Closed;
            bool flag2 = false;
            if (flag)
            {
                this.OpenConnection(command.Connection);
            }
            try
            {
                DateTime now = DateTime.Now;
                object obj2 = command.ExecuteScalar();
                this.ProcessSlowCommandLog(command, now, DateTime.Now);
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                flag2 = true;
                this.LogCommandError(exception, command);
                throw exception;
            }
            finally
            {
                if (flag || flag2)
                {
                    command.Connection.Close();
                }
            }
            return obj3;
        }

        public object ExecuteScalar(string commandText)
        {
            return this.ExecuteScalar(this.CreateDbCommandWrapped(commandText));
        }

        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(this.CreateDbCommandWrapped(commandType, commandText));
        }

        public object ExecuteScalar(string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteScalar(this.CreateDbCommandWrapped(commandText, parameters));
        }

        public object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return this.ExecuteScalar(this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        protected void FillDataSet(DataSet dataset, DbCommandWrapped command)
        {
            using (command)
            {
                DateTime now = DateTime.Now;
                using (DbDataAdapter adapter = this.CreateDbDataAdapter(command.Command))
                {
                    command.FillDataSet(adapter, dataset);
                }
                this.ProcessSlowCommandLog(command.Command, now, DateTime.Now);
            }
        }

        public void FillDataSet(DataSet dataset, DbCommand command)
        {
            try
            {
                DateTime now = DateTime.Now;
                using (DbDataAdapter adapter = this.CreateDbDataAdapter(command))
                {
                    adapter.Fill(dataset);
                }
                this.ProcessSlowCommandLog(command, now, DateTime.Now);
            }
            catch (Exception exception)
            {
                this.LogCommandError(exception, command);
                throw exception;
            }
        }

        public void FillDataSet(DataSet dataset, string commandText)
        {
            this.FillDataSet(dataset, this.CreateDbCommandWrapped(commandText));
        }

        public void FillDataSet(DataSet dataset, CommandType commandType, string commandText)
        {
            this.FillDataSet(dataset, this.CreateDbCommandWrapped(commandType, commandText));
        }

        public void FillDataSet(DataSet dataset, string commandText, params DbParameter[] parameters)
        {
            this.FillDataSet(dataset, this.CreateDbCommandWrapped(commandText, parameters));
        }

        public void FillDataSet(DataSet dataset, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            this.FillDataSet(dataset, this.CreateDbCommandWrapped(commandType, commandText, parameters));
        }

        public virtual string GetDbParameterName(string name)
        {
            if (!name.StartsWith("@"))
            {
                return ("@" + name.Replace('.', '_'));
            }
            return name;
        }

        protected virtual object GetDbParameterValue(object value, DbType dbType, int? size = new int?())
        {
            if (value == null)
            {
                return DBNull.Value;
            }
            if (value is DateTime)
            {
                DateTime time = (DateTime)value;
                if (!(time == DateTime.MinValue) && !(time == DateTime.MaxValue))
                {
                    return value.ToString();
                }
                return DBNull.Value;
            }
            if ((size.HasValue && (((dbType == DbType.String) || (dbType == DbType.StringFixedLength)) || ((dbType == DbType.AnsiString) || (dbType == DbType.AnsiStringFixedLength)))) && (value != DBNull.Value))
            {
                string str = value.ToString();
                if ((size.Value > 0) && (str.Length > size.Value))
                {
                    return str.Substring(0, size.Value);
                }
            }
            return value;
        }

        protected virtual DbProviderFactory GetDbProviderFactory()
        {
            if (this.ConnectionDbProviderFactory == null)
            {
                DbConnection connection = this.Connection;
                if (this.DbTransaction != null)
                {
                    connection = this.DbTransaction.Connection;
                }
                this.ConnectionDbProviderFactory = GetDbProviderFactory(connection);
            }
            return this.ConnectionDbProviderFactory;
        }

        public static DbProviderFactory GetDbProviderFactory(DbConnection connection)
        {
            if (typeof(SqlConnection).IsInstanceOfType(connection))
            {
                return SqlClientFactory.Instance;
            }
            if (typeof(OleDbConnection).IsInstanceOfType(connection))
            {
                return OleDbFactory.Instance;
            }
            if (typeof(OdbcConnection).IsInstanceOfType(connection))
            {
                return OdbcFactory.Instance;
            }
            return connection.GetType().GetProperty("DbProviderFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(connection, null).As<DbProviderFactory>();
        }

        private System.Data.Common.DbCommandBuilder GetIdentifierFormatter()
        {
            if (this.IdentifierFormatter == null)
            {
                DbProviderFactory dbProviderFactory = this.GetDbProviderFactory();
                this.IdentifierFormatter = dbProviderFactory.CreateCommandBuilder();
                this.IdentifierFormatter.DataAdapter = dbProviderFactory.CreateDataAdapter();
                this.IdentifierFormatter.DataAdapter.SelectCommand = this.CreateDbCommand();
            }
            DbCommand command = (this.IdentifierFormatter.DataAdapter == null) ? null : this.IdentifierFormatter.DataAdapter.SelectCommand;
            if (((command != null) && (command.Connection != null)) && (typeof(OleDbConnection).IsInstanceOfType(command.Connection) || typeof(OdbcConnection).IsInstanceOfType(command.Connection)))
            {
                this.OpenConnection(command.Connection);
            }
            return this.IdentifierFormatter;
        }

        protected virtual DbConnection GetSlaveConnection(CommandType commandType, string commandText)
        {
            if ((this._DbTransaction != null) || (this.SlaveConnections.Count == 0))
            {
                return null;
            }
            if (this.SlaveConnections.Count == 1)
            {
                return this.SlaveConnections[0];
            }
            int hashCode = 0;
            System.Func<CommandType, string, int> computeCommandTextHashCode = this.ComputeCommandTextHashCode;
            if (computeCommandTextHashCode == null)
            {
                hashCode = commandText.GetHashCode();
            }
            else
            {
                hashCode = computeCommandTextHashCode(commandType, commandText);
            }
            return this.SlaveConnections[Math.Abs(hashCode) % this.SlaveConnections.Count];
        }

        protected virtual bool IsSelectCommandText(CommandType commandType, string commandText)
        {
            return ((commandType == CommandType.TableDirect) || (((commandType == CommandType.Text) && !string.IsNullOrEmpty(commandText)) && commandText.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)));
        }

        protected virtual void LogCommandError(Exception ex, DbCommand command)
        {
            Logger.Error(ex, "Execute query error, ds: {0}, command: {1}", new object[] { command.Connection.DataSource, command.CommandText });
        }

        private void OnDbConnectionReset()
        {
            if (this.IdentifierFormatter != null)
            {
                this.IdentifierFormatter.Dispose();
            }
            this.IdentifierFormatter = null;
            this.ConnectionDbProviderFactory = null;
        }

        public void Open()
        {
            this.OpenConnection(this.Connection);
        }

        internal void OpenConnection(DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                try
                {
                    connection.Open();
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Open db connection error: {0}", new object[] { connection.ConnectionString });
                    throw exception;
                }
            }
        }

        internal void ProcessSlowCommandLog(DbCommand command, DateTime startTime, DateTime endTime)
        {
            int slowCommandExecuteTime = this.SlowCommandExecuteTime;
            if (slowCommandExecuteTime > 1)
            {
                int totalMilliseconds = (int)endTime.Subtract(startTime).TotalMilliseconds;
                if (totalMilliseconds >= slowCommandExecuteTime)
                {
                    StringBuilder builder = new StringBuilder(0x80);
                    builder.AppendFormat("[sql-slow-command] {0}\r\n", command.CommandText);
                    object[] args = new object[] { totalMilliseconds, (((double)totalMilliseconds) / 1000.0).ToString("0.000"), startTime.ToString("yyyy-MM-dd HH:mm:ss:fff"), endTime.ToString("yyyy-MM-dd HH:mm:ss:fff") };
                    builder.AppendFormat("[sql-execute-time] spend time: {0}ms/{1}sec, start: {2}, end: {3}\r\n", args);
                    if (command.Parameters != null)
                    {
                        foreach (DbParameter parameter in command.Parameters)
                        {
                            builder.AppendFormat("[sql-slow-command-param] {0} = {1}\r\n", parameter.ParameterName, parameter.Value);
                        }
                    }
                    Logger.Info(builder.ToString());
                }
            }
        }

        public virtual string QuoteIdentifier(string unquotedIdentifier)
        {
            if (string.IsNullOrEmpty(unquotedIdentifier))
            {
                return string.Empty;
            }
            if (unquotedIdentifier.IndexOfAny(new char[] { '.', '_' }) != -1)
            {
                return unquotedIdentifier;
            }
            return this.GetIdentifierFormatter().QuoteIdentifier(unquotedIdentifier);
        }

        protected void RecycleTransaction()
        {
            if (this.DbTransactionIsCreatedByDbConnection)
            {
                if (this.DbTransactionConnectionIsClosed && !this.KeepConnection)
                {
                    this.CloseConnection(this.DbTransaction.Connection);
                }
                this.DbTransaction.Dispose();
            }
            this.DbTransaction = null;
        }

        public void RollbackTransaction()
        {
            if (this.DbTransaction != null)
            {
                try
                {
                    this.DbTransaction.Rollback();
                }
                catch
                {
                }
                finally
                {
                    this.RecycleTransaction();
                }
            }
        }

        public System.Func<CommandType, string, int> ComputeCommandTextHashCode { get; set; }

        public DbConnection Connection { get; protected set; }

        public System.Data.Common.DbTransaction DbTransaction
        {
            get
            {
                return this._DbTransaction;
            }
            set
            {
                this._DbTransaction = value;
                this.DbTransactionIsCreatedByDbConnection = false;
                this.OnDbConnectionReset();
            }
        }

        public bool DbTransactionIsCreatedByDbConnection { get; private set; }

        public DbCommandExecutor Executor
        {
            get
            {
                if (this._Executor == null)
                {
                    this._Executor = this.CreateDbCommandExecutor();
                }
                return this._Executor;
            }
        }

        public bool IgnoreSlaveConnection
        {
            get
            {
                return _ignoreSlaveConnection;
            }
            set
            {
                _ignoreSlaveConnection = value;
            }
        }

        public bool KeepConnection { get; internal set; }

        public List<DbConnection> SlaveConnections { get; private set; }

        public int SlowCommandExecuteTime { get; set; }
    }
}