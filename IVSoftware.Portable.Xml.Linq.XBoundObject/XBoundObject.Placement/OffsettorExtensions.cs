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
            bool includeSelf = false)
        {
            XElement? current = includeSelf
                ? @this
                : @this.PreviousAscendor();

            while (current is not null)
            {
                if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                {
                    yield return current;
                }
                current = current.PreviousAscendor();
            }
        }

        /// <summary>
        /// Ascends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Ascendors(
            this XElement @this,
            Enum stdName,
            bool includeSelf = false)
            => @this.Ascendors(stdName.ToString(), includeSelf);

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
            bool includeSelf = false)
        {
            XElement? current = includeSelf
                ? @this
                : @this.NextDescendor();

            while (current is not null)
            {
                if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                {
                    yield return current;
                }
                current = current.NextDescendor();
            }
        }

        /// <summary>
        /// Descends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            Enum stdName,
            bool includeSelf = false)
            => @this.Descendors(stdName.ToString(), includeSelf);

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement? OffsettorAt(
            this XElement @this,
            Enum stdName,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute)
            => @this.OffsettorAt(stdName.ToString(), plusOrMinus, offsetZeroPolicy);

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? OffsettorAt(
            this XElement @this,
            string? name,
            int plusOrMinus,
            OffsetZeroPolicy offsetZeroPolicy = OffsetZeroPolicy.Absolute)
        {
            if (name is null)
            {
                return resolveRawOffset();
            }

            return resolveFilteredOffset();

            XElement? resolveRawOffset()
            {
                var current = getRawAnchor();

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
                        current = current.NextDescendor();
                        if (current is not XElement)
                        {
                            throw new InvalidOperationException(
                                "Modeled offset exceeds the available forward range.");
                        }
                    }
                }
                else if (plusOrMinus < 0)
                {
                    for (int i = 0; i < -plusOrMinus; i++)
                    {
                        current = current.PreviousAscendor();
                        if (current is not XElement)
                        {
                            throw new InvalidOperationException(
                                "Modeled offset exceeds the available backward range.");
                        }
                    }
                }

                return current;
            }

            XElement? resolveFilteredOffset()
            {
                var anchor = getFilteredAnchor();

                if (plusOrMinus == 0)
                {
                    if (anchor is XElement xel)
                    {
                        return xel.Name.LocalName.Equals(
                            name,
                            StringComparison.Ordinal)
                            ? xel
                            : returnFilteredZeroMiss();
                    }
                    return returnFilteredZeroMiss();
                }

                if (anchor is not XElement current)
                {
                    return null;
                }

                if (plusOrMinus > 0)
                {
                    return filteredDescendorsFrom(current)
                        .Skip(plusOrMinus)
                        .FirstOrDefault();
                }

                return filteredAscendorsFrom(current)
                    .Skip(-plusOrMinus)
                    .FirstOrDefault();
            }

            XElement? getRawAnchor()
            {
                return offsetZeroPolicy switch
                {
                    OffsetZeroPolicy.Absolute => @this,
                    OffsetZeroPolicy.FirstFilterMatch =>
                        @this.Descendors(includeSelf: true).FirstOrDefault(),
                    OffsetZeroPolicy.ForceAscendingFilterMatch =>
                        @this.PreviousAscendor(),
                    _ => throw new NotImplementedException(
                        $"Bad case: {offsetZeroPolicy}"),
                };
            }

            XElement? getFilteredAnchor()
            {
                return offsetZeroPolicy switch
                {
                    OffsetZeroPolicy.Absolute => @this,
                    OffsetZeroPolicy.FirstFilterMatch =>
                        @this.Descendors(name, includeSelf: true)
                            .FirstOrDefault(),
                    OffsetZeroPolicy.ForceAscendingFilterMatch =>
                        @this.Ascendors(name, includeSelf: false)
                            .FirstOrDefault(),
                    _ => throw new NotImplementedException(
                        $"Bad case: {offsetZeroPolicy}"),
                };
            }

            IEnumerable<XElement> filteredDescendorsFrom(XElement anchor)
            {
                foreach (var xel in anchor.Descendors(name, includeSelf: true))
                {
                    yield return xel;
                }
            }

            IEnumerable<XElement> filteredAscendorsFrom(XElement anchor)
            {
                foreach (var xel in anchor.Ascendors(name, includeSelf: true))
                {
                    yield return xel;
                }
            }

            XElement? returnFilteredZeroMiss()
            {
                @this.ThrowSoft<InvalidOperationException>(
                    "FilteredZeroMiss",
                    $"Explicit filter '{name}' requires zero to resolve " +
                    $"within the filtered domain.");
                return null;
            }
        }

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        public static XElement? PreviousAscendor(this XElement @this, Enum stdEnum)
            => @this.PreviousAscendor(stdEnum.ToString());

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? PreviousAscendor(this XElement @this, string? name=null)
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
        public static XElement? NextDescendor(this XElement @this, Enum stdEnum)
            => @this.NextDescendor(stdEnum.ToString());

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? NextDescendor(this XElement @this, string? name = null)
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
