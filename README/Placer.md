# [<](https://github.com/IVSoftware/IVSoftware.Portable.Xml.Linq.XBoundObject/blob/master/README.md)

## Placer

`Placer` turns a flat path-like description into a navigable XML structure.

This is the structural engine behind a lot of `XBoundObject` usage. It lets a portable model own hierarchy even when the incoming shape is still just a string path, an enum key path, or some other flattened descriptor.

___

## Core Surface

- `new Placer(...)`
- `Place(...)`
- `FindOrCreate(...)`
- `FindOrCreate<T>(...)`

___

## The Main Idea

Given a root element and a path such as:

```csharp
Path.Combine("USB", "Controller", "Family")
```

`Placer` can:

- find the matching structural position if it already exists
- create the missing intermediate nodes if it does not
- bind content or runtime objects at the destination
- report whether the operation found, created, or only partially matched the path

___

## PlacerMode

`PlacerMode` controls what happens when one or more path segments do not already exist.

- `FindOrPartial`
  Stop at the deepest existing node and report partial progress.

- `FindOrCreate`
  Create missing nodes as needed.

- `FindOrThrow`
  Treat missing path segments as an exception case.

- `FindOrAssert`
  Treat missing path segments as a debug assertion case.

For most day-to-day work, `FindOrCreate` is the default and the most natural place to start.

___

## PlacerResult

`PlacerResult` explains what happened after the operation ran.

- `NotFound`
- `Partial`
- `Exists`
- `Created`
- `Assert`
- `Throw`

That result is useful when the caller needs to distinguish between locating an existing node and shaping new structure.

___

## Place Extensions

The `Place(...)` extensions are the quickest entry point.

```csharp
var root = new XElement("root");
var result = root.Place(
    Path.Combine("USB", "Controller", "Family"),
    out XElement xel);
```

These extensions accept optional arguments by type, which is what makes them flexible without requiring a long parameter list.

Common optional arguments include:

- `PlacerMode`
- `Dictionary<StdPlacerKeys, string>`
- `XAttribute`
- `XBoundAttribute`
- enum placement payloads
- substitute `XElement`
- one remaining scalar value for element content

___

## StdPlacerKeys

`StdPlacerKeys` lets the caller override basic structural conventions at the call site.

The two most important keys are:

- `NewXElementName`
- `PathAttributeName`

This makes it possible to adapt placement behavior without changing the model-wide defaults.

___

## FindOrCreate

`FindOrCreate(...)` is the higher-level convenience surface for a common case: ensure a structural location exists.

`FindOrCreate<T>(...)` goes one step further by binding an instance of `T` at the destination when needed.

That is where placement and binding start to work together as one flow instead of two unrelated steps.
