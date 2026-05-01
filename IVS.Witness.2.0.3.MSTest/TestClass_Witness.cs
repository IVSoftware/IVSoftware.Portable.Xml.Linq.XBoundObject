using IVSoftware.Portable.Xml.Linq;

namespace IVS.Witness._2._0._3.MSTest
{
    [TestClass]
    public sealed class TestClass_Witness
    {
        [TestMethod]
        public void Test_Witness()
        {
            string actual, expected;

            string
                contractOrig =
                    typeof(XBoundAttribute)
                    .Assembly
                    .ToPublicContract()
                    .ToString();

#if false && SAVE
            // EmbeddedResource
            File.WriteAllText(@"Version=1.0.1.xml", contractOrig);
#endif
        }
    }
}
