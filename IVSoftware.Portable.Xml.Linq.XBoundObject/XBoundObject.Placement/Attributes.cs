using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    public enum EnumPlacement
    {
        UseXAttribute,
        UseXBoundAttribute,
    }

    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    [DebuggerDisplay("{Placement} {DisplayName} FullKey={AlwaysUseFullKey}")]
    public class PlacementAttribute : Attribute 
    {
        public PlacementAttribute(EnumPlacement placement, string name = null, bool alwaysUseFullKey = false)
        {
            Placement = placement;
            Name = 
                string.IsNullOrWhiteSpace(name)
                ? null  // Downgrade whitespace to null
                : name;
            AlwaysUseFullKey = alwaysUseFullKey;
        }
        public EnumPlacement Placement { get; }
        public bool AlwaysUseFullKey { get; } = false;
        public string Name { get; }

        internal string DisplayName =>
            string.IsNullOrWhiteSpace(Name)
            ? Name
            : $"Name='{Name}'";
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DataModelAttribute : Attribute
    {
        public DataModelAttribute(string xname = null)
            => XName = xname;
        public string XName { get; }
    }
}
