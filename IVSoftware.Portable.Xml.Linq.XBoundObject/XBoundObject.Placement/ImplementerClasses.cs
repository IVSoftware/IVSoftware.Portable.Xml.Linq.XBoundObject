using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
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
        bool _reentrancyCheck = false;
        object _lock = new object();
        public XBoundObjectImplementer() { }
        public XBoundObjectImplementer(XElement xel) => InitXEL(xel);
        public XElement XEL => _xel;
        private XElement _xel;
        public virtual XElement InitXEL(XElement xel)
        {
            if(_xel is null)
            {
                _xel = xel;
                _xel.Changing += onXObjectChange;
                _xel.Changed += onXObjectChange;                

                #region L o c a l F x
		        void onXObjectChange(object sender, XObjectChangeEventArgs e)
                {
                    if (sender is XAttribute xattr && ReferenceEquals(xattr.Parent, XEL))
                    {
                        // Actionable change to one of "this" objects attributes.
                        lock (_lock)
                        {
                            if (_reentrancyCheck)
                            {
                                return;
                            }
                            try
                            {
                                _reentrancyCheck = true;
                                OnAttributeChanged(xattr, e);
                            }
                            finally
                            {
                                _reentrancyCheck = false;
                            }
                        }
                    }
                }
                #endregion L o c a l F x
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

        protected virtual void OnAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e) { }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class XBoundViewObjectImplementer : XBoundObjectImplementer, IXBoundViewObject
    {
        public XBoundViewObjectImplementer() { }
        public XBoundViewObjectImplementer(XElement xel) : base(xel) { }

        /// <summary>
        /// When an actionable (has parent) attribute change occurs, the
        /// value is retrieved and set to the implementer property value
        /// which handles any circularity. 
        /// </summary>
        /// <remarks>
        /// For example, when isvisible is set to "false" and that
        /// attribute gets removed, the IsVisible property receives
        /// IsVisible = false teo times but only responds once.
        /// </remarks>
        protected override void OnAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            base.OnAttributeChanged(xattr, e);
            var xel = xattr.Parent;     // [Careful] This is a LATERAL 'parent' !!
            switch (xattr.Name.LocalName)
            {
                case nameof(StdAttributeNameXBoundViewObject.plusminus):
                    // - Retrieve the current value for 'plusminus'
                    // - Forward to datamodel.
                    PlusMinus = 
                        xel.TryGetAttributeValue(out PlusMinus plusMinus)
                        ? plusMinus
                        : PlusMinus.Leaf;
                    break;
                case nameof(StdAttributeNameXBoundViewObject.isvisible):
                    // - Retrieve the current value for 'isvisible' and
                    // - Forward to datamodel.
                    // - A null attribute converts to 'false'.
                    IsVisible =
                        xel
                        .TryGetAttributeValue(out IsVisible value) &&
                        bool.Parse(value.ToString());
                    break;
            }
        }
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

        /// <summary>
        /// If XAttribute is not present, default to false
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (!Equals(_isVisible, value))
                {
                    _isVisible = value;
                    if (value)
                    {
                        // For 'True' only, parent nodes must take on isvisible also.
                        // [Careful] Exclude the root node!
                        if (XEL.Parent is XElement pxel && pxel.Parent != null)
                        {
                            pxel.SetAttributeValue(Placement.IsVisible.True);
                            pxel.SetAttributeValue(Placement.PlusMinus.Auto);
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }
        bool _isVisible;

        /// <summary>
        /// If XAttribute is not present, default to PlusMinus.Leaf
        /// </summary>
        public PlusMinus PlusMinus
        {
            get => _plusMinus;
            set
            {
                if (!Equals(PlusMinus, value))
                {
                    if (value == PlusMinus.Auto)
                    {
                        // [Careful]
                        // - This is 'not' like IsVisible where we ascend the hierarchy.
                        // - In fact, this is probably being called in response to IsVisible = true.
                        // - Point is, we're dealing with this level only!
                        var elements = XEL.Elements().ToArray();
                        var elementsCount = elements.Length;
                        var visibleCount =
                            elements
                            .Count(_ =>
                                _
                                .Attribute(nameof(StdAttributeNameXBoundViewObject.isvisible))
                                ?.Value.ToLower() == "true");

                        // In each case, set backing store first so that
                        // the attribute change doesn't cause reentry.
                        if (elements.Any())
                        {
                            if (elementsCount == visibleCount)
                            {
                                _plusMinus = PlusMinus.Expanded;
                                XEL.SetAttributeValue(PlusMinus.Expanded);
                            }
                            else
                            {
                                _plusMinus = PlusMinus.Partial;
                                XEL.SetAttributeValue(PlusMinus.Partial);
                            }
                        }
                        else
                        {
                            _plusMinus = PlusMinus.Leaf;
                            XEL.SetAttributeValue(PlusMinus.Leaf);
                        }
                    }
                    else
                    {
                        _plusMinus = value;
                    }
                    OnPropertyChanged();
                }
            }
        }
        PlusMinus _plusMinus = PlusMinus.Leaf;
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
    public class ViewContext : XBoundObjectImplementer
    {
        public int Indent { get; }
        public ViewContext(int indent) => Indent = indent;
        public ViewContext(XElement xel, int indent)
            : this(indent) => InitXEL(xel);
        public override XElement InitXEL(XElement xel)
        {
            if (xel.Parent != null)
            {
                throw new InvalidOperationException("The receiver must be a root element.");
            }
            return base.InitXEL(xel);
        }
        protected override void OnAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            base.OnAttributeChanged(xattr, e);
            switch (xattr.Name.LocalName)
            {
                case nameof(StdAttributeNameXBoundViewObject.plusminus):
                    Debug.Fail("Unexpected because this should be a root node.");
                    break;
                case nameof(StdAttributeNameXBoundViewObject.isvisible):
                    Debug.Fail("Unexpected because this should be a root node.");
                    break;
            }
        }
    }
}
