using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// Return Single or Default where type is T. Null testing will be done by client.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static T To<T>(this XElement xel, bool @throw = false)
            => To<T>(
                xel,
                enumErrorReporting: @throw
                ? EnumErrorReportOption.Throw
                : EnumErrorReportOption.Default,
                enumParsing: EnumParsingOption.AllowEnumParsing);

        /// <summary>
        /// Return Single or Default where type is T. Null testing will be done by client.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static T To<T>(
            this XElement xel,
            EnumParsingOption enumParsing)
            => To<T>(
                xel,
                enumErrorReporting: EnumErrorReportOption.Default,
                enumParsing: enumParsing);

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
            // Try, but don't throw yet!
            if (xel.TryGetSingleBoundAttributeByType(out T result, enumErrorReporting: EnumErrorReportOption.None))
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
                            case EnumErrorReportOption.None:
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
        /// See full documentation on forwarded method.
        /// </remarks>
        public static bool TryGetSingleBoundAttributeByType<T>(this XElement xel, out T o, bool @throw = false)
            => TryGetSingleBoundAttributeByType<T>(
                xel, 
                out o,
                enumErrorReporting: @throw
                ? EnumErrorReportOption.Throw
                : EnumErrorReportOption.Default
        );


        /// <summary>
        /// Tries to retrieve a single attribute of type T from the provided XElement, enforcing strict constraints based on the specified behavior.
        /// This method targets attributes of type XBoundAttribute with a Tag property of type T, ensuring either the existence of exactly one such attribute (Single behavior),
        /// or tolerating the absence of such attributes while ensuring no multiples exist (SingleOrDefault behavior).
        /// </summary>
        /// <typeparam name="T">The expected type of the Tag property of the XBoundAttribute.</typeparam>
        /// <param name="xel">The XElement from which to try and retrieve the attribute.</param>
        /// <param name="o">The output parameter that will contain the value of the Tag if exactly one such attribute is found or none in case of SingleOrDefault behavior.</param>
        /// <param name="throw">If true, operates in 'Single' mode where the absence or multiplicity of attribute results in an InvalidOperationException. If false, operates in 'SingleOrDefault' mode where only multiplicity results in an exception.</param>
        /// <returns>True if exactly one attribute was found and successfully retrieved, otherwise false if no attributes are found and 'throw' is false.</returns>
        /// <exception cref="InvalidOperationException">Thrown when conditions for the selected mode ('Single' or 'SingleOrDefault') are not met:
        /// 1. In 'Single' mode, if no attribute or multiple attributes of type T are found.
        /// 2. In 'SingleOrDefault' mode, if multiple attributes of type T are found.</exception>
        /// <remarks>
        /// The 'throw' parameter determines the operational mode:
        /// - 'Single': Requires exactly one matching attribute. An exception is thrown for no match or multiple matches.
        /// - 'SingleOrDefault': Allows zero or one matching attribute. An exception is thrown only for multiple matches.
        /// This ensures that the method name "TryGetSingleBoundAttributeByType" accurately reflects its functionality by clearly defining the outcome expectations based on the operational mode.
        /// </remarks>

        public static bool TryGetSingleBoundAttributeByType<T>(
                this XElement xel, 
                out T o,
                EnumErrorReportOption enumErrorReporting,
                EnumParsingOption enumParsing = EnumParsingOption.AllowEnumParsing,
                [CallerMemberName] string caller = null
            )
        {
            if (Equals(enumErrorReporting, EnumErrorReportOption.Default))
            {
                enumErrorReporting = Compatibility.DefaultErrorReportOption;
            }
            if (Equals(enumErrorReporting, EnumErrorReportOption.Throw))
            {
                try
                {
                    var single = xel
                        .Attributes()
                        .OfType<XBoundAttribute>()
                        .Single(_ => _.Tag is T);
                    // And if this does not throw...
                    o = (T)single.Tag;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException(InvalidOperationNotFoundMessage<T>());
                }
            }
            else
            {
                var type = typeof(T);
                var candidates =
                    xel
                    .Attributes()
                    .OfType<XBoundAttribute>()
                    .Where(_ => _.Tag is T);
                switch (candidates.Count())
                {
                    case 0:
                        o = default;
                        if (type.IsEnum)
                        {
                            switch (enumErrorReporting)
                            {
                                case EnumErrorReportOption.None:
                                    switch (caller)
                                    {
                                        case nameof(To):
                                        case nameof(TryGetAttributeValue):
                                            // Expected!
                                            break;
                                        default:
                                            Debug.Fail(
                                                $"ADVISORY: Unexpected call from: '{caller}'. {EnumErrorReportOption.None.ToFullKey()} is intended for internal use only.");
                                            break;
                                    }
                                    break;
                                case EnumErrorReportOption.Assert:
                                    Debug.Fail(InvalidOperationNotFoundMessage<T>());
                                    // But to avoid crashing pre-1.4 apps, return a value that we know is probably wrong!
                                    break;
                                case EnumErrorReportOption.Throw:
                                    throw new InvalidOperationException(InvalidOperationNotFoundMessage<T>());
                            }
                        }
                        return false;
                    case 1:
                        o = (T)candidates.First().Tag;
                        return true;
                    default:
                        if (Equals(enumErrorReporting, EnumErrorReportOption.Assert))
                        {
                            Debug.Fail(InvalidOperationNotFoundMessage<T>());
                            // But to avoid crashing pre-1.4 apps, return a value that we know is probably wrong!
                            o = (T)candidates.First().Tag;
                            return false;
                        }
                        else throw new InvalidOperationException(InvalidOperationMultipleFoundMessage<T>());
                }
            }
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
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase,
            EnumParsingOption enumParsing = EnumParsingOption.AllowEnumParsing,
            EnumErrorReportOption errorReporting = EnumErrorReportOption.Default)
        where T : struct, Enum
        {
            var type = typeof(T);
            value = default;

            if (@this.TryGetSingleBoundAttributeByType(out T aspirant, enumErrorReporting: EnumErrorReportOption.None))
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
