using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.WinOS.MSTest.Extensions;
using System.Xml.Linq;
using static IVSoftware.Portable.Xml.Linq.XBoundObject.Extensions;

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

            Assert.IsNotNull(dict[Scan.Barcode]);
            Assert.IsNotNull(dict[Scan.Barcode].To<Button>());

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
        /// <summary>
        /// Tests the behavior of retrieving and converting an attribute value from an XElement with specific focus
        /// on exception handling when the required attribute or enum type is not present.
        /// The test is structured into multiple sub-tests:
        /// 1. subtestWhereAttributeExists - Ensures that existing attributes are retrieved successfully.
        /// 2. subtestDoNotThrowWhereEnumAttributeDoesNotExist - Tests the scenario where an attribute does not exist,
        ///    and the method should handle it gracefully without throwing an exception.
        /// 3. subtestThrowWhereEnumAttributeDoesNotExist - Tests the behavior when an attribute does not exist
        ///    but the method is expected to throw an InvalidOperationException due to the explicit instruction
        ///    to throw an exception in cases of missing attributes.
        /// This method encompasses three internal sub-tests that specifically test the behavior of the
        /// TryGetSingleBoundAttributeByType and To<T> methods under different conditions:
        /// - Ensuring the system throws InvalidOperationException when attempting to forcefully retrieve or convert
        ///   attributes that are marked as non-existent or when the conversion is not valid.
        /// </summary>
        [TestMethod]
        public void Test_TryGetAttributeValue()
        {
            var xel = new XElement("tmp");
            xel.SetAttributeValue(NodeType.folder);

            subtestWhereAttributeExists();
            subtestDoNotThrowWhereEnumAttributeDoesNotExist();
            subtestThrowWhereEnumAttributeDoesNotExist();
            subtestMultiples();

            #region S U B T E S T S
            void subtestWhereAttributeExists()
            {
                Assert.IsTrue(
                    xel.TryGetAttributeValue(out NodeType result1),
                    $"Expecting nested TryGetAttributeValueByType call returns false but parsed enum succeds.");

                Assert.AreEqual(
                    NodeType.folder, 
                    result1, 
                    "Expecting FIXED version 1.4.0-prerelease bug.");

                Assert.IsTrue(
                    xel.To<NodeType>() is NodeType result2,
                    $"Expecting parsed enum fallback success based on {nameof(EnumParsingOption.AllowEnumParsing)}");

                try
                {
                    _ = xel.To<NodeType>(enumParsing: EnumParsingOption.BoundEnumTypeOnly);
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    Assert.AreEqual(
                        EnumErrorReportOption.Assert,
                        Compatibility.DefaultErrorReportOption);

                    Assert.AreEqual(
                        "DebugAssertException",
                        ex.GetType().Name);
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                }

                Assert.AreEqual(
                    NodeType.folder, 
                    result2, 
                    "Expecting new overload with allowEnumParsing to work.");

                Assert.IsTrue(xel.To<NodeType?>() is NodeType, "Expecting nullable enum type to return valid T in this case.");
            }

            void subtestDoNotThrowWhereEnumAttributeDoesNotExist()
            {
                Assert.IsTrue(xel.To<NotFoundTypeForTest?>() is null, "Expecting nullable enum type to return null without throwing exception.");
                Assert.IsFalse(xel.TryGetAttributeValue(out NotFoundTypeForTest doNotUse), "Expecting false without throwing exception");
            }

            void subtestThrowWhereEnumAttributeDoesNotExist()
            {
                try
                {
                    _ = xel.TryGetSingleBoundAttributeByType(out NotFoundTypeForTest na, @throw: true);
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (InvalidOperationException ex)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                    Assert.AreEqual(ex.Message, localInvalidOperationExceptionMessage<NotFoundTypeForTest>());
                }

                // Even though @throw is EXPLICITLY set to false, we need to override it. 
                // IT'S AN EMERGENCY!
                // The returned value is simply not valid in this case.
                try
                {
                    _ = xel.To<NotFoundTypeForTest>(@throw: false);
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                    Assert.AreEqual(
                        EnumErrorReportOption.Assert,
                        Compatibility.DefaultErrorReportOption);

                    Assert.AreEqual(
                        ex.GetType().Name,
                        "DebugAssertException");
                }

                try
                {
                    _ = xel.To<NotFoundTypeForTest>();
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                    Assert.AreEqual(
                        EnumErrorReportOption.Assert,
                        Compatibility.DefaultErrorReportOption);

                    Assert.AreEqual(
                        ex.GetType().Name,
                        "DebugAssertException");
                }
                try
                {
                    _ = xel.To<NotFoundTypeForTest>(@throw: true);
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (InvalidOperationException ex)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                    Assert.AreEqual(ex.Message, localInvalidOperationExceptionMessage<NotFoundTypeForTest>());
                }
            }

            void subtestMultiples()
            {
                xel.RemoveAttributes();
                xel.SetBoundAttributeValue(SortOrder.Ascending);
                xel.SetBoundAttributeValue(NodeType.folder);

                try
                {
                    _ = xel.To<Enum>();
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (InvalidOperationException ex)
                {
                    Assert.AreEqual(ex.Message, localInvalidOperationMultipleFoundMessage<Enum>());
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                }
            }

            static string localInvalidOperationExceptionMessage<T>() => $"No valid {typeof(T).Name} found. To handle cases where an enum attribute might not exist, use a nullable version: To<{typeof(T).Name}?>() or check @this.Has<{typeof(T).Name}>() first.";

            static string localInvalidOperationMultipleFoundMessage<T>() => $@"Multiple valid {typeof(T).Name} found. To disambiguate them, obtain the attribute by name: Attributes().OfType<XBoundAttribute>().Single(_=>_.name=""targetName""";
        #endregion S U B T E S T S
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
