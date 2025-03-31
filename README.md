## XBoundAttribute

A lightweight extension for `System.Xml.Linq` that provides a `Tag` property for `XAttribute`, enabling runtime object storage and action binding.

##### Overview
**XBoundAttribute** enhances `XAttribute` by introducing a **runtime-only** `Tag` property, allowing attributes to store and retrieve objects in a type-safe manner using `xel.To<T>()`. Note that while the `Tag` property can be visualized in-memory (e.g., when printed), it is not intended to be serializable. That is, objects attached at runtime are not reconstructed when the file is read back.

___

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

## New Changes in Release 2.0

- High level extensions `FindOrCreate()` and `FindOrCreate<T>()` for `Placer` class put it on steroids.

- `ViewContext` class and `IXBoundViewObject` interface make direct-linking to an `ObservableCollection<T>` possible for platform-agnostic view sources.

- High level _View_ extensions `Show()`, `Show<T>()`, `Expand()`, and `Collapse()` for `Placer` class to fasttrack UI development for Maui, WinForms, WPF, etc.

___

### Changes in Release 1.4

This release introduces a new XML placement feature and extended support for named enums. It also adds a new debug assertion for detecting an edge case that can occur when using named enums in conjunction with the `To<T>()` method.

### New Feature: Placer

A common requirement when working with `System.Xml.Linq` is to find or place an `XElement` instance in the runtime XML document based on a flat path descriptor string (e.g. a filename). The example below demonstrates how using the `Place` extension can make the fully featured `Placer` class simple to use.

```
public void TestBasicPlacement()
{
    // Test setup
    string actual, expected;
    var xroot = new XElement("root");

    // Perform a placement
    var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
    actual = xroot.Place(path, out XElement xelnew).ToFullKey();
    expected = "PlacerResult.Created";

    actual = xroot.ToString();
    expected = @" 
<root>
    <xnode text=""C:"">
        <xnode text=""Child Folder"">
            <xnode text=""Leaf Folder"" />
        </xnode>
    </xnode>
</root>";

    // Inspect the newly created 'out XElement' value
    actual = xelnew.ToShallow().ToString();
    expected = @"<xnode text=""Leaf Folder"" />";
}
```


The content of the newly created `XElement` can be modified using optional parameters to streamline this process. The content can be supplied directly in the `Place` call using any number of optional parameters supplied in any order. See the [Optional Parameters](#optional-parameters) section for a quick-start guide to using this feature.
___

### Enhancements: Working with `Enum` and `enum` Attribute Values

Named enum values are often used in conjuction with `XBoundObject` and can take it to a higher level. They can, for example, be the `Key` members of a flat `Dictionary` to access the 2-dimensional runtime XML document. 

Consider a button with an ID property that holds an enum key, and where clicking the button uses the ID to look up an `XElement`. Now that it has the element, it might use a construct like `xel.To<View>()` and show an XBound UI in reponse. Given this kind of far-reaching capability, supporting the manipulation of named enums is vital to the framework and well supported in the extensions.

___

#### Nullable Enums

When a single named enum is bound to an `XElement` e.g. by `xel.SetBoundAttributeValue(MyColors.Aqua)` it can be detected by `xel.Has<Enum>()` because it's the only one. It also works to use `xel.Has<MyColors>()`. But there is a critical distinction: the `Enum` class is nullable, so it's fine to use what we might call the "implicit try" of the `To<Enum>()`. In this case, the method's `@throw` argument defaults to false, making the effective call `To<Enum>(@throw=false)`. 

Here's what a typical call might look like using C# 9 and above.

```
if(xel.To<Enum>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```
So far, everything works as expected because when `T` is `Enum` it's nullable and `null` is the default returned by `To<Enum>()`. However, this wouldn't be the case when `T` is a named `enum` type because such types aren't nullable. This means that it's critical to use the nullable operator for `T?` so that this delivers the intended result:

```
// Correctly returns null if not exists
if(xel.To<MyColors?>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```

If one sticks to these two patterns, everything is going to be fine. If one chooses, named enum values of `MyEnumA.Value` and `MyEnumB.Value` can coexist and be unambiguously retrieved using the second pattern.

___

#### Safety Enhancement

So the question is, what happens if this nullable operator is left out accidentally? In previous versions, the way this edge case behaves is:

1. The `To<T>()` method returns `default(T)` for a non-existent attribute, possibly deceiving us into thinking it exists.
2. Since the default is to 'not' throw, previous versions would act as though the call succeeded because the method was returning an instance of `T` even though the default value it was returning wasn't meaningful.
 
For previous versions to ever allow this without a debug assert is a bug, and this release fixes it. The developer will now be notified when this happens, but only when running in `Debug` mode because (out of an abundance of caution) the reporting level is `Debug.Assert` not `throw`. This is a tradeoff of course. Most importantly, the new release isn't allowed to actually _crash_ your app by throwing new exceptions where it wasn't before. And the presence of a debug assert should allow your Unit Testing to detect occurrences where previously it couldn't. But the thing is, this _will_ require a little bit of testing, because in `Release` mode (where debug asserts are ignored) this is still essentially a silent spurious failure in this narrow edge case. 

```
// Pathological case
if(xel.To<MyColors>() is { } myColor)
{
    Debug.WriteLine(myColor.ToString());
}
```
___
### Recommendation

_Always use the nullable operator with named enum values, e.g. `if(xel.To<MyColors?>() is { } myColor){...}` which avoids the issue altogether._

___

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

#### New fluent versions in release 2.0

```
/// <summary>
/// Fluent version
/// </summary>
public static XElement WithBoundAttributeValue(
    this XElement @this,
    object tag,
    string name = null,
    string text = null,
    SetOption options = SetOption.NameToLower){...}
```

___

```
/// <summary>
/// Fluent version
/// </summary>
public static XElement WithBoundAttributeValue(
    this XElement @this,
    object tag,
    Enum stdName,
    string text = null,
    SetOption options = SetOption.NameToLower)
{
    @this.SetBoundAttributeValue(tag, stdName, text, options);
    return @this;
}{...}
```

___

```
/// <summary>
/// Converts an XElement to its corresponding type T, based on bound XML attributes with an option to
/// throw an exception if the conversion fails. Otherwise, this method silently returns null if 
/// the conversion fails and T is a reference type or if nullable T? is explicitly requested. Most commonly
/// success is checked inline with `is` e.g. in C# 12 it might be if(xel.To<MyClass>() is { } myClass)){ ... }
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

#### New in Release 1.4

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

#### New in release 1.4

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
/// Constructs a path by traversing the element and its ancestors, collecting the value
/// of the specified attribute from each. If the attribute is null, it defaults to 
/// <see cref="DefaultStdAttributeName.text"/>.
/// The <paramref name="pathAttribute"/> parameter accepts any user-defined enum, 
/// such e.g. 'StdAttributeNames'. This enum can also be reused with related 
/// methods like SortAttributes.
/// </summary>
public static string GetPath(this XElement @this, Enum pathAttribute){...}
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

The `Place` method in the `Extensions` class allows for dynamic configuration and manipulation of XML elements within an existing XML structure. This method is designed to handle various configurations through optional parameters, enhancing flexibility and allowing detailed customization during the XML manipulation process.


```
/// <summary>
/// Places or modifies an XML element at a specified path within the XML structure of the source element, 
/// allowing for dynamic configuration through additional parameters. This method simplifies XML manipulations 
/// by optionally configuring the element's attributes and values during the placement process, without returning the modified or created element.
/// </summary>

public static PlacerResult Place(
    this XElement source,
    string path,
    params object[] args){...}
```

___

```
/// <summary>
/// Places or modifies an XML element at a specified path within the XML structure of the source element. This method 
/// allows dynamic configuration through additional parameters and returns the newly created or modified XML element.
/// It supports complex configurations including attribute settings and event handling, facilitating detailed control over the XML manipulation process.
/// </summary>
public static PlacerResult Place(
    this XElement source,
    string path,
    out XElement xel,
    params object[] args){...}
```
___

### Optional Parameters

Using the 'out' value, the content of the newly created `XElement` can be modified using standard `System.Xml.Linq` as well as `XBoundObject` methods in the usual manner. Optional parameters can streamline this process however, by allowing the content can be supplied direcly in the `Place` call using any number of optional parameters suppied in any order:

1. **PlacerMode**: This enum dictates the placement behavior, choosing between creating new elements if they do not exist or finding and returning existing ones.

2. **Dictionary of `StdPlacerKeys` to `string`**: Enables dynamic setting of properties such as the name of a new XML element (`NewXElementName`) and the attribute name (`PathAttributeName`) used to navigate through the XML structure. This customization is applied during runtime based on the needs of the operation.

3. **Attribute Configurations (`XAttribute` and instances of `XBoundAttribute`)**: These parameters facilitate the addition of new attributes to the XML elements being placed or modified. They can include standard XML attributes or instances of `XBoundAttribute`, which might carry additional metadata or behavior definitions.

4. **Enum with Custom Attribute (`EnumPlacement`)**: Facilitates the use of enums in a way that their values can be applied directly as attribute values on XML elements, according to their defined placement strategy (using the enum value as an XML attribute or a bound attribute).

5. **XElement** For scenarios where the `XElement` to be inserted is already instantiated (this might be the removed node of a drag drop, for example), this node will be substituted instead of created. 

6. **Value**: After filtering any of the above types, any remaining single value of any other type will be set as the XElement value e.g. `<xel>Value</xel>`.

These arguments are auto-detected by type and can be supplied in any sequence to make the `Place` method extremely versatile.

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



