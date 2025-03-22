using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
        /// Retrieves a single attribute of type T from an XElement and returns it. 
        /// Null testing will be performed by the caller.
        /// </summary>
        /// <remarks>
        /// This method attempts to retrieve a single bound attribute by the specified type T.
        /// If multiple attributes of the type are found or if no attributes of the type are found, 
        /// the behavior of the method depends on the throw parameter:
        /// - If @throw is true, an InvalidOperationException is thrown indicating that the operation
        ///   is not valid given the object's current state. This is particularly relevant when the 
        ///   expected single result is not achievable.
        /// - If the type T is an Enum and no attribute is found, an InvalidOperationException is also thrown
        ///   suggesting the use of nullable types for Enums to properly handle cases where an attribute is not found.
        /// - If @throw is false, the method returns the default value of type T.
        /// </remarks>
        /// <param name="xel">The XElement to search for the attribute.</param>
        /// <param name="throw">Whether to throw an exception if the attribute is not found or if multiple are found.</param>
        /// <returns>The attribute of type T if found and valid; otherwise, the default value of type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the retrieval of a single attribute of type T is not possible either due to multiple attributes of the type existing or none being found, and when @throw is true. For Enums, suggests using nullable types if an attribute cannot be returned.</exception>
        public static T To<T>(this XElement xel, bool @throw = false)
        {
            var type = typeof(T);
            if (xel.TryGetSingleBoundAttributeByType(out T result, @throw: false))
            {
                // Don't throw yet!
                return result;
            }
            else
            {
                if (@throw) throw new InvalidOperationException($"typeof({typeof(T).Name})");
                else if (localTryGetParsedEnum(out T parsedEnum))
                {
                    return parsedEnum;
                }
                else
                {
                    if (@throw || type.IsEnum)
                    {
                        throw new InvalidOperationException(InvalidOperationExceptionMessage<T>());
                    }
                    else return default;
                };
                bool localTryGetParsedEnum(out T parsedEnum)
                {
                    Type nullableSafeType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                    if (AllowEnumParsing && nullableSafeType.IsEnum)
                    {
                        // The attribute name is expected to be the same as the enum type's name but in lowercase, 
                        // and the value is stored as a case-sensitive string. This approach is used typically when 
                        // the attribute is set using SetEnumValue(EnumType.Value) which writes the enum as a string.
                        if (xel
                            .Attributes()
                            .FirstOrDefault(_ => string.Equals(
                                    _.Name.LocalName,
                                    nullableSafeType.Name, StringComparison.OrdinalIgnoreCase
                                )) is XAttribute attr)

                        {
                            foreach (var value in nullableSafeType.GetEnumValues())
                            {
                                if(string.Equals(value.ToString(), attr.Value))
                                {
                                    parsedEnum = (T)value;
                                    return true;
                                }
                            }
                        }
                        parsedEnum = default;
                        return false;
                    }
                    else
                    {
                        parsedEnum = default;
                        return false;
                    }
                }
            }
        }
        internal static string InvalidOperationExceptionMessage<T>() => $"No valid {typeof(T).Name} found. To handle cases where an enum attribute might not exist, use a nullable version: To<{typeof(T).Name}?>() or check @this.Has<{typeof(T).Name}>() first.";
        public static bool AllowEnumParsing { get; set; } = true;

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
            Type safeType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            XBoundAttribute xba;
            if (@throw)
            {
                try
                {
                    xba =
                        xel
                        .Attributes()
                        .OfType<XBoundAttribute>()
                        .Single(_=>Equals(_.Tag.GetType(), safeType));
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException(InvalidOperationExceptionMessage<T>());
                }
            }
            else
            {
                var candidates =
                    Equals(safeType, typeof(Enum))
                    ? xel
                        .Attributes()
                        .OfType<XBoundAttribute>()
                        .Where(_ => _.Tag.GetType().IsEnum)
                        .ToArray()
                    : xel
                        .Attributes()
                        .OfType<XBoundAttribute>()
                        .Where(_ => _.Tag.GetType().IsAssignableFrom(safeType))
                        .ToArray();
                if (candidates.Count() > 1)
                {
                    Debug.Fail($"Multiple instances of type {typeof(T)} exist.");
                    xba = default;
                }
                else
                {
                    xba = candidates.FirstOrDefault();
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
        /// Attempts to retrieve an enum member from an Enum type T in the given XElement.
        /// </summary>
        /// <typeparam name="T">The enum type to parse.</typeparam>
        /// <param name="this">The XElement to retrieve the attribute from.</param>
        /// <param name="value">
        /// When this method returns, contains the parsed enum value if the attribute exists and is valid; 
        /// otherwise, the default value of T.
        /// </param>
        /// <param name="stringComparison">
        /// The string comparison method used for matching attribute names. Defaults to StringComparison.OrdinalIgnoreCase.
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

            if (@this.TryGetSingleBoundAttributeByType(out T aspirant))
            {
                value = aspirant;
                return true;
            }
            else
            {
                // The attribute name is expected to be the same as the enum type's name but in lowercase, 
                // and the value is stored as a case-sensitive string. This approach is used typically when 
                // the attribute is set using SetEnumValue(EnumType.Value) which writes the enum as a string.
                var attribute = @this
                    .Attributes()
                    .FirstOrDefault(attr => string.Equals(attr.Name.LocalName, type.Name, stringComparison));

                if (attribute != null && Enum.TryParse(attribute.Value, out T parsedValue))
                {
                    value = parsedValue;
                    return true;
                }
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
        /// Sorts the attributes of the given <see cref="XElement"/> and its descendants based on the names of an enum type.
        /// The attribute order follows the sequence of names in the specified enum.
        /// The sort order is determined using <see cref="Enum.GetNames(Type)"/> for the specified enum type.
        /// </summary>
        /// <typeparam name="T">An enum type whose names define the attribute order.</typeparam>
        /// <param name="this">The <see cref="XElement"/> whose attributes will be sorted.</param>
        /// <returns>The <see cref="XElement"/> with sorted attributes.</returns>
        public static XElement SortAttributes<T>(this XElement @this) where T : Enum
        {
            lock (_lock)
            {
                IsSorting = true;
                var dict = @this
                    .Attributes()
                    .ToDictionary(
                    attr => attr.Name.LocalName,
                    comparer: StringComparer.OrdinalIgnoreCase);
                @this.RemoveAttributes();
                foreach (var key in Enum.GetNames(typeof(T)))
                {
                    if (dict.TryGetValue(key, out var xattr))
                    {
                        @this.Add(xattr);
                        dict.Remove(key);
                    }
                }
                @this.Add(dict.Values);
                foreach (var xel in @this.Elements())
                {
                    xel.SortAttributes<T>();
                }
                IsSorting = false;
                return @this;
            }
        }
        private static object _lock = new object();
        internal static bool IsSorting { get; private set; }
    }
}
