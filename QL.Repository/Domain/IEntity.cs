using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntity
    {
        int Id { get; set; }
    }

    /// <summary>
    /// 可指定实体主键类型的接口.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey> : IEntity
    {
        /// <summary>
        /// 表示为该实体的主键.
        /// </summary>
        new TKey Id { get; set; }
    }

}
