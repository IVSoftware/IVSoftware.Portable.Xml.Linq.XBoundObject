using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
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

    public class LeadingAffinityInfo
    {
        public LeadingAffinityInfo(XElement xel)
        {
            Root = xel;

            List<XElement>
                listAboves = new(),
                listBelows = new();

            foreach (var cxel in xel.Elements())
            {
                if (bool.TryParse(
                    cxel.Attribute(StdOffsettorAttribute.above)?.Value,
                    out var @bool) && @bool)
                {
                    listAboves.Add(cxel);
                }
                else
                {
                    listBelows.Add(cxel);
                }
            }

            Aboves = listAboves.ToArray();
            Belows = listBelows.ToArray();
        }

        public XElement Root { get; }

        public XElement[] Aboves { get; }

        public XElement[] Belows { get; }

        public IEnumerable<XElement> Ascend
        {
            get
            {
                for (int index = Aboves.Length - 1; index >= 0; index--)
                {
                    yield return Aboves[index];
                }
            }
        }

        public IEnumerable<XElement> Descend
        {
            get
            {
                for (int index = 0; index < Belows.Length; index++)
                {
                    yield return Belows[index];
                }
            }
        }
    }

    /// <summary>
    /// Standard attributes used to annotate an affinity-style offset field.
    /// </summary>
    /// <remarks>
    /// In IVS shorthand:
    /// - <c>xel</c> is the current <see cref="XElement"/>.
    /// - <c>pxel</c> is the parent <see cref="XElement"/>.
    /// - <c>cxel</c> is a child <see cref="XElement"/>.
    /// </remarks>
    public enum StdOffsettorAttribute
    {
        /// <summary>
        /// Marks a child <see cref="XElement"/> (<c>cxel</c>) as belonging to the leading band of its parent.
        /// </summary>
        /// <remarks>
        /// - These are the child nodes that participate in the ascending
        ///   field rather than in its ordinary trailing band.
        /// - The <see cref="LeadingAffinity"/> determines the yield sequence of the enumeration.
        /// </remarks>
        above,

        /// <summary>
        /// Marks the parent <see cref="XElement"/> (<c>pxel</c>) of the ascending field.
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

    public static partial class Extensions
    {
        /// <summary>
        /// Resolves leading-affinity information for the immediate child
        /// field of the current element.
        /// </summary>
        public static bool HasLeadingAffinity(
            this XElement @this,
            out LeadingAffinityInfo lai)
        {
            lai = new LeadingAffinityInfo(@this);
            return lai.Aboves.Length != 0;
        }

        /// <summary>
        /// Ascends modeled linear order using a BCL-style local-name filter.
        /// </summary>
        /// <remarks>
        /// - Filtering follows BCL-style local-name semantics.
        /// - <paramref name="includeSelf"/> means "begin traversal at self" rather than "force-yield self".
        /// - Therefore, when a filter is supplied, self is only returned if self matches the filter.
        /// </remarks>
        [Canonical]
        public static IEnumerable<XElement> Ascendors(
            this XElement @this,
            string? localName = null,
            bool includeSelf = false,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? current = includeSelf
                ? @this
                : Extensions.PreviousAscendor(
                    @this: @this,
                    name: null,
                    affinity: affinity);

            while (current is not null)
            {
                if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                {
                    yield return current;
                }
                current = Extensions.PreviousAscendor(
                    @this: current,
                    name: null,
                    affinity: affinity);
            }
        }

        /// <summary>
        /// Ascends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Ascendors(
            this XElement @this,
            Enum stdName,
            bool includeSelf = false,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            IEnumerable<XElement> result;

            if (stdName.IsAffinityPositionalPolicyViolation())
            {
                result = Enumerable.Empty<XElement>();
            }
            else
            {
                result = @this.Ascendors(
                    localName: stdName.ToString(),
                    includeSelf: includeSelf,
                    affinity: affinity);
            }
            return result;
        }

        /// <summary>
        /// Descends modeled linear order using a BCL-style local-name filter.
        /// </summary>
        /// <remarks>
        /// - When @this is the model root, Descendors().First() is the first item in the linear collection.
        /// - As a calibration, Descendors().Skip(0) does the same thing.
        /// - Filtering follows BCL-style local-name semantics.
        /// - <paramref name="includeSelf"/> means "begin traversal at self" rather than "force-yield self".
        /// - Therefore, when a filter is supplied, self is only returned if self matches the filter.
        /// </remarks>
        [Canonical]
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            string? localName = null,
            bool includeSelf = false,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? current = includeSelf
                ? @this
                : Extensions.NextDescendor(
                    @this: @this,
                    name: null,
                    affinity: affinity);

            while (current is not null)
            {
                if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                {
                    yield return current;
                }
                current = Extensions.NextDescendor(
                    @this: current,
                    name: null,
                    affinity: affinity);
            }
        }

        /// <summary>
        /// Descends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            Enum stdName,
            bool includeSelf = false,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            IEnumerable<XElement> result;

            if (stdName.IsAffinityPositionalPolicyViolation())
            {
                result = Enumerable.Empty<XElement>();
            }
            else
            {
                result = @this.Descendors(
                    localName: stdName.ToString(),
                    includeSelf: includeSelf,
                    affinity: affinity);
            }
            return result;
        }

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement? OffsettorAt(
            this XElement @this,
            Enum stdName,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            if (stdName.IsAffinityPositionalPolicyViolation())
            {
                return null;
            }
            return @this.OffsettorAt(
                name: stdName.ToString(),
                plusOrMinus: plusOrMinus,
                offsetZeroPolicy: offsetZeroPolicy,
                affinity: affinity);
        }

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement? OffsettorAt(
            this XElement @this,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute,
            LeadingAffinity affinity = LeadingAffinity.None)
            => @this.OffsettorAt(
                name: null,
                plusOrMinus: plusOrMinus,
                offsetZeroPolicy: offsetZeroPolicy,
                affinity: affinity);

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? OffsettorAt(
            this XElement @this,
            string? name,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            if (name is null)
            {
                return localResolveRawOffset();
            }

            return localResolveFilteredOffset();

            #region L o c a l F x
            XElement? localResolveRawOffset()
            {
                XElement? current = localGetRawAnchor();

                if (plusOrMinus == 0)
                {
                    return current;
                }

                if (current is null)
                {
                    return null;
                }

                if (plusOrMinus > 0)
                {
                    for (int i = 0; i < plusOrMinus; i++)
                    {
                        current =
                            current is null
                            ? null
                            : Extensions.NextDescendor(
                                @this: current,
                                name: null,
                                affinity: affinity);
                        if (current is not XElement)
                        {
                            @this.ThrowSoft<InvalidOperationException>(
                                $"Modeled offset exceeds the available forward range.");
                            return null;
                        }
                    }
                }
                else if (plusOrMinus < 0)
                {
                    for (int i = 0; i < -plusOrMinus; i++)
                    {
                        current =
                            current is null
                            ? null
                            : Extensions.PreviousAscendor(
                                @this: current,
                                name: null,
                                affinity: affinity);
                        if (current is not XElement)
                        {
                            @this.ThrowSoft<InvalidOperationException>(
                                $"Modeled offset exceeds the available backward range.");
                            return null;
                        }
                    }
                }

                return current;
            }

            XElement? localResolveFilteredOffset()
            {
                var anchor = localGetFilteredAnchor();

                if (plusOrMinus == 0)
                {
                    if (anchor is XElement xel)
                    {
                        return xel.Name.LocalName.Equals(
                            name,
                            StringComparison.Ordinal)
                            ? xel
                            : localReturnFilteredZeroMiss();
                    }
                    return localReturnFilteredZeroMiss();
                }

                if (anchor is not XElement current)
                {
                    return null;
                }

                if (plusOrMinus > 0)
                {
                    return localFilteredDescendorsFrom(current)
                        .Skip(plusOrMinus)
                        .FirstOrDefault();
                }

                return localFilteredAscendorsFrom(current)
                    .Skip(-plusOrMinus)
                    .FirstOrDefault();
            }

            XElement? localGetRawAnchor()
            {
                return offsetZeroPolicy switch
                {
                    OffsetZeroPolicy.Absolute => @this,
                    OffsetZeroPolicy.FirstFilterMatch =>
                        Extensions.Descendors(
                            @this: @this,
                            localName: null,
                            includeSelf: true,
                            affinity: affinity)
                        .FirstOrDefault(),
                    OffsetZeroPolicy.ForceAscendingFilterMatch =>
                        Extensions.PreviousAscendor(
                            @this: @this,
                            name: null,
                            affinity: affinity),
                    _ => throw new NotImplementedException(
                        $"Bad case: {offsetZeroPolicy}"),
                };
            }

            XElement? localGetFilteredAnchor()
            {
                return offsetZeroPolicy switch
                {
                    OffsetZeroPolicy.Absolute => @this,
                    OffsetZeroPolicy.FirstFilterMatch =>
                        Extensions.Descendors(
                            @this: @this,
                            localName: name,
                            includeSelf: true,
                            affinity: affinity)
                            .FirstOrDefault(),
                    OffsetZeroPolicy.ForceAscendingFilterMatch =>
                        Extensions.Ascendors(
                            @this: @this,
                            localName: name,
                            includeSelf: false,
                            affinity: affinity)
                            .FirstOrDefault(),
                    _ => throw new NotImplementedException(
                        $"Bad case: {offsetZeroPolicy}"),
                };
            }

            IEnumerable<XElement> localFilteredDescendorsFrom(XElement anchor)
            {
                foreach (var xel in Extensions.Descendors(
                    @this: anchor,
                    localName: name,
                    includeSelf: true,
                    affinity: affinity))
                {
                    yield return xel;
                }
            }

            IEnumerable<XElement> localFilteredAscendorsFrom(XElement anchor)
            {
                foreach (var xel in Extensions.Ascendors(
                    @this: anchor,
                    localName: name,
                    includeSelf: true,
                    affinity: affinity))
                {
                    yield return xel;
                }
            }

            XElement? localReturnFilteredZeroMiss()
            {
                @this.ThrowSoft<InvalidOperationException>(
                    "FilteredZeroMiss",
                    $"Explicit filter '{name}' requires zero to resolve " +
                    $"within the filtered domain.");
                return null;
            }
            #endregion L o c a l F x
        }

        private static bool IsAffinityPositionalPolicyViolation(this Enum sbFilter)
        {
            bool isPositionalViolation = false;
            if (sbFilter is LeadingAffinity)
            {
                isPositionalViolation = true;
                sbFilter.ThrowHard<InvalidOperationException>(
                    $"Detected {nameof(LeadingAffinity)} in filter position; This qualifier must be explicitly named or positionally last.");
            }
            return isPositionalViolation;
        }

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        public static XElement? PreviousAscendor(
            this XElement @this,
            Enum stdEnum,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? result;

            if (stdEnum.IsAffinityPositionalPolicyViolation())
            {
                result = null;
            }
            else
            {
                result = @this.PreviousAscendor(
                    name: stdEnum.ToString(),
                    affinity: affinity);
            }
            return result;
        }

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? PreviousAscendor(
            this XElement @this,
            string? name = null,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement current = @this;

            while (true)
            {
                var previous =
                    current.ElementsBeforeSelf().LastOrDefault() is XElement prevNode
                    ? prevNode.DescendantsAndSelf().Last()
                    : current.Parent;

                if (previous is XElement xel)
                {

                    if (name is null || xel.Name.LocalName.Equals(name, StringComparison.Ordinal))
                    {
                        return xel;
                    }

                    current = xel;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        public static XElement? NextDescendor(
            this XElement @this, 
            Enum stdEnum,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? result;

            if (stdEnum.IsAffinityPositionalPolicyViolation())
            {
                result = null;
            }
            else
            {
                result = @this.NextDescendor(
                    name: stdEnum.ToString(),
                    affinity: affinity);
            }
            return result;
        }

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? NextDescendor(
            this XElement @this, 
            string? name = null,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? current = @this;
            while (true)
            {
                XElement? next =
                    current?.FirstNode as XElement ??
                    current?.ElementsAfterSelf().FirstOrDefault() as XElement;

                while (next is null && current is not null)
                {
                    current = current.Parent;
                    next = current?.ElementsAfterSelf().FirstOrDefault() as XElement;
                }

                if (next is not XElement xel)
                {
                    return null;
                }

                if (name is null || xel.Name.LocalName.Equals(name, StringComparison.Ordinal))
                {
                    return xel;
                }

                current = xel;
            }
        }
    }
}
