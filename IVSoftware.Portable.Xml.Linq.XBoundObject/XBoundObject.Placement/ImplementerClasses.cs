using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

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

        public ExpandedState Collapse(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        public ExpandedState Expand(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        public XElement Show(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;

    }
    public class ViewContext : XBoundObjectImplementer
    {
        SemaphoreSlim 
            _reentrancyCheck = new SemaphoreSlim(1, 1);
        public ViewContext(int indent) => Indent = indent;
        public ViewContext(XElement xel, int indent)
            : this(indent)
        {
            InitXEL(xel);
            xel.Changing += onChanging;
            xel.Changed += onChanged;
        }

        private void onChanging(object sender, XObjectChangeEventArgs e)
        {
            switch(sender)
            {
                case XAttribute xattr:
                    if(xattr.Parent is XElement)
                    {
                        onXAttributeChange(xattr.Parent, xattr, e);
                    }
                    break;
                case XElement xel:
                    if (xel.Parent is XElement)
                    {
                        onXElementChange(xel, xel.Parent, e);
                    }
                    break;
            }
        }

        private void onChanged(object sender, XObjectChangeEventArgs e)
        {
            switch (sender)
            {
                case XAttribute xattr:
                    if (xattr.Parent is XElement)
                    {
                        onXAttributeChange(xattr.Parent, xattr, e);
                    }
                    break;
                case XElement xel:
                    if (xel.Parent is XElement)
                    {
                        onXElementChange(xel, xel.Parent, e);
                    }
                    break;
            }
        }

        private void onXAttributeChange(XElement xel, XAttribute xattr, XObjectChangeEventArgs e)
        {
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                    break;
                case XObjectChange.Name:
                    break;
                case XObjectChange.Remove:
                    break;
                case XObjectChange.Value:
                    break;
                default: throw new NotImplementedException();
            }
        }
        void onXElementChange(XElement xel, XElement pxel, XObjectChangeEventArgs e)
        {3
            switch (e.ObjectChange)
            {
                case XObjectChange.Add:
                    break;
                case XObjectChange.Name:
                    break;
                case XObjectChange.Remove:
                    break;
                case XObjectChange.Value:
                    break;
                default: throw new NotImplementedException();
            }
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

    /// <summary>
    /// Uses SQLite Markdown to query and filter the elements by path.
    /// </summary>
    public class XBoundFilteredViewImplementer : ViewContext
    {
        public XBoundFilteredViewImplementer(object dataSource, int indent) 
            : base(indent)
            => _dataSource = dataSource;
        public XBoundFilteredViewImplementer(
            XElement xel,
            object databaseConnection,
            int indent)
            : base(xel, indent)
            => _dataSource = databaseConnection;

        private readonly object _dataSource;
        public T GetDatabaseConnection<T>() => (T)_dataSource;
    }
}
