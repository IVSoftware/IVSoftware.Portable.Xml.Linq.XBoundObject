using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

    static bool _expectingAutoSyncEvents = false;

    private static void OnAwaited(object? sender, AwaitedEventArgs e)
    {
        switch (e.Caller)
        {
            case "OnPropertyChanged":
                break;
            case "WDTAutoSync":
                Assert.IsTrue(
                    _expectingAutoSyncEvents,
                    "Expecting SyncList() only occurs when requested."
                );
                break;
            default:
                break;
        }
        _eventQueue.Enqueue(new SenderEventPair(sender, e));        
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
        => Awaited += OnAwaited;

    [ClassCleanup]
    public static void ClassCleanup() 
        => Awaited -= OnAwaited;

    [TestInitialize]
    public void TestInitialize() => _eventQueue.Clear();

    [TestMethod]
    public void Test_PlusMinus()
    {
        string 
            actual, 
            expected,
            adhocPath,
            origPath =
                @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj",
            demoPath = 
                Path.Combine("C:", "Github", "IVSoftware", "Demo");
        var items = new ObservableCollection<Item>();
        Item? item = null;
        var xroot = new XElement("root").WithXBoundView(items, indent: 2, autoSyncEnabled: true);
        var context = xroot.To<ViewContext>(@throw: true);

        subtestShowPathSimpleThenSyncList();
        subtestCollapseDemoNode();
        subtestAddNotVisibleFloppyB();
        subtestMakeBDriveVisible();
        subtestExpandDemoNode();
        subtestCollapseC();
        subtestEnsureNoAutoSyncEvent();

        subtestExpandLeaf();

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
            actual = context.ItemsToString();
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
            actual = context.ItemsToString();
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
            actual = context.ItemsToString();

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
            actual = context.ItemsToString();
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
            actual = context.ItemsToString();
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
            actual = context.ItemsToString();
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

        void subtestEnsureNoAutoSyncEvent()
        {
            // If an unexpected event is received in this
            // time window, it will assert in OnAwaited.
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }
        #endregion S U B T E S T S
    }

    [TestMethod]
    public async Task Test_ExpandCollapseMethods()
    {
        string actual, expected;
        XElement? xel;
        Item item;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    awaiter.Release();
                    break;
                default:
                    break;
            }
        }
        try
        {
            _expectingAutoSyncEvents = true;
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollection<Item>(),
                    indent: 2);
            var context = xroot.To<ViewContext>();
            Assert.IsNotNull(context);

            xroot.Show(@"C:\");
            await awaiter.WaitAsync();

            actual = xroot.ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"" />
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting path is trimmed, resulting in ONE NOT TWO nodes for trailing delimiter."
            );
            xel = xroot.Descendants().First();
            item = xel.To<Item>();
            Assert.IsInstanceOfType<Item>(item);
            Assert.AreEqual(PlusMinus.Leaf, item.Expand());
            Assert.AreEqual(PlusMinus.Leaf, item.Expand(allowPartial: true));
            Assert.AreEqual(PlusMinus.Leaf, item.Collapse());

            foreach (var tmp in new[]
            {
                Path.Combine("B:", "A"),
                Path.Combine("B:", "B"),
                Path.Combine("B:", "C"),
                Path.Combine("C:", "E"),
                Path.Combine("C:", "F"),
                Path.Combine("C:", "G"),
                Path.Combine("D:", "H"),
                Path.Combine("D:", "I"),
                Path.Combine("D:", "J"),
            })
            {
                xroot.FindOrCreate<Item>(tmp);
            }
            await awaiter.WaitAsync();

            actual = xroot.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"">
    <xnode item=""[Item]"" text=""E"" />
    <xnode item=""[Item]"" text=""F"" />
    <xnode item=""[Item]"" text=""G"" />
  </xnode>
  <xnode item=""[Item]"" text=""B:"">
    <xnode item=""[Item]"" text=""A"" />
    <xnode item=""[Item]"" text=""B"" />
    <xnode item=""[Item]"" text=""C"" />
  </xnode>
  <xnode item=""[Item]"" text=""D:"">
    <xnode item=""[Item]"" text=""H"" />
    <xnode item=""[Item]"" text=""I"" />
    <xnode item=""[Item]"" text=""J"" />
  </xnode>
</root>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting 9 nodes UNSORTED provisioned with Item"
            );

            xroot.Sort();
            await awaiter.WaitAsync();

            actual = xroot.ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode item=""[Item]"" text=""B:"">
    <xnode item=""[Item]"" text=""A"" />
    <xnode item=""[Item]"" text=""B"" />
    <xnode item=""[Item]"" text=""C"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"">
    <xnode item=""[Item]"" text=""E"" />
    <xnode item=""[Item]"" text=""F"" />
    <xnode item=""[Item]"" text=""G"" />
  </xnode>
  <xnode item=""[Item]"" text=""D:"">
    <xnode item=""[Item]"" text=""H"" />
    <xnode item=""[Item]"" text=""I"" />
    <xnode item=""[Item]"" text=""J"" />
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting SORTED"
            );

            // Reverse sort
            xroot.Sort(( a,  b)
                => (b
                    .Attribute(nameof(StdAttributeNameXBoundViewObject.text))
                    ?.Value ?? string.Empty)
                    .CompareTo(
                    a
                    .Attribute(nameof(StdAttributeNameXBoundViewObject.text))
                    ?.Value ?? string.Empty));

            await awaiter.WaitAsync();

            actual = xroot.ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode item=""[Item]"" text=""D:"">
    <xnode item=""[Item]"" text=""J"" />
    <xnode item=""[Item]"" text=""I"" />
    <xnode item=""[Item]"" text=""H"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"">
    <xnode item=""[Item]"" text=""G"" />
    <xnode item=""[Item]"" text=""F"" />
    <xnode item=""[Item]"" text=""E"" />
  </xnode>
  <xnode item=""[Item]"" text=""B:"">
    <xnode item=""[Item]"" text=""C"" />
    <xnode item=""[Item]"" text=""B"" />
    <xnode item=""[Item]"" text=""A"" />
  </xnode>
</root>"
            ;

            actual = context.ItemsToString();

            actual.ToClipboard();
            actual.ToClipboardExpected();
            expected = @" 
  C:";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ONLY the root is visible"
            );


            await subtestImplicitVisible();
            async Task subtestImplicitVisible()
            {
                xel = xroot.Descendants().Skip(7).First();
                xel.To<Item>().Expand();
                await awaiter.WaitAsync();
                { }
            }


            actual = context.ItemsToString();
            actual.ToClipboard();
            actual.ToClipboardExpected();
            actual.ToClipboardAssert();
            { }
            expected = @" 
* C:
    E";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting values to match."
            );

            // Wait for unintended sync events.
            Thread.Sleep(TimeSpan.FromSeconds(0.5));
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
            _expectingAutoSyncEvents = false;
        }
    }

    [TestMethod]
    public async Task Test_PlusMinusCommand()
    {
        string actual, expected;
        XElement? xel;
        Item item;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    awaiter.Release();
                    break;
                default:
                    break;
            }
        }
        try
        {
            _expectingAutoSyncEvents = true;
            Awaited += localOnAwaited;
            var xroot = 
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollection<Item>(),
                    indent: 2);
            var context = xroot.To<ViewContext>();
            Assert.IsNotNull(context);

            string path =
                @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj"
                .Replace('\\', Path.DirectorySeparatorChar);
            awaiter.Wait(0);
            xroot.Show(path);
            await awaiter.WaitAsync();

            actual = context.ItemsToString();
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
                "Expecting tree expanded to '.csproj' file"
            );
            xel = xroot.Descendants().Skip(3).First();
            item = xel.To<Item>();
            Assert.AreEqual("Demo", item.Text);
            item.PlusMinusToggleCommand?.Execute(item);
            Assert.AreEqual(
                PlusMinus.Collapsed, 
                item.PlusMinus,
                $"Expecting item collapsed after toggle command.");
            await awaiter.WaitAsync();
            actual = context.ItemsToString();
            expected = @" 
- C:
  - Github
    - IVSoftware
      + Demo"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting collapsed Demo node"
            );

            await subtestPartialExpand();
            async Task subtestPartialExpand()
            {
                path = Path.Combine(xel.GetPath(), "README.md");
                item = xroot.FindOrCreate<Item>(path);
                Assert.IsInstanceOfType<Item>(item);
                await awaiter.WaitAsync();
                actual = context.ItemsToString();
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting no change to expected data. The new node is not visible."
                );
                item.IsVisible = true;
                await awaiter.WaitAsync();
                actual = context.ItemsToString();
                expected = @" 
- C:
  - Github
    - IVSoftware
      * Demo
          README.md";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting partial expansion."
                );
            }
            var parent = item.Parent.To<Item>();
            Assert.IsInstanceOfType<Item>(item);
            parent.Expand();
            await awaiter.WaitAsync();
            actual = context.ItemsToString();


            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
            expected = @" 
- C:
  - Github
    - IVSoftware
      - Demo
        - IVSoftware.Demo.CrossPlatform.FilesAndFolders
          - BasicPlacement.Maui
              BasicPlacement.Maui.csproj
          README.md"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting full expansion of Demo node."
            );
        }
        catch
        {
            throw new Exception("You shouldn't be here.");
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
            _expectingAutoSyncEvents = false;
        }
    }

    /// <summary>
    /// Class for testing automatic type injection.
    /// </summary>
    private class Item : XBoundViewObjectImplementer { }
}
