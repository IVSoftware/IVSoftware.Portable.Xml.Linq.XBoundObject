using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class XBoundViewObjectImplementer : XBoundObjectImplementer, IXBoundViewObject
    {
        public PlusMinus Collapse(string path, Enum pathAttribute = null)
        {
            throw new NotImplementedException();
        }

        public PlusMinus Expand(string path, Enum pathAttribute = null)
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
            xel.Changing += onXObjectChange;
            xel.Changed += onXObjectChange;
        }
        private void onXObjectChange(object sender, XObjectChangeEventArgs e) 
        {
            if(sender is XAttribute xattr && xattr.Parent != null)
            {
                // XAttribute has a parent and is therefore actionable.
                switch (xattr.Name.LocalName)
                {
                    case nameof(StdAttributeNameXBoundViewObject.plusminus):
                        OnPropertyChanged(nameof(PlusMinus));
                        break;
                    case nameof(StdAttributeNameXBoundViewObject.isvisible):
                        OnPropertyChanged(nameof(IsVisible));
                        break;
                }
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

        /// <summary>
        /// If XAttribute is not present, default to false
        /// </summary>
        public bool IsVisible
        {
            get
            {
                var value = XEL.TryGetAttributeValue(out IsVisible visibility)
                ? bool.Parse(visibility.ToString())
                : false;
                if (value && XEL.Parent?.Parent != null)
                {
                    // Parents are all visible.
                    XEL.Parent.SetAttributeValue(nameof(StdAttributeNameXBoundViewObject.isvisible), bool.TrueString);
                }
                return value;
            }
            set
            {
                if (!Equals(IsVisible, value))
                {
                    if (value)
                    {
                        XEL.SetAttributeValue(nameof(StdAttributeNameXBoundViewObject.isvisible), bool.TrueString);
                    }
                    else
                    {
                        XEL.SetAttributeValue(null);
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If XAttribute is not present, default to PlusMinus.Leaf
        /// </summary>
        public PlusMinus PlusMinus
        {
            get
            {
                // The value of PlusMinus.Auto is never returned!
                var value = XEL.TryGetAttributeValue(out PlusMinus plusMinus)
                ? plusMinus
                : PlusMinus.Leaf;
                if (value == PlusMinus.Auto)
                {
                    if (XEL.Parent?.Parent != null)
                    {
                        XEL.Parent.SetAttributeValue(PlusMinus.Auto);
                    }
                    var elements = XEL.Elements().ToArray();
                    var elementsCount = elements.Length;
                    var visibleCount =
                        elements
                        .Count(_ =>
                            _
                            .Attribute(nameof(StdAttributeNameXBoundViewObject.isvisible))
                            ?.Value.ToLower() == "true");
                    if (elements.Any())
                    {
                        if (elementsCount == visibleCount)
                        {
                            XEL.SetAttributeValue(PlusMinus.Expanded);
                        }
                        else
                        {
                            XEL.SetAttributeValue(PlusMinus.Partial);
                        }
                    }
                    else
                    {
                        XEL.SetAttributeValue(PlusMinus.Leaf);
                    }
                }
                return value;
            }
            set
            {
                if (!Equals(PlusMinus, value))
                {
                    PlusMinus = value;
                    OnPropertyChanged();
                }
            }
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
