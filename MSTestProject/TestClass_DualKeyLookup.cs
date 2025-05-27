using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using System.Collections.Specialized;
using System.Xml.Linq;
using XBoundObjectMSTest;

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


    static Queue<SenderEventPair> SenderEventQueue = new();

    [TestMethod]
    public void Test_ObservableX2ID2X()
    {
        var x2id2x = new DualKeyLookup();
        bool shortCircuit = true;

        x2id2x.CollectionChanged += (sender, e) =>
        {
            SenderEventQueue.Enqueue((sender, e));
        };
        x2id2x.BeforeModifyMapping += (sender, e) =>
        {
            SenderEventQueue.Enqueue((sender, e));
            e.Cancel = shortCircuit;
        };

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
        Assert.AreEqual(SenderEventQueue.Count, 2);

        SenderEventQueue.Clear();
        x2id2x.Clear();
        Assert.AreEqual(
            (SenderEventQueue.DequeueSingle()?.e as NotifyCollectionChangedEventArgs)?.Action,
            NotifyCollectionChangedAction.Reset);

        Assert.AreEqual(x2id2x.Count, 0);

        // Test benign reentry
        x2id2x[ID.A, @throw: true] = xelA;
        _ = SenderEventQueue.DequeueSingle();

        // Test detectable remapping
        x2id2x[ID.A, @throw: true] = xelB;
        Assert.IsInstanceOfType(
            SenderEventQueue.DequeueSingle()?.e,
            typeof(BeforeModifyMappingCancelEventArgs));

        // Test inintended remapping
        try
        {
            shortCircuit = false;
            x2id2x[ID.A, @throw: true] = xelB;
            Assert.Fail("Expecting this operation is disallowed.");
        }
        catch (InvalidOperationException)
        {
            // PASS! This exception SHOULD BE THROWN.
        }
        finally
        {
            shortCircuit = true;
        }
    }
}
