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

    public enum EnumErrorReportOption
    {
        /// <summary>
        /// Skip error reporting.
        /// </summary>
        /// <remarks>
        /// Intended for internal use where client method is
        /// responsible for high-level error reporting.
        /// </remarks>
        None,

        /// <summary>
        /// Assert only when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - USE WITH CAUTION!
        /// - The tradeoff is that this allows the silent failure but avoids crashing your app.
        /// </remarks>
        Assert = 1,

        /// <summary>
        /// Throws exception when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - RECOMMENDED
        /// </remarks>
        Throw = 2,

        /// <summary>
        /// Uses compatibility setting
        /// </summary>
        /// <remarks>
        /// - RECOMMENDED but YOU MUST EXPLICITLY SET THIS!
        /// </remarks>
        Default = int.MinValue,
    }
    public static class Compatibility
    {
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
                            $"ADVISORY: It is recommended that you initialize {nameof(Compatibility)}.{nameof(DefaultErrorReportOption)} option to {EnumErrorReportOption.Throw.ToFullKey()}");
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
