using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    public class XBoundObjectImplementer : IXBoundObject
    {
        public XBoundObjectImplementer() { }
        public XBoundObjectImplementer(XElement xel) => _xel = xel;
        public XElement XEL => _xel;
        private XElement _xel;
        public virtual XElement InitXEL(XElement xel)
        {
            if(_xel is null)
            {
                _xel = xel;
            }
            else
            {
                if(!ReferenceEquals(xel, _xel))
                {
                    throw new InvalidOperationException($"{nameof(XEL)} is already initialized to a diiferent value.");
                }
            }
            return _xel;
        }
    }
    public class XBoundViewObjectImplementer : XBoundObjectImplementer, IXBoundViewObject
    {
        public XElement XEL => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

        public ExpandedState Collapse(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        public ExpandedState Expand(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        public XElement InitXEL(XElement xel)
        {
            throw new NotImplementedException();
        }

        public XElement Show(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }
    }
    public class XBoundViewImplementer : XBoundObjectImplementer
    {
        public XBoundViewImplementer(int indent) => Indent = indent;
        public XBoundViewImplementer(XElement xel, int indent)
            : this(indent)
        {
            InitXEL(xel);
        }
        public int Indent { get; }

        public override XElement InitXEL(XElement xel)
        {
            if (xel.Parent != null)
            {
                throw new InvalidOperationException("The receiver must be a root element.");
            }
            return base.InitXEL(xel);
        }
    }
    public class XBoundIndexedViewImplementer : XBoundViewImplementer
    {
        public XBoundIndexedViewImplementer(object databaseConnection, int indent) 
            : base(indent)
            => _databaseConnection = databaseConnection;
        public XBoundIndexedViewImplementer(
            XElement xel,
            object databaseConnection,
            int indent)
            : base(xel, indent)
            => _databaseConnection = databaseConnection;

        private readonly object _databaseConnection;
        public T GetDatabaseConnection<T>() => (T)_databaseConnection;
    }
}
