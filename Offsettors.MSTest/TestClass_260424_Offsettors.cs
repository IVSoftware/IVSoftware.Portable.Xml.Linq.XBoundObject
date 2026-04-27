using IVSoftware.Portable.Collections;
using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.MSTest.Preview;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml.Linq;

namespace Offsettors.MSTest
{
    [TestClass]
    public sealed class TestClass_260424_Offsettors
    {
        private class OCMLocal
            : ObservableCollection<PlaceableModel>
            , IDisposable
        {
            public OCMLocal(int count, int seed, int maxDepth)
            {
                _te = this.TestableEpoch();
                Rando = new(seed);

                string[] guids =
                    Enumerable.Range(0, count)
                    .Select(_ => new Guid().WithTestability().ToString())
                    .ToArray();
                for (int i = 0; i < count; i++)
                {
                    Debug.WriteLine($"Index={i:D2}");
#if DEBUG
                    if (i == 11)
                    { }
#endif
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
        public void Test_Ascendors()
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
            { }
        }

#if false

            ObservableCollection<PlaceableModel> oc = new();
            XElement model = StdModelElement.model.MakeXElement();
            int index = 0;
            foreach (var item in oc.PopulateForDemo(25))
            {
                model.Place(item.Id, out var xel);
                xel.Name =
                    ocm.Rando.Next(5) == 0
                    ? nameof(StdModelElement.proxy)
                    : nameof(StdModelElement.item);
                xel.SetStdAttributeValue(StdModelAttribute.index, index++);
            }
            model.WithRandomisedDepth(ocm.Rando);
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
#endif
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
    }
}
