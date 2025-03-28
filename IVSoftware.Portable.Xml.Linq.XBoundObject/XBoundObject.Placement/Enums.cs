using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{

    /// <summary>
    /// Represents the expansion state of a view element.
    /// </summary>
    [Placement(EnumPlacement.UseXAttribute)]
    public enum PlusMinus
    {
        /// <summary>
        /// All child elements are collapsed.
        /// </summary>
        Collapsed,

        /// <summary>
        /// Expanded, but not all child elements are visible.
        /// </summary>
        Partial,

        /// <summary>
        /// All child elements are visible.
        /// </summary>
        Expanded,

        /// <summary>
        /// No child elements exist.
        /// </summary>
        Leaf,

        /// <summary>
        /// Transitive state that results in stable
        /// expanded state based on visible children.
        /// </summary>
        Auto,
    }
    public enum StdAttributeNameXBoundViewObject
    {
        text,
        isvisible,
        plusminus,
        datamodel,
    }

    internal enum StdAttributeNameInternal
    {
        text,
    }

    [Placement(EnumPlacement.UseXAttribute)]
    internal enum IsVisible
    {
        True,
        False,
    }
}
