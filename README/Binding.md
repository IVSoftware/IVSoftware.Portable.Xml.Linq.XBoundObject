# [<](../README.md)

## Binding

At the center of the package is `XBoundAttribute`, a runtime-aware attribute that can bind an object to XML.

That sounds small, but it changes what in-memory XML can do.

- An `XElement` can represent a live object, not just text.
- A tree can carry runtime behavior, not just structure.
- Configuration, workflow, and model nodes can stay readable as XML while still participating in polymorphic application logic.
- Portable code can own node-bound runtime state instead of leaving that concern inside platform views.

This is largely an in-memory distinction. Bound objects are not meant to round-trip through serialization automatically. That is deliberate. The serialized XML remains clean while the running model remains rich.

___

## Core Surface

These are the methods that define the basic binding story:

- `SetBoundAttributeValue(...)`
- `WithBoundAttributeValue(...)`
- `To<T>()`
- `Has<T>()`

___

## SetBoundAttributeValue

Use `SetBoundAttributeValue(...)` when you want to attach a runtime object to an element by adding an `XBoundAttribute`.

```csharp
xel.SetBoundAttributeValue(person, "model", "[Person]");
```

There are two common forms:

- fully qualified string name
- enum-backed standard name

___

## WithBoundAttributeValue

Use `WithBoundAttributeValue(...)` when you want the same operation in fluent form.

```csharp
var xel =
    new XElement("node")
    .WithBoundAttributeValue(person, "model", "[Person]");
```

This is especially useful when shaping trees inline or composing placement calls.

___

## To<T>

`To<T>()` retrieves the object bound to the element.

```csharp
if (xel.To<Person>() is { } boundPerson)
{
    Console.WriteLine(boundPerson.Name);
}
```

This is the method that turns XML from a passive structure into an active model surface.

___

## Has<T>

`Has<T>()` answers the smaller question first: does this element expose a bound value of type `T`?

```csharp
if (xel.Has<Person>())
{
    var person = xel.To<Person>();
}
```

`Has<T>()` and `To<T>()` are often used together when the caller wants explicit control over absence.

___

## Named Enums

Named enums are an important part of the package because they allow stable, readable keys in the model.

The practical rule is simple:

- prefer nullable named enum reads such as `To<MyEnum?>()`

That avoids the misleading `default(T)` problem for non-nullable enum retrieval.
