using System;
using System.Collections.Generic;
using System.Xml.Linq;

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
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            string? localName,
            bool includeSelf = false)
            => throw new NotImplementedException();

        /// <summary>
        /// Descends modeled linear order using a standard enum local-name filter.
        /// </summary>
        public static IEnumerable<XElement> Descendors(
            this XElement @this,
            Enum? stdName,
            bool includeSelf = false)
            => throw new NotImplementedException();

        /// <summary>
        /// Resolves an element by relative offset within modeled linear order.
        /// </summary>
        public static XElement OffsettorAt(this XElement @this, int plusOrMinus)
            => throw new NotImplementedException();

        /// <summary>
        /// Resolves the previous element in modeled linear order.
        /// </summary>
        public static XElement PreviousOffsettor(this XElement @this)
            => throw new NotImplementedException();

        /// <summary>
        /// Resolves the next element in modeled linear order.
        /// </summary>
        public static XElement NextOffsettor(this XElement @this)
            => throw new NotImplementedException();
    }
}
