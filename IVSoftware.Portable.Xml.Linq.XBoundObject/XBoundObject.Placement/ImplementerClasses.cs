using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
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
        /// Responds to attribute changes by updating the corresponding model properties,
        /// including handling <see cref="XObjectChange.Remove"/> in a pre-removal state to maintain parent context.
        /// </summary>
        /// <remarks>
        /// When an actionable (has parent) attribute change occurs, the value is retrieved and set to the implementer property,
        /// which handles any circularity. For example, when "isvisible" is set to false and then removed, the model receives the
        /// false value only once.
        /// For <see cref="XObjectChange.Remove"/>, the method is invoked before the attribute is actually removed,
        /// allowing access to its parent but not its value; the model is updated as if the attribute were already removed.
        /// </remarks>
        protected override void OnAttributeChanged(XAttribute xattr, XObjectChangeEventArgs e)
        {
            base.OnAttributeChanged(xattr, e);

            if (e.ObjectChange == XObjectChange.Remove)
            {
                // [Careful]
                // - This is called on _xel.Changing not _xel.Changed.
                // - We do this in order to still have an attr parent for the operation.
                // - But this means that the xattr HASN'T BEEN REMOVED YET.
                // [CRITICAL]
                // - Do not attempt to read the value of the xattr being removed!!!
                // - Instead, set the model value as it it were already gone.
                switch (xattr.Name.LocalName)
                {
                    case nameof(StdAttributeNameXBoundViewObject.plusminus):
                        PlusMinus = PlusMinus.Leaf;
                        break;
                    case nameof(StdAttributeNameXBoundViewObject.isvisible):
                        IsVisible = false;
                        break;
                }
            }
            else
            {
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
                            xel.TryGetAttributeValue(out IsVisible value) &&
                            bool.Parse(value.ToString());
                        break;
                }
            }
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
                    else XEL.SetAttributeValueNull<Placement.PlusMinus>();
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
    public class ViewContext : XBoundObjectImplementer
    {
        public int Indent { get; }
        public IList Items { get; }

        public ViewContext(IList items, int indent)
        {
            Indent = indent;
            Items = items;
        }
        public ViewContext(XElement xel, IList items, int indent)
            : this(items, indent) => InitXEL(xel);
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

        public void SyncList()
        {
            var type = Items.GetType();
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(ObservableCollection<>) &&
                typeof(IXBoundViewObject).IsAssignableFrom(type.GetGenericArguments()[0]))
            {
                localSyncDynamic();
            }
            else throw new InvalidOperationException(
                $"SyncList requires Items to be an ObservableCollection<T> where T implements IXBoundViewObject. " +
                $"Actual type: {type.FullName}");

            void localSyncDynamic()
            {
                var index = 0;
                foreach (var xel in XEL.VisibleElements())
                {
                    // Get the item that "should be" at this index
                    if (xel.To<IXBoundObject>() is IXBoundObject sbAtIndex)
                    {
                        if (index < Items.Count)
                        {
                            var isAtIndex = Items[index];
                            if (ReferenceEquals(isAtIndex, sbAtIndex))
                            {
                                index++;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            Items.Insert(index++, sbAtIndex);
                        }
                    }
                    else
                    {
                        Debug.Fail($"Expecting {nameof(IXBoundObject)} instance is always bound to xel.");
                    }
                }
                while(index < Items.Count)
                {
                    Items.RemoveAt(index);
                }
            }
#if false
dynamic observable = Items;
observable.Move(oldIndex, newIndex);


            fileItem.PlusMinus = "-";
            foreach (var xel in XRoot.VisibleElements())
            {
                var currentFileItem = xel.To<FileItem>();
                var currentIndex = FileItems.IndexOf(currentFileItem);
                if (index < FileItems.Count)
                {
                    var current = FileItems[index];
                    if (ReferenceEquals(current, currentFileItem))
                    {   /* G T K */
                        // Item exists at the correct index.
                    }
                    else
                    {
                        if (currentIndex == -1)
                        {
                            FileItems.Insert(index, currentFileItem);
                        }
                        else
                        {
                            Debug.Fail("First Time! Make sure this works.");
                            FileItems.Move(currentIndex, index);
                        }
                    }
                }
                else
                {
                    FileItems.Insert(index, currentFileItem);
                }
                index++;
            }
#endif
        }

        public string GetIndentedText(
            XElement xel, 
            Func<string,string> spacerFunc = null,
            Enum pathAttribute = null)
        {
            pathAttribute = pathAttribute ?? StdAttributeNameInternal.text;
            spacerFunc = spacerFunc ?? localSpacerFunc;
            if (xel.Attribute(pathAttribute.ToString()) is XAttribute xattr)
            {
                return spacerFunc(xattr.Value);
            }
            else return spacerFunc("Error");

            string localSpacerFunc(string text)
            {
                var spaces = string.Join(
                    string.Empty,
                    Enumerable.Repeat(
                        " ", 
                        Indent * (xel.Ancestors().Count() - 1)));
                return $"{spaces}{text}";
            }
        }
    }
}
