/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DbFiledAttribute
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using QL.Core.Extensions;
namespace QL.Database
{
    /// <summary>
    /// 数据库字段的操作性
    /// </summary>
    public enum DbFieldOperable
    {
        /// <summary>
        /// 允许操作
        /// </summary>
        Allowed,
        /// <summary>
        /// 只限同组的操作
        /// </summary>
        OnlyGroup,
        /// <summary>
        /// 不允许操作
        /// </summary>
        Unallowed
    }

    /// <summary>
    /// 数据库字段的定义属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class DbFieldAttribute : Attribute
    {
        /// <summary>
        /// 采用自动检测字段类型方式实例化
        /// </summary>
        public DbFieldAttribute() : this(DbType.Object)
        {
            this.AutoDetectDbType = true;
        }
        /// <summary>
        /// 根据字段类型实例化
        /// </summary>
        public DbFieldAttribute(DbType dbType) 
        {
            this.AutoDetectDbType = false;
            this.Insertable = this.Updateable = DbFieldOperable.Allowed;
            this.DbType = dbType;
            this.Size = 0;
            this.PrimaryKey = false;
            this.Groups = DbFieldAttribute.EmptyGroup;
        }

        /// <summary>
        /// 对应于数据库的字段名称。如果为空或null则默认采用属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 自动检测字段类型。即根据属性字段的类型自动处理DbType的值
        /// </summary>
        public bool AutoDetectDbType { get; private set; }

        /// <summary>
        /// 数据字段类型
        /// </summary>
        protected DbType DbType { get; private set; }

        /// <summary>
        /// 是否是主键字段。如果是则在UPDATE时将使用此属性值做为更新条件
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// 是否是自增标识字段，如果是则不参与INSERT与UPDATE，并且在INSERT后会自动获取
        /// </summary>
        public bool Identity { get; set; }

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// 字段值大小
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 字段所在组，在插入或更新数据时可根据组更新或者插入部分字段值。
        /// 多个组之间可用“值的或操作”处理，如“1|2|8”表示属于三个组。
        /// </summary>
        public long Groups { get; set; }

        /// <summary>
        /// 常量值，表示是空的组
        /// </summary>
        public const long EmptyGroup = 0;
        /// <summary>
        /// 常量值，表示不属于任何的组
        /// </summary>
        public const long NoGroup = -1;

        /// <summary>
        /// 是否允许进行INSERT操作，默认允许
        /// </summary>
        public DbFieldOperable Insertable { get; set; }

        /// <summary>
        /// 是否允许进行UPDATE操作，默认允许
        /// </summary>
        public DbFieldOperable Updateable { get; set; }

        

        /// <summary>
        /// 获取数据类型
        /// </summary>
        /// <param name="propertyType">特性所在的属性值的类型</param>
        /// <returns></returns>
        public DbType GetDbType(Type propertyType)
        {
            if (this.AutoDetectDbType)
            {
                return propertyType.GetDbType();
            }
            else
            {
                return this.DbType;
            }
        }
        /// <summary>
        /// 判断此属性是否拥有groups中的某一组
        /// </summary>
        /// <param name="groups">要判断的组，如果有多个组，则各组之间可用“值的或操作”处理，如“1|2|8”表示属于三个组。</param>
        /// <returns>
        /// 1,如果groups为常量EmptyGroup则返回true;
        /// 2,如果groups为常量NoGroup则如果此属性也是空组(常量EmptyGroup)则返回true;否则返回false
        /// 3,否则此属性定义的组拥有groups定义的某一组，则返回true，否则返回false</returns>
        public bool InGroups(long groups)
        {
            if (groups == DbFieldAttribute.EmptyGroup) return true;

            if (this.Groups == DbFieldAttribute.EmptyGroup || this.Groups == DbFieldAttribute.NoGroup)
            {
                return groups == DbFieldAttribute.NoGroup;
            }
            else if (groups == DbFieldAttribute.NoGroup)
            {
                return false;
            }
            else
            {
                return (this.Groups & groups) != DbFieldAttribute.EmptyGroup;
            }
        }

        /// <summary>
        /// 是否允许操作
        /// </summary>
        /// <param name="groups">要判断的组，如果有多个组，则各组之间可用“值的或操作”处理，如“1|2|8”表示属于三个组。</param>
        /// <param name="insertion">是否插入操作，true=插入操作，false=更新操作</param>
        /// <returns></returns>
        public bool AllowOperation(long groups, bool insertion)
        {
            var op = insertion ? this.Insertable : this.Updateable;
            if (op == DbFieldOperable.Unallowed) return false;

            switch (op)
            {
                case DbFieldOperable.Unallowed: 
                    return false;
                case DbFieldOperable.Allowed: 
                    return this.InGroups(groups);
                default:
                    if (this.Groups != DbFieldAttribute.EmptyGroup
                        && this.Groups != DbFieldAttribute.NoGroup)
                    {
                        //定义了组，则也必须是处于组中
                        return groups != DbFieldAttribute.EmptyGroup &&
                               groups != DbFieldAttribute.NoGroup &&
                               (this.Groups & groups) != DbFieldAttribute.EmptyGroup;
                    }
                    else
                    {
                        //未定义组
                        return groups == DbFieldAttribute.NoGroup;
                    }
            }
        }
    }
}
