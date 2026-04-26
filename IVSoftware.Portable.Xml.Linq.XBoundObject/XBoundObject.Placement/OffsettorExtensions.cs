using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using IVSoftware.Portable.Common.Attributes;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
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
            Enum? stdName,
            bool includeSelf = false)
            => @this.Ascendors(stdName?.ToString(), includeSelf);

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
            Enum? stdName,
            bool includeSelf = false)
            => @this.Descendors(stdName?.ToString(), includeSelf);

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement OffsettorAt(this XElement @this, int plusOrMinus)
        {
            XElement? current = @this;

            if (plusOrMinus > 0)
            {
                for (int i = 0; i < plusOrMinus; i++)
                {
                    current = current?.NextDescendor();
                    if (current is null)
                    {
                        throw new InvalidOperationException("Modeled offset exceeds the available forward range.");
                    }
                }
            }
            else if (plusOrMinus < 0)
            {
                for (int i = 0; i < -plusOrMinus; i++)
                {
                    current = current?.PreviousAscendor();
                    if (current is null)
                    {
                        throw new InvalidOperationException("Modeled offset exceeds the available backward range.");
                    }
                }
            }

            return current;
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
        {
            XElement? current = @this;
            string name = stdEnum.ToString();

            while ((current = current.NextDescendor()) is XElement xel)
            {
                if (xel.Name.LocalName.Equals(name, StringComparison.Ordinal))
                {
                    return xel;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? NextDescendor(this XElement @this)
        {
            if (@this.FirstNode is XElement child)
            {
                return child;
            }

            XElement? current = @this;
            while (current is not null)
            {
                if (current.ElementsAfterSelf().FirstOrDefault() is XElement nextSibling)
                {
                    return nextSibling;
                }
                current = current.Parent;
            }

            return null;
        }
    }
}
