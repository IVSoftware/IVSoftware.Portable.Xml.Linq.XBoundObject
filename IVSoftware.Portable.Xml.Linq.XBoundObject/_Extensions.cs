using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject
{
    public static partial class Extensions
    {
        /// <summary>
        /// Fully qualified XBoundAttribute Setter
        /// </summary>
        public static void SetBoundAttributeValue(
            this XElement xel,
            object tag,
            string name = null,
            string text = null,
            SetOption options = SetOption.NameToLower)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = tag.GetType().Name.Split('`')[0];
            }
            var xbo = new XBoundAttribute(name: name, tag: tag, text: text);
            xel.Add(xbo);
            xbo.RaiseObjectBound(xel);
        }

        /// <summary>
        /// Name come from user-supplied standard enumerated names. 
        /// </summary>
        public static void SetBoundAttributeValue(
            this XElement xel,
            object tag,
            Enum stdName,
            string text = null,
            SetOption options = SetOption.NameToLower) =>
                xel.SetBoundAttributeValue(
                    tag,
                    stdName.ToString(),
                    text,
                    options);

        /// <summary>
        /// Return Single or Default where type is T. Null testing will be done by client.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static T To<T>(this XElement xel, bool @throw = false)
        {
            xel.TryGetSingleBoundAttributeByType(out T attr, @throw);
            return attr;
        }

        /// <summary>
        /// Return true if xel has any attribute of type T"/>
        /// </summary>
        public static bool Has<T>(this XElement xel) =>
            xel
            .Attributes()
            .Any(_ => (_ is XBoundAttribute) && (((XBoundAttribute)_).Tag is T));

        /// <summary>
        /// Try return Single or Default where type is T.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static bool TryGetSingleBoundAttributeByType<T>(this XElement xel, out T o, bool @throw = false)
        {
            XBoundAttribute xba;
            if (@throw)
            {
                xba =
                    (XBoundAttribute)
                    xel.Attributes()
                    .Single(battr => (battr is XBoundAttribute) && (((XBoundAttribute)battr).Tag is T));
            }
            else
            {
                var candidates =
                    xel.Attributes()
                    .Where(battr => (battr is XBoundAttribute) && (((XBoundAttribute)battr).Tag is T));
                if (candidates.Count() > 1)
                {
                    Debug.Fail($"Multiple instances of type {typeof(T)} exist.");
                    xba = default;
                }
                else
                {
                    xba = (XBoundAttribute)candidates.FirstOrDefault();
                }
            }
            if (Equals(xba, default(XBoundAttribute)))
            {
                o = default;
                return false;
            }
            else
            {
                o = (T)xba.Tag;
                return true;
            }
        }

        /// <summary>
        /// Returns the first ancestor that Has XBoundAttribute of type T.
        /// </summary>
        public static T AncestorOfType<T>(this XElement @this, bool includeSelf = false, bool @throw = false)
        {
            if(@throw)
            {
                return 
                    includeSelf
                    ? @this.AncestorsAndSelf().First(_ => _.Has<T>()).To<T>()
                    : @this.Ancestors().First(_ => _.Has<T>()).To<T>();
            }
            else
            {
                XElement anc = 
                    includeSelf
                    ? @this.AncestorsAndSelf().FirstOrDefault(_ => _.Has<T>())
                    : @this.Ancestors().FirstOrDefault(_ => _.Has<T>());
                return
                    anc is null
                    ? default
                    : anc.To<T>();
            }
        }

        /// <summary>
        /// Sets an attribute on the given XElement using the name of the Enum type as the attribute name 
        /// and the Enum value as the attribute value.
        /// </summary>
        /// <param name="this">The XElement to set the attribute on.</param>
        /// <param name="value">The Enum value to store as an attribute.</param>
        /// <param name="useLowerCaseName">If true, the attribute name will be the Enum type name in lowercase; otherwise, it will use the exact type name.</param>
        public static void SetAttributeValue(this XElement @this, Enum value, bool useLowerCaseName = true)
            => @this
            .SetAttributeValue(
                useLowerCaseName
                ? value.GetType().Name.ToLower()
                : value.GetType().Name,
                $"{value}");

        /// <summary>
        /// Attempts to retrieve an enum value from an attribute in the given XElement.
        /// </summary>
        /// <typeparam name="T">The enum type to parse.</typeparam>
        /// <param name="this">The XElement to retrieve the attribute from.</param>
        /// <param name="value">
        /// When this method returns, contains the parsed enum value if the attribute exists and is valid; 
        /// otherwise, the default value of T.
        /// </param>
        /// <param name="stringComparison">
        /// The string comparison method used for matching attribute names. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the attribute exists and was successfully parsed as an enum of type T; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
        this XElement @this,
        out T value,
        StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        where T : struct, Enum
        {
            var type = typeof(T);
            value = default;

            var attribute = @this
                .Attributes()
                .FirstOrDefault(attr => string.Equals(attr.Name.LocalName, type.Name, stringComparison));

            if (attribute != null && Enum.TryParse(attribute.Value, out T parsedValue))
            {
                value = parsedValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a shallow copy of the given XElement, preserving only its name and attributes,
        /// but excluding its child elements.
        /// </summary>
        /// <param name="this">The XElement to copy.</param>
        /// <returns>A new XElement with the same name and attributes, but without child elements.</returns>
        public static XElement ToShallow(this XElement @this)
            => new XElement(@this.Name, @this.Attributes());


        /// <summary>
        /// Creates a new XElement that includes only the specified attributes, removing all others.
        /// </summary>
        /// <param name="this">The XElement to filter.</param>
        /// <param name="names">The attribute names to keep.</param>
        /// <returns>A new XElement with only the specified attributes.</returns>
        public static XElement WithOnlyAttributes(this XElement @this, params string[] names)
            => new XElement(
                @this.Name,
                @this.Attributes().Where(attr => names.Contains(attr.Name.LocalName))
            );

        /// <summary>
        /// Creates a new XElement that removes the specified attributes, keeping all others.
        /// </summary>
        /// <param name="this">The XElement to modify.</param>
        /// <param name="names">The attribute names to remove.</param>
        /// <returns>A new XElement without the specified attributes.</returns>
        public static XElement WithoutAttributes(this XElement @this, params string[] names)
            => new XElement(@this.Name,
                @this.Attributes().Where(attr => !names.Contains(attr.Name.LocalName))
            );

        /// <summary>
        /// Sorts the attributes of the given <see cref="XElement"/> based on the specified order.
        /// Attributes listed in <paramref name="sortOrder"/> will appear first in the specified order,
        /// while any attributes not included in <paramref name="sortOrder"/> will be appended at the end in their original order.
        /// This method is applied recursively to all descendant elements.
        /// If <paramref name="sortOrder"/> is empty, the method attempts to retrieve a default
        /// sort order using <see cref="DefaultSortOrderRequestEventArgs"/> before throwing an exception.
        /// </summary>
        /// <param name="this">The <see cref="XElement"/> whose attributes will be sorted.</param>
        /// <param name="sortOrder">An array of attribute names defining the desired sort order.</param>
        /// <returns>The <see cref="XElement"/> with sorted attributes.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="sortOrder"/> is empty and no default sort order is available.
        /// </exception>
        public static XElement SortAttributes(this XElement @this, params string[] sortOrder)
        {
            if (!sortOrder.Any())
            {
                if (DefaultSortOrderRequestEventArgs.RaiseEvent() is string[] defaultSortOrder &&
                    defaultSortOrder.Any())
                {
                    sortOrder = defaultSortOrder;
                }
                else
                {
                    throw new ArgumentException(message: "The sortOrder array is empty");
                }
            }
            var dict = @this
                .Attributes()
                .ToDictionary(
                attr => attr.Name.LocalName,
                comparer: StringComparer.OrdinalIgnoreCase);

            @this.RemoveAttributes();

            foreach (var key in sortOrder)
            {
                if (dict.TryGetValue(key, out var xattr))
                {
                    Debug.WriteLine($"250303.A {key} FOUND");
                    @this.Add(xattr);
                    dict.Remove(key);
                }
                else Debug.WriteLine($"250303.A {key} NOT FOUND");
            }
            @this.Add(dict.Values);
            foreach (var xel in @this.Elements())
            {
                xel.SortAttributes(sortOrder);
            }
            return @this;
        }

        /// <summary>
        /// Sorts the attributes of the given <see cref="XElement"/> and its descendants based on the names of an enum type.
        /// The attribute order follows the sequence of names in the specified enum.
        /// The sort order is determined using <see cref="Enum.GetNames(Type)"/> for the specified enum type.
        /// </summary>
        /// <typeparam name="T">An enum type whose names define the attribute order.</typeparam>
        /// <param name="this">The <see cref="XElement"/> whose attributes will be sorted.</param>
        /// <returns>The <see cref="XElement"/> with sorted attributes.</returns>
        public static XElement SortAttributes<T>(this XElement @this) where T : Enum =>
            @this.SortAttributes(Enum.GetNames(typeof(T)));
    }
}
