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
              name: 
                  Equals(options, SetOption.NameToLower)
                  ? name.LocalName.ToLower()
                  : name,
              value: 
                text is null
                ? tag is null
                    ? throw new ArgumentNullException("tag")
                    : getNameForType(tag)
                : text)
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
