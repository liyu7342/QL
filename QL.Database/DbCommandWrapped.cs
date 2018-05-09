namespace QL.Database
{
    using QL.Core.Log;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;

    public class DbCommandWrapped : IDisposable
    {
        internal DbCommandWrapped(DbCommand command, bool keepConnection)
        {
            this.Command = command;
            this.KeepConnection = keepConnection;
            this.ConnectionIsOpened = command.Connection.State != ConnectionState.Closed;
        }

        protected void BeginExecute()
        {
            if (this.Command.Connection.State == ConnectionState.Closed)
            {
                this.ConnectionIsOpened = false;
                this.Command.Connection.Open();
            }
        }

        public static DbCommandWrapped Create(DbCommand command, bool keepConnection)
        {
            return new DbCommandWrapped(command, keepConnection);
        }

        public static DbCommandWrapped Create(DbConnection connection, string commandText)
        {
            return Create(connection, false, CommandType.Text, commandText, null);
        }

        public static DbCommandWrapped Create(DbConnection connection, bool keepConnection, string commandText)
        {
            return Create(connection, keepConnection, CommandType.Text, commandText, null);
        }

        public static DbCommandWrapped Create(DbConnection connection, string commandText, params DbParameter[] parameters)
        {
            return Create(connection, false, CommandType.Text, commandText, parameters);
        }

        public static DbCommandWrapped Create(DbConnection connection, bool keepConnection, string commandText, params DbParameter[] parameters)
        {
            return Create(connection, keepConnection, CommandType.Text, commandText, parameters);
        }

        public static DbCommandWrapped Create(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            return Create(connection, false, commandType, commandText, parameters);
        }

        public static DbCommandWrapped Create(DbConnection connection, bool keepConnection, CommandType commandType, string commandText, params DbParameter[] parameters)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = commandText;
            if (connection.ConnectionTimeout > command.CommandTimeout)
            {
                command.CommandTimeout = connection.ConnectionTimeout;
            }
            if (parameters != null)
            {
                foreach (DbParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        command.Parameters.Add(parameter);
                    }
                }
            }
            return new DbCommandWrapped(command, keepConnection);
        }

        public void Dispose()
        {
            if (this.Command != null)
            {
                this.Command.Parameters.Clear();
                this.Command.Dispose();
                this.Command = null;
            }
        }

        private void EndExecute(bool revert, bool error)
        {
            if (revert && (error || (!this.ConnectionIsOpened && !this.KeepConnection)))
            {
                try
                {
                    if (this.Command.Connection.State != ConnectionState.Closed)
                    {
                        this.Command.Connection.Close();
                    }
                }
                catch (DbException)
                {
                }
            }
        }

        public List<T> ExecuteDbObjectList<T>()
        {
            using (DbDataReader reader = this.ExecuteReader())
            {
                return reader.ToObjectList<T>();
            }
        }

        public List<T> ExecuteDbObjectList<T>(int startIndex, int count)
        {
            int num = 0;
            int num2 = 0;
            using (DbDataReader reader = this.ExecuteReader())
            {
                List<T> list = new List<T>(count);
                while ((num < count) && reader.Read())
                {
                    if (num2 >= startIndex)
                    {
                        list.Add(reader.ToObject<T>());
                        num++;
                    }
                    num2++;
                }
                return list;
            }
        }

        public int ExecuteNonQuery()
        {
            int num2;
            bool error = false;
            try
            {
                this.BeginExecute();
                num2 = this.Command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                error = true;
                Logger.Error(exception, "Execute query error: {0}", new object[] { this.Command.CommandText });
                throw exception;
            }
            finally
            {
                this.EndExecute(true, error);
            }
            return num2;
        }

        public DbDataReader ExecuteReader()
        {
            DbDataReader reader2;
            try
            {
                this.BeginExecute();
                CommandBehavior closeConnection = CommandBehavior.Default;
                if (!this.ConnectionIsOpened && !this.KeepConnection)
                {
                    closeConnection = CommandBehavior.CloseConnection;
                }
                reader2 = this.Command.ExecuteReader(closeConnection);
            }
            catch (Exception exception)
            {
                this.EndExecute(true, true);
                Logger.Error(exception, "Execute query error: {0}", new object[] { this.Command.CommandText });
                throw exception;
            }
            finally
            {
                this.EndExecute(false, false);
            }
            return reader2;
        }

        public object ExecuteScalar()
        {
            object obj3;
            bool error = false;
            try
            {
                this.BeginExecute();
                obj3 = this.Command.ExecuteScalar();
            }
            catch (Exception exception)
            {
                error = true;
                Logger.Error(exception, "Execute query error: {0}", new object[] { this.Command.CommandText });
                throw exception;
            }
            finally
            {
                this.EndExecute(true, error);
            }
            return obj3;
        }

        public void FillDataSet(DbDataAdapter adapter, DataSet ds)
        {
            bool error = false;
            try
            {
                this.BeginExecute();
                adapter.SelectCommand = this.Command;
                adapter.Fill(ds);
            }
            catch (Exception exception)
            {
                error = true;
                Logger.Error(exception, "Execute query error: {0}", new object[] { this.Command.CommandText });
                throw exception;
            }
            finally
            {
                this.EndExecute(true, error);
            }
        }

        public DbCommand Command { get; private set; }

        public bool ConnectionIsOpened { get; private set; }

        public bool KeepConnection { get; private set; }
    }
}
