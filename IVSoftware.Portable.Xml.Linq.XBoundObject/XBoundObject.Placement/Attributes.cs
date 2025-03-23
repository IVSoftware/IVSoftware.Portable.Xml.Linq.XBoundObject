using System;
using System.Collections.Generic;
using System.Text;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    public enum EnumPlacement
    {
        UseXAttribute,
        UseXBoundAttribute,
    }

    [AttributeUsage(AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class PlacementAttribute : Attribute 
    {
        public PlacementAttribute(EnumPlacement placement, string name = null)
        {
            Placement = placement;
            Name = 
                string.IsNullOrWhiteSpace(name)
                ? null  // Downgrade whitespace to null
                : name;
        }
        public EnumPlacement Placement { get; }
        public string Name { get; }
    }
}
