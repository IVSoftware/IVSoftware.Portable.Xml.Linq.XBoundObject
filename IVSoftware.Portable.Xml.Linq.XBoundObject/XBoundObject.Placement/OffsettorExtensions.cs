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
    public enum AffinityOption
    {
        /// <summary>
        /// Attributes that are <see cref="StdModelAttribute.above"/> receive no special treatment.
        /// </summary>
        None,

        /// <summary>
        /// Process <see cref="StdModelAttribute.above"/> as leading the parent node where first child yields first.
        /// </summary>
        /// <remarks>
        /// Corresponds to the index order of the modeled collection.
        /// </remarks>
        Linear,

        /// <summary>
        /// Process <see cref="StdModelAttribute.above"/> as leading the parent node where first child yields last.
        /// </summary>
        /// <remarks>
        /// This is used, for example, then the parent holds a starting time for calculating "countdown times".
        /// </remarks>
        Reverse,
    }

    public static partial class Extensions
    {
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
            AffinityOption affinity = AffinityOption.None)
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
            AffinityOption affinity = AffinityOption.None)
            => @this.Ascendors(
                localName: stdName.ToString(),
                includeSelf: includeSelf,
                affinity: affinity);

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
            AffinityOption affinity = AffinityOption.None)
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
            AffinityOption affinity = AffinityOption.None)
            => @this.Descendors(
                localName: stdName.ToString(),
                includeSelf: includeSelf,
                affinity: affinity);

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement? OffsettorAt(
            this XElement @this,
            Enum stdName,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute,
            AffinityOption affinity = AffinityOption.None)
            => @this.OffsettorAt(
                name: stdName.ToString(),
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
            AffinityOption affinity = AffinityOption.None)
        {
            localThrowIfAffinityMasqueradesAsName();

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

            void localThrowIfAffinityMasqueradesAsName()
            {
                if (name is string localName &&
                    Enum.TryParse<AffinityOption>(localName, ignoreCase: false, out _))
                {
                    @this.ThrowHard<InvalidOperationException>(
                        $"'{localName}' is an {nameof(AffinityOption)} value and cannot be used as a filter name. " +
                        $"Pass it only as the named trailing argument '{nameof(affinity)}: ...'.");
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

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        public static XElement? PreviousAscendor(
            this XElement @this,
            Enum stdEnum,
            AffinityOption affinity = AffinityOption.None)
            => @this.PreviousAscendor(
                name: stdEnum.ToString(),
                affinity: affinity);

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? PreviousAscendor(
            this XElement @this,
            string? name = null,
            AffinityOption affinity = AffinityOption.None)
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
            AffinityOption affinity = AffinityOption.None)
            => @this.NextDescendor(
                name: stdEnum.ToString(),
                affinity: affinity);

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? NextDescendor(
            this XElement @this, 
            string? name = null,
            AffinityOption affinity = AffinityOption.None)
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
