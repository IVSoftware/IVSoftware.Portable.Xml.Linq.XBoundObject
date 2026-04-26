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
        public static IEnumerable<XElement> Ascendors(
            this XElement @this,
            string? localName,
            bool includeSelf = false)
            => throw new NotImplementedException();

        /// <summary>
        /// Ascends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Ascendors(
            this XElement @this,
            Enum? stdName,
            bool includeSelf = false)
            => throw new NotImplementedException();

        /// <summary>
        /// Descends modeled linear order using a BCL-style local-name filter.
        /// </summary>
        /// <remarks>
        /// - When @this is the model root, Descendors().First() is the first item in the linear collection.
        /// - As a calibration, Descendors().Skip(0) does the same thing.
        /// </remarks>
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            string? localName,
            bool includeSelf = false)
        {
            var xroot = @this.AncestorsAndSelf().Last();

            bool wantYield = false;

            foreach (var xel in xroot.DescendantsAndSelf())
            {
                if (!wantYield)
                {
                    if (ReferenceEquals(@this, xel))
                    {
                        wantYield = true;
                        if (includeSelf && localIsNameMatch(xel))
                        {
                            yield return @this;
                        }
                    }
                }
                else if (localIsNameMatch(xel))
                {
                    yield return xel;
                }
            }

            bool localIsNameMatch(XElement xel) =>
                localName is null ||
                xel.Name.LocalName.Equals(localName, StringComparison.Ordinal);
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
            => throw new NotImplementedException();

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        public static XElement? PreviousAscendor(this XElement @this, Enum stdEnum)
            => @this.PreviousOffsettor(stdEnum.ToString());

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        [Canonical]
        public static XElement? PreviousOffsettor(this XElement @this, string? name=null)
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
            => throw new NotImplementedException();

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        public static XElement? NextOffsettor(this XElement @this)
            => throw new NotImplementedException();
    }
}
