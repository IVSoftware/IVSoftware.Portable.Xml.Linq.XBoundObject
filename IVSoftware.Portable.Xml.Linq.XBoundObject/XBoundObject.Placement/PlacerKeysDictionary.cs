using IVSoftware.Portable.Common.Attributes;
using System;
using System.Collections.Generic;
namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    /// <summary>
    /// Semantic option bag for placer customization.
    /// </summary>
    /// <remarks>
    /// Includes 3 inert signatures for the express purpose of
    /// fulfilling an implicit public contract on the BC.
    /// </remarks>
    public class PlacerKeysDictionary : Dictionary<StdPlacerKeys, string>
    {
        [Careful("Contract fulfillment on base class - do not remove.")]
        public int Capacity => Count;

        [Careful("Contract fulfillment on base class - do not remove.")]
        public object GetAlternateLookup() => this;

        [Careful("Contract fulfillment on base class - do not remove.")]
        public bool TryGetAlternateLookup(out object lookup)
        {
            lookup = this;
            return true;
        }
    }
}
