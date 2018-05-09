/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DbAccessorProviderFactory
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using QL.Core;

namespace QL.Database
{
    /// <summary>
    /// 数据访问者驱动的工厂
    /// </summary>
    public sealed class DbAccessorProviderFactory
    {
        /// <summary>
        /// 返回数据访问者的实例
        /// </summary>
        /// <param name="accessorInstance">"类名,程序集",例如: "QL.Project.OleDbDataAccessor.AccessorProvider,QL.Project"</param>
        /// <returns></returns>
        public static IDbAccessorProvider CreateInstance(string accessorInstance)
        {
            //从程序集反射生成实例
            if (string.IsNullOrEmpty(accessorInstance)) throw new ArgumentNullException("accessorInstance", "还未配置数据访问者驱动实例");
            
            IDbAccessorProvider provider;
            string[] k = accessorInstance.Trim().Split(new char[] { ',' }, 2);
            if (k.Length == 1)
            {
                //只配置了类名
                provider = Activator.CreateInstance(null, k[0]).Unwrap() as IDbAccessorProvider;
            }
            else
            {
                //分解程序集和实例类
                if (k.Length != 2) throw new ArgumentException("accessorInstance", "accessorInstance配置错误");

                k[0] = k[0].Trim();
                k[1] = k[1].Trim();
                //动态装载生成实例
                if (Utility.IsAbsolutePath(k[1]))
                {
                    //采用的是程序集文件
                    provider = Activator.CreateInstanceFrom(k[1], k[0]).Unwrap() as IDbAccessorProvider;
                }
                else
                {
                    provider = Activator.CreateInstance(k[1], k[0]).Unwrap() as IDbAccessorProvider;
                }
            }
            if (provider == null) throw new ArgumentNullException("accessorInstance", "accessorInstance配置错误,非IDbAccessorProvider接口实例");

            return provider;
        }
    }
}
