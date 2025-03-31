using System.Collections.ObjectModel;
using System.Configuration.Internal;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using IVSoftware.Portable;
using IVSoftware.Portable.Threading;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;
using XBoundObjectMSTest.TestClassesForXBVO;
using static IVSoftware.Portable.Threading.Extensions;

namespace XBoundObjectMSTest;

[TestClass]
public class TestClass_XBoundViewObject
{
    static Queue<SenderEventPair> SenderEventQueue = new ();

    static string[] AllowedCallers = [];

    [TestInitialize]
    public void TestInitialize()
    {
        SenderEventQueue.Clear();
        AllowedCallers = [];
    }

    /// <summary>
    /// Exhaustively verifies manual control over PlusMinus and IsVisible states without auto-sync. 
    /// Exercises granular tree expansion, collapsing, adhoc node visibility, and sorting behavior. 
    /// Tests precise effects of SyncList() and ensures no unintended AutoSync events.
    /// </summary>
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
        var xroot = 
            new XElement("root")
            .WithXBoundView(
                items, 
                indent: 2, 
                autoSyncEnabled: false,
                sortingEnabled: false
             );
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
            xroot.Show<Item>(origPath);

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
            actual = context.ToString();
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
            actual = context.ToString();
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
  <xnode text=""B:(Floppy Disk)"" datamodel=""[Item]"" />
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
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" datamodel=""[Item]"" />
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
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" datamodel=""[Item]"" />
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
            actual = context.ToString();

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
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" datamodel=""[Item]"" />
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
            actual = context.ToString();
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
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" datamodel=""[Item]"" />
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
            actual = context.ToString();
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
  <xnode text=""B:(Floppy Disk)"" isvisible=""True"" datamodel=""[Item]"" />
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
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting full expansion"
            );

            context.SyncList();
            actual = context.ToString();
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

    /// <summary>
    /// Verifies Expand() and Collapse() method behavior on bound items, including trailing path normalization,
    /// node provisioning, sorting (default and custom), and visibility effects. Confirms correct sync behavior
    /// following structural changes.
    /// </summary>
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
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollection<Item>(),
                    indent: 2,
                    sortingEnabled: false
                 );
            var context = xroot.To<ViewContext>();
            Assert.IsNotNull(context);
            await awaiter.WaitAsync();

            xroot.Show<Item>(@"C:\");
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
            Assert.AreEqual(PlusMinus.Leaf, item.Expand(ExpandDirection.ToItems));
            Assert.AreEqual(PlusMinus.Leaf, item.Expand(ExpandDirection.FromItems));
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
    <xnode datamodel=""[Item]"" text=""E"" />
    <xnode datamodel=""[Item]"" text=""F"" />
    <xnode datamodel=""[Item]"" text=""G"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""B:"">
    <xnode datamodel=""[Item]"" text=""A"" />
    <xnode datamodel=""[Item]"" text=""B"" />
    <xnode datamodel=""[Item]"" text=""C"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""D:"">
    <xnode datamodel=""[Item]"" text=""H"" />
    <xnode datamodel=""[Item]"" text=""I"" />
    <xnode datamodel=""[Item]"" text=""J"" />
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
            actual.ToClipboardExpected();
            { }
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode datamodel=""[Item]"" text=""B:"">
    <xnode datamodel=""[Item]"" text=""A"" />
    <xnode datamodel=""[Item]"" text=""B"" />
    <xnode datamodel=""[Item]"" text=""C"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"">
    <xnode datamodel=""[Item]"" text=""E"" />
    <xnode datamodel=""[Item]"" text=""F"" />
    <xnode datamodel=""[Item]"" text=""G"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""D:"">
    <xnode datamodel=""[Item]"" text=""H"" />
    <xnode datamodel=""[Item]"" text=""I"" />
    <xnode datamodel=""[Item]"" text=""J"" />
  </xnode>
</root>"
            ;

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
  <xnode datamodel=""[Item]"" text=""D:"">
    <xnode datamodel=""[Item]"" text=""J"" />
    <xnode datamodel=""[Item]"" text=""I"" />
    <xnode datamodel=""[Item]"" text=""H"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""C:"" isvisible=""True"">
    <xnode datamodel=""[Item]"" text=""G"" />
    <xnode datamodel=""[Item]"" text=""F"" />
    <xnode datamodel=""[Item]"" text=""E"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""B:"">
    <xnode datamodel=""[Item]"" text=""C"" />
    <xnode datamodel=""[Item]"" text=""B"" />
    <xnode datamodel=""[Item]"" text=""A"" />
  </xnode>
</root>"
            ;

            actual = context.ToString();

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

            // Wait for unintended sync events.
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            #region S U B T E S T S
            async Task subtestImplicitVisible()
            {
                xel = xroot.Descendants().Skip(7).First();
                xel.To<Item>().Expand(ExpandDirection.ToItems);
                await awaiter.WaitAsync();

                actual = context.ToString();
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
                    "Expecting item E is visible and downgraded to leaf."
                );
            }
            #endregion S U B T E S T S
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }
    }

    /// <summary>
    /// Validates use of the PlusMinusToggleCommand to trigger expand/collapse behavior via ICommand, 
    /// distinct from direct method or extension calls. Confirms UI-compatible toggling and subsequent 
    /// visibility logic, including partial and full expansion sequences.
    /// </summary>
    [TestMethod]
    public async Task Test_PlusMinusCommand()
    {
        string actual, expected;
        XElement? xel;
        Item xbvo;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot = 
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollection<Item>(),
                    indent: 2,
                    sortingEnabled: false
                 );
            var context = xroot.To<ViewContext>();
            Assert.IsNotNull(context);
            await awaiter.WaitAsync();

            string path =
                @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj"
                .Replace('\\', Path.DirectorySeparatorChar);
            awaiter.Wait(0);
            xroot.Show<Item>(path);
            await awaiter.WaitAsync();

            actual = context.ToString();
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
            xbvo = xel.To<Item>();
            Assert.AreEqual("Demo", xbvo.Text);
            xbvo.PlusMinusToggleCommand?.Execute(xbvo);
            Assert.AreEqual(
                PlusMinus.Collapsed, 
                xbvo.PlusMinus,
                $"Expecting item collapsed after toggle command.");
            await awaiter.WaitAsync();
            actual = context.ToString();
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

            Assert.IsInstanceOfType<Item>(
                xbvo.Parent.To<Item>(),
                $"Nevermind. Not using parent after all but nice to know it works as advertised.."
            );

            // Expand Demo again. Now it should just have direct child showing.
            xbvo.Expand(ExpandDirection.ToItems);
            await awaiter.WaitAsync();


            actual = context.ToString();
            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }

            expected = @" 
- C:
  - Github
    - IVSoftware
      - Demo
        + IVSoftware.Demo.CrossPlatform.FilesAndFolders"
            ;


            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting all (2) direct children of Demo node are visible."
            );
            // So far the only...
            await subtestPartialExpand();

            #region S U B T E S T S
            async Task subtestPartialExpand()
            {
                // Add a new file
                path = Path.Combine(xel.GetPath(), "README.md");

                xbvo = xroot.FindOrCreate<Item>(path);

                Assert.IsInstanceOfType<Item>(xbvo);
                await awaiter.WaitAsync();

                actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[Item]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[Item]"">
            <xnode text=""BasicPlacement.Maui"" datamodel=""[Item]"">
              <xnode text=""BasicPlacement.Maui.csproj"" datamodel=""[Item]"" />
            </xnode>
          </xnode>
          <xnode text=""README.md"" datamodel=""[Item]"" />
        </xnode>
      </xnode>
    </xnode>
  </xnode>
</root>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting new file"
                );

                actual = context.ToString();
                expected = @" 
- C:
  - Github
    - IVSoftware
      - Demo
        + IVSoftware.Demo.CrossPlatform.FilesAndFolders"
                ;
                Assert.AreEqual(
                    expected.NormalizeResult(), // Matches the SAME context as B4
                    actual.NormalizeResult(),
                    "Expecting no change to expected data. The new node is not visible."
                );

                xbvo.IsVisible = true;
                await awaiter.WaitAsync();
                actual = context.ToString();
                expected = @" 
- C:
  - Github
    - IVSoftware
      - Demo
        + IVSoftware.Demo.CrossPlatform.FilesAndFolders
          README.md";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting peers."
                );
                // Already shown. but this gets it.
                xel = xroot.Collapse("C:");
                await awaiter.WaitAsync();

                actual = context.ToString();
                expected = @" 
+ C:"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );

                xbvo.IsVisible = true;
                await awaiter.WaitAsync();

                actual = context.ToString();
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
            { }
            #endregion S U B T E S T S
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
        }
    }

    /// <summary>
    /// Verifies that sorting occurs automatically during sync without explicitly calling Sort(). 
    /// Tests both default alphanumeric sorting with Item and a custom reverse sorter using ItemEx, 
    /// ensuring correct order and visibility behavior with implicit sync triggers.
    /// </summary>
    [TestMethod]
    public async Task Test_DefaultSorting()
    {
        string actual, expected;
        XElement? xel;
        string path;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            AllowedCallers = new[] { "WDTAutoSync.RanToCompletion" }; // Artifact!
            Awaited += localOnAwaited;

            await subtestDefaultAlphaNumericBuiltIn();
            await subtestDefaultCustomAlphaNumericReverse();

            #region S U B T E S T S
            async Task subtestDefaultAlphaNumericBuiltIn()
            {
                var xroot =
                    new XElement("root")
                    .WithXBoundView(
                        items: new ObservableCollection<Item>(),
                        indent: 2
                );
                var context = xroot.To<ViewContext>();
                Assert.IsNotNull(context);
                await awaiter.WaitAsync();

                foreach (var tmp in new[]
                {
                    Path.Combine("D:", "I"),
                    Path.Combine("B:", "A"),
                    Path.Combine("B:", "B"),
                    Path.Combine("C:", "F"),
                    Path.Combine("B:", "C"),
                    Path.Combine("C:", "E"),
                    Path.Combine("C:", "G"),
                    Path.Combine("D:", "H"),
                    Path.Combine("D:", "J"),
                })
                {
                    xroot.FindOrCreate<Item>(tmp);
                }
                await awaiter.WaitAsync();

                actual = xroot.ToString();
                actual.ToClipboardExpected();
                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode datamodel=""[Item]"" text=""B:"">
    <xnode datamodel=""[Item]"" text=""A"" />
    <xnode datamodel=""[Item]"" text=""B"" />
    <xnode datamodel=""[Item]"" text=""C"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""C:"">
    <xnode datamodel=""[Item]"" text=""E"" />
    <xnode datamodel=""[Item]"" text=""F"" />
    <xnode datamodel=""[Item]"" text=""G"" />
  </xnode>
  <xnode datamodel=""[Item]"" text=""D:"">
    <xnode datamodel=""[Item]"" text=""H"" />
    <xnode datamodel=""[Item]"" text=""I"" />
    <xnode datamodel=""[Item]"" text=""J"" />
  </xnode>
</root>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting DEFAULT SORTED with NONE VISIBLE."
                );

                path = Path.Combine("D:", "I");
                SenderEventQueue.Clear();
                Assert.AreEqual(
                    PlacerResult.Exists,
                    xroot.Place(path, out xel),
                    "Expecting to find the existing node.");

                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // [Careful] Do not await the awaiter here!
                Assert.AreEqual(
                    0,
                    SenderEventQueue.Count,
                    $"Expecting Place (Exists) did 'not' make any changes to the XML."
                 );

                await subtestImplicitVisibleOnCollapse();
                async Task subtestImplicitVisibleOnCollapse()
                {
                    xel.To<Item>().Collapse();

                    await awaiter.WaitAsync();
                    actual = context.ToString();

                    actual.ToClipboard();
                    expected = @" 
* D:
    I";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting node is shown explicitly from collapsing."
                    );

                    // Wait for unintended sync events.
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            async Task subtestDefaultCustomAlphaNumericReverse()
            {
                var xroot =
                    new XElement("root")
                    .WithXBoundView(
                        items: new ObservableCollection<ItemEx>(),
                        indent: 2,
                        customSorter: (a, b) =>
                        {
                            if (a.To<IXBoundViewObject>() is IComparable<IXBoundViewObject> xbvoa &&
                                b.To<IXBoundViewObject>() is IXBoundViewObject xbvob)
                            {
                                return xbvoa.CompareTo(xbvob);
                            }
                            else
                            {
                                return
                                 (a
                                  .Attribute(nameof(StdAttributeNameXBoundViewObject.text))
                                  ?.Value ?? string.Empty)
                                  .CompareTo(
                                        b
                                        .Attribute(nameof(StdAttributeNameXBoundViewObject.text))
                                        ?.Value ?? string.Empty);
                            }
                        }
                    );

                var context = xroot.To<ViewContext>();
                Assert.IsNotNull(context);
                await awaiter.WaitAsync();

                foreach (var tmp in new[]
                {
                    Path.Combine("D:", "I"),
                    Path.Combine("B:", "A"),
                    Path.Combine("B:", "B"),
                    Path.Combine("C:", "F"),
                    Path.Combine("B:", "C"),
                    Path.Combine("C:", "E"),
                    Path.Combine("C:", "G"),
                    Path.Combine("D:", "H"),
                    Path.Combine("D:", "J"),
                })
                {
                    xroot.FindOrCreate<ItemEx>(tmp);
                }
                await awaiter.WaitAsync();

                actual = xroot.ToString();
                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode item=""[ItemEx]"" text=""D:"">
    <xnode item=""[ItemEx]"" text=""J"" />
    <xnode item=""[ItemEx]"" text=""I"" />
    <xnode item=""[ItemEx]"" text=""H"" />
  </xnode>
  <xnode item=""[ItemEx]"" text=""C:"">
    <xnode item=""[ItemEx]"" text=""G"" />
    <xnode item=""[ItemEx]"" text=""F"" />
    <xnode item=""[ItemEx]"" text=""E"" />
  </xnode>
  <xnode item=""[ItemEx]"" text=""B:"">
    <xnode item=""[ItemEx]"" text=""C"" />
    <xnode item=""[ItemEx]"" text=""B"" />
    <xnode item=""[ItemEx]"" text=""A"" />
  </xnode>
</root>"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting REVERSE SORTED with NONE VISIBLE. The xname s.b. 'item' per [DataModel] attribute"
                );

                path = Path.Combine("D:", "I");
                SenderEventQueue.Clear();
                Assert.AreEqual(
                    PlacerResult.Exists,
                    xroot.Place(path, out xel),
                    "Expecting to find the existing node.");

                await Task.Delay(TimeSpan.FromSeconds(0.25));

                // [Careful] Do not await the awaiter here!
                Assert.AreEqual(
                    0,
                    SenderEventQueue.Count,
                    $"Expecting Place (Exists) did 'not' make any changes to the XML."
                 );


                await subtestImplicitVisibleOnCollapse();
                async Task subtestImplicitVisibleOnCollapse()
                {
                    xel.To<ItemEx>().Collapse();
                    await awaiter.WaitAsync();
                    actual = context.ToString();

                    actual.ToClipboard();
                    expected = @" 
* D:
    I";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting node is shown explicitly from collapsing."
                    );

                    // Wait for unintended sync events.
                    Thread.Sleep(TimeSpan.FromSeconds(0.5));
                }
            }

            #endregion S U B T E S T S
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }
    }

    /// <summary>
    /// Tests both overloads of Show(): the non-generic read-only variant, which throws if the element is missing or unbound, 
    /// and the generic FindOrCreate variant, which binds and returns a concrete IXBoundViewObject. 
    /// Validates fluent creation of a file structure using DriveItem, FolderItem, and FileItem.
    /// </summary>
    [TestMethod]
    public void Test_Show()
    {
        string actual, expected;
        IXBoundViewObject xbvo;

        var xroot = new XElement("root");
        try
        {
            xroot.Show("C:");
            Assert.Fail($"Expecting {nameof(InvalidOperationException)} to be thrown. You shouldn't be here.");
        }
        catch (InvalidOperationException ex)
        {
            // Pass! This exception SHOULD BE THROWN. It's what we're testing.
            actual = ex.Message;
            expected = @" 
Element at path 'C:' is does not exist.";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting matching exception message."
            );
        }
        try
        {
            xroot.FindOrCreate("C:");

            actual = xroot.ToString();
            expected = @" 
<root>
  <xnode text=""C:"" />
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting nod exists, but is NOT BOUND"
            );

            xroot.Show("C:");
            Assert.Fail($"Expecting {nameof(InvalidOperationException)} to be thrown. You shouldn't be here.");
        }
        catch (InvalidOperationException ex)
        {
            // Pass! This exception SHOULD BE THROWN. It's what we're testing.
            actual = ex.Message;
            expected = @" 
Element at path 'C:' exists, but is not bound to an IXBoundViewObject. Ensure the element is correctly bound before calling Show()."
            ;
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting matching exception message."
            );
        }
        xroot.RemoveAll();
        xbvo = xroot.Show<DriveItem>("C:");

        actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
        expected = @" 
<root>
  <xnode text=""C:"" isvisible=""True"" datamodel=""[DriveItem]"" />
</root>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting properly set up drive node."
        );

        xbvo = 
            xbvo
            .XEL.FindOrCreate<FolderItem>("Users")
            .XEL.FindOrCreate<FolderItem>("Documents")
            .XEL.Show<FileItem>("README.md");

        actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();

        expected = @" 
<root>
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" isvisible=""True"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting fluent file structure creation."
        );
    }

    /// <summary>
    /// Validates sync behavior using an ObservableCollection (T = FilesystemItem) subclass,
    /// placing derived DriveItem, FolderItem, and FileItem instances. Verifies visibility updates,
    /// collapse/expand behavior, and fluent creation across polymorphic IXBoundViewObject types.
    /// </summary>
    [TestMethod]
    public async Task Test_PolymorphicSync()
    {
        string actual, expected;
        IXBoundViewObject xbvo;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollectionFSI(),
                    indent: 2
            );
            var context = xroot.To<ViewContext>();
            await awaiter.WaitAsync();
            await subtestFluentCreateFilesystem();
            await subtestCollapsePath();
            await subtestExpandAgain();

            #region S U B T E S T S
            async Task subtestFluentCreateFilesystem()
            {
                xbvo =
                    xroot.FindOrCreate<DriveItem>("C:")
                    .XEL.FindOrCreate<FolderItem>("Users")
                    .XEL.FindOrCreate<FolderItem>("Documents")
                    .XEL.Show<FileItem>("README.md");

                await awaiter.WaitAsync();

                actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" isvisible=""True"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting fluent-configured folders and files."
                );

                actual = context.ToString();
                expected = @" 
- C:
  - Users
    - Documents
        README.md";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting filesystem view (expanded)."
                );
            }

            async Task subtestCollapsePath()
            {
                xroot.Collapse(Path.Combine("C:", "Users"));
                await awaiter.WaitAsync();

                actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();


                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting Users is collapsed with no visible items below it."
                );

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting Users is collapsed with no visible items below it."
                );

                actual = context.ToString();
                expected = @" 
- C:
  + Users"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting Users is collapsed with no visible items below it."
                );

                // Wait for unintended sync events.
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            async Task subtestExpandAgain()
            {
                xroot.Collapse("C:");
                await awaiter.WaitAsync();
                { }

                actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
                actual.ToClipboard();
                actual.ToClipboardExpected();
                actual.ToClipboardAssert("Expecting Expecting only C is visible.");
                { }
                expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting Expecting only C is visible."
                );

                actual = context.ToString();

                actual.ToClipboardExpected();
                { }
                expected = @" 
+ C:"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting only C is visible"
                );
            }
            #endregion S U B T E S T S
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }
    }

    /// <summary>
    /// Ensures that SortAttributes() does not trigger unintended WDT AutoSync events by suppressing
    /// XObjectChange notifications. Confirms that genuine structural updates, such as Show(), still
    /// properly trigger synchronization.
    /// </summary>
    [TestMethod]
    public async Task Test_AttributeSortArtifacts()
    {
        string actual, expected;
        IXBoundViewObject xbvo;
        bool localIsSorting = false;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            if (localIsSorting)
            {
                Assert.Fail($"Expecting to SortAttributes without a sync event.");
            }
            else
            {
                switch (e.Caller)
                {
                    case "WDTAutoSync":
                        switch (e.Args)
                        {
                            case "InitialAction":
                                break;
                            case "RanToCompletion":
                                awaiter.Release();
                                break;
                            default: throw new NotImplementedException();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollectionFSI(),
                    indent: 2
            );
            var context = xroot.To<ViewContext>();
            await awaiter.WaitAsync();

            xbvo =
                xroot.FindOrCreate<DriveItem>("C:")
                .XEL.FindOrCreate<FolderItem>("Users")
                .XEL.FindOrCreate<FolderItem>("Documents")
                .XEL.Show<FileItem>("README.md");
            await awaiter.WaitAsync();

            // Sort events. This should NOT make an auto sync event.
            localIsSorting = true;
            xbvo.XEL.SortAttributes<StdAttributeNameXBoundViewObject>();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            localIsSorting = false;

            // This should make an autosync however!
            xroot.Show<FileItem>(Path.Combine("C:", "Users", "Documents", "dotnet_bot.png"));
            await awaiter.WaitAsync();
            { }
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }
    }

    [TestMethod]
    public async Task Test_DiagnoseExpandDoesDescendantsInsteadOfElements()
    {
        string actual, expected;
        IXBoundViewObject xbvo;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollectionFSI(),
                    indent: 2
            );
            xbvo =
                xroot.FindOrCreate<DriveItem>("C:")
                .XEL.FindOrCreate<FolderItem>("Users")
                .XEL.FindOrCreate<FolderItem>("Documents")
                .XEL.Show<FileItem>("README.md");
            var context = xroot.To<ViewContext>();
            await awaiter.WaitAsync();

            xbvo = xbvo.Parent.To<IXBoundViewObject>();
            xbvo.Collapse();
            await awaiter.WaitAsync();
            { }

            xbvo = xbvo.Parent.To<IXBoundViewObject>();
            xbvo.Collapse();
            await awaiter.WaitAsync();

            xbvo = xbvo.Parent.To<IXBoundViewObject>();
            xbvo.Collapse();
            await awaiter.WaitAsync();



#if false
            Debug.Assert(DateTime.Now.Date == new DateTime(2025, 3, 30).Date, "Don't forget disabled");
            context.AutoSyncEnabled = false;
#endif

            actual = context.ToString();
            expected = @" 
+ C:";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting collapsed to C."
            );


            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting child items vith null isvisible and plusminus attributes."
            );

            xbvo.Expand(ExpandDirection.ToItems);
            await awaiter.WaitAsync();
            { }

            actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();

            actual.ToClipboardExpected();
            { }

            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[DriveItem]"">
    <xnode text=""Users"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Documents"" datamodel=""[FolderItem]"">
        <xnode text=""README.md"" datamodel=""[FileItem]"" />
      </xnode>
    </xnode>
  </xnode>
</root>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting only the first child is showing with PlusMinus.Collapsed."
            );

            actual = context.ToString();
            actual.ToClipboard();
            actual.ToClipboardExpected();
            actual.ToClipboardAssert("Expecting only the first child is showing with PlusMinus.Collapsed.");
            { }
            expected = @" 
- C:
  + Users";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting only the first child is showing with PlusMinus.Collapsed."
            );
        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }
    }

    [TestMethod]
    public async Task Test_PortableInitFileSystem()
    {
        string actual, expected;
        IXBoundViewObject xbvo;

        SemaphoreSlim awaiter = new SemaphoreSlim(0, 1);
        void localOnAwaited(object? sender, AwaitedEventArgs e)
        {
            switch (e.Caller)
            {
                case "WDTAutoSync":
                    switch (e.Args)
                    {
                        case "InitialAction":
                            break;
                        case "RanToCompletion":
                            awaiter.Release();
                            break;
                        default: throw new NotImplementedException();
                    }
                    break;
                default:
                    break;
            }
        }
        try
        {
            Awaited += localOnAwaited;
            var xroot =
                new XElement("root")
                .WithXBoundView(
                    items: new ObservableCollection<FilesystemItem>(),
                    indent: 2
            );
            var context = xroot.To<ViewContext>();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                xroot.Show<DriveItem>(drive);
            }
            foreach (var path in
                     Enum.GetValues<Environment.SpecialFolder>()
                     .Select(_ => Environment.GetFolderPath(_))
                     .Where(_ => !string.IsNullOrWhiteSpace(_) && Directory.Exists(_)))
            {
                xroot.FindOrCreate<FolderItem>(path);
            }
            foreach (var drive in xroot.Elements().Select(_ => _.To<DriveItem>()))
            {
                drive.Expand(ExpandDirection.FromItems);
            }
            await awaiter.WaitAsync();

            actual = context.ToString();
            expected = @" 
+ C:
  D:
  E:
+ F:
  G:
  Z:";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting logical drives where C: and F: are not empty."
            );

            // View the results in a debugger watch window here.
            actual = 
                xroot
                .SortAttributes<StdAttributeNameXBoundViewObject>()
                .ToString();

            var cDrive = xroot.FindOrCreate<DriveItem>("C:");
            cDrive.PlusMinusToggleCommand?.Execute(cDrive);

            await awaiter.WaitAsync();
            actual = context.ToString();

            expected = @" 
- C:
  + Program Files
  + Program Files (x86)
  + ProgramData
  + Users
  + WINDOWS
  D:
  E:
+ F:
  G:
  Z:";

            var programFiles = cDrive.XEL.FindOrCreate<FolderItem>("Program Files");
            programFiles.PlusMinusToggleCommand?.Execute(programFiles);
            await awaiter.WaitAsync();

            actual = context.ToString();

            actual.ToClipboardExpected();
            { }
            expected = @" 
- C:
  - Program Files
      Common Files
  + Program Files (x86)
  + ProgramData
  + Users
  + WINDOWS
  D:
  E:
+ F:
  G:
  Z:"
            ;

            context.AutoSyncEnabled = false;
            programFiles.PlusMinusToggleCommand?.Execute(programFiles);

            // View the results in a debugger watch window here.
            actual =
                xroot
                .SortAttributes<StdAttributeNameXBoundViewObject>()
                .ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[DriveItem]"">
    <xnode text=""Program Files"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Common Files"" datamodel=""[FolderItem]"" />
    </xnode>
    <xnode text=""Program Files (x86)"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Common Files"" datamodel=""[FolderItem]"" />
    </xnode>
    <xnode text=""ProgramData"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Microsoft"" datamodel=""[FolderItem]"">
        <xnode text=""Windows"" datamodel=""[FolderItem]"">
          <xnode text=""Start Menu"" datamodel=""[FolderItem]"">
            <xnode text=""Programs"" datamodel=""[FolderItem]"">
              <xnode text=""Administrative Tools"" datamodel=""[FolderItem]"" />
              <xnode text=""Startup"" datamodel=""[FolderItem]"" />
            </xnode>
          </xnode>
          <xnode text=""Templates"" datamodel=""[FolderItem]"" />
        </xnode>
      </xnode>
    </xnode>
    <xnode text=""Users"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Public"" datamodel=""[FolderItem]"">
        <xnode text=""Desktop"" datamodel=""[FolderItem]"" />
        <xnode text=""Documents"" datamodel=""[FolderItem]"" />
        <xnode text=""Music"" datamodel=""[FolderItem]"" />
        <xnode text=""Pictures"" datamodel=""[FolderItem]"" />
        <xnode text=""Videos"" datamodel=""[FolderItem]"" />
      </xnode>
      <xnode text=""tgreg"" datamodel=""[FolderItem]"">
        <xnode text=""AppData"" datamodel=""[FolderItem]"">
          <xnode text=""Local"" datamodel=""[FolderItem]"">
            <xnode text=""Microsoft"" datamodel=""[FolderItem]"">
              <xnode text=""Windows"" datamodel=""[FolderItem]"">
                <xnode text=""Burn"" datamodel=""[FolderItem]"">
                  <xnode text=""Burn"" datamodel=""[FolderItem]"" />
                </xnode>
                <xnode text=""History"" datamodel=""[FolderItem]"" />
                <xnode text=""INetCache"" datamodel=""[FolderItem]"" />
                <xnode text=""INetCookies"" datamodel=""[FolderItem]"" />
              </xnode>
            </xnode>
          </xnode>
          <xnode text=""Roaming"" datamodel=""[FolderItem]"">
            <xnode text=""Microsoft"" datamodel=""[FolderItem]"">
              <xnode text=""Windows"" datamodel=""[FolderItem]"">
                <xnode text=""Network Shortcuts"" datamodel=""[FolderItem]"" />
                <xnode text=""Recent"" datamodel=""[FolderItem]"" />
                <xnode text=""SendTo"" datamodel=""[FolderItem]"" />
                <xnode text=""Start Menu"" datamodel=""[FolderItem]"">
                  <xnode text=""Programs"" datamodel=""[FolderItem]"">
                    <xnode text=""Administrative Tools"" datamodel=""[FolderItem]"" />
                    <xnode text=""Startup"" datamodel=""[FolderItem]"" />
                  </xnode>
                </xnode>
                <xnode text=""Templates"" datamodel=""[FolderItem]"" />
              </xnode>
            </xnode>
          </xnode>
        </xnode>
        <xnode text=""Favorites"" datamodel=""[FolderItem]"" />
        <xnode text=""Music"" datamodel=""[FolderItem]"" />
        <xnode text=""Videos"" datamodel=""[FolderItem]"" />
      </xnode>
    </xnode>
    <xnode text=""WINDOWS"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[FolderItem]"">
      <xnode text=""Fonts"" datamodel=""[FolderItem]"" />
      <xnode text=""resources"" datamodel=""[FolderItem]"" />
      <xnode text=""system32"" datamodel=""[FolderItem]"" />
      <xnode text=""SysWOW64"" datamodel=""[FolderItem]"" />
    </xnode>
  </xnode>
  <xnode text=""D:"" isvisible=""True"" datamodel=""[DriveItem]"" />
  <xnode text=""E:"" isvisible=""True"" datamodel=""[DriveItem]"" />
  <xnode text=""F:"" isvisible=""True"" plusminus=""Collapsed"" datamodel=""[DriveItem]"">
    <xnode text=""one-drive-ivsoftware"" datamodel=""[FolderItem]"">
      <xnode text=""OneDrive"" datamodel=""[FolderItem]"">
        <xnode text=""Desktop"" datamodel=""[FolderItem]"" />
        <xnode text=""Documents"" datamodel=""[FolderItem]"" />
        <xnode text=""Pictures"" datamodel=""[FolderItem]"" />
      </xnode>
    </xnode>
  </xnode>
  <xnode text=""G:"" isvisible=""True"" datamodel=""[DriveItem]"" />
  <xnode text=""Z:"" isvisible=""True"" datamodel=""[DriveItem]"" />
</root>"
            ;

            // Manual
            context.SyncList();

        }
        finally
        {
            awaiter.Wait(0);
            awaiter.Release();
            awaiter.Dispose();
            Awaited -= localOnAwaited;
        }

    }

    /// <summary>
    /// Class for testing automatic type injection.
    /// </summary>

    [DataModel]
    private class Item : XBoundViewObjectImplementer { }


    [DataModel(xname: "Item")]
    private class ItemEx : XBoundViewObjectImplementer, IComparable<IXBoundViewObject>
    {
        /// <summary>
        /// Do a reverse sort for no good reason except to test!
        /// </summary>
        public int CompareTo(IXBoundViewObject? other)
            => ((other?.Text) ?? string.Empty).CompareTo(this.Text);
    }
}
