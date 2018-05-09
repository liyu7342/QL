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
    /// 用户
    /// </summary>
    [Serializable]
    public class User : IEntity
    {

        /// <summary>
        /// 标识，自增
        /// </summary>
        [DbField(DbType.Int32, Name = "id", PrimaryKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        [DbField(DbType.String, Name = "uuid")]
        public string Uuid { get; set; }

        /// <summary>
        /// 机器标识
        /// </summary>
        [DbField(DbType.String, Name = "device_id")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [DbField(DbType.Int32, Name = "device_type")]
        public int DeviceType { get; set; }

        /// <summary>
        /// 所用网络
        /// </summary>
        [DbField(DbType.String, Name = "device_net_type")]
        public string DeviceNetType { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        [DbField(DbType.String, Name = "device_model")]
        public string DeviceModel { get; set; }

        /// <summary>
        /// 开始使用时间
        /// </summary>
        [DbField(DbType.DateTime, Name = "beg_use_time")]
        public DateTime BegUseTime { get; set; }

        /// <summary>
        /// 最后使用时间
        /// </summary>
        [DbField(DbType.DateTime, Name = "last_use_time")]
        public DateTime LastUseTime { get; set; }

        /// <summary>
        /// 使用次数
        /// </summary>
        [DbField(DbType.Int32, Name = "use_count")]
        public int UseCount { get; set; }

        /// <summary>
        /// app版本
        /// </summary>
        [DbField(DbType.String, Name = "app_version")]
        public string AppVersion { get; set; }

        /// <summary>
        /// 分辨率 
        /// </summary>
        [DbField(DbType.String, Name = "resolution")]
        public string Resolution { get; set; }

        /// <summary>
        /// 系统版本
        /// </summary>
        [DbField(DbType.String, Name = "sys_version")]
        public string SysVersion { get; set; }

        /// <summary>
        /// 字体大小
        /// </summary>
        [DbField(DbType.Int32, Name = "font_size")]
        public int FontSize { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DbField(DbType.String, Name = "remark")]
        public string Remark { get; set; }

        /// <summary>
        /// ip地址
        /// </summary>
        [DbField(DbType.String, Name = "ip_address")]
        public string IPAddress { get; set; }

        /// <summary>
        /// ip地址
        /// </summary>
        [DbField(DbType.String, Name = "ip_geo")]
        public string IPGEO { get; set; }

        /// <summary>
        /// 注册用户名
        /// </summary>
        [DbField(DbType.String, Name = "member_name", ReadOnly = true)]
        public string MemberName { get; set; }
    }
}
