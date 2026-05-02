using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
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

    public static partial class Extensions
    {
        /// <summary>
        /// Resolves leading-affinity information for the immediate child
        /// field of the current element.
        /// </summary>
        internal static bool HasLeadingAffinity(
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
        internal static IEnumerable<XElement> Ascendors(
            this XElement @this,
            string? localName = null,
            bool includeSelf = false)
        {
            XElement? current = includeSelf
                ? @this
                : Extensions.PreviousAscendor(
                    @this: @this,
                    name: null);

            while (current is not null)
            {
                if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                {
                    yield return current;
                }
                current = Extensions.PreviousAscendor(
                    @this: current,
                    name: null);
            }
        }

        /// <summary>
        /// Ascends modeled linear order using a standard enum local-name filter.
        /// </summary>
        internal static IEnumerable<XElement> Ascendors(
            this XElement @this,
            Enum stdName,
            bool includeSelf = false)
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
                    includeSelf: includeSelf);
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
        internal static IEnumerable<XElement> Descendors(
            this XElement @this,
            string? localName = null,
            bool includeSelf = false,
            LeadingAffinity affinity = LeadingAffinity.None)
        {
            XElement? current =
                includeSelf
                ? @this
                : Extensions.NextDescendor(
                    @this: @this,
                    name: null,
                    affinity: LeadingAffinity.None);

            while (current is not null)
            {
                if (affinity != LeadingAffinity.None
                    && current.HasLeadingAffinity(out var lai))
                {
                    foreach (var xel in localAffinitySegment(lai))
                    {
                        if (localName is null || xel.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                        {
                            yield return xel;
                        }
                    }

                    current =
                        Extensions.NextDescendor(
                            @this: localSegmentTail(lai),
                            name: null,
                            affinity: LeadingAffinity.None);
                }
                else
                {
                    if (localName is null || current.Name.LocalName.Equals(localName, StringComparison.Ordinal))
                    {
                        yield return current;
                    }
                    current =
                        Extensions.NextDescendor(
                            @this: current,
                            name: null,
                            affinity: LeadingAffinity.None);
                }
            }

            #region L o c a l F x
            IEnumerable<XElement> localAffinitySegment(LeadingAffinityInfo lai)
            {
                switch (affinity)
                {
                    case LeadingAffinity.Linear:
                        foreach (var xel in lai.Aboves)
                        {
                            yield return xel;
                        }
                        yield return lai.Root;
                        foreach (var xel in lai.Descend)
                        {
                            yield return xel;
                        }
                        break;

                    case LeadingAffinity.Ascending:
                        foreach (var xel in lai.Ascend)
                        {
                            yield return xel;
                        }
                        yield return lai.Root;
                        foreach (var xel in lai.Descend)
                        {
                            yield return xel;
                        }
                        break;

                    case LeadingAffinity.AscendingFirst:
                        foreach (var xel in lai.Ascend)
                        {
                            yield return xel;
                        }
                        for (int index = 1; index < lai.Aboves.Length; index++)
                        {
                            yield return lai.Aboves[index];
                        }
                        yield return lai.Root;
                        foreach (var xel in lai.Descend)
                        {
                            yield return xel;
                        }
                        break;

                    case LeadingAffinity.None:
                    default:
                        yield return lai.Root;
                        break;
                }
            }

            XElement localSegmentTail(LeadingAffinityInfo lai)
                => lai.Belows.LastOrDefault() ?? lai.Root;
            #endregion L o c a l F x
        }

        /// <summary>
        /// Descends modeled linear order using a standard enum local-name filter.
        /// </summary>
        internal static IEnumerable<XElement> Descendors(
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
        internal static XElement? OffsettorAt(
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
        internal static XElement? OffsettorAt(
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
        internal static XElement? OffsettorAt(
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
                                name: null);
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
                            name: null),
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
                            includeSelf: false)
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
                    includeSelf: true))
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
        internal static XElement? PreviousAscendor(
            this XElement @this,
            Enum stdEnum)
        {
            XElement? result;

            if (stdEnum.IsAffinityPositionalPolicyViolation())
            {
                result = null;
            }
            else
            {
                result = @this.PreviousAscendor(
                    name: stdEnum.ToString());
            }
            return result;
        }

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        [Canonical]
        internal static XElement? PreviousAscendor(
            this XElement @this,
            string? name = null)
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
        internal static XElement? NextDescendor(
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
        internal static XElement? NextDescendor(
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
