using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;

namespace XBoundObjectMSTest;

[TestClass]
public class TestClass_XBoundViewObject
{
    [TestMethod]
    public void Test()
    {
        string actual, expected;
        var xroot = new XElement("root").UseXBoundView();

        // WinOS path in a WinOS test.
        var path =
            @"C:\Github\IVSoftware\Demo\IVSoftware.Demo.CrossPlatform.FilesAndFolders\BasicPlacement.Maui\BasicPlacement.Maui.csproj";
        xroot.Show(path);
    }
}
