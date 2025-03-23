using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Diagnostics;
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
        PlacerResult result;

        result = xroot.Place(path, out XElement xelnew);
        Assert.AreEqual(
            PlacerResult.Created,
            result,
            $"Expecting {PlacerResult.Created.ToFullKey()}");

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


        // Attempting to place the same node should result in Exists
        result = xroot.Place(path, PlacerMode.FindOrPartial);

        Assert.AreEqual(
            PlacerResult.Exists,
            result,
            $"Expecting {PlacerResult.Created.ToFullKey()}");
    }

    [TestMethod]
    public void Test_PlaceExtensionWithArgs()
    {
        string actual, expected;

        var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
        var xroot = new XElement("root");
        PlacerResult result;


        subtestHandleNotFound();
        xroot.RemoveAll();
        result = xroot.Place(
            path, 
            PlacerMode.FindOrCreate,
            new PlacerKeysDictionary
            {
                { StdPlacerKeys.NewXElementName, "xel" },
                { StdPlacerKeys.PathAttributeName, "label" }, 
            });

        Assert.AreEqual(
            PlacerResult.Created,
            result,
            $"Expecting {PlacerResult.Created.ToFullKey()}");


        actual = xroot.ToString();
        expected = @" 
<root>
  <xel label=""C:"">
    <xel label=""Child Folder"">
      <xel label=""Leaf Folder"" />
    </xel>
  </xel>
</root>";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting values to match."
        );

        #region S U B T E S T S
        void subtestHandleNotFound()
        {//Development block that (unlike a local function) allows edit and continue.

            result = xroot.Place(
                path,
                PlacerMode.FindOrPartial);
            try
            {
                result = xroot.Place(
                    path,
                    PlacerMode.FindOrAssert);
            }
            catch (Exception ex)
            {
                switch (ex.GetType().Name)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                    case "AssertFailedException":   // Correct response in Release mode
                    case "DebugAssertException":    // Correct response in Debug mode (but this is an MSTest internal class)
                        break;
                    default:
                        Assert.Fail("Expecting a different exception here.");
                        break;
                }
            }
            try
            {
                result = xroot.Place(
                    path,
                    PlacerMode.FindOrThrow);
                Debug.Fail($"Expecting {nameof(KeyNotFoundException)} to be thrown. You shouldn't be here.");
            }
            catch (KeyNotFoundException ex)
            {
                // Pass! This exception SHOULD BE THROWN. It's what we're testing.
            }
        }

        #endregion S U B T E S T S
    }
}
