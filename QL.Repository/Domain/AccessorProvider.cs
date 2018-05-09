using MySql.Data.MySqlClient;
using QL.Core.ObjectPool;
using QL.Database;
using QL.Database.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Repository
{
    /// <summary>
    /// 基于MySql数据库的访问者驱动提供者
    /// </summary>
    public class AccessorProvider
        : IAccessorProvider
    {
        #region IDbAccessorProvider 成员
        /// <summary>
        /// 返回数据库帮助对象实例
        /// </summary>
        public DbHelper DbHelper
        {

            get
            {

                return ObjectPoolContext.Current.GetOrAdd<DbHelper>("AccessorProvider.DbHelper", () =>
                {
                    string connectionString = "Data Source=localhost;Initial Catalog=test;uid=root;pwd=";;
                    DbHelper dbHelper = null;
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        dbHelper = new MySqlDbHelper(new MySqlConnection(connectionString),false); 

                    }
                    return dbHelper;
                });
            }
        }

        #endregion



        #region IDisposable 成员
        /// <summary>
        /// 释放内存资源
        /// </summary>
        public void Dispose()
        {

        }
        #endregion


    }
}
