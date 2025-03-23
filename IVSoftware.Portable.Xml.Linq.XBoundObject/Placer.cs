using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq
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
                    fqpath.GetValid().Trim().Split(Path.DirectorySeparatorChar),
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
                            (i == 0) &&
                            _xTraverse.Attribute(pathAttributeName)?.Value is string value1 &&
                            (value1 == level)
                        )
                    {
                        // Detected a benign explicit match for path attribute set on root (not recommended).
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
                            (XResult == null) ?
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
                                if ((e.InsertIndex == null) || ((int)e.InsertIndex >= existing.Length))
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
        static string _defaultNewXElementName = "xel";

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
    public delegate void AddEventHandler(Object sender, AddEventArgs e);
    public delegate void IterateEventHandler(Object sender, IterateEventArgs e);
    public class AddEventArgs : EventArgs
    {
        public AddEventArgs(XElement parent, string path, string xElementName, bool isPathMatch)
        {
            Parent = parent;
            Path = path;
            Xel = new XElement(xElementName);
            IsPathMatch = isPathMatch;
        }
        public AddEventArgs(XElement parent, string path, bool isPathMatch)
            : this(parent, path, "xnode", isPathMatch) { }

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
    public enum StdPlaceKeys
    {
        NewXElementName,
        PathAttributeName,
    }
    public static partial class Extensions
    {
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
            object value = null;

            foreach (var arg in args)
            {
                switch (arg)
                {
                    case PlacerMode _:
                        mode = (PlacerMode)arg;
                        break;
                    case Dictionary<StdPlaceKeys, string> dict:
                        foreach (var key in dict.Keys)
                        {
                            switch (key)
                            {
                                case StdPlaceKeys.NewXElementName:
                                    newXElementName = dict[key];
                                    break;
                                case StdPlaceKeys.PathAttributeName:
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
                    default:
                        if (value is null)
                        {
                            value = arg;
                        }
                        else throw new InvalidOperationException("Sequence contains more than one value element");
                        break;
                }
            }
            var pp = new Placer(
                source,
                path,
                onBeforeAdd: (sender, e) =>
                {
                    if (e.IsPathMatch)
                    {
                        e.Xel.Name = newXElementName;
                        if (attrs.Any()) e.Xel.Add(attrs);
                        if (value != null) e.Xel.Add(value);
                    }
                },
                mode: mode,
                pathAttributeName: pathAttributeName);
            xel = pp.XResult;
            return pp.PlacerResult;
        }
        public static PlacerResult Place(
            this XElement source,
            string path,
            params object[] args)
            => source.Place(path, out XElement _, args);
    }
    static partial class ExtensionsInternal
    {
        public static string GetValid(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new FormatException("Empty string not allowed.");
            return @this;
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
}