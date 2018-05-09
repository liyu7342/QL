namespace QL.Core.Extensions
{
    using System;
    using System.Data;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class Types
    {
        public static Type CreateType(string typeInstance)
        {
            Assembly assembly2;
            if (string.IsNullOrEmpty(typeInstance))
            {
                return null;
            }
            string str = typeInstance.Trim();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            int index = str.IndexOf(',');
            Type type = null;
            if (index == -1)
            {
                type = Type.GetType(str, false, true);
                if (type == null)
                {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(str, false, true);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
                return type;
            }
            string assemblyFile = str.Substring(index + 1).TrimStart(new char[0]);
            str = str.Substring(0, index).TrimEnd(new char[0]);
            if (assemblyFile.IndexOf(":") != -1)
            {
                assembly2 = Assembly.LoadFrom(assemblyFile);
            }
            else
            {
                assembly2 = Assembly.Load(assemblyFile);
            }
            if (assembly2 != null)
            {
                type = assembly2.GetType(str, false, true);
            }
            return type;
        }

        public static DbType GetDbType(this Type type)
        {
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return DbType.Object;

                case TypeCode.Object:
                    if (!type.Equals(typeof(Guid)))
                    {
                        if (type.IsArray && type.Equals(typeof(byte[])))
                        {
                            return DbType.Binary;
                        }
                        return DbType.Object;
                    }
                    return DbType.Guid;

                case TypeCode.Char:
                    return DbType.StringFixedLength;
            }
            return typeCode.ToString().As<DbType>();
        }

        public static object GetDefaultValue(this Type type)
        {
            if (!type.Equals(typeof(string)))
            {
                if (type.Equals(typeof(DateTime)))
                {
                    return DateTime.MinValue;
                }
                if (type.Equals(typeof(bool)))
                {
                    return false;
                }
                if (((type.Equals(typeof(int)) || type.Equals(typeof(uint))) || (type.Equals(typeof(long)) || type.Equals(typeof(ulong)))) || (((type.Equals(typeof(float)) || type.Equals(typeof(double))) || (type.Equals(typeof(byte)) || type.Equals(typeof(sbyte)))) || ((type.Equals(typeof(short)) || type.Equals(typeof(ushort))) || type.Equals(typeof(decimal)))))
                {
                    return 0;
                }
                if (type.Equals(typeof(char)))
                {
                    return '\0';
                }
                if (type.Equals(typeof(Guid)))
                {
                    return Guid.Empty;
                }
                if (type.Equals(typeof(TimeSpan)))
                {
                    return TimeSpan.MinValue;
                }
                if (type.IsEnum)
                {
                    return Enum.GetValues(type).GetValue(0);
                }
            }
            return null;
        }
    }
}