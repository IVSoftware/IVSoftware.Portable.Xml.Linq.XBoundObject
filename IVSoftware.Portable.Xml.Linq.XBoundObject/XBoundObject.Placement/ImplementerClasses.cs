using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class XBoundObjectImplementer : IXBoundObject
    {
        public XBoundObjectImplementer() { }
        public XBoundObjectImplementer(XElement xel) => InitXEL(xel);
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
        internal string DebuggerDisplay
            => XEL?.Attribute(nameof(StdAttributeNameInternal.text))?.Value ?? "Error";
    }
    public class XBoundViewObjectImplementer : XBoundObjectImplementer, IXBoundViewObject
    {
        public XBoundViewObjectImplementer() { }
        public XBoundViewObjectImplementer(XElement xel) : base(xel) { }


        /// <summary>
        /// If XAttribute is not present, default to false
        /// </summary>
        public bool IsVisible
        {
            get => 
                XEL.TryGetAttributeValue(out IsVisible enumValue)
                ? bool.Parse(enumValue.ToString())
                : false;
            set
            {
                if (!Equals(IsVisible, value))
                {
                    if (value)
                    {
                        XEL.SetAttributeValue(Placement.IsVisible.True);
                        // For 'True' only, parent nodes become visible also.
                        if (XEL.Parent?.To<IXBoundViewObject>() is IXBoundViewObject pxbo)
                        {
                            // [Careful]
                            // These must be set as INPC Properties not as XATTR!
                            pxbo.IsVisible = true;
                            pxbo.PlusMinus = PlusMinus.Auto;
                        }
                    }
                    else
                    {
                        XEL.SetAttributeValueNull<Placement.PlusMinus>();
                        XEL.SetAttributeValueNull<Placement.IsVisible>();
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
            get => 
                XEL.TryGetAttributeValue(out PlusMinus value)
                ? value
                : PlusMinus.Leaf;
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
                    else
                    {
                        XEL.SetAttributeValue(value);
                    }
                    OnPropertyChanged();
                }
            }
        }
    }

    /// <summary>
    /// Uses SQLite Markdown to query and filter the elements by path.
    /// </summary>
    public class ViewContext : XBoundObjectImplementer
    {
        public int Indent { get; }
        public IList Items { get; }

        private readonly Dictionary<IXBoundObject, int> _o1 = null;

        public ViewContext(IList items, int indent)
        {
            Indent = indent;
            Items = items;
            if (Items is INotifyCollectionChanged incc)
            {
                _o1 = new Dictionary<IXBoundObject, int>();
                incc.CollectionChanged += (sender, e) =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            localAdd();
                            break;
                        case NotifyCollectionChangedAction.Move:
                            {   /* G T K */
                                // N O O P
                            }
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            localRemove();
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            localRemove();
                            localAdd();
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            _o1.Clear();
                            break;
                        default: throw new NotImplementedException();
                    }
                    void localAdd()
                    {
                        if (e
                            .NewItems?
                            .OfType<IXBoundObject>()
                            .SingleOrDefault() is IXBoundObject newXBO)
                        {
                            _o1[newXBO] = e.NewStartingIndex;
                        }
                    }
                    void localRemove()
                    {
                        if (e
                            .OldItems?
                            .OfType<IXBoundObject>()
                            .SingleOrDefault() is IXBoundObject oldXBO)
                        {
                            _o1.Remove(oldXBO);
                        }
                    }
                };
            }
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
                                if (_o1.TryGetValue(sbAtIndex, out int currentIndex))
                                {
                                    Debug.Assert(
                                        currentIndex > index,
                                        $"Expecting higher index otherwise it's 'eating its own'");
                                    Items.RemoveAt(currentIndex);
                                    Items.Insert(index++, sbAtIndex);
                                    _o1[sbAtIndex] = index;
                                }
                                else
                                {
                                    Items.Insert(index++, sbAtIndex);
                                }
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
