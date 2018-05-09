using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Repository
{
    /// <summary>
    /// 工作单元的目标是维护变化的对象列表。使用IUnitOfWorkRepository负责对象的持久化，
    /// 使用IUnitOfWork收集变化的对象，并将变化的对象放到各自的增删改列表中，最后Commit，
    /// Commit时需要循环遍历这些列表，并由Repository来持久化。
    /// </summary>
    public class KtUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        //[Dependency]
        //public IAccessorProvider AccessorProvider { get; set; }

        private IAccessorProvider _accessorProvider { get; set; }

        public KtUnitOfWork(IAccessorProvider accessorProvider)
        {
            _accessorProvider = accessorProvider;
        }

        public void Begin()
        {
            this._accessorProvider.DbHelper.BeginTransaction();
        }

        public void Begin(IsolationLevel il)
        {
            this._accessorProvider.DbHelper.BeginTransaction(il);
        }

        public void Commit()
        {
            this._accessorProvider.DbHelper.CommitTransaction();
        }

        public void RollBack()
        {
            this._accessorProvider.DbHelper.RollbackTransaction();
        }

        
    }
}
