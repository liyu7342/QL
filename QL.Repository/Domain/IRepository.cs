using QL.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Repository
{


    /// <summary>
    /// Repository模式充当业务实体的内存集合或仓储，完全将底层数据基础设施抽象出来
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> : IDisposable
        where TEntity : class
    {

        /// <summary>
        /// 添加新的记录
        /// </summary>
        /// <param name="item"></param>
        bool Insert(TEntity item);

        /// <summary>
        /// 插入记录后，返回标识
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dbHelper"></param>
        /// <returns></returns>
        bool InsertAndReturn(TEntity item);

        /// <summary>
        /// 批量插入
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool BatchInsert(List<TEntity> list);

        /// <summary>
        /// 更新某个记录
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Update(TEntity item);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool BatchUpdate(List<TEntity> list);

        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="entity">需要保存的实体对象</param>
        bool Save(TEntity entity);

        /// <summary>
        /// 通过id获取某记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        TEntity GetById(int id);

        /// <summary>
        /// 判断是否已存在数据
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        bool Exists(int id);

        /// <summary>
        /// 删除某些记录,并返回成功删除的数量
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        int Delete(params int[] ids);

        ///// <summary>
        ///// 删除某些记录,并返回成功删除的数量
        ///// </summary>
        ///// <param name="item">要删除的实体</param
        ///// <returns></returns>
        //int Delete(TEntity item);

        /// <summary>
        /// 获取一定数量的数据
        /// </summary>
        /// <param name="condition">搜索条件</param>
        /// <param name="quantity">获取的数量, 如果值为0则获取所有符合条件的数据</param>
        /// <returns></returns>
        List<TEntity> SelectBy(DbConditionBuilder condition, int quantity);

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="condition">搜索条件</param>
        /// <param name="page">显示的页码</param>
        /// <param name="pageSize">页码大小</param>
        /// <param name="pageCount">页码总数</param>
        /// <param name="recordCount">数据总数</param>
        /// <returns></returns>
        List<TEntity> SelectBy(DbConditionBuilder condition, ref int page, int pageSize, out int pageCount, out int recordCount);


        /// <summary>
        /// 设置访问表名
        /// </summary>
        /// <param name="tableName"></param>
        void SetTableName(string tableName);


        /// <summary>
        /// 多表联合查询
        /// </summary>
        /// <param name="selColumn">查询列（有别名需带别名）</param>
        /// <param name="selTableInner">>查询表（有别名需带别名）</param>
        /// <param name="condition">条件</param>
        /// <param name="selSort">>排序列（有别名需带别名，需加ASC、DESC）</param>
        /// <param name="quantity">查询记录条数</param>
        /// <returns>DataSet</returns>
        DataSet GetDataListByMoreTable(string selColumn, string selTableInner, DbConditionBuilder condition, string selSort, int quantity);


        /// <summary>
        /// 查询总数
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        int GetDataCount(DbConditionBuilder condition);
    }
}
