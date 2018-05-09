using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QL.Repository
{
    /// <summary>
    /// 表示所有集成于该接口的类型都是Unit Of Work的一种实现。
    /// 说明：IUnitOfWork,复杂维护变化的对象列表，并最后Commit，依次遍历变化的列表，并持久化，这就是Commit的事情。
    /// </summary>
    public interface IUnitOfWork
    {
        ///// <summary>
        ///// 更新
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <param name="unitofWorkRepository"></param>
        //void RegisterUpdate(IEntity entity, IUnitOfWorkRepository unitofWorkRepository);
        ///// <summary>
        ///// 新增
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <param name="unitofWorkRepository"></param>
        //void RegisterAdd(IEntity entity, IUnitOfWorkRepository unitofWorkRepository);
        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="entity"></param>
        ///// <param name="unitofWorkRepository"></param>
        //void RegisterRemoved(IEntity entity, IUnitOfWorkRepository unitofWorkRepository);
        ///// <summary>
        ///// 提交
        ///// </summary>
        //bool Commit();


        void Begin();

        void Commit();

        void RollBack();
    }
}
