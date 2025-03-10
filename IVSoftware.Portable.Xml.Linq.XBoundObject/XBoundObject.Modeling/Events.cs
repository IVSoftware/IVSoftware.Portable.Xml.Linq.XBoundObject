using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    class XObjectChangedOrChangingEventArgs : XObjectChangeEventArgs
    {
        public XObjectChangedOrChangingEventArgs(XObjectChangeEventArgs e, bool isChanging)
            : base(e.ObjectChange)
        {
            IsChanging = isChanging;
        }
        public bool IsChanging { get; }
    }
}
