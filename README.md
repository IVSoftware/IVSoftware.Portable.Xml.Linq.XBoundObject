# XBoundObject

`XBoundObject` extends `System.Xml.Linq` with runtime object binding, structural placement, and tree-style navigation over in-memory XML.

The simplest way to think about it is this: sometimes an `XElement` or `XAttribute` wants a runtime companion object the way a UI tree node wants a `Tag` property. `XBoundObject` provides that missing binding surface without changing the serialized XML contract.

That small idea solves a larger portability problem. Hierarchical UI frameworks often keep important runtime state attached to platform-specific nodes. `XBoundObject` moves that binding layer into the portable model so that different frameworks can adapt to the same cross-platform structural runtime.

The result is still XML, but it is XML that can carry live instances, structural rules, and runtime intent while your process is running.

___

## Table Of Contents

- [Binding](README/Binding.md)
- [Placer](README/Placer.md)
- [Navigation](README/Navigation.md)
- [Examples](#examples)

___

## Why The Model

A flat collection is good at order. It is not naturally good at depth, placement, or local structural meaning.

An XML model helps when you need:

- durable hierarchy over a flat or partially flattened set of items
- a readable structural surface for runtime inspection
- a place to accumulate view-state and routing metadata
- a way to let live objects participate in a tree without inventing a custom tree format first

This is useful even in a console app. A flattened list can still be modeled as a hierarchical internal tree when the runtime problem is better expressed that way.

___

## Start Here

If you only learn three ideas first, make them these:

1. `Tag`
   `XBoundAttribute` gives XML a runtime-only binding slot for live objects.

2. `To<T>()` and `Has<T>()`
   These are the core retrieval and discovery primitives for bound state.

3. `Placer`
   This is the structural engine that turns flat path-like descriptions into a working XML tree.

The rest of the package grows naturally from those three ideas.

___

## Getting Started

Bind an object to an element:

```csharp
var xel = new XElement("node");
var person = new Person { Name = "Ada" };

xel.SetBoundAttributeValue(person, "model", "[Person]");

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

## Navigation In One Paragraph

`Ascendors` and `Descendors` provide a linear navigation grammar over a hierarchical in-memory tree. That matters when a model began as a flat collection, or when a hierarchy needs to behave like a calibrated sequence. `Ascendors` walk toward prior modeled context, `Descendors` walk toward next modeled context, and `OffsettorAt` resolves relative position from a chosen zero policy.

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
