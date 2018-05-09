namespace QL.Core.Data
{
    using QL.Core.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class DbObject : ICustomTypeDescriptor
    {
        private Hashtable _slot;
        private string[] DataNamesCache;
        private Dictionary<string, InternalData> InternalDatas;
        private PropertyDescriptorCollection PropertyDescriptorCollectionCache;

        public DbObject()
            : this(0x10)
        {
        }

        public DbObject(int capacity)
        {
            this._slot = new Hashtable(capacity, StringComparer.OrdinalIgnoreCase);
        }

        protected void AddInternalData(string name, Func<object> get, Action<object> set)
        {
            if (this.InternalDatas == null)
            {
                this.InternalDatas = new Dictionary<string, InternalData>(StringComparer.OrdinalIgnoreCase);
            }
            if (!this.InternalDatas.ContainsKey(name))
            {
                this.InternalDatas.Add(name, new InternalData(get, set));
                this.OnObjectChanged();
            }
        }

        public bool ContainData(string name)
        {
            return (((this.InternalDatas != null) && this.InternalDatas.ContainsKey(name)) || this._slot.ContainsKey(name));
        }

        public void CopyTo(DbObject obj)
        {
            string[] allDataNames = this.GetAllDataNames();
            lock (obj._slot.SyncRoot)
            {
                foreach (string str in allDataNames)
                {
                    bool flag;
                    obj.SetDataInternal(str, this.GetDataInternal(str, out flag));
                }
            }
        }

        public string[] GetAllDataNames()
        {
            if (this.DataNamesCache == null)
            {
                lock (this._slot.SyncRoot)
                {
                    if ((this.InternalDatas == null) || (this.InternalDatas.Count == 0))
                    {
                        this.DataNamesCache = new string[this._slot.Count];
                        this._slot.Keys.CopyTo(this.DataNamesCache, 0);
                    }
                    else
                    {
                        List<string> list = new List<string>(this.InternalDatas.Keys);
                        foreach (string str in this._slot.Keys)
                        {
                            if (!this.InternalDatas.ContainsKey(str))
                            {
                                list.Add(str);
                            }
                        }
                        this.DataNamesCache = list.ToArray();
                    }
                }
            }
            return this.DataNamesCache;
        }

        public T GetData<T>(string name)
        {
            bool flag;
            object dataInternal = this.GetDataInternal(name, out flag);
            if (!flag)
            {
                return default(T);
            }
            return dataInternal.As<T>();
        }

        public T GetData<T>(string name, T replacement)
        {
            bool flag;
            object dataInternal = this.GetDataInternal(name, out flag);
            if (!flag)
            {
                return replacement;
            }
            return dataInternal.As<T>(replacement);
        }

        public int GetDataCount()
        {
            return this.GetAllDataNames().Length;
        }

        internal object GetDataInternal(string name, out bool existing)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "数据名称不能为空");
            }
            if ((this.InternalDatas != null) && this.InternalDatas.ContainsKey(name))
            {
                existing = true;
                InternalData data = this.InternalDatas[name];
                if (data.Get != null)
                {
                    return data.Get();
                }
                return null;
            }
            existing = this._slot.ContainsKey(name);
            if (!existing)
            {
                return null;
            }
            return this._slot[name];
        }

        private PropertyDescriptorCollection GetPropertyDescriptorCollection()
        {
            if (this.PropertyDescriptorCollectionCache == null)
            {
                Type componentType = base.GetType();
                PropertyInfo[] properties = componentType.GetProperties();
                string[] allDataNames = this.GetAllDataNames();
                Dictionary<string, DbObjectPropertyDescriptor> dictionary = new Dictionary<string, DbObjectPropertyDescriptor>(properties.Length + allDataNames.Length, StringComparer.OrdinalIgnoreCase);
                foreach (PropertyInfo info in properties)
                {
                    if (info.GetIndexParameters().Length == 0)
                    {
                        dictionary.Add(info.Name, new DbObjectPropertyDescriptor(info.Name, componentType, info.PropertyType, info.GetCustomAttributes(true).Cast<Attribute>().ToArray<Attribute>()));
                    }
                }
                foreach (string str in allDataNames)
                {
                    if (!dictionary.ContainsKey(str))
                    {
                        object obj2 = this[str];
                        Type propertyType = (obj2 == null) ? typeof(object) : obj2.GetType();
                        dictionary.Add(str, new DbObjectPropertyDescriptor(str, componentType, propertyType, null));
                    }
                }
                this.PropertyDescriptorCollectionCache = new PropertyDescriptorCollection(dictionary.Values.ToArray<DbObjectPropertyDescriptor>());
            }
            return this.PropertyDescriptorCollectionCache;
        }

        protected virtual bool OnBeforeSetData(string name, object value)
        {
            return true;
        }

        protected virtual void OnObjectChanged()
        {
            this.DataNamesCache = null;
            this.PropertyDescriptorCollectionCache = null;
        }

        public void SetData(string name, object value)
        {
            lock (this._slot.SyncRoot)
            {
                this.SetDataInternal(name, value);
            }
        }

        protected void SetDataImp(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "数据名称不能为空");
            }
            if ((this.InternalDatas != null) && this.InternalDatas.ContainsKey(name))
            {
                InternalData data = this.InternalDatas[name];
                if (data.Set != null)
                {
                    data.Set(value);
                }
            }
            else if (this._slot.ContainsKey(name))
            {
                this._slot[name] = value;
            }
            else
            {
                this._slot.Add(name, value);
                this.OnObjectChanged();
            }
        }

        public void SetDataInternal(string name, object value)
        {
            if (this.OnBeforeSetData(name, value))
            {
                this.SetDataImp(name, value);
            }
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection(base.GetType().GetCustomAttributes(true).Cast<Attribute>().ToArray<Attribute>());
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return base.GetType().FullName;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return new TypeConverter();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return ((ICustomTypeDescriptor)this).GetEvents(null);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection propertyDescriptorCollection = this.GetPropertyDescriptorCollection();
            if ((attributes == null) || (attributes.Length == 0))
            {
                return propertyDescriptorCollection;
            }
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor descriptor in propertyDescriptorCollection)
            {
                if (descriptor.Attributes.Matches(attributes))
                {
                    list.Add(descriptor);
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public object this[string name]
        {
            get
            {
                bool flag;
                return this.GetDataInternal(name, out flag);
            }
            set
            {
                this.SetData(name, value);
            }
        }

        private class InternalData
        {
            public InternalData(Func<object> get, Action<object> set)
            {
                this.Get = get;
                this.Set = set;
            }

            public Func<object> Get { get; private set; }

            public Action<object> Set { get; private set; }
        }
    }
}
