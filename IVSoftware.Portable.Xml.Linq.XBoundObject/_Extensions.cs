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
        /// Converts an XElement attribute to its corresponding type T.
        /// </summary>
        /// <typeparam name="T">The type to which the attribute should be converted. When T represents Enum or enum types, additional parsing features are available.</typeparam>
        /// <param name="xel">The XElement from which to retrieve and convert the attribute.</param>
        /// <param name="@throw">When true, enables more robust fault detection by throwing InvalidOperationException when T cannot be assigned.</param>
        /// <returns>The converted attribute of type T if successful, based on the specified Enum parsing option.</returns>
        /// <remarks>
        /// This method first attempts to retrieve and convert an attribute of type T that is bound as an XBoundAttribute. For Enum values, if no suitable XBoundAttribute is found, the method then attempts to parse the attribute from a standard XAttribute. In this case, the lower-case type is the attribute Name, and the method attempts to parse the case sensitive value.
        /// If `throw` is set to true and neither conversion succeeds, an exception is thrown. If `throw` is set to false, the method returns the default value of T, allowing silent handling of the absence or incorrect format of the expected attribute. For the special case of named enum types which are not nullable, version 1.4 brings new capabilities to avoid the inadvertent use of an incorrect default enum value when @throw is set to false. The preferred error handling when T is Enum or enum is EnumErrorReportOption.Throw (which will have priority over @throw=false when T is Enum or enum) but since this carries side-effects for pre-1.4 versions, to take advantage of this new safety feature change the `Compatibility.DefaultEnumErrorReportOption` from its default value of Assert to the more robust setting of Throw.
        /// </remarks>
        public static T To<T>(this XElement xel, bool @throw = false)
            => To<T>(
                xel,
                enumErrorReporting: @throw
                ? EnumErrorReportOption.Throw
                : EnumErrorReportOption.Default,
                enumParsing: EnumParsingOption.AllowEnumParsing);


        /// <summary>
        /// Converts an XElement attribute to its corresponding type T.
        /// </summary>
        /// <typeparam name="T">The type to which the attribute should be converted. When T represents Enum or enum types, additional parsing features are available.</typeparam>
        /// <param name="xel">The XElement from which to retrieve and convert the attribute.</param>
        /// <param name="enumParsing">Adds a non-default option to disable the fallback to string-based parsing for Enums when no XBoundAttribute is found.</param>
        /// <returns>The converted attribute of type T if successful, based on the specified Enum parsing option.</returns>
        /// <remarks>
        /// This method first attempts to retrieve and convert an attribute of type T that is bound as an XBoundAttribute. For Enum values, if no suitable XBoundAttribute is found, the method then attempts to parse the attribute from a standard XAttribute. In this case, the lower-case type is the attribute Name, and the method attempts to parse the case sensitive value.
        /// For the special case of named enum types which are not nullable, version 1.4 brings new capabilities to avoid the inadvertent use of an incorrect default enum value when a valid T value cannot be determined. This error reporting for enums in this overload defaults to `EnumErrorReportOption.Default` which is linked to `Compatibility.DefaultEnumErrorReportOption`. The preferred error handling when T is Enum or enum is EnumErrorReportOption.Throw (which will have priority over @throw=false when T is Enum or enum) but since this carries side-effects for pre-1.4 versions, to take advantage of this new safety feature change the `Compatibility.DefaultEnumErrorReportOption` from its default value of Assert to the more robust setting of Throw.
        /// </remarks>
        public static T To<T>(
            this XElement xel,
            EnumParsingOption enumParsing)
            => To<T>(
                xel,
                enumErrorReporting: EnumErrorReportOption.Default,
                enumParsing: enumParsing);

        /// <summary>
        /// Converts an XElement attribute to its corresponding type T, with comprehensive options for handling enums.
        /// </summary>
        /// <typeparam name="T">The type to which the attribute should be converted. Special handling is provided for Enum or enum types.</typeparam>
        /// <param name="xel">The XElement from which to retrieve and convert the attribute.</param><param name="enumErrorReporting">Designed specifically for named enum types, this parameter addresses unreported silent failures by enabling an option to throw exceptions when enums are set to default values because they are not nullable.</param>
        /// <param name="enumParsing">Specifies the parsing strategy for enums, allowing the disabling of fallback to string-based parsing when no XBoundAttribute is found. Defaults to allowing enum parsing.</param>
        /// <returns>The converted attribute of type T, successful based on the specified parsing and error reporting options.</returns>
        /// <remarks>
        /// This method first attempts to convert an attribute of type T from an XBoundAttribute within the XElement. If no suitable XBoundAttribute is found, particularly for Enum values, it defaults to parsing from a standard XAttribute using the attribute name as the key. 
        /// For the special case of named enum types which are not nullable, version 1.4 brings new capabilities to avoid the inadvertent use of an incorrect default enum value when a valid T value cannot be determined.  The preferred error handling when T is Enum or enum is EnumErrorReportOption.Throw but carries the potential of side-effects for existing pre-1.4 version clients. To manage this this new safety feature globally, change the `Compatibility.DefaultEnumErrorReportOption` from its default value of Assert to the more robust setting of Throw, and use EnumErrorReportOption.Default as the argument to this method.
        /// Version 1.4 introduces a safety feature specifically for non-nullable named enum types, designed to detect incorrect default enum values when a valid T cannot be established. This feature, activated by setting `EnumErrorReportOption.Throw`, is not enabled by default to avoid disrupting existing clients with unexpected exceptions. Existing implementations might encounter silent failures in the specific edge case where T is a named enum value and @throw is false. To opt into this more robust error handling, set the global `Compatibility.DefaultEnumErrorReportOption` to 'Throw', thus enabling the feature across your application. Ensure that your application can handle these new exceptions appropriately.
        /// </remarks>
        public static T To<T>(
                this XElement xel,
                EnumErrorReportOption enumErrorReporting,
                EnumParsingOption enumParsing = EnumParsingOption.AllowEnumParsing
            )
        {
            if (Equals(enumErrorReporting, EnumErrorReportOption.Default))
            {
                enumErrorReporting = Compatibility.DefaultErrorReportOption;
            }
            var type = typeof(T);

            if (xel.TryGetSingleBoundAttributeByType(out T result))
            {
                return result;
            }
            else
            {
                // Try the Enum special-case fallback.
                if (localTryGetParsedEnum(out T parsedEnum))
                {
                    return parsedEnum;
                }
                return default;
            }
            bool localTryGetParsedEnum(out T parsedEnum)
            {
                Type nullableAspirantType = Nullable.GetUnderlyingType(typeof(T));
                Type safeType = nullableAspirantType ?? typeof(T);
                if(safeType.IsEnum)
                {
                    if (Equals(enumParsing, EnumParsingOption.AllowEnumParsing))
                    {
                        // The attribute name is expected to be the same as the enum type's name but in lowercase, 
                        // and the value is stored as a case-sensitive string. This approach is used typically when 
                        // the attribute is set using SetEnumValue(EnumType.Value) which writes the enum as a string.
                        if (xel
                            .Attributes()
                            .FirstOrDefault(_ => string.Equals(
                                    _.Name.LocalName,
                                    safeType.Name, StringComparison.OrdinalIgnoreCase
                                )) is XAttribute attr)

                        {
                            foreach (var value in safeType.GetEnumValues())
                            {
                                if (string.Equals(value.ToString(), attr.Value))
                                {
                                    parsedEnum = (T)value;
                                    return true;
                                }
                            }
                        }
                    }
                    if (nullableAspirantType is null)
                    {
                        switch (enumErrorReporting)
                        {
                            case 0:
                                break;
                            case EnumErrorReportOption.Assert:
                                // This IS going to return an unintended default enum value.
                                // The tradeoff is, we don't want to risk crashing a pre-1.4 app by throwing the exception.
                                // IDEALLY: Set the Compatibility.DefaultEnumErrorReportOption to Throw.
                                Debug.Fail(InvalidOperationNotFoundMessage<T>());
                                break;
                            case EnumErrorReportOption.Throw:
                                throw new InvalidOperationException(InvalidOperationNotFoundMessage<T>());
                            default:
                                Debug.Fail("Unexpected");
                                break;
                        }
                    }
                    else
                    {   /* G T K */
                        // This will be returning null for the nullable enum type as requested!
                    }
                }
                parsedEnum = default;
                return false;
            }
        }
        internal static string InvalidOperationNotFoundMessage<T>() => $"No valid {typeof(T).Name} found. To handle cases where an enum attribute might not exist, use a nullable version: To<{typeof(T).Name}?>() or check @this.Has<{typeof(T).Name}>() first.";
        internal static string InvalidOperationMultipleFoundMessage<T>() => $@"Multiple valid {typeof(T).Name} found. To disambiguate them, obtain the attribute by name: Attributes().OfType<XBoundAttribute>().Single(_=>_.name=""targetName""";

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
            if (xbas.SingleOrDefault(_ => _.Tag is T) is XBoundAttribute xba)
            {
                o = (T)xba.Tag;
                result = TrySingleStatus.FoundOne;
                return true;
            }
            else 
            {
                if (xbas.Length > 1)
                {
                    result = TrySingleStatus.FoundMany;
                    o = default;
                    return false;
                }
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

            // Under no circumstances is this even considered a failure condition!
            // - The returned boolean is false. This is correct.
            // - Yes, its true that a non-nullable enum will hold its default
            //   value. But you're supposed to check the bool. That's the point.
            o = default;
            result = TrySingleStatus.FoundNone;
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

        public static bool TryGetAttributeValue<T>(
            this XElement @this,
            out T value,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase,
            EnumErrorReportOption errorReporting = EnumErrorReportOption.Default,
            bool useStrictEnumParsing = false)
        where T : struct, Enum
        {
            var type = typeof(T);

            value = default;

            // Uses strict enum parsing by default
            if (@this.TryGetSingleBoundAttributeByType(
                out T aspirant,
                out TrySingleStatus status))
            {
                value = aspirant;
                return true;
            }
            else if(!useStrictEnumParsing)
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
