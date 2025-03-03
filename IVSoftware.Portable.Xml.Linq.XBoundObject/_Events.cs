using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq
{

    /// <summary>
    /// Provides event data for requesting the default sort order.
    /// </summary>
    public class DefaultSortOrderRequestEventArgs : EventArgs
    {
        /// <summary>
        /// A predefined set of standard attribute names for XBound objects.
        /// </summary>
        private enum StdXBOAttribute
        {
            /// <summary>
            /// When part of an observable bindable collection, this optional
            /// attribute broadcasts status including node-specific bindings.
            /// </summary>
            statusobc,

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
        }
        public string[] SortOrder { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Raised when a default sort order is requested.
        /// </summary>
        public static event EventHandler<DefaultSortOrderRequestEventArgs> DefaultSortOrderRequest;

        /// <summary>
        /// Raises the default sort order request event and returns the determined sort order.
        /// </summary>
        /// <returns>An array of sort order names.</returns>
        internal static string[] RaiseEvent()
        {
            var e = new DefaultSortOrderRequestEventArgs();
            DefaultSortOrderRequest?
                .Invoke(
                    nameof(DefaultSortOrderRequestEventArgs)
                    .Replace(nameof(EventArgs), string.Empty), e);
            return 
                e.SortOrder?.Any() == true
                ? e.SortOrder
                : Enum.GetNames(typeof(StdXBOAttribute));            
        }
    }
}
