## XBoundAttribute

A lightweight extension for `System.Xml.Linq` that provides a `Tag` property for `XAttribute`, enabling runtime object storage and action binding.

##### Overview
**XBoundAttribute** enhances `XAttribute` by introducing a **runtime-only** `Tag` property, allowing attributes to store and retrieve objects in a type-safe manner using `xel.To<T>()`. Note that while the `Tag` property can be visualized in-memory (e.g., when printed), it is not intended to be serializable. That is, objects attached at runtime are not reconstructed when the file is read back.

##### Features

- **Extended XAttribute Functionality** – Introduces a `Tag` property for `XAttribute`, enabling metadata enrichment
- **Runtime Object Storage** – Attach arbitrary objects or actions to XML attributes at runtime.
- **Action Binding** – Store and invoke actions associated with XML attributes, facilitating event-driven XML processing.
- **Enhanced XML Processing** – Ideal for scenarios requiring richer, context-aware attributes, such as serialization, templating, and dynamic transformations.

##### Use Cases
- **Dynamic XML Metadata** – Store additional information within attributes without modifying XML structures.
- **Templating & Transformation** – Bind runtime behaviors to XML attributes for dynamic rendering.
- **Workflow & Event Binding** – Attach callbacks to attributes, enabling event-driven XML interactions.
- **Cross-Platform Backing Store for Hierarchical Views** – Seamlessly models TreeView-like structures across WinForms, WPF, MAUI, Xamarin, and Web UIs.

XBoundAttribute bridges the gap between XML structures and runtime logic, making XML more powerful and adaptive in modern applications.

___

## New in Release 1.4

This release introduces a new feature and an important enhancement to improve your experience.

### New Feature: Placer

An instance of the Placer microclass allows for a single flat path to be efficiently placed in an XML runtime document. One example would be projecting a flat list of file names to a two-dimensional runtime XML structure. Options include useful values like `FindOrReplace` and the placement reports status including whether the element pre-existed. A placer instance can be invoked with inline lambda event handlers for before and after element additions, to gain real-time control over each step of the XML path traversal. Specifically, this is often an optimal hook for `SetBoundAttributeValue()` initializations.

### Enhancements: Working with `Enum` and `enum` Attribute Values

Named enum values are often used in conjuction with `XBoundObject`. They can, for example, be the `Key` members of a flat `Dictionary` to access the 2-dimensional runtime XML document. To complete the example, a button might hold the enum key as its ID, and clicking the button uses the ID to look up an `XElement`, and bound to it is an instance of a `View` class. Enumc can be the glue that holds everything together, so thay take on a certain importance in the framework.

#### Nullable Enums

When a single named enum is bound to an `XElement` e.g. by `xel.SetBoundAttributeValue(MyColors.Aqua)` it can be detected by `xel.Has<Enum>()` because it's the only one. It also works to use `xel.Has<MyColors>()`. But there is a critical distinction: the `Enum` class is nullable, so it's fine to use what we might call the "implicit try" of the `To<Enum>()` method because there's a default argument involved making the effective call `To<Enum>(@throw=false)`. In C# 9+ we might use this shortcut to test whether it exists:

```
if(xel.To<Enum>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```
We can do the same thing with named enum `MyColors` but one must be careful to make the request as nullable:

```
// Correctly returns null if not exists
if(xel.To<MyColors?>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```

If one sticks to these two patterns, everything is going to be fine. If one chooses, named enum values of `MyEnumA.Value` and `MyEnumB.Value` can coexist and be unambiguously retrieved using the second pattern.

#### Safety Enhancement

In previous versions, what was _not_ fine is to invoke the second pattern without indicating that the named enum is nullable. The problem is that `To<T>()` returns `default(T)`. For a named enum type, since `T` isn't nullable, this expression returned `true` (because by definition it _cannot_ return `null`) while at the same time the value (if `@throw` is `false`) might be an unintended default value. 

```
// Pathological case
if(xel.To<MyColors>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```

Version 1.4 introduces a safety feature that prevents this from ever occurring. The fix is simple: for any type that `IsEnum`, the `@throw` argument can automatically be elevated to `true`. This feature, activated by setting `EnumErrorReportOption.Throw`, is _disabled_ by default to avoid disrupting existing clients with unexpected exceptions. The tradeoff of this default setting is that occurences in code of the "pathological edge case" shown above will remain undetected. Existing implementations might encounter silent failures in the specific edge condition where T is a named enum value and @throw is false. 

### Recommendations

- Opt into this more robust error handling by setting the global `Compatibility.DefaultEnumErrorReportOption` to 'Throw'
- Always use the nullable operator with named enum values, e.g. `if(xel.To<MyColors?>() is { } myColor){...}` which avoids the issue altogether.

####

___
## Extension Methods in this Package

### XBound Object Extensions

```
/// <summary>
/// Fully qualified XBoundAttribute Setter
/// </summary>
public static void SetBoundAttributeValue(
    this XElement xel,
    object tag,
    string name = null,
    string text = null,
    SetOption options = SetOption.NameToLower){...}
```
___    

```
/// <summary>
/// Name supplied by user-defined standard enumerated names. 
/// </summary>
public static void SetBoundAttributeValue(
    this XElement xel,
    object tag,
    Enum stdName,    
    string text = null,
    SetOption options = SetOption.NameToLower){...}
```

___

```
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
public static T To<T>(this XElement xel, bool @throw = false){...}
```

___

```

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
    EnumParsingOption enumParsing){...}
```

___

```
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
    ){...}
```

___

```
```
___

```
/// <summary>
/// Return true if xel has any bound attribute of type T"/>
/// </summary>
public static bool Has<T>(this XElement xel){...}
```
___

```
/// <summary>
/// Returns the first ancestor that Has XBoundAttribute of type T.
/// </summary>
public static T AncestorOfType<T>(this XElement @this, bool includeSelf = false, bool @throw = false)
```
___

### XElement Extensions

This package also includes extended functionality for System.Xml.Linq that is not directly tied to XBoundObject.

```
/// <summary>
/// Sets an attribute on the given XElement using the name of the Enum type 
/// as the attribute name and the Enum value as the attribute value.
/// </summary>
/// <param name="this">The XElement to set the attribute on.</param>
/// <param name="value">The Enum value to store as an attribute.</param>
/// <param name="useLowerCaseName">If true, the attribute name will be the Enum type name in lowercase; otherwise, it will use the exact type name.</param>
public static void SetAttributeValue(this XElement @this, Enum value, bool useLowerCaseName = true){...}
```
___

```
/// <summary>
/// Tries to retrieve a single attribute of the specified type T from an XElement and assigns it to the out parameter.
/// </summary>
/// <remarks>
/// This method delegates error handling strategy to the <see cref="EnumErrorReportOption"/> specified by the caller.
/// For complete behavior details and error handling strategies, refer to the forwarded method documentation.
/// </remarks>
/// <param name="xel">The XElement to retrieve the attribute from.</param>
/// <param name="o">Out parameter to hold the result if successful.</param>
/// <param name="throw">If true, uses <see cref="EnumErrorReportOption.Throw"/> to enforce throwing an exception on failure. Otherwise, defaults to <see cref="EnumErrorReportOption.Default"/>, which follows the system-wide configuration set in <see cref="Compatibility.DefaultErrorReportOption"/>.</param>
/// <returns>True if the attribute was successfully retrieved; otherwise, false.</returns>
public static bool TryGetSingleBoundAttributeByType<T>(this XElement xel, out T o, bool @throw = false){...}}

```
---

```
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
public static bool TryGetSingleBoundAttributeByType<T>(this XElement xel, out T o, bool @throw = false){...}
```
---

```
/// <summary>
/// Tries to retrieve a single attribute of type T from the provided XElement, applying strict constraints based on the specified behavior.
/// This method is enhanced to allow configurable error reporting through the <see cref="EnumErrorReportOption"/> enumeration, reflecting a more granular control over how missing or multiple attributes are handled. It is particularly useful for applications transitioning from versions prior to 1.4, as it helps manage changes in error handling behaviors.
/// </summary>
/// <typeparam name="T">The expected type of the Tag property of the XBoundAttribute.</typeparam>
/// <param name="xel">The XElement from which to try and retrieve the attribute.</param>
/// <param name="o">The output parameter that will contain the value of the Tag if exactly one such attribute is found or none in case of SingleOrDefault behavior.</param>
/// <param name="enumErrorReporting">Specifies the error reporting strategy, affecting behavior when the target attribute is not found or multiple are encountered.</param>
/// <param name="caller">[Optional] The name of the calling method, used internally for debugging and logging purposes.</param>
/// <returns>True if exactly one attribute was found and successfully retrieved, otherwise false.</returns>
/// <exception cref="InvalidOperationException">Thrown when conditions specified by the <paramref name="enumErrorReporting"/> are not met.</exception>
/// <remarks>
/// The <paramref name="enumErrorReporting"/> parameter determines the operational mode:
/// - <see cref="EnumErrorReportOption.None"/>: Used internally when the calling method handles higher-level error reporting.
/// - <see cref="EnumErrorReportOption.Assert"/>: Provides an assertion failure for debugging purposes if a default value might be returned. Use cautiously as it allows silent failures in production.
/// - <see cref="EnumErrorReportOption.Throw"/>: Throws an exception if a default enum value might be inadvertently returned, ensuring robust error handling.
/// - <see cref="EnumErrorReportOption.Default"/>: Applies the default setting from <see cref="Compatibility.DefaultErrorReportOption"/>, allowing centralized control over error handling behavior. This is particularly important for transitioning existing applications from pre-1.4 versions, facilitating adjustments to the new error handling paradigm without extensive code modifications.
/// This method enforces strict attribute retrieval rules, distinguishing clearly between single and multiple attribute scenarios to maintain data integrity and prevent erroneous data usage.
/// </remarks>
public static bool TryGetAttributeValue<T>(
    this XElement @this, 
    out T value, 
    StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    where T : struct, Enum{...}
```
---

```
/// <summary>
/// Creates a shallow copy of the given XElement, preserving only its name and attributes,
/// but excluding its child elements.
/// </summary>
/// <param name="this">The XElement to copy.</param>
/// <returns>A new XElement with the same name and attributes, but without child elements.</returns>
public static XElement ToShallow(this XElement @this) {...}
```
---

```

/// <summary>
/// Creates a new XElement that includes only the specified attributes, removing all others.
/// </summary>
/// <param name="this">The XElement to filter.</param>
/// <param name="names">The attribute names to keep.</param>
/// <returns>A new XElement with only the specified attributes.</returns>
public static XElement WithOnlyAttributes(this XElement @this, params string[] names {...}
```
---

```

/// <summary>
/// Creates a new XElement that removes the specified attributes, keeping all others.
/// </summary>
/// <param name="this">The XElement to modify.</param>
/// <param name="names">The attribute names to remove.</param>
/// <returns>A new XElement without the specified attributes.</returns>
public static XElement WithoutAttributes(this XElement @this, params string[] names) {...}
```
---

```
/// <summary>
/// Sorts the attributes of the given <see cref="XElement"/> and its descendants based on the names of an enum type.
/// The attribute order follows the sequence of names in the specified enum.
/// The sort order is determined using <see cref="Enum.GetNames(Type)"/> for the specified enum type.
/// </summary>
/// <typeparam name="T">An enum type whose names define the attribute order.</typeparam>
/// <param name="this">The <see cref="XElement"/> whose attributes will be sorted.</param>
/// <returns>The <see cref="XElement"/> with sorted attributes.</returns>
public static IEnumerable<XElement> SortAttributes(this IEnumerable<XElement> @this, params string[] sortOrder) {...}
```
___

### Nested Enum Extensions

```
/// <summary>
/// Retrieves all descendant enum values related to the specified enum type.
/// This method identifies hierarchical relationships among "flat" enum groups
/// by searching for other enums that share names with the current enum values.
/// </summary>
public static IEnumerable<Enum> Descendants(
        this Type type,
        DiscoveryScope options = DiscoveryScope.ConstrainToAssembly | DiscoveryScope.ConstrainToNamespace){...}
```
___

```
/// <summary>
/// Constructs a hierarchical XML representation of an enum and its related enums,
/// effectively mapping "flat" enum structures into a nested format.
/// </summary>
public static XElement BuildNestedEnum(
        this Type type,
        DiscoveryScope options = DiscoveryScope.ConstrainToAssembly | DiscoveryScope.ConstrainToNamespace,
        string root = "root"){...}
```
___

```

/// <summary>
/// Generates a fully qualified string representation of an enum value,
/// including its type name and value.
/// </summary>
public static string ToFullKey(this Enum @this){...}

```
___

```
/// <summary>
/// Retrieve the concatenated ID member names from root to leaf.
/// From any ID, the XElement can be retrieved from the DKL.
/// </summary>
public static string ToFullIdPath(this XElement @this, char delim = '.') 

```
___

```
/// <summary>
/// Retrieve the concatenated ID member names from root to leaf.
/// From any ID, the XElement can be retrieved from the DKL.
/// </summary>
public static string ToFullIdPath(this Enum @this, DualKeyLookup dkl, char delim = '.')){...}
```
___

## Placer Class

```
/// <summary>
/// Manages XML element placement based on attribute-driven paths with configurable behavior 
/// for node creation and existence checks. Supports event-driven notifications for node manipulation 
/// and traversal, allowing for extensive customization and error handling in XML document modifications.
/// </summary>
/// <remarks>
/// The 'fqpath' argument is always assumed to be relative to an implicit root. Avoid setting the path 
/// attribute for this implicit root node, i.e. if the "text" attribute holds the label text for the
/// level, than the root node should not have a value for the "text" attribute.
/// </remarks>
/// <param name="fqpath">
/// Specifies the fully qualified path of the target XML element as a string. The path is used to navigate through the XML structure, 
/// where each segment of the path represents an element identified by its attribute value. This path should be delimited by the 
/// platform-specific `Path.DirectorySeparatorChar`, and is always assumed to be relative to the root element of the XML document.
/// </param>
/// <param name="onBeforeAdd">
/// Optional. An event handler that is invoked before a new XML element is added. Provides a chance to customize the addition process.
/// </param>
/// <param name="onAfterAdd">
/// Optional. An event handler that is invoked after a new XML element is added. Allows for actions to be taken immediately following the addition.
/// </param>
/// <param name="onIterate">
/// Optional. An event handler that is invoked as each path segment is processed, providing real-time feedback and control over the traversal.
/// </param>
/// <param name="mode">
/// Specifies the behavior of the Placer when path segments are not found. Default is PlacerMode.FindOrCreate.
/// </param>
/// <param name="pathAttributeName">
/// The name of the attribute used to match each XML element during the path navigation. Default is "text".
/// </param>
public class Placer
{
    public Placer(
        XElement xSource,
        string fqpath,  // String delimited using platform Path.DirectorySeparatorChar
        AddEventHandler onBeforeAdd = null,
        AddEventHandler onAfterAdd = null,
        IterateEventHandler onIterate = null,
        PlacerMode mode = PlacerMode.FindOrCreate,
        string pathAttributeName = "text"
    ){...}
```

___

```
    /// <summary>
    /// Initializes a new instance of the Placer class, allowing XML element placement using a pre-defined array of path segments. 
    /// This constructor is suited for scenarios where path segments are already determined and not bound to the platform's path delimiter.
    /// </summary>
    /// <param name="xSource">
    /// The root XML element from which path traversal begins.
    /// </param>
    /// <param name="parse">
    /// An array of strings representing the segments of the path to navigate through the XML structure. Each element of the array 
    /// represents one segment of the path, corresponding to an element identified by its attribute value.
    /// </param>
    /// <param name="onBeforeAdd">
    /// Optional. An event handler that is invoked before a new XML element is added. Provides a chance to customize the addition process.
    /// </param>
    /// <param name="onAfterAdd">
    /// Optional. An event handler that is invoked after a new XML element is added. Allows for actions to be taken immediately following the addition.
    /// </param>
    /// <param name="onIterate">
    /// Optional. An event handler that is invoked as each path segment is processed, providing real-time feedback and control over the traversal.
    /// </param>
    /// <param name="mode">
    /// Specifies the behavior of the Placer when path segments are not found. Default is PlacerMode.FindOrCreate.
    /// </param>
    /// <param name="pathAttributeName">
    /// The name of the attribute used to match each XML element during the path navigation. Default is "text".
    /// </param>
    public Placer(
        XElement xSource,
        string[] parse, // Array of strings (decoupled from any set delimiter) 
        AddEventHandler onBeforeAdd = null,
        AddEventHandler onAfterAdd = null,
        IterateEventHandler onIterate = null,
        PlacerMode mode = PlacerMode.FindOrCreate,
        string pathAttributeName = "text"
    ){...}
```
___

# Examples

### [Build Nested Enum Example](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/BuildNestedEnum.md)

Convert a set of flat enumerations and turn it into a runtime hierarchy.

___

### [XBound Clickable Objects](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/XBoundClickableObjects.md)


Using `xroot` from the prior example, iterate the XML, attach a clickable object to each node then use the ID to fire its click event. 

___


### [Dual Key Lookup](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/DualKeyLookup.md)

When an Enum type is expanded to an XML hierarchy, a two way lookup is xbound to the root node. A typical flow might be:

1. Use the `enum` "Friendly Name" to look up an `XElement` to which other objects are bound.
2. Use the `To<T>()` extension to retrieve objects or actions to perform. 

___


### [NotifyWithDescendants](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/NotifyOnDescendants.md)


___


### [Placer](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/Placer.md)



