namespace QL.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public static class DbObjectExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<DbObject> dbObjects)
        {
            if (dbObjects == null)
            {
                return null;
            }
            DbObject component = dbObjects.FirstOrDefault<DbObject>();
            if (component == null)
            {
                return null;
            }
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
            DataTable table = new DataTable();
            foreach (PropertyDescriptor descriptor in properties)
            {
                table.Columns.Add(descriptor.Name, descriptor.PropertyType);
            }
            foreach (DbObject obj3 in dbObjects)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor descriptor2 in properties)
                {
                    bool flag;
                    row[descriptor2.Name] = obj3.GetDataInternal(descriptor2.Name, out flag);
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public static DbObject ToDbObject(this NameValueCollection collection)
        {
            return collection.ToDbObject<DbObject>();
        }

        public static T ToDbObject<T>(this NameValueCollection collection) where T : DbObject, new()
        {
            T local = Activator.CreateInstance<T>();
            foreach (string str in collection)
            {
                local.SetDataInternal(str, collection[str]);
            }
            return local;
        }

        public static List<DbObject> ToDbObjectList(this DataTable table)
        {
            return table.ToDbObjectList<DbObject>();
        }

        public static List<T> ToDbObjectList<T>(this DataTable table) where T : DbObject, new()
        {
            List<T> list = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T item = Activator.CreateInstance<T>();
                foreach (DataColumn column in table.Columns)
                {
                    item.SetDataInternal(column.ColumnName, row[column]);
                }
                list.Add(item);
            }
            return list;
        }
    }
}