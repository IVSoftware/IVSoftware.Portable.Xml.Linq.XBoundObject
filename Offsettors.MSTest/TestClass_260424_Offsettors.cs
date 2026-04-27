using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.MSTest.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace Offsettors.MSTest
{
    [TestClass]
    public sealed class TestClass_260424_Offsettors
    {
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
        private class OCMLocal
            : ObservableCollection<PlaceableModel>
            , IDisposable
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
                _te = this.TestableEpoch();
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
                    if(xel.To<PlaceableModel>() is { } item)
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
            public void Dispose() => _te.Dispose();
            private IDisposable _te;
        }

        [TestMethod, DoNotParallelize]
        public void Test_OCMLocal_CTor()
        {
            string actual, expected;
            using var ocm = new OCMLocal(25, 10, 2);

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

        [TestMethod, DoNotParallelize]
        public void Test_AscendFromDescendor()
        {
            string actual, expected;
            string[] builder;

            using var ocm = new OCMLocal(count: 10, seed: 1);

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
                "Expecting test set with mix of item + proxy at various depth."
            );

            subtest_AscendFromLast();
            subtest_AscendFromModel();
            subtest_AscendFromSibling();

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
                    .Select(_ =>_.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
9 312d1c21-0000-0000-0000-000000000009
- 312d1c21-0000-0000-0000-000000000002
- 312d1c21-0000-0000-0000-000000000006
8 312d1c21-0000-0000-0000-000000000008
7 312d1c21-0000-0000-0000-000000000005
6 312d1c21-0000-0000-0000-000000000004
- 312d1c21-0000-0000-0000-000000000009
5 312d1c21-0000-0000-0000-000000000003
- 312d1c21-0000-0000-0000-000000000004
4 312d1c21-0000-0000-0000-000000000002
3 312d1c21-0000-0000-0000-000000000007
2 312d1c21-0000-0000-0000-000000000001
1 312d1c21-0000-0000-0000-000000000006
0 312d1c21-0000-0000-0000-000000000000
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
                    .Select(_ =>_.Formatted())
                    .ToArray();

                actual = string.Join(Environment.NewLine, builder);
                actual.ToClipboardExpected();
                { }
                expected = @" 
- 312d1c21-0000-0000-0000-000000000002
- 312d1c21-0000-0000-0000-000000000006
8 312d1c21-0000-0000-0000-000000000008
7 312d1c21-0000-0000-0000-000000000005
6 312d1c21-0000-0000-0000-000000000004
- 312d1c21-0000-0000-0000-000000000009
5 312d1c21-0000-0000-0000-000000000003
- 312d1c21-0000-0000-0000-000000000004
4 312d1c21-0000-0000-0000-000000000002
3 312d1c21-0000-0000-0000-000000000007
2 312d1c21-0000-0000-0000-000000000001
1 312d1c21-0000-0000-0000-000000000006
0 312d1c21-0000-0000-0000-000000000000
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
                    .SkipLast(1)
                    .Select(_ => $"{_.Attribute(StdModelAttribute.index)!.Value} {_.Attribute(StdModelAttribute.text)!.Value}" )
                    .ToArray();
                { }

                actual = string.Join(Environment.NewLine, builder); 
                actual.ToClipboardExpected();
                { }
                expected = @" 
9 312d1c21-0000-0000-0000-000000000009
8 312d1c21-0000-0000-0000-000000000008
7 312d1c21-0000-0000-0000-000000000005
6 312d1c21-0000-0000-0000-000000000004
5 312d1c21-0000-0000-0000-000000000003
4 312d1c21-0000-0000-0000-000000000002
3 312d1c21-0000-0000-0000-000000000007
2 312d1c21-0000-0000-0000-000000000001
1 312d1c21-0000-0000-0000-000000000006";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
                builder =
                    xlast
                    .Ascendors(StdModelElement.item, includeSelf: false)
                    .SkipLast(1)
                    .Select(_ => $"{_.Attribute(StdModelAttribute.index)!.Value} {_.Attribute(StdModelAttribute.text)!.Value}" )
                    .ToArray();
                { }

                actual = string.Join(Environment.NewLine, builder); 
                actual.ToClipboardExpected();
                { }
                expected = @" 
8 312d1c21-0000-0000-0000-000000000008
7 312d1c21-0000-0000-0000-000000000005
6 312d1c21-0000-0000-0000-000000000004
5 312d1c21-0000-0000-0000-000000000003
4 312d1c21-0000-0000-0000-000000000002
3 312d1c21-0000-0000-0000-000000000007
2 312d1c21-0000-0000-0000-000000000001
1 312d1c21-0000-0000-0000-000000000006";

                Assert.AreEqual(
                    expected.NormalizeResult(),
                    actual.NormalizeResult(),
                    "Expecting builder content to match."
                );
            }

            void subtest_AscendFromSibling()
            {
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
            if (@this.Attribute(StdModelAttribute.text) is { } text)
            {
                return $"{(@this.Attribute(StdModelAttribute.index)?.Value ?? "-").PadRight(2)} {text.Value}";
            }
            else
            {
                return @this.Name.LocalName;
            }
        }
    }
}
