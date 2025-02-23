using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject
{
    public static partial class Extensions
    {
        /// <summary>
        /// Fully qualified XBoundAttribute Setter
        /// </summary>
        public static void SetBoundAttributeValue(
            this XElement xel,
            object tag,
            string name = null,
            string text = null,
            SetOption options = SetOption.NameToLower)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = tag.GetType().Name.Split('`')[0];
            }
            var xbo = new XBoundAttribute(name: name, tag: tag, text: text);
            xel.Add(xbo);
            xbo.RaiseObjectBound(xel);
        }

        /// <summary>
        /// Name come from user-supplied standard enumerated names. 
        /// </summary>
        public static void SetBoundAttributeValue(
            this XElement xel,
            object tag,
            Enum stdName,
            string text = null,
            SetOption options = SetOption.NameToLower) =>
                xel.SetBoundAttributeValue(
                    tag,
                    stdName.ToString(),
                    text,
                    options);





        /// <summary>
        /// Return Single or Default where type is T. Null testing will be done by client.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static T To<T>(this XElement xel, bool @throw = false)
        {
            xel.TryGetSingleBoundAttributeByType(out T attr, @throw);
            return attr;
        }

        /// <summary>
        /// Return true if xel has any attribute of type T"/>
        /// </summary>
        public static bool Has<T>(this XElement xel) =>
            xel
            .Attributes()
            .Any(_ => (_ is XBoundAttribute) && (((XBoundAttribute)_).Tag is T));

        /// <summary>
        /// Try return Single or Default where type is T.
        /// </summary>
        /// <remarks>
        /// By default, downgrades Single() exception to Debug.Fail and 
        /// return false (but no assert) for null and true for single.
        /// </remarks>
        public static bool TryGetSingleBoundAttributeByType<T>(this XElement xel, out T o, bool @throw = false)
        {
            XBoundAttribute xba;
            if (@throw)
            {
                xba =
                    (XBoundAttribute)
                    xel.Attributes()
                    .Single(battr => (battr is XBoundAttribute) && (((XBoundAttribute)battr).Tag is T));
            }
            else
            {
                var candidates =
                    xel.Attributes()
                    .Where(battr => (battr is XBoundAttribute) && (((XBoundAttribute)battr).Tag is T));
                if (candidates.Count() > 1)
                {
                    Debug.Fail($"Multiple instances of type {typeof(T)} exist.");
                    xba = default;
                }
                else
                {
                    xba = (XBoundAttribute)candidates.FirstOrDefault();
                }
            }
            if (Equals(xba, default(XBoundAttribute)))
            {
                o = default;
                return false;
            }
            else
            {
                o = (T)xba.Tag;
                return true;
            }
        }
        public static T AncestorOfType<T>(this XElement @this, bool includeSelf = false, bool @throw = false)
        {

            if(@throw)
            {
                return 
                    includeSelf
                    ? @this.AncestorsAndSelf().First(_ => _.Has<T>()).To<T>()
                    : @this.Ancestors().First(_ => _.Has<T>()).To<T>();
            }
            else
            {
                XElement anc = 
                    includeSelf
                    ? @this.AncestorsAndSelf().FirstOrDefault(_ => _.Has<T>())
                    : @this.Ancestors().FirstOrDefault(_ => _.Has<T>());
                return
                    anc is null
                    ? default
                    : anc.To<T>();
            }
        }
    }
}
