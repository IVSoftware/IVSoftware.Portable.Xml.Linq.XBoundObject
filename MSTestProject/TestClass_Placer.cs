using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Xml.Linq;

namespace XBoundObjectMSTest;

[TestClass]
public class TestClass_Placer
{
    [TestMethod]
    public void Test_BasicPlacement()
    {
        string actual, expected;

        var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
        var xroot = new XElement("root");

        switch (xroot.Place(path, PlacerMode.FindOrPartial))
        {
            case PlacerResult.Partial:
                break;
            case PlacerResult.NotFound:
            case PlacerResult.Exists:
            case PlacerResult.Created:
            case PlacerResult.Assert:
            case PlacerResult.Throw:
            default:
                Assert.Fail($"Expecting {PlacerResult.Partial.ToFullKey()}");
                break;
        }

        switch (xroot.Place(path, out XElement xelnew))
        {
            case PlacerResult.Created:
                actual = xroot.ToString();
                expected = @" 
<root>
  <xnode text=""C:"">
    <xnode text=""Child Folder"">
      <xnode text=""Leaf Folder"" />
    </xnode>
  </xnode>
</root>";
                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );
                actual = xelnew.ToShallow().ToString();
                expected = @" 
<xnode text=""Leaf Folder"" />";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting values to match."
                );

                break;
            case PlacerResult.NotFound:
            case PlacerResult.Partial:
            case PlacerResult.Exists:
            case PlacerResult.Assert:
            case PlacerResult.Throw:
            default:
                Assert.Fail($"Expecting {PlacerResult.Created.ToFullKey()}");
                break;
        }

        // Attempting to place the same node should result in Exists
        switch (xroot.Place(path))
        {
            case PlacerResult.Exists:
                break;
            case PlacerResult.NotFound:
            case PlacerResult.Partial:
            case PlacerResult.Created:
            case PlacerResult.Assert:
            case PlacerResult.Throw:
            default:
                Assert.Fail($"Expecting {PlacerResult.Exists.ToFullKey()}");
                break;
        }
    }

    [TestMethod]
    public void Test_PlaceExtensionWithContext()
    {
        string actual, expected;

        var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
        var xroot = new XElement("root");


        //switch (xroot.Place(path, PlacerMode.FindOrPartial))
        //{

        //}
    }

    [TestMethod]
    public void Test_PlaceExtensionWithoutContext()
    {
        string actual, expected;

        var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
        var xroot = new XElement("root");

    }
}
