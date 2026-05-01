using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;

namespace XBoundObject.MSTest;

[TestClass]
public class TestClass_ReadMeClaims
{
    [TestMethod]
    public void Test_Default()
    {

        string actual, expected;

        XElement xel = new XElement("xel");
        var person = new Person { Name = "Ada" };

        // Default
        xel.SetBoundAttributeValue(person);

        actual = xel.ToString();
        actual.ToClipboardExpected();
        { }
        expected = @" 
<xel person=""[Person]"" />";

        Assert.AreEqual(
            expected.NormalizeResult(),
            actual.NormalizeResult(),
            "Expecting result to match."
        );
    }
    private class Person
    {
        public string Name { get; set; } = string.Empty;
    }
}
