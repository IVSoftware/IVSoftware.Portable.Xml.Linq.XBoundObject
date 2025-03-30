
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Placement
{
    public enum PlacerMode
    {
        FindOrPartial,
        FindOrCreate,
        FindOrThrow,
        FindOrAssert,
    }
    public enum PlacerResult
    {
        NotFound,
        Partial,
        Exists,
        Created,
        Assert,
        Throw,
    }

    /// <summary>
    /// Manages XML element placement based on attribute-driven paths with configurable behavior 
    /// for node creation and existence checks. Supports event-driven notifications for node manipulation 
    /// and traversal, allowing for extensive customization and error handling in XML document modifications.
    /// </summary>
    /// <remarks>
    /// The 'fqpath' argument is always assumed to be relative to an implicit root. Avoid setting the path 
    /// attribute for this implicit root node, i.e. if the "text" attribute holds the label text for the
    /// level, than the root node should not have a value for the "text" attribute.
    /// </remarks>
    /// <param name="fqpath">
    /// Specifies the fully qualified path of the target XML element as a string. The path is used to navigate through the XML structure, 
    /// where each segment of the path represents an element identified by its attribute value. This path should be delimited by the 
    /// platform-specific `Path.DirectorySeparatorChar`, and is always assumed to be relative to the root element of the XML document.
    /// </param>
    /// <param name="onBeforeAdd">
    /// Optional. An event handler that is invoked before a new XML element is added. Provides a chance to customize the addition process.
    /// </param>
    /// <param name="onAfterAdd">
    /// Optional. An event handler that is invoked after a new XML element is added. Allows for actions to be taken immediately following the addition.
    /// </param>
    /// <param name="onIterate">
    /// Optional. An event handler that is invoked as each path segment is processed, providing real-time feedback and control over the traversal.
    /// </param>
    /// <param name="mode">
    /// Specifies the behavior of the Placer when path segments are not found. Default is PlacerMode.FindOrCreate.
    /// </param>
    /// <param name="pathAttributeName">
    /// The name of the attribute used to match each XML element during the path navigation. Default is "text".
    /// </param>
    public class Placer
    {
        public Placer(
            XElement xSource,
            string fqpath,  // String delimited using platform Path.DirectorySeparatorChar
            AddEventHandler onBeforeAdd = null,
            AddEventHandler onAfterAdd = null,
            IterateEventHandler onIterate = null,
            PlacerMode mode = PlacerMode.FindOrCreate,
            string pathAttributeName = "text"
        ) : this(
                xSource: xSource,
                parse:
                    fqpath
                    .GetValid()
                    .Trim()
                    .Split(Path.DirectorySeparatorChar)
                    .Where(_ => !string.IsNullOrWhiteSpace(_))  // For example, a drive iteration like C:\ that ends with a delimiter
                    .ToArray(),
                onBeforeAdd: onBeforeAdd,
                onAfterAdd: onAfterAdd,
                onIterate: onIterate,
                mode: mode,
                pathAttributeName: pathAttributeName
            )
        { }

        /// <summary>
        /// Initializes a new instance of the Placer class, allowing XML element placement using a pre-defined array of path segments. 
        /// This constructor is suited for scenarios where path segments are already determined and not bound to the platform's path delimiter.
        /// </summary>
        /// <param name="xSource">
        /// The root XML element from which path traversal begins.
        /// </param>
        /// <param name="parse">
        /// An array of strings representing the segments of the path to navigate through the XML structure. Each element of the array 
        /// represents one segment of the path, corresponding to an element identified by its attribute value.
        /// </param>
        /// <param name="onBeforeAdd">
        /// Optional. An event handler that is invoked before a new XML element is added. Provides a chance to customize the addition process.
        /// </param>
        /// <param name="onAfterAdd">
        /// Optional. An event handler that is invoked after a new XML element is added. Allows for actions to be taken immediately following the addition.
        /// </param>
        /// <param name="onIterate">
        /// Optional. An event handler that is invoked as each path segment is processed, providing real-time feedback and control over the traversal.
        /// </param>
        /// <param name="mode">
        /// Specifies the behavior of the Placer when path segments are not found. Default is PlacerMode.FindOrCreate.
        /// </param>
        /// <param name="pathAttributeName">
        /// The name of the attribute used to match each XML element during the path navigation. Default is "text".
        /// </param>
        public Placer(
            XElement xSource,
            string[] parse, // Array of strings (decoupled from any predetermined delimiter) 
            AddEventHandler onBeforeAdd = null,
            AddEventHandler onAfterAdd = null,
            IterateEventHandler onIterate = null,
            PlacerMode mode = PlacerMode.FindOrCreate,
            string pathAttributeName = null
        )
        {
            pathAttributeName = pathAttributeName ?? DefaultPathAttributeName;
            _xSource = _xTraverse = xSource;
            _onBeforeAdd = onBeforeAdd;
            _onAfterAdd = onAfterAdd;
            _onIterate = onIterate;
            _mode = mode;
            placeUsingAttributePath(parse, pathAttributeName);
        }

        AddEventHandler _onBeforeAdd { get; }
        AddEventHandler _onAfterAdd { get; }
        IterateEventHandler _onIterate { get; }

        PlacerMode _mode;

        private readonly XElement _xSource;     // Please don't remove. This helps us debug monitor progress.
        private XElement _xTraverse;
        public XElement XResult { get; private set; }
        private void placeUsingAttributePath(string[] parse, string pathAttributeName)
        {
            string requestPath, currentPath;
            bool isPathMatch;

            var builder = new List<string>();
            string level = string.Empty;

            // The first loop traverses the existing paths
            XElement @try;
            int i = 0;
            while (i < parse.Length)
            {
                level = parse[i];
                try
                {
                    if (
                            i == 0 &&
                            _xTraverse.Attribute(pathAttributeName)?.Value is string value1 &&
                            value1 == level
                        )
                    {
                        // Detected a benign explicit match for path attribute set on root (not recommended).
                        Debug.WriteLine(string.Join(Environment.NewLine, Enumerable.Repeat("*", 5)));
                        Debug.WriteLine(
                            $"ADVISORY: Setting path attribute on root is 'not' recommended.");
                        Debug.WriteLine(string.Join(Environment.NewLine, Enumerable.Repeat("*", 5)));
                        @try = _xTraverse;
                    }
                    else
                    {
                        @try =
                            _xTraverse.Elements()
                            .SingleOrDefault(xel =>
                                xel.Attribute(pathAttributeName)?.Value is string value2 &&
                                value2 == level);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message);
                    @try =
                        _xTraverse.Elements()
                        .FirstOrDefault(xel => xel.Attribute(pathAttributeName).Value == level);
                }
                if (@try == null)
                {
                    break;
                }
                else
                {
                    _xTraverse = @try;
                }
                builder.Add(level);
                if (_onIterate != null)
                {
                    requestPath = string.Join(@"\", parse);
                    currentPath = string.Join(@"\", builder);
                    isPathMatch = requestPath == currentPath;
                    _onIterate.Invoke(this, new IterateEventArgs(
                        current: @try,
                        path: string.Join(@"\", builder),
                        isPathMatch: isPathMatch
                    ));
                }
                i++;
            }
            if (i == parse.Length)
            {
                PlacerResult = PlacerResult.Exists;
                XResult = _xTraverse;
            }
            else
            {
                switch (_mode)
                {
                    case PlacerMode.FindOrPartial:
                        XResult = _xTraverse;
                        PlacerResult =
                            XResult == null ?
                                PlacerResult.NotFound :
                                PlacerResult.Partial;
                        return;
                    case PlacerMode.FindOrCreate:
                        break;
                    case PlacerMode.FindOrThrow:
                        PlacerResult = PlacerResult.Throw;
                        throw new KeyNotFoundException("Missing paths identified.");
                    default:
                    case PlacerMode.FindOrAssert:
                        PlacerResult = PlacerResult.Assert;
                        Debug.Fail("Missing paths identified.");
                        return;
                }
                // The second loop appends XElements to the path until it's complete
                while (i < parse.Length)
                {
                    level = parse[i];
                    builder.Add(level);

                    switch (_mode)
                    {
                        case PlacerMode.FindOrCreate:
                            // When using the mode of 'PathSource.AttributeValue'
                            // it's important to NFW the xElementName.
                            requestPath = string.Join(@"\", parse);
                            currentPath = string.Join(@"\", builder);
                            isPathMatch = requestPath == currentPath;
                            var e = new AddEventArgs(parent: _xTraverse, path: currentPath, isPathMatch: isPathMatch);
                            // Give user an opportunity to perform a custom placement of the element
                            _onBeforeAdd?.Invoke(this, e);
                            e.Xel.SetAttributeValue(pathAttributeName, level);
                            if (!e.Handled)
                            {
                                var existing = _xTraverse.Elements().ToArray();
                                if (e.InsertIndex == null || (int)e.InsertIndex >= existing.Length)
                                {
                                    _xTraverse.Add(e.Xel);
                                }
                                else
                                {
                                    var insertIndex = (int)e.InsertIndex;
                                    var insertBefore = existing[insertIndex];
                                    insertBefore.AddBeforeSelf(e.Xel);
                                }
                            }
                            _onAfterAdd?.Invoke(this, e);
                            if (_onIterate != null)
                            {
                                _onIterate.Invoke(this, e);
                            }
                            _xTraverse = e.Xel;
                            Placed++;
                            break;
                        default:
                            break;
                    }
                    i++;
                }
                // This operation is a success.
                PlacerResult = PlacerResult.Created;
                XResult = _xTraverse;
            }
        }
        public static implicit operator bool(Placer ppr)
        {
            // True ONLY if already existed.
            return ppr.PlacerResult == PlacerResult.Exists;
        }

        public uint Placed { get; private set; }

        #region P R O P E R T I E S
        public PlacerResult PlacerResult { get; private set; } = (PlacerResult)(-1);
        public static string DefaultNewXElementName
        {
            get => _defaultNewXElementName;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (Equals(_defaultNewXElementName, value)) return;
                _defaultNewXElementName = value;
            }
        }
        static string _defaultNewXElementName = "xnode";

        public static string DefaultPathAttributeName
        {
            get => _defaultPathAttributeName;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                if (Equals(_defaultPathAttributeName, value)) return;
                _defaultPathAttributeName = value;
            }
        }
        static string _defaultPathAttributeName = "text";



        #endregion P R O P E R T I E S
    }
    public delegate void AddEventHandler(object sender, AddEventArgs e);
    public delegate void IterateEventHandler(object sender, IterateEventArgs e);
    public class AddEventArgs : EventArgs
    {
        public AddEventArgs(XElement parent, string path, string newXElementName, bool isPathMatch)
        {
            newXElementName =
                string.IsNullOrWhiteSpace(newXElementName)
                ? Placer.DefaultNewXElementName
                : newXElementName;
            Parent = parent;
            Path = path;
            Xel = new XElement(newXElementName);
            IsPathMatch = isPathMatch;
        }
        public AddEventArgs(XElement parent, string path, bool isPathMatch)
            : this(parent, path, null, isPathMatch) { }

        /// <summary>
        /// The current partial or full path of the traverse.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// User has the option to provide (swap out) the XElement. Otherwise default XElement({xElementName})
        /// </summary>
        public XElement Xel { get; set; }

        /// <summary>
        /// The parent of the traverse node (never null).
        /// </summary>
        public XElement Parent { get; }

        /// <summary>
        /// Returns true if the element being added matches the path of the original request.
        /// </summary>
        public bool IsPathMatch { get; }

        // User has option of adding/sorting/cancelling
        public bool Handled { get; set; }
        public int? InsertIndex { get; set; }
    }

    public class IterateEventArgs : EventArgs
    {
        public IterateEventArgs(XElement current, string path, bool isPathMatch)
        {
            XelCurrent = current;
            Path = path;
            IsPathMatch = isPathMatch;
        }
        public static implicit operator IterateEventArgs(AddEventArgs e) =>
            new IterateEventArgs(e.Xel, e.Path, e.IsPathMatch);

        /// <summary>
        /// The current partial or full path of the traverse.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The traverse node (never null).
        /// </summary>
        public XElement XelCurrent { get; }

        /// <summary>
        /// Returns true if the element being added matches the path of the original request.
        /// </summary>
        public bool IsPathMatch { get; }

        // User has option of adding/sorting/cancelling
        public bool Handled { get; set; }
        public int? InsertIndex { get; set; }
    }
    public enum StdPlacerKeys
    {
        NewXElementName,
        PathAttributeName,
    }
    public static partial class Extensions
    {
        /// <summary>
        /// Finds or replaces an <see cref="XElement"/> at the specified path within the current element. 
        /// If the path does not exist, it is created. Supports optional event handlers for customization.
        /// </summary>
        /// <param name="path">The hierarchical path to locate or create.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to "text".</param>
        /// <param name="onBeforeAdd">Optional handler invoked before adding a new element.</param>
        /// <param name="onAfterAdd">Optional handler invoked after adding a new element.</param>
        /// <param name="onIterate">Optional handler invoked during path traversal.</param>
        /// <returns>The located or newly created <see cref="XElement"/>.</returns>

        public static XElement FindOrCreate(
                this XElement @this,
                string path,
                Enum pathAttribute = null,
                AddEventHandler onBeforeAdd = null,
                AddEventHandler onAfterAdd = null,
                IterateEventHandler onIterate = null
            )
        {
            var placer = new Placer(
                @this,
                path,
                onBeforeAdd,
                onAfterAdd,
                onIterate, 
                PlacerMode.FindOrCreate,
                pathAttribute?.ToString() ?? "text");

            return placer.XResult;
        }

        /// <summary>
        /// Finds or replaces an <see cref="XElement"/> at the specified path. If the
        /// path does not exist, it is created. If a bound T is not detected, a new T()
        /// is bound to the target XElement.
        /// </summary>
        /// <typeparam name="T">A type implementing <see cref="IXBoundObject"/> with a public parameterless constructor.</typeparam>
        /// <param name="path">The hierarchical path to locate or create.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to "text".</param>
        /// <param name="onBeforeAdd">Optional handler invoked before adding a new element.</param>
        /// <param name="onAfterAdd">Optional handler invoked after adding a new element.</param>
        /// <param name="onIterate">Optional handler invoked during path traversal.</param>
        /// <returns>The bound object of type <typeparamref name="T"/> or null if not found.</returns>
        public static T FindOrCreate<T>(
                this XElement @this,
                string path,
                Enum pathAttribute = null,
                AddEventHandler onBeforeAdd = null,
                AddEventHandler onAfterAdd = null,
                IterateEventHandler onIterate = null
            )
            where T : class, IXBoundObject, new()
        {
            var placer = new Placer(
                @this,
                path,
                localOnBeforeAdd,
                onAfterAdd,
                onIterate,
                PlacerMode.FindOrCreate,
                pathAttribute?.ToString() ?? "text");

            return placer.XResult?.To<T>() ?? default;

            void localOnBeforeAdd(object sender, AddEventArgs e)
            {
                if (!e.Xel.Has<T>())
                {
                    var type = typeof(T);
                    string xname =
                        type.GetCustomAttribute<DataModelAttribute>() is DataModelAttribute dmattr
                        ? dmattr.XName
                        : null;
                    xname = xname ?? nameof(StdAttributeNameXBoundViewObject.datamodel);
                    var t = new T();
                    t.InitXEL(e.Xel);
                    e.Xel.SetBoundAttributeValue(
                        t, 
                        name: xname.ToLower());
                }
                onBeforeAdd?.Invoke( sender, e );
            }
        }

        /// <summary>
        /// Finds an existing <see cref="IXBoundViewObject"/> element at the specified path 
        /// relative to <paramref name="@this"/> and makes it visible. The target element must 
        /// already exist and be bound to an object implementing <see cref="IXBoundViewObject"/>; 
        /// otherwise, an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        public static XElement Show(
            this XElement @this,
            string path,
            Enum pathAttribute = null)
        {
            pathAttribute = pathAttribute ?? StdAttributeNameInternal.text;
            var placer = new Placer(
                @this,
                path,
                pathAttributeName: pathAttribute?.ToString(),
                mode: PlacerMode.FindOrPartial
            );
            if (placer.PlacerResult == PlacerResult.Exists)
            {
                if (placer.XResult is XElement xel)
                {
                    if (xel.To<IXBoundViewObject>() is IXBoundViewObject xbvo)
                    {
                        xbvo.IsVisible = true;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Element at path '{path}' exists, but is not bound to an IXBoundViewObject. " +
                            $"Ensure the element is correctly bound before calling Show().");
                    }
                }
                return placer.XResult;
            }
            else
            {
                path = Path.Combine(@this.GetPath(pathAttribute), path);
                throw new InvalidOperationException(
                    $"Element at path '{path}' is does not exist.");
            }
        }


        /// <summary>
        /// Find or create element at the specified path relative to @this and make it visible
        /// </summary>
        public static T Show<T>(
                this XElement @this,
                string path,
                Enum pathAttribute = null)
            where T : class, IXBoundViewObject, new()
        {
            // Use FoT which auto-creates T.
            if(@this.FindOrCreate<T>(path, pathAttribute) is T xbvo)
            {
                xbvo.IsVisible = true;
                return xbvo;
            }
            else
            {
                path = Path.Combine(@this.GetPath(pathAttribute), path);
                throw new InvalidOperationException(
                    $"Element at path '{path}' is does not exist.");
            }
        }

        /// <summary>
        /// Expands the element at the specified path using the given <paramref name="mode"/> to locate or create it.
        /// </summary>
        /// <param name="path">The hierarchical path to the element.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to user-defined enum or null.</param>
        /// <param name="mode">
        /// Determines how the target element is located:
        /// <see cref="PlacerMode.FindOrCreate"/> to create missing elements,
        /// <see cref="PlacerMode.FindOrThrow"/> to throw if not found,
        /// <see cref="PlacerMode.FindOrAssert"/> to assert failure if not found.
        /// </param>
        /// <returns>The <see cref="XElement"/> after the operation.</returns>
        public static XElement Expand(
                this XElement @this,
                string path,
                Enum pathAttribute = null,
                PlacerMode mode = PlacerMode.FindOrCreate)
        {
            pathAttribute = pathAttribute ?? StdAttributeNameInternal.text;

            // Use Placer instance (instead of FindOrCreate) in
            // order to to support the 'mode' argument.
            var placer = new Placer(
                @this,
                path,
                onBeforeAdd: (sender, e) =>
                {
                    @this.internalOnBeforeAddViewObject(e);
                },
                pathAttributeName: pathAttribute.ToString(),
                mode: mode);
            if(placer.XResult is XElement xel)
            {
                foreach (
                    var cxbvo in
                    xel
                    .Elements()
                    .Select(_=>_.To<IXBoundViewObject>())
                    .Where(_=>_ != null)
                    )
                {
                    cxbvo.IsVisible = true;
                }
                var xbvo = xel.To<IXBoundViewObject>(@throw: true);
                xbvo.Expand(allowPartial: true);
                xbvo.IsVisible = true;
            }
            return placer.XResult;
        }

        /// <summary>
        /// Collapses the element at the specified path using the given <paramref name="mode"/> to locate or create it.
        /// </summary>
        /// <param name="path">The hierarchical path to the element.</param>
        /// <param name="pathAttribute">Optional enum specifying the attribute used to build the path; defaults to user-defined enum or null.</param>
        /// <param name="mode">
        /// Determines how the target element is located:
        /// <see cref="PlacerMode.FindOrPartial"/> to allow partial matches,
        /// <see cref="PlacerMode.FindOrCreate"/> to create missing elements,
        /// <see cref="PlacerMode.FindOrThrow"/> to throw if not found,
        /// <see cref="PlacerMode.FindOrAssert"/> to assert failure if not found.
        /// </param>
        /// <returns>The <see cref="XElement"/> after the operation.</returns>
        public static XElement Collapse(
                this XElement @this,
                string path,
                Enum pathAttribute = null,
                PlacerMode mode = PlacerMode.FindOrCreate)
        {
            pathAttribute = pathAttribute ?? StdAttributeNameInternal.text;

            // Use Placer instance (instead of FindOrCreate) in
            // order to to support the 'mode' argument.
            var placer = new Placer(
                @this,
                path,
                onBeforeAdd: (sender, e) =>
                {
                    @this.internalOnBeforeAddViewObject(e);
                },
                pathAttributeName: pathAttribute.ToString(),
                mode: mode);
            if (placer.XResult is XElement xel)
            {
                foreach (
                    var xbvo in 
                    xel
                    .Descendants()
                    .Reverse()
                    .Select(_=>_.To<IXBoundViewObject>())
                    .Where(_=>_ != null))
                {
                    xbvo.IsVisible = false;
                }
                xel.To<IXBoundViewObject>(@throw: true).Collapse();
            }
            return placer.XResult;
        }

        /// <summary>
        /// Places or modifies an XML element at a specified path within the XML structure of the source element. This method 
        /// allows dynamic configuration through additional parameters and returns the newly created or modified XML element.
        /// It supports complex configurations including attribute settings and event handling, facilitating detailed control 
        /// over the XML manipulation process.
        /// </summary>
        public static PlacerResult Place(
            this XElement source,
            string path,
            out XElement xel,
            params object[] args)
        {
            PlacerMode mode = PlacerMode.FindOrCreate;
            string
                newXElementName = Placer.DefaultNewXElementName,
                pathAttributeName = Placer.DefaultPathAttributeName;
            var attrs = new List<XAttribute>();
            XElement substitute = null;
            object value = null;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case PlacerMode _:
                        mode = (PlacerMode)arg;
                        break;
                    case Dictionary<StdPlacerKeys, string> dict:
                        foreach (var key in dict.Keys)
                        {
                            switch (key)
                            {
                                case StdPlacerKeys.NewXElementName:
                                    newXElementName = dict[key];
                                    break;
                                case StdPlacerKeys.PathAttributeName:
                                    pathAttributeName = dict[key];
                                    break;
                                default: throw new NotImplementedException();
                            }
                        }
                        break;
                    case XBoundAttribute _:
                    case XAttribute _:
                        attrs.Add((XAttribute)arg);
                        break;
                    case XElement _:
                        substitute = (XElement)arg;
                        break;
                    case Enum enumVal:
                        var enumType = enumVal.GetType();
                        var pattr = enumType.GetCustomAttribute<PlacementAttribute>();
                        var placement = pattr?.Placement ?? EnumPlacement.UseXAttribute;
                        var name = pattr?.Name;
                        if(string.IsNullOrWhiteSpace(name)) name = enumType.Name.ToLower();
                        switch (placement)
                        {
                            case EnumPlacement.UseXAttribute:
                                attrs.Add(new XAttribute(name, enumVal));
                                break;
                            case EnumPlacement.UseXBoundAttribute:
                                attrs.Add(new XBoundAttribute(name, enumVal));
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        if (value is null)
                        {
                            value = arg;
                        }
                        else throw new InvalidOperationException("Sequence contains more than one value element");
                        break;
                }
            }
            if(substitute != null && mode != PlacerMode.FindOrCreate)
            {
                var msg = $"{nameof(XElement)} substitution is only allowed for {PlacerMode.FindOrCreate.ToFullKey()}";
                throw new InvalidOperationException(msg);
            }
            var pp = new Placer(
                source,
                path,
                onBeforeAdd: (sender, e) =>
                {
                    e.Xel.Name = newXElementName;
                    if (e.IsPathMatch)
                    {
                        if(substitute != null)
                        {
                            e.Xel = substitute;
                        }
                        if (attrs.Any()) e.Xel.Add(attrs);
                        if (value != null) e.Xel.Add(value);
                    }
                },
                mode: mode,
                pathAttributeName: pathAttributeName);
            xel = pp.XResult;
            return pp.PlacerResult;
        }
        
        /// <summary>
        /// Places or modifies an XML element at a specified path within the XML structure of the source element, 
        /// allowing for dynamic configuration through additional parameters. This method simplifies XML manipulations 
        /// by optionally configuring the element's attributes and values during the placement process, without returning the modified or created element.
        /// </summary>
        public static PlacerResult Place(
            this XElement source,
            string path,
            params object[] args)
            => source.Place(path, out XElement _, args);


        private static void internalOnBeforeAddViewObject(this XElement @this, AddEventArgs e)
        {
            if (!e.Xel.Has<IXBoundViewObject>())
            {
                // [Careful]
                // - Use @this or other parented node for ancestor search, not the aspirant!
                // - Use items.GetGenericArguments() which is our extension NOT Type.GetGenericArguments()
                if (@this.AncestorsAndSelf().Last()?.To<ViewContext>() is ViewContext context &&
                    context.Items is IList items &&
                    items.GetGenericArguments().SingleOrDefault() is Type type)
                {
                    string xname = 
                        type.GetCustomAttribute<DataModelAttribute>() is DataModelAttribute dmattr
                        ? dmattr.XName 
                        : null;
                    xname = xname ?? nameof(StdAttributeNameXBoundViewObject.datamodel);

                    if (Activator.CreateInstance(type) is IXBoundObject xbo)
                    {
                        xbo.InitXEL(e.Xel);
                        e.Xel.SetBoundAttributeValue(
                            xbo,
                            name: xname.ToLower());
                    }
                    else Debug.Fail($"Expecting successful creation of type '{type.Name}'");
                }
                else
                {
                    e.Xel.SetBoundAttributeValue(
                        new XBoundViewObjectImplementer(e.Xel),
                        name: nameof(StdAttributeNameXBoundViewObject.datamodel));
                }
            }
        }

        /// <summary>
        /// Recursively enumerates visible descendant elements starting from the root <see cref="XElement"/>, 
        /// optionally including all immediate root children regardless of visibility state.
        /// </summary>
        /// <param name="alwaysShowRootElements">
        /// If true, all direct child elements of the root are included regardless of their visibility; 
        /// otherwise, only visible elements are processed.
        /// </param>
        /// <returns>An enumeration of <see cref="XElement"/> nodes that are considered visible.</returns>
        public static IEnumerable<XElement> VisibleElements(this XElement @this, bool alwaysShowRootElements = false)
        {
            Debug.Assert(@this.Parent is null, "Expecting root node");

            IEnumerable<XElement> elements = alwaysShowRootElements
                ? @this.Elements()
                : @this.Elements().Where(localGetIsVisible);

            foreach (var element in localAddChildItems(elements))
            {
                yield return element;
            }

            #region L o c a l F x       
            IEnumerable<XElement> localAddChildItems(IEnumerable<XElement> items)
            {
                foreach (var element in items)
                {
                    yield return element;
                    foreach (var child in localAddChildItems(element.Elements().Where(localGetIsVisible)))
                    {
                        yield return child;
                    }
                }
            }
            bool localGetIsVisible(XElement xel) =>
                xel.TryGetAttributeValue(out IsVisible value) &&
                bool.Parse(value.ToString());		
            #endregion L o c a l F x
        }
    }
    static partial class ExtensionsInternal
    {
        public static string GetValid(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new FormatException("Empty string not allowed.");
            return @this;
        }

        /// <summary>
        /// Returns the generic type arguments of the first generic type found in the inheritance chain of the object's runtime type.
        /// If no generic base type is found, returns an empty array.
        /// </summary>
        public static Type[] GetGenericArguments(this object @this)
        {
            Type type;

            for (type = @this.GetType(); type != null; type = type.BaseType)
            {
                if (type.IsGenericType)
                {
                    return type.GetGenericArguments();
                }
            }
            return Array.Empty<Type>();
        }

        [Obsolete]
        public static bool IsCreated(this XElement source, string path, out XElement xel, string name = null, object value = null, Enum id = null)
        {
            var pp = new Placer(source, path, onBeforeAdd: (sender, e) =>
            {
                if (e.IsPathMatch)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        e.Xel.Name = name;
                    }
                    if (value != null)
                    {
                        e.Xel.SetValue(value);
                    }
                    if (id != null)
                    {
                        e.Xel.SetAttributeValue(id);
                    }
                }
            });
            xel = pp.XResult;
            return pp.PlacerResult == PlacerResult.Created;
        }
    }
    public class PlacerArgs
    {
        public PlacerArgs(params object[] valuesToAdd) => Payload = valuesToAdd;

        public object[] Payload { get; }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Configures an XElement as the root of a view-bound XML tree, enabling automatic synchronization 
        /// with an optional IList and other view-model behaviors.
        ///
        /// This extension sets up a ViewContext for the given element, allowing:
        /// - Automatic expansion and collapse tracking
        /// - Visibility filtering
        /// - Hierarchical indentation
        /// - Automatic synchronization with an ObservableCollection<T>
        /// - Optional sorting logic
        ///
        /// Default Sorting Behavior:
        /// If a custom sorter is not provided and sorting is enabled:
        /// - If the 'items' parameter can be evaluated as IList<T>, then:
        ///     - If T implements IComparable<T>, sorting will use T.CompareTo.
        ///     - Otherwise, elements are sorted by their "text" attribute alphanumerically.
        /// - If 'items' is not provided, sorting falls back to alphanumeric ordering on the "text" attribute.
        ///
        /// Providing 'items' is optional. However, passing it enables automatic synchronization with the bound view model.
        /// </summary>
        /// <param name="this">The root XElement to bind. Must not have a parent element.</param>
        /// <param name="items">
        /// Optional backing list representing the visible view model items.
        /// Should be an ObservableCollection<T> where T implements IXBoundViewObject.
        /// </param>
        /// <param name="indent">The number of spaces to indent per level. Default is 10.</param>
        /// <param name="autoSyncEnabled">Whether to enable automatic list synchronization. Default is true.</param>
        /// <param name="autoSyncSettleDelay">
        /// Optional debounce delay used to batch rapid changes before syncing. Default is 100ms.
        /// </param>
        /// <param name="sortingEnabled">Whether child elements should be sorted after structural or visibility changes.</param>
        /// <param name="customSorter">
        /// Optional comparison function for custom sorting logic. Used in place of the default sorter.
        /// Must accept two objects and return a comparison result (like Comparer<T>.Compare).
        /// </param>
        /// <returns>The original XElement, now configured as a bound view root.</returns>

        public static XElement WithXBoundView(
            this XElement @this,
            IList items = null,
            int indent = 10,
            bool autoSyncEnabled = true,
            TimeSpan? autoSyncSettleDelay = null,
            bool sortingEnabled = true,
            Func<XElement, XElement, int> customSorter = null)
        {
            if (@this.Parent != null)
            {
                throw new InvalidOperationException("The receiver must be a root element.");
            }
            @this.SetBoundAttributeValue(new ViewContext(
                @this,
                items,
                indent,
                autoSyncEnabled,
                autoSyncSettleDelay,
                sortingEnabled,
                customSorter
                ));
            return @this;
        }
    }
}