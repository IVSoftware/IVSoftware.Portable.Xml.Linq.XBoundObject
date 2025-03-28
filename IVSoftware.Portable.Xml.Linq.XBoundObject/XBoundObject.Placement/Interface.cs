using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    /// <summary>
    /// Interface for objects bound to an <see cref="XElement"/>.
    /// Provides access to the bound element and a method to initialize it once.
    /// </summary>
    public interface IXBoundObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the bound <see cref="XElement"/> instance.
        /// </summary>
        XElement XEL { get; }

        /// <summary>
        /// Initializes the <see cref="XEL"/> property.
        /// Throws <see cref="InvalidOperationException"/> if already 
        /// initialized with a conflicting reference.
        /// Returns the hosting <see cref="XElement"/> to allow fluent chaining.
        /// </summary>
        XElement InitXEL(XElement xel);
    }

    /// <summary>
    /// Interface for view objects bound to an <see cref="XElement"/>, supporting expansion and display operations.
    /// </summary>
    public interface IXBoundViewObject : IXBoundObject
    {
        /// <summary>
        /// Displays the element at the specified path.
        /// </summary>
        /// <param name="path">The hierarchical path to the element.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to user-defined enum or null.</param>
        /// <returns>The <see cref="XElement"/> at the specified path.</returns>
        XElement Show(string path, Enum pathAttribute = null);

        /// <summary>
        /// Expands the element at the specified path.
        /// </summary>
        /// <param name="path">The hierarchical path to the element.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to user-defined enum or null.</param>
        /// <returns>The <see cref="PlusMinus"/> after the operation.</returns>
        PlusMinus Expand(string path, Enum pathAttribute = null);

        /// <summary>
        /// Collapses the element at the specified path.
        /// </summary>
        /// <param name="path">The hierarchical path to the element.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to user-defined enum or null.</param>
        /// <returns>The <see cref="PlusMinus"/> after the operation.</returns>
        PlusMinus Collapse(string path, Enum pathAttribute = null);

        PlusMinus PlusMinus { get; }
        bool IsVisible { get; }
    }
}
