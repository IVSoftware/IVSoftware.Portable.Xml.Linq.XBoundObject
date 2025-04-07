using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq
{
    /// <summary>
    /// Provides a bidirectional lookup between Enum values and XElement nodes.
    /// 
    /// The DualKeyLookup class maintains a synchronized mapping between:
    /// - Enums (keys) → XML Elements (values)
    /// - XML Elements (keys) → Enums (values)
    ///
    /// This enables efficient two-way retrieval, allowing elements to be looked up
    /// by their associated Enum identifier and vice versa. It is useful for XML 
    /// structures that require both hierarchical data representation and runtime object binding.
    ///
    /// Features:
    /// - Direct access via indexers for Enum-to-XElement and XElement-to-Enum lookups.
    /// - Automatic synchronization between mappings to prevent inconsistencies.
    /// - Optional exception throwing for strict retrieval.
    /// - Supports dynamic modification with add, remove, and clear operations.
    /// </summary>

    [DebuggerDisplay("{Count}")]
    public class DualKeyLookup
    {
        private readonly Dictionary<Enum, XElement> _id2x = new Dictionary<Enum, XElement>();
        public XElement this[Enum key, bool @throw = false]
        {
            get =>
                @throw
                ? _id2x[key]
                : _id2x.TryGetValue(key, out var xel)
                ? xel
                : null;
            set
            {
                if (value is null)
                {
                    if (_id2x.TryGetValue(key, out var xel))
                    {
                        _id2x.Remove(key);
                        if(_x2id.ContainsKey(xel))
                        {
                            _x2id.Remove(xel);
                        }
                    }
                }
                else
                {
                    if (this[key] is XElement prevXEL)
                    {
                        if(Equals(value, prevXEL))
                        {
                            return;
                        }
                        else
                        {
                            // Eradicate the previous pair.
                            this[key] = null;
                        }
                    }
                    _id2x[key] = value;
                    _x2id[value] = key;
                }
            }
        }
        private readonly Dictionary<XElement, Enum> _x2id = new Dictionary<XElement, Enum>();

        public Enum this[XElement key, bool @throw = false]
        {
            get =>
                @throw
                ? _x2id[key]
                : _x2id.TryGetValue(key, out var id)
                ? id
                : null;
            set
            {
                if (value is null)
                {
                    if (_x2id.TryGetValue(key, out var id))
                    {
                        _x2id.Remove(key);
                        if (_id2x.ContainsKey(id))
                        {
                            _id2x.Remove(id);
                        }
                    }
                }
                else
                {
                    if (this[key] is Enum prevID)
                    {
                        if (Equals(value, prevID))
                        {
                            return;
                        }
                        else
                        {
                            // Eradicate the previous pair.
                            this[key] = null;
                        }
                    }
                    _x2id[key] = value;
                    _id2x[value] = key;
                }
            }
        }
        public int Count
        {
            get
            {
                Debug.Assert(_x2id.Count == _id2x.Count);
                return _x2id.Count;
            }
        }

        public void Clear()
        {
            _x2id.Clear();
            _id2x.Clear();
        }
    }
}
