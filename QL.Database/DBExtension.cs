namespace QL.Database
{
    using QL.Core.Data;
    using QL.Core.Extensions;
    using QL.Core.Log;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Runtime.CompilerServices;

    public static class DBExtension
    {
        internal static void CopyToDbObject(this IDataRecord record, DbObject dbObject)
        {
            for (int i = 0; i < record.FieldCount; i++)
            {
                string name = record.GetName(i);
                object obj2 = record.GetValue(i);
                dbObject.SetDataInternal(name, obj2);
            }
        }

        internal static Dictionary<string, PropertyDescriptor> GetDbFileds(this object obj)
        {
            return obj.GetType().GetDbFileds();
        }

        internal static Dictionary<string, PropertyDescriptor> GetDbFileds(this Type type)
        {
            Type type2 = typeof(DbFieldAttribute);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(type);
            if (properties.Count == 0)
            {
                return null;
            }
            Dictionary<string, PropertyDescriptor> dictionary = new Dictionary<string, PropertyDescriptor>(properties.Count, StringComparer.OrdinalIgnoreCase);
            foreach (PropertyDescriptor descriptor in properties)
            {
                DbFieldAttribute attribute = (DbFieldAttribute)descriptor.Attributes[type2];
                if (attribute != null)
                {
                    string key = string.IsNullOrEmpty(attribute.Name) ? descriptor.Name : attribute.Name;
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, descriptor);
                    }
                }
            }
            return dictionary;
        }

        internal static object GetValue(this IDataRecord record, int i, Type dataType)
        {
            object defaultValue = dataType.GetDefaultValue();
            if (!record.IsDBNull(i))
            {
                return record.GetValue(i).As(dataType, defaultValue);
            }
            return defaultValue;
        }

        public static T ToObject<T>(this IDataRecord record)
        {
            try
            {
                T local = Activator.CreateInstance<T>();
                if (local is DbObject)
                {
                    DbObject dbObject = local as DbObject;
                    record.CopyToDbObject(dbObject);
                    return local;
                }
                Dictionary<string, PropertyDescriptor> dbFileds = local.GetDbFileds();
                if ((dbFileds != null) && (dbFileds.Count != 0))
                {
                    for (int i = 0; i < record.FieldCount; i++)
                    {
                        string name = record.GetName(i);
                        if (dbFileds.ContainsKey(name))
                        {
                            PropertyDescriptor descriptor = dbFileds[name];
                            if (!descriptor.IsReadOnly)
                            {
                                descriptor.SetValue(local, record.GetValue(i, descriptor.PropertyType));
                            }
                        }
                    }
                }
                return local;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "在从数据集转换为对象时出错");
            }
            return default(T);
        }

        public static List<T> ToObjectList<T>(this DataTable table)
        {
            bool flag = false;
            Dictionary<string, PropertyDescriptor> dbFileds = null;
            List<T> list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T component = default(T);
                try
                {
                    component = Activator.CreateInstance<T>();
                    if (component is DbObject)
                    {
                        DbObject obj2 = component as DbObject;
                        foreach (DataColumn column in table.Columns)
                        {
                            obj2.SetDataInternal(column.ColumnName, row[column]);
                        }
                    }
                    else
                    {
                        if (!flag)
                        {
                            flag = true;
                            dbFileds = typeof(T).GetDbFileds();
                        }
                        if ((dbFileds != null) && (dbFileds.Count != 0))
                        {
                            foreach (DataColumn column2 in table.Columns)
                            {
                                string columnName = column2.ColumnName;
                                if (dbFileds.ContainsKey(columnName))
                                {
                                    PropertyDescriptor descriptor = dbFileds[columnName];
                                    if (!descriptor.IsReadOnly)
                                    {
                                        descriptor.SetValue(component, row[column2].As(descriptor.PropertyType, descriptor.PropertyType.GetDefaultValue()));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "在从数据集转换为对象时出错");
                    return list;
                }
                list.Add(component);
            }
            return list;
        }

        public static List<T> ToObjectList<T>(this IDataReader reader)
        {
            bool flag = false;
            Dictionary<string, PropertyDescriptor> dbFileds = null;
            List<T> list = new List<T>();
            while (reader.Read())
            {
                IDataRecord record = reader;
                T component = default(T);
                try
                {
                    component = Activator.CreateInstance<T>();
                    if (component is DbObject)
                    {
                        DbObject dbObject = component as DbObject;
                        record.CopyToDbObject(dbObject);
                    }
                    else
                    {
                        if (!flag)
                        {
                            flag = true;
                            dbFileds = typeof(T).GetDbFileds();
                        }
                        if ((dbFileds != null) && (dbFileds.Count != 0))
                        {
                            for (int i = 0; i < record.FieldCount; i++)
                            {
                                string name = record.GetName(i);
                                if (dbFileds.ContainsKey(name))
                                {
                                    PropertyDescriptor descriptor = dbFileds[name];
                                    if (!descriptor.IsReadOnly)
                                    {
                                        descriptor.SetValue(component, record.GetValue(i, descriptor.PropertyType));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "在从数据集转换为对象时出错");
                    return list;
                }
                list.Add(component);
            }
            return list;
        }
    }
}
