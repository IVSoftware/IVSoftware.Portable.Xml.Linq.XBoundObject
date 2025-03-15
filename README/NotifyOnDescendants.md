[◀](../README.md)

### **NotifyOnDescendants: Observable Nested Objects**

This extension creates and transparently maintains a dynamic model of an arbitrary class or collection by aggregating the `INotifyPropertyChanged` and `INotifyCollectionChanged` events of any descendants of the `Property` hierachy. This model automatically subscribes a common handler to _any member at any nested level_ that exists or that arrives, and manages their unsubscription when removed from the model (without assuming that the removed object will go out of existence and be disposed independently).

#### **Overview**

When `ObservableCollection<T>` is used with platforms like WPF and Maui, the data models therein contained are often directly bound to a `DataTemplate` by implementing `INotifyPropertyChanged`. Most of the time, there's simply no need for the observable collection to react to these `PropertyChanged` events. If for some reason the collection is expected to issue aggregated change events when its items invoke `PropertyChanged`, the `BindingList<T>` class takes things one step farther and provides this functionality provided that the hierarchy is flat.

This extension brings about a fundamentally different approach:

- Works with classes, not just collections.
- Does not require subclassing the class or collection.
- Works with member properties _and any descendants_ of those properties.
- Works with `INotifyCollectionChanged` not just `INotifyPropertyChanged`.

To put it another way, we can add `ClassA` to an observable collection where `ClassA.B` is an instance of `ClassB` and `ClassB.C` is an instance of `ClassC`. When a bindable property in `ClassC` changes, the aggregate handler will receive an event from three levels down. 

##### Required Condition

- `ClassC` implements `INotifyPropertyChanged` and notified on that specific property.

##### Things That Don't Matter

- Works even if intermediate classes like `ClassA` and `ClassB` are 'not' INPC.
- Works even if a nested property is declared as a type (e.g. `object`) that doesn't inherently notify and isn't inherently observable.

___
### **Practical Use Cases**

The `NotifyOnDescendants` extension offers significant advantages in a variety of applications where complex data models and dynamic content management are crucial. Here are some key use cases:

- **Complex Data Models in UI Applications**: Ideal for frameworks like WPF or Maui, this extension manages notifications across complex nested data structures, facilitating real-time UI updates without extensive manual notification setup.

- **Dynamic Content Applications**: Enhances applications with frequently changing content (e.g., adding or removing items from collections), by ensuring all nested objects are appropriately managed for event notifications.

- **Form and Survey Applications**: Simplifies the development of dynamic forms or surveys with nested structures (such as sections and questions), by automating the change tracking throughout the survey's hierarchy.

- **Financial Applications**: Provides a robust solution for financial systems where nested structures like portfolios and transactions are common, automating the propagation of changes that affect computations or display in different parts of the application.

- **Game Development**: Supports game development scenarios where game states and properties are nested and reactive updates are necessary, streamlining state management across various game object levels.


___

**High-level Observability Extensions**

```
/// <summary>
/// Attaches notification delegates to the descendants of the given object,
/// allowing for property changes, collection changes, and object changes to be monitored.
/// </summary>
/// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
/// <param name="this">The object whose descendants should be monitored.</param>
/// <param name="model">The XElement representing the model, returned as an output parameter.</param>
/// <param name="onPC">The delegate to handle property change notifications (required).</param>
/// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
/// <param name="onXO">The delegate to handle object change notifications (optional).</param>
/// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
public static T WithNotifyOnDescendants<T>(
    this T @this,
    out XElement model,
    PropertyChangedDelegate onPC,
    NotifyCollectionChangedDelegate onCC = null,
    XObjectChangeDelegate onXO = null,
    ModelingOption options = 0){...}
```
___

```
/// <summary>
/// Attaches notification delegates to the descendants of the given object, 
/// allowing property, collection, and object changes to be monitored. 
/// This overload discards the model output.
/// </summary>
/// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
/// <param name="this">The object whose descendants should be monitored.</param>
/// <param name="onPC">The delegate to handle property change notifications (required).</param>
/// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
/// <param name="onXO">The delegate to handle object change notifications (optional).</param>
/// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
public static T WithNotifyOnDescendants<T>(
    this T @this,
    PropertyChangedDelegate onPC,
    NotifyCollectionChangedDelegate onCC = null,
    XObjectChangeDelegate onXO = null,
    ModelingOption options = 0){...}
```
___

**Medium-level Discovery Enumerator and Extensions**

```
/// <summary>
/// Recursively models the properties of an object and its descendants into XML elements, managing each modeled property 
/// and collection with respect to the provided or default modeling context. This method dynamically tracks changes and 
/// raises an event when new nodes are added to the model. It handles both simple properties and enumerable collections,
/// applying specified modeling options such as type name verbosity, caching of property info, and inclusion of value types.
/// </summary>
/// <param name="this">The object to model.</param>
/// <param name="context">The modeling context to use, which controls the behavior of the modeling process.</param>
/// <returns>An enumerable of XElement, each representing a modeled property or object.</returns>
public static IEnumerable<XElement> ModelDescendantsAndSelf(this object @this, ModelingContext context = null){...}
```
___

```
/// <summary>
/// Creates a model of the object and its properties as XML elements, using a default sorting order.
/// </summary>
/// <param name="this">The object to model.</param>
/// <param name="context">The modeling context to use, which controls the behavior of the modeling process.</param>
/// <returns>The root XElement of the modeled object, with attributes sorted in default order.</returns>
public static XElement CreateModel(this object @this, ModelingContext context = null){...}
```
___

```
/// <summary>
/// Creates a model of the object and its properties as XML elements, allowing for custom attribute sorting based on the specified enum.
/// </summary>
/// <typeparam name="T">The enum type used to define the sorting order of attributes in the model.</typeparam>
/// <param name="this">The object to model.</param>
/// <param name="context">The modeling context to use, which controls the behavior of the modeling process.</param>
/// <returns>The root XElement of the modeled object, with attributes sorted according to the specified enum type.</returns>
public static XElement CreateModel<T>(this object @this, ModelingContext context = null){...}
```
___

```
/// <summary>
/// Refreshes the model represented by an XElement by removing all existing descendant elements and attributes
/// that do not represent structural identifiers, then recreates the model based on a new value. This method
/// is useful for updating the model representation in response to significant changes in the underlying data.
/// </summary>
/// <param name="model">The XElement that represents the model to be refreshed.</param>
/// <param name="newValue">The new value to use for recreating the model.</param>
public static void RefreshModel(this XElement model, object newValue){...}
```
___

**Low-level XBoundAttribute Extensions**

```
/// <summary>
/// Retrieves the object instance associated with the specified XElement by accessing the 'instance' attribute,
/// which should be of type XBoundAttribute. If the attribute or the instance within it is missing, the method can either
/// throw a NullReferenceException or return null or default, based on the specified parameter.
/// </summary>
/// <param name="this">The XElement from which to retrieve the object instance.</param>
/// <param name="throw">A boolean indicating whether to throw an exception or return the default value.</param>
/// <returns>The object instance associated with the XElement; returns null or the default value if unsuccessful.</returns>
public static object GetInstance(this XElement @this, bool @throw = false){...}
```
___

```
/// <summary>
/// Retrieves the object instance of type T associated with the specified XElement by accessing the 'instance' attribute,
/// which should be of type XBoundAttribute. If the attribute or the instance within it is missing, or if the instance cannot be cast to type T,
/// the method can either throw a NullReferenceException or return the default value of type T, based on the specified parameter.
/// </summary>
/// <typeparam name="T">The type to which the retrieved instance should be cast.</typeparam>
/// <param name="this">The XElement from which to retrieve the object instance.</param>
/// <param name="throw">A boolean indicating whether to throw an exception or return the default value.</param>
/// <returns>The type T instance associated with the XElement; returns default T value if unsuccessful.</returns>
public static object GetInstance(this XElement @this, bool @throw = false){...}
```
___

```
/// <summary>
/// Retrieves a child XElement corresponding to a specified property name from the parent XElement.
/// </summary>
/// <param name="this">The XElement from which to find the child element.</param>
/// <param name="propertyName">The name of the property corresponding to the child element to retrieve.</param>
/// <returns>The XElement representing the member with the given property name; returns null if no matching element is found.</returns>
public static object GetMember(this XElement @this, string propertyName){...}
```
___

**Modeling Options and Delegate Declarations**

```
[Flags]
public enum ModelingOption
{
    /// <summary>
    /// Binds the PropertyInfo to the member XElement enabling singletion reflection.
    /// </summary>
    CachePropertyInfo = 0x1,

    /// <summary>
    /// Shows the FullName of the type as attribute text values, normalized to a non-generic type name.
    /// </summary>
    /// <remarks>When not set, the short name is used after normalizing to a non-generic type name.</remarks>
    ShowFullNameForTypes = 0x2,

    /// <summary>
    /// Overrides default behavior where only reference types are bound to the member XElement.
    /// </summary>
    /// <remarks>
    /// Since value types, enums and strings aren't observable entities themselves, and have 
    /// no potential for hosting observable entities, these instances (especially stings, which 
    /// might be quite large) are deliberately not bound to the member XElement.
    /// </remarks>
    IncludeValueTypeInstances = 0x4,
}
```
___

**Delegate Declarations**

```
public delegate void PropertyChangedDelegate(object sender, PropertyChangedEventArgs e);
public delegate void NotifyCollectionChangedDelegate(object sender, NotifyCollectionChangedEventArgs e);
public delegate void XObjectChangeDelegate(object sender, XObjectChangeEventArgs e);
```






