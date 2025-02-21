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

**Nested Enum Extensions**

```
/// <summary>
/// Retrieves all descendant enum values related to the specified enum type.
/// This method identifies hierarchical relationships among "flat" enum groups
/// by searching for other enums that share names with the current enum values.
/// </summary>
public static IEnumerable<Enum> Descendants(
        this Type type,
        DiscoveryScope options = DiscoveryScope.ConstrainToAssembly | DiscoveryScope.ConstrainToNamespace){... }

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





