using System.Collections.ObjectModel;
using System.Xml.Linq;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;
using static IVSoftware.Portable.Threading.Extensions;

namespace XBoundObjectMSTest;

[TestClass]
public class TestClass_XBoundViewObject
{
    static Queue<SenderEventPair> _eventQueue = new ();

    static bool _expectingAwaitedEvents = false;

    private static void OnAwaited(object? sender, AwaitedEventArgs e)
    {
        Assert.IsTrue(
            _expectingAwaitedEvents,
            "Expecting SyncList() only occurs when requested."
        );
        _eventQueue.Enqueue(new SenderEventPair(sender, e));        
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
        => Awaited += OnAwaited;

    [ClassCleanup]
    public static void ClassCleanup() 
        => Awaited -= OnAwaited;

    [TestMethod]
    public void Test_PlusMinus()
    {
        string 
            actual, 
            expected,
            joined,
            adhocPath,
            origPath =
                @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj",
            demoPath = 
                Path.Combine("C:", "Github", "IVSoftware", "Demo");
        var items = new ObservableCollection<Item>();
        Item? item = null;
        var xroot = new XElement("root").UseXBoundView(items, indent: 2, autoSyncEnabled: true);
        var context = xroot.To<ViewContext>(@throw: true);

        subtestShowPathSimpleThenSyncList();
        subtestCollapseDemoNode();
        subtestAddNotVisibleFloppyB();
        subtestMakeBDriveVisible();
        subtestExpandDemoNode();
        subtestCollapseC();

        subtestExpandLeaf();
        void subtestExpandLeaf() 
        {
            xroot.Expand(origPath);

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();

            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting full expansion");
            { }
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" item=""[Item]"" />
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" isvisible=""True"" plusminus=""Leaf"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting full expansion"
            );

            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
  B:(Floppy Disk)
- C:
  - Github
    - IVSoftware
      - Demo
        - IVSoftware.Demo.CrossPlatform.FilesAndFolders
          - BasicPlacement.Maui
              BasicPlacement.Maui.csproj"
            ;
        }

        #region S U B T E S T S

        void subtestShowPathSimpleThenSyncList()
        {
            // WinOS path in a WinOS test.
            xroot.Show(origPath);

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" isvisible=""True"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting an expanded tree with XBO='Item' culminating in a leaf node."
            );

            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;
            expected = @" 
- C:
  - Github
    - IVSoftware
      - Demo
        - IVSoftware.Demo.CrossPlatform.FilesAndFolders
          - BasicPlacement.Maui
              BasicPlacement.Maui.csproj"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting indented monospace text."
            );
        }

        void subtestCollapseDemoNode()
        {
            xroot.Collapse(demoPath);

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @"
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting removal of all isvisible and plusminus attrs below the collapsed node."
            );

            // Apps may put this on a WDT...
            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;
            expected = @" 
- C:
  - Github
    - IVSoftware
      + Demo"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting indented monospace text."
            );
        }

        void subtestAddNotVisibleFloppyB()
        {

            adhocPath = Path.Combine("B:(Floppy Disk)");
            item = xroot.FindOrCreate<Item>(adhocPath);
            Assert.IsTrue(item is Item, $"Expecting adhoc not-visible {nameof(Item)}");

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
  <xnode text=""B:(Floppy Disk)"" item=""[Item]"" />
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xroot UNSORTED with item not visible."
            );
        }

        void subtestMakeBDriveVisible()
        {
            Assert.IsNotNull(item);
            item.IsVisible = true;

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" item=""[Item]"" />
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xroot unsorted with item VISIBLE."
            );

            xroot.Sort();

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" item=""[Item]"" />
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xroot SORTED with item visible."
            );


            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;

            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
  B:(Floppy Disk)
- C:
  - Github
    - IVSoftware
      + Demo"
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting B: is visible and sorted alphabetically."
            );
        }

        void subtestExpandDemoNode()
        {
            xroot.Expand(demoPath);

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" item=""[Item]"" />
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" isvisible=""True"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting single child is now Visible"
            );

            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
B:(Floppy Disk)
C:
  Github
    IVSoftware
      Demo
        IVSoftware.Demo.CrossPlatform.FilesAndFolders"
            ;
        }

        void subtestCollapseC()
        {
            xroot.Collapse("C:");

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" item=""[Item]"" />
  <xnode text=""C:"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
    <xnode text=""Github"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" datamodel=""[Item]"">
        <xnode text=""Demo"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting single child is now Visible"
            );

            context.SyncList();
            joined =
                string
                .Join(
                    Environment.NewLine,
                    items.Select(_ =>
                        context.GetIndentedText(_.XEL)));
            actual = joined;
            expected = @" 
  B:(Floppy Disk)
+ C:"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting B and C roots only"
            );
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Class for testing type injection.
    /// </summary>
    private class Item : XBoundViewObjectImplementer { }
}
