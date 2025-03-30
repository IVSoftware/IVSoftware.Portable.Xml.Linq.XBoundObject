using IVSoftware.Portable.Disposable;
using IVSoftware.Portable.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class XBoundObjectImplementer : IXBoundObject
    {
        public XBoundObjectImplementer() { }
        public XBoundObjectImplementer(XElement xel) => InitXEL(xel);
        public XElement XEL
        {
            get
            {
                if(_xel == null)
                {
                    throw new NullReferenceException(
                        $"XEL has not been initialized. Initialize it via the constructor or by calling {nameof(InitXEL)} explicitly.");
                }
                return _xel;
            }
        }
        private XElement _xel = null;
        public virtual XElement InitXEL(XElement xel)
        {
            if(_xel is null)
            {
                _xel = xel;
                _xel.Changed += (sender, e) =>
                {
                    if (sender is XAttribute xattr && ReferenceEquals(xattr.Parent, _xel))
                    {
                        Parent = _xel.Parent;
                    }
                };
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

        public XElement Parent
        {
            get => _parent;
            set
            {
                if (!ReferenceEquals(_parent, value))
                {
                    _parent = value;
                    OnPropertyChanged();
                }
            }
        }
        XElement _parent = default;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        internal string DebuggerDisplay
            => XEL?.Attribute(nameof(StdAttributeNameInternal.text))?.Value ?? "Error";
    }
    public class XBoundViewObjectImplementer : XBoundObjectImplementer, IXBoundViewObject
    {
        public XBoundViewObjectImplementer() { }
        public XBoundViewObjectImplementer(XElement xel) : base(xel) { }
        public virtual ICommand PlusMinusToggleCommand
        {
            get
            {
                if (_plusMinusToggleCommand is null)
                {
                    _plusMinusToggleCommand = new CommandPCL<IXBoundViewObject>(
                        execute: (xbvo) =>
                        {
                            switch (xbvo?.PlusMinus)
                            {
                                case PlusMinus.Collapsed:
                                    xbvo.Expand();
                                    break;
                                case PlusMinus.Partial:
                                    Debug.Fail("TODO");
                                    break;
                                case PlusMinus.Expanded:
                                    xbvo.Collapse();
                                    break;
                                default:
                                    // N O O P
                                    break;
                            }
                        });
                }
                return _plusMinusToggleCommand;
            }
        }
        ICommand _plusMinusToggleCommand = null;

        public string Text
        {
            get =>
                XEL
                .Attribute(nameof(StdAttributeNameInternal.text))?
                .Value 
                ?? string.Empty;
            set
            {
                if (value != null)
                {
                    if (!Equals(Text, value))
                    {
                        XEL.SetAttributeValue(nameof(StdAttributeNameInternal.text), value);
                        OnPropertyChanged();
                    }
                }
            }
        }
        public int Space => _indent * _depth;

        private int _indent
        {
            get => __indent ?? 5;
            set
            {
                if (!Equals(__indent, value))
                {
                    OnPropertyChanged();
                    __indent = value;
                }
            }
        }
        int? __indent = default;
        int _space;

        public int _depth
        {
            get => __depth;
            set
            {
                if (!Equals(__depth, value))
                {
                    OnPropertyChanged();
                    __depth = value;
                }
            }
        }
        int __depth = default;


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
                            pxbo.Expand(allowPartial: true);
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
            internal set
            {
                if (!Equals(PlusMinus, value))
                {
                    switch (value)
                    {
                        case PlusMinus.Auto:
                            localOnAuto();
                            break;
                        case PlusMinus.Collapsed:
                            localOnCollapse();
                            break;
                        case PlusMinus.Partial:
                            localOnPartial();
                            break;
                        case PlusMinus.Expanded:
                            localOnExpand();
                            break;
                        case PlusMinus.Leaf:
                            localOnLeaf();
                            break;
                        default: throw new NotImplementedException();
                    }
                    #region L o c a l F x       
                    void localOnAuto()
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
                            XEL.SetAttributeValueNull<PlusMinus>();
                        }
                    }

                    void localOnCollapse()
                    {
                        if(XEL.HasElements)
                        {
                            XEL.SetAttributeValue(value);
                        }
                        else
                        {
                            // Can't be collapsed!
                            XEL.SetAttributeValueNull<PlusMinus>();
                        }
                    }

                    void localOnPartial()
                    {
                        // Allow
                        XEL.SetAttributeValue(value);
                    }

                    void localOnExpand()
                    {
                        if (XEL.HasElements)
                        {
                            XEL.SetAttributeValue(value);
                        }
                        else
                        {
                            // Can't expand!
                            XEL.SetAttributeValueNull<PlusMinus>();
                        }
                    }

                    void localOnLeaf()
                    {
                        if (XEL.HasElements)
                        {
                            // Can't be leaf!
                            XEL.SetAttributeValue(PlusMinus.Collapsed);
                        }
                        else
                        {
                            XEL.SetAttributeValueNull<PlusMinus>();
                        }
                    }

                    #endregion L o c a l F x

                    OnPropertyChanged();
                }
            }
        }
        public virtual string PlusMinusGlyph
        {
            get
            {
                switch (PlusMinus)
                {
                    case PlusMinus.Collapsed:
                        return "+";
                    case PlusMinus.Partial:
                        return "*";
                    case PlusMinus.Expanded:
                        return "-";
                    default:
                        return " ";
                }
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(__depth):
                case nameof(__indent):
                case nameof(PlusMinus):
                    OnPropertyChanged(nameof(PlusMinusGlyph));
                    OnPropertyChanged(nameof(Space));
                    if(propertyName == nameof(PlusMinus))
                    {
                        OnPlusMinusChanged();
                    }
                    break;
                case nameof(Parent):
                    if( Parent is XElement pxel &&
                        XEL.AncestorOfType<ViewContext>() is ViewContext context)
                    {
                        _indent = context.Indent;
                        _depth = XEL.Ancestors().Count() - 1;
                        this.OnAwaited();
                    }
                    break;
            }
        }

        protected virtual void OnPlusMinusChanged()
        {
            switch (PlusMinus)
            {
                case PlusMinus.Collapsed:
                    foreach (
                        var item in 
                        XEL
                        .Descendants()
                        .Reverse()
                        .Select(_=>_.To<IXBoundViewObject>())
                        .Where(_=> _ != null))
                    {
                        item.IsVisible = false;
                    }
                    break;
                case PlusMinus.Partial:
                    break;
                case PlusMinus.Expanded:
                    break;
                case PlusMinus.Leaf:
                    break;
                case PlusMinus.Auto:
                    break;
                default:
                    break;
            }
        }

        public PlusMinus Expand(bool allowPartial = false)
        {
            IsVisible = true;
            if (!allowPartial)
            {
                foreach (
                    var item in
                    XEL
                    .Descendants()
                    .Reverse()
                    .Select(_ => _.To<IXBoundViewObject>())
                    .Where(_ => _ != null))
                {
                    item.IsVisible = true;
                }
            }
            PlusMinus = PlusMinus.Auto;
            Debug.Assert(!Equals(PlusMinus, PlusMinus.Auto));
            return PlusMinus; // The result is 'not' necessarily the same.
        }

        public PlusMinus Collapse()
        {
            IsVisible = true;
            PlusMinus = PlusMinus.Collapsed;
            return PlusMinus; // The result is 'not' necessarily the same.
        }
    }

    /// <summary>
    /// Represents a view model context that manages a synchronized relationship between
    /// a root <see cref="XElement"/> and a bound <see cref="IList"/> of view objects.
    /// Supports automatic synchronization, hierarchical indentation logic, and dynamic 
    /// tracking of visible elements with positional mapping.
    /// </summary>
    public class ViewContext : XBoundObjectImplementer
    {
        public int Indent { get; }
        public IList Items { get; }
        public TimeSpan AutoSyncSettleDelay { get; } = TimeSpan.FromSeconds(0.1);
        public bool SortingEnabled { get; }
        public Func<XElement, XElement, int> CustomSorter { get; }
        public bool AutoSyncEnabled { get; set; } = true;

        private readonly Dictionary<IXBoundObject, int> _o1 = null;

        DisposableHost DHostSyncing { get; } = new DisposableHost();

        public ViewContext(
            IList items,
            int indent = 10, 
            bool autoSyncEnabled = true,
            TimeSpan? autoSyncSettleDelay = null,
            bool sortingEnabled = true,
            Func<XElement, XElement, int> customSorter = null)
        {
            Indent = indent;
            Items = items;
            AutoSyncEnabled = autoSyncEnabled;
            AutoSyncSettleDelay = autoSyncSettleDelay ?? TimeSpan.FromSeconds(0.1);
            SortingEnabled = sortingEnabled;
            CustomSorter = customSorter;
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
                    #region L o c a l F x       
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
                    #endregion L o c a l F x
                };
            }
        }
        public ViewContext(
            XElement xel,
            IList items = null,
            int indent = 10,
            bool autoSyncEnabled = true,
            TimeSpan? autoSyncSettleDelay = null,
            bool sortingEnabled = true,
            Func<XElement, XElement, int> customSorter = null)
            : this(items, indent, autoSyncEnabled, autoSyncSettleDelay, sortingEnabled, customSorter) => InitXEL(xel);
        public override XElement InitXEL(XElement xel)
        {
            if (xel.Parent != null)
            {
                throw new InvalidOperationException("The receiver must be a root element.");
            }
            xel.Changed += (sender, e) =>
            {
                if (AutoSyncEnabled)
                {
                    var now = DateTime.Now;
                    if(now - _preFilter < TimeSpan.FromMilliseconds(50))
                    {
                        return;
                    }
                    if (DHostSyncing.IsZero())
                    {
                        WDTAutoSync.StartOrRestart();
                    }
                    _preFilter = now;
                }
            };
            return base.InitXEL(xel);
        }
        DateTime _preFilter = DateTime.MinValue;

        /// <summary>
        /// Synchronizes the <see cref="Items"/> collection to match the current set of visible
        /// <see cref="XElement"/> nodes in <see cref="XEL"/>. Ensures each bound object is in 
        /// the correct order, inserts missing items, and removes extraneous ones.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Items"/> is not an <see cref="ObservableCollection{T}"/> where T implements <see cref="IXBoundViewObject"/>.
        /// </exception>
        public void SyncList()
        {
            using (DHostSyncing.GetToken())
            {
                if (SortingEnabled)
                {
                    XEL.Sort(CustomSorter);
                }
                Type type = Items.GetType();
                Type genericType = null;

                for (type = Items.GetType(); type != null; type = type.BaseType)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
                    {
                        var candidateType = type.GetGenericArguments()[0];
                        if (typeof(IXBoundViewObject).IsAssignableFrom(candidateType))
                        {
                            genericType = candidateType;
                            break;
                        }
                    }
                }
                { }

                if (genericType == null)
                {
                    throw new InvalidOperationException(
                        $"SyncList requires Items to be (or inherit from) an ObservableCollection<T> where T implements IXBoundViewObject. ");
                }
                else
                {
                    var index = 0;
                    foreach (var xel in XEL.VisibleElements())
                    {
                        // Get the item that "should be" at this index
                        if (xel.To<IXBoundObject>() is IXBoundObject sbAtIndex)
                        {
                            if (!genericType.IsAssignableFrom(sbAtIndex.GetType()))
                            {
                                throw new InvalidCastException(
                                    $"XElement at path '{xel.GetPath()}' is bound to an instance of type '{sbAtIndex.GetType().FullName}', " +
                                    $"which is not assignable to the expected collection type '{genericType.FullName}'.\n" +
                                    $"Ensure that all bound elements match the declared ObservableCollection<T> type."
                                );
                            }
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
                    while (index < Items.Count)
                    {
                        Items.RemoveAt(index);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a formatted string representing the given <see cref="XElement"/> with indentation and expansion state.
        /// Intended primarily for test diagnostics and visual inspection of tree structure.
        /// </summary>
        /// <param name="xel">The element to format.</param>
        /// <param name="spacerFunc">Optional transformation function for the text value.</param>
        /// <param name="pathAttribute">Optional attribute enum used to extract the text; defaults to internal text attribute.</param>
        /// <returns>A string showing the element’s expansion state and hierarchical position.</returns>
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
                char exp;
                switch (xel.To<IXBoundViewObject>().PlusMinus)
                {
                    case PlusMinus.Collapsed:
                        exp = '+';
                        break;
                    case PlusMinus.Partial:
                        exp = '*';
                        break;
                    case PlusMinus.Expanded:
                        exp = '-';
                        break;
                    case PlusMinus.Leaf:
                    default:
                        exp = ' ';
                        break;
                }
                var spaces = string.Join(
                    string.Empty,
                    Enumerable.Repeat(
                        " ", 
                        Indent * (xel.Ancestors().Count() - 1)));
                return $"{spaces}{exp} {text}";
            }
        }
        public string ItemsToString() =>
            string
            .Join(
                Environment.NewLine,
                Items?.OfType<IXBoundViewObject>().Select(_ =>
                    GetIndentedText(_.XEL)))
                ?? "NULL items list.";

        SemaphoreSlim _busyAutoSync = new SemaphoreSlim(1, 1);
        public WatchdogTimer WDTAutoSync
        {
            get
            {
                if (_wdtAutoSync is null)
                {
                    _wdtAutoSync = new WatchdogTimer (
                        defaultInitialAction: () =>
                        {
                            this.OnAwaited(caller: $"{nameof(WatchdogTimer.StartOrRestart)}");
                        })
                    { Interval = AutoSyncSettleDelay };
                    _wdtAutoSync.RanToCompletion += async (sender, e) =>
                    {
                        await _busyAutoSync.WaitAsync();
                        try
                        {
                            SyncList();
                            this.OnAwaited(caller: nameof(WDTAutoSync));
                        }
                        finally
                        {
                            _busyAutoSync.Wait(0);
                            _busyAutoSync.Release();
                        }
                    };
                }
                return _wdtAutoSync;
            }
        }
        WatchdogTimer _wdtAutoSync = null;
    }
}
