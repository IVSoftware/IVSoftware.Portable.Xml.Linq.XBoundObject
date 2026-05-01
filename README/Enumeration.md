# [<](../README.md)

## Enumeration

`Ascendors` and `Descendors` provide a LINQ-like enumeration grammar over a hierarchical in-memory tree.

The important idea is not that XML stops being hierarchical. It is that a hierarchical model can still expose a meaningful linear traversal surface.

That is useful when:

- a model began as a flat collection
- a view needs to move through hierarchy as though it were a calibrated sequence
- structure and sequence both matter at the same time

___

## Core Surface

- `PreviousAscendor(...)`
- `Ascendors(...)`
- `NextDescendor(...)`
- `Descendors(...)`
- `OffsettorAt(...)`

___

## Ascendors And Descendors

In practical terms:

- `Ascendors` walk toward prior modeled context.
- `Descendors` walk toward next modeled context.

These are intentionally small APIs. Their value comes from the fact that they operate over a modeled tree that may represent a flattened list, a routed structure, or some other calibrated hierarchy.

___

## OffsettorAt

`OffsettorAt(...)` resolves relative position from a chosen zero policy.

That makes it possible to treat a modeled tree as a navigable coordinate system rather than only as nested XML.

___

## Affinity

Affinity semantics are a descension-side concern.

This allows a modeled tree to reinterpret a local leading band while still returning to ordinary forward traversal afterward. The result is an enumeration surface that can express more than simple document order without abandoning the underlying model.
