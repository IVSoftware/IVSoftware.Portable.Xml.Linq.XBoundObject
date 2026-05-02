using System;
using System.Collections.Generic;
namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    /// <summary>
    /// Semantic option bag for placer customization.
    /// </summary>
    /// <remarks>
    /// This public wrapper remains part of the published contract for callers
    /// that pass structured options into the variadic <c>Place(...)</c> API.
    /// </remarks>
    public class PlacerKeysDictionary : Dictionary<StdPlacerKeys, string>
    {
        [Obsolete("Compatibility shim for the published 2.0.3 contract. Prefer Count or direct key access.")]
        public int Capacity => Count;

        [Obsolete("Compatibility shim for the published 2.0.3 contract. Use the dictionary instance directly.")]
        public object GetAlternateLookup() => this;

        [Obsolete("Compatibility shim for the published 2.0.3 contract. Use the dictionary instance directly.")]
        public bool TryGetAlternateLookup(out object lookup)
        {
            lookup = this;
            return true;
        }
    }
}
