using QL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Attributes;

namespace QL.Services
{
    /// <summary>
    /// 应用服务基类
    /// TODO：本来应用服务层不应用出现IAccessorProvider的，但是由于新闻资讯网站的业务不算复杂（把DomainServcies这一层砍掉了），
    /// TODO：所以大部分业务处理都集中于Mongcent.News.Services，业务的处理涉及到事务，所以引入了IAccessorProvider
    /// 去除IAccessorProvider，由IunitOfWork 提供事务。
    /// </summary>
    public abstract class AppServiceBase
    {
        /// <summary>
        /// 数据库访问驱动，提供了事务处理的功能
        /// </summary>
        //[Dependency]
        //public virtual IAccessorProvider AccessorProvider { get; set; }

        /// <summary>
        /// 工作单元提交（由于框架是居于KT或者QL的，所以暂时不能充分的利用UnitOfWork模式）
        /// </summary>
        [Dependency]
        public IUnitOfWork UnitOfWork { get; set; }
    }
}
