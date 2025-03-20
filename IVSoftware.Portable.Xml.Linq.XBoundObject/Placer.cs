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

    public enum PathSource
    {
        AttributeValue,
        ElementName,
    }
    public enum DefaultAttributeName
    {
        text,
    }

    /// <summary>
    /// No-fail path creation. Fires an event for each XElement created.
    /// </summary>
    /// <remarks>
    /// - The 'fqpath' argument is always assumed to be relative to an implicit root.
    /// - RECOMMENDATION:
    ///   When using 'PathSource.AttributeValue' do not set the path attribute for the root node.
    /// </remarks>
    /// <param fqpath>
    /// If the root happens to match the path attribute or element name, then the remainder of
    /// the path will be used to locates a node. It's usually much easier to just set this
    /// argument in the form of {root\}level0\level1.
    /// </param>
    public class Placer
    {
        public Placer(
            XElement xSource,
            string fqpath,
            AddEventHandler onBeforeAdd = null,
            AddEventHandler onAfterAdd = null,
            IterateEventHandler onIterate = null,
            PlacerMode mode = PlacerMode.FindOrCreate,
            PathSource pathSource = PathSource.AttributeValue,
            Enum pathAttribute = null
        ) : this(
                xSource: xSource,
                parse: fqpath.GetValid().Split('\\'),
                onBeforeAdd: onBeforeAdd,
                onAfterAdd: onAfterAdd,
                onIterate: onIterate,
                mode: mode,
                pathSource: pathSource,
                pathAttribute: pathAttribute ?? DefaultAttributeName.text
            )
        { }

        public Placer(
            XElement xSource,
            string[] parse,
            AddEventHandler onBeforeAdd = null,
            AddEventHandler onAfterAdd = null,
            IterateEventHandler onIterate = null,
            PlacerMode mode = PlacerMode.FindOrCreate,
            PathSource pathSource = PathSource.AttributeValue,
            Enum pathAttribute = null
        )
        {
            _xSource = _xTraverse = xSource;
            _onBeforeAdd = onBeforeAdd;
            _onAfterAdd = onAfterAdd;
            _onIterate = onIterate;
            _mode = mode;
            switch (pathSource)
            {
                case PathSource.AttributeValue:
                    placeUsingAttributePath(parse, pathAttribute ?? DefaultAttributeName.text);
                    break;
                case PathSource.ElementName:
                    placeUsingElementName(parse);
                    break;
                default:
                    break;
            }
        }

        AddEventHandler _onBeforeAdd { get; }
        AddEventHandler _onAfterAdd { get; }
        IterateEventHandler _onIterate { get; }

        PlacerMode _mode;

        private readonly XElement _xSource;     // Please don't remove. This helps us debug monitor progress.
        private XElement _xTraverse;
        public XElement XResult { get; private set; }
        private void placeUsingAttributePath(string[] parse, Enum pathAttribute)
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
                            _xTraverse.Attribute(pathAttribute.ToString())?.Value is string value1 &&
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
                                xel.Attribute(pathAttribute.ToString())?.Value is string value2 &&
                                value2 == level);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.Message);
                    @try =
                        _xTraverse.Elements()
                        .FirstOrDefault(xel => xel.Attribute(pathAttribute.ToString()).Value == level);
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
                            e.Xel.SetAttributeValue(pathAttribute.ToString(), level);
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

        private void placeUsingElementName(string[] parse)
        {
            var builder = new List<string>();
            string level = string.Empty;

            // The first loop traverses the existing paths
            XElement @try;
            int i = 0;
            while (i < parse.Length)
            {
                level = parse[i];
                // REMEMBER: Root matches are ALLOWED.
                if ((i == 0) && (_xTraverse.Name.LocalName == level))
                {
                    // Detected a benign explicit match for root element name (not required).
                    @try = _xTraverse;
                }
                else
                {
                    @try = _xTraverse.Element(level);
                    if (@try == null)
                    {
                        break;
                    }
                    else
                    {
                        _xTraverse = @try;
                    }
                }
                builder.Add(level);
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
                    case PlacerMode.FindOrCreate:
                        break;
                    case PlacerMode.FindOrThrow:
                        PlacerResult = PlacerResult.Throw;
                        throw new KeyNotFoundException("Missing paths identified.");
                    case PlacerMode.FindOrPartial:
                        PlacerResult = PlacerResult.Partial;
                        return;
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
                            // When using the mode of 'PathSource.ElementName' it's
                            // important to assign the xElementName in this CTor.
                            var requestPath = string.Join(@"\", parse);
                            var currentPath = string.Join(@"\", builder);
                            var e = new AddEventArgs(parent: _xTraverse, path: currentPath, xElementName: level, isPathMatch: requestPath == currentPath);
                            _onBeforeAdd?.Invoke(this, e);
                            if (!e.Handled)
                            {
                                _xTraverse.Add(e.Xel);
                            }
                            _onAfterAdd?.Invoke(this, e);
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
        #endregion P R O P E R T I E S
    }
    public delegate void AddEventHandler(Object sender, AddEventArgs e);
    public delegate void IterateEventHandler(Object sender, IterateEventArgs e);
    public delegate XElement Creator();
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

#if false

	public delegate void ObjectChangingEventHandler(Object sender, ObjectChangingEventArgs e);
	public class ObjectChangingEventArgs : EventArgs
	{
		public ObjectChangingEventArgs(object oldObject, object newObject)
		{
			OldObject = oldObject;
			NewObject = newObject;
		}
		public object OldObject { get; }
		public object NewObject { get; }
		public bool Cancel { get; set; }
	}
#endif
    static partial class Extensions
    {
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
        public static string GetValid(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                throw new FormatException("Empty string not allowed.");
            return @this;
        }
    }
}