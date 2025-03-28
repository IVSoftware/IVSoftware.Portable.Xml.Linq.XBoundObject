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
        string actual, expected;
        var items = new ObservableCollection<Item>()
        var xroot = new XElement("root").UseXBoundView();

        // WinOS path in a WinOS test.
        var path =
            @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj";
        xroot.Show(path);

        actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
        actual.ToClipboard();
        actual.ToClipboardAssert();
        { }
        expected = @" 
<root viewcontext=""[ViewContext]"">
  <xnode text=""C:"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
    <xnode text=""Github"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
      <xnode text=""IVSoftware"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
        <xnode text=""Demo"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
          <xnode text=""IVSoftware.Demo.CrossPlatform.FilesAndFolders"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
            <xnode text=""BasicPlacement.Maui"" isvisible=""True"" plusminus=""Expanded"" datamodel=""[XBoundViewObjectImplementer]"">
              <xnode text=""BasicPlacement.Maui.csproj"" isvisible=""True"" datamodel=""[XBoundViewObjectImplementer]"" />
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
            "Expecting an expanded tree culminating in a leaf node."
        );
        { }
    }

    private class Item : XBoundViewObjectImplementer { }
}
