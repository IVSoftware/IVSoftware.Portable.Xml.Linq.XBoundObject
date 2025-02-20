using IVSoftware.Portable.Xml.Linq;
using System.Xml.Linq;

namespace MSTestProject;

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
