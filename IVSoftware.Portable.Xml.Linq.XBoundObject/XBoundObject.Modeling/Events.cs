using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    public class XObjectChangedOrChangingEventArgs : XObjectChangeEventArgs
    {
        public XObjectChangedOrChangingEventArgs(XObjectChangeEventArgs e, bool isChanging)
            : base(e.ObjectChange)
        {
            IsChanging = isChanging;
        }
        public bool IsChanging { get; }
        public string ToString(object sender, bool timestamp, string format = @"hh\:mm\:ss tt")
        {
            string
                name = null,
                typeName = null,
                propertyName = null;

            switch (sender)
            {
                case XAttribute xattr:
                    typeName = nameof(XAttribute);
                    name = xattr.Name.LocalName;
                    break;
                case XElement xel:
                    typeName = nameof(XElement);
                    name = xel.Name.LocalName;
                    propertyName = xel.Attribute(nameof(SortOrderNOD.name))?.Value;
                    break;
                default:
                    var msg = $"ERROR: Sender is {(sender?.GetType()?.Name ?? "Unknown")}";
                    Debug.Fail(msg);
                    return msg;
            }
            var value = $@"{ObjectChange.ToString().PadRight(6)} {typeName.PadRight(10)} {(IsChanging ? "Changing:" : "Changed :")} {name.PadRight(6)} {propertyName}";
            if (timestamp)
            {
                return $"[{DateTime.Now.ToString(format)}] {value}";
            }
            else
            {
                return value;
            }
        }
    }
}
