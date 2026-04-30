using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Common.Attributes;
using IVSoftware.Portable.Common.Exceptions;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.MSTest.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Offsettors.MSTest
{
    [TestClass]
    public sealed class TestClass_260424_Offsettors
    {
        // STEP THREE
        // - "AND we will need to RENAME to reflect the new intent."
        private static XElement? _nextSibMarkedAbove(XElement cxel) =>
            cxel.NextNode is XElement { } xnext
            && bool.TryParse(xnext.Attribute(StdOffsettorAttribute.above)?.Value, out var @bool)
            && @bool
            ? xnext
            : null;


        /// <summary>
        /// Generates a deterministic flat-to-hierarchal modeled test fixture.
        /// </summary>
        /// <remarks>
        /// Optimized for offsettor tests that need repeatable scatter, depth,
        /// and filter behavior.
        /// 
        /// Requires <c>DoNotParallelize</c> because <c>TestableEpoch</c> is
        /// static and must remain isolated per test.
        /// 
        /// Intentionally mixes item nodes with default nodes so filtered
        /// traversal can prove it skips, lands, and terminates correctly.
        /// 
        /// Only item nodes receive index attributes, which keeps assertions
        /// aligned with the modeled collection rather than helper path nodes.
        /// </remarks>
        private class OCMLocal : ObservableCollection<PlaceableModel>
        {
            /// <summary>
            /// Builds a seeded scatter of modeled items with bounded path depth.
            /// </summary>
            /// <remarks>
            /// Accepts <paramref name="count"/> and <paramref name="seed"/> so
            /// tests can generate arbitrary but fully repeatable structures.
            /// 
            /// The generated model starts from a flat source collection and
            /// projects it into hierarchal XML through placed full paths.
            /// </remarks>
            public OCMLocal(int count, int seed = 1, int maxDepth = 2)
            {
                Rando = new(seed);

                string[] guids =
                    Enumerable.Range(0, count)
                    .Select(_ => new Guid().WithTestability().ToString())
                    .ToArray();
                for (int i = 0; i < count; i++)
                {
                    HashSet<string> visited = new();
                    int length = Rando.Next(maxDepth + 1);
                    List<string> segments = new();
                    for (int depth = 0; depth < length; depth++)
                    {
                        string segment;
                        while (!visited.Add(segment = guids[Rando.Next(count)])) ;
                        segments.Add(segment);
                    }
                    segments.Add(guids[i]);
                    var fullPath = string.Join('\\', segments);
                    Model.Place(fullPath, out var xel);
                    xel.SetBoundAttributeValue(new PlaceableModel(fullPath), StdModelAttribute.model);
                }
                foreach (var xel in Model.Descendants())
                {
                    if (xel.To<PlaceableModel>() is { } item)
                    {
                        xel.Name = nameof(StdModelElement.item);
                        xel.SetStdAttributeValue(StdModelAttribute.index, Count);
                        Add(item);
                        item.Description = $"Item{Count:D2}";
                    }
                }
            }
            public XElement Model { get; } =
                StdModelElement.model.MakeXElement();
            public Random Rando { get; }
        }




        [TestMethod, DoNotParallelize]
        public void Test_OCMLocal_CTor()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            var ocm = new OCMLocal(25, 10, 2);

            actual = ocm.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000012"" model=""[PlaceableModel]"" index=""0"">
    <xnode text=""312d1c21-0000-0000-0000-000000000011"">
      <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""1"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-000000000017"" model=""[PlaceableModel]"" index=""2"" />
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000007"">
    <xnode text=""312d1c21-0000-0000-0000-000000000009"">
      <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""3"" />
    </xnode>
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000005"">
    <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""4"" />
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000010"" model=""[PlaceableModel]"" index=""5"">
    <xnode text=""312d1c21-0000-0000-0000-000000000006"">
      <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""6"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""7"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""8"">
    <xnode text=""312d1c21-0000-0000-0000-000000000001"">
      <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""9"" />
    </xnode>
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000004"">
    <xnode text=""312d1c21-0000-0000-0000-000000000003"">
      <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[PlaceableModel]"" index=""10"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[PlaceableModel]"" index=""11"" />
    <item text=""312d1c21-0000-0000-0000-000000000016"" model=""[PlaceableModel]"" index=""12"" />
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[PlaceableModel]"" index=""13"" />
  <xnode text=""312d1c21-0000-0000-0000-00000000000d"">
    <xnode text=""312d1c21-0000-0000-0000-000000000010"">
      <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[PlaceableModel]"" index=""14"" />
    </xnode>
    <xnode text=""312d1c21-0000-0000-0000-000000000016"">
      <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[PlaceableModel]"" index=""15"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[PlaceableModel]"" index=""16"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[PlaceableModel]"" index=""17"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000002"">
    <xnode text=""312d1c21-0000-0000-0000-000000000007"">
      <item text=""312d1c21-0000-0000-0000-00000000000d"" model=""[PlaceableModel]"" index=""18"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-00000000000e"" model=""[PlaceableModel]"" index=""19"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[PlaceableModel]"" index=""20"" />
  <item text=""312d1c21-0000-0000-0000-000000000011"" model=""[PlaceableModel]"" index=""21"" />
  <item text=""312d1c21-0000-0000-0000-000000000013"" model=""[PlaceableModel]"" index=""22"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000015"">
    <item text=""312d1c21-0000-0000-0000-000000000015"" model=""[PlaceableModel]"" index=""23"" />
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000003"">
    <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[PlaceableModel]"" index=""24"" />
  </xnode>
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting test set with mix of item + proxy at various depth."
            );
        }

        /// <summary>
        /// Verifies reverse modeled traversal from leaves, root, and anchors.
        /// </summary>
        /// <remarks>
        /// Uses seeded OCMLocal scatters to prove includeSelf and item-only
        /// filtering over mixed item and default nodes.
        /// </remarks>
        [TestMethod, DoNotParallelize]
        public void Test_Ascendor()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            string[] builder;
            OCMLocal ocm = null!;
            ocm = new OCMLocal(count: 10, seed: 1);
                actual = ocm.Model.ToString();
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"">
    <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""1"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""2"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[PlaceableModel]"" index=""3"">
    <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""4"" />
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000004"">
    <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""5"" />
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000009"">
    <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""6"" />
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[PlaceableModel]"" index=""8"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000006"">
    <xnode text=""312d1c21-0000-0000-0000-000000000002"">
      <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[PlaceableModel]"" index=""9"" />
    </xnode>
  </xnode>
</model>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting test set with mix of item + default at various depth."
                );

                subtest_AscendFromLast();
                subtest_AscendFromModel();
                subtest_Offsettor();

            te.ResetEpoch();
            ocm = new OCMLocal(count: 25, seed: 1);

                actual = ocm.Model.ToString(); ;
                actual.ToClipboardExpected();
                { }
                expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"">
    <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""1"" />
    <xnode text=""312d1c21-0000-0000-0000-000000000018"">
      <item text=""312d1c21-0000-0000-0000-000000000012"" model=""[PlaceableModel]"" index=""2"" />
    </xnode>
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""3"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000013"">
    <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""4"" />
    <item text=""312d1c21-0000-0000-0000-00000000000e"" model=""[PlaceableModel]"" index=""5"" />
  </xnode>
  <item text=""312d1c21-0000-0000-0000-00000000000a"" model=""[PlaceableModel]"" index=""6"">
    <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""7"" />
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000017"">
    <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""8"" />
    <xnode text=""312d1c21-0000-0000-0000-000000000002"">
      <item text=""312d1c21-0000-0000-0000-00000000000c"" model=""[PlaceableModel]"" index=""9"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""10"" />
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[PlaceableModel]"" index=""11"">
    <xnode text=""312d1c21-0000-0000-0000-000000000014"">
      <item text=""312d1c21-0000-0000-0000-000000000010"" model=""[PlaceableModel]"" index=""12"" />
    </xnode>
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[PlaceableModel]"" index=""13"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000011"">
    <xnode text=""312d1c21-0000-0000-0000-000000000010"">
      <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[PlaceableModel]"" index=""14"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-00000000000b"" model=""[PlaceableModel]"" index=""15"" />
    <xnode text=""312d1c21-0000-0000-0000-000000000009"">
      <item text=""312d1c21-0000-0000-0000-000000000016"" model=""[PlaceableModel]"" index=""16"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-00000000000d"" model=""[PlaceableModel]"" index=""17"">
    <xnode text=""312d1c21-0000-0000-0000-000000000011"">
      <item text=""312d1c21-0000-0000-0000-000000000011"" model=""[PlaceableModel]"" index=""18"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-000000000014"" model=""[PlaceableModel]"" index=""19"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-00000000000f"" model=""[PlaceableModel]"" index=""20"" />
  <item text=""312d1c21-0000-0000-0000-000000000015"" model=""[PlaceableModel]"" index=""21"">
    <xnode text=""312d1c21-0000-0000-0000-000000000001"">
      <item text=""312d1c21-0000-0000-0000-000000000013"" model=""[PlaceableModel]"" index=""22"" />
    </xnode>
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000010"">
    <item text=""312d1c21-0000-0000-0000-000000000017"" model=""[PlaceableModel]"" index=""23"" />
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-00000000000b"">
    <xnode text=""312d1c21-0000-0000-0000-000000000006"">
      <item text=""312d1c21-0000-0000-0000-000000000018"" model=""[PlaceableModel]"" index=""24"" />
    </xnode>
  </xnode>
</model>";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting test set with mix of item + default at various depth."
                );
                subtest_AscendFromOffsettor();

            #region S U B T E S T S
            void subtest_AscendFromModel()
            {
                builder =
                    ocm.Model
                    .Ascendors(includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
model"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );

                builder =
                    ocm.Model
                    .Ascendors(includeSelf: false)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_AscendFromLast()
            {
                var xlast = ocm.Model.Descendants().Last();

                builder =
                    xlast
                    .Ascendors(includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
9  312d1c21-0000-0000-0000-000000000009 Item10    
-  312d1c21-0000-0000-0000-000000000002
-  312d1c21-0000-0000-0000-000000000006
8  312d1c21-0000-0000-0000-000000000008 Item09    
7  312d1c21-0000-0000-0000-000000000005 Item08    
6  312d1c21-0000-0000-0000-000000000004 Item07    
-  312d1c21-0000-0000-0000-000000000009
5  312d1c21-0000-0000-0000-000000000003 Item06    
-  312d1c21-0000-0000-0000-000000000004
4  312d1c21-0000-0000-0000-000000000002 Item05    
3  312d1c21-0000-0000-0000-000000000007 Item04    
2  312d1c21-0000-0000-0000-000000000001 Item03    
1  312d1c21-0000-0000-0000-000000000006 Item02    
0  312d1c21-0000-0000-0000-000000000000 Item01    
model"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );

                builder =
                    xlast
                    .Ascendors(includeSelf: false)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
-  312d1c21-0000-0000-0000-000000000002
-  312d1c21-0000-0000-0000-000000000006
8  312d1c21-0000-0000-0000-000000000008 Item09    
7  312d1c21-0000-0000-0000-000000000005 Item08    
6  312d1c21-0000-0000-0000-000000000004 Item07    
-  312d1c21-0000-0000-0000-000000000009
5  312d1c21-0000-0000-0000-000000000003 Item06    
-  312d1c21-0000-0000-0000-000000000004
4  312d1c21-0000-0000-0000-000000000002 Item05    
3  312d1c21-0000-0000-0000-000000000007 Item04    
2  312d1c21-0000-0000-0000-000000000001 Item03    
1  312d1c21-0000-0000-0000-000000000006 Item02    
0  312d1c21-0000-0000-0000-000000000000 Item01    
model"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );

                builder =
                    xlast
                    .Ascendors(StdModelElement.item, includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
9  312d1c21-0000-0000-0000-000000000009 Item10    
8  312d1c21-0000-0000-0000-000000000008 Item09    
7  312d1c21-0000-0000-0000-000000000005 Item08    
6  312d1c21-0000-0000-0000-000000000004 Item07    
5  312d1c21-0000-0000-0000-000000000003 Item06    
4  312d1c21-0000-0000-0000-000000000002 Item05    
3  312d1c21-0000-0000-0000-000000000007 Item04    
2  312d1c21-0000-0000-0000-000000000001 Item03    
1  312d1c21-0000-0000-0000-000000000006 Item02    
0  312d1c21-0000-0000-0000-000000000000 Item01    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
                builder =
                    xlast
                    .Ascendors(StdModelElement.item, includeSelf: false)
                    .Select(_ => _.Formatted())
                    .ToArray();
                { }

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
8  312d1c21-0000-0000-0000-000000000008 Item09    
7  312d1c21-0000-0000-0000-000000000005 Item08    
6  312d1c21-0000-0000-0000-000000000004 Item07    
5  312d1c21-0000-0000-0000-000000000003 Item06    
4  312d1c21-0000-0000-0000-000000000002 Item05    
3  312d1c21-0000-0000-0000-000000000007 Item04    
2  312d1c21-0000-0000-0000-000000000001 Item03    
1  312d1c21-0000-0000-0000-000000000006 Item02    
0  312d1c21-0000-0000-0000-000000000000 Item01    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_Offsettor()
            {
                for (int i = 0; i < ocm.Count; i++)
                {
                    var ddor = ocm.Model.OffsettorAt(
                        StdModelElement.item, +i, OffsetZeroPolicy.FirstFilterMatch);
                    Assert.AreEqual(
                        i,
                        int.Parse(ddor?.Attribute(StdModelAttribute.index)?.Value ?? string.Empty));
                }
                { }
            }
            void subtest_AscendFromOffsettor()
            {
                // TODO
                // var ddor = ocm.Model.OffsettorAt(StdModelElement.item, 4);
            }
            #endregion S U B T E S T S
        }

        [TestMethod, DoNotParallelize]
        public void Test_Descendor()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            string[] builder;
            var ocm = new OCMLocal(count: 10, seed: 2);
            actual = ocm.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <xnode text=""312d1c21-0000-0000-0000-000000000004"">
    <xnode text=""312d1c21-0000-0000-0000-000000000001"">
      <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"" />
    </xnode>
    <xnode text=""312d1c21-0000-0000-0000-000000000002"">
      <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""1"" />
    </xnode>
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000001"">
    <xnode text=""312d1c21-0000-0000-0000-000000000003"">
      <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""2"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""3"">
    <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[PlaceableModel]"" index=""4"" />
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000000"">
    <xnode text=""312d1c21-0000-0000-0000-000000000005"">
      <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""5"" />
    </xnode>
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000002"">
    <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""6"" />
    <xnode text=""312d1c21-0000-0000-0000-000000000007"">
      <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""7"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[PlaceableModel]"" index=""8"" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[PlaceableModel]"" index=""9"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting test set with mix of item + default at various depth."
            );

            subtest_DescendFromModel();
            subtest_DescendFromFirst();
            subtest_DescendFromOffsettor();

            #region S U B T E S T S
            void subtest_DescendFromModel()
            {
                builder =
                    ocm.Model
                    .Descendors(includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
model
-  312d1c21-0000-0000-0000-000000000004
-  312d1c21-0000-0000-0000-000000000001
0  312d1c21-0000-0000-0000-000000000000 Item01    
-  312d1c21-0000-0000-0000-000000000002
1  312d1c21-0000-0000-0000-000000000002 Item02    
-  312d1c21-0000-0000-0000-000000000001
-  312d1c21-0000-0000-0000-000000000003
2  312d1c21-0000-0000-0000-000000000001 Item03    
3  312d1c21-0000-0000-0000-000000000003 Item04    
4  312d1c21-0000-0000-0000-000000000009 Item05    
-  312d1c21-0000-0000-0000-000000000000
-  312d1c21-0000-0000-0000-000000000005
5  312d1c21-0000-0000-0000-000000000004 Item06    
-  312d1c21-0000-0000-0000-000000000002
6  312d1c21-0000-0000-0000-000000000005 Item07    
-  312d1c21-0000-0000-0000-000000000007
7  312d1c21-0000-0000-0000-000000000006 Item08    
8  312d1c21-0000-0000-0000-000000000007 Item09    
9  312d1c21-0000-0000-0000-000000000008 Item10    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );

                builder =
                    ocm.Model
                    .Descendors(StdModelElement.item, includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
0  312d1c21-0000-0000-0000-000000000000 Item01    
1  312d1c21-0000-0000-0000-000000000002 Item02    
2  312d1c21-0000-0000-0000-000000000001 Item03    
3  312d1c21-0000-0000-0000-000000000003 Item04    
4  312d1c21-0000-0000-0000-000000000009 Item05    
5  312d1c21-0000-0000-0000-000000000004 Item06    
6  312d1c21-0000-0000-0000-000000000005 Item07    
7  312d1c21-0000-0000-0000-000000000006 Item08    
8  312d1c21-0000-0000-0000-000000000007 Item09    
9  312d1c21-0000-0000-0000-000000000008 Item10    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_DescendFromFirst()
            {
                var xfirst = ocm.Model.Descendors().First();

                builder =
                    xfirst
                    .Descendors(includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
-  312d1c21-0000-0000-0000-000000000004
-  312d1c21-0000-0000-0000-000000000001
0  312d1c21-0000-0000-0000-000000000000 Item01    
-  312d1c21-0000-0000-0000-000000000002
1  312d1c21-0000-0000-0000-000000000002 Item02    
-  312d1c21-0000-0000-0000-000000000001
-  312d1c21-0000-0000-0000-000000000003
2  312d1c21-0000-0000-0000-000000000001 Item03    
3  312d1c21-0000-0000-0000-000000000003 Item04    
4  312d1c21-0000-0000-0000-000000000009 Item05    
-  312d1c21-0000-0000-0000-000000000000
-  312d1c21-0000-0000-0000-000000000005
5  312d1c21-0000-0000-0000-000000000004 Item06    
-  312d1c21-0000-0000-0000-000000000002
6  312d1c21-0000-0000-0000-000000000005 Item07    
-  312d1c21-0000-0000-0000-000000000007
7  312d1c21-0000-0000-0000-000000000006 Item08    
8  312d1c21-0000-0000-0000-000000000007 Item09    
9  312d1c21-0000-0000-0000-000000000008 Item10    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );

                builder =
                    xfirst
                    .Descendors(StdModelElement.item, includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
0  312d1c21-0000-0000-0000-000000000000 Item01    
1  312d1c21-0000-0000-0000-000000000002 Item02    
2  312d1c21-0000-0000-0000-000000000001 Item03    
3  312d1c21-0000-0000-0000-000000000003 Item04    
4  312d1c21-0000-0000-0000-000000000009 Item05    
5  312d1c21-0000-0000-0000-000000000004 Item06    
6  312d1c21-0000-0000-0000-000000000005 Item07    
7  312d1c21-0000-0000-0000-000000000006 Item08    
8  312d1c21-0000-0000-0000-000000000007 Item09    
9  312d1c21-0000-0000-0000-000000000008 Item10    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_DescendFromOffsettor()
            {
                var ddor = ocm.Model.OffsettorAt(
                    StdModelElement.item,
                    +4,
                    OffsetZeroPolicy.FirstFilterMatch);

                builder =
                    ddor!
                    .Descendors(StdModelElement.item, includeSelf: true)
                    .Select(_ => _.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
4  312d1c21-0000-0000-0000-000000000009 Item05    
5  312d1c21-0000-0000-0000-000000000004 Item06    
6  312d1c21-0000-0000-0000-000000000005 Item07    
7  312d1c21-0000-0000-0000-000000000006 Item08    
8  312d1c21-0000-0000-0000-000000000007 Item09    
9  312d1c21-0000-0000-0000-000000000008 Item10    "
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }
            #endregion S U B T E S T S
        }

        [TestMethod, DoNotParallelize]
        public void Test_EdgeSentinels()
        {
            string actual, expected;
            using var te = this.TestableEpoch();


            var ocm = new OCMLocal(count: 10, seed: 3);

            #region L o c a l F x
            var builderThrow = new List<string>();
            void localOnBeginThrowOrAdvise(object? sender, Throw e)
            {
                builderThrow.Add($"{e.Mode}: {e.Message}");
                e.Handled = true;
            }
            #endregion L o c a l F x
            using var local = this.WithOnDispose(
                onInit: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise += localOnBeginThrowOrAdvise;
                },
                onDispose: (sender, e) =>
                {
                    Throw.BeginThrowOrAdvise -= localOnBeginThrowOrAdvise;
                });

            actual = ocm.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"" />
  <xnode text=""312d1c21-0000-0000-0000-000000000008"">
    <xnode text=""312d1c21-0000-0000-0000-000000000001"">
      <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""1"" />
    </xnode>
  </xnode>
  <xnode text=""312d1c21-0000-0000-0000-000000000001"">
    <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""2"" />
    <xnode text=""312d1c21-0000-0000-0000-000000000008"">
      <item text=""312d1c21-0000-0000-0000-000000000007"" model=""[PlaceableModel]"" index=""3"" />
    </xnode>
  </xnode>
  <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""4"">
    <xnode text=""312d1c21-0000-0000-0000-000000000005"">
      <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""5"" />
    </xnode>
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""6"">
    <xnode text=""312d1c21-0000-0000-0000-000000000000"">
      <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""7"" />
    </xnode>
  </item>
  <xnode text=""312d1c21-0000-0000-0000-000000000004"">
    <xnode text=""312d1c21-0000-0000-0000-000000000002"">
      <item text=""312d1c21-0000-0000-0000-000000000008"" model=""[PlaceableModel]"" index=""8"" />
    </xnode>
    <item text=""312d1c21-0000-0000-0000-000000000009"" model=""[PlaceableModel]"" index=""9"" />
  </xnode>
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting test set with mix of item + default at various depth."
            );

            subtest_TerminalNulls();
            subtest_FilteredZeroMiss();
            subtest_FilteredExhaustion();
            subtest_RawOverrun();
            subtest_IsAffinityPositionalPolicyViolation();

            #region S U B T E S T S
            void subtest_TerminalNulls()
            {
                var xfirstItem = ocm.Model.OffsettorAt(
                    StdModelElement.item,
                    0,
                    OffsetZeroPolicy.FirstFilterMatch);
                var xlast = ocm.Model.Descendors().Last();

                Assert.IsNull(
                    ocm.Model.PreviousAscendor(),
                    "Expecting root to have no previous ascendor.");

                Assert.IsNull(
                    xfirstItem?.PreviousAscendor(StdModelElement.item),
                    "Expecting first filtered item to have no previous item.");

                Assert.IsNull(
                    xlast.NextDescendor(),
                    "Expecting last modeled node to have no next descendor.");
            }

            void subtest_FilteredZeroMiss()
            {
                Assert.HasCount(0, builderThrow);
                Assert.IsNull(
                    ocm.Model.OffsettorAt(
                        StdModelElement.item,
                        0,
                        OffsetZeroPolicy.Absolute),
                    "Expecting explicit filtered zero from non-matching root to return null.");

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowSoft: Explicit filter 'item' requires zero to resolve within the filtered domain.";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_FilteredExhaustion()
            {
                Assert.IsNull(
                    ocm.Model.OffsettorAt(
                        StdModelElement.item,
                        ocm.Count,
                        OffsetZeroPolicy.FirstFilterMatch),
                    "Expecting filtered forward exhaustion to return null.");

                Assert.IsNull(
                    ocm.Model.OffsettorAt(
                        StdModelElement.item,
                        -(ocm.Count + 1),
                        OffsetZeroPolicy.ForceAscendingFilterMatch),
                    "Expecting filtered backward exhaustion to return null.");
            }

            void subtest_RawOverrun()
            {
                Assert.HasCount(0, builderThrow);
                var xlast = ocm.Model.Descendors().Last();
                    _ = xlast.OffsettorAt(
                        name: null,
                        plusOrMinus: +1,
                        OffsetZeroPolicy.Absolute);

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowSoft: Modeled offset exceeds the available forward range.";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_IsAffinityPositionalPolicyViolation()
            {
                Assert.HasCount(0, builderThrow);
                XElement
                    xitem = ocm.Model.Descendors(StdModelElement.item).First();
                XElement?
                    xrtn;
                IEnumerable<XElement>
                    nrtn;

                // Violate policy on a single offsettor call
                xrtn = ocm.Model.OffsettorAt(
                        name: nameof(LeadingAffinity.Linear),
                        plusOrMinus: 0,
                        offsetZeroPolicy: OffsetZeroPolicy.Absolute);

                Assert.IsNull(
                    xrtn,
                    "Expecting explicit string filter to remain a normal filtered zero miss.");
                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowSoft: Explicit filter 'Linear' requires zero to resolve within the filtered domain."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );

                // Violate policy on a single offsettor call
                xrtn = ocm.Model.OffsettorAt(
                    stdName: LeadingAffinity.Linear,
                    plusOrMinus: 0,
                    offsetZeroPolicy: OffsetZeroPolicy.Absolute);

                Assert.IsNull(
                    xrtn,
                    "Expecting enum affinity misuse in the filter slot to return null when handled.");

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowHard: Detected LeadingAffinity in filter position; This qualifier must be explicitly named or positionally last."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );

                // Violate policy on an enumerator call
                nrtn = ocm.Model.Ascendors(stdName: LeadingAffinity.Linear);

                Assert.HasCount(0,
                    nrtn,
                    "Expecting enum affinity misuse to produce no ascending results.");


                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowHard: Detected LeadingAffinity in filter position; This qualifier must be explicitly named or positionally last."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );

                // Violate policy on an enumerator call
                nrtn = ocm.Model.Descendors(stdName: LeadingAffinity.Linear);

                Assert.HasCount(0,
                    nrtn,
                    "Expecting enum affinity misuse to produce no descending results.");

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowHard: Detected LeadingAffinity in filter position; This qualifier must be explicitly named or positionally last."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );

                // Violate policy on an call to Prev
                xrtn = xitem.PreviousAscendor(stdEnum: LeadingAffinity.Linear);

                Assert.IsNull(
                    xrtn,
                    "Expecting enum affinity misuse to produce no previous ascendor.");

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowHard: Detected LeadingAffinity in filter position; This qualifier must be explicitly named or positionally last."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );

                // Violate policy on an call to Next
                xrtn = xitem.NextDescendor(stdEnum: LeadingAffinity.Linear);

                Assert.IsNull(
                    xrtn,
                    "Expecting enum affinity misuse to produce no next descendor.");

                actual = string.Join(Environment.NewLine, builderThrow); builderThrow.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
ThrowHard: Detected LeadingAffinity in filter position; This qualifier must be explicitly named or positionally last."
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting SINGLE EXCEPTION in build queue."
                );
            }
            #endregion S U B T E S T S
        }

        [TestMethod, DoNotParallelize]
        public void Test_AffinityDescendor()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            List<string> builder = new();
            var ocm = new OCMLocal(count: 7, seed: 2, maxDepth: 0);

            #region G E N    D A T A
            foreach (var xel in ocm.Model.Descendants().Skip(1))
            {
                xel.MoveRight();
            }
            var xroot = 
                ocm
                .Model
                .OffsettorAt(StdModelElement.item, 0, OffsetZeroPolicy.FirstFilterMatch)!;

            actual = xroot.ToShallow().ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting root is correctly identified."
            );

            foreach (var xel in xroot.Descendors(StdModelElement.item).Take(3))
            {
                xel.SetStdAttributeValue(StdOffsettorAttribute.above, bool.TrueString);
            }
            actual = ocm.Model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"">
    <item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""1"" above=""True"" />
    <item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""2"" above=""True"" />
    <item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""3"" above=""True"" />
    <item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""4"" />
    <item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""5"" />
    <item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""6"" />
  </item>
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting MRE test data for affinity enumerator."
            );
            #endregion G E N    D A T A

            subtest_MockAffinityLinearLookAhead();
            subtest_MockAffinityReverseLookAhead();

            #region S U B T E S T S
            // Strategy explainer:
            // If the current node has a first child marked above=True, then
            // the affinity descendor should not descend into that leading band
            // in normal forward order. Instead, for Linear, the descendor
            // should look ahead until the above=True run is over, and then
            // continue with the first ordinary trailing child.
            [Scaffolding, DoNotParallelize]
            void subtest_MockAffinityLinearLookAhead()
            {
                // Lens: Normal Descendant iteration
                builder =
                    [..
                    ocm.Model.Descendants()
                        .Select(xel =>
                            xel.To<PlaceableModel>()
                               .Description
                               .PadRightAndTruncate())
                    ];

                actual = string.Join(Environment.NewLine, builder); builder.Clear();
                actual.ToClipboardExpected();
                { }
                expected = @" 
Item01    
Item02    
Item03    
Item04    
Item05    
Item06    
Item07    ";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting numbered 'Item' descriptions."
                );

                var tod = DateTimeOffset.Now.WithTestability().TimeOfDay;
                xroot.SetBoundAttributeValue(tod, "tod", $"[{tod}]");

                foreach (var xel in xroot.Descendors(StdModelElement.item, includeSelf: true))
                {
                    // Look ahead to first child
                    if (xel.Elements().FirstOrDefault() is { } cxel && _nextSibMarkedAbove(cxel) is { } cxelNext)
                    {
                        next:
                        XElement? cxelPrevAscending = null;
                        cxel.SetStdAttributeValue(StdOffsettorAttribute.direction, LeadingAffinity.Ascending);
                        cxel.SetBoundAttributeValue(xel, StdOffsettorAttribute.pxel);
                        if (cxelPrevAscending is not null)
                        {
                            cxel.SetBoundAttributeValue(cxelPrevAscending, StdOffsettorAttribute.xascprev);
                        }
                        while(_nextSibMarkedAbove(cxel) is { } cxel_)
                        {
                            cxel = cxel_;
                        }

#if false

                        // Affinity sample calculation on yield cxel
                        if (cxel.Attribute(StdOffsettorAttribute.xascprev) is { } ascPrev)
                        {

                        }
                        else
                        {
                            if ((cxel.Attribute(StdOffsettorAttribute.pxel) as XBoundAttribute)?.Tag is XElement pxel
                                && pxel.To<TimeSpan>() is { } todRoot)
                            {
                                cxel.SetBoundAttributeValue(tod, "tod", $"[{(todRoot - TimeSpan.FromMinutes(5))}]");
                            }
                            builder.Add($"Yield: {cxel.ToShallow().ToString()}");
                        }


                        { }
                        //foreach (var xasc in xel.Elements())
                        //{
                        //    if (xasc.Attribute(StdOffsettorAttribute.above)?.Value.Equals(
                        //        bool.TrueString,
                        //        StringComparison.Ordinal) == true)
                        //    {
                        //        xasc.SetBoundAttributeValue(xel, StdOffsettorAttribute.pxel);
                        //        xasc.SetStdAttributeValue(
                        //            StdOffsettorAttribute.direction,
                        //            LeadingAffinity.Ascending);
                        //        builder.Add(xasc.ToShallow().ToString());
                        //    }
                        //    else
                        //    {
                        //        xasc.SetBoundAttributeValue(xel, StdOffsettorAttribute.pxel);
                        //        xasc.SetBoundAttributeValue(
                        //            LeadingAffinity.Linear,
                        //            StdOffsettorAttribute.direction);
                        //        builder.Add(xasc.ToShallow().ToString());
                        //        break;
                        //    }
                        //}

#endif
                        if (_nextSibMarkedAbove(cxel) is { } z)
                        { }
                    }
                    builder.Add(xel.ToShallow().ToString());
                }

                actual = string.Join(Environment.NewLine, builder); builder.Clear();
                actual.ToClipboardExpected();
                { }
                // CODEX: This is the human clipboard.
                expected = @" 
<item text=""312d1c21-0000-0000-0000-000000000000"" model=""[PlaceableModel]"" index=""0"" />
<item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""1"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""2"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""3"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""4"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Linear]"" />
<item text=""312d1c21-0000-0000-0000-000000000001"" model=""[PlaceableModel]"" index=""1"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000002"" model=""[PlaceableModel]"" index=""2"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000003"" model=""[PlaceableModel]"" index=""3"" above=""True"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Ascending]"" />
<item text=""312d1c21-0000-0000-0000-000000000004"" model=""[PlaceableModel]"" index=""4"" pxel=""[XElement]"" direction=""[ChildAboveAffinity.Linear]"" />
<item text=""312d1c21-0000-0000-0000-000000000005"" model=""[PlaceableModel]"" index=""5"" />
<item text=""312d1c21-0000-0000-0000-000000000006"" model=""[PlaceableModel]"" index=""6"" />"
                ;

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting linear look-ahead to skip the leading 'above' band."
                );
            }

            // Strategy explainer:
            // The same look-ahead begins at the same place, but Reverse treats
            // the leading above=True run as meaningful output. So the first hit
            // is the last node in that leading run, then the rest of the run
            // walks backward, and only after that does traversal rejoin the
            // ordinary trailing children.
            [Scaffolding]
            void subtest_MockAffinityReverseLookAhead()
            {
                builder.Clear();
                var leadingAbove = new List<XElement>();

                foreach (var xel in xroot.Descendors(StdModelElement.item))
                {
                    if (xel.Attribute(StdOffsettorAttribute.above)?.Value.Equals(
                        bool.TrueString,
                        StringComparison.Ordinal) == true)
                    {
                        leadingAbove.Add(xel);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int i = leadingAbove.Count - 1; i >= 0; i--)
                {
                    builder.Add(leadingAbove[i].Formatted());
                }

                foreach (var xel in xroot.Descendors(StdModelElement.item))
                {
                    if (xel.Attribute(StdOffsettorAttribute.above)?.Value.Equals(
                        bool.TrueString,
                        StringComparison.Ordinal) == true)
                    {
                        continue;
                    }
                    builder.Add(xel.Formatted());
                }
                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
3  312d1c21-0000-0000-0000-000000000003 Item04    
2  312d1c21-0000-0000-0000-000000000002 Item03    
1  312d1c21-0000-0000-0000-000000000001 Item02    
4  312d1c21-0000-0000-0000-000000000004 Item05    
5  312d1c21-0000-0000-0000-000000000005 Item06    
6  312d1c21-0000-0000-0000-000000000006 Item07    ";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting reverse look-ahead to emit the leading 'above' band in reverse order."
                );
            }
            #endregion S U B T E S T S
        }
    }

    static class TestClass_260424_OffsettorsExtensions
    {
        public static string MoveRight(this XElement @this)
        {
            if (@this.Parent is not XElement parent)
            {
                return @this.GetPath();
            }

            if (@this.ElementsBeforeSelf().LastOrDefault() is not XElement previousSibling)
            {
                return @this.GetPath();
            }

            @this.Remove();
            previousSibling.Add(@this);
            return @this.GetPath();
        }

        public static XElement WithRandomisedDepth(this XElement root, Random rando)
        {
            var nodes = root.Elements().ToArray();
            if (nodes.Length == 0)
            {
                return root;
            }
            root.RemoveNodes();
            root.Add(nodes);

            var siblingsInCurrentGroup = 1;
            var targetGroupSize = rando.Next(2, 4);
            var indented = false;

            for (int i = 1; i < nodes.Length; i++)
            {
                if (siblingsInCurrentGroup >= targetGroupSize)
                {
                    var maxMoves = indented ? rando.Next(0, 4) : rando.Next(1, 4);
                    for (int move = 0; move < maxMoves; move++)
                    {
                        var pathBefore = nodes[i].GetPath();
                        var pathAfter = nodes[i].MoveRight();
                        if (string.Equals(pathBefore, pathAfter, StringComparison.Ordinal))
                        {
                            break;
                        }
                        indented = true;
                    }
                    siblingsInCurrentGroup = 0;
                    targetGroupSize = rando.Next(2, 4);
                }
                siblingsInCurrentGroup++;
            }

            if (!indented && nodes.Length > 1)
            {
                nodes[1].MoveRight();
            }
            return root;
        }
        public static string Formatted(this XElement @this)
        {
            var builder = new List<string>();
            if (@this.Attribute(StdModelAttribute.text) is { } attrText)
            {
                builder.Add((@this.Attribute(StdModelAttribute.index)?.Value ?? "-").PadRight(2));
                builder.Add(attrText.Value);
                if (@this.To<PlaceableModel>() is { } model)
                {
                    builder.Add(model.Description.PadRightAndTruncate());
                }
                return string.Join(" ", builder);
            }
            else
            {
                return @this.Name.LocalName;
            }
        }
        public static string PadRightAndTruncate(this string? @this, int length=10)
            => (@this ??= string.Empty).PadRight(length).Substring(0, length);

    }
}
