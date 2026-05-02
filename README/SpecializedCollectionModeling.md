# [<](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README.md)

## Specialized Collection Modeling

This is a separate but related layer.

`XBoundObject` itself does not require you to think in terms of modeled collections. Ordinary `System.Xml.Linq` traversal still works in the usual way. But once a flat collection is projected into an internal tree model, the XML can begin to carry more than structure alone.

That is where specialized collection modeling becomes useful.

___

## In One Paragraph

Sometimes a flat collection wants an internal tree model so that depth,
filtering, collapsible structure, or temporal semantics can become part of
the runtime surface. That is where specialized collection modeling begins,
and that is where concepts like `Ascendors`, `Descendors`, and
`OffsettorAt(...)` come into play.

`Ascendors` matter because the prior modeled context is not always the
visual parent. In a modeled sequence, the next ascending node may be the
deepest leaf of the previous structural branch.

`Descendors` matter because a node may have ordinary trailing children in
the usual sense, but it may also have leading children that participate in
a specialized modeled sequence, including temporal interpretations such as
looking back from an anchor point.

___

## Why The Model

A linear collection is good at order. It is not naturally good at durable depth, placement, or local structural meaning.

An XML model helps when you need:

- hierarchy over a flat or partially flattened set of items
- a readable structural surface for runtime inspection
- a place to accumulate view-state and routing metadata
- a way to let live objects participate in a tree without inventing a custom tree format first

This is useful even in a console app. A flattened list can still be modeled as a hierarchical internal tree when the runtime problem is better expressed that way.

___

## Why Specialized Traversal Exists

Once a flat collection is modeled as a hierarchy, plain document-order traversal is not always enough.

That is the role of:

- `Ascendors`
- `Descendors`
- `OffsettorAt(...)`

These APIs provide a calibrated traversal surface over the modeled tree.

In practical terms:

- `Ascendors` walk toward prior modeled context.
- `Descendors` walk toward next modeled context.
- `OffsettorAt(...)` resolves relative position from a chosen zero policy.

___

## Where This Helps

This layer becomes useful when the model is doing more than simply holding bound objects.

Typical examples include:

- perceived depth over a flat collection
- collapsible nodes in a collection view
- filtered views over a canonical superset
- temporal or affinity-style interpretation over a modeled sequence

At that point, the XML is not merely a container. It is an internal modeled surface with its own traversal semantics.
