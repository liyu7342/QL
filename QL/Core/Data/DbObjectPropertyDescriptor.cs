namespace QL.Core.Data
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    internal sealed class DbObjectPropertyDescriptor : PropertyDescriptor
    {
        private Type _ComponentType;
        private Type _PropertyType;

        public DbObjectPropertyDescriptor(string dataName, Type componentType, Type propertyType, Attribute[] attrs)
            : base(dataName, attrs)
        {
            this._ComponentType = componentType;
            this._PropertyType = propertyType;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            DbObject obj2 = component as DbObject;
            if (obj2 != null)
            {
                bool flag;
                object dataInternal = obj2.GetDataInternal(this.Name, out flag);
                if (flag)
                {
                    return dataInternal;
                }
            }
            PropertyInfo property = component.GetType().GetProperty(this.Name);
            if (property != null)
            {
                return property.GetValue(component, null);
            }
            return null;
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            DbObject obj2 = component as DbObject;
            if ((obj2 != null) && obj2.ContainData(this.Name))
            {
                obj2.SetDataInternal(this.Name, value);
            }
            else
            {
                PropertyInfo property = component.GetType().GetProperty(this.Name);
                if (property != null)
                {
                    property.SetValue(component, value, null);
                }
            }
            this.OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return this._ComponentType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this._PropertyType;
            }
        }
    }
}
