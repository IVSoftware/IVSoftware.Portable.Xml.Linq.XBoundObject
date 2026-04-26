using IVSoftware.Portable.Collections;
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
        [TestMethod, DoNotParallelize]
        public void Test_Ascendors()
        {
            Random rando = new(10);
            using var te = this.TestableEpoch();

            ObservableCollection<PlaceableModel> oc = new();
            XElement model = StdModelElement.model.MakeXElement();

            int index = 0;
            foreach (var item in oc.PopulateForDemo(25))
            {
                model.Place(item.Id, out var xel);
                xel.Name =
                    rando.Next(5) == 0
                    ? nameof(StdModelElement.proxy)
                    : nameof(StdModelElement.item);
                xel.SetStdAttributeValue(StdModelAttribute.index, index++);
            }
            model.WithRandomisedDepth();

            var proxy20 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "20");

            CollectionAssert.AreEqual(
                new[] { "19", "18", "17", "16", "15", "14", "13", "12", "11", "10", "9", "8", "7", "6", "5", "4", "3", "2", "1", "0", null },
                proxy20
                .Ascendors()
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting unfiltered ascendors walk modeled linear order backward.");

            CollectionAssert.AreEqual(
                new[] { "20", "19", "18", "17", "16", "15", "14", "13", "12", "11", "10", "9", "8", "7", "6", "5", "4", "3", "2", "1", "0", null },
                proxy20
                .Ascendors(includeSelf: true)
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting includeSelf prepends the current node.");

            CollectionAssert.AreEqual(
                new[] { "18", "15", "14", "13", "12", "11", "10", "9", "8", "7", "6", "5", "4", "3", "2", "1", "0" },
                proxy20
                .Ascendors(nameof(StdModelElement.item))
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting item filter skips proxy nodes while preserving reverse modeled order.");

            CollectionAssert.AreEqual(
                new[] { "19", "17", "16" },
                proxy20
                .Ascendors(nameof(StdModelElement.proxy))
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting proxy filter returns only prior proxies in reverse modeled order.");

            var first = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "0");
            CollectionAssert.AreEqual(
                new string?[] { null },
                first
                .Ascendors()
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting the first item ascends to the model root when unfiltered.");
            CollectionAssert.AreEqual(
                new string?[] { "0", null },
                first
                .Ascendors(includeSelf: true)
                .Select(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value)
                .ToArray(),
                "Expecting includeSelf on the first item yields self then the model root.");
        }

        [TestMethod, DoNotParallelize]
        public void Test_PrevOffsettor()
        {
            Random rando = new(10);
            string actual, expected;
            using var te = this.TestableEpoch();

            ObservableCollection<PlaceableModel> oc = new();
            XElement model = StdModelElement.model.MakeXElement();

            int index = 0;
            foreach (var item in oc.PopulateForDemo(25))
            {
                model.Place(item.Id, out var xel);
                xel.Name =
                    rando.Next(5) == 0
                    ? nameof(StdModelElement.proxy)
                    : nameof(StdModelElement.item);
                xel.SetStdAttributeValue(StdModelAttribute.index, index++);
            }
            model.WithRandomisedDepth();

            actual = model.ToString();
            actual.ToClipboardExpected();
            { }
            expected = @" 
<model>
  <item text=""312d1c21-0000-0000-0000-000000000000"" index=""0"" />
  <item text=""312d1c21-0000-0000-0000-000000000001"" index=""1"" />
  <item text=""312d1c21-0000-0000-0000-000000000002"" index=""2"">
    <item text=""312d1c21-0000-0000-0000-000000000003"" index=""3"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000004"" index=""4"" />
  <item text=""312d1c21-0000-0000-0000-000000000005"" index=""5"">
    <item text=""312d1c21-0000-0000-0000-000000000006"" index=""6"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000007"" index=""7"" />
  <item text=""312d1c21-0000-0000-0000-000000000008"" index=""8"" />
  <item text=""312d1c21-0000-0000-0000-000000000009"" index=""9"" />
  <item text=""312d1c21-0000-0000-0000-00000000000a"" index=""10"" />
  <item text=""312d1c21-0000-0000-0000-00000000000b"" index=""11"" />
  <item text=""312d1c21-0000-0000-0000-00000000000c"" index=""12"">
    <item text=""312d1c21-0000-0000-0000-00000000000d"" index=""13"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-00000000000e"" index=""14"" />
  <item text=""312d1c21-0000-0000-0000-00000000000f"" index=""15"">
    <proxy text=""312d1c21-0000-0000-0000-000000000010"" index=""16"" />
  </item>
  <proxy text=""312d1c21-0000-0000-0000-000000000011"" index=""17"">
    <item text=""312d1c21-0000-0000-0000-000000000012"" index=""18"" />
  </proxy>
  <proxy text=""312d1c21-0000-0000-0000-000000000013"" index=""19"">
    <proxy text=""312d1c21-0000-0000-0000-000000000014"" index=""20"" />
  </proxy>
  <item text=""312d1c21-0000-0000-0000-000000000015"" index=""21"" />
  <item text=""312d1c21-0000-0000-0000-000000000016"" index=""22"">
    <item text=""312d1c21-0000-0000-0000-000000000017"" index=""23"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000018"" index=""24"" />
</model>"
            ;

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting basic model schema."
            );

            subtest_Edge1();
            subtest_Edge2();
            subtest_Edge3();
            subtest_Edge4();
            subtest_Edge5();
            subtest_Edge6();
            subtest_Edge7();
            subtest_Edge8();
            subtest_Edge9();
            subtest_Edge10();

            #region S U B T E S T S
            void subtest_Edge1()
            {
                var first =
                    model
                    .DescendantsAndSelf()
                    .First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "0");

                Assert.IsNull(
                    first.PreviousOffsettor(nameof(StdModelElement.item)),
                    "Expecting first item has no previous item offsettor.");

                var proxy20 =
                    model
                    .DescendantsAndSelf()
                    .First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "20");

                var prevItem = proxy20.PreviousOffsettor(nameof(StdModelElement.item));
                Assert.IsNotNull(prevItem, "Expecting filtered previous item exists.");
                Assert.AreEqual(
                    "18",
                    prevItem.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting proxy chain resolves backward to item index 18.");
            }

            void subtest_Edge2()
            {
                var child3 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "3");
                var prevItem = child3.PreviousOffsettor(nameof(StdModelElement.item));

                Assert.IsNotNull(prevItem, "Expecting child item has a previous item.");
                Assert.AreEqual(
                    "2",
                    prevItem.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous item is the parent when there is no previous sibling.");
            }

            void subtest_Edge3()
            {
                var child6 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "6");
                var prevAny = child6.PreviousOffsettor();

                Assert.IsNotNull(prevAny, "Expecting child has a previous offsettor.");
                Assert.AreEqual(
                    "5",
                    prevAny.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous offsettor is the parent when there is no previous sibling.");
            }

            void subtest_Edge4()
            {
                var item4 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "4");
                var prevItem = item4.PreviousOffsettor(nameof(StdModelElement.item));

                Assert.IsNotNull(prevItem, "Expecting previous item exists.");
                Assert.AreEqual(
                    "3",
                    prevItem.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous sibling's leaf is selected.");
            }

            void subtest_Edge5()
            {
                var proxy17 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "17");
                var prevAny = proxy17.PreviousOffsettor();

                Assert.IsNotNull(prevAny, "Expecting previous offsettor exists.");
                Assert.AreEqual(
                    "16",
                    prevAny.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous sibling leaf is returned regardless of name.");
            }

            void subtest_Edge6()
            {
                var proxy16 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "16");
                var prevItem = proxy16.PreviousOffsettor(nameof(StdModelElement.item));

                Assert.IsNotNull(prevItem, "Expecting filtered previous item exists.");
                Assert.AreEqual(
                    "15",
                    prevItem.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting first child resolves to parent when filtering to items.");
            }

            void subtest_Edge7()
            {
                var proxy20 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "20");
                var prevProxy = proxy20.PreviousOffsettor(nameof(StdModelElement.proxy));

                Assert.IsNotNull(prevProxy, "Expecting filtered previous proxy exists.");
                Assert.AreEqual(
                    "19",
                    prevProxy.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting first child proxy resolves to proxy parent.");
            }

            void subtest_Edge8()
            {
                var item21 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "21");
                var prevProxy = item21.PreviousOffsettor(nameof(StdModelElement.proxy));

                Assert.IsNotNull(prevProxy, "Expecting filtered previous proxy exists.");
                Assert.AreEqual(
                    "20",
                    prevProxy.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous sibling branch leaf is returned when filtering to proxies.");
            }

            void subtest_Edge9()
            {
                var item18 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "18");
                var prevItem = item18.PreviousOffsettor(nameof(StdModelElement.item));

                Assert.IsNotNull(prevItem, "Expecting previous item exists across proxy-parent boundary.");
                Assert.AreEqual(
                    "15",
                    prevItem.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting first child climbs to proxy parent, skips non-item leaf, and lands on item 15.");
            }

            void subtest_Edge10()
            {
                var proxy17 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "17");
                var prevProxy = proxy17.PreviousOffsettor(nameof(StdModelElement.proxy));

                Assert.IsNotNull(prevProxy, "Expecting previous proxy exists.");
                Assert.AreEqual(
                    "16",
                    prevProxy.Attribute(nameof(StdModelAttribute.index))?.Value,
                    "Expecting previous sibling leaf proxy is returned when filtering to proxies.");

                var item1 = model.DescendantsAndSelf().First(_ => _.Attribute(nameof(StdModelAttribute.index))?.Value == "1");
                Assert.IsNull(
                    item1.PreviousOffsettor(nameof(StdModelElement.proxy)),
                    "Expecting no previous proxy before the early flat items.");
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

        public static XElement WithRandomisedDepth(this XElement root, int seed=123)
        {
            var nodes = root.Elements().ToArray();
            if (nodes.Length == 0)
            {
                return root;
            }

            var rng = new Random(seed);
            root.RemoveNodes();
            root.Add(nodes);

            var siblingsInCurrentGroup = 1;
            var targetGroupSize = rng.Next(2, 4);
            var indented = false;

            for (int i = 1; i < nodes.Length; i++)
            {
                if (siblingsInCurrentGroup >= targetGroupSize)
                {
                    var maxMoves = indented ? rng.Next(0, 4) : rng.Next(1, 4);
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
                    targetGroupSize = rng.Next(2, 4);
                }
                siblingsInCurrentGroup++;
            }

            if (!indented && nodes.Length > 1)
            {
                nodes[1].MoveRight();
            }

            return root;
        }
    }
}
