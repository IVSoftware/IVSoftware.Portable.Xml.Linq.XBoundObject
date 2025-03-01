[◀](../README.md)

### **DualKeyLookup: Bidirectional Enum-to-XML Mapping**

#### **Overview**

`DualKeyLookup` is a specialized **bidirectional dictionary** that enables seamless mapping between **enum values and XML elements (`XElement`)**. This allows for **two-way lookups**, where:
- **Enums serve as keys** to retrieve corresponding XML nodes.
- **XML nodes serve as keys** to retrieve their associated enums.

This functionality is essential for **XML structures requiring hierarchical data representation and runtime object binding**, making it ideal for **templating, serialization, and dynamic UI interactions**.

---

#### **Features**

• **Efficient Two-Way Lookup** – Retrieve an `XElement` using an `Enum`, or get the `Enum` that corresponds to an `XElement`.  
• **Direct Indexer Access** – Supports lookups via **`this[Enum]`** and **`this[XElement]`** for intuitive retrieval.  
• **Automatic Synchronization** – Ensures that **both mappings remain in sync**, preventing inconsistencies.  
• **Exception Handling Option** – Supports optional **strict retrieval** with an exception-throwing mode.  
• **Dynamic Modification** – Easily **add, remove, and clear** mappings at runtime.

---


**Code Example**

```csharp

[TestClass]
public class TestClass_X2ID2X
{
    enum ID
    {
        A,
        B,
    }

    [TestMethod]
    public void Test_X2ID2X()
    {
        var x2id2x = new DualKeyLookup();

        XElement xelA = new XElement("xel", "A");
        XElement xelB = new XElement("xel", "B");

        x2id2x[ID.A] = xelA;
        x2id2x[xelB] = ID.B;

        // Loopback two-way binding
        Assert.IsTrue(ReferenceEquals(x2id2x[ID.A], xelA));
        Assert.AreEqual(x2id2x[xelA], ID.A);

        Assert.IsTrue(ReferenceEquals(x2id2x[ID.B], xelB));
        Assert.AreEqual(x2id2x[xelB], ID.B);

        Assert.AreEqual(x2id2x.Count, 2);

        x2id2x.Clear();

        Assert.AreEqual(x2id2x.Count, 0);
    }
}
```

___
### **Why Use DualKeyLookup?**  

🔹 **Bridges Enums and XML** – Enables **structured data representation** with enum-based XML bindings.  
🔹 **Runtime Object Binding** – Essential for **templating, serialization, and dynamic workflows**.  
🔹 **Two-Way Mapping for Interactivity** – Ideal for **UI-driven XML processing, menu structures, and event handling**.  

**With `DualKeyLookup`, enum values and XML elements become seamlessly linked, enabling powerful, structured interactions in your XML-driven applications.**  
~~~
