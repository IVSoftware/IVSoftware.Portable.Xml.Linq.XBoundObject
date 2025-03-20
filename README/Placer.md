[◀](../README.md)


## Placer Class Overview

The `Placer` class is designed to project a flat string path onto a new or existing node within an XML hierarchy, effectively managing intermediate paths and their creation as necessary.

### Basic Usage

Here's a simple example of using the `Placer` to create or find a node in an XML document:

```csharp
var placer = new Placer(xmlSource, @"path\to\element", mode: PlacerMode.FindOrCreate);
switch (placer.PlacerResult) {
    case PlacerResult.Created:
        // Handle newly created path
        break;
    case PlacerResult.Exists:
        // Handle existing path
        break;
    // Additional cases as necessary
}
```
___

### Advanced Usage with Event Handlers

One typical use case involves a standalone `XElement` that has been instantiated and constructed, with the idea the _now_ we want to place this at a particular location in the hierarchy. The way this is done is by intercepting the `onBeforeAdd`action, and replacing the "proposed" `XElement` (which the placer is newly creating ad hoc) with the existing target. As shown in the snippet below, using an inline lambda makes for an especially clean and encapsulated call to the placer.

```
foreach (var file in files)
{
    new Placer(
        _xroot, 
        file, 
        onBeforeAdd: (sender, e) =>
    {
        // EXAMPLE: Attach an instance of FileItem to the XElement.
        e.Xel.SetBoundAttributeValue(
            new FileItem(e.Xel),
            name: nameof(NodeSortOrder.node));
    });
}
```


## Why Placer is Implemented as a Class and Not an Extension Method

The `Placer` class in the IVSoftware.Portable.Xml.Linq library manages XML element placement more effectively than what could be achieved with extension methods due to its complex and stateful nature. Here are the key reasons for choosing a class implementation:

### State Management
`Placer` maintains state throughout its operations, tracking the current node (`_xTraverse`), placement results (`PlacerResult`), and other operational states. This capability is essential for handling complex XML manipulations and is impractical with stateless extension methods.

### Complex Configuration
The class supports detailed configurations, such as event handlers for node addition (`onBeforeAdd`, `onAfterAdd`) and during iterations (`onIterate`). These features require a structured approach that is best handled within a class.

### Reusable and Isolated Logic
Using a class allows for the encapsulation of complex logic into a reusable and isolated unit, enhancing code testability and reuse without side effects—advantages not typically available with extension methods, which are generally limited to extending functionality in a more linear fashion.

### Result and Error Handling
`Placer` provides detailed feedback on operations through comprehensive results and supports structured error handling strategies, including exception throwing and condition asserting. This level of detailed response is better suited to a class structure.

### Usage Context
`Placer` is designed for scenarios requiring initialization with complex parameters and management through object-oriented practices, which aligns well with the instantiation and lifecycle management of class objects.

### Performance Considerations
As an object, `Placer` can optimize operations, particularly in managing large or complex XML documents. This control over performance and memory management is beyond the capability of extension methods.

