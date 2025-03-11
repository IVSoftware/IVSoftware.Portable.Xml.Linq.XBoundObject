using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    public enum SortOrderNOD
    {
        /// <summary>
        /// The property name, OR in the case of the 
        /// origin model this holds the name of the type.
        /// </summary>
        name,

        /// <summary>
        /// When part of an observable bindable collection, this optional
        /// attribute broadcasts status including node-specific bindings.
        /// </summary>
        statusnod,

        /// <summary>
        /// XBound object containing PropertyInfo.
        /// </summary>
        pi,

        /// <summary>
        /// XBound instance of a reference type.
        /// </summary>
        instance,

        /// <summary>
        /// For Enum, ValueType, and String, this indicates that
        /// the instance of this property has a runtime type that
        /// differs from its declared type (e.g., object).
        /// </summary>
        runtimetype,

        /// <summary>
        /// OnPropertyChanged delegate
        /// </summary>
        onpc,

        /// <summary>
        /// OnCollectionChanged delegate
        /// </summary>
        oncc,

        /// <summary>
        /// Configuration of delegates for notifications.
        /// </summary>
        notifyinfo,
    }
    public enum StdFrameworkName
    {
        xel,
        model,
        member,
        context,
        OnPC,
        OnCC,
    }
}
