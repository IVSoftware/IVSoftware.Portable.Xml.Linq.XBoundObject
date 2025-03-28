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
        var xroot = new XElement("root").UseXBoundView();

        // WinOS path in a WinOS test.
        var path =
            @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj";
        xroot.Show(path);

        actual = xroot.SortAttributes<StdAttributeNameXBoundViewObject>().ToString();
        actual.ToClipboard();
        actual.ToClipboardAssert();
        { }
    }
}
