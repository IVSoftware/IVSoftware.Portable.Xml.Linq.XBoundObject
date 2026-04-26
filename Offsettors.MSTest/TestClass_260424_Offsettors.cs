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
        [TestMethod]
        public void Test_PrevOffsettor()
        {
            string actual, expected;
            using var te = this.TestableEpoch();

            ObservableCollection<PlaceableModel> oc = new();
            XElement model = StdModelElement.model.MakeXElement();

            int index = 0;
            foreach(var item in oc.PopulateForDemo(25))
            {
                model.Place(item.Id, out var xel);
                xel.Name = nameof(StdModelElement.item);
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
  <marklar text=""312d1c21-0000-0000-0000-000000000004"" index=""4"" />
  <marklar text=""312d1c21-0000-0000-0000-000000000005"" index=""5"">
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
    <item text=""312d1c21-0000-0000-0000-000000000010"" index=""16"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000011"" index=""17"">
    <item text=""312d1c21-0000-0000-0000-000000000012"" index=""18"" />
  </item>
  <item text=""312d1c21-0000-0000-0000-000000000013"" index=""19"">
    <item text=""312d1c21-0000-0000-0000-000000000014"" index=""20"" />
  </item>
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
