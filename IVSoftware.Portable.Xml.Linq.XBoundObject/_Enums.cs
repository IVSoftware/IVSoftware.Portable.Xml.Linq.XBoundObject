using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq
{

    [DefaultValue(IgnoreAllCase)]
    public enum GetOption
    {
        None,
        /// <summary>
        /// The case of the 'name' argument will be ignored
        /// </summary>
        IgnoreNameCase = 0x1,

        /// <summary>
        /// The case of the 'value' argument will be ignored
        /// e.g. when calling AttributeValueEquals
        /// </summary>
        IgnoreValueCase = 0x2,

        IgnoreAllCase = 0x3,

        /// <summary>
        ///  Return value as a lower case string with spaces removed.
        /// </summary>
        NormalizeForCompare = 0x4,

        All = 0xFFFF,
    }

    [Flags, DefaultValue(NameToLower)]
    public enum SetOption
    {
        None,

        // Attribute name will be written as a lower-case string.
        NameToLower = 0x1,

        // Attribute value will be written as a lower-case string.
        ValueToLower = 0x2,

        // The default is to make boolean values lowercase
        AllowUppercaseBool = 0x4,

        // Attribute value will be written as a lower-case string.
        AllToLower = 0x3,
    }
    public enum EnumParsingOption
    {
        /// <summary>
        /// This option attempts to infer an Enum value that 
        /// is stored as a string value on an ordinary XAttribute.
        /// It parses the attribute name as the EnumType lower case,
        /// and the value as a case-sensitive string.
        /// </summary>
        AllowEnumParsing,

        /// <summary>
        /// This option retrieves an Enum value only when bound
        /// as a typed XBoundAttribute on the XElement node.
        /// </summary>
        BoundEnumTypeOnly,
    }

    /// <summary>
    /// Enum to specify how errors are reported when retrieving Enum values that might default.
    /// </summary>
    /// <remarks>
    /// Value of 0 is RESERVED for internal use.
    /// </remarks>
    public enum EnumErrorReportOption
    {
        /// <summary>
        /// Assert only when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - USE WITH CAUTION!
        /// - The tradeoff is that this allows the silent failure but avoids crashing your app.
        /// </remarks>
        Assert = 1,

        /// <summary>
        /// Throws an exception when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - Recommended: Actively prevents errors by throwing exceptions where defaults might otherwise silently pass.
        /// - Note: Transitioning from older versions? This change could impact existing implementations, as it introduces
        ///  exceptions where there were previously only assertions. Rigorous testing is advised to ensure compatibility.
        /// </remarks>
        Throw = 2,

        /// <summary>
        /// Use the current value of Compatibility.DefaultErrorReportOption
        /// </summary>
        Default = int.MinValue,
    }
    /// <summary>
    /// Manages default settings for error reporting options across the application.
    /// </summary>
    public static class Compatibility
    {
        /// <summary>
        /// Gets or sets the default error reporting option used throughout the application.
        /// </summary>
        /// <remarks>
        /// This setting determines the behavior of enum error handling when no explicit choice is made in method calls.
        /// Changing this from the default 'Assert' to 'Throw' can lead to exceptions where previously there were assertions, 
        /// potentially affecting existing code. This change should be treated as a breaking change, 
        /// and rigorous testing is advised to ensure compatibility with older versions of the application.
        /// 
        /// Upon first access, if the option is still set to 'Assert', a warning is issued to recommend setting it to 'Throw'
        /// for more robust error handling.
        /// </remarks>
        public static EnumErrorReportOption DefaultErrorReportOption
        {
            get
            {
                if(_warnAssert)
                { 
                    _warnAssert = false;    // One time warning;
                    if(Equals(_defaultErrorReportOption, EnumErrorReportOption.Assert))
                    {
                        Debug.WriteLine(string.Join(Environment.NewLine, Enumerable.Repeat("*", 5)));
                        Debug.WriteLine(
                            $"ADVISORY: It is recommended to initialize the {nameof(Compatibility)}.{nameof(DefaultErrorReportOption)} option to {EnumErrorReportOption.Throw.ToFullKey()}");
                        Debug.WriteLine(string.Join(Environment.NewLine, Enumerable.Repeat("*", 5)));
                    }
                }
                return _defaultErrorReportOption;
            }
            set
            {
                if (!Equals(_defaultErrorReportOption, value))
                {
                    _defaultErrorReportOption = value;
                }
                _warnAssert = false;
            }
        }
        private static EnumErrorReportOption _defaultErrorReportOption = EnumErrorReportOption.Assert;
        private static bool _warnAssert = true;
    }
}
