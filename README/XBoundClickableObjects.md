[◀](../README.md)


### **Binding Clickable Actions to XML Nodes**

#### **Overview**

This example demonstrates how to **attach interactive behaviors to XML nodes** by mapping **enum-based identifiers** to **clickable objects** at runtime. Using `XBoundAttribute`, we bind `Button` objects to XML nodes, enabling **event-driven interactions**.

With this setup, you can:

- **Generate clickable elements dynamically** from XML.
- **Map enums to UI components** with event-driven behavior.
- **Perform actions via XML lookups**, allowing flexible, data-driven interactions.

By leveraging `DualKeyLookup`, the XML structure becomes a **bidirectional mapping system**, where nodes store interactive objects, and events can be triggered based on their **enum identifiers**.

---

### Defining a Clickable Button (Minimal Example)

```csharp
interface IClickable
{
    Enum Id { get; }
    event EventHandler? Click;
    void PerformClick();
}

class Button : IClickable
{
    public Button(Enum id) => Id = id;
    public Enum Id { get; }
    public event EventHandler? Click;

    public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
}
```

#### Bind Buttons as XBound Attributes

A simple interface and class define **clickable objects** that store an `Enum Id` and fire a `Click` event.

```csharp
// A string builder to log click events as they occur,
var builder = new List<string>();


foreach (var node in xroot.Descendants().Where(_=>_.Has<Enum>()))
{
    var button = new Button(node.To<Enum>());
    button.Click += localClickHandler;
    node.SetBoundAttributeValue(button);
}

// The handler, in local scope as a function.
void localClickHandler(object? sender, EventArgs e)
{
    if (sender is IClickable button)
    {
        builder.Add($"Clicked: {button.Id}");
    }
}
```
#### Performing a Click via DualKeyLookup

Once buttons are bound to XML nodes, we can retrieve them via DualKeyLookup and trigger their events dynamically.

##### Obtain the dictionary from xroot

```csharp
var dict = xroot.To<DualKeyLookup>();
```

##### Obtain a button from the dictionary and perform the click.

```csharp
 dict[Scan.Barcode]
    .To<Button>()
    .PerformClick();
```