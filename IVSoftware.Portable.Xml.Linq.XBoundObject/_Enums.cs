﻿using IVSoftware.Portable.Xml.Linq.XBoundObject.Placement;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        [Obsolete("This library no longer concerns itself with raw bool values.")]
        AllowUppercaseBool = 0x4,

        // Attribute value will be written as a lower-case string.
        AllToLower = 0x3,
    }
    public enum EnumParsingOption
    {
        /// <summary>
        /// This option retrieves an Enum value only when bound
        /// stated rules using a [Placement] attribute.
        /// </summary>
        UseStrictRules,

        /// <summary>
        /// This option attempts to infer an Enum value that 
        /// is stored as a string value on an ordinary XAttribute.
        /// It parses the attribute name as the EnumType lower case,
        /// and the value as a case-sensitive string.
        /// </summary>
        FindUsingLowerCaseNameThenParseValue,

        /// <summary>
        /// This option attempts to infer an Enum value that 
        /// is stored as a string value on an ordinary XAttribute.
        /// It parses the attribute name as the EnumType lower case,
        /// and the value ignoring the case.
        /// </summary>
        FindUsingLowerCaseNameThenParseValueIgnoreCase,
    }
    public enum TrySingleStatus
    {
        FoundOne,
        FoundNone,
        FoundMany,
    }
}
