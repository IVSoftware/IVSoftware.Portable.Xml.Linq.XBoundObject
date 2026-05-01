# XBoundObject

`XBoundObject` extends `System.Xml.Linq` with runtime object binding, structural placement, and tree-style navigation over in-memory XML.

The simplest way to think about it is this: sometimes an `XElement` or `XAttribute` wants a runtime companion object the way a UI tree node wants a `Tag` property. `XBoundObject` provides that missing binding surface without changing the serialized XML contract.

That small idea solves a larger portability problem. Hierarchical UI frameworks often keep important runtime state attached to platform-specific nodes. `XBoundObject` moves that binding layer into the portable model so that different frameworks can adapt to the same cross-platform structural runtime.

The result is still XML, but it is XML that can carry live instances, structural rules, and runtime intent while your process is running.

___

## The Binding Story

At the center of the package is `XBoundAttribute`, a runtime-aware attribute that can bind an object to XML.

That sounds small, but it changes what in-memory XML can do.

- An `XElement` can represent a live object, not just text.
- A tree can carry runtime behavior, not just structure.
- Configuration, workflow, and model nodes can stay readable as XML while still participating in polymorphic application logic.
- Portable code can own node-bound runtime state instead of leaving that concern inside platform views.

This is largely an in-memory distinction. Bound objects are not meant to round-trip through serialization automatically. That is deliberate. The serialized XML remains clean while the running model remains rich.

___

## Why The Model

A flat collection is good at order. It is not naturally good at depth, placement, or local structural meaning.

An XML model helps when you need:

- durable hierarchy over a flat or partially flattened set of items
- a readable structural surface for runtime inspection
- a place to accumulate view-state and routing metadata
- a way to let live objects participate in a tree without inventing a custom tree format first

This is useful even when the authoritative data shape started life as a list. A flattened list can still be modeled as a hierarchical internal tree when the runtime problem is better expressed that way.

___

## Placement

`Placer` is the part of the package that turns flat path-like descriptions into XML structure.

That makes it practical to:

- build trees from file-system-like paths
- maintain model nodes for routed views
- create or locate structural positions quickly
- attach content and bound objects at the point of placement

This is one of the reasons the package works well for smart trees and interactive model surfaces. The XML is not just stored. It is actively shaped.

___

## Navigation

`Ascendors` and `Descendors` provide a linear navigation grammar over the modeled tree.

The important idea is not that XML suddenly stops being hierarchical. It is that a hierarchical in-memory tree can still expose a meaningful linear traversal surface.

That is useful when a model began as a flat collection, or when a view needs to move through a hierarchy as though it were a calibrated sequence.

In practical terms:

- `Ascendors` walk toward the prior modeled context.
- `Descendors` walk toward the next modeled context.
- `OffsettorAt` resolves relative position from a chosen zero policy.

These APIs are intentionally small, but they unlock a lot of higher-level behavior once structure and runtime binding live together.

___

## Typical Uses

Common uses for `XBoundObject` include:

- smart tree models where nodes know what can be attached beneath them
- runtime configuration surfaces backed by readable XML
- UI models that need live objects attached to structural nodes
- workflow and rule trees with polymorphic bound instances
- cross-platform model surfaces where the XML remains the common denominator

A typical example is a drag-drop model tree where attaching one node beneath another changes what the system considers valid, available, or calculable. In that kind of system, the XML model remains inspectable while the bound runtime objects remain active.

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

## Notes

- Bound objects are runtime-only by design.
- Named enums are supported and work well as standard attribute keys.
- The package is meant to stay useful as both a practical tool and a structural foundation.

If you want the API surface in detail, rely on XML documentation and IntelliSense. This README is meant to explain what the package is for and how its pieces fit together.
