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

___

**Minimal Example**

Refer to the Unit Testing section of this repo for functional examples of all aspects of this Extension. 

- The `BCollection` contains items of `ClassB`.
- `ClassB` has a member named `C` that is a `ClassC`.
- `ClassC` implements `INotifyPropertyChanged` and in particular will raise this event when `C.Cost` changes.

And finally, we have `ClassA` which contains the `BCollection` and has a `SumOfBCost` that should be updated whenever:

1. `C.Cost` changes
2. `ClassB.C` instance is replaced with a new instance of `ClassC`. (Note that this is 'not' a collection changed event when this occurs!)
3. `BCollection` undergoes changes of `ClassB` items that are added, removed or replaced. (All of which _are_ collection changed events!)

___

As an alternative to subclassing `ObservableCollection<T>`, the snippet below employs the `WithNotifyOnDescendants(...)` extension.

Here, the aggregated traffic of all nested `INotifyPropertyChanged` descendants is routed to the designated `PropertyChangedEventHandler` delegate. Note that for the calculation of `SumOfBCost`, we not only have to respond to changes of the value of `C.Cost`, there is also a case where the `ClassB.C` is replaced by a different instance of `ClassC`. This scenario might be contributing to the problems you're describing, because this swap doesn't change the _collection_ of `ClassB` items. Therefore, trying to handle this scenario by responding to `NotifyCollectionChangedAction.Replace` isn't going to work.

A working WPF example can be found [here](https://github.com/IVSoftware/wpf-nested-observable.git);

```
class ClassA : INotifyPropertyChanged
{
    public ClassA() 
    {
        BCollection = new ObservableCollection<ClassB>
        {
            new ClassB{C = new ClassC{Name = $"Item C{_autoIncrement++}"} },
            new ClassB{C = new ClassC{Name = $"Item C{_autoIncrement++}"} },
            new ClassB{C = new ClassC{Name = $"Item C{_autoIncrement++}"} },
        }.WithNotifyOnDescendants(OnPropertyChanged);
    }
    public ObservableCollection<ClassB> BCollection { get; }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ClassC.Cost):
            case nameof(ClassB.C):
                SumOfBCost = BCollection.Sum(_ => _.C?.Cost ?? 0);
                break;
            default:
                break;
        }
    }

    static int _autoIncrement = 1;

    public int SumOfBCost
    {
        get => _sumOfBCost;
        set
        {
            if (!Equals(_sumOfBCost, value))
            {
                _sumOfBCost = value;
                OnPropertyChanged();
            }
        }
    }
    int _sumOfBCost = default;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    public event PropertyChangedEventHandler? PropertyChanged;
}
```

___

**Testing Off-List Swaps**

In order to verify that `ClassA` continues to respond to new instances of `ClassC` that are swapped into `ClassB.C` we can devise this simple test.

```
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        dataGridClassC.AutoGeneratingColumn += (sender, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ClassC.Name):
                    e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    break;
            }
        };
    }
    /// <summary>
    /// Replace the C objects and make sure the new ones are still responsive.
    /// </summary>
    private void OnTestReplaceCObjects(object sender, RoutedEventArgs e)
    {
        foreach(ClassB classB in DataContext.BCollection)
        {
            classB.C = new ClassC { Name = classB.C?.Name?.Replace("Item C", "Replace C") ?? "Error" };
        }
    }
    new ClassA DataContext => (ClassA)base.DataContext;
}
```

___

**Under the Hood**

When the `WithNotifyOnDescendants(...)` extension is run on a collection or class instance, it creates a dynamic XML model where instances and handlers are bound to the same `XElement`. To view the model, use this overload instead and view `model.ToString()`.

```
BCollection = new ObservableCollection<ClassB>().WithNotifyOnDescendants(out XElement model, OnPropertyChanged);
```
___

```xml
<model name="(Origin)ObservableCollection" instance="[ObservableCollection]" onpc="[OnPC]" context="[ModelingContext]">
  <member name="Count" />
  <model name="(Origin)ClassB" instance="[ClassB]" onpc="[OnPC]">
    <member name="C" instance="[ClassC]" onpc="[OnPC]">
      <member name="Name" />
      <member name="Cost" />
      <member name="Currency" />
    </member>
  </model>
  <model name="(Origin)ClassB" instance="[ClassB]" onpc="[OnPC]">
    <member name="C" instance="[ClassC]" onpc="[OnPC]">
      <member name="Name" />
      <member name="Cost" />
      <member name="Currency" />
    </member>
  </model>
  <model name="(Origin)ClassB" instance="[ClassB]" onpc="[OnPC]">
    <member name="C" instance="[ClassC]" onpc="[OnPC]">
      <member name="Name" />
      <member name="Cost" />
      <member name="Currency" />
    </member>
  </model>
</model>
```

___

**Example Unit Test using DFT Hooks**

```
[TestMethod]
public void Test_XObjectChangeDelegate()
{
    List<string> builder = new();
    void localOnAwaited(object? sender, AwaitedEventArgs e)
    {
        builder.Add($"{e.Args}");
    }
    try
    {
        Awaited += localOnAwaited;
        string joined;
        var classA = new ClassA(false);
        _ = classA.WithNotifyOnDescendants(
            out XElement model,
            onPC: (sender, e) =>
            {
                eventsPC.Enqueue(new SenderEventPair(sender, e));
            },
            onCC: (sender, e) =>
            {
                eventsCC.Enqueue(new SenderEventPair(sender, e));
            },
            onXO: (sender, e) =>
            {
                if (e is XObjectChangedOrChangingEventArgs ePlus && !ePlus.IsChanging)
                {
                    builder.Add(ePlus.ToString(sender, timestamp: false) ?? "Error");
                }
            });

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;
        expected = @" 
Add    XAttribute Changed : name   
Add    XAttribute Changed : instance 
Add    XElement   Changed : member TotalCost
Add    XElement   Changed : member BCollection
Add    XAttribute Changed : instance 
Add    XElement   Changed : member Count
Add    XAttribute Changed : onpc   
Added INPC Subscription
<member name=""BCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" />
Add    XAttribute Changed : oncc   
Added INCC Subscription
<member name=""BCollection"" instance=""[ObservableCollection]"" onpc=""[OnPC]"" oncc=""[OnCC]"" />";
        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting un-timestamped message reporting"
        );
        builder.Clear();

        classA.BCollection.Add(new());

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;

        actual.ToClipboard();
        actual.ToClipboardAssert();
        expected = @" 
Added INPC Subscription
<model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Add    XElement   Changed : model  (Origin)ClassB";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting un-timestamped message reporting"
        );
        builder.Clear();
        classA.BCollection[0] = new();

        joined = string.Join(Environment.NewLine, builder);
        actual = joined;
        expected = @" 
Remove XElement   Changed : member Currency
Remove XElement   Changed : member Cost
Remove XElement   Changed : member Name
Removing INPC Subscription
Remove <member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Remove XElement   Changed : member C
Removing INPC Subscription
Remove <model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Remove XElement   Changed : model  (Origin)ClassB
Added INPC Subscription
<model name=""(Origin)ClassB"" instance=""[ClassB]"" onpc=""[OnPC]"" />
Added INPC Subscription
<member name=""C"" instance=""[ClassC]"" onpc=""[OnPC]"" />
Add    XElement   Changed : model  (Origin)ClassB";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting subscription removal"
        );
    }
    finally
    {
        Awaited -= localOnAwaited;
    }
}
```



