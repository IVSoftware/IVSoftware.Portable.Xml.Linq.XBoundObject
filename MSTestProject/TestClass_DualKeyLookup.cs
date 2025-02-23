using IVSoftware.Portable.Xml.Linq;
using System.Xml.Linq;

namespace MSTestProject;

[TestClass]
public class TestClass_X2ID2X
{
    enum ID
    {
        A,
        A1,
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

        // Test replacement

        x2id2x[ID.A] = xelA;
        x2id2x[ID.B] = xelB;

        Assert.AreEqual(x2id2x.Count, 2);

        XElement xelB1 = new XElement("xel", "B1");

        // REPLACE XEL: This needs to annihilate xelB.
        x2id2x[ID.B] = xelB1;

        Assert.IsNull(x2id2x[xelB], "Expecting the old xel has been removed.");
        Assert.AreEqual(x2id2x[ID.B], xelB1);

        Assert.AreEqual(x2id2x.Count, 2);


        // REPLACE ID: This needs to annihilate "A".
        x2id2x[xelA] = ID.A1;

        Assert.IsNull(x2id2x[ID.A], "Expecting the old ID has been removed.");
        Assert.AreEqual(x2id2x[xelA], ID.A1);

        Assert.AreEqual(x2id2x.Count, 2);

        // Test null ID setters
        x2id2x[ID.A] = null; // Null out a NON EXISTENT entry
        x2id2x[ID.A1] = null; 
        x2id2x[ID.B] = null;

        Assert.AreEqual(x2id2x.Count, 0);

        // Test null XEL setters
        x2id2x[ID.A] = xelA;
        x2id2x[ID.B] = xelB;

        Assert.AreEqual(x2id2x.Count, 2);

        x2id2x[xelA] = null;
        x2id2x[xelB] = null;
        x2id2x[xelB1] = null; // Null out a NON EXISTENT entry

        Assert.AreEqual(x2id2x.Count, 0);
    }
}
