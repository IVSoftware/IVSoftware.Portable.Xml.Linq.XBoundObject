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

When a single named enum is bound to an `XElement` e.g. by `xel.SetBoundAttributeValue(MyColors.Aqua)` it can be detected by `xel.Has<Enum>()` because it's the only one. It also works to use `xel.Has<MyColors>()`. But there is a critical distinction: the `Enum` class is nullable, so it's fine to use what we might call the "implicit try" of the `To<Enum>()` method because there's a default argument involved making the effective call `To<Enum>(@throw=false)`. In C# 9 and above we might use this shortcut to test whether it exists:

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
/// Converts an XElement to its corresponding type T, based on bound XML attributes with an option to
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

public static T To<T>(this XElement xel, bool @throw = false){...}
```

___

#### New in version 1.4

```
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

public static T To<T>(this XElement xel, EnumParsingOption enumParsingOption, bool @throw = false){...}
```
___

```
/// <summary>
/// Determines whether the XElement has an attribute representing type T.
/// - Returns true if a matching XBoundAttribute exists.
/// - Returns true if T (or its underlying type, if nullable) is a named enum
///   as determined using strict rules.
/// </summary>

public static bool Has<T>(this XElement xel){...}
```
___

#### New in version 1.4

```
/// <summary>
/// Determines whether the XElement has an attribute representing type T, with an option to
/// This method also allows specifying an enum parsing strategy.
/// - Returns true if a matching XBoundAttribute exists.
/// - Returns true if T (or its underlying type, if nullable) is a named enum
///   as determined using specified enumParsingOptions.
/// </summary>

public static bool Has<T>(this XElement xel, EnumParsingOption enumParsingOption){...}
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

public static void SetAttributeValue(
    this XElement @this,
    Enum value,
    bool useLowerCaseName = true){...}
```
___

```
/// <summary>
/// Attempts to retrieve a single bound attribute of type T from the specified XElement without providing detailed status of the result.
/// This overload simplifies the interface for scenarios where only the presence of a single attribute is of concern, returning a boolean indicating success. It is suited for situations where detailed result enumeration is not required, streamlining attribute retrieval while still leveraging the robust handling and precision control of the primary method.
/// </summary>
/// <typeparam name="T">The expected type of the Tag property of the XBoundAttribute.</typeparam>
/// <param name="xel">The XElement from which to try and retrieve the attribute.</param>
/// <param name="o">The output parameter that will contain the value of the Tag if one attribute is found; otherwise, it will be default.</param>
/// <returns>True if exactly one attribute was found and successfully retrieved; otherwise, false.</returns>
/// <remarks>
/// This method calls the more detailed overload, discarding the status result, to provide a simplified interface.
/// </remarks>

public static bool TryGetSingleBoundAttributeByType<T>(
    this XElement xel, 
    out T o){...}}
```
---

```
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
    out TrySingleStatus result){...}
```
---

```
/// <summary>
/// Try get named enum value.
/// - Priority is given to XBoundAttribute.
/// - Falls back to strict [Placement] attribute rules.
/// </summary>

public static bool TryGetAttributeValue<T>(
    this XElement xel, out T enumValue)
    where T : struct, Enum{...}
```
---

```
/// <summary>
/// Try get named enum value.
/// - Priority is given to XBoundAttribute.
/// - Falls back to strict or loose enum parsing option as specified.
/// </summary>
public static bool TryGetAttributeValue<T>(
    this XElement xel, out T enumValue,
    EnumParsingOption enumParsingOption)
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

## Placer Extensions

```
```

___

```
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



