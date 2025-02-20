[◀](../README.md)

### **Nested Enum: Bringing Order to Flat Enums**  

#### **Overview**  

Enums are great for defining discrete sets of values, but they lack built-in hierarchy. **Nested Enums** solve this by transforming flat enums into structured XML representations, preserving relationships and enabling dynamic lookups at runtime.  

With **XBoundAttribute**, you can **project enums into an XML hierarchy** where each node represents an enum value—complete with **dual-key lookup** for seamless retrieval of bound objects and actions.  

By keeping enums **constrained within a namespace**, the resulting hierarchy remains **safe, unambiguous, and instantly queryable**—perfect for workflows, dynamic UI generation, and structured metadata.  

---

### **From Enums to structured XML in one call**  

With just one line of code, your enums gain structure and accessibility:  

##### **Raw Enums: Flat and Isolated**  

```csharp
enum DiscoveryDemo { Home, Scan, Settings }

enum Scan { QRCode, Barcode }

enum Settings { Apply, Cancel }
```
---

##### Call the `BuildNestedEnum()` extension in the C# code

```csharp
XElement xroot = typeof(DiscoveryDemo).BuildNestedEnum();
```


##### **Transformed XML: Hierarchical and Dynamic**  

```xml
<root dualkeylookup="[DualKeyLookup]">
  <node id="[DiscoveryDemo.Home]" />
  <node id="[DiscoveryDemo.Scan]">
    <node id="[Scan.QRCode]" />
    <node id="[Scan.Barcode]" />
  </node>
  <node id="[DiscoveryDemo.Settings]">
    <node id="[Settings.Apply]" />
    <node id="[Settings.Cancel]" />
  </node>
</root>
```

---

## Why Use Nested Enums?  

✅ **Transforms static enums into structured, navigable XML**  
✅ **Eliminates ambiguity with namespace-based constraints**  
✅ **Enables dynamic object binding & dual-key lookup**  
✅ **Ideal for UI structures, workflow logic, and metadata definitions**  

Nested Enums **bridge the gap between enums and structured data**, unlocking new possibilities for **data-driven applications** and **runtime interactions**.  


### What’s happening?  

- `BuildNestedEnum()` **scans the provided enum type** and its related enums.  
- It **constructs an `XElement` tree**, mapping enum values to XML nodes.  
- The `XElement` root is stored in `xroot`, **which, when printed, becomes this XML**.  

### **Why does this matter?**  

- **Structured data** → Instead of a flat list of enums, you now have **an XML hierarchy**.  
- **Seamless integration** → This structure supports **dual-key lookups and runtime object binding**.  
- **Interoperability** → XML output can be **saved, queried, transformed, or serialized**.  



