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






