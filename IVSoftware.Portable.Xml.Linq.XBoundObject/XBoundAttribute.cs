using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq
{
	public class XBoundAttribute : XAttribute
    {
        /// <summary>
        /// Fully Qualified Base Method
        /// </summary>
        public XBoundAttribute(
            XName name, 
            object tag,
            string text = null,
            SetOption options = SetOption.NameToLower)
        : base(
              name: getSafeName(name, tag, text, options),
              value: getSafeValue(tag, text, options))
        {
            if (tag == null) throw new ArgumentNullException("tag");
            switch (options)
            {
                case SetOption.None:
                case SetOption.NameToLower:
                    break;
                default:
                    Debug.WriteLine($"Unsupported {nameof(SetOption)}: {options} will be IGNORED." );
                    break;
            }
            Tag = tag;
        }
        private static string getSafeName(
            XName xname,
            object tag,
            string text = null,
            SetOption options = SetOption.NameToLower)
        {
            var name = xname?.LocalName;
            if (name == null || string.IsNullOrWhiteSpace(name))
            {
                if (tag == null)
                {
                    throw new ArgumentNullException(nameof(tag), "Cannot infer name from null tag.");
                }

                name = tag.GetType().Name.Split('`')[0];
            }

            return options.HasFlag(SetOption.NameToLower)
                ? name.ToLower()
                : name;
        }

        private static string getSafeValue(
           object tag,
           string text = null,
           SetOption options = SetOption.NameToLower)
        {
            if (tag == null)
            {
                throw new ArgumentNullException(nameof(tag));
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                return options.HasFlag(SetOption.ValueToLower)
                    ? text.ToLower()
                    : text;
            }

            // Default fallback: derive from tag
            return getNameForType(tag);
        }

        internal void RaiseObjectBound(XElement xel) =>
            ObjectBound?.Invoke(xel, new ObjectBoundEventArgs(this));


        /// <summary>
        /// Copy-Construct from another XBoundAttribute
        /// </summary>
        public XBoundAttribute(XBoundAttribute other)
		: base(other)
		{
			Tag = other.Tag;
		}
		/// <summary>
		/// Initialize standard XAttribute where value
		/// is rendered as value.ToString()
		/// </summary>
		public XBoundAttribute(XName name, object tag)
		: this(name, tag, getNameForType(tag), SetOption.NameToLower)
		{
			if (tag == null) throw new ArgumentNullException("tag");
			Tag = tag;
		}
		/// <summary>
		/// Initialize standard XAttribute where value
		/// is rendered as value.ToString()
		/// </summary>
		public XBoundAttribute(XName name, object tag, string text)
        : this(name, tag, text, SetOption.NameToLower)
        {
			if (tag == null) throw new ArgumentNullException("tag");
			Tag = tag;
        }
        public object Tag { get; set; }

		private static string getNameForType(object tag)
		{
			var type = tag.GetType();
            if (type.IsEnum)
            {
                return $"[{tag.GetType().Name}.{tag.ToString()}]";
            }
            else
            {
                return $"[{tag.GetType().Name.Split('`')[0]}]";
            }
        }
        public static event ObjectBoundEventHandler ObjectBound;
    }

    public delegate void ObjectBoundEventHandler(object sender, ObjectBoundEventArgs e);
    public class ObjectBoundEventArgs : EventArgs
    {
        public ObjectBoundEventArgs(object o) => BoundObject = o;
        public object BoundObject { get; }
    }
}
