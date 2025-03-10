using IVSoftware.Portable.Xml.Linq.XBoundObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace IVSoftware.Portable.Xml.Linq.XBoundObject.Modeling
{
    [Flags]
    public enum ModelingOption
    {
        IncludeValueTypeInstances = 0x01,
    }

    public delegate void PropertyChangedDelegate(object sender, PropertyChangedEventArgs e);
    public delegate void NotifyCollectionChangedDelegate(object sender, NotifyCollectionChangedEventArgs e);
    public delegate void XObjectChangeDelegate(object sender, XObjectChangeEventArgs e);
    public class ModelingContext
    {
        public ModelingContext(object o)
        {
            This = o;
            Type = o.GetType();
        }
        internal object This { get; set; }
        internal Type Type { get; }
        public XElement OriginModel { get; set; } = new XElement("model");
        public PropertyChangedDelegate PropertyChangedDelegate { get; set; } = null;
        public NotifyCollectionChangedDelegate NotifyCollectionChangedDelegate { get; set; } = null;
        public XObjectChangeDelegate XObjectChangeDelegate { get; set; } = null;

        public ModelingOption Options { get; set; } = 0;

        internal void RaiseElementAvailable(object sender, XElement element)
        {
            ElementAvailable?.Invoke(sender, new ElementAvailableEventArgs(element));
        }
        public event EventHandler<ElementAvailableEventArgs> ElementAvailable;
    }
    public class ElementAvailableEventArgs : EventArgs
    {
        public ElementAvailableEventArgs(XElement xel) => Element = xel;

        public XElement Element { get; }
    }
    public static class ModelingExtensions
    {
        public static XElement CreateModel(this object @this, ModelingContext context = null)
        {
            context = context ?? new ModelingContext(@this);
            foreach (var xel in context.ModelDescendantsAndSelf())
            {
                context?.RaiseElementAvailable(sender: @this, element: xel);
            }
            return context.OriginModel;
        }
        public static IEnumerable<XElement> ModelDescendantsAndSelf(this object @this, ModelingContext context = null)
        {
            if (@this is null) throw new ArgumentNullException(nameof(@this));
            if (context is null) return new ModelingContext(@this).ModelDescendantsAndSelf();
            else
            {
                if (context.This is null) context.This = @this;
                if (ReferenceEquals(@this, context.This)) return context.ModelDescendantsAndSelf();
                else throw new ArgumentException($"References for @this and {nameof(ModelingContext)}.This must be the same.");
            }
        }
        public static IEnumerable<XElement> ModelDescendantsAndSelf(this ModelingContext context) 
        { 
            context = context ?? new ModelingContext(context.This);
            if (!context.OriginModel.Ancestors().Any())
            {
                context.OriginModel.SetAttributeValue(nameof(SortOrderNOD.name), $"(Origin){context.Type.ToShortTypeNameText()}");
            }
            localDiscoverModel(context.This, context.OriginModel);
            foreach (var xel in context.OriginModel.DescendantsAndSelf())
            {
                yield return xel;
            }

            void localDiscoverModel(object instance, XElement localModel, HashSet<object> visited = null)
            {
                visited = visited ?? new HashSet<object>();
                localModel.SetBoundAttributeValue(instance, name: nameof(instance));
                localRunRecursiveDiscovery(localModel);

                void localRunRecursiveDiscovery(XElement currentElement)
                {
                    object localInstance;
                    localInstance = (currentElement.Attribute(nameof(instance)) as XBoundAttribute)?.Tag;
                    if (localInstance != null && visited.Add(localInstance))
                    {
                        var pis = localInstance.GetType().GetProperties()
                            .Where(pi =>
                                pi.GetCustomAttribute<IgnoreNODAttribute>() is null &&
                                pi.GetIndexParameters().Length == 0);
                        foreach (var pi in pis.ToArray())
                        {
                            XElement member = new XElement(nameof(member));
                            member.SetAttributeValue(nameof(pi.Name).ToLower(), pi.Name);
                            currentElement.Add(member);
                            if (pi.GetValue(localInstance) is object childInstance)
                            {
                                if (childInstance is string ||
                                    childInstance is Enum ||
                                    childInstance is ValueType)
                                {
                                    if (context?.Options.HasFlag(ModelingOption.IncludeValueTypeInstances) == true)
                                    {
                                        member.SetBoundAttributeValue(childInstance, nameof(instance));
                                    }
                                    continue;
                                }
                                else
                                {
                                    member.SetBoundAttributeValue(childInstance, nameof(instance));
                                    if (childInstance is IEnumerable collection)
                                    {
                                        foreach (var item in collection)
                                        {
                                            var childModel = new XElement("model");
                                            localDiscoverModel(item, childModel, visited);
                                            member.Add(childModel);
                                        }
                                    }
                                    localRunRecursiveDiscovery(member);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attaches notification delegates to the descendants of the given object, 
        /// allowing property, collection, and object changes to be monitored. 
        /// This overload discards the model output.
        /// </summary>
        /// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
        /// <param name="this">The object whose descendants should be monitored.</param>
        /// <param name="onPC">The delegate to handle property change notifications (required).</param>
        /// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
        /// <param name="onXO">The delegate to handle object change notifications (optional).</param>
        /// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
        public static T WithNotifyOnDescendants<T>(
            this T @this,
            PropertyChangedDelegate onPC,
            NotifyCollectionChangedDelegate onCC = null,
            XObjectChangeDelegate onXO = null)
            => @this.WithNotifyOnDescendants(out XElement _, onPC, onCC, onXO);

        /// <summary>
        /// Attaches notification delegates to the descendants of the given object,
        /// allowing for property changes, collection changes, and object changes to be monitored.
        /// </summary>
        /// <typeparam name="T">The type of the object, which must have a parameterless constructor.</typeparam>
        /// <param name="this">The object whose descendants should be monitored.</param>
        /// <param name="model">The XElement representing the model, returned as an output parameter.</param>
        /// <param name="onPC">The delegate to handle property change notifications (required).</param>
        /// <param name="onCC">The delegate to handle collection change notifications (optional).</param>
        /// <param name="onXO">The delegate to handle object change notifications (optional).</param>
        /// <returns>The same instance of <typeparamref name="T"/> for fluent chaining.</returns>
        public static T WithNotifyOnDescendants<T>(
            this T @this,
            out XElement model,
            PropertyChangedDelegate onPC,
            NotifyCollectionChangedDelegate onCC = null,
            XObjectChangeDelegate onXO = null)
        {
            var context = new ModelingContext(@this)
            {
                PropertyChangedDelegate = onPC,
                NotifyCollectionChangedDelegate = onCC,
                XObjectChangeDelegate = onXO,               
            };
            context.ElementAvailable += (sender, e) =>
            {
                var xel = e.Element;
                { }
            };
            model = @this.CreateModel(context);



            throw new NotImplementedException();
            return @this;
        }

        public static string ToTypeNameText(this Type @this)
        {
            if (@this?.FullName == null) return "Unknown";

            var fullName = @this.FullName.Split('`').First(); // Remove generic type info
            int lastPlusIndex = fullName.LastIndexOf('+');

            if (lastPlusIndex < 0) return fullName; // No nested class, return as is

            int lastDotIndex = fullName.LastIndexOf('.', lastPlusIndex);
            return fullName.Remove(lastDotIndex + 1, lastPlusIndex - lastDotIndex);
        }
        public static string ToShortTypeNameText(this Type @this)
            => @this.ToTypeNameText().Split('.').Last();

        public static string InSquareBrackets(this string @this) => $"[{@this}]";
        public static string InSquareBrackets(this Enum @this) => $"[{@this.ToString()}]";
    }
}
