# IVSoftware.Portable.Xml.Linq.XBoundObject [[GitHub]](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject.git)

`XBoundObject` extends `System.Xml.Linq` with runtime object binding, structural placement, and Linq-like enumeration over in-memory instance hierarchies.

The simplest way to think about it is this: sometimes an `XElement` or `XAttribute` wants a runtime companion object the way a UI tree node wants a `Tag` property. `XBoundObject` provides that missing binding surface without changing the serialized XML contract.

That small idea solves a larger portability problem. Hierarchical UI frameworks often keep important runtime state attached to platform-specific nodes. `XBoundObject` moves that binding layer into the portable model so that different frameworks can adapt to the same cross-platform structural runtime.

The result is still XML, but it is XML that can carry live instances, structural rules, and runtime intent while your process is running.

___

## Table Of Contents

- [Binding](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/Binding.md)
- [Placer](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/Placer.md)
- [Examples](#examples)

___

## Start Here

If you already know:

```csharp
xel.SetAttributeValue(name, value);
```

then the new idea is:

```csharp
xel.SetBoundAttributeValue(tag, name, text);
```

That is the move that gives an `XElement` a runtime-only binding slot for live objects.

A few practical notes follow from that:

- multiple bound attributes can live on the same `XElement`
- the XML stays readable
- the bound objects remain in memory only

And in the common case, retrieval is just:

```csharp
MyObjectType myObject = xel.To<MyObjectType>();
```

`Has<T>()` is the companion question when the caller wants to check for presence first.

From there, `Placer` is the structural engine that turns flat path-like descriptions into a working XML tree.

Ordinary enumeration still works exactly the way you expect:

```csharp
foreach (var xel in xroot.Descendants().Where(_ => _.Has<Person>()))
{
}
```

`XBoundObject` does not replace normal LINQ to XML traversal. It gives those nodes richer runtime meaning.

___

## What Comes Next

- You might want to place nodes quickly from delimited paths.
  That is what `Placer` is for.

- Once you understand the raw placement model, you may want to streamline it.
  That is where the higher-level placer extensions come in.

- You might want to link multiple live objects on the same node.
  IDs, enums, and lookup-oriented helpers support that style of modeling.

- You might want a model surface for hardware configuration, automation trees, domain-language parsers, or abstract syntax trees.
  Bound XML works well for those.

___

## Getting Started

Bind an object to an element:

```csharp
var xel = new XElement("node");
var person = new Person { Name = "Ada" };

// With default arguments for `name` and `text`
xel.SetBoundAttributeValue(person);   // <xel person=""[Person]"" />

if (xel.To<Person>() is { } boundPerson)
{
    Console.WriteLine(boundPerson.Name);
}
```

Place a path into a tree:

```csharp
var root = new XElement("root");
root.Place(Path.Combine("USB", "Controller", "Family"), out XElement xelNew);
```

Navigate the placed structure:

```csharp
var controller = root.Descendants("Controller").Single();
var family = controller.Descendants("Family").Single();
```

___

## Examples

- [Build Nested Enum](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/BuildNestedEnum.md)
- [XBound Clickable Objects](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/XBoundClickableObjects.md)
- [Dual Key Lookup](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/DualKeyLookup.md)
- [Notify On Descendants](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README/NotifyOnDescendants.md)

___

## Notes

- Bound objects are runtime-only by design.
- Named enums are supported and work well as standard attribute keys.
- The package is meant to stay useful as both a practical tool and a structural foundation.

If you want the API surface in detail, rely on XML documentation and IntelliSense. This README is meant to explain what the package is for and how its pieces fit together.
