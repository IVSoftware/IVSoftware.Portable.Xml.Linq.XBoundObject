using System.Collections.ObjectModel;
using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;

namespace XBoundObjectMSTest;

[TestClass]
public class TestClass_XBoundViewObject
{
    [TestMethod]
    public void Test_PlusMinus()
    {
        string actual, expected, path, joined;
        var items = new ObservableCollection<Item>();
        Item? item = null;
        var xroot = new XElement("root").UseXBoundView(items, 2);
        var context = xroot.To<ViewContext>(@throw: true);

        subtestShowPathSimpleThenSyncList();
        subtestCollapseDemoNode();
        subtestAddNotVisibleFloppyB();
        subtestMakeBDriveVisible();

        #region S U B T E S T S

        void subtestShowPathSimpleThenSyncList()
        {
            // WinOS path in a WinOS test.
            path =
                @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj";
            xroot.Show(path);

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
C:
  Github
    IVSoftware
      Demo
        IVSoftware.Demo.CrossPlatform.FilesAndFolders
          BasicPlacement.Maui
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
            path = Path.Combine("C:", "Github", "IVSoftware", "Demo");
            xroot.Collapse(path);

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
C:
  Github
    IVSoftware
      Demo";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting indented monospace text."
            );
        }

        void subtestAddNotVisibleFloppyB()
        {

            path = Path.Combine("B:(Floppy Disk)");
            item = xroot.FindOrCreate<Item>(path);
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

            actual.ToClipboard();
            actual.ToClipboardExpected();
            { }
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
C:
  Github
    IVSoftware
      Demo";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting B: is visible and sorted alphabetically."
            );
        }
        #endregion S U B T E S T S
    }

    /// <summary>
    /// Class for testing type injection.
    /// </summary>
    private class Item : XBoundViewObjectImplementer { }
}
