﻿using IVSoftware.Portable.Disposable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
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
        XElement Parent { get; }
    }

    /// <summary>
    /// Interface for view objects bound to an <see cref="XElement"/>, supporting expansion and display operations.
    /// </summary>
    public interface IXBoundViewObject : IXBoundObject
    {
        string Text { get; set; }
        bool IsVisible { get; set; }
        int Space { get; }
        PlusMinus PlusMinus { get; }
        PlusMinus Expand(ExpandDirection direction);
        PlusMinus Collapse(); 
        ICommand PlusMinusToggleCommand { get; }
    }

    public interface IXObjectChangeEventSink
    { 
        DisposableHost DisableXObjectChangeEvents { get; }
    }
}
