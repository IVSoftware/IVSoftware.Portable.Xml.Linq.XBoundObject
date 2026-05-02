using IVSoftware.WinOS.MSTest.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBoundObject.MSTest
{
    [TestClass]
    public class TestClass_PublicContract
    {
        [TestMethod]
        public void Test_GetBreakingChanges()
        {
            string actual, expected;
            string baseline;


            subtest_AssemblyOnly();
            subtest_AssemblyAndDependencies();

            #region S U B T E S T S
            void subtest_AssemblyOnly()
            {
                baseline =
                    "XBoundObject.MSTest.Witness.XBoundObject Version=2.0.3.xml"
                    .ReadManifestResourceFile<TestClass_PublicContract>();
                string revision =
                    typeof(IVSoftware.Portable.Xml.Linq.XBoundAttribute)
                    .Assembly
                    .ToPublicContract()
                    .ToString();

                if (baseline.IsContractValid(revision, ManifestTypePolicy.AssemblyOnly))
                {   /* G T K */
                }
                else
                {
                    var diff = baseline.GetBreakingChanges(revision, ManifestTypePolicy.AssemblyOnly);

                    actual = diff!.ToString(); ;
                    actual.ToClipboardExpected();
                    { }
                    // CODEX: The test is failing, returning what's shown #false
#if false
                    expected = @" 
<breakingChanges policy=""AssemblyOnly"">
  <namespace name=""IVSoftware.Portable.Xml.Linq.XBoundObject.Placement"">
    <type name=""PlacerKeysDictionary"">
      <method name=""GetAlternateLookup"" signature=""M:IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlacerKeysDictionary|GetAlternateLookup()-&gt;[external]"" />
      <method name=""TryGetAlternateLookup"" signature=""M:IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlacerKeysDictionary|TryGetAlternateLookup([external])-&gt;[external]"" />
      <property name=""Capacity"" type=""[external]"" canRead=""true"" canWrite=""false"" signature=""P:IVSoftware.Portable.Xml.Linq.XBoundObject.Placement.PlacerKeysDictionary|Capacity|[external]|true|false"" />
    </type>
  </namespace>
</breakingChanges>"
                    ;

#endif
                    expected = @" 
<breakingChanges policy=""AssemblyOnly"" />";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting no breaking changes. See diff for more."
                    );
                }
            }
            void subtest_AssemblyAndDependencies()
            {
                baseline =
                    "XBoundObject.MSTest.Witness.XBoundObject Version=2.0.3.Dependencies.xml"
                    .ReadManifestResourceFile<TestClass_PublicContract>();
                string revision =
                    typeof(IVSoftware.Portable.Xml.Linq.XBoundAttribute)
                    .Assembly
                    .ToPublicContract(ManifestTypePolicy.IVSoftwareAssembliesOnly)
                    .ToString();

                if (baseline.IsContractValid(revision, ManifestTypePolicy.IVSoftwareAssembliesOnly))
                {   /* G T K */
                }
                else
                {
                    var diff = baseline.GetBreakingChanges(revision, ManifestTypePolicy.IVSoftwareAssembliesOnly);

                    actual = diff!.ToString(); ;
                    actual.ToClipboardExpected();
                    { }
                    expected = @" 
<breakingChanges policy=""IVSoftwareAssembliesOnly"" />";

                    Assert.AreEqual(
                        expected.NormalizeResult(),
                        actual.NormalizeResult(),
                        "Expecting no breaking changes. See diff for more."
                    );
                }
            }

            #endregion S U B T E S T S
        }
    }
}
