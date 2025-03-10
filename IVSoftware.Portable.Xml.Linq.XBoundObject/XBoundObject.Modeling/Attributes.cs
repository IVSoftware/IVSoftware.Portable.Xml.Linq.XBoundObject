using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    /// <summary>
    /// Attribute to mark properties that should be ignored in the notification system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreNODAttribute : Attribute { }

    /// <summary>
    /// Attribute that specifies a property to watch for value creation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class WaitForValueCreatedAttribute : Attribute
    {
        public WaitForValueCreatedAttribute(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            IsValueCreatedPropertyName = propertyName;
        }
        public string IsValueCreatedPropertyName { get; }
    }
}
