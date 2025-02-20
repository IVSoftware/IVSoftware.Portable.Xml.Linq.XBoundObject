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
}
