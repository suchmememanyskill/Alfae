using System;
using System.Reflection;

namespace Launcher.Extensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class BindingAttribute : Attribute
    {
        public string ControlName { get; set; }
        public string FieldName { get; set; }
        
        public object Control { get; set; }
        public PropertyInfo ControlField { get; set; }
        public object Instance { get; set; }
        public PropertyInfo AttachedField { get; set; }

        public BindingAttribute(string controlName, string fieldName)
        {
            ControlName = controlName;
            FieldName = fieldName;
        }

        public void Set() => ControlField.SetValue(Control, AttachedField.GetValue(Instance));
    }
}