namespace QL.Database
{
    using QL.Core.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class DbConditionBuilder
    {
        private StringBuilder Builder;
        private int NeedWriteLeftParentheses;
        private StringBuilder OrderByBuilder;
        private List<DbParameter> Parames;
        private int ParenthesesCount;
        private int WritedLeftParentheses;

        internal DbConditionBuilder(QL.Database.DbHelper dbHelper)
        {
            this.DbHelper = dbHelper;
            this.Builder = new StringBuilder();
            this.Parames = new List<DbParameter>();
            this.OrderByBuilder = new StringBuilder();
        }

        public void Add(string condition)
        {
            this.Add(condition, "AND");
        }

        public void Add(string condition, string relation)
        {
            if (string.IsNullOrEmpty(condition))
            {
                throw new ArgumentNullException("condition");
            }
            bool flag = this.WriteLeftParentheses(relation);
            if ((this.Builder.Length != 0) && !flag)
            {
                this.Builder.AppendFormat(" {0} ", relation);
            }
            this.Builder.Append(condition);
        }

        public void Add(string condition, params DbParameter[] pars)
        {
            this.Add(condition, "AND", pars);
        }

        public void Add(string condition, string relation, params DbParameter[] pars)
        {
            this.Add(condition, relation);
            this.Parames.AddRange(pars);
        }

        public void AddCompareCriteria<T>(string name, DbConditionCompare compare, T value) where T : struct
        {
            this.AddCompareCriteria<T>(name, compare, value, "AND");
        }

        public void AddCompareCriteria<T>(string name, DbConditionCompare compare, T? value) where T : struct
        {
            this.AddCompareCriteria<T>(name, compare, value, "AND");
        }

        public void AddCompareCriteria(string name, DbConditionCompare compare, string value)
        {
            this.AddCompareCriteria(name, compare, value, 0x7fffffff, "AND");
        }

        public void AddCompareCriteria<T>(string name, DbConditionCompare compare, T value, string relation) where T : struct
        {
            string str = this.GetCompareCondition(name, compare, relation);
            if (!string.IsNullOrEmpty(str) && !value.Equals(default(T)))
            {
                string str2 = this.DbHelper.CreateSeqParameterName();
                this.Add(string.Format("{0}{1}{2}", this.DbHelper.QuoteIdentifier(name), str, this.DbHelper.GetDbParameterName(str2)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str2, typeof(T).GetDbType(), value) });
            }
        }

        public void AddCompareCriteria<T>(string name, DbConditionCompare compare, T? value, string relation) where T : struct
        {
            string str = this.GetCompareCondition(name, compare, relation);
            if (!string.IsNullOrEmpty(str) && value.HasValue)
            {
                string str2 = this.DbHelper.CreateSeqParameterName();
                this.Add(string.Format("{0}{1}{2}", this.DbHelper.QuoteIdentifier(name), str, this.DbHelper.GetDbParameterName(str2)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str2, typeof(T).GetDbType(), value.Value) });
            }
        }

        public void AddCompareCriteria(string name, DbConditionCompare compare, string value, int length)
        {
            this.AddCompareCriteria(name, compare, value, length, "AND");
        }

        public void AddCompareCriteria(string name, DbConditionCompare compare, string value, int length, string relation)
        {
            string str = this.GetCompareCondition(name, compare, relation);
            if (!string.IsNullOrEmpty(str) && (value != null))
            {
                string str2 = this.DbHelper.CreateSeqParameterName();
                this.Add(string.Format("{0}{1}{2}", this.DbHelper.QuoteIdentifier(name), str, this.DbHelper.GetDbParameterName(str2)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str2, DbType.String, length, value) });
            }
        }

        public void AddCriteria(string name, string value)
        {
            this.AddCriteria(name, value, false, "AND");
        }

        public void AddCriteria<T>(string name, T value) where T : struct
        {
            this.AddCriteria<T>(name, value, "AND");
        }

        public void AddCriteria<T>(string name, T? value) where T : struct
        {
            this.AddCriteria<T>(name, value, "AND");
        }

        public void AddCriteria<T>(string name, T? value, string relation) where T : struct
        {
            if (!string.IsNullOrEmpty(name) && value.HasValue)
            {
                string str = this.DbHelper.CreateSeqParameterName();
                this.Add(string.Format("{0}={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str, value.GetType().GetDbType(), value.Value) });
            }
        }

        public void AddCriteria(string name, string value, bool fuzzy)
        {
            this.AddCriteria(name, value, fuzzy, "AND");
        }

        public void AddCriteria(string name, string value, int length)
        {
            this.AddCriteria(name, value, length, false, "AND");
        }

        public void AddCriteria(string name, string value, string relation)
        {
            this.AddCriteria(name, value, false, relation);
        }

        public void AddCriteria<T>(string name, T value, string relation) where T : struct
        {
            if (!string.IsNullOrEmpty(name) && !value.Equals(default(T)))
            {
                string str = this.DbHelper.CreateSeqParameterName();
                this.Add(string.Format("{0}={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str, value.GetType().GetDbType(), value) });
            }
        }

        public void AddCriteria(string name, string value, bool fuzzy, string relation)
        {
            this.AddCriteria(name, value, 0x7fffffff, fuzzy, relation);
        }

        public void AddCriteria(string name, string value, int length, bool fuzzy)
        {
            this.AddCriteria(name, value, length, fuzzy, "AND");
        }

        public void AddCriteria(string name, string value, int length, bool fuzzy, string relation)
        {
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(name))
            {
                string str = this.DbHelper.CreateSeqParameterName();
                if (fuzzy)
                {
                    this.Add(string.Format("{0} LIKE {1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str, DbType.String, length, "%" + value + "%") });
                }
                else
                {
                    this.Add(string.Format("{0}={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str, DbType.String, length, value) });
                }
            }
        }

        public void AddFormat(string condition, params object[] values)
        {
            this.AddFormat(condition, "AND", values);
        }

        public void AddFormat(string format, string value)
        {
            this.Add(string.Format(format, value), "AND");
        }

        public void AddFormat(string condition, string relation, params object[] values)
        {
            this.Add(string.Format(condition, values), relation);
        }

        public void AddInCriteria<T>(string name, IEnumerable<T> values)
        {
            if (!string.IsNullOrEmpty(name) && (values != null))
            {
                List<string> list = new List<string>();
                DbType dbType = typeof(T).GetDbType();
                foreach (T local in values)
                {
                    string str = this.DbHelper.CreateSeqParameterName();
                    list.Add(this.DbHelper.GetDbParameterName(str));
                    this.Parames.Add(this.DbHelper.CreateDbParameter(str, dbType, local));
                }
                if (list.Count > 0)
                {
                    this.Add(string.Format("{0} IN ({1})", this.DbHelper.QuoteIdentifier(name), string.Join(",", list)));
                }
            }
        }

        public void AddNotInCriteria<T>(string name, IEnumerable<T> values)
        {
            if (!string.IsNullOrEmpty(name) && (values != null))
            {
                List<string> list = new List<string>();
                DbType dbType = typeof(T).GetDbType();
                foreach (T local in values)
                {
                    string str = this.DbHelper.CreateSeqParameterName();
                    list.Add(this.DbHelper.GetDbParameterName(str));
                    this.Parames.Add(this.DbHelper.CreateDbParameter(str, dbType, local));
                }
                if (list.Count > 0)
                {
                    this.Add(string.Format("NOT {0} IN ({1})", this.DbHelper.QuoteIdentifier(name), string.Join(",", list)));
                }
            }
        }

        public void AddOrderBy(string name, bool desc)
        {
            if (this.OrderByBuilder.Length > 0)
            {
                this.OrderByBuilder.Append(",");
            }
            this.OrderByBuilder.Append(this.DbHelper.QuoteIdentifier(name));
            this.OrderByBuilder.Append(desc ? " DESC" : " ASC");
        }

        public void AddRangeCriteria(string name, DateTimeRange range)
        {
            if (range != null)
            {
                this.AddRangeCriteria<DateTime>(name, range.Start, range.End, "AND");
            }
        }

        public void AddRangeCriteria<T>(string name, ValueRange<T> range) where T : struct
        {
            if (range != null)
            {
                this.AddRangeCriteria<T>(name, range.Start, range.End, "AND");
            }
        }

        public void AddRangeCriteria(string name, DateTimeRange range, string relation)
        {
            if (range != null)
            {
                this.AddRangeCriteria<DateTime>(name, range.Start, range.End, relation);
            }
        }

        public void AddRangeCriteria<T>(string name, ValueRange<T> range, string relation) where T : struct
        {
            if (range != null)
            {
                this.AddRangeCriteria<T>(name, range.Start, range.End, relation);
            }
        }

        public void AddRangeCriteria<T>(string name, T? startValue, T? endValue) where T : struct
        {
            this.AddRangeCriteria<T>(name, startValue, endValue, "AND");
        }

        public void AddRangeCriteria<T>(string name, T? startValue, T? endValue, string relation) where T : struct
        {
            if (startValue.HasValue && endValue.HasValue)
            {
                string str = this.DbHelper.CreateSeqParameterName();
                string str2 = this.DbHelper.CreateSeqParameterName();
                DbType dbType = typeof(T).GetDbType();
                this.Add(string.Format("{0} BETWEEN {1} AND {2}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str), this.DbHelper.GetDbParameterName(str2)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str, dbType, startValue), this.DbHelper.CreateDbParameter(str2, dbType, endValue) });
            }
            else if (startValue.HasValue)
            {
                string str3 = this.DbHelper.CreateSeqParameterName();
                DbType type2 = typeof(T).GetDbType();
                this.Add(string.Format("{0}>={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str3)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str3, type2, startValue) });
            }
            else if (endValue.HasValue)
            {
                string str4 = this.DbHelper.CreateSeqParameterName();
                DbType type3 = typeof(T).GetDbType();
                this.Add(string.Format("{0}<={1}", this.DbHelper.QuoteIdentifier(name), this.DbHelper.GetDbParameterName(str4)), relation, new DbParameter[] { this.DbHelper.CreateDbParameter(str4, type3, endValue) });
            }
        }

        public void BeginParentheses()
        {
            this.NeedWriteLeftParentheses++;
            this.ParenthesesCount++;
        }

        public void Clear()
        {
            this.Builder.Length = 0;
            this.Parames.Clear();
            this.ClearParentheses();
        }

        private void ClearParentheses()
        {
            this.ParenthesesCount = 0;
            this.NeedWriteLeftParentheses = 0;
            this.WritedLeftParentheses = 0;
        }

        public void EndParentheses()
        {
            if (this.ParenthesesCount > 0)
            {
                if (this.WritedLeftParentheses > 0)
                {
                    this.Builder.Append(")");
                    this.WritedLeftParentheses--;
                }
                this.ParenthesesCount--;
            }
            this.NeedWriteLeftParentheses = Math.Min(this.ParenthesesCount, this.NeedWriteLeftParentheses);
        }

        private string GetCompareCondition(string name, DbConditionCompare compare, string relation)
        {
            switch (compare)
            {
                case DbConditionCompare.LT:
                    return "<";

                case DbConditionCompare.LTOrEqual:
                    return "<=";

                case DbConditionCompare.GT:
                    return ">";

                case DbConditionCompare.GTOrEqual:
                    return ">=";

                case DbConditionCompare.Equal:
                    return "=";

                case DbConditionCompare.Unequal:
                    return "!=";

                case DbConditionCompare.IsNull:
                    this.Add(string.Format("{0} IS NULL", this.DbHelper.QuoteIdentifier(name)), relation);
                    return string.Empty;

                case DbConditionCompare.IsNotNull:
                    this.Add(string.Format("{0} IS NOT NULL", this.DbHelper.QuoteIdentifier(name)), relation);
                    return string.Empty;
            }
            return string.Empty;
        }

        public void SetOrderBy(string orderBy)
        {
            this.OrderByBuilder.Length = 0;
            this.OrderByBuilder.Append(orderBy);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.HasCondition)
            {
                builder.Append(" WHERE ").Append(this.Condition);
            }
            if (this.OrderByBuilder.Length > 0)
            {
                builder.Append(" ORDER BY ").Append(this.OrderByBuilder.ToString());
            }
            return builder.ToString();
        }

        private bool WriteLeftParentheses(string relation)
        {
            bool flag = this.NeedWriteLeftParentheses > 0;
            if (flag)
            {
                if (this.HasCondition)
                {
                    if (!string.IsNullOrEmpty(relation))
                    {
                        this.Builder.Append(" ");
                        this.Builder.Append(relation);
                    }
                    this.Builder.Append(" ");
                }
                while (this.NeedWriteLeftParentheses > 0)
                {
                    this.Builder.Append("(");
                    this.NeedWriteLeftParentheses--;
                    this.WritedLeftParentheses++;
                }
            }
            return flag;
        }

        public string Condition
        {
            get
            {
                if (!this.HasCondition)
                {
                    return string.Empty;
                }
                while (this.ParenthesesCount > 0)
                {
                    this.EndParentheses();
                }
                return this.Builder.ToString();
            }
        }

        public QL.Database.DbHelper DbHelper { get; private set; }

        public bool HasCondition
        {
            get
            {
                return (this.Builder.Length > 0);
            }
        }

        public bool HasOrderBy
        {
            get
            {
                return (this.OrderByBuilder.Length > 0);
            }
        }

        public bool IsEmpty
        {
            get
            {
                return ((this.Builder.Length == 0) && (this.OrderByBuilder.Length == 0));
            }
        }

        public string OrderBy
        {
            get
            {
                if (this.HasOrderBy)
                {
                    return this.OrderByBuilder.ToString();
                }
                return string.Empty;
            }
        }

        public DbParameter[] Parameters
        {
            get
            {
                return this.Parames.ToArray();
            }
        }
    }
}
