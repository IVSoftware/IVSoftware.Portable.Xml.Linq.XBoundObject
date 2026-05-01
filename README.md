# XBoundObject

`XBoundObject` extends `System.Xml.Linq` with runtime object binding, structural placement, and LINQ-like enumeration over in-memory instance hierarchies.

The simplest way to think about it is this: sometimes an `XElement` or `XAttribute` wants a runtime companion object the way a UI tree node wants a `Tag` property. `XBoundObject` provides that missing binding surface without changing the serialized XML contract.

That small idea solves a larger portability problem. Hierarchical UI frameworks often keep important runtime state attached to platform-specific nodes. `XBoundObject` moves that binding layer into the portable model so that different frameworks can adapt to the same cross-platform structural runtime.

The result is still XML, but it is XML that can carry live instances, structural rules, and runtime intent while your process is running.

___

## Table Of Contents

- [Binding](README/Binding.md)
- [Placer](README/Placer.md)
- [Enumeration](README/Enumeration.md)
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

- You might want a modeled collection with perceived depth, collapsible nodes, filtering, or temporal semantics.
  That is a separate but related layer, and XML is a strong internal surface for it.

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

Navigate the modeled structure:

```csharp
var next = root.NextDescendor();
var prior = next?.PreviousAscendor();
var offset = root.OffsettorAt(plusOrMinus: 3);
```

___

## Enumeration In One Paragraph

`Ascendors` and `Descendors` provide a LINQ-like enumeration grammar over a hierarchical in-memory tree. That matters when a hierarchy needs to behave like a calibrated sequence instead of only as nested XML. `Ascendors` walk toward prior modeled context, `Descendors` walk toward next modeled context, and `OffsettorAt` resolves relative position from a chosen zero policy.

___

## Examples

- [Build Nested Enum](README/BuildNestedEnum.md)
- [XBound Clickable Objects](README/XBoundClickableObjects.md)
- [Dual Key Lookup](README/DualKeyLookup.md)
- [Notify On Descendants](README/NotifyOnDescendants.md)

___

## Notes

- Bound objects are runtime-only by design.
- Named enums are supported and work well as standard attribute keys.
- The package is meant to stay useful as both a practical tool and a structural foundation.

If you want the API surface in detail, rely on XML documentation and IntelliSense. This README is meant to explain what the package is for and how its pieces fit together.
