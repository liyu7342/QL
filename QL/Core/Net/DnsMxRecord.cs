/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  DnsMxRecord
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace QL.Core.Net
{
    /// <summary>
    /// MX记录的查询器,使用此类必须是在winnt系统或以上才可以使用
    /// </summary>
    /// <example>
    /// <code>
    /// var address = DnsMxRecord.Query("gmail.com");
    /// Console.WriteLine(string.Join("\n",address));
    /// </code>
    /// </example>
    public static class DnsMxRecord
    {
        /// <summary>
        /// 用于同步的对象
        /// </summary>
        private static object objSyc = new object();

        /// <summary>
        /// 可缓存的MX记录数
        /// </summary>
        public static volatile int CacheSize = 200;
        
        /// <summary>
        /// MX记录
        /// </summary>
        private class MxRecord
        {
            /// <summary>
            /// 刷新时间
            /// </summary>
            public DateTime RefreshTime { get; set; }

            /// <summary>
            /// 地址数据
            /// </summary>
            public string[] Addresses { get; set; }

            /// <summary>
            /// 此MX记录是否已过期。默认1小时后过期
            /// </summary>
            public bool IsExpired
            {
                get
                {
                    return DateTime.Now.Subtract(this.RefreshTime).TotalHours > 1;
                }
            }
        }

        /// <summary>
        /// MX记录缓存
        /// </summary>
        private static Dictionary<string, MxRecord> mxCaches = new Dictionary<string, MxRecord>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 根据邮件地址获取邮件域名的MX记录
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <returns></returns>
        public static string[] Query(MailAddress mailAddress)
        {
            if (mailAddress == null) return null;
            return Query(mailAddress.Host);
        }
        /// <summary>
        /// 查询某个域名对应的MX记录
        /// </summary>
        /// <param name="hostName">域名地址</param>
        /// <returns></returns>
        public static string[] Query(string hostName)
        {
            lock (objSyc)
            {
                if (mxCaches.ContainsKey(hostName))
                {
                    var record = mxCaches[hostName];
                    if (!record.IsExpired) return record.Addresses;
                }
            }
            string[] addresses = null;
            try
            {
                addresses = InternalQuery(hostName);
            }
            catch
            {
                addresses = null;
            }
            if (addresses != null && addresses.Length > 0 && DnsMxRecord.CacheSize > 0)
            {
                lock (objSyc)
                {
                    if (!mxCaches.ContainsKey(hostName))
                    {
                        if (mxCaches.Count >= DnsMxRecord.CacheSize)
                        {
                            //移除已过期的
                            string firstKey = null;
                            string expiredKey = null;
                            foreach (var p in mxCaches)
                            {
                                if (firstKey == null) firstKey = p.Key;
                                if (p.Value.IsExpired)
                                {
                                    expiredKey = p.Key;
                                    break;
                                }
                            }
                            if (expiredKey == null) expiredKey = firstKey;
                            if (expiredKey != null) mxCaches.Remove(expiredKey);
                        }
                        mxCaches.Add(hostName, new MxRecord() { Addresses = addresses, RefreshTime = DateTime.Now });
                    }
                    else
                    {
                        //刷新数据
                        var record = mxCaches[hostName];
                        record.Addresses = addresses;
                        record.RefreshTime = DateTime.Now;
                    }
                }
            }
            return addresses;
        }

        #region 获取MX记录
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pszName"></param>
        /// <param name="wType"></param>
        /// <param name="options"></param>
        /// <param name="aipServers"></param>
        /// <param name="ppQueryResults"></param>
        /// <param name="pReserved"></param>
        /// <returns></returns>
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery([MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName, 
            QueryTypes wType, QueryOptions options, int aipServers, ref IntPtr ppQueryResults, int pReserved);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRecordList"></param>
        /// <param name="freeType"></param>
        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(IntPtr pRecordList, DnsFreeType freeType);

        /// <summary>
        /// 获取某个域名对应的MX地址记录.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private static string[] InternalQuery(string domain)
        {
            IntPtr ptr = IntPtr.Zero;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new NotSupportedException();
            }
            List<string> addresses = new List<string>();
            int error = DnsQuery(ref domain, QueryTypes.DNS_TYPE_MX, QueryOptions.DNS_QUERY_BYPASS_CACHE, 0, ref ptr, 0);
            if (error != 0) { throw new Win32Exception(error); }

            IntPtr ptr2 = ptr;
            while (ptr2 != IntPtr.Zero)
            {
                DnsRecord dr = (DnsRecord)Marshal.PtrToStructure(ptr2, typeof(DnsRecord));
                if (dr.wType == (short)QueryTypes.DNS_TYPE_MX)
                {
                    string address = Marshal.PtrToStringAuto(dr.pNameExchange);
                    addresses.Add(address);
                }
                ptr2 = dr.pNext;
            }
            DnsRecordListFree(ptr, DnsFreeType.DnsFreeRecordList);
            return addresses.ToArray();

        }
        /// <summary>
        /// 查询选项
        /// </summary>
        private enum QueryOptions
        {
            /// <summary>
            /// Bypasses the resolver cache on the lookup. 
            /// </summary>
            DNS_QUERY_BYPASS_CACHE = 8
        }
        /// <summary>
        /// 查询类型
        /// </summary>
        private enum QueryTypes
        { 
            /// <summary>
            /// 查询MX记录
            /// </summary>
            DNS_TYPE_MX = 0x000f
        }
        /// <summary>
        /// DNS释放类型
        /// </summary>
        private enum DnsFreeType
        {
            /// <summary>
            /// 
            /// </summary>
            DnsFreeFlat = 0,
            /// <summary>
            /// 
            /// </summary>
            DnsFreeRecordList = 1,
            /// <summary>
            /// 
            /// </summary>
            DnsFreeParsedMessageFields = 2 
        }
        /// <summary>
        /// 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct DnsRecord
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameExchange;
            public short wPreference;
            public short Pad;
        }
        #endregion
    }
}
