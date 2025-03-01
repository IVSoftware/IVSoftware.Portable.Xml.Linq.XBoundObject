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

        /// <summary>
        /// Returns the first ancestor that Has XBoundAttribute of type T.
        /// </summary>
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

        /// <summary>
        /// Sets an attribute on the given XElement using the name of the Enum type as the attribute name 
        /// and the Enum value as the attribute value.
        /// </summary>
        /// <param name="this">The XElement to set the attribute on.</param>
        /// <param name="value">The Enum value to store as an attribute.</param>
        /// <param name="useLowerCaseName">If true, the attribute name will be the Enum type name in lowercase; otherwise, it will use the exact type name.</param>
        public static void SetAttributeValue(this XElement @this, Enum value, bool useLowerCaseName = true)
            => @this
            .SetAttributeValue(
                useLowerCaseName
                ? value.GetType().Name.ToLower()
                : value.GetType().Name,
                $"{value}");

        /// <summary>
        /// Attempts to retrieve an enum value from an attribute in the given XElement.
        /// </summary>
        /// <typeparam name="T">The enum type to parse.</typeparam>
        /// <param name="this">The XElement to retrieve the attribute from.</param>
        /// <param name="value">
        /// When this method returns, contains the parsed enum value if the attribute exists and is valid; 
        /// otherwise, the default value of T.
        /// </param>
        /// <param name="stringComparison">
        /// The string comparison method used for matching attribute names. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the attribute exists and was successfully parsed as an enum of type T; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetAttributeValue<T>(
        this XElement @this,
        out T value,
        StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        where T : struct, Enum
        {
            var type = typeof(T);
            value = default;

            var attribute = @this
                .Attributes()
                .FirstOrDefault(attr => string.Equals(attr.Name.LocalName, type.Name, stringComparison));

            if (attribute != null && Enum.TryParse(attribute.Value, out T parsedValue))
            {
                value = parsedValue;
                return true;
            }
            return false;
        }
    }
}
