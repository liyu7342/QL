namespace QL.Database
{
    using QL.Core.Data;
    using QL.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    public class DbDataPagingReader
    {
        private DbConditionBuilder _Condition;
        private static Regex _groupByRegex = new Regex(@"\bGROUP\s+BY\b", RegexOptions.IgnoreCase);
        private int _PageNumber;
        private int _RecordCount;
        private bool KeepConnection;

        public DbDataPagingReader(QL.Database.DbHelper dbHelper)
        {
            this.DbHelper = dbHelper;
            this.PageNumber = 1;
            this.PageSize = 10;
            this.RecordCount = 0;
            this.KeepConnection = false;
        }

        public DbDataPagingReader(DbConnection connection)
            : this(new QL.Database.DbHelper(connection))
        {
        }

        protected virtual void BeginExecute()
        {
            if (string.IsNullOrEmpty(this.Select))
            {
                throw new ArgumentNullException("Select", "无法获取空的数据，请先设置Select属性");
            }
            this.KeepConnection = this.DbHelper.KeepConnection;
            this.DbHelper.KeepConnection = true;
        }

        private string CreateSqlCountCommand()
        {
            StringBuilder builder = new StringBuilder(this.Select.Length + 40);
            if (this.Select.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && !_groupByRegex.IsMatch(this.Condition.ToString()))
            {
                Match match = Regex.Match(this.Select, @"\bFROM\b", RegexOptions.IgnoreCase);
                int startIndex = 6;
                int index = -1;
                while (match.Success)
                {
                    index = match.Index;
                    if (!Regex.IsMatch(this.Select.Substring(startIndex, match.Index - startIndex), @"\bSELECT\b", RegexOptions.IgnoreCase))
                    {
                        break;
                    }
                    startIndex = (match.Index + match.Length) + 1;
                    match = match.NextMatch();
                }
                if (index != -1)
                {
                    builder.Append("SELECT COUNT(*) ");
                    builder.Append(this.Select.Substring(index));
                    if (this.Condition.HasCondition)
                    {
                        builder.Append(" WHERE ").Append(this.Condition.Condition);
                    }
                }
            }
            if (builder.Length == 0)
            {
                builder.Append("SELECT COUNT(*) FROM (");
                builder.Append(this.Select);
                if (this.Condition.HasCondition)
                {
                    builder.Append(" WHERE ").Append(this.Condition.Condition);
                }
                builder.Append(")").Append(" AS ").Append("_tb1");
            }
            return builder.ToString();
        }

        protected virtual void EndExecute()
        {
            if (this.KeepConnection != this.DbHelper.KeepConnection)
            {
                this.DbHelper.KeepConnection = this.KeepConnection;
                if (!this.KeepConnection)
                {
                    this.DbHelper.Close();
                }
            }
        }

        protected virtual void InitPageCount()
        {
            using (DbCommandWrapped wrapped = this.DbHelper.CreateDbCommandWrapped(this.CreateSqlCountCommand(), this.Condition.Parameters))
            {
                this.RecordCount = wrapped.ExecuteScalar().As<int>();
            }
        }

        public virtual DataTable ReadAsDataTable()
        {
            return this.ReadAsDbObjectList<DbObject>().ToDataTable();
        }

        public virtual List<T> ReadAsDbObjectList<T>()
        {
            List<T> list;
            try
            {
                this.BeginExecute();
                this.InitPageCount();
                int startIndex = (this.PageNumber - 1) * this.PageSize;
                if (this.RecordCount > 0)
                {
                    StringBuilder builder = new StringBuilder(this.Select.Length + 40);
                    builder.Append(this.Select);
                    builder.Append(this.Condition.ToString());
                    using (DbCommandWrapped wrapped = this.DbHelper.CreateDbCommandWrapped(builder.ToString(), this.Condition.Parameters))
                    {
                        return wrapped.ExecuteDbObjectList<T>(startIndex, this.PageSize);
                    }
                }
                list = new List<T>();
            }
            finally
            {
                this.EndExecute();
            }
            return list;
        }

        public DbConditionBuilder Condition
        {
            get
            {
                if (this._Condition == null)
                {
                    this._Condition = new DbConditionBuilder(this.DbHelper);
                }
                return this._Condition;
            }
            set
            {
                if (this._Condition != null)
                {
                    this._Condition.Clear();
                }
                this._Condition = value;
            }
        }

        public QL.Database.DbHelper DbHelper { get; private set; }

        public int PageCount
        {
            get
            {
                if (this.RecordCount < 1)
                {
                    return 1;
                }
                return (int)Math.Ceiling((double)(((double)this.RecordCount) / ((double)this.PageSize)));
            }
        }

        public int PageNumber
        {
            get
            {
                return this._PageNumber;
            }
            set
            {
                this._PageNumber = Math.Max(1, value);
            }
        }

        public int PageSize { get; set; }

        public int RecordCount
        {
            get
            {
                return this._RecordCount;
            }
            protected set
            {
                this._RecordCount = value;
                this.PageNumber = Math.Min(this.PageNumber, this.PageCount);
            }
        }

        public string Select { get; set; }
    }
}
