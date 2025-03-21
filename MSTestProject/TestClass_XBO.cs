using System;
using System.Collections;
using System.Xml.Linq;
using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;

namespace MSTestProject
{
    [TestClass]
    public sealed class TestClass_XBO
    {

        [TestMethod]
        public void Test_Descendants()
        {
            string actual, expected;

            var builder = new List<string>();
            foreach (var value in typeof(DiscoveryDemo).Descendants())
            {
                builder.Add($"{value.ToFullKey()}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting msg");
            { }
            expected = @" 
DiscoveryDemo.Home
DiscoveryDemo.Scan
Scan.QRCode
Scan.Barcode
DiscoveryDemo.Settings
Settings.Apply
Settings.Cancel";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting msg"
            );

            builder.Clear();

            foreach (var value in typeof(DiscoveryDemo).Descendants(DiscoveryScope.ConstrainToAssembly))
            {
                builder.Add($"{value.ToFullKey()}");
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting msg");
            { }
            expected = @" 
DiscoveryDemo.Home
DiscoveryDemo.Scan
Scan.QRCode
Scan.Barcode
DiscoveryDemo.Settings
Settings.Apply
Apply.All
Apply.Selected
Settings.Cancel";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting msg"
            );
        }

        [TestMethod]
        public void Test_NestedEnum()
        {
            string actual, expected;
            var builder = new List<string>();

            var xroot =
                typeof(DiscoveryDemo).BuildNestedEnum();

            actual = xroot.ToString();
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting Nested Enum Structure");
            { }
            expected = @" 
<root type=""[DiscoveryDemo]"" dualkeylookup=""[DualKeyLookup]"">
  <node id=""[DiscoveryDemo.Home]"" />
  <node id=""[DiscoveryDemo.Scan]"">
    <node id=""[Scan.QRCode]"" />
    <node id=""[Scan.Barcode]"" />
  </node>
  <node id=""[DiscoveryDemo.Settings]"">
    <node id=""[Settings.Apply]"" />
    <node id=""[Settings.Cancel]"" />
  </node>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Nested Enum Structure"
            );

            foreach (var node in xroot.Descendants().Where(_ => _.Has<Enum>()))
            {
                var button = new Button(node.To<Enum>());
                button.Click += localClickHandler;
                node.SetBoundAttributeValue(button);
            }

            actual = xroot.ToString();
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting Attached Clickable");
            { }
            expected = @" 
<root type=""[DiscoveryDemo]"" dualkeylookup=""[DualKeyLookup]"">
  <node id=""[DiscoveryDemo.Home]"" button=""[Button]"" />
  <node id=""[DiscoveryDemo.Scan]"" button=""[Button]"">
    <node id=""[Scan.QRCode]"" button=""[Button]"" />
    <node id=""[Scan.Barcode]"" button=""[Button]"" />
  </node>
  <node id=""[DiscoveryDemo.Settings]"" button=""[Button]"">
    <node id=""[Settings.Apply]"" button=""[Button]"" />
    <node id=""[Settings.Cancel]"" button=""[Button]"" />
  </node>
</root>";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting Attached Clickable"
            );

            // Obtain the dictionary 
            var dict = xroot.To<DualKeyLookup>();

            dict[Scan.Barcode]
                .To<Button>()
                .PerformClick();

            dict[Scan.QRCode]
                .To<Button>()
                .PerformClick();

            void localClickHandler(object? sender, EventArgs e)
            {
                if (sender is IClickable button)
                {
                    builder.Add($"Clicked: {button.Id}");
                }
            }

            actual = string.Join(Environment.NewLine, builder);
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting msg");
            { }

            expected = @" 
Clicked: Barcode
Clicked: QRCode";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting button clicks"
            );
        }

        [TestMethod]
        public void Test_AncestorOfType()
        {
            string actual, expected;

            var xroot =
                typeof(DiscoveryDemo).BuildNestedEnum();
            var dkl =
                xroot.To<DualKeyLookup>(@throw: true);
            var node =
                dkl[Settings.Apply];
            var dklFromAnc =
                node.AncestorOfType<DualKeyLookup>(@throw: true);
            var notExist =
                node.AncestorOfType<bool>(@throw: false);
            bool caught = false;
            try
            {
                notExist =
                    node.AncestorOfType<bool>(@throw: true);
            }
            catch (InvalidOperationException)
            {
                caught = true;
            }
            Assert.IsTrue(caught, "Expecting exception was thrown.");
        }

        [TestMethod]
        public void Test_ToFullIdPath()
        {
            string actual, expected;

            var xroot =
                typeof(DiscoveryDemo).BuildNestedEnum(DiscoveryScope.ConstrainToAssembly);
            var dkl = 
                xroot.To<DualKeyLookup>(@throw: true);

            var node = dkl[Deep.Apply.Selected];
            var path = node.ToFullIdPath();

            actual = path;
            actual.ToClipboard();
            actual.ToClipboardAssert("Expecting ID path from root to leaf.");
            { }
            expected = @" 
Settings.Apply.Selected";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting ID path from root to leaf."
            );

            actual = Deep.Apply.Selected.ToFullIdPath(dkl);
            expected = @" 
Settings.Apply.Selected";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting identical result."
            );
        }

        [TestMethod]
        public void Test_TryGetAttributeValue()
        {
            var xel = new XElement("tmp");
            xel.SetAttributeValue(NodeType.folder);
            if(xel.TryGetAttributeValue(out NodeType result))
            { }
        }
    }
    interface IClickable
    {
        Enum Id { get; }
        event EventHandler? Click;
        void PerformClick();
    }
    class Button : IClickable
    {
        public Button(Enum id) => Id = id;
        public Enum Id { get; }
        public event EventHandler? Click;

        public void PerformClick() => Click?.Invoke(this, EventArgs.Empty);
    }
}
