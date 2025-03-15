#### XBoundAttribute

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

**XBound Object Extensions**

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
/// Return Single or Default where type is T. Null testing will be done by client.
/// </summary>
public static T To<T>(this XElement xel, bool @throw = false){...}
```

___

```
/// <summary>
/// Return true if xel has any attribute of type T"/>
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

**XElement Extensions**

This package also includes extended functionality for System.Xml.Linq that is not directly tied to XBoundObject.

```
/// <summary>
/// Sets an attribute on the given XElement using the name of the Enum type as the attribute name 
/// and the Enum value as the attribute value.
/// </summary>
/// <param name="this">The XElement to set the attribute on.</param>
/// <param name="value">The Enum value to store as an attribute.</param>
/// <param name="useLowerCaseName">If true, the attribute name will be the Enum type name in lowercase; otherwise, it will use the exact type name.</param>
public static void SetAttributeValue(this XElement @this, Enum value, bool useLowerCaseName = true){...}
```

___

```
/// <summary>
/// Retrieves an Enum value from an attribute on the given XElement. If the attribute is missing, 
/// it either throws an exception or returns a fallback value (-1 cast to T or a provided default).
/// </summary>
/// <typeparam name="T">The Enum type to retrieve.</typeparam>
/// <param name="this">The XElement to retrieve the attribute from.</param>
/// <param name="stringComparison">The string comparison method for matching attribute names.</param>
/// <param name="defaultValue">
/// Optional default value to return if the attribute is not found. 
/// This is not the same as default(T)! If null, -1 is used unless T already defines -1, 
/// in which case an exception is thrown.
/// </param>
/// <param name="throw">If true, throws an exception if the attribute is missing instead of returning a fallback value.</param>
/// <returns>The parsed Enum value of type T.</returns>
/// <exception cref="InvalidOperationException">Thrown if -1 is already a defined value in the Enum type T.</exception>
/// <exception cref="FormatException">Thrown if parsing the attribute value fails.</exception>
public static T GetAttributeValue<T>(
    this XElement @this,
    StringComparison stringComparison = StringComparison.OrdinalIgnoreCase,
    T? defaultValue = null, // [Careful] This is not the same as default T !!
    bool @throw = false)
    where T : struct, Enum {...}
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

**Nested Enum Extensions**

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

## Examples

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


### [NotifyWithDescendants](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/NotifyWithDescendants.md)


