using IVSoftware.Portable.Xml.Linq;
using IVSoftware.Portable.Xml.Linq.XBoundObject;
using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
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

            subtestTryGetAttributeValueWhereAttributeIsXATTR();
            subtestToTWhereAttributeIsXATTR();

            subtestToTNullableWhereAttributeIsXATTR();
            void subtestToTNullableWhereAttributeIsXATTR() { }
            {
                // To T? with implicit strict rules.
                Assert.IsTrue(
                    xel.To<NodeType?>()
                    is null, 
                    "Expecting nullable not found (due to strict rules) returns correct value of 'null' and NOT assert."
                );
                // To T? with loose rules.
                Assert.IsTrue(
                    xel.To<NodeType?>(enumParsingOption: EnumParsingOption.FindUsingLowerCaseNameThenParseValue)
                    is NodeType,
                    "Expecting nullable enum type to return valid T in this case."
                );
            }
            subtestDoNotThrowWhereEnumAttributeDoesNotExist();
            subtestThrowWhereEnumAttributeDoesNotExist();
            subtestMultiples();

            #region S U B T E S T S
            void subtestTryGetAttributeValueWhereAttributeIsXATTR()
            {
                // Loose enum parsing rules.
                Assert.IsTrue(
                    xel.TryGetAttributeValue(out NodeType result1a, enumParsingOption: EnumParsingOption.FindUsingLowerCaseNameThenParseValue),
                    $"Expecting success using loose parsing rules.");

                Assert.AreEqual(
                    NodeType.folder,
                    result1a,
                    "Expecting FIXED version 1.4.0-prerelease bug.");

                Assert.IsFalse(
                    xel.TryGetAttributeValue(out NodeType result1b, enumParsingOption: EnumParsingOption.UseStrictRules),
                    $"Expecting non-detection. The value is XATTR not XBA and does NOT have a [Placement] attribute to guide.");
 
                // This is the 'incorrect' default value but that's ok
                // because it's flagged with the FALSE return value.
                Assert.AreEqual(
                    NodeType.drive,
                    result1b,
                    "Expecting FIXED version 1.4.0-prerelease bug.");

                // Default: Use loose rules EnumParsingOption.FindUsingLowerCaseNameThenParseValue
                Assert.IsTrue(
                    xel.TryGetAttributeValue(out NodeType result1c),
                    $"Expecting success. Since the value exists, parsing rules never come into play.");

                Assert.AreEqual(
                    NodeType.folder,
                    result1c,
                    "Expecting FIXED version 1.4.0-prerelease bug.");

                // To T with explicit loose rules.
                Assert.IsTrue(
                    xel.To<NodeType>(enumParsingOption: EnumParsingOption.FindUsingLowerCaseNameThenParseValue) is NodeType result2,
                    $"Expecting parsed enum fallback success based on {nameof(EnumParsingOption.UseStrictRules)}");

                Assert.AreEqual(
                    NodeType.folder,
                    result2,
                    "Expecting new overload with allowEnumParsing to work.");
            }
            
            void subtestToTWhereAttributeIsXATTR()
            {
                // To T with explicit loose rules.
                Assert.IsTrue(
                    xel.To<NodeType>(enumParsingOption: EnumParsingOption.FindUsingLowerCaseNameThenParseValue) is NodeType result2,
                    $"Expecting parsed enum fallback success based on {nameof(EnumParsingOption.UseStrictRules)}");

                Assert.AreEqual(
                    NodeType.folder,
                    result2,
                    "Expecting new overload with allowEnumParsing to work.");

                // To T with explicit strict rules.
                try
                {
                    _ = xel.To<NodeType>(enumParsingOption: EnumParsingOption.UseStrictRules);
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    switch (ex.GetType().Name)
                    {
                        // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                        case "AssertFailedException":   // Correct response in Release mode
                        case "DebugAssertException":    // Correct response in Debug mode (but this is an MSTest internal class)
                            break;
                        default:
                            Assert.Fail("Expecting a different exception here.");
                            break;
                    }
                }

                // To T with implicit strict rules.
                try
                {
                    _ = xel.To<NodeType>();
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    switch (ex.GetType().Name)
                    {
                        // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                        case "AssertFailedException":   // Correct response in Release mode
                        case "DebugAssertException":    // Correct response in Debug mode (but this is an MSTest internal class)
                            break;
                        default:
                            Assert.Fail("Expecting a different exception here.");
                            break;
                    }
                }
            }

            void subtestDoNotThrowWhereEnumAttributeDoesNotExist()
            {
                Assert.IsTrue(xel.To<NotFoundTypeForTest?>() is null, "Expecting nullable enum type to return null without throwing exception.");
                Assert.IsFalse(xel.TryGetAttributeValue(out NotFoundTypeForTest doNotUse), "Expecting false without throwing exception");
            }

            void subtestThrowWhereEnumAttributeDoesNotExist()
            {
                Assert.IsFalse(
                     _ = xel.TryGetSingleBoundAttributeByType(
                         out NotFoundTypeForTest na,
                         out TrySingleStatus status),
                     $"Expecting returns false and doesn't throw");

                Assert.AreEqual(
                    TrySingleStatus.FoundNone, 
                    status,
                    "Expecting correct status reporting.");

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
                    switch (ex.GetType().Name)
                    {
                        // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                        case "AssertFailedException":   // Correct response in Release mode
                        case "DebugAssertException":    // Correct response in Debug mode (but this is an MSTest internal class)
                            break;
                        default:
                            Assert.Fail("Expecting a different exception here.");
                            break;
                    }
                }

                try
                {
                    _ = xel.To<NotFoundTypeForTest>();
                    Assert.Fail($"Expecting {nameof(InvalidOperationException)}");
                }
                catch (Exception ex)
                {
                    // Pass! This exception SHOULD BE THROWN. It's what we're testing.

                    switch (ex.GetType().Name)
                    {
                        // Pass! This exception SHOULD BE THROWN. It's what we're testing.
                        case "AssertFailedException":   // Correct response in Release mode
                        case "DebugAssertException":    // Correct response in Debug mode (but this is an MSTest internal class)
                            break;
                        default:
                            Assert.Fail("Expecting a different exception here.");
                            break;
                    }
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

                Assert.IsNull(
                    xel.To<Enum>(),
                    $"Expecting the system works here and simply returns a null for a multiple."
                );
                try
                {
                    _ = xel.To<Enum>(@throw: true);
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

        [TestMethod]
        public void Test_TryGetSingleBoundAttributeByType()
        {
            string actual, expected;
            XElement xel = new XElement("xel");

            // structs with [Placement]
            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalXAttrEnum a));
            Assert.AreEqual(LocalXAttrEnum.Default, a, "Expecting default because the call does not use nullable");

            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalXBAEnum b));
            Assert.AreEqual(LocalXBAEnum.Default, b, "Expecting default because the call does not use nullable");

            // structs without [Placement]
            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out NodeType n));
            Assert.AreEqual(NodeType.drive, n, "Expecting default because the call does not use nullable");

            // Nullable
            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalXAttrEnum? c));
            Assert.IsNull(c, "Expecting null because the call uses nullable");

            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalXBAEnum? d));
            Assert.IsNull(d, "Expecting null because the call uses nullable");

            // structs without [Placement]
            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out NodeType? nn));
            Assert.IsNull(nn, "Expecting null because the call uses nullable");

            // Class and Nullable Class
            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalClass e));
            Assert.IsNull(e, "Expecting null because default(T) is null");

            Assert.IsFalse(xel.TryGetSingleBoundAttributeByType(out LocalClass? f));
            Assert.IsNull(f, "Expecting null because default(T) is null");

            xel.SetAttributeValue(LocalXAttrEnum.NonDefault);
            xel.SetAttributeValue(LocalXBAEnum.NonDefault);
            xel.SetBoundAttributeValue(new LocalClass());
            xel.SetAttributeValue(NodeType.file);

            actual = xel.SortAttributes<LocalSortAttributeOrder>().ToString();
            expected = @" 
<xel xattr=""NonDefault"" xba=""[LocalXBAEnum.NonDefault]"" localclass=""[LocalClass]"" nodetype=""file"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting XAttribute x 1 and XBoundAttribute x 2 with custom names for enums."
            );

            // struct
            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalXAttrEnum aa));
            Assert.AreEqual(LocalXAttrEnum.NonDefault, aa);

            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalXBAEnum bb));
            Assert.AreEqual(LocalXBAEnum.NonDefault, bb);

            // Nullable
            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalXAttrEnum? cc));
            Assert.AreEqual(LocalXAttrEnum.NonDefault, cc);

            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalXBAEnum? dd));
            Assert.AreEqual(LocalXBAEnum.NonDefault, dd);

            // Class and Nullable Class
            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalClass ee));
            Assert.IsTrue(ee is LocalClass, "Expecting non-null correctly typed instance." );

            Assert.IsTrue(xel.TryGetSingleBoundAttributeByType(out LocalClass? ff));
            Assert.IsTrue(ff is LocalClass, "Expecting non-null correctly typed instance." );

            // There's just one more thing...
            xel.RemoveAttributes();
            xel.SetAttributeValue(LocalFullKeyEnum.NonDefault);

            actual = xel.SortAttributes<LocalSortAttributeOrder>().ToString();
            expected = @" 
<xel localfullkeyenum=""LocalFullKeyEnum.NonDefault"" />";

            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting FullKeyAttribute is properly formatted."
            );
        }

        [TestMethod]
        public void Test_HasT()
        {
            string actual, expected;
            XElement xel = new XElement("xel");

            // structs with [Placement]
            Assert.IsFalse(xel.Has<LocalXAttrEnum>());
            Assert.IsFalse(xel.Has<LocalXBAEnum>());

            // structs without [Placement]
            Assert.IsFalse(xel.Has<NodeType>());

            // Nullable
            Assert.IsFalse(xel.Has<LocalXAttrEnum?>());
            Assert.IsFalse(xel.Has<LocalXBAEnum?>());

            // Class and Nullable Class
            Assert.IsFalse(xel.Has<LocalClass>());
            Assert.IsFalse(xel.Has<LocalClass?>());

            xel.SetAttributeValue(LocalXAttrEnum.NonDefault);
            xel.SetAttributeValue(LocalXBAEnum.NonDefault);
            xel.SetAttributeValue(NodeType.file);
            xel.SetBoundAttributeValue(new LocalClass());

            actual = xel.SortAttributes<LocalSortAttributeOrder>().ToString();
            expected = @" 
<xel xattr=""NonDefault"" xba=""[LocalXBAEnum.NonDefault]"" nodetype=""file"" localclass=""[LocalClass]"" />";


            Assert.AreEqual(
                expected.NormalizeResult(),
                actual.NormalizeResult(),
                "Expecting XAttribute x 2 and XBoundAttribute x 2 with custom names for enums."
            );

            // structs with [Placement]
            Assert.IsTrue(xel.Has<LocalXAttrEnum>());
            Assert.IsTrue(xel.Has<LocalXBAEnum>());

            // structs without [Placement] uses strict rules
            Assert.IsFalse(xel.Has<NodeType>());

            // structs without [Placement] specifying loose rules
            Assert.IsTrue(xel.Has<NodeType>(EnumParsingOption.FindUsingLowerCaseNameThenParseValue));

            // Nullable
            Assert.IsTrue(xel.Has<LocalXAttrEnum>());
            Assert.IsTrue(xel.Has<LocalXBAEnum?>());

            // Class and Nullable Class
            Assert.IsTrue(xel.Has<LocalClass>());
            Assert.IsTrue(xel.Has<LocalClass?>());
        }

        #region L O C A L S
        [Placement(EnumPlacement.UseXAttribute, "xattr")]
        private enum LocalXAttrEnum
        {
            Default,
            NonDefault,
        }

        [Placement(EnumPlacement.UseXBoundAttribute, "xba")]
        private enum LocalXBAEnum
        {
            Default,
            NonDefault,
        }

        [Placement(EnumPlacement.UseXAttribute, alwaysUseFullKey: true )]
        private enum LocalFullKeyEnum
        {
            Default,
            NonDefault,
        }

        private enum LocalSortAttributeOrder
        {
            text,
        }
        class LocalClass() { }
        #endregion L O C A L S
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
