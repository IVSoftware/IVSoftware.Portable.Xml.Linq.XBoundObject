using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject
{
    public static partial class Extensions
    {
        internal static EnumErrorReportOption EnumErrorReportOptionDisabled { get; } = 0;

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
        /// Converts an XElement to a specified type T, based on bound XML attributes with an option to
        /// throw an exception if the conversion fails. Otherwise, this method silently returns null if 
        /// the conversion fails and T is a reference type or if nullable T? is explicitly requested.
        /// </summary>
        /// <typeparam name="T">The type to which the XElement is to be converted.</typeparam>
        /// <param name="xel">The XElement to convert.</param>
        /// <param name="throw">Optional. If true, throws an exception when conversion is not successful. Defaults to false.</param>
        /// <returns>The converted object of type T, or default of T if unsuccessful and throw is false.</returns>
        /// <remarks>
        /// Version 1.4 adds a safety feature for when T is a named enum type and the attribute is not found.
        /// The issue arises because default(T) is returned in this case, giving the impression that the
        /// operation succeeded (attribute was found). This release takes the minimally invasive approach
        /// of warning the developer using a debug assert when this edge case occurs. THE EASY FIX when using
        /// named enum types is to always request them as nullable T? i.e. T = "MyNamedEnumType?"
        /// </remarks>
        public static T To<T>(
            this XElement xel,
            bool @throw = false)
        {
            // Uses strict rules for enums.
            if (xel.TryGetSingleBoundAttributeByType(out T result, out TrySingleStatus status))
            {
                return result;
            }
            else
            {
                string msg;
                switch (status)
                {
                    default:
                        msg = InvalidOperationNotFoundMessage<T>();
                        break;
                    case TrySingleStatus.FoundMany:
                        msg = InvalidOperationMultipleFoundMessage<T>();
                        break;
                }
                if (@throw)
                {
                    throw new InvalidOperationException(msg);
                }
                else
                {
                    if (Equals(default, null))
                    {   /* G T K */
                        // This is ideal including:
                        // - Enum (which is nullable)
                        // - Nullable named enum types.
                    }
                    else
                    {
                        // This assert is new in release 1.4 to bring our attention
                        // to the fact that a meaningless default named enum value
                        // is being returned for a non-existent attribute.
                        // THE EASY FIX: Use the nullable T? in this call instead.
                        Debug.Fail(msg);
                    }
                    return default;
                }
            }
        }

        /// <summary>
        /// Converts an XElement to a specified type T, based on bound XML attributes with an option to
        /// throw an exception if the conversion fails. For enum types, this method also allows specifying
        /// an enum parsing strategy. Otherwise, this method silently returns null if the conversion fails and
        /// T is a reference type or if nullable T? is explicitly requested.
        /// </summary>
        /// <typeparam name="T">The type to which the XElement is to be converted.</typeparam>
        /// <param name="xel">The XElement to convert.</param>
        /// <param name="enumParsingOption">Specifies the parsing strategy for enum types, ignored if T is not an enum.
        /// See <see cref="EnumParsingOption"/> for details on how each option modifies the parsing behavior.</param>
        /// <param name="throw">Optional. If true, throws an exception when conversion is not successful. Defaults to false.</param>
        /// <returns>The converted object of type T, or default of T if unsuccessful and throw is false.</returns>
        /// <remarks>
        /// For named enum values, this method first attempts to retrieve and convert an unambiguously matching XBoundAttribute. 
        /// If one is not found, the method then attempts to parse the attribute from a standard XAttribute using the specified
        /// rules. If the rule is strict, the custom enum type requires <see cref="IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlacementAttribute"/>
        /// otherwise the operation fails. If the rule is loose, the method attempts to locate a standard XAttribute using the 
        /// lower-case type as the attribute Name and if that is found uses the attribute value to parse the named enum value.
        /// </remarks>
        public static T To<T>(
            this XElement xel,
            EnumParsingOption enumParsingOption,
            bool @throw = false)
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (!type.IsEnum)
            {
                Debug.WriteLine($"ADVISORY: Ignoring {nameof(EnumParsingOption)} for non-enum type");
                return xel.To<T>(@throw);
            }
            if (enumParsingOption == EnumParsingOption.UseStrictRules)
            {
                return xel.To<T>(@throw);
            }
            // The attribute name is expected to be the same as the enum type's name but in lowercase.
            if (xel
                .Attributes()
                .FirstOrDefault(_ => string.Equals(
                        _.Name.LocalName,
                        type.Name, StringComparison.OrdinalIgnoreCase
                    )) is XAttribute attr)

            {
                StringComparison stringComparison;
                switch (enumParsingOption)
                {
                    case EnumParsingOption.FindUsingLowerCaseNameThenParseValue:
                        stringComparison = StringComparison.Ordinal;
                        break;
                    case EnumParsingOption.FindUsingLowerCaseNameThenParseValueIgnoreCase:
                        stringComparison = StringComparison.OrdinalIgnoreCase;
                        break;
                    default:
                        if (@throw) throw new Exception("Unexpected please report.");
                        Debug.Fail("Unexpected please report.");
                        return default;
                }
                foreach (var value in type.GetEnumValues())
                {
                    if (string.Equals(value.ToString(), attr.Value, stringComparison))
                    {
                        return (T)value;
                    }
                }
            }
            var msg = InvalidOperationNotFoundMessage<T>();
            if (@throw)
            {
                throw new InvalidOperationException(msg);
            }
            else
            {
                // This assert is new in release 1.4 to bring our attention
                // to the fact that a meaningless default named enum value
                // is being returned for a non-existent attribute.
                // THE EASY FIX: Use the nullable T? in this call instead.
                Debug.Fail(msg);
                return default;
            }
        }

        /// <summary>
        /// Determines whether the XElement has an attribute representing type T.
        /// - Returns true if a matching XBoundAttribute exists.
        /// - Returns true if T (or its underlying type, if nullable) is an enum decorated with [Placement(EnumPlacement.UseXAttribute)],
        ///   and the string attribute can be successfully parsed as a defined enum value.
        /// </summary>
        public static bool Has<T>(this XElement xel)
        {
            if(xel
            .Attributes()
            .OfType<XBoundAttribute>()
            .Any(_ => _.Tag is T))
            {
                return true;
            }
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if ( type.GetCustomAttribute<PlacementAttribute>() is PlacementAttribute pattr &&
                pattr.Placement == EnumPlacement.UseXAttribute)
            {
                var name = pattr.Name ?? type.Name.ToLower();
                return
                    xel.Attribute(name)?.Value is string value &&
                    type.GetEnumNames().Any(_=>string.Equals(_, value));
            }
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a single bound attribute of type T from the specified XElement without providing detailed status of the result.
        /// This overload simplifies the interface for scenarios where only the presence of a single attribute is of concern, returning a boolean indicating success. It is suited for situations where detailed result enumeration is not required, streamlining attribute retrieval while still leveraging the robust handling and precision control of the primary method.
        /// </summary>
        /// <typeparam name="T">The expected type of the Tag property of the XBoundAttribute.</typeparam>
        /// <param name="xel">The XElement from which to try and retrieve the attribute.</param>
        /// <param name="o">The output parameter that will contain the value of the Tag if one attribute is found; otherwise, it will be default.</param>
        /// <returns>True if exactly one attribute was found and successfully retrieved; otherwise, false.</returns>
        /// <remarks>
        /// This method calls the more detailed overload, discarding the status result, to provide a simplified interface with reduced complexity for clients not requiring detailed status feedback.
        /// </remarks>
        public static bool TryGetSingleBoundAttributeByType<T>(
                this XElement xel,
                out T o,
                [CallerMemberName] string caller = null
            ) => TryGetSingleBoundAttributeByType<T>(xel, out o, out TrySingleStatus _);

        /// <summary>
        /// Attempts to retrieve a single bound attribute of type T from the specified XElement, encapsulating the result's status in a structured manner.
        /// This method improves handling scenarios where the attribute may not be found or multiple matches may occur, returning a boolean indicating success and an enumeration for detailed status. It is designed to support robust error handling and precise control over attribute retrieval outcomes, facilitating migration and compatibility with different application versions.
        /// </summary>
        /// <typeparam name="T">The expected type of the Tag property of the XBoundAttribute.</typeparam>
        /// <param name="xel">The XElement from which to try and retrieve the attribute.</param>
        /// <param name="o">The output parameter that will contain the value of the Tag if one attribute is found; otherwise, it will be default.</param>
        /// <param name="result">An output parameter indicating the result of the attempt, such as FoundOne, FoundNone, or FoundMany, providing clear feedback on the operation's outcome.</param>
        /// <returns>True if exactly one attribute was found and successfully retrieved; otherwise, false.</returns>
        /// <remarks>
        /// The method enforces strict rules for attribute retrieval and differentiates clearly between scenarios of single and multiple attribute occurrences to maintain data integrity and prevent erroneous usage. The use of the TrySingleStatus enumeration provides additional clarity on the operation outcome, enhancing error handling and debugging capabilities.
        /// </remarks>
        public static bool TryGetSingleBoundAttributeByType<T>(
                this XElement xel, 
                out T o,
                out TrySingleStatus result
            )
        {
            var xbas = xel
               .Attributes()
               .OfType<XBoundAttribute>()
               .Where(_ => _.Tag is T)
               .ToArray();
            switch (xbas.Length)
            {
                case 0:
                    break;
                case 1:
                    o = (T)xbas[0].Tag;
                    result = TrySingleStatus.FoundOne;
                    return true;
                default:
                    result = TrySingleStatus.FoundMany;
                    o = default;
                    return false;
            }
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // - This option strictly requires [Placement(EnumPlacement.EnumUseXAttribute)]
            // - If enabled, provides for enum parsing from string.
            if (type.GetCustomAttribute<PlacementAttribute>() is PlacementAttribute pattr &&
                pattr.Placement == EnumPlacement.UseXAttribute)
            {
                var name = pattr.Name ?? type.Name.ToLower();
                if(xel.Attribute(name) is XAttribute xattr)
                {
                    if(xattr.Value is string stringValue)
                    {
                        foreach (var enumValue in type.GetEnumValues())
                        {
                            // Allow for splitting a FullKey JUST IN CASE we decide to allow this.
                            if(string.Equals(stringValue.Split('.').Last(), enumValue.ToString()))
                            {
                                o = (T)enumValue;
                                result = TrySingleStatus.FoundOne;
                                return true;
                            }
                        }
                    }
                }
            }
            // In a 'try' method this is not considered a failure condition!
            // - The returned boolean is false. This is correct.
            // - Yes, its true that a non-nullable enum will hold its default
            //   value. We're supposed to check the bool. That's the point.
            o = default;
            result = TrySingleStatus.FoundNone;
            return false;
        }

        /// <summary>
        /// Try get enum value using strict [Placement] attribute rules.
        /// </summary>
        public static bool TryGetAttributeValue<T>(
            this XElement xel, out T enumValue)
            where T : struct, Enum
            => xel.TryGetAttributeValue(out enumValue, EnumParsingOption.FindUsingLowerCaseNameThenParseValue);

        /// <summary>
        /// Try get enum value with opt-ins for loose enum parsing rules.
        /// </summary>
        public static bool TryGetAttributeValue<T>(
            this XElement xel, out T enumValue,
            EnumParsingOption enumParsingOption)
            where T : struct, Enum
        {
            if (enumParsingOption == EnumParsingOption.UseStrictRules)
            {
                return xel.TryGetSingleBoundAttributeByType(out enumValue);
            }

            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            // The attribute name is expected to be the same as the enum type's name but in lowercase.
            if (xel
                .Attributes()
                .FirstOrDefault(_ => string.Equals(
                        _.Name.LocalName,
                        type.Name, StringComparison.OrdinalIgnoreCase
                    )) is XAttribute attr)

            {
                StringComparison stringComparison;
                switch (enumParsingOption)
                {
                    case EnumParsingOption.FindUsingLowerCaseNameThenParseValue:
                        stringComparison = StringComparison.Ordinal;
                        break;
                    case EnumParsingOption.FindUsingLowerCaseNameThenParseValueIgnoreCase:
                        stringComparison = StringComparison.OrdinalIgnoreCase;
                        break;
                    default:
                        Debug.Fail("Unexpected please report.");
                        enumValue = default;
                        return false;
                }
                foreach (var value in type.GetEnumValues())
                {
                    if (string.Equals(value.ToString(), attr.Value, stringComparison))
                    {
                        enumValue = (T)value;
                        return true;
                    }
                }
            }
            // In a 'try' method this is not considered a failure condition!
            // - The returned boolean is false. This is correct.
            // - Yes, its true that a non-nullable enum will hold its default
            //   value. We're supposed to check the bool. That's the point.
            enumValue = default;
            return false;
        }

        /// <summary>
        /// Returns the first ancestor that Has XBoundAttribute of type T.
        /// </summary>
        public static T AncestorOfType<T>(this XElement @this, bool includeSelf = false, bool @throw = false)
        {
            if (@throw)
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
        /// Sets an attribute using the enum's type name or a custom name specified via [PlacementAttribute] as the attribute name,
        /// and the enum value as the attribute value. Uses XBoundAttribute if [Placement] specifies UseXBoundAttribute.
        /// </summary>
        public static void SetAttributeValue(this XElement @this, Enum value, bool useLowerCaseName = true)
        {
            var type = value.GetType();
            PlacementAttribute pattr = @type.GetCustomAttribute<PlacementAttribute>();
            if (pattr != null && pattr.Placement == EnumPlacement.UseXBoundAttribute) 
            {
                @this
                .SetBoundAttributeValue(
                    tag: value,
                    name: useLowerCaseName
                        ? pattr.Name?.ToLower() ?? type.Name.ToLower()
                        : pattr.Name ?? type.Name,
                    text: $"[{value.ToFullKey()}]"); // XBAs always use FullKey for this.
            }
            else
            {
                // There is an option to use FullKey for the value.
                string attrValue;
                if (pattr is null) attrValue = value.ToString();
                else
                {
                    attrValue = 
                        pattr.AlwaysUseFullKey
                        ? value.ToFullKey()
                        : value.ToString();
                }
                @this
                .SetAttributeValue(
                    useLowerCaseName
                    ? pattr?.Name?.ToLower() ?? type.Name.ToLower()
                    : pattr?.Name ?? type.Name,
                    attrValue);                   
            }
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

        internal static string InvalidOperationNotFoundMessage<T>() => $"No valid {typeof(T).Name} found. To handle cases where an enum attribute might not exist, use a nullable version: To<{typeof(T).Name}?>() or check @this.Has<{typeof(T).Name}>() first.";
        internal static string InvalidOperationMultipleFoundMessage<T>() => $@"Multiple valid {typeof(T).Name} found. To disambiguate them, obtain the attribute by name: Attributes().OfType<XBoundAttribute>().Single(_=>_.name=""targetName""";
    }
}
