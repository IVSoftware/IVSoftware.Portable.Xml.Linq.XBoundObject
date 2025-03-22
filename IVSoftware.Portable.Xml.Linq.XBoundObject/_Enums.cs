using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        RequireEnumIsType,
    }

    [Flags]
    public enum Version1_4_ErrorReportingOption
    {
        /// <summary>
        /// Assert only when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - DEFAULT VALUE but USE WITH CAUTION!
        /// - The tradeoff is that this allows the silent failure but avoids crashing your app.
        /// </remarks>
        Assert = 0x0,

        /// <summary>
        /// Throws exception when a default enum value might be inadvertently returned.
        /// </summary>
        /// <remarks>
        /// - RECOMMENDED but YOU MUST EXPLICITLY SET THIS!
        /// </remarks>
        Throw = 0x1,
    }
}
