using System;
using System.Collections.Generic;
using System.Text;
using IVSoftware.Portable.Common.Attributes;

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

    [Placement(EnumPlacement.UseXAttribute)]
    public enum StdAttributeNameXBoundViewObject
    {
        text,
        isvisible,
        plusminus,
        datamodel,
    }


    [Placement(EnumPlacement.UseXAttribute)]
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
    public enum ExpandDirection
    {
        /// <summary>
        /// Make child items visible if they exist.
        /// PlusMinus will be Expanded or Leaf only when done.
        /// </summary>
        ToItems,

        /// <summary>
        /// Determine expandion state based on the presence
        /// and or visibility of child items.
        /// PlusMinus can be Expanded, Partial or Leaf when done.
        /// </summary>
        FromItems,
    }

    /// <summary>
    /// Selects how offset zero is established for filtered lookups.
    /// </summary>
    /// <remarks>
    /// Only affects the ambiguous case when a filter name is supplied and
    /// the requested offset is 0. Non-zero offsets keep their normal
    /// directional semantics.
    /// </remarks>
    public enum OffsetZeroPolicy
    {
        /// <summary>
        /// Zero is calibrated to the receiver itself.
        /// </summary>
        Absolute = 0,

        /// <summary>
        /// Zero is calibrated to the first forward filtered match,
        /// including self.
        /// </summary>
        /// <remarks>
        /// This is the natural policy for root-anchored filtered indexing.
        /// </remarks>
        FirstFilterMatch = 1,

        /// <summary>
        /// Zero is calibrated from the ascending filtered direction.
        /// </summary>
        /// <remarks>
        /// Use when zero must be established by walking backward through
        /// the filtered domain rather than forward from the receiver.
        /// This explicitly overrides the default forward interpretation of
        /// <see cref="FirstFilterMatch"/>.
        /// </remarks>
        ForceAscendingFilterMatch = 2,
    }

    /// <summary>
    /// Enables affinity semantics.
    /// </summary>
    /// <remarks>
    /// Given:
    /// An arbitrary XElement 3 at depth 0
    /// 3 arbitrary child nodes of 3: [0, 1, 2] with above="True" and where 0 is the first element of 3.
    /// 3 arbitrary child nodes of 3: [4, 5, 6] that are implicitly 'below' 3.
    /// </remarks>
    public enum LeadingAffinity
    {
        /// <summary>
        /// Attributes that are <see cref="StdModelAttribute.above"/> receive no special treatment.
        /// </summary>
        None,

        /// <summary>
        /// Process <see cref="StdOffsettorAttribute.above"/> as leading the parent node where first child yields first.
        /// </summary>
        /// <remarks>
        /// Corresponds to the index order of the modeled collection.
        /// Yield: 0, 1, 2, 3, 4, 5, 6
        /// </remarks>
        Linear,

        /// <summary>
        /// Process <see cref="StdOffsettorAttribute.above"/> as leading the parent node where apex child yields first.
        /// </summary>
        /// <remarks>
        /// This is used, for example, then the parent holds a starting time for calculating "countdown times".
        /// Yield: 2, 1, 0, 3, 4, 5, 6
        /// </remarks>
        Ascending,

        /// <summary>
        /// Process <see cref="StdOffsettorAttribute.above"/> where yields occur in both directions.
        /// </summary>
        /// <remarks>
        /// This is used, for example, then the parent holds a starting time for calculating "countdown times".
        /// Yield: 2, 1, 0, 1, 2, 3, 4, 5, 6
        /// </remarks>
        AscendingFirst,
    }

    public enum LeadingAffinityTraversal
    {
        /// <summary>
        /// Marks the first phase, iterating upward toward lower linear indexes.
        /// </summary>
        /// <remarks>
        /// Mental model: "Moving backwards in time - set start time based on duration".
        /// </remarks>
        Ascending,

        /// <summary>
        /// Marks the highest point of the field before returning downward.
        /// </summary>
        Apex,

        /// <summary>
        /// Marks the return phase, iterating downward toward higher linear indexes.
        /// </summary>
        /// <remarks>
        /// Mental model: "Moving forward in time - set end time based on remaining".
        /// </remarks>
        Descending,
    }

    /// <summary>
    /// Standard attributes used to annotate an affinity-style offset field.
    /// </summary>
    /// <remarks>
    /// In IVS shorthand:
    /// - <c>xel</c> is the current <see cref="System.Xml.Linq.XElement"/>.
    /// - <c>pxel</c> is the parent <see cref="System.Xml.Linq.XElement"/>.
    /// - <c>cxel</c> is a child <see cref="System.Xml.Linq.XElement"/>.
    /// </remarks>
    public enum StdOffsettorAttribute
    {
        /// <summary>
        /// Marks a child <see cref="System.Xml.Linq.XElement"/> (<c>cxel</c>) as belonging to the leading band of its parent.
        /// </summary>
        /// <remarks>
        /// - These are the child nodes that participate in the ascending
        ///   field rather than in its ordinary trailing band.
        /// - The <see cref="LeadingAffinity"/> determines the yield sequence of the enumeration.
        /// </remarks>
        above,

        /// <summary>
        /// Marks the parent <see cref="System.Xml.Linq.XElement"/> (<c>pxel</c>) of the ascending field.
        /// </summary>
        /// <remarks>
        /// This is the governing root.
        /// EXAMPLE
        /// - Think of this as, perhaps, a "pinned" point in time.
        /// - As the field ascends, the "duration" of such nodes
        ///   would then be progressively subtracted from that point.
        /// </remarks>
        [Canonical("The effective depth of pxel is authoritatively defined as 0.")]
        pxel,

        /// <summary>
        /// Indicates the current direction of the field enumeration.
        /// </summary>
        direction,

        /// <summary>
        /// When ascending an affinity field from root,
        /// </summary>
        cxelprevasc,
    }
}
