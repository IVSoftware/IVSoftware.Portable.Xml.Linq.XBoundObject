[◀](../README.md)

### **NotifyWithDescendants: Observable Nested Objects**

This extension creates and transparently maintains a dynamic model of an arbitrary class or collection by aggregating the `INotifyPropertyChanged` and `INotifyCollectionChanged` events of any descendants of the `Property` hierachy. This model automatically subscribes a common handler to _any member at any nested level_ that exists or that arrives, and manages their unsubscription when removed from the model (without assuming that the removed object will go out of existence and be disposed independently).

#### **Overview**

When `ObservableCollection<T>` is used with platforms like WPF and Maui, the data models therein contained are often directly bound to a `DataTemplate` by implementing `INotifyPropertyChanged`. Most of the time, there's simply no need for the observable collection to react to these `PropertyChanged` events. And if you did need a collection to issue aggregated change events when its items invoke `PropertyChanged`, the `BindingList<T>` class takes things one step farther and provides this functionality provided that the hierarchy is flat.



