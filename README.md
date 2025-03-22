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
An instance of Placer class allows for efficient dynamic path-based XML element placement. For example, it would be ideal for projecting a flat list of file names to a two-dimensional runtime XML structure. Options include useful values like `FindOrReplace` and the placement reports status including whether the element pre-existed. A placer instance can be invoke with inline lambda event handlers for before and after element additions, to gain real-time control over each step of the XML path traversal. Specifically, this is often an optimal hook for `SetBoundAttributeValue()` initializations.

### Enhancement: Working with `Enum` and `enum` Attribute Values
This release significantly improves how named enum values are retrieved from an XBoundAttribute, addressing an important issue where a default enum value was used unintentionally in a specific edge case, as detailed in the code below. Both code blocks aim to retrieve an `enum` value for `NamedEnumType`:

1. When there is only one such attribute bound to a given `XElement`, it suffices to cast it to `Enum`. This remains a safe pattern to use even if no such attribute can be found.

```csharp
if(xel.To<Enum>() is NamedEnumType enumValue) 
{
    // Code Based on enum NamedEnumType.Value
}
```

2. When the possibility of multiple enum attributes exists, a disambiguating pattern might be used instead.

if(xel.To<NamedEnumType>() is NamedEnumType enumValue) 
{
    // Code Based on enum NamedEnumType.Value
}

In previous releases, this second pattern has been shown to be unsafe when no bound attribute of type `NamedEnumType` can be located. In this specific case:
- The boolean cause incorrectly evaluates to `true` even when no such attribute exists
- The `enumValue` will be set to default value for the `enum` potentially causing spurious failures.
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

**Placer Class**

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



