using System.Data;

using Microsoft.Practices.Unity;
using QL.Core.Log;

namespace QL.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using QL.Database.MySql;
    using QL.Database;

    /// <summary>
    /// Repository base class
    /// </summary>
    /// <typeparam name="TEntity">The type of underlying entity in this repository</typeparam>
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class,IEntity
    {

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        IAccessorProvider _Provider;

        #region Constructor

        /// <summary>
        /// Create a new instance of repository
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="tableName">表名</param>
        public Repository(IAccessorProvider provider, string tableName)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            TableName = tableName;
            _Provider = provider;
        }

        /// <summary>
        /// <see cref="Microsoft.Samples.NLayerApp.Domain.Seedwork{TValueObject}"/>
        /// </summary>
        public IAccessorProvider Provider
        {
            get
            {
                return _Provider;
            }
        }
        #endregion

        #region IRepository Members


        /// <summary>
        /// 插入记录
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool Insert(TEntity item)
        {
            return this.Provider.DbHelper.Executor.Insert(TableName, item);
        }

        /// <summary>
        /// 插入记录后，返回标识
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        public virtual bool InsertAndReturn(TEntity item)
        {
            bool res = this.Provider.DbHelper.Executor.Insert(TableName, item);
            if (res)
            {
                string sql = "SELECT @@IDENTITY";
                object ob = this.Provider.DbHelper.ExecuteScalar(sql);
                item.Id = Convert.ToInt32(ob);
            }
            return res;
        }

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual bool BatchInsert(List<TEntity> list)
        {
            bool res = false;
            try
            {
                this.Provider.DbHelper.BeginTransaction();
                foreach (var item in list)
                {
                    this.Provider.DbHelper.Executor.Insert(TableName, item);
                }
                this.Provider.DbHelper.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "批量插入数据出错");
                this.Provider.DbHelper.RollbackTransaction();
                return false;
            }
        }

        /// <summary>
        /// 更新某个记录
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual bool Update(TEntity item)
        {
            return this.Provider.DbHelper.Executor.Update(TableName, item);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public virtual bool BatchUpdate(List<TEntity> list)
        {
            try
            {
                this.Provider.DbHelper.BeginTransaction();
                foreach (var item in list)
                {
                    this.Provider.DbHelper.Executor.Update(TableName, item);
                }
                this.Provider.DbHelper.CommitTransaction();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "批量更新数据出错");
                this.Provider.DbHelper.RollbackTransaction();
                return false;
            }
        }

        /// <summary>
        /// 将新实体对象进行存储.
        /// </summary>
        /// <param name="entity">需要保存的实体对象.</param>
        /// <returns>返回已经保存的实体对象.</returns>
        public virtual bool Save(TEntity entity)
        {
            bool flag = false;
            if (entity.Id > 0)
            {
                flag = Update(entity);
            }
            else
            {
                flag = Insert(entity);
            }
            return flag;
        }

        /// <summary>
        /// 通过id获取某记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity GetById(int id)
        {
            if (id < 1) return null;
            var condition = this.Provider.DbHelper.CreateDBConditionBuilder();
            condition.AddCriteria("id", id);
            return this.Provider.DbHelper.Executor.GetBy<TEntity>(TableName, condition);
        }

        /// <summary>
        /// 判断是否已存在数据
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        public virtual bool Exists(int id)
        {
            var condition = this.Provider.DbHelper.CreateDBConditionBuilder();
            condition.AddCriteria("id", id);

            return this.Provider.DbHelper.Executor.Exists(TableName, condition);
        }

        /// <summary>
        /// 删除某些记录,并返回成功删除的数量
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual int Delete(params int[] ids)
        {
            var condition = this.Provider.DbHelper.CreateDBConditionBuilder();
            condition.AddInCriteria<int>("id", ids);
            return this.Provider.DbHelper.Executor.Delete(TableName, condition);
        }


        /// <summary>
        /// 获取一定数量的数据
        /// </summary>
        /// <param name="condition">搜索条件</param>
        /// <param name="quantity">获取的数量, 如果值为0则获取所有符合条件的数据</param>
        /// <returns></returns>
        public virtual List<TEntity> SelectBy(DbConditionBuilder condition, int quantity)
        {
            if (quantity > 0)
            {
                return this.Provider.DbHelper.Executor.Select<TEntity>(quantity, TableName, condition);
            }
            else
            {
                return this.Provider.DbHelper.Executor.Select<TEntity>(TableName, condition);
            }
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="condition">搜索条件</param>
        /// <param name="page">显示的页码</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">页码总数</param>
        /// <param name="recordCount">数据总数</param>
        /// <returns></returns>
        public virtual List<TEntity> SelectBy(DbConditionBuilder condition, ref int page, int pageSize, out int pageCount, out int recordCount)
        {
            MySqlDataPagingReader reader = new MySqlDataPagingReader((MySqlDbHelper)this.Provider.DbHelper);

            reader.Select = string.Format("SELECT * FROM {0}", this.Provider.DbHelper.QuoteIdentifier(TableName));
            reader.Condition = condition;

            reader.PageNumber = page;
            reader.PageSize = pageSize;

            var entities = reader.ReadAsDbObjectList<TEntity>();

            page = reader.PageNumber;
            pageCount = reader.PageCount;
            recordCount = reader.RecordCount;

            return entities;
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="condition">搜索条件</param>
        /// <param name="page">显示的页码</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">页码总数</param>
        /// <param name="recordCount">数据总数</param>
        /// <returns></returns>
        public virtual List<T> SelectBy<T>(DbConditionBuilder condition, ref int page, int pageSize, out int pageCount, out int recordCount)
        {
            MySqlDataPagingReader reader = new MySqlDataPagingReader((MySqlDbHelper)this.Provider.DbHelper);

            reader.Select = string.Format("SELECT * FROM {0}", this.Provider.DbHelper.QuoteIdentifier(TableName));
            reader.Condition = condition;

            reader.PageNumber = page;
            reader.PageSize = pageSize;

            var entities = reader.ReadAsDbObjectList<T>();

            page = reader.PageNumber;
            pageCount = reader.PageCount;
            recordCount = reader.RecordCount;

            return entities;
        }

        /// <summary>
        /// 设置当前实体所对应的数据库表
        /// </summary>
        /// <param name="tableName"></param>
        public void SetTableName(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// 多表联合查询
        /// </summary>
        /// <param name="selColumn">查询列（有别名需带别名）</param>
        /// <param name="selTableInner">>查询表（有别名需带别名）</param>
        /// <param name="condition">条件</param>
        /// <param name="selSort">>排序列（有别名需带别名，需加ASC、DESC）</param>
        /// <param name="quantity">查询记录条数</param>
        /// <returns>DataSet</returns>
        public virtual DataSet GetDataListByMoreTable(string selColumn, string selTableInner, DbConditionBuilder condition, string selSort, int quantity)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("SELECT {0} FROM {1}", selColumn, selTableInner);
            if (condition == null)
            {
                condition = this.Provider.DbHelper.CreateDBConditionBuilder();
            }

            builder.Append(condition);

            if (selSort != null && selSort.Length > 0)
            {
                builder.AppendFormat(" ORDER BY {0} ", selSort);
            }
            if (quantity > 0)
            {
                builder.AppendFormat(" limit 0,{0} ", quantity);
            }
            builder.Append(";");
            return this.Provider.DbHelper.ExecuteDataSet(builder.ToString(), condition.Parameters);
        }

        /// <summary>
        /// 查询总数
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public virtual int GetDataCount(DbConditionBuilder condition)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendFormat("SELECT COUNT(1) FROM {0}", TableName);
            if (condition == null)
            {
                condition = this.Provider.DbHelper.CreateDBConditionBuilder();
                builder.Append(condition);
            }
            builder.Append(condition);
            builder.Append(";");
            int count = Convert.ToInt32(this.Provider.DbHelper.ExecuteScalar(builder.ToString(), condition.Parameters));
            return count;

        }
        #endregion

        #region IDisposable Members


        public void Dispose()
        {
            if (_Provider != null)
                _Provider.Dispose();
        }

        #endregion

    }
}
