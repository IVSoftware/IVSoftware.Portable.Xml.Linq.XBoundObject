using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
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

    private enum LocalSortAttributeOrder
    {
        text,
    }

    [Placement(EnumPlacement.UseXAttribute, "xattr")]
    private enum LocalXAttrEnum
    {
        Default,
        NonDefault,
    }


    [Placement(EnumPlacement.UseXBoundAttribute, "xba")]
    private enum LocalXBAEnum
    {
        Default,
        NonDefault,
    }


    [TestMethod]
    public void Test_PlaceExtensionWithArgs()
    {
        string actual, expected;

        var path = Path.Combine("C:", "Child Folder", "Leaf Folder");
        XElement xroot = new XElement("root");
        XElement? xelnew = null;
        PlacerResult result;
        subtestHandleNotFound();
        subtestPlacerKeysDictionary();
        subtestPlaceXObjects();

        subtestPlaceEnums();
        void subtestPlaceEnums() { }
        {
            xroot.RemoveAll();
            result = xroot.Place(
                path,
                out xelnew,
                LocalXAttrEnum.NonDefault,
                LocalXBAEnum.NonDefault
            );
            actual = xroot.SortAttributes<LocalSortAttributeOrder>().ToString();
            expected = @" 
<root>
  <xnode text=""C:"">
    <xnode text=""Child Folder"">
      <xnode text=""Leaf Folder"" xattr=""NonDefault"" xba=""[LocalXBAEnum.NonDefault]"" />
    </xnode>
  </xnode>
</root>";
            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting xattr is XAttribute and xba is XBoundAttrubute."
            );
            Assert.IsTrue(xelnew?.Has<Enum>(), $"Expecting Single {nameof(Enum)}.");
            // This was going to get found regardless.
            Assert.IsTrue(xelnew?.Has<LocalXBAEnum>(), $"Expecting Single {nameof(LocalXBAEnum)}.");
            // This, because of the attribute, will use the string fallback.
            Assert.IsTrue(xelnew?.Has<LocalXAttrEnum>(), $"Expecting Single {nameof(LocalXBAEnum)}.");

            // Same test, using nullable T? 
            Assert.IsTrue(xelnew?.Has<Enum?>(), $"Expecting Single {nameof(Enum)}.");
            // This was going to get found regardless.
            Assert.IsTrue(xelnew?.Has<LocalXBAEnum?>(), $"Expecting Single {nameof(LocalXBAEnum)}.");
            // This, because of the attribute, will use the string fallback.
            Assert.IsTrue(xelnew?.Has<LocalXAttrEnum?>(), $"Expecting Single {nameof(LocalXBAEnum)}.");
        }

        #region S U B T E S T S
        void subtestHandleNotFound()
        {
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
                Assert.Fail($"Expecting {nameof(KeyNotFoundException)} to be thrown. You shouldn't be here.");
            }
            catch (KeyNotFoundException ex)
            {
                // Pass! This exception SHOULD BE THROWN. It's what we're testing.
            }
        }

        void subtestPlacerKeysDictionary()
        {
            xroot.RemoveAll();
            result = xroot.Place(
                path,
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
        }

        void subtestPlaceXObjects()
        {
            xroot.RemoveAll();
            result = xroot.Place(
                path,
                new XAttribute("xattr", "XAttribute"),
                new XBoundAttribute(nameof(SortOrder), SortOrder.None),
                "This is a value"
            );

            actual = xroot.SortAttributes<LocalSortAttributeOrder>().ToString();
            expected = @" 
<root>
  <xnode text=""C:"">
    <xnode text=""Child Folder"">
      <xnode text=""Leaf Folder"" xattr=""XAttribute"" sortorder=""[SortOrder.None]"">This is a value</xnode>
    </xnode>
  </xnode>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting values to match."
            );
            try
            {
                xroot.RemoveAll();
                result = xroot.Place(
                    path,
                    new XAttribute("xattr", "XAttribute"),
                    new XBoundAttribute(nameof(SortOrder), SortOrder.None),
                    "This is a legal first value",
                    "This is an illegal second value"
                );
                Assert.Fail($"Expecting {nameof(InvalidOperationException)} to be thrown. You shouldn't be here.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(
                    typeof(InvalidOperationException),
                    ex.GetType(),
                    $"Expecting {nameof(InvalidOperationException)}");
            }
        }


        #endregion S U B T E S T S
    }
}
