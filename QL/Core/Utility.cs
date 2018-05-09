/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  KTUtility
 *
 * ***********************************************/

namespace QL.Core
{
    using QL.Core.Extensions;
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    public static class Utility
    {
        private static System.Random _Random;

        public static T CreateInstance<T>(string instance)
        {
            if (!string.IsNullOrEmpty(instance))
            {
                int index = instance.IndexOf(',');
                object obj2 = null;
                if (index == -1)
                {
                    obj2 = Activator.CreateInstance(null, instance.Trim()).Unwrap();
                }
                else
                {
                    string assemblyFile = instance.Substring(index + 1).Trim();
                    instance = instance.Substring(0, index).Trim();
                    if (assemblyFile.IndexOf(':') != -1)
                    {
                        obj2 = Activator.CreateInstanceFrom(assemblyFile, instance).Unwrap();
                    }
                    else
                    {
                        obj2 = Activator.CreateInstance(assemblyFile, instance).Unwrap();
                    }
                }
                if ((obj2 != null) && (obj2 is T))
                {
                    return (T)obj2;
                }
            }
            return default(T);
        }

        public static System.Random CreateRandom()
        {
            return new System.Random(CreateRndSeed());
        }

        public static string CreateRndCode(int length)
        {
            return CreateRndCode("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", length);
        }

        public static string CreateRndCode(string codeChars, int length)
        {
            if (string.IsNullOrEmpty(codeChars) || (length < 1))
            {
                return string.Empty;
            }
            int maxValue = codeChars.Length - 1;
            StringBuilder builder = new StringBuilder(length);
            System.Random random = Random;
            for (int i = 1; i <= length; i++)
            {
                builder.Append(codeChars[random.Next(0, maxValue)]);
            }
            return builder.ToString();
        }

        public static int CreateRndSeed()
        {
            byte[] data = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(data);
            return BitConverter.ToInt32(data, 0);
        }

        public static int IPToInt32(string ip)
        {
            IPAddress address = null;
            if (IPAddress.TryParse(ip, out address))
            {
                return BitConverter.ToInt32(address.GetAddressBytes(), 0);
            }
            return -1;
        }

        public static long IPToInt64(string ip)
        {
            int num = IPToInt32(ip);
            if (num == -1)
            {
                return -1L;
            }
            return (num & ((long)0xffffffffL));
        }

        public static bool IsAbsolutePath(string path)
        {
            Uri uri;
            return Uri.TryCreate(path, UriKind.Absolute, out uri);
        }

        public static bool IsValidFileExt(string file, string fileExt)
        {
            if (string.IsNullOrEmpty(file))
            {
                return false;
            }
            if (string.IsNullOrEmpty(fileExt))
            {
                return true;
            }
            string extension = VirtualPathUtility.GetExtension(file);
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            return fileExt.IsContain(extension, ";", true);
        }

        public static bool IsValidIP(string ip)
        {
            string[] strArray = ip.Split(new char[] { '.' }, 4);
            if (strArray.Length != 4)
            {
                return false;
            }
            foreach (string str in strArray)
            {
                int num = str.As<int>(-1);
                if ((num < 0) || (num > 0xff))
                {
                    return false;
                }
            }
            return true;
        }

        public static string ToAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }
            if (IsAbsolutePath(path))
            {
                return path;
            }
            HttpContext current = HttpContext.Current;
            if ((current != null) && (current.Server != null))
            {
                return current.Server.MapPath(path);
            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.TrimStart(new char[] { '~', '\\', '/' }));
        }

        public static string ToIPAddress(int address)
        {
            return ToIPAddress((long)(address & ((long)0xffffffffL)));
        }

        public static string ToIPAddress(long address)
        {
            if ((address >= 0L) && (address <= 0xffffffffL))
            {
                return new IPAddress(address).ToString();
            }
            return "Unknown";
        }

        public static System.Random Random
        {
            get
            {
                if (_Random == null)
                {
                    lock (typeof(Utility))
                    {
                        if (_Random == null)
                        {
                            _Random = CreateRandom();
                        }
                    }
                }
                return _Random;
            }
        }
    }
}

